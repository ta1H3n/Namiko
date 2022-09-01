using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Model.Exceptions;
using Namiko.Addons.Handlers;
using Namiko.Data;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Namiko.Handlers.Services;
using Namiko.Handlers.TypeConverters;
using Namiko.Modules.Leaderboard;
using Namiko.Modules.Pro;

#pragma warning disable CS1998

namespace Namiko
{
    public class Program
    {
        private static DiscordShardedClient Client;
        private static IServiceProvider Services;
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static readonly CancellationToken ct = cts.Token;
        public static bool Development = false;
        public static bool GuildLeaveEvent = true;
        private static int ShardCount;

        private static int Startup = 0;
        private static int AllShardsReady = 0;
        private HashSet<int> ShardsDownloadingUsers = new HashSet<int>();

        static void Main(string[] args)
        {
            using Mutex mutex = new Mutex(true, "Global-NamikoBot", out bool createdNew);
            if (!createdNew)
            {
                Console.WriteLine("Instance Already Running!");
                return;
            }

            using (SentrySdk.Init(options =>
            {
                options.Dsn = new Dsn(AppSettings.SentryWebhook);
                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                options.Environment = env == null || env == "" ? "Production" : env;
            }))
            {
                SetUp();
                new Program().MainAsync().GetAwaiter().GetResult();
            }
        }
        private async Task MainAsync()
        {
            var logger = new Logger();
            
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Warning,
                DefaultRetryMode = RetryMode.Retry502,
                AlwaysDownloadUsers = false,
                MessageCacheSize = 0,
                LargeThreshold = 250,
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildVoiceStates |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildInvites |
                    GatewayIntents.GuildMessageReactions |
                    GatewayIntents.DirectMessages |
                    GatewayIntents.DirectMessageReactions
            });

            Client.Log += logger.Console_Log;
            Client.ShardConnected += SetUp_FirstShardConnected;
            Client.ShardReady += SetUp_AllShardsReady;

            Client.ShardConnected += Client_ShardConnected;
            Client.ShardDisconnected += Client_ShardDisconnected;
            Client.ShardReady += Client_ShardReady;
            //Client.ShardReady += Client_ShardReady_DownloadUsers;

            // Client.ReactionAdded += Client_ReactionAdded;
            // Client.ReactionAdded += BanroyaleGame.HandleBanroyaleReactionAsync;
            // Client.MessageReceived += Client_ReadCommand;
            Client.UserVoiceStateUpdated += Client_UserVoiceChannel;

            // Namiko join/leave
            Client.JoinedGuild += Client_JoinedGuild;
            Client.LeftGuild += Client_LeftGuild;

            // Join/leave logging.
            if (!Development)
                Client.UserJoined += Client_UserJoinedWelcome;
            Client.UserJoined += Client_UserJoinedLog;
            Client.UserLeft += Client_UserLeftLog;
            Client.UserBanned += Client_UserBannedLog;

