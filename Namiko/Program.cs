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

#pragma warning disable CS1998

namespace Namiko
{
    public class Program
    {
        private static DiscordShardedClient Client;
        private static CommandService Commands;
        private static InteractionService Interactions;
        private static IServiceProvider Services;
        private static Dictionary<ulong, string> Prefixes;
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static readonly CancellationToken ct = cts.Token;
        public static HashSet<ulong> Blacklist;
        public static bool Development = false;
        private static bool Pause = false;
        public static bool GuildLeaveEvent = true;
        private static int ShardCount;


        private static int Startup = 0;
        private static int AllShardsReady = 0;
        private static int ReadyCount = 0;
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
            Client = new DiscordShardedClient(new DiscordSocketConfig {
                LogLevel = LogSeverity.Warning,
                DefaultRetryMode = RetryMode.Retry502,
                AlwaysDownloadUsers = false,
                MessageCacheSize = 0,
                LargeThreshold = 250,
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildVoiceStates |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildMessageReactions |
                    GatewayIntents.GuildScheduledEvents |
                    GatewayIntents.GuildInvites |
                    GatewayIntents.DirectMessages |
                    GatewayIntents.DirectMessageReactions 
            });

            Client.Log += Console_Log;
            Client.ShardConnected += SetUp_FirstShardConnected;
            Client.ShardReady += SetUp_AllShardsReady;

            Client.ShardConnected += Client_ShardConnected;
            Client.ShardDisconnected += Client_ShardDisconnected;
            Client.ShardReady += Client_ShardReady;
            Client.ShardReady += Client_ShardReady_DownloadUsers;

