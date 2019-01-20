using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using Namiko.Core.Util;
using Namiko.Resources.Preconditions;

namespace Namiko.Core.Modules
{
    public class Server : ModuleBase<SocketCommandContext>
    {
        [Command("SetJoinLogChannel"), Alias("jlch"), Summary("Sets a channel to log users joining/leaving the guild.\n**Usage**: `!jlch`"), CustomUserPermission(GuildPermission.ManageChannels)]
        public async Task SetJoinLogChannel()
        {
            var server = ServerDb.GetServer(Context.Guild.Id);

            if(server.JoinLogChannelId == Context.Channel.Id)
            {
                server.JoinLogChannelId = 0;
                await ServerDb.UpdateServer(server);
                await Context.Channel.SendMessageAsync("Join Log channel removed.");
                return;
            }

            server.JoinLogChannelId = Context.Channel.Id;
            await ServerDb.UpdateServer(server);
            await Context.Channel.SendMessageAsync("Join Log channel set.");
        }

        [Command("SetTeamLogChannel"), Alias("tlch"), Summary("Sets a channel to log users joining/leaving teams.\n**Usage**: `!tlch`"), CustomUserPermission(GuildPermission.ManageChannels)]
        public async Task SetTeamLogChannel()
        {
            var server = ServerDb.GetServer(Context.Guild.Id);

            if (server.TeamLogChannelId == Context.Channel.Id)
            {
                server.TeamLogChannelId = 0;
                await ServerDb.UpdateServer(server);
                await Context.Channel.SendMessageAsync("Team Log channel removed.");
                return;
            }

            server.TeamLogChannelId = Context.Channel.Id;
            await ServerDb.UpdateServer(server);
            await Context.Channel.SendMessageAsync("Team Log channel set.");
        }

        [Command("SetWelcomeChannel"), Alias("wch"), Summary("Sets a channel to welcome members.\n**Usage**: `!wch`"), CustomUserPermission(GuildPermission.ManageChannels)]
        public async Task SetWelcomeChannel()
        {
            var server = ServerDb.GetServer(Context.Guild.Id);

            if (server.WelcomeChannelId == Context.Channel.Id)
            {
                server.WelcomeChannelId = 0;
                await ServerDb.UpdateServer(server);
                await Context.Channel.SendMessageAsync("Welcome channel removed.");
                return;
            }

            server.WelcomeChannelId = Context.Channel.Id;
            await ServerDb.UpdateServer(server);
            await Context.Channel.SendMessageAsync("Welcome channel set.");
        }

        [Command("BlacklistChannel"), Alias("blch"), Summary("Disables or enables bot commands in a channel.\n**Usage**: `!blch [optional_channel_id]`"), CustomUserPermission(GuildPermission.ManageChannels)]
        public async Task BlacklistChannel(ulong channelId = 0)
        {
            if (channelId == 0)
                channelId = Context.Channel.Id;

            else if(Context.Guild.GetChannel(channelId) == null)
            {
                await Context.Channel.SendMessageAsync("Can't find channel.");
                return;
            }

            if(BlacklistedChannelDb.IsBlacklisted(channelId))
            {
                await BlacklistedChannelDb.DeleteBlacklistedChannel(channelId);
                await Context.Channel.SendMessageAsync("Channel unblacklisted.");
                return;
            }

            await BlacklistedChannelDb.UpdateBlacklistedChannel(new BlacklistedChannel { ChannelId = channelId });
            await Context.Channel.SendMessageAsync("Channel blacklisted.");
        }

        [Command("SetBotPrefix"), Alias("sbp"), Summary("Sets a prefix for the bot in the server.\n**Usage**: `!sbp [prefix]`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task SetBotPrefix(string prefix)
        {
            var server = ServerDb.GetServer(Context.Guild.Id);
            
            server.Prefix = prefix;
            await ServerDb.UpdateServer(server);
            await Context.Channel.SendMessageAsync($"My prefix is now {prefix}");
        }
    }
}
