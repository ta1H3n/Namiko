using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;
using Namiko.Resources.Attributes;
using Discord.WebSocket;
using Discord.Rest;

namespace Namiko.Core.Basic
{
    public class Basic : ModuleBase<SocketCommandContext>
    {
        [Command("Hi Namiko"), Alias("Hi", "ping"), Summary("Hi Namiko command")]
        public async Task HiNamiko([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync($"Hi {Context.User.Mention} :fox:");
        }

        [Command("RandomUser"), Alias("ru"), Summary("Randomly picks one user from all the reactions on a message.\nOnly works with default discord emotes.\n" +
            "Message has to be in the same channel.\n**Usage**:`!ru [message_id]`")]
        public async Task RandomUser(ulong msgId, [Remainder] string str = "")
        {
            var msg = await Context.Channel.GetMessageAsync(msgId) as RestUserMessage;
            List<IUser> users = new List<IUser>();
            foreach (var reaction in msg.Reactions)
            {
                try
                {
                    var tUsers = await msg.GetReactionUsersAsync(reaction.Key.ToString());
                    users.AddRange(tUsers);
                } catch (Exception ex) { }
            }
            
            users = users.Distinct(new DistintUserComparer()).ToList();
            var user = users[new Random().Next(users.Count)];
          
            await Context.Channel.SendMessageAsync($"Aaaaaand ... it's {user.Username}! :star:");
        }


        public async Task Help(SocketCommandContext context, CommandService commandService)
        {
            var msgStr = context.Message.Content;
            string[] words = msgStr.Split(null);
            string cmd = null;
            if (words[0] != $"{StaticSettings.prefix}help" && words[0] != $"{StaticSettings.prefix}h")
                return;
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
                    desc = CommandHelpString(commandService.Commands.Where(x => x.Aliases.Any(y => y.Equals(cmd, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault());
                } catch (Exception ex) { };
                try
                {
                    eb = ModuleHelpEmbed(commandService.Modules.Where(x => x.Name.Equals(cmd, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
                } catch (Exception ex) { };
            #pragma warning restore CS0168 // Variable is declared but never used
            }
            else
                eb = AllHelpEmbed(commandService, context.Guild == null ? false : context.Guild.Id == StaticSettings.home_server);

            if(!desc.Equals(""))
            {
                await context.Channel.SendMessageAsync(desc);
                return;
            }
            await context.Channel.SendMessageAsync($":star: Type `{StaticSettings.prefix}h [command_name]` for more information about a command.", false, eb.Build());
        }

        private EmbedBuilder AllHelpEmbed(CommandService commandService, bool all = false)
        {
            var eb = new EmbedBuilder();
            //eb.WithTitle("Commands");

            foreach(var x in commandService.Modules)
            {
                if ((x.Name != "SpecialCommands" && x.Name != "SpecialModes") || all)
                {
                    var fb = new EmbedFieldBuilder();
                    fb.Name = x.Name;

                    string commandList = "";
                    foreach (var y in x.Commands)
                        commandList += $"`{y.Name}` ";

                    fb.Value = commandList;
                    eb.AddField(fb);
                }
            }

            //eb.WithDescription($"Type `{StaticSettings.prefix}h [command_name]` for more information about a command.");
            eb.WithFooter(@"""What are you? Twelve?"" -Namiko");
            eb.WithColor(BasicUtil.RandomColor());
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
                    //desc += $"  **{x.Name}** - `{StaticSettings.prefix}{x.Aliases[1]}`\n{x.Summary}\n";
                    desc += $"  **{x.Name}**\n{x.Summary}\n";
#pragma warning disable CS0168 // Variable is declared but never used
                } catch (Exception ex) { }
#pragma warning restore CS0168 // Variable is declared but never used
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

        public string CommandHelpString(CommandInfo commandInfo)
        {
            string desc = "";
            desc += $":star: **{commandInfo.Name.ToUpper()}**\n";
            
            desc += $"**Description**: {commandInfo.Summary}\n\n";
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
                } catch (Exception ex) { }

                try
                {
                    var prec = x as CustomPrecondition;
                    desc += $"{prec.GetName()} ";
                } catch (Exception ex) { }
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
