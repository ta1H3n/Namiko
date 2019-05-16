﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;
using Discord.WebSocket;


using Newtonsoft.Json;

using Namiko.Data;
using System.Timers;


using Microsoft.Extensions.DependencyInjection;
using Discord.Addons.Interactive;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable CS1998

namespace Namiko
{
    public class Program
    {
        private static DiscordShardedClient Client;
        private static CommandService Commands;
        private static IServiceProvider Services;
        private static bool Pause = false;
        private static Dictionary<ulong, string> Prefixes = new Dictionary<ulong, string>();
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static CancellationToken ct = cts.Token;
        private static bool Launch = true;
        public static bool Debug = false;

        static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();
        private async Task MainAsync()
        {
            SetUpDebug();
            //SetUpRelease();

            Client = new DiscordShardedClient(new DiscordSocketConfig {
                LogLevel = LogSeverity.Info
            });
            
            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Critical
            });
            
            //Client.Ready += Client_Ready;
            Client.ShardReady += Client_ShardReady;
            Client.Log += Client_Log;
            Client.ReactionAdded += Client_ReactionAdded;
            Client.JoinedGuild += Client_JoinedGuild;
            Client.LeftGuild += Client_LeftGuild;
            Client.MessageReceived += Client_MessageReceived;
            Client.MessageReceived += Client_MessageReceivedSpecialModes;
            Client.MessageReceived += Client_MessageReceivedHeart;

            // Join/leave logging.
            Client.UserJoined += Client_UserJoinedWelcome;
            Client.UserJoined += Client_UserJoinedLog;
            Client.UserLeft += Client_UserLeftLog;
            Client.UserBanned += Client_UserBannedLog;

            Commands.Log += Client_Log;
            Commands.CommandExecuted += Commands_CommandExecuted;
            
            await Client.LoginAsync(TokenType.Bot, ParseSettingsJson());
            await Client.StartAsync();

            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

            //await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
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
            await Commands.AddModuleAsync(typeof(Web), Services);

