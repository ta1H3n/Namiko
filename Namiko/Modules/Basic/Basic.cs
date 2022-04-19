using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Namiko.Modules.Basic;
using System;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Namiko
{
    public class Basic : InteractiveBase<ShardedCommandContext>
    {
        [Command("Hi Namiko"), Alias("Hi", "ping", "Awoo"), Summary("Hi Namiko command. Counts response time.")]
        public async Task HiNamiko([Remainder] string str = "")
        {
            var msgTime = Context.Message.CreatedAt;
            var msg = await Context.Channel.SendMessageAsync($"Hi {Context.User.Mention} :fox: `Counting...`");
            var msgTime2 = msg.CreatedAt;
            var ping = msgTime2 - msgTime;
            await msg.ModifyAsync(a => a.Content = $"Hi {Context.User.Mention} :fox: `{ping.TotalMilliseconds}ms`");
        }
        
        [Command("Info"), Alias("About"), Summary("Bot info.")]
        public async Task Info([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync("", false, BasicUtil.InfoEmbed().Build());
        }

        [Command("Pro"), Alias("Premium", "Support", "Patreon", "Paypal", "Donate"), Summary("Donation Links.")]
        public async Task Donate([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync("", false, BasicUtil.DonateEmbed(Program.GetPrefix(Context)).Build());
        }

        [Command("InfoPro"), Summary("Donation Links.")]
        public async Task InfoPro([Remainder] string str = "")
        {
            string prefix = Program.GetPrefix(Context);
            var eb = new EmbedBuilder()
                .WithAuthor(Program.GetClient().CurrentUser)
                .WithDescription($"Namiko Pro Upgrades :star:\n" +
                    $"\n" +
                    $"- Waifus from your wishlist have a much higher chance to appear in the `{prefix}waifushop`\n" +
                    $"- Much higher chance to find waifus from your wishlist in your lootboxes\n" +
                    $"- Get upgraded lootboxes for voting: chance to get **T1** and **T2** waifus, more toasties.\n" +
                    $"- Marry up to **3** users\n" +
                    $"\n" +
                    $":star: Subscribe on [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info-pro")})")
                .WithImageUrl("https://i.imgur.com/NIAP5QC.png")
                .WithFooter("-What are you? Twelve?")
                .WithColor(BasicUtil.RandomColor());

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("InfoProPlus"), Summary("Donation Links.")]
        public async Task InfoProPlus([Remainder] string str = "")
        {
            string prefix = Program.GetPrefix(Context);
            var eb = new EmbedBuilder()
                .WithAuthor(Program.GetClient().CurrentUser)
                .WithDescription($"Namiko Pro+ Upgrades :star2:\n" +
                    $"\n" +
                    $"- All upgrades in Pro\n" +
                    $"- **Halved** `{prefix}daily` cooldown in all servers\n" +
                    $"- Get a **T1 Waifu** Lootbox when using `{prefix}weekly`\n" +
                    $"- Request a **custom waifu** that is only obtainable by you (Stays after premium ends)\n" +
                    $"- Waifu **wishlist** limit increased to **12**\n" +
                    $"- Marry up to **10** users\n" +
                    $"- Behind the scenes access in *Namiko Test Realm*\n" +
                    $"\n" +
                    $":star: Subscribe on [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info-proplus")})")
                .WithImageUrl("https://i.imgur.com/uq9eip4.png")
                .WithFooter("-What are you? Twelve?")
                .WithColor(BasicUtil.RandomColor());

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("InfoGuild"), Summary("Donation Links.")]
        public async Task InfoGuild([Remainder] string str = "")
        {
            string prefix = Program.GetPrefix(Context);
            var eb = new EmbedBuilder()
                .WithAuthor(Program.GetClient().CurrentUser)
                .WithDescription($"Namiko Pro Guild Upgrades :star:\n" +
                    $"\n" +
                    $"- **Music** with a lot of features! Try `{prefix}join`\n" +
                    $"- Halved `{prefix}weekly` cooldown\n" +
                    $"- **3** times more waifus in waifu shop and gacha shop\n" +
                    $"- Subreddit follow limit increased to **5**\n" +
                    $"\n" +
                    $":star: Subscribe on [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info-guild")})")
                .WithImageUrl("https://i.imgur.com/XxFJ1gH.png")
                .WithFooter("-What are you? Twelve?")
                .WithColor(BasicUtil.RandomColor());

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("InfoGuildPlus"), Summary("Donation Links.")]
        public async Task InfoGuildPlus([Remainder] string str = "")
        {
            var eb = new EmbedBuilder()
                .WithAuthor(Program.GetClient().CurrentUser)
                .WithDescription($"Namiko Pro Guild+ Upgrades :star2:\n" +
                    $"\n" +
                    $"- All upgrades in Guild\n" +
                    $"- Music queue limit increased from **100** to **500**\n" +
                    $"- A new waifu shop that can be controlled by your mods\n" +
                    $"- Waifu **lootboxes** will be up for purchase for users\n" +
                    $"- Create **reaction image commands** exclusive to your server (Stays after premium ends)\n" +
                    $"- Weekly increased by **1000** for everyone\n" +
                    $"- Subreddit follow limit increased to **10**\n" +
                    $"\n" +
                    $":star: Subscribe on [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info-guildplus")})")
                .WithImageUrl("https://i.imgur.com/f8XYQxU.png")
                .WithFooter("-What are you? Twelve?")
                .WithColor(BasicUtil.RandomColor());

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("Vote")]
        public async Task Vote([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                .WithDescription($"Vote for Namiko on [Discord Bots]({LinkHelper.GetRedirectUrl(LinkHelper.Vote, "Vote", "cmd-vote")}) and receive a lootbox!")
                .Build());
        }

        [Command("Burn")]
        public async Task Burn([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync(ToastieUtil.GetFalseBegMessage());
        }

        [Command("JoinMessageTest"), OwnerPrecondition]
        public async Task JoinMessageTest([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync("Hi! Please take good care of me!", false, BasicUtil.GuildJoinEmbed("!").Build());
        }

        [Command("PermTest"), CustomBotPermission(GuildPermission.Administrator)]
        public async Task PermTest()
        {
            await Context.Channel.SendMessageAsync("???");
        }

        [Command("GuildList"), OwnerPrecondition]
        public async Task GuildTest()
        {
            var msg = new CustomPaginatedMessage
            {
                Pages = CustomPaginatedMessage.PagesArray(Program.GetClient().Guilds, 20, (x) => $"`{x.Id}` - **{x.Name}**\n`{x.OwnerId}` - **{x.Owner}**\n")
            };
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
            await Context.Channel.SendMessageAsync("Done.");
        }

        [Command("CleanData"), OwnerPrecondition]
        public async Task CleanData()
        {
            Timers.Timer_CleanData(null, null);
            await Context.Channel.SendMessageAsync("Done.");
        }

        [Command("StealToasties"), OwnerPrecondition]
        public async Task StealToasties()
        {
            Timers.Timer_NamikoSteal(null, null);
            await Context.Channel.SendMessageAsync("Done.");
        }

        [Command("SShipWaifu"), Summary("\n **Usage**: `!shipwaifu [waifu] [userid] [guildid_optional]`"), OwnerPrecondition]
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
                await Context.Channel.SendMessageAsync($"They already own **{waifu.Name}**");
                return;
            }

            await UserInventoryDb.AddWaifu(userId, waifu, guildId);
            await Context.Channel.SendMessageAsync($"**{waifu.Name}** shipped!");
        }

        //  [Command("Test"), OwnerPrecondition]
        //  public async Task Test()
        //  {
        //      Timers.Timer_NamikoSteal(null, null);
        //      await Context.Channel.SendMessageAsync($"It has been done.");
        //  }

        [Command("Blacklist"), OwnerPrecondition]
        public async Task Blacklist(ulong id)
        {
            if (BlacklistDb.IsBlacklisted(id))
            {
                await BlacklistDb.Remove(id);
                await Context.Channel.SendMessageAsync($"Unblacklisted.");
                await WebhookClients.NamikoLogChannel.SendMessageAsync($"Unblacklisted {id}");
                Program.Blacklist.Remove(id);
                return;
            }
            else
            {
                await BlacklistDb.Add(id);
                await Context.Channel.SendMessageAsync($"Blacklisted.");
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

        [Command("Status"), Summary("Bot status.\n **Usage**: `!status`")]
        public async Task Status()
        {
            var shards = Context.Client.Shards;
            var eb = new EmbedBuilderPrepared();

            int homeShard = -1;
            if (Context.Guild != null)
            {
                homeShard = (int)(Context.Guild.Id << 22) % Context.Client.Shards.Count;
            }

            foreach(var shard in shards)
            {
                eb.AddField($"Shard {shard.ShardId}{(shard.ShardId == homeShard ? " - your shard" : "")}",
                    $"State: {shard.ConnectionState}\n" +
                    $"Latency: {shard.Latency}\n",
                    true);
            }

            await Context.Channel.SendMessageAsync(embed: eb.Build());
        }

        // HELP COMMAND STUFF

        [Command("Help"), Alias("h"), Summary("Shows more information about a command.\n**Usage**: `!help [command/module_name]`")]
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
                await Context.Channel.SendMessageAsync(desc);
                return;
            }

            if (eb != null)
            {
                await Context.Channel.SendMessageAsync(embed: eb.Build());
                string msg = $"Check out this simple guide detailing my main features: <{LinkHelper.Guide}>\n" +
                    $"Find the command list online: <{LinkHelper.Commands}>\n" +
                    $"Type `{prefix}info` to learn more about me and find useful links!\n" +
                    $"Type `{prefix}images` for a list of my reaction image commands!";
                await Context.Channel.SendMessageAsync(msg);
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
        private EmbedBuilder ModuleHelpEmbed(ModuleInfo moduleInfo)
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
                desc += $"**Permissions**: ";
            foreach (var x in commandInfo.Preconditions)
            {
                if (x is RequireUserPermissionAttribute)
                { 
                    var prec = x as RequireUserPermissionAttribute;
                    desc += prec.ChannelPermission != null ? $"{prec.ChannelPermission} " : "";
                    desc += prec.GuildPermission != null ? $"{prec.GuildPermission} " : "";
                }

                else if (x is CustomPrecondition)
                {
                    var prec = x as CustomPrecondition;
                    desc += $"{prec.GetName()} ";
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
