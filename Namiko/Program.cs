using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Namiko.Data;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Victoria;

#pragma warning disable CS1998

namespace Namiko
{
    public class Program
    {
        private static DiscordShardedClient Client;
        private static CommandService Commands;
        private static IServiceProvider Services;
        private static bool Pause = false;
        private static readonly Dictionary<ulong, string> Prefixes = new Dictionary<ulong, string>();
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static readonly CancellationToken ct = cts.Token;
        public static HashSet<ulong> Blacklist;
        private static bool Launch = true;
        public static bool Debug = false;
        private static bool Diag = false;
        private static bool ReadySetup = true;
        private static int ShardCount;

        static void Main(string[] args)
        {
            using (Mutex mutex = new Mutex(true, "Global-NamikoBot", out bool createdNew))
            {
                if (!createdNew)
                {
                    Console.WriteLine("Instance Already Running!");
                    return;
                }

                SetUpConfig();
                using (SentrySdk.Init(Config.SentryWebhook))
                {
                    SetUp();
                    new Program().MainAsync().GetAwaiter().GetResult();
                }
            }
        }
        private async Task MainAsync()
        {
            Client = new DiscordShardedClient(new DiscordSocketConfig {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.Retry502,
                ExclusiveBulkDelete = true,
                AlwaysDownloadUsers = false,
                MessageCacheSize = 0,
                LargeThreshold = 50
            });
            
            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = Diag ? RunMode.Sync : RunMode.Async,
                LogLevel = LogSeverity.Info
            });
            
            //Client.Ready += Client_Ready;
            Client.ShardReady += Client_ShardReady;
            //Client.Log += Client_Log;
            Client.ReactionAdded += Client_ReactionAdded;
            Client.JoinedGuild += Client_JoinedGuild;
            Client.LeftGuild += Client_LeftGuild;
            Client.MessageReceived += Client_ReadCommand;
            Client.UserVoiceStateUpdated += Client_UserVoiceChannel;

            // Join/leave logging.
            if (!Debug)
                Client.UserJoined += Client_UserJoinedWelcome;
            Client.UserJoined += Client_UserJoinedLog;
            Client.UserLeft += Client_UserLeftLog;
            Client.UserBanned += Client_UserBannedLog;

            //Commands.Log += Client_Log;
            Commands.CommandExecuted += Commands_CommandExecuted;

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();

            ShardCount = Client.Shards.Count;

            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton<InteractiveService>()
                .AddSingleton<LavaShardClient>()
                .AddSingleton<LavaRestClient>()
                .BuildServiceProvider();

            await Commands.AddModuleAsync(typeof(Banroulettes), Services);
            await Commands.AddModuleAsync(typeof(Basic), Services);
            await Commands.AddModuleAsync(typeof(Currency), Services);
            await Commands.AddModuleAsync(typeof(Images), Services);
            await Commands.AddModuleAsync(typeof(Roles), Services);
            await Commands.AddModuleAsync(typeof(ServerModule), Services);
            await Commands.AddModuleAsync(typeof(Special), Services);
            await Commands.AddModuleAsync(typeof(SpecialModes), Services);
            await Commands.AddModuleAsync(typeof(User), Services);
            await Commands.AddModuleAsync(typeof(Waifus), Services);
            await Commands.AddModuleAsync(typeof(WaifuEditing), Services);
            await Commands.AddModuleAsync(typeof(Web), Services);
            await Commands.AddModuleAsync(typeof(Music), Services);

