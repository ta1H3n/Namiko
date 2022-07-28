using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Namiko.Modules.Basic;
using System;
using System.Linq;
using System.Threading.Tasks;
using PreconditionAttribute = Namiko.Handlers.Attributes.PreconditionAttribute;

namespace Namiko
{
    public class Basic : CustomModuleBase<ICustomContext>
    {
        [Command("Hi Namiko"), Alias("Hi", "ping", "Awoo"), Description("Hi Namiko command. Counts response time.")]
        [SlashCommand("ping", "Test command response time")]
        public async Task HiNamiko([Remainder] string str = "")
        {
            var msgTime = Context.CreatedAt;
            var msg = await ReplyAsync($"Hi {Context.User.Mention} :fox: `Counting...`");
            var msgTime2 = msg.CreatedAt;
            var ping = msgTime2 - msgTime;
            await msg.ModifyAsync(a => a.Content = $"Hi {Context.User.Mention} :fox: `{ping.TotalMilliseconds}ms`");
        }
        
        [Command("Info"), Alias("About"), Description("Bot info.")]
        [SlashCommand("info", "Info command")]
        public async Task Info([Remainder] string str = "")
        {
            await ReplyAsync("", false, BasicUtil.InfoEmbed().Build());
        }

        [Command("Vote")]
        [SlashCommand("vote", "Vote for Namiko and get lootboxes!")]
        public async Task Vote([Remainder] string str = "")
        {
            await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                .WithDescription($"Vote for Namiko on [Discord Bots]({LinkHelper.GetRedirectUrl(LinkHelper.Vote, "Vote", "cmd-vote")}) and receive a lootbox!")
                .Build());
        }

        [Command("Burn")]
        public async Task Burn([Remainder] string str = "")
        {
            await ReplyAsync(CurrencyUtil.GetFalseBegMessage());
        }

        [Command("JoinMessageTest"), OwnerPrecondition]
        public async Task JoinMessageTest([Remainder] string str = "")
        {
            await ReplyAsync("Hi! Please take good care of me!", false, BasicUtil.GuildJoinEmbed("!").Build());
        }

        [Command("PermTest"), BotPermission(GuildPermission.Administrator)]
        public async Task PermTest()
        {
            await ReplyAsync("???");
        }

        [Command("GuildList"), OwnerPrecondition]
        public async Task GuildTest()
        {
            var msg = new PaginatedMessage<SocketGuild>(Program.GetClient().Guilds, 20, (x) => $"`{x.Id}` - **{x.Name}**\n`{x.OwnerId}` - **{x.Owner}**\n");
            await PagedReplyAsync(msg);
        }

        [Command("MarkdownCommands"), OwnerPrecondition]
        public async Task MarkdownCommands()
        {
            using var stream = Timers.GenerateStreamFromString(MarkdownCommandList(Program.GetCommands()));
            await Context.Channel.SendFileAsync(stream, "CommandsMarkdown.txt");
        }

        [Command("Wait"), OwnerPrecondition]
        public async Task Wait(int sec)
        {
            await Task.Delay(sec * 1000);
            await ReplyAsync("Done.");
        }

        [Command("CleanData"), OwnerPrecondition]
        public async Task CleanData()
        {
            Timers.Timer_CleanData(null, null);
            await ReplyAsync("Done.");
        }

        [Command("StealToasties"), OwnerPrecondition]
        public async Task StealToasties()
        {
            Timers.Timer_NamikoSteal(null, null);
            await ReplyAsync("Done.");
        }

