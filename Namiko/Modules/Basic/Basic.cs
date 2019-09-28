﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;



using Discord.WebSocket;
using Discord.Rest;
using Discord.Addons.Interactive;

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

        [Command("Donate"), Alias("Premium", "Support", "Patreon", "Paypal"), Summary("Donation Links.")]
        public async Task Donate([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync("", false, BasicUtil.DonateEmbed().Build());
        }

        [Command("Vote")]
        public async Task Vote([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                .WithDescription("Vote for Namiko on [Discord Bots](https://discordbots.org/bot/418823684459855882/vote) and receive a lootbox!")
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

        [Command("HourlyTest"), OwnerPrecondition]
        public async Task HourlyTest()
        {
            Timers.Timer_HourlyStats(null, null);
        }

        [Command("DailyTest"), OwnerPrecondition]
        public async Task DailyTest()
        {
            Timers.Timer_DailyStats(null, null);
        }

        [Command("MarkdownCommands"), OwnerPrecondition]
        public async Task MarkdownCommands()
        {
            using (var stream = Timers.GenerateStreamFromString(MarkdownCommandList(Program.GetCommands())))
            {
                await Context.Channel.SendFileAsync(stream, "CommandsMarkdown.txt");
            }
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
            if (guildId == 0)
                guildId = Context.Guild.Id;

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), this);
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
                    var ch = await guild.Owner.GetOrCreateDMChannelAsync();
                    await ch.SendMessageAsync($"Your guild ({guild.Name} - {id}) has been blacklisted.\n" +
                        $"Please contact taiHen#2839 in https://discord.gg/W6Ru5sM for more information or if you think this is a mistake.");
                    what = $"Guild ({guild.Name} {id}) Blacklisted.";
                } else
                {
                    var user = client.GetUser(id);
                    if (user != null)
                    {
                        var ch = await user.GetOrCreateDMChannelAsync();
                        await ch.SendMessageAsync($"You ({user.Username} - {id}) have been blacklisted.\n" +
                            $"Please contact taiHen#2839 in https://discord.gg/W6Ru5sM for more information or if you think this is a mistake.");
                        what = $"User ({user.Username} {id}) Blacklisted.";
                    }
                }
                await WebhookClients.NamikoLogChannel.SendMessageAsync(what);
            }
        }

        // HELP COMMAND STUFF

        [Command("Help"), Summary("Shows more information about a command.\n**Usage**: `!help [command/module_name]`")]
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
                eb = AllHelpEmbed(commandService, Context.Guild == null ? false : ((SocketGuildUser)Context.User).Roles.Any(x => x.Id == Config.InsiderRoleId));

            if(!desc.Equals(""))
            {
                await Context.Channel.SendMessageAsync(desc);
                return;
            }

            if(eb != null) 
                await Context.Channel.SendMessageAsync($":star: Type `{prefix}h [command_name]` for more information about a command. `{prefix}info` to learn more about me!", false, eb.Build());
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
                        bool prec = y.Preconditions.Any(z => (z.GetType() == typeof(HomePrecondition)) || (z.GetType() == typeof(OwnerPrecondition)));
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
            eb.WithDescription($"Check out Namiko's usage guide [here](https://github.com/ta1H3n/Namiko/wiki) :star:");
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
                if (x.GetType() == typeof(RequireUserPermissionAttribute))
                { 
                    var prec = x as RequireUserPermissionAttribute;
                    desc += $"{prec.ChannelPermission} ";
                    desc += $"{prec.GuildPermission} ";
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
                    bool insider = y.Preconditions.Any(z => (z.GetType() == typeof(HomePrecondition)) || (z.GetType() == typeof(OwnerPrecondition)));
                    if (!commandList.Contains(y.Name))
                        commandList += $"* **{y.Name}** - {y.Summary}.{(insider?" **Insider only.**":"")}\n";
                }

                text += commandList + "\n";
            }

            return text;
        }
    }
}
