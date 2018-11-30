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
    public class BasicCommands : ModuleBase<SocketCommandContext>
    {
        static ISocketMessageChannel ch;

        [Command("Hi Namiko"), Alias("Hi"), Summary("Hi Namiko command")]
        public async Task HiNamiko()
        {
            await Context.Channel.SendMessageAsync($"Hi {Context.User.Mention} :fox:");
        }

        [Command("SetSayCh"), Alias("ssch"), OwnerPrecondition]
        public async Task SetSayChannel(ulong id)
        {
            ch = Context.Client.GetChannel(id) as ISocketMessageChannel;
            await Context.Channel.SendMessageAsync($"{ch.Name} set as say channel.");
        }

        [Command("SayCh"), Alias("sch"), OwnerPrecondition]
        public async Task Say([Remainder] string str)
        {
            if(ch == null)
            {
                ch = Context.Client.GetChannel(417064769309245475) as ISocketMessageChannel;
            }
            await ch.SendMessageAsync(str);
        }

        [Command("Say"), Alias("s"), OwnerPrecondition]
        public async Task SayChannel(ulong id, [Remainder] string str)
        {
            ISocketMessageChannel ch = Context.Client.GetChannel(id) as ISocketMessageChannel;
            await ch.SendMessageAsync(str);
        }

        [Command("Sayd"), Alias("sd"), OwnerPrecondition]
        public async Task SayDelete([Remainder] string str)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(str);
        }

        [Command("Playing"), Summary("Sets the palying status."), OwnerPrecondition]
        public async Task Playing([Remainder] string str)
        {
            await Context.Client.SetGameAsync(str);
        }

        [Command("RandomUser"), Alias("ru"), Summary("Randomly picks one user from all the reactions on a message.\n`!ru [message_id]`")]
        public async Task RandomUser(ulong msgId)
        {
            var msg = await Context.Channel.GetMessageAsync(msgId) as RestUserMessage;
            List<IUser> users = new List<IUser>();
            Console.WriteLine($"{msg.Content}   {msg.GetType()}");
            Console.WriteLine($"{msg.Reactions.First().Key.ToString()}");
            foreach (var reaction in msg.Reactions)
            {
                var tUsers = await msg.GetReactionUsersAsync(reaction.Key.ToString());
                Console.WriteLine($"{tUsers.Count}");
                users.AddRange(tUsers);
            }
          
            var user = users[new Random().Next(users.Count)];
          
            await Context.Channel.SendMessageAsync($"Aaaaaand ... it's {user.Username}!");
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
                eb = AllHelpEmbed(commandService);

            if(!desc.Equals(""))
            {
                await context.Channel.SendMessageAsync(desc);
                return;
            }
            eb.Color = BasicUtil.GetColor(context.Guild.GetUser(context.User.Id));
            await context.Channel.SendMessageAsync("", false, eb.Build());
        }

        private EmbedBuilder AllHelpEmbed(CommandService commandService)
        {
            var eb = new EmbedBuilder();
            eb.WithTitle("Commands");

            foreach(var x in commandService.Modules)
            {
                if (x.Name != "BasicCommands")
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

            eb.WithDescription($"Type `{StaticSettings.prefix}h [command_name]` for more information about a command.");
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
                desc += $"{x.ToString()} ";

            return desc;
        }
    }
}