            Client.ReactionAdded += Client_ReactionAdded;
            Client.ReactionAdded += BanroyaleGame.HandleBanroyaleReactionAsync;
            Client.MessageReceived += Client_ReadCommand;
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
            Client.SlashCommandExecuted += Client_SlashCommandExecuted;

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = Discord.Commands.RunMode.Async,
                LogLevel = LogSeverity.Debug
            });

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.Log += Console_Log;
            Commands.Log += Error_Log;

            await Client.LoginAsync(TokenType.Bot, AppSettings.Token);
            await Client.StartAsync();
            _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
                $"------------------------------\n" +
                $"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `Logged in`\n" +
                $"------------------------------");

            ShardCount = Client.Shards.Count;

            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

            await Commands.AddModuleAsync(typeof(Banroulettes), Services);
            await Commands.AddModuleAsync(typeof(Banroyales), Services);
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

            Interactions = new InteractionService(Client, new InteractionServiceConfig
            {
                LogLevel = LogSeverity.Debug
            });
            Interactions.Log += Console_Log;
            await Interactions.AddModuleAsync(typeof(Banroulettes), Services);
            await Interactions.AddModuleAsync(typeof(Currency), Services);
            await Interactions.AddModuleAsync(typeof(Waifus), Services);
            //await Interactions.AddModuleAsync(typeof(Basic), Services);


            Interactions.SlashCommandExecuted += Interactions_SlashCommandExecuted;

            try
            {
                await Task.Delay(-1, ct);
            }
            catch { }
            cts.Dispose();
            Console.WriteLine("Shutting down...");
            await Client.LogoutAsync();
        }

        private async Task Client_SlashCommandExecuted(SocketSlashCommand arg)
        {
            var context = new CustomInteractionContext(Client, arg, arg.Channel);
            await arg.DeferAsync();
            await Interactions.ExecuteCommandAsync(context, Services);
        }
        private async Task Interactions_SlashCommandExecuted(SlashCommandInfo cmd, IInteractionContext con, Discord.Interactions.IResult res)
        {
            var context = (CustomInteractionContext)con;
            string cmdName = cmd?.Name;

            if (!res.IsSuccess)
            {
                // If the command is found but failed then send help message for said command
                if (!(res.Error == InteractionCommandError.UnknownCommand || res.Error == InteractionCommandError.Exception))
                {
                    string reason = res.ErrorReason + "\n";
                    if (res.Error != InteractionCommandError.UnmetPrecondition)
                        reason += CommandHelpString(cmdName, GetPrefix(context.Guild));
                    await context.ReplyAsync(embed: new EmbedBuilder().WithColor(Color.DarkRed).WithDescription(":x: " + reason).Build());
                }
            }

            // prevent doing "Bot is thinking..." for 15min if command fails
            //if (await context.Interaction.GetOriginalResponseAsync() == null)
            //{
            //    await context.ReplyAsync(embed: new EmbedBuilderPrepared(":x: Unkown error occured").Build());
            //}
        }

        // SET-UP  

        private static void SetUp()
        {
            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Development":
                    Development = true;
                    Pause = false;
                    break;
                default:
                    Console.WriteLine("Entry: " + Assembly.GetEntryAssembly().Location);
                    _ = ImgurAPI.ImgurSetup();
                    break;
            }
            NamikoDbContext.ConnectionString = AppSettings.ConnectionString;
            _ = LootboxStats.Reload(Locations.LootboxStatsJSON);
            Prefixes = ServerDb.GetPrefixes();
            Images.ReactionImageCommands = ImageDb.GetReactionImageCommandHashSet();
            Blacklist = BlacklistDb.GetAll();
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
                    await Client.SetActivityAsync(new Game($"Chinese Cartoons. Try @{arg.CurrentUser.Username} help", ActivityType.Watching));
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
                await Timers.SetUp();
            else
                await Timers.SetUpRelease();
        }
        private async Task SetUp_AllShardsReady(DiscordSocketClient arg)
        {
            ReadyCount++;
            // Making sure this part only runs once, unless an exception is thrown. Thread safe.
            if (ReadyCount >= ShardCount && Interlocked.Exchange(ref AllShardsReady, 1) == 0)
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


        // COMMANDS

        private async Task Client_ReadCommand(SocketMessage MessageParam)
        {
            if (Client == null)
                return;

            if (!(MessageParam is SocketUserMessage Message))
                return;

            var context = new CustomCommandContext(Client, Message);
            string prefix = GetPrefix(context.Guild);

            if (Blacklist.Contains(context.User.Id) || (context.Guild != null && Blacklist.Contains(context.Guild.Id)) || (context.Channel != null && Blacklist.Contains(context.Channel.Id)))
                return;
            if (context.User.IsBot)
                return;
            if (context.Message == null || context.Message.Content == "")
                return;
            if (BlacklistedChannelDb.IsBlacklisted(context.Channel.Id))
                return;
            if (RateLimit.InvokeLockout.TryGetValue(context.Channel.Id, out var time) && time > DateTime.Now)
                return;

            await SpecialModeResponse(context);
            await SpecialResponse(Message);

            int ArgPos = 0;
            bool isPrefixed = Message.HasStringPrefix(prefix, ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos);

            if (!isPrefixed)
                return;

            var cmds = Commands.Search(context, ArgPos);
            if (cmds.IsSuccess)
            {
                if (context.Guild != null && cmds.Commands.Any(x => DisabledCommandHandler.IsDisabled(x.Command.Name, context.Guild.Id, DisabledCommandType.Command)))
                    return;
                else if (context.Guild != null && cmds.Commands.Any(x => DisabledCommandHandler.IsDisabled(x.Command.Module.Name, context.Guild.Id, DisabledCommandType.Module)))
                    return;

                else if (!RateLimit.CanExecute(context.Channel.Id))
                {
                    await context.Channel.SendMessageAsync($"Woah there, Senpai, calm down! I locked this channel for **{RateLimit.InvokeLockoutPeriod.Seconds}** seconds <:MeguExploded:627470499278094337>\n" +
                        $"You can only use **{RateLimit.InvokeLimit}** commands per **{RateLimit.InvokeLimitPeriod.Seconds}** seconds per channel.");
                    return;
                }

                else if (Pause && context.User.Id != AppSettings.OwnerId)
                {
                    await context.Channel.SendMessageAsync("Commands disabled temporarily. Try again later.");
                    return;
                }

                else if (context.Channel is SocketTextChannel ch
                    && (!ch.Guild.CurrentUser.GetPermissions(ch).Has(ChannelPermission.SendMessages) || !ch.Guild.CurrentUser.GetPermissions(ch).Has(ChannelPermission.EmbedLinks)))
                {
                    var dm = await context.User.CreateDMChannelAsync();
                    await dm.SendMessageAsync(embed: new EmbedBuilderPrepared(context.Guild.CurrentUser)
                        .WithDescription($"I don't have permission to reply to you in **{ch.Name}**.\n" +
                        $"Make sure I have a role that allows me to send messages and embed links in the channels you want to use me in.")
                        .WithImageUrl("https://i.imgur.com/lrPHjyt.png")
                        .Build());
                    return;
                }
            }

            _ = Commands.ExecuteAsync(context, ArgPos, Services);
        }
        private async Task Commands_CommandExecuted(Optional<CommandInfo> cmd, ICommandContext context, Discord.Commands.IResult res)
        {
            string cmdName = cmd.IsSpecified ? cmd.Value.Name : null;
            bool success = res.IsSuccess;

            if (!success)
            {
                // Try sending a reaction image if there is no such command
                if (await new Images().SendRandomImage(context))
                {
                    cmdName = "ReactionImage";
                    success = true;
                }

                // If the command is found but failed then send help message for said command
                else
                if (!(res.Error == CommandError.UnknownCommand || res.Error == CommandError.Exception))
                {
                    string reason = res.ErrorReason + "\n";
                    if (res.Error != CommandError.UnmetPrecondition)
                        reason += CommandHelpString(context.Message.Content.Split(null)[0].Replace(GetPrefix(context.Guild), ""), GetPrefix(context.Guild));
                    await context.Channel.SendMessageAsync(embed: new EmbedBuilder().WithColor(Color.DarkRed).WithDescription(":x: " + reason).Build());
                }
            }
            // If the command is found and completed but it is a music command in a guild with no premium - set as failed
            else
            if ((cmdName == nameof(Music.Join) || cmdName == nameof(Music.Play) || cmdName == nameof(Music.PlayNext) || cmdName == nameof(Music.PlayFirst))
                && !(PremiumDb.IsPremium(context.Guild.Id, ProType.GuildPlus) || PremiumDb.IsPremium(context.Guild.Id, ProType.Guild)))
            {
                success = false;
            }


            // If command is found - save a log of it
            if (cmdName != null && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
                await Stats.LogCommand(cmdName, context, success);
        }
        private async Task Error_Log(LogMessage logMessage)
        {
            if (logMessage.Exception is CommandException cmdException)
            {
                if (cmdException.InnerException is NamikoException ex)
                {
                    await cmdException.Context.Channel.SendMessageAsync(":x: " + ex.Message);
                }
                else
                {
                    SentrySdk.WithScope(scope =>
                    {
                        scope.SetTag("Command", cmdException.Command.Name);
                        scope.SetExtra("GuildId", cmdException.Context.Guild.Id);
                        scope.SetExtra("Guild", cmdException.Context.Guild.Name);
                        scope.SetExtra("GuildOwnerId", cmdException.Context.Guild.OwnerId);
                        scope.SetExtra("ChannelId", cmdException.Context.Channel.Id);
                        scope.SetExtra("Channel", cmdException.Context.Channel.Name);
                        scope.SetExtra("UserId", cmdException.Context.User.Id);
                        scope.SetExtra("User", cmdException.Context.User.Username);
                        scope.SetExtra("MessageId", cmdException.Context.Message.Id);
                        scope.SetExtra("Message", cmdException.Context.Message.Content);
                        if (cmdException.InnerException is HttpException)
                            scope.Level = Sentry.Protocol.SentryLevel.Warning;
                        SentrySdk.CaptureException(cmdException.InnerException);
                    });

                    if (cmdException.Command.Module.Name.Equals(nameof(WaifuEditing)))
                    {
                        await cmdException.Context.Channel.SendMessageAsync(cmdException.InnerException.Message);
                    }
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
        private Task Console_Log(LogMessage arg)
        {
            Logger.Log(arg.ToString(), arg.Severity);
            return Task.CompletedTask;
        }
        private async Task Client_ShardConnected(DiscordSocketClient arg)
        {
            Console.WriteLine($"{DateTime.Now} - Shard {arg.ShardId} Connected");
            _ = WebhookClients.NamikoLogChannel.SendMessageAsync($"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `Shard {arg.ShardId} Connected`");
            if (Development)
            {
                await Interactions.RegisterCommandsToGuildAsync(418900885079588884, true);
            }
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
                await ch?.SendMessageAsync("Hi! Please take good care of me!", false, BasicUtil.GuildJoinEmbed(server.Prefix).Build());
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

        // PREFIXES

        public static string GetPrefix(ulong guildId)
        {
            return Prefixes.GetValueOrDefault(guildId) ?? AppSettings.DefaultPrefix;
        }
        public static string GetPrefix(ICustomContext context)
        {
            return context.Guild == null ? AppSettings.DefaultPrefix : GetPrefix(context.Guild.Id);
        }
        public static string GetPrefix(ICommandContext context)
        {
            return context.Guild == null ? AppSettings.DefaultPrefix : GetPrefix(context.Guild.Id);
        }
        public static string GetPrefix(SocketGuildUser user)
        {
            return user.Guild == null ? AppSettings.DefaultPrefix : GetPrefix(user.Guild.Id);
        }
        public static string GetPrefix(IGuild guild)
        {
            return guild == null ? AppSettings.DefaultPrefix : GetPrefix(guild.Id);
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
        public static bool SetPause()
        {
            if (Pause)
                Pause = false;
            else
                Pause = true;

            return Pause;
        }

        private async Task SpecialModeResponse(ICommandContext context)
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

            if (Client?.CurrentUser == null)
                return;

            string msg = message.Content.Replace("!", "");
            string mention = Client.CurrentUser.Mention.Replace("!", "");
            if (msg.Contains(mention) && (!msg.StartsWith(mention) || msg.Equals(mention)))
            {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} <a:loveme:536705504798441483>");
            }
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