        [Command("SShipWaifu"), Description("\n **Usage**: `!shipwaifu [waifu] [userid] [guildid_optional]`"), OwnerPrecondition]
        public async Task ShipWaifu(string name, ulong userId, ulong guildId = 0)
        {
            Program.GetPrefix(Context);

            if (guildId == 0)
                guildId = Context.Guild.Id;

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name), this);
            if (waifu == null)
                return;

            if (UserInventoryDb.OwnsWaifu(userId, waifu, guildId))
            {
                await ReplyAsync($"They already own **{waifu.Name}**");
                return;
            }

            await UserInventoryDb.AddWaifu(userId, waifu, guildId);
            await ReplyAsync($"**{waifu.Name}** shipped!");
        }

        //  [Command("Test"), OwnerPrecondition]
        //  public async Task Test()
        //  {
        //      Timers.Timer_NamikoSteal(null, null);
        //      await ReplyAsync($"It has been done.");
        //  }

        [Command("Blacklist"), OwnerPrecondition]
        public async Task Blacklist(ulong id)
        {
            if (BlacklistDb.IsBlacklisted(id))
            {
                await BlacklistDb.Remove(id);
                await ReplyAsync($"Unblacklisted.");
                await WebhookClients.NamikoLogChannel.SendMessageAsync($"Unblacklisted {id}");
                Program.Blacklist.Remove(id);
                return;
            }
            else
            {
                await BlacklistDb.Add(id);
                await ReplyAsync($"Blacklisted.");
                Program.Blacklist.Add(id);
                var client = Program.GetClient();
                var guild = client.GetGuild(id);
                string what = $"Blacklisted {id}";
                if (guild != null)
                {
                    var ch = await guild.Owner.CreateDMChannelAsync();
                    await ch.SendMessageAsync($"Your guild ({guild.Name} - {id}) has been blacklisted.\n" +
                        $"Please contact taiHen#2839 in {LinkHelper.SupportServerInvite} for more information or if you think this is a mistake.");
                    what = $"Guild ({guild.Name} {id}) Blacklisted.";
                } else
                {
                    var user = client.GetUser(id);
                    if (user != null)
                    {
                        var ch = await user.CreateDMChannelAsync();
                        await ch.SendMessageAsync($"You ({user.Username} - {id}) have been blacklisted.\n" +
                            $"Please contact taiHen#2839 in {LinkHelper.SupportServerInvite} for more information or if you think this is a mistake.");
                        what = $"User ({user.Username} {id}) Blacklisted.";
                    }
                }
                await WebhookClients.NamikoLogChannel.SendMessageAsync(what);
            }
        }

        [Command("Status"), Description("Bot status.\n **Usage**: `!status`")]
        [SlashCommand("status", "Namiko <-> Discord connection status")]
        public async Task Status()
        {
            var client = Context.Client as DiscordShardedClient;

            var shards = client.Shards;
            var eb = new EmbedBuilderPrepared();

            int homeShard = -1;
            if (Context.Guild != null)
            {
                homeShard = (int)(Context.Guild.Id << 22) % client.Shards.Count;
            }

            foreach(var shard in shards)
            {
                eb.AddField($"Shard {shard.ShardId}{(shard.ShardId == homeShard ? " - your shard" : "")}",
                    $"State: {shard.ConnectionState}\n" +
                    $"Latency: {shard.Latency}\n",
                    true);
            }

            await ReplyAsync(embed: eb.Build());
        }

        // HELP COMMAND STUFF
        [Command("Help"), Alias("h"), Description("Shows more information about a command.\n**Usage**: `!help [command/module_name]`")]
        public async Task Help([Remainder] string cmd = "")
        {
            var commandService = Program.GetCommands();
            string prefix = Program.GetPrefix(Context);

            EmbedBuilder eb = null;
            string desc = "";

            if (cmd != null && cmd != "")
            {

                desc = CommandHelpString(commandService.Commands.Where(x => x.Aliases.Any(y => y.Equals(cmd, StringComparison.OrdinalIgnoreCase))).FirstOrDefault(), prefix);

                if (desc == "")
                    eb = ModuleHelpEmbed(commandService.Modules.Where(x => x.Name.Equals(cmd, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());
            }

            else
                eb = AllHelpEmbed(commandService, Context.Guild == null ? false : ((SocketGuildUser)Context.User).Roles.Any(x => x.Id == AppSettings.InsiderRoleId));

            if(!desc.Equals(""))
            {
                await ReplyAsync(desc);
                return;
            }

            if (eb != null)
            {
                await ReplyAsync(embed: eb.Build());
                string msg = $"Check out this simple guide detailing my main features: <{LinkHelper.Guide}>\n" +
                    $"Find the command list online: <{LinkHelper.Commands}>\n" +
                    $"Type `{prefix}info` to learn more about me and find useful links!\n" +
                    $"Type `{prefix}images` for a list of my reaction image commands!";
                await ReplyAsync(msg);
            }
        }

        private EmbedBuilder AllHelpEmbed(CommandService commandService, bool all = false)
        {
            var eb = new EmbedBuilder();
            //eb.WithTitle("Commands");

            foreach(var x in commandService.Modules)
            {
                if ((x.Name != "Special" && x.Name != "Basic" && x.Name != "SpecialModes" && x.Name != "WaifuEditing") || all)
                {
                    var fb = new EmbedFieldBuilder{
                        Name = x.Name
                    };
                    //fb.Name = x.Name;

                    string commandList = "";
                    foreach (var y in x.Commands)
                    {
                        bool prec = y.Preconditions.Any(z => (z.GetType() == typeof(InsiderAttribute)) || (z.GetType() == typeof(OwnerPrecondition)));
                        if (!prec || all)
                        {
                            if (!commandList.Contains(y.Name))
                                commandList += $"`{y.Name}` ";
                        }
                    }

                    fb.Value = commandList;
                    eb.AddField(fb);
                }
            }
            
            eb.WithFooter(@"""What are you? Twelve?"" -Namiko");
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithDescription($"Check out Namiko's usage guide [here]({LinkHelper.Guide}) :star:\n" +
                $"Open in [browser]({LinkHelper.Commands}) :star:");
            eb.WithTitle("Commands");
            return eb;
        }
        private EmbedBuilder ModuleHelpEmbed(Discord.Commands.ModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
                return null;

            var eb = new EmbedBuilder();
            eb.WithTitle(moduleInfo.Name);

            string desc = "";
            foreach (var x in moduleInfo.Commands)
            {
                try
                {
                    desc += $"  **{x.Name}**\n{x.Summary}\n";
                } catch { }
            }

            eb.WithColor(BasicUtil.RandomColor());
            eb.WithDescription(desc);
            return eb;
        }
        public EmbedBuilder CommandHelpEmbed(CommandInfo commandInfo)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(commandInfo.Name);
            string desc = "";

            //desc += $"**Name**: {commandInfo.Name}\n";
            desc += $"**Aliases**: ";
            foreach(var x in commandInfo.Aliases)
                desc += $"`{x}` ";
            desc += "\n";
            desc += $"**Description**: {commandInfo.Summary}\n";
            desc += $"**Permissions**: ";
            foreach (var x in commandInfo.Preconditions)
                desc += $"{x} ";

            eb.WithColor(BasicUtil.RandomColor());
            eb.WithDescription(desc);
            return eb;
        }

        public string CommandHelpString(CommandInfo commandInfo, string prefix)
        {
            if (commandInfo == null)
                return "";

            string desc = "";
            desc += $":star: **{commandInfo.Name.ToUpper()}**\n";
            
            desc += $"**Description**: {commandInfo.Summary.Replace("!", prefix)}\n\n";
            desc += $"**Aliases**: ";
            foreach (var x in commandInfo.Aliases)
                desc += $"`{x}` ";
            desc += "\n";
            if(commandInfo.Preconditions.Count > 0)
                desc += $"**Conditions**: ";
            foreach (var x in commandInfo.Attributes)
            {
                if (x is UserPermissionAttribute)
                { 
                    var prec = x as UserPermissionAttribute;
                    desc += $"`User:{prec.Name}` ";
                }
                else if (x is BotPermissionAttribute)
                {
                    var prec = x as BotPermissionAttribute;
                    desc += $"`Bot:{prec.Name}` ";
                }
                else if (x is PreconditionAttribute)
                {
                    var prec = x as PreconditionAttribute;
                    desc += $"`{prec.Name}` ";
                }
            }

            return desc;
        }

        public string MarkdownCommandList(CommandService commandService)
        {
            string text = "";
            
            foreach (var x in commandService.Modules)
            {
                text += $"##{x.Name}\n";
                string commandList = "";
                foreach (var y in x.Commands)
                {
                    bool insider = y.Preconditions.Any(z => (z.GetType() == typeof(InsiderAttribute)) || (z.GetType() == typeof(OwnerPrecondition)));
                    if (!commandList.Contains(y.Name))
                        commandList += $"* **{y.Name}** - {y.Summary}.{(insider?" **Insider only.**":"")}\n";
                }

                text += commandList + "\n";
            }

            return text;
        }
    }
}
