using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Resources.Datatypes;
using Namiko.Core.Modules;
using Newtonsoft.Json;
using Namiko.Core.Util;
using Namiko.Data;
using System.Timers;
using Namiko.Core;
using Namiko.Resources.Database;

namespace Namiko
{
    public class Program
    {
        private static DiscordSocketClient Client;
        private static CommandService Commands;
        private static bool Pause = false;
        private static bool Debug = false;

        static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();
        private async Task MainAsync()
        {
            SetUpDebug();
            //SetUpRelease();
            Timers.SetUp();

            Client = new DiscordSocketClient();
            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
            });
            
            Client.Ready += Client_Ready;
            Client.Log += Client_Log;
            Client.ReactionAdded += Client_ReactionAdded;
            Client.JoinedGuild += Client_JoinedGuild;
            Client.LeftGuild += Client_LeftGuild;
            Client.MessageReceived += Client_MessageReceived;
            Client.MessageReceived += Client_MessageReceivedSpecialModes;
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            // Join/leave logging.
            Client.UserJoined += Client_UserJoinedLog;
            Client.UserLeft += Client_UserLeftLog;
            Client.UserBanned += Client_UserBannedLog;

            await Client.LoginAsync(TokenType.Bot, ParseSettingsJson());
            await Client.StartAsync();
            
            await Task.Delay(-1);
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
            
            var chid = WelcomeMessageDb.GetWelcomeChannel(sch.Guild.Id);
            var ch = sch.Guild.GetTextChannel(chid);
            await ch.SendMessageAsync(WelcomeUtil.GetWelcomeMessageString(user));
        }
        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            var ch = await arg1.GetOrCreateDMChannelAsync();
            await ch.SendMessageAsync(WelcomeDm());
        }
        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            // var welcomes = new Welcomes();
            // await welcomes.SendWelcome(arg, Client);

            var chid = WelcomeMessageDb.GetWelcomeChannel(arg.Guild.Id);
            var ch = arg.Guild.GetTextChannel(chid);
            await ch.SendMessageAsync(WelcomeUtil.GetWelcomeMessageString(arg));

            var dmch = await arg.GetOrCreateDMChannelAsync();
            await dmch.SendMessageAsync(WelcomeDm());
        }
        private async Task Client_Ready()
        {
            var ch = Client.GetChannel(StaticSettings.log_channel) as ISocketMessageChannel;
            await ch.SendMessageAsync($"`{DateTime.Now} - Ready`");
        }
        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task Client_Log(LogMessage arg)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string message = $"`{DateTime.Now} at {arg.Source}] {arg.Message}`";
            Console.WriteLine(message);
           // var channel = Client.GetGuild(StaticSettings.home_server).GetChannel(StaticSettings.log_channel) as ISocketMessageChannel;
           // await channel.SendMessageAsync(message);
        }
        private async Task Client_MessageReceived(SocketMessage MessageParam)
        {
            if (MessageParam.Channel.Id == 420319722723082275)
                return;

            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(Client, Message);

            if (Context.Message == null || Context.Message.Content == "")
                return;
            if (Context.User.IsBot)
                return;
            if (BlacklistedChannelDb.IsBlacklisted(Context.Channel.Id))
                return;

            int ArgPos = 0;
            if (!(Message.HasStringPrefix(StaticSettings.prefix, ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos)) && !Pause)
            {
                await Blackjack.BlackjackInput(Context);
                return;
            }
            var cmds = Commands.Search(Context, ArgPos);
            if (cmds.IsSuccess && Pause && Context.User.Id != StaticSettings.owner)
            {
                await Context.Channel.SendMessageAsync("Commands disabled temporarily. Try again later.");
                return;
            }
            
            var Result = await Commands.ExecuteAsync(Context, ArgPos, null);

            if(!Result.IsSuccess)
            {
                await new Basic().Help(Context, Commands);
                await new Images().SendRandomImage(Context);
                if (!Result.ErrorReason.Equals("Unknown command."))
                {
                    Console.WriteLine($"{DateTime.Now} at Commands] Text: {Context.Message.Content} | Error: {Result.ErrorReason}");
                    //await Context.Channel.SendMessageAsync("", false, CommandHelpEmbed(MessageParam.Content.Split(null)[0].Substring(1)).Build());
                    await Context.Channel.SendMessageAsync(CommandHelpString(MessageParam.Content.Split(null)[0].Substring(1)));
                }
                return;
            }

            Timers.CommandCallTickIncrement();
        }
        private async Task Client_MessageReceivedSpecialModes(SocketMessage MessageParam)
        {
            if (!SpecialModes.ChristmasModeEnable && !SpecialModes.SpookModeEnable)
                return;

            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(Client, Message);

            if (Context.Message == null || Context.Message.Content == "")
                return;
            if (Context.User.IsBot)
                return;

            await new SpecialModes().Spook(Context);
            await new SpecialModes().Christmas(Context);
        }
        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            Resources.Datatypes.Server server = new Resources.Datatypes.Server
            {
                GuildId = arg.Id,
                JoinDate = System.DateTime.Now,
                Prefix = StaticSettings.prefix
            };
            await ServerDb.UpdateServer(server);

            SocketTextChannel ch = arg.SystemChannel ?? arg.DefaultChannel;
            await ch?.SendMessageAsync("Helloooo! Take good care of me! Try `!info` for a quick guide, or `!help` for a list of my commands!");
            await ((ISocketMessageChannel)Client.GetChannel(StaticSettings.log_channel)).SendMessageAsync($":white_check_mark: I joined `{arg.Id}` {arg.Name}.\nOwner: `{arg.Owner.Id}` {arg.Owner.Username}");
        }
        private async Task Client_LeftGuild(SocketGuild arg)
        {
            var server = ServerDb.GetServer(arg.Id);
            server.LeaveDate = DateTime.Now;
            await ServerDb.UpdateServer(server);

            await ((ISocketMessageChannel)Client.GetChannel(StaticSettings.log_channel)).SendMessageAsync($":x: I left `{arg.Id}` {arg.Name}.\nOwner: `{arg.Owner.Id}` {arg.Owner.Username}");
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
            return $"`{user.Id}` {user.Username} {user.Mention}";
        }
        private static SocketTextChannel GetJoinLogChannel(SocketGuild guild)
        {
            return (SocketTextChannel) guild.GetChannel(ServerDb.GetServer(guild.Id).JoinLogChannelId);
        }

        // SET-UP / TECHNICAL

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
            StaticSettings.home_server = Settings.home_server;
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
            Locations.SetUpDebug();
            Debug = true;
        }
        private static void SetUpRelease()
        {
            Locations.SetUpRelease();
            Console.WriteLine(Locations.SettingsJSON);
            Console.WriteLine(Locations.SpookyLinesXml);
            Console.WriteLine(Locations.SqliteDb);
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
        public static string CommandHelpString(string commandName)
        {
            var cmd = Commands.Commands.Where(x => x.Aliases.Any(y => y.Equals(commandName, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
            return new Basic().CommandHelpString(cmd);
        }

        //  public static EmbedBuilder CommandHelpEmbed(string commandName)
        //  {
        //      var cmd = Commands.Commands.Where(x => x.Aliases.Any(y => y.Equals(commandName))).FirstOrDefault();
        //      return new BasicCommands().CommandHelpEmbed(cmd);
        //  }

        public static DiscordSocketClient GetClient()
        {
            return Client;
        }
    }
}
