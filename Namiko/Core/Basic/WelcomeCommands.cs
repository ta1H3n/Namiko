using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Resources.Attributes;
using Namiko.Resources.Datatypes;

namespace Namiko.Core.Basic
{
    public class Welcomes : ModuleBase<SocketCommandContext>
    {
        //private Dictionary<ulong, SocketTextChannelO> WelcomeChannels;

        [Command("NewWelcome"), Alias("nwlc"), Summary("Adds a new welcome message. @_ will be replaced with a mention.\n**Usage**: `!nw [welcome]`"), HomePrecondition]
        public async Task NewWelcome([Remainder] string message)
        {
            Contract.Requires(Context.Channel.Id.Equals(StaticSettings.home_server) || Context.User.Id.Equals(StaticSettings.owner));

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

        // [Command("TestWelcome"), Alias("tw"), HomePrecondition]
        // public async Task TestWelcome()
        // {
        //     var channel = GetWelcomeChannel(Context.Guild.Id, Context.Client);
        //     string str = WelcomeMessageConvert(Context.User);
        //     await channel.SendMessageAsync(str);
        // }

        [Command("SetWelcomeChannel"), Alias("swc"), Summary("Set's the welcome channel. Current channel or provided ID.\n**Usage**: `!swc [id]`"), RequireUserPermission(GuildPermission.Administrator)]
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
            //SetUpWelcomeChannels(Context.Client);
            //WelcomeChannels.Add(Context.Guild.Id, Context.Client.GetChannel(id) as SocketTextChannel);

            await Context.Channel.SendMessageAsync($"{id} Set as the welcome channel.");
            // await TestWelcome();
        }


    }
    public static class WelcomeUtil { 
        // public void SetUpWelcomeChannels(DiscordSocketClient client)
        // {
        //     var channels = WelcomeMessageDb.GetWelcomeChannels();
        //     WelcomeChannels = new Dictionary<ulong, SocketTextChannel>();
        //     Console.WriteLine($"Count: {channels.Count}");
        //     foreach(var x in channels)
        //     {
        //         var guild = client.GetGuild(x.GuildId);
        //         var ch = guild.GetTextChannel(x.ChannelId);
        //         Console.WriteLine($"Adding: {ch.Id}");
        //         WelcomeChannels.Add(x.GuildId, ch);
        //     }
        // }
        // public async Task SendWelcome(SocketGuildUser user, DiscordSocketClient client)
        // {
        //     var channel = GetWelcomeChannel(user.Guild.Id, client);
        //     await channel.SendMessageAsync(WelcomeMessageConvert(user));
        // }
        public static string WelcomeMessageConvert(SocketUser user)
        {
            string message = WelcomeMessageDb.GetRandomMessage();
            message = message.Replace("@_", user.Mention);
            return message;
        }
        // private SocketTextChannel General(SocketGuild guild)
        // {
        //     var channels = guild.TextChannels;
        //     foreach(SocketTextChannel x in channels)
        //     {
        //         if(x.Name == "general")
        //         {
        //             return x;
        //         }
        //     }
        //     return null;
        // }
        // private SocketTextChannel GetWelcomeChannel(ulong guildId, DiscordSocketClient client)
        // {
        //     if(WelcomeChannels == null)
        //     {
        //         SetUpWelcomeChannels(client);
        //     }
        //     return WelcomeChannels.GetValueOrDefault(guildId);
        // }
    }
}