            try
            {
                await Task.Delay(-1, ct);
            }
            catch { }
            cts.Dispose();
        }


        // COMMANDS

        private async Task Client_ReadCommand(SocketMessage MessageParam)
        {
            if (Client == null)
                return; 

            var Message = MessageParam as SocketUserMessage;
            if (Message == null)
                return;

            var Context = new ShardedCommandContext(Client, Message);
            string prefix = GetPrefix(Context);

            if (Blacklist.Contains(Context.User.Id) || (Context.Guild != null && Blacklist.Contains(Context.Guild.Id)) || (Context.Channel != null && Blacklist.Contains(Context.Channel.Id)))
                return;
            if (Context.User.IsBot)
                return;
            if (Context.Message == null || Context.Message.Content == "")
                return;
            if (BlacklistedChannelDb.IsBlacklisted(Context.Channel.Id))
                return;
            if (RateLimit.InvokeLockout.TryGetValue(Context.Channel.Id, out var time) && time > DateTime.Now)
                return;

            await SpecialModeResponse(Context);
            await SpecialResponse(Message);

            int ArgPos = 0;
            bool isPrefixed = Message.HasStringPrefix(prefix, ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos);
            if (!isPrefixed && !Pause)
            {
                await Blackjack.BlackjackInput(Context);
                return;
            }

            if (!isPrefixed)
                return;

            var cmds = Commands.Search(Context, ArgPos);
            if (cmds.IsSuccess && !RateLimit.CanExecute(Context.Channel.Id))
            {
                await Context.Channel.SendMessageAsync($"Woah there, Senpai, calm down! I locked this channel for **{RateLimit.InvokeLockoutPeriod.Seconds}** seconds <:MeguExploded:627470499278094337>\n" +
                    $"You can only use **{RateLimit.InvokeLimit}** commands per **{RateLimit.InvokeLimitPeriod.Seconds}** seconds per channel.");
                return;
            }

            if (Pause && Context.User.Id != Config.OwnerId)
            {
                if (cmds.IsSuccess && Pause && Context.User.Id != Config.OwnerId)
                {
                    await Context.Channel.SendMessageAsync("Commands disabled temporarily. Try again later.");
                    return;
                }
            }

            if (Diag)
            {
                var watch = new Stopwatch();
                watch.Start();
                var task = Commands.ExecuteAsync(Context, ArgPos, Services).ContinueWith(x =>
                {
                    watch.Stop();
                    Context.Channel.SendMessageAsync($"Execution time: `{watch.ElapsedMilliseconds}ms`");
                });
                return;
            }

            var res = await Commands.ExecuteAsync(Context, ArgPos, Services);

            string text = null;
            if (!res.IsSuccess)
            {
                if (await new Images().SendRandomImage(Context))
                    text = "ReactionImage";

                else if (!(res.Error == CommandError.UnknownCommand))
                {
                    string reason = res.ErrorReason + "\n";
                    if (res.Error != CommandError.UnmetPrecondition)
                        reason += CommandHelpString(MessageParam.Content.Split(null)[0].Replace(prefix, ""), prefix);
                    await Context.Channel.SendMessageAsync(reason);
                    return;
                }
            }

            if (text == null)
                return;

            Stats.IncrementServer(Context.Guild.Id);
            Stats.IncrementCommand(text);
            Stats.IncrementCalls();
        }
        private async Task Commands_CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
                return;

            Stats.IncrementServer(arg2.Guild.Id);
            Stats.IncrementCommand(arg1.Value.Name);
            Stats.IncrementCalls();
        }

        // EVENTS

        private async Task Client_UserVoiceChannel(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (user.IsBot)
                return;
            if (Music.LavaClient == null)
                return;
            if (user == null || !(user is SocketGuildUser))
                return;
            if (((SocketGuildUser)user).Guild == null)
                return;

            var player = Music.LavaClient?.GetPlayer(((SocketGuildUser)user).Guild.Id);
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
        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if(arg3.MessageId != 511188952640913408)
                return;

            SocketTextChannel sch = arg2 as SocketTextChannel;
            var user = sch.Guild.GetUser(arg3.UserId);
            var role = sch.Guild.GetRole(417112174926888961);

            if (RoleUtil.HasRole(user, role))
                return;

            await user.AddRoleAsync(role);
            
            var chid = ServerDb.GetServer(sch.Guild.Id).WelcomeChannelId;
            var ch = sch.Guild.GetTextChannel(chid);
            await ch.SendMessageAsync(GetWelcomeMessageString(user));
        }
        //private async Task Client_UserLeftToasties(SocketGuildUser arg)
        //{
        //    var amount = ToastieDb.GetToasties(arg.Id, arg.Guild.Id) / 4;
        //    await ToastieDb.AddToasties(arg.Id, -amount, arg.Guild.Id);
        //    await ToastieDb.AddToasties(Client.CurrentUser.Id, amount, arg.Guild.Id);
        //}
        private async Task Client_Log(LogMessage arg)
        {
            if (arg.Exception.Message.Contains("403") || arg.Exception.Message.Contains("500"))
                return; 

            string shortdate = DateTime.Now.ToString("HH:mm:ss");
            string longdate = DateTime.Now.ToString();

            string exc = arg.Exception == null ? "" : $"\n`{arg.Exception.Message}- ` ```cs\n{arg.Exception.StackTrace ?? "..."}- ``` At: `{arg.Exception.TargetSite?.Name ?? "..."}- `";
            switch(arg.Severity)
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

            if(arg.Severity == LogSeverity.Critical || ((arg.Message.Contains("Connected") || arg.Message.Contains("Disconnected")) && arg.Source.Contains("Shard")))
                await WebhookClients.NamikoLogChannel.SendMessageAsync($":information_source:`{shortdate} {arg.Source}]` {arg.Message}{exc}");
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
            server.LeaveDate = new DateTime(0);
            server.Prefix = Config.DefaultPrefix;
            await ServerDb.UpdateServer(server);

            if(server.JoinDate.Equals(now))
            {
                await ToastieDb.SetToasties(Client.CurrentUser.Id, 1000000, arg.Id);
            }

            SocketTextChannel ch = arg.SystemChannel ?? arg.DefaultChannel;
            try
            {
                await ch?.SendMessageAsync("Hi! Please take good care of me!", false, BasicUtil.GuildJoinEmbed(server.Prefix).Build());
            } catch { }
            await WebhookClients.GuildJoinLogChannel.SendMessageAsync($"<:TickYes:577838859107303424> {Client.CurrentUser.Username} joined `{arg.Id}` **{arg.Name}**.\nOwner: `{arg.Owner.Id}` **{arg.Owner}**");
        }
        private async Task Client_LeftGuild(SocketGuild arg)
        {
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
        private async Task Client_UserLeftLog(SocketGuildUser arg)
        {
            var ch = GetJoinLogChannel(arg.Guild);
            if (ch != null)
                await ch.SendMessageAsync($"<:TickNo:577838859077943306> {UserInfo(arg)} left the server.");
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
            return (SocketTextChannel) guild.GetChannel(ServerDb.GetServer(guild.Id).JoinLogChannelId);
        }

        // SET-UP / READY

        private int ReadyCount = 0;
        private async Task Client_ShardReady(DiscordSocketClient arg)
        {
            if (ReadySetup)
            {
                ReadySetup = false;
                WebUtil.SetUpDbl(Client.CurrentUser.Id);
                _ = Music.Initialize(Client);
                _ = Client.SetActivityAsync(new Game("Chinese Cartoons. Try @Namiko help", ActivityType.Watching));
            }

            ReadyCount++;
            string name = Client.CurrentUser.Username;
            Console.WriteLine($"{DateTime.Now} - Shard {arg.ShardId} Ready");
            _ = WebhookClients.NamikoLogChannel.SendMessageAsync($"`{DateTime.Now.ToString("HH:mm:ss")}` - `Shard {arg.ShardId} Ready`");
            
            int res;
            res = await CheckJoinedGuilds(arg);
            if (res > 0)
            {
                Console.WriteLine($"{DateTime.Now} - Joined {res} Guilds.");
                _ = WebhookClients.NamikoLogChannel.SendMessageAsync($"`{DateTime.Now.ToString("HH:mm:ss")}` <:TickYes:577838859107303424> {name} joined **{res}** Guilds.");
            }

            if (Launch && ReadyCount >= ShardCount)
            {
                Launch = false;
                Ready();
                res = await CheckLeftGuilds();
                if (res > 0)
                {
                    Console.WriteLine($"{DateTime.Now} - Left {res} Guilds.");
                    _ = WebhookClients.NamikoLogChannel.SendMessageAsync($"`{DateTime.Now.ToString("HH:mm:ss")}` <:TickNo:577838859077943306> {name} left {res} Guilds.`");
                }
            }
        }
        private async void Ready()
        {
            if (!Debug)
            {
                RedditAPI.Poke();
                ImgurAPI.Poke();
            }
            StartTimers();
        }
        private static void SetUp()
        {
            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Development":
                    SetUpDebug();
                    break;
                default:
                    SetUpRelease();
                    break;
            }
        }
        private static void SetUpConfig()
        {
            string JSON = "";
            string JSONLocation;
            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Development":
                    JSONLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.1\Namiko.dll", @"Data\Settings.json");
                    break;
                default:
                    JSONLocation = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/Settings.json");
                    break;
            }

            using (var Stream = new FileStream(JSONLocation, FileMode.Open, FileAccess.Read))
            using (var ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            JsonConvert.DeserializeObject<Config>(JSON);
            return;
        }
        public static bool SetPause()
        {
            if (Pause)
                Pause = false;
            else
                Pause = true;

            return Pause;
        }
        private static void SetUpDebug()
        {
            Diag = false;
            Debug = true;
            Pause = true;
            Locations.SetUpDebug();
            _ = LootboxStats.Reload();
            SetUpPrefixes();
            Blacklist = BlacklistDb.GetAll();
        }
        private static void SetUpRelease()
        {
            Console.WriteLine("Entry: " + Assembly.GetEntryAssembly().Location);
            Locations.SetUpRelease();
            _ = LootboxStats.Reload();
            SetUpPrefixes();
            Blacklist = BlacklistDb.GetAll();
        }
        private static void StartTimers()
        {
            if (Debug)
                _ = Timers.SetUp();
            else
                Timers.SetUpRelease();
        }
        private static async Task<int> CheckJoinedGuilds(DiscordSocketClient shard = null)
        {
            //IReadOnlyCollection<SocketGuild> guilds = null;
            //if (shard == null)
            //    guilds = Client.Guilds;
            //else
            //    guilds = shard.Guilds;

            //var zerotime = new DateTime(0);
            //int added = 0;

            ////var servers = new List<Server>();
            ////var toasties = new List<Balance>();
            //using (var db = new SqliteDbContext())
            //{
            //    var ids = db.Servers.Where(x => x.LeaveDate == zerotime).Select(x => x.GuildId).ToHashSet();
            //    foreach (var guild in guilds)
            //    {
            //        if (!ids.Contains(guild.Id))
            //        {
            //            db.Servers.Add(new Server
            //            {
            //                GuildId = guild.Id,
            //                JoinDate = System.DateTime.Now,
            //                LeaveDate = zerotime,
            //                Prefix = Config.DefaultPrefix
            //            });

            //            var bal = await db.Toasties.FirstOrDefaultAsync(x => x.UserId == Client.CurrentUser.Id && x.GuildId == guild.Id);
            //            if (bal == null)
            //                db.Toasties.Add(new Balance { UserId = Client.CurrentUser.Id, Amount = 1000000, GuildId = guild.Id });

            //            added++;
            //        }
            //    }
            //    //db.AddRange(servers);
            //    //db.AddRange(toasties);
            //    await db.SaveChangesAsync();
            //}

            //return added;

            IReadOnlyCollection<SocketGuild> guilds;
            if (shard == null)
                guilds = Client.Guilds;
            else
                guilds = shard.Guilds;

            var existingIds = ServerDb.GetNotLeft();
            var zerotime = new DateTime(0);

            int added = 0;
            foreach (var guild in guilds)
            {
                if (!existingIds.Contains(guild.Id))
                {
                    var server = new Server
                    {
                        GuildId = guild.Id,
                        JoinDate = System.DateTime.Now,
                        LeaveDate = zerotime,
                        Prefix = Config.DefaultPrefix
                    };
                    await ServerDb.UpdateServer(server);
                    await ToastieDb.SetToasties(Client.CurrentUser.Id, 1000000, guild.Id);
                    added++;
                }
            }

            return added;
        }
        private static async Task<int> CheckLeftGuilds()
        {
            var guilds = Client.Guilds;
            HashSet<ulong> existingIds = new HashSet<ulong>(guilds.Select(x => x.Id));
            int left = 0;

            //var servers = ServerDb.GetNotLeft();

            //foreach (var srv in servers)
            //{
            //    if (!existingIds.Contains(srv.GuildId))
            //    {
            //        srv.LeaveDate = DateTime.Now;
            //        await ServerDb.UpdateServer(srv);
            //        left++;
            //    }
            //}

            using (var db = new SqliteDbContext())
            {
                var zerotime = new DateTime(0);
                var now = DateTime.Now;
                var servers = db.Servers.Where(x => x.LeaveDate == zerotime && !existingIds.Contains(x.GuildId));

                await servers.ForEachAsync(x => x.LeaveDate = now);

                left = servers.Count();
                db.UpdateRange(servers);
                await db.SaveChangesAsync();
            }

            return left;
        }

        // PREFIXES

        public static string GetPrefix(ulong guildId)
        {
            return Prefixes.GetValueOrDefault(guildId) ?? Config.DefaultPrefix;
        }
        public static string GetPrefix(SocketCommandContext context)
        {
            return context.Guild == null ? Config.DefaultPrefix : GetPrefix(context.Guild.Id);
        }
        public static string GetPrefix(SocketGuildUser user)
        {
            return user.Guild == null ? Config.DefaultPrefix : GetPrefix(user.Guild.Id);
        }
        public static string GetPrefix(SocketGuild guild)
        {
            return guild == null ? Config.DefaultPrefix : GetPrefix(guild.Id);
        }
        private static void SetUpPrefixes()
        {
            var servers = ServerDb.GetAll();
            foreach (var x in servers)
            {
                try
                {
                    Prefixes.Add(x.GuildId, x.Prefix);
                }
                catch { }
            }
        }
        public static bool UpdatePrefix(ulong guildId, string prefix)
        {
            if (guildId == 0 || prefix == null || prefix == "")
                return false;

            if(Prefixes.GetValueOrDefault(guildId) != null)
                Prefixes.Remove(guildId);

            Prefixes.Add(guildId, prefix);
            return true;
        }

        // RANDOM

        public static string CommandHelpString(string commandName, string prefix)
        {
            try
            {
                var cmd = Commands.Commands.Where(x => x.Aliases.Any(y => y.Equals(commandName, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
                string str = cmd.Summary;
                string result = "**Description**: " + str;
                result = result.Replace("!", prefix);
                return result;
            } catch
            {
                return "";
            }
        }
        //  public static EmbedBuilder CommandHelpEmbed(string commandName)
        //  {
        //      var cmd = Commands.Commands.Where(x => x.Aliases.Any(y => y.Equals(commandName))).FirstOrDefault();
        //      return new BasicCommands().CommandHelpEmbed(cmd);
        //  }
        public static DiscordShardedClient GetClient()
        {
            return Client;
        }
        public static CommandService GetCommands()
        {
            return Commands;
        }
        public static IServiceProvider GetServices()
        {
            return Services;
        }
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

        private async Task SpecialModeResponse(ShardedCommandContext context)
        {
            if (SpecialModes.ChristmasModeEnable)
                await new SpecialModes().Christmas(context);

            if (SpecialModes.SpookModeEnable)
                await new SpecialModes().Spook(context);
        }
        private async Task SpecialResponse(SocketUserMessage message)
        {
            if (Client == null)
                return;

            if (message.Content.Contains("rep", StringComparison.OrdinalIgnoreCase))
                return;

            if (message.Content.StartsWith("Hi Namiko", StringComparison.OrdinalIgnoreCase))
            {
                await message.Channel.SendMessageAsync($"Hi {message.Author.Mention} :fox:");
                return;
            }
            if (message.Content.StartsWith("Namiko-sama", StringComparison.OrdinalIgnoreCase))
            {
                var msgs = new List<string>
                {
                    $"Rise, {message.Author.Mention}",
                    $"Yes, {message.Author.Mention}?"
                };
                await message.Channel.SendMessageAsync(msgs[new Random().Next(msgs.Count)]);
                return;
            }

            string msg = message.Content.Replace("!", "");
            string mention = Client.CurrentUser.Mention.Replace("!", "");
            if (msg.Contains(mention) && (!msg.StartsWith(mention) || msg.Equals(mention)))
            {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} <a:loveme:536705504798441483>");
            }
        }
    }
}
