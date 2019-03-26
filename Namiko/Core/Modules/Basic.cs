using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;
using Namiko;
using Namiko.Resources.Preconditions;
using Namiko.Core.Util;
using Discord.WebSocket;
using Discord.Rest;
using Discord.Addons.Interactive;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Namiko.Core.Modules
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

        [Command("PermTest"), CustomBotPermission(GuildPermission.Administrator)]
        public async Task PermTest()
        {
            await Context.Channel.SendMessageAsync("???");
        }

        [Command("GuildList"), OwnerPrecondition]
        public async Task GuildTest()
        {
            var msg = new CustomPaginatedMessage();
            msg.Pages = CustomPaginatedMessage.PagesArray(Program.GetClient().Guilds, 20, (x) => $"`{x.Id}` - **{x.Name}**\n`{x.OwnerId}` - **{x.Owner}**\n");
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

        [Command("Test"), OwnerPrecondition]
        public async Task Test()
        {
            Timers.Timer_NamikoSteal(null, null);
            await Context.Channel.SendMessageAsync($"It has been done.");
        }

        // HELP COMMAND STUFF

        public async Task<bool> Help(SocketCommandContext context, CommandService commandService)
        {
            string prefix = Program.GetPrefix(context);
            var msgStr = context.Message.Content;
            string[] words = msgStr.Split(null);
            string cmd = null;
            if (words[0] != $"{prefix}help" && words[0] != $"{prefix}h")
                return false;
            try
            {
                if (words[1] != null)
                    cmd = words[1];
            #pragma warning disable CS0168 // Variable is declared but never used
            } catch(IndexOutOfRangeException ex) { }

            EmbedBuilder eb = null;
            string desc = "";

            if (cmd != null)
            {
                try
                {
                    //eb = CommandHelpEmbed(commandService.Commands.Where(x => x.Aliases.Equals(cmd, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
                    desc = CommandHelpString(commandService.Commands.Where(x => x.Aliases.Any(y => y.Equals(cmd, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault(), prefix);
                } catch { };
                try
                {
                    eb = ModuleHelpEmbed(commandService.Modules.Where(x => x.Name.Equals(cmd, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
                } catch { };
            }
            else
                eb = AllHelpEmbed(commandService, context.Guild == null ? false : ((SocketGuildUser)context.User).Roles.Any(x => x.Id == StaticSettings.insider_role));

            if(!desc.Equals(""))
            {
                await context.Channel.SendMessageAsync(desc);
                return true;
            }
            await context.Channel.SendMessageAsync($":star: Type `{prefix}h [command_name]` for more information about a command. `{prefix}info` to learn more about me!", false, eb.Build());
            return true;
        }

        private EmbedBuilder AllHelpEmbed(CommandService commandService, bool all = false)
        {
            var eb = new EmbedBuilder();
            //eb.WithTitle("Commands");

            foreach(var x in commandService.Modules)
            {
                if ((x.Name != "Special" && x.Name != "Basic" && x.Name != "SpecialModes") || all)
                {
                    var fb = new EmbedFieldBuilder{
                        Name = x.Name
                    };
                    //fb.Name = x.Name;

                    string commandList = "";
                    foreach (var y in x.Commands)
                        commandList += $"`{y.Name}` ";

                    fb.Value = commandList;
                    eb.AddField(fb);
                }
            }
            
            eb.WithFooter(@"""What are you? Twelve?"" -Namiko");
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithDescription($"Find Namiko's user guide [here](https://github.com/ta1H3n/Namiko/wiki)");
            return eb;
        }
        private EmbedBuilder ModuleHelpEmbed(ModuleInfo moduleInfo)
        {
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
                try
                {
                    var prec = x as RequireUserPermissionAttribute;
                    desc += $"{prec.ChannelPermission} ";
                    desc += $"{prec.GuildPermission} ";
                } catch { }

                try
                {
                    var prec = x as CustomPrecondition;
                    desc += $"{prec.GetName()} ";
                } catch { }
            }

            return desc;
        }
    }

    public class DistintUserComparer : IEqualityComparer<IUser>
    {
        public bool Equals(IUser x, IUser y)
        {
            return x.Id.Equals(y.Id);
        }
        public int GetHashCode(IUser obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
