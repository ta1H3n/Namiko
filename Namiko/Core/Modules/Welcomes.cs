using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Resources.Preconditions;
using Namiko.Resources.Datatypes;
using Namiko.Resources.Database;
using Namiko.Core.Util;
using Discord.Addons.Interactive;

namespace Namiko.Core.Modules
{
    public class Welcomes : InteractiveBase<SocketCommandContext>
    {
        [Command("NewWelcome"), Alias("nwlc"), Summary("Adds a new welcome message. @_ will be replaced with a mention.\n**Usage**: `!nw [welcome]`"), HomePrecondition]
        public async Task NewWelcome([Remainder] string message)
        {
            if (message.Length < 20)
            {
                await Context.Channel.SendMessageAsync("Message must be longer than 20 characters.");
                return;
            }
            await WelcomeMessageDb.AddMessage(message);
            await Context.Channel.SendMessageAsync("Message added: '" + message.Replace("@_", Context.User.Mention) + "'");
        }

        [Command("ListWelcome"), Alias("lw"), Summary("Lists all welcomes and their IDs."), HomePrecondition]
        public async Task ListWelcome()
        {

            List<WelcomeMessage> messages = WelcomeMessageDb.GetMessages();
            string list = @"```";
            foreach (WelcomeMessage x in messages)
            {
                list += x.Id + ". ";
                list += x.Message;
                list += '\n';
            }
            list += "```";
            await Context.Channel.SendMessageAsync(list);
        }

        [Command("DeleteWelcome"), Alias("dw", "delwelcome"), Summary("Deletes a welcome message by ID.\n**Usage**: `!dw [id]`"), HomePrecondition]
        public async Task DeleteWelcome(int id)
        {

            WelcomeMessage message = WelcomeMessageDb.GetMessage(id);
            if (message == null)
                await Context.Channel.SendMessageAsync($"Message with id: {id} not found");
            else
            {
                await WelcomeMessageDb.DeleteMessage(id);
                await Context.Channel.SendMessageAsync($"Deleted welcome message with id: {id}");
            }
        }

        [Command("SetWelcomeChannel"), Alias("swc"), Summary("Set's the welcome channel. Current channel or provided ID.\n**Usage**: `!swc [id]`"), CustomUserPermission(GuildPermission.ManageChannels)]
        public async Task SetWelcomeChannel(long inputId = 1)
        {
            ulong id = 0;
            if (inputId == 1)
            {
                id = Context.Channel.Id;
            }
            else
            {
                id = (ulong)inputId;
            }
            await WelcomeMessageDb.SetWelcomeChannel(new WelcomeChannel { GuildId = Context.Guild.Id, ChannelId = id });

            await Context.Channel.SendMessageAsync($"{id} Set as the welcome channel.");
        }
    }
}