            try
            {
                await Task.Delay(-1, ct);
            }
            catch { }
            cts.Dispose();
        }
        

        // EVENTS

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if(arg3.MessageId != 511188952640913408)
            {
                return;
            }

            SocketTextChannel sch = arg2 as SocketTextChannel;
            var user = sch.Guild.GetUser(arg3.UserId);
            var role = sch.Guild.GetRole(417112174926888961);

            if (RoleUtil.HasRole(user, role))
            {
                return;
            }

            await user.AddRoleAsync(role);
            
            var chid = ServerDb.GetServer(sch.Guild.Id).WelcomeChannelId;
            var ch = sch.Guild.GetTextChannel(chid);
            await ch.SendMessageAsync(GetWelcomeMessageString(user));
        }
        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            var ch = await arg1.GetOrCreateDMChannelAsync();
            await ch.SendMessageAsync(WelcomeDm());
        }
        private async Task Client_UserJoinedWelcome(SocketGuildUser arg)
        {
            if (arg.Guild.Id == 417064769309245471)
                return;

            var chid = ServerDb.GetServer(arg.Guild.Id).WelcomeChannelId;
            var ch = arg.Guild.GetTextChannel(chid);
            await ch.SendMessageAsync(GetWelcomeMessageString(arg));
        }
        private async Task Client_UserLeftToasties(SocketGuildUser arg)
        {
            var amount = ToastieDb.GetToasties(arg.Id, arg.Guild.Id) / 4;
            await ToastieDb.AddToasties(arg.Id, -amount, arg.Guild.Id);
            await ToastieDb.AddToasties(Client.CurrentUser.Id, amount, arg.Guild.Id);
        }
        private async Task Client_Log(LogMessage arg)
        {
            string message = $"{DateTime.Now} at {arg.Source}] {arg.Message}";
            Console.WriteLine(message);

            if(arg.Severity == LogSeverity.Critical)
            await (await Client.GetUser(StaticSettings.owner).GetOrCreateDMChannelAsync()).SendMessageAsync($"`{message}`");
        }
        private async Task Client_MessageReceived(SocketMessage MessageParam)
        {
            if (MessageParam.Channel.Id == 420319722723082275)
                return;

            var Message = MessageParam as SocketUserMessage;
            var Context = new ShardedCommandContext(Client, Message);

            string prefix = GetPrefix(Context);

            if (Context.Message == null || Context.Message.Content == "")
                return;
            if (Context.User.IsBot)
                return;
            if (BlacklistedChannelDb.IsBlacklisted(Context.Channel.Id))
                return;

            if(Message.Content.StartsWith("Hi Namiko", StringComparison.InvariantCultureIgnoreCase))
            {
                await Message.Channel.SendMessageAsync($"Hi {Context.User.Mention} :fox:");
                return;
            }

            int ArgPos = 0;
            bool isPrefixed = Message.HasStringPrefix(prefix, ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos);
            if (!isPrefixed && !Pause)
            {
                await Blackjack.BlackjackInput(Context);
                return;
            }

            if (!isPrefixed)
                return;
            if (Pause && Context.User.Id != StaticSettings.owner)
            {
                var cmds = Commands.Search(Context, ArgPos);
                if (cmds.IsSuccess && Pause && Context.User.Id != StaticSettings.owner)
                {
                    await Context.Channel.SendMessageAsync("Commands disabled temporarily. Try again later.");
                    return;
                }
            }
            
            var Result = await Commands.ExecuteAsync(Context, ArgPos, Services);
            string text = null;

            if(!Result.IsSuccess)
            {
                if (await new Basic().Help(Context, Commands))
                    text = "Help";
                else if (await new Images().SendRandomImage(Context))
                    text = "ReactionImage";

                else if (!(Result.Error == CommandError.UnknownCommand))
                {
                    string reason = Result.ErrorReason + "\n";
                    if (!(Result.Error == CommandError.UnmetPrecondition))
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
        private async Task Client_MessageReceivedSpecialModes(SocketMessage MessageParam)
        {
            if (!SpecialModes.ChristmasModeEnable && !SpecialModes.SpookModeEnable && !MessageParam.Author.IsBot)
                return;

            var Message = MessageParam as SocketUserMessage;
            var Context = new ShardedCommandContext(Client, Message);

            if (Context.Message == null || Context.Message.Content == "")
                return;
            if (Context.User.IsBot)
                return;

            await new SpecialModes().Spook(Context);
            await new SpecialModes().Christmas(Context);
        }
        private async Task Client_MessageReceivedHeart(SocketMessage arg)
        {
            var Message = arg as SocketUserMessage;
            if (Message.Author.IsBot)
                return;
            string msg = Message.Content.Replace("!", "");
            string mention = Client.CurrentUser.Mention.Replace("!", "");
            if (msg.Contains(mention) && (!msg.StartsWith(mention) || msg.Equals(mention)))
            {
                await Message.Channel.SendMessageAsync($"{Message.Author.Mention} <a:loveme:536705504798441483>");
            }
        }
        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            DateTime now = DateTime.Now;
            Server server = ServerDb.GetServer(arg.Id) ?? new Server
            {
                GuildId = arg.Id,
                JoinDate = now
            };
            server.LeaveDate = new DateTime(0);
            server.Prefix = StaticSettings.prefix;
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
            await ((ISocketMessageChannel)Client.GetChannel(StaticSettings.log_channel)).SendMessageAsync($":white_check_mark: I joined `{arg.Id}` **{arg.Name}**.\nOwner: `{arg.Owner.Id}` **{arg.Owner}**");
        }
        private async Task Client_LeftGuild(SocketGuild arg)
        {
            var server = ServerDb.GetServer(arg.Id);
            server.LeaveDate = DateTime.Now;
            await ServerDb.UpdateServer(server);

            await ((ISocketMessageChannel)Client.GetChannel(StaticSettings.log_channel)).SendMessageAsync($":x: I left `{arg.Id}` **{arg.Name}**.\nOwner: `{arg.Owner.Id}` **{arg.Owner}**");
        }

        // USER JOIN LOGS

        private async Task Client_UserBannedLog(SocketUser arg1, SocketGuild arg2)
        {
            await GetJoinLogChannel(arg2)?.SendMessageAsync($":hammer: {UserInfo(arg1)} was banned.");
        }
        private async Task Client_UserLeftLog(SocketGuildUser arg)
        {
            await GetJoinLogChannel(arg.Guild)?.SendMessageAsync($":x: {UserInfo(arg)} left the server.");
        }
        private async Task Client_UserJoinedLog(SocketGuildUser arg)
        {
            await GetJoinLogChannel(arg.Guild)?.SendMessageAsync($":white_check_mark: {UserInfo(arg)} joined the server.");
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

        private async Task Client_ShardReady(DiscordSocketClient arg)
        {
            var ch = Client.GetChannel(StaticSettings.log_channel) as ISocketMessageChannel;
            Console.WriteLine($"{DateTime.Now} - Shard {arg.ShardId} Ready");
            _ = ch.SendMessageAsync($"`{DateTime.Now} - Shard {arg.ShardId} Ready`");
            int res = 0;
            if (Launch)
            {
                Ready();
                res = await CheckLeftGuilds();
                if (res > 0)
                {
                    Console.WriteLine($"{DateTime.Now} - Left {res} Guilds.");
                    _ = ch.SendMessageAsync($"`{DateTime.Now} - Left {res} Guilds.`");
                }
            }
            
            res = await CheckJoinedGuilds(arg);
            if (res > 0)
            {
                Console.WriteLine($"{DateTime.Now} - Joined {res} Guilds.");
                _ = ch.SendMessageAsync($"`{DateTime.Now} - Joined {res} Guilds.`");
            }
        }
        private void Ready()
        {
            if (!Debug)
            {
                RedditAPI.Poke();
                ImgurAPI.Poke();
                WebUtil.SetUpDbl(Client.CurrentUser.Id);
            }
        }
        private static string ParseSettingsJson()
        {
            string JSON = "";
            string JSONLocation = Locations.SettingsJSON;
            using (var Stream = new FileStream(JSONLocation, FileMode.Open, FileAccess.Read))
            using (var ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            Settings Settings = JsonConvert.DeserializeObject<Settings>(JSON);
            StaticSettings.owner = Settings.Owner;
            StaticSettings.prefix = Settings.Prefix;
            StaticSettings.insider_role = Settings.home_server;
            StaticSettings.log_channel = Settings.log_channel;
            StaticSettings.version = Settings.Version;

            return Settings.Token;
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
            Debug = true;
            Locations.SetUpDebug();
            Timers.SetUp();
            SetUpPrefixes();
        }
        private static void SetUpRelease()
        {
            Console.WriteLine("Entry: " + Assembly.GetEntryAssembly().Location);
            Locations.SetUpRelease();
            Timers.SetUpRelease();
            SetUpPrefixes();
        }
        private static async Task<int> CheckJoinedGuilds(DiscordSocketClient shard = null)
        {
            IReadOnlyCollection<SocketGuild> guilds = null;
            if (shard == null)
                guilds = Client.Guilds;
            else
                guilds = shard.Guilds;

            var servers = ServerDb.GetNotLeft();
            HashSet<ulong> existingIds = new HashSet<ulong>(servers.Select(x => x.GuildId));
            var zerotime = new DateTime(0);

            int added = 0;
            foreach(var guild in guilds)
            {
                if(!existingIds.Contains(guild.Id))
                {
                    var server = new Server
                    {
                        GuildId = guild.Id,
                        JoinDate = System.DateTime.Now,
                        LeaveDate = zerotime,
                        Prefix = StaticSettings.prefix
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

            var servers = ServerDb.GetNotLeft();

            int left = 0;
            foreach (var srv in servers)
            {
                if (!existingIds.Contains(srv.GuildId))
                {
                    srv.LeaveDate = DateTime.Now;
                    await ServerDb.UpdateServer(srv);
                    left++;
                }
            }

            return left;
        }
        private static async Task<int[]> SetUpServers(DiscordSocketClient shard = null)
        {
            IReadOnlyCollection<SocketGuild> guilds = null;
            if (shard == null)
                guilds = Client.Guilds;
            else
                guilds = shard.Guilds;

            var servers = ServerDb.GetAll();

            int added = 0;
            foreach(var x in guilds)
            {
                if(!servers.Any(y => y.GuildId == x.Id))
                {
                    var server = new Server
                    {
                        GuildId = x.Id,
                        JoinDate = System.DateTime.Now,
                        Prefix = StaticSettings.prefix
                    };
                    await ServerDb.UpdateServer(server);
                    await ToastieDb.SetToasties(Client.CurrentUser.Id, 1000000, x.Id);
                    added++;
                }
                await Task.Delay(2);
            }

            int left = 0;
            foreach(var srv in servers.Where(x => x.LeaveDate == new DateTime(0)))
            {
                if(!guilds.Any(y => y.Id == srv.GuildId))
                {
                    srv.LeaveDate = DateTime.Now;
                    await ServerDb.UpdateServer(srv);
                    left++;
                }
                await Task.Delay(2);
            }
            Console.WriteLine($"Servers ready. Added {added}. Left {left}.");

            return new int[2] { added, left };
        }

        // PREFIXES

        public static string GetPrefix(ulong guildId)
        {
            return Prefixes.GetValueOrDefault(guildId) ?? StaticSettings.prefix;
        }
        public static string GetPrefix(SocketCommandContext context)
        {
            return context.Guild == null ? StaticSettings.prefix : GetPrefix(context.Guild.Id);
        }
        public static string GetPrefix(SocketGuildUser user)
        {
            return user.Guild == null ? StaticSettings.prefix : GetPrefix(user.Guild.Id);
        }
        public static string GetPrefix(SocketGuild guild)
        {
            return guild == null ? StaticSettings.prefix : GetPrefix(guild.Id);
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

        private void Test()
        {
            string assemblyFile = new System.Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            Console.WriteLine(assemblyFile);
        }
        private string WelcomeDm()
        {
            string sb = "Hai domo! Namiko here, the server's original mascot girl! \nWelcome to **AMFWT**! Please read the rules and visit the #info channel!\r\n" +
            "\r\n" +
            "1. **English**. Lots of us know loooooots of languages, but everyone knows English, so use that.\r\n" +
            "\r\n" +
            "2. **Keep topics relevant to the chat**. **Don't spam!** We know you need to show best girl to everyone once in a while, but try not to.\r\n" +
            "\r\n" +
            "3. **NSFW content in NSFW channels only. No gore whatsoever.** Including cursed images, anything violent, lewd, disgusting or disturbing. Remember, *The Mom* is watching from the shadows.\r\n" +
            "\r\n" +
            "4. NO. **YOU CANT LEWD THE LOLI**. NOT EVEN IN NSFW CHANNELS. ITS AGAINST DISCORD TOS. Patting the loli is okay and encouraged. BUT IF YOU LEWD I WILL DEAL WITH YOU PERSONALLY. **SHOTA INCLUDED**.\r\n" +
            "\r\n" +
            "5. **No harassment**. Especially the new guys, don't bulli them! Racism, bulli - we having none of that. Pats and Hugs only.\r\n" +
            "\r\n" +
            "6. **No spoilers**. We don't want our days ruined just because we haven't seen the latest episode. You can talk about them in the spoilers channels, for specific popular ongoing shows.\r\n" +
            "\r\n" +
            "7. **No advertising**. Includes streams, channels, other discord servers etc. However, you can show off your art skills in #oc-art \r\n" +
            "\r\n" +
            "8. **No random pings**. Including teams, or spam mentioning someone ... unless you have a very good reason, such as news of real genetically engineered neko-girls.\r\n" +
            "\r\n" +
            "9. **No impersonation**. Using someone else's name or profile picture. Bad. No. Don't want the cat to be an undercover dog. \r\n" +
            "\r\n" +
            "10. Have Fun. Uppercase F.\r\n";

            return sb;
        }
        public static string CommandHelpString(string commandName, string prefix)
        {
            try
            {
                var cmd = Commands.Commands.Where(x => x.Aliases.Any(y => y.Equals(commandName, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
                string St = cmd.Summary;

                int pFrom = St.IndexOf("**Usage**:");

                string result = St.Substring(pFrom);
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
    }
}