            var commandConfig = new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = Discord.Commands.RunMode.Async,
                LogLevel = LogSeverity.Warning
            };

            

            var interactionConfig = new InteractionServiceConfig
            {
                LogLevel = LogSeverity.Warning,
                WildCardExpression = "*",
            };
            
            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(interactionConfig)
                .AddSingleton<SlashCommandService>()
                .AddSingleton(commandConfig)
                .AddSingleton<TextCommandService>()
                .AddSingleton<InteractiveService>()
                .AddSingleton(logger)
                .BuildServiceProvider();

            await Client.LoginAsync(TokenType.Bot, AppSettings.Token);
            await Client.StartAsync();
            _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
                $"------------------------------\n" +
                $"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `Logged in`\n" +
                $"------------------------------");

            ShardCount = Client.Shards.Count;

            try
            {
                await Task.Delay(-1, ct);
            }
            catch { }
            cts.Dispose();
            Console.WriteLine("Shutting down...");
            await Client.LogoutAsync();
        }

        // SET-UP  

        private static void SetUp()
        {
            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Development":
                    Development = true;
                    break;
                default:
                    _ = ImgurAPI.ImgurSetup();
                    break;
            }
            NamikoDbContext.ConnectionString = AppSettings.ConnectionString;
            _ = LootboxStats.Reload(Locations.LootboxStatsJSON);
            Images.ReactionImageCommands = ImageDb.GetReactionImageDictionary().Result;
            //Blacklist = BlacklistDb.GetAll();
            Waifu.Path = AppSettings.ImageUrlPath + "waifus/";
        }
        private async Task SetUp_FirstShardConnected(DiscordSocketClient arg)
        {
            // Making sure this part only runs once, unless an exception is thrown. Thread safe.
            if (Interlocked.Exchange(ref Startup, 1) == 0)
            {
                try
                {
                    WebUtil.SetUpDbl(arg.CurrentUser.Id);
                    await StartTimers();
                    if (!Development)
                        await Music.Initialize(Client);
                    await Client.SetActivityAsync(new Game($"with your waifu", ActivityType.Playing));
                }
                catch (Exception ex)
                {
                    Startup = 0;
                    SentrySdk.CaptureException(ex);
                }
            }
        }
        private async static Task StartTimers()
        {
            if (Development)
                await Timers.SetUp(Client);
            else
                await Timers.SetUpRelease(Client);
        }
        private async Task SetUp_AllShardsReady(DiscordSocketClient arg)
        {
            // Making sure this part only runs once, unless an exception is thrown. Thread safe.
            if (Client.Shards.All(x => x.ConnectionState == ConnectionState.Connected) && Interlocked.Exchange(ref AllShardsReady, 1) == 0)
            {
                try
                {
                    int res = await CheckLeftGuilds();
                    if (res > 0)
                    {
                        Console.WriteLine($"{DateTime.Now} - Left {res} guilds.");
                        _ = WebhookClients.NamikoLogChannel.SendMessageAsync($"<:TickNo:577838859077943306> `{DateTime.Now.ToString("HH:mm:ss")}` - `Left {res} guilds`");
                    }

                    res = Client.Guilds.Count;
                    Console.WriteLine($"{DateTime.Now} - Loaded {res} guilds.");
                    _ = WebhookClients.NamikoLogChannel.SendMessageAsync($"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `{res} guilds ready`");
                }
                catch (Exception ex)
                {
                    AllShardsReady = 0;
                    SentrySdk.CaptureException(ex);
                }
            }
        }


        // EVENTS

        private async Task Client_UserVoiceChannel(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (user.IsBot)
                return;
            if (Music.Node == null)
                return;
            if (user == null || !(user is SocketGuildUser))
                return;
            if (((SocketGuildUser)user).Guild == null)
                return;

            var player = Music.Node?.GetPlayer(((SocketGuildUser)user).Guild);
            if (player == null)
                return;

            if (after.VoiceChannel != null && after.VoiceChannel == player.VoiceChannel)
            {
                if (before.VoiceChannel == after.VoiceChannel)
                    return;

                if (after.VoiceChannel.Users.Count == 2)
                {
                    await player.PlayLocal("someonejoinalone");
                    return;
                }

                await player.PlayLocal("someonejoin");
                return;
            }

            if (before.VoiceChannel == player.VoiceChannel && after.VoiceChannel != player.VoiceChannel)
            {
                await player.PlayLocal("someoneleft");
                return;
            }
        }
        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            if (arg3.MessageId != 700399700196458546)
                return;

            SocketTextChannel sch = (await arg2.GetOrDownloadAsync()) as SocketTextChannel;
            var user = sch.Guild.GetUser(arg3.UserId);
            var role = sch.Guild.GetRole(697234413360119808);

            if (RoleUtil.HasRole(user, role))
                return;

            await user.AddRoleAsync(role);

            var chid = ServerDb.GetServer(sch.Guild.Id).WelcomeChannelId;
            var ch = sch.Guild.GetTextChannel(chid);
            await ch.SendMessageAsync(GetWelcomeMessageString(user));
        }
        private async Task Client_Log(LogMessage arg)
        {
            if (arg.Exception.Message.Contains("403") || arg.Exception.Message.Contains("500"))
                return;

            string shortdate = DateTime.Now.ToString("HH:mm:ss");
            string longdate = DateTime.Now.ToString();

            string exc = arg.Exception == null ? "" : $"\n`{arg.Exception.Message}- ` ```cs\n{arg.Exception.StackTrace ?? "..."}- ``` At: `{arg.Exception.TargetSite?.Name ?? "..."}- `";
            switch (arg.Severity)
            {
                case LogSeverity.Info:
                    Console.WriteLine($"I3 {longdate} at {arg.Source}] {arg.Message}{exc}");
                    break;
                case LogSeverity.Warning:
                    Console.WriteLine($"W2 {longdate} at {arg.Source}] {arg.Message}{exc}");
                    await WebhookClients.ErrorLogChannel.SendMessageAsync($":warning:`{shortdate}` - `{arg.Message}` s{exc}");
                    break;
                case LogSeverity.Error:
                    Console.WriteLine($"E1 {longdate} at {arg.Source}] {arg.Message}{exc}");
                    await WebhookClients.ErrorLogChannel.SendMessageAsync($"<:TickNo:577838859077943306>`{shortdate}` - `{arg.Message}` {exc}");
                    break;
                case LogSeverity.Critical:
                    Console.WriteLine($"C0 {longdate} at {arg.Source}] {arg.Message}{exc}");
                    await WebhookClients.ErrorLogChannel.SendMessageAsync($"<:TickNo:577838859077943306><:TickNo:577838859077943306>`{shortdate}` - `-{arg.Message}` {exc}");
                    break;
                default:
                    break;
            }

            if (arg.Severity == LogSeverity.Critical || ((arg.Message.Contains("Connected") || arg.Message.Contains("Disconnected")) && arg.Source.Contains("Shard")))
                await WebhookClients.NamikoLogChannel.SendMessageAsync($":information_source:`{shortdate} {arg.Source}]` {arg.Message}{exc}");
        }
        private async Task Client_ShardConnected(DiscordSocketClient arg)
        {
            Console.WriteLine($"{DateTime.Now} - Shard {arg.ShardId} Connected");
            _ = WebhookClients.NamikoLogChannel.SendMessageAsync($"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `Shard {arg.ShardId} Connected`");
        }
        private async Task Client_ShardDisconnected(Exception arg1, DiscordSocketClient arg2)
        {
            try
            {
                if (arg1.Message.Equals("The operation has timed out."))
                    return;

                await WebhookClients.NamikoLogChannel.SendMessageAsync(
                    $"<:TickNo:577838859077943306> `{DateTime.Now.ToString("HH:mm:ss")}` - `Shard {arg2.ShardId} Disconnected` - `{arg1.Message}`");
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
        private async Task Client_ShardReady(DiscordSocketClient arg)
        {
            try
            {
                Console.WriteLine($"{DateTime.Now} - Shard {arg.ShardId} ready. {arg.Guilds.Count} guilds.");
                _ = WebhookClients.NamikoLogChannel.SendMessageAsync($":european_castle: `{DateTime.Now.ToString("HH:mm:ss")}` - `Shard {arg.ShardId} ready - {arg.Guilds.Count} guilds`");

                int res;
                res = await CheckJoinedGuilds(arg);
                if (res > 0)
                {
                    Console.WriteLine($"{DateTime.Now} - Joined {res} guilds.");
                    _ = WebhookClients.NamikoLogChannel.SendMessageAsync($"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `Joined {res} guilds`");
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
        private async Task Client_ShardReady_DownloadUsers(DiscordSocketClient arg)
        {
            //method not thread safe, but it's ok.
            if (ShardsDownloadingUsers.Contains(arg.ShardId))
                return;

            try
            {
                ShardsDownloadingUsers.Add(arg.ShardId);

                await Task.Delay(5000);
                await arg.DownloadUsersAsync(arg.Guilds);
                int users = arg.Guilds.Sum(x => x.Users.Count);
                _ = WebhookClients.NamikoLogChannel.SendMessageAsync($":space_invader: `{DateTime.Now:HH:mm:ss}` - `Shard {arg.ShardId} downloaded {users} users.`");
            }
            finally
            {
                ShardsDownloadingUsers.Remove(arg.ShardId);
            }
        }

        // NAMIKO JOIN

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            DateTime now = DateTime.Now;
            Server server = ServerDb.GetServer(arg.Id) ?? new Server
            {
                GuildId = arg.Id,
                JoinDate = now
            };
            server.LeaveDate = null;
            server.Prefix = AppSettings.DefaultPrefix;
            await ServerDb.UpdateServer(server);

            if (server.JoinDate.Equals(now))
            {
                await BalanceDb.SetToasties(Client.CurrentUser.Id, 1000000, arg.Id);
            }

            SocketTextChannel ch = arg.SystemChannel ?? arg.DefaultChannel;
            try
            {
                await ch?.SendMessageAsync("Hi! Please take good care of me!", false, BasicUtil.GuildJoinEmbed(Client).Build());
            }
            catch { }
            await WebhookClients.GuildJoinLogChannel.SendMessageAsync($"<:TickYes:577838859107303424> {Client.CurrentUser.Username} joined `{arg.Id}` **{arg.Name}**.\nOwner: `{arg.Owner.Id}` **{arg.Owner}**");
        }
        private async Task Client_LeftGuild(SocketGuild arg)
        {
            if (!GuildLeaveEvent)
                return;

            var server = ServerDb.GetServer(arg.Id);
            server.LeaveDate = DateTime.Now;
            await ServerDb.UpdateServer(server);

            await WebhookClients.GuildJoinLogChannel.SendMessageAsync($"<:TickNo:577838859077943306> {Client.CurrentUser.Username} left `{arg.Id}` **{arg.Name}**.\nOwner: `{arg.Owner.Id}` **{arg.Owner}**");
        }

        // USER JOIN

        private async Task Client_UserJoinedWelcome(SocketGuildUser arg)
        {
            if (arg?.Guild?.Id == 417064769309245471)
                return;

            var server = ServerDb.GetServer(arg.Guild.Id);
            if (server != null && server.WelcomeChannelId != 0)
            {
                var ch = arg.Guild.GetTextChannel(server.WelcomeChannelId);
                if (ch != null)
                    await ch.SendMessageAsync(GetWelcomeMessageString(arg));
            }
        }
        private async Task Client_UserBannedLog(SocketUser arg1, SocketGuild arg2)
        {
            var ch = GetJoinLogChannel(arg2);
            if (ch != null)
                await ch.SendMessageAsync($":hammer: {UserInfo(arg1)} was banned.");
        }
        private async Task Client_UserLeftLog(SocketGuild guild, SocketUser user)
        {
            var ch = GetJoinLogChannel(guild);
            if (ch != null)
                await ch.SendMessageAsync($"<:TickNo:577838859077943306> {UserInfo(user)} left the server.");
        }
        private async Task Client_UserJoinedLog(SocketGuildUser arg)
        {
            var ch = GetJoinLogChannel(arg.Guild);
            if (ch != null)
                await ch.SendMessageAsync($"<:TickYes:577838859107303424> {UserInfo(arg)} joined the server.");
        }
        private static string UserInfo(SocketUser user)
        {
            return $"`{user.Id}` {user} {user.Mention}";
        }
        private static SocketTextChannel GetJoinLogChannel(SocketGuild guild)
        {
            return (SocketTextChannel)guild.GetChannel(ServerDb.GetServer(guild.Id).JoinLogChannelId);
        }

        // RANDOM
        
        private static string GetWelcomeMessageString(SocketUser user)
        {
            string message = WelcomeMessageDb.GetRandomMessage();
            message = message.Replace("@_", user.Mention);
            return message;
        }
        public static CancellationTokenSource GetCts()
        {
            return cts;
        }


        private static async Task<int> CheckJoinedGuilds(DiscordSocketClient shard = null)
        {
            IReadOnlyCollection<SocketGuild> guilds;
            if (shard == null)
                guilds = Client.Guilds;
            else
                guilds = shard.Guilds;

            var existingIds = ServerDb.GetNotLeft();
            var newIds = guilds.Where(x => !existingIds.Contains(x.Id)).Select(x => x.Id);

            int addedBal = await BalanceDb.AddNewServerBotBalance(newIds, Client.CurrentUser.Id);
            int added = await ServerDb.AddNewServers(newIds, AppSettings.DefaultPrefix);

            return added;
        }
        private static async Task<int> CheckLeftGuilds()
        {
            var guilds = Client.Guilds;
            HashSet<ulong> existingIds = new HashSet<ulong>(guilds.Select(x => x.Id));
            int left = 0;

            using (var db = new NamikoDbContext())
            {
                var zerotime = new DateTime(0);
                var now = DateTime.Now;
                IQueryable<Server> servers = db.Servers.AsQueryable().Where(x => x.LeaveDate == null && !existingIds.Contains(x.GuildId));

                await servers.ForEachAsync(x => x.LeaveDate = now);

                left = servers.Count();
                db.UpdateRange(servers);
                await db.SaveChangesAsync();
            }

            return left;
        }
    }
}
