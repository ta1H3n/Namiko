using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Namiko.Modules.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Discord.Interactions;

namespace Namiko
{
    [RequireGuild]
    [Name("Server")]
    public class ServerModule : CustomModuleBase<ICustomContext>
    {
        public BaseSocketClient Client { get; set; }
        public TextCommandService TextCommands { get; set; }
        
        
        
        [Command("Server"), Alias("serverinfo", "guild", "stats"), Description("Stats about the server.\n**Usage**: `!server`")] 
        [SlashCommand("server", "Show server stats")]
        public async Task ServerInfo()
        {
            await ReplyAsync("", false, (await ServerUtil.ServerInfo(Context.Guild, Client)).Build());
        }

        [UserPermission(GuildPermission.ManageMessages)]
        [Command("SetPrefix"), Alias("sp", "sbp", "setbotprefix"), Description("Sets a prefix for the bot in the server.\n**Usage**: `!sp [prefix]`")]
        public async Task SetBotPrefix(string prefix)
        {
            if (prefix.Length < 1)
                return;

            var server = ServerDb.GetServer(Context.Guild.Id);

            server.Prefix = prefix;
            await ServerDb.UpdateServer(server);
            TextCommandService.UpdatePrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"My prefix is now `{prefix}`");
        }

        [Command("Prefix"), Alias("sp", "sbp", "setbotprefix"), Description("View the current prefix.\n**Usage**: `!sp [prefix]`")]
        public async Task Prefix()
        {
            var prefix = TextCommandService.GetPrefix(Context);
            await ReplyAsync($"My current prefix is `{prefix}`.\nYou can change it by typing `{prefix}sp [new_prefix]` without the brackets :fox:");
        }

        [UserPermission(GuildPermission.ManageChannels)]
        [Command("SetJoinLogChannel"), Alias("jch", "jlch", "sjch", "sjlch"), Description("Sets a channel to log users joining/leaving the guild.\n**Usage**: `!jlch`")]
        [SlashCommand("channel-join-log", "Set a channel to log users joining and leaving")]
        public async Task SetJoinLogChannel()
        {
            var server = ServerDb.GetServer(Context.Guild.Id);

            if(server.JoinLogChannelId == Context.Channel.Id)
            {
                server.JoinLogChannelId = 0;
                await ServerDb.UpdateServer(server);
                await ReplyAsync("Join Log channel removed.");
                return;
            }

            server.JoinLogChannelId = Context.Channel.Id;
            await ServerDb.UpdateServer(server);
            await ReplyAsync("Join Log channel set.");
        }

        [UserPermission(GuildPermission.ManageChannels)]
        [Command("SetTeamLogChannel"), Alias("tch"), Description("Sets a channel to log users joining/leaving teams.\n**Usage**: `!tlch`")]
        [SlashCommand("channel-team-log", "Set a channel to log users joining and leaving teams")]
        public async Task SetTeamLogChannel()
        {
            var server = ServerDb.GetServer(Context.Guild.Id);

            if (server.TeamLogChannelId == Context.Channel.Id)
            {
                server.TeamLogChannelId = 0;
                await ServerDb.UpdateServer(server);
                await ReplyAsync("Team Log channel removed.");
                return;
            }

            server.TeamLogChannelId = Context.Channel.Id;
            await ServerDb.UpdateServer(server);
            await ReplyAsync("Team Log channel set.");
        }

        [UserPermission(GuildPermission.ManageChannels)]
        [Command("SetWelcomeChannel"), Alias("wch"), Description("Sets a channel to welcome members.\n**Usage**: `!wch`")]
        [SlashCommand("channel-welcome", "Sets a channel to welcome members")]
        public async Task SetWelcomeChannel()
        {
            var server = ServerDb.GetServer(Context.Guild.Id);

            if (server.WelcomeChannelId == Context.Channel.Id)
            {
                server.WelcomeChannelId = 0;
                await ServerDb.UpdateServer(server);
                await ReplyAsync("Welcome channel removed.");
                return;
            }

            server.WelcomeChannelId = Context.Channel.Id;
            await ServerDb.UpdateServer(server);
            await ReplyAsync("Welcome channel set.");
        }

        [UserPermission(GuildPermission.ManageChannels)]
        [Command("BlacklistChannel"), Alias("blch"), Description("Disables or enables bot commands in a channel.\n**Usage**: `!blch [optional_channel_id]`")]
        public async Task BlacklistChannel(ulong channelId = 0)
        {
            if (channelId == 0)
                channelId = Context.Channel.Id;

            else if(Context.Guild.GetChannel(channelId) == null)
            {
                await ReplyAsync("Can't find channel.");
                return;
            }

            if(BlacklistedChannelDb.IsBlacklisted(channelId))
            {
                await BlacklistedChannelDb.DeleteBlacklistedChannel(channelId);
                await ReplyAsync("Channel unblacklisted.");
                return;
            }

            await BlacklistedChannelDb.UpdateBlacklistedChannel(new BlacklistedChannel { ChannelId = channelId });
            await ReplyAsync($"Channel blacklisted. Use `{TextCommandService.GetPrefix(Context)}blch [channel_id]` in another channel to undo.\n The ID of this channel is `{Context.Channel.Id}`.");
        }

        [UserPermission(GuildPermission.Administrator)]
        [Command("ToggleModule"), Alias("tm"), Description("Disables or enables a command module.\n**Usage**: `!tm [module_name]`")]
        public async Task ToggleModule([Remainder] string name)
        {
            var module = TextCommands.Commands.Modules.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (module == null)
            {
                await ReplyAsync($":x: There is no module called `{name}`");
                return;
            }

            if (module.Name.Equals("Server") || module.Name.Equals("Basic"))
            {
                await ReplyAsync($":x: The **{module.Name}** module can't be disabled.");
                return;
            }

            if (await DisabledCommandHandler.AddNew(module.Name, Context.Guild.Id, DisabledCommandType.Module))
            {
                await ReplyAsync($":star: Module **{module.Name}** disabled... Use the same command again to re-enable");
                return;
            }
            else
            {
                await DisabledCommandHandler.Remove(module.Name, Context.Guild.Id, DisabledCommandType.Module);
                await ReplyAsync($":star: Module **{module.Name}** re-enabled.");
                return;
            }
        }

        [UserPermission(GuildPermission.Administrator)]
        [Command("ToggleCommand"), Alias("tc"), Description("Disables or enables a command.\n**Usage**: `!tc [command_name]`")]
        public async Task ToggleCommand([Remainder] string name)
        {
            var command = TextCommands.Commands.Commands.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (command == null)
            {
                await ReplyAsync($":x: There is no command called `{name}`");
                return;
            }

            if (command.Name.Equals("ToggleModule") || 
                command.Name.Equals("ToggleCommand") || 
                command.Name.Equals("ToggleReactionImages") ||
                command.Name.Equals("ListDisabledCommands"))
            {
                await ReplyAsync($":x: The **{command.Name}** command can't be disabled.");
                return;
            }

            if (await DisabledCommandHandler.AddNew(command.Name, Context.Guild.Id, DisabledCommandType.Command))
            {
                await ReplyAsync($":star: Command **{command.Name}** disabled... Use the same command again to re-enable");
                return;
            }
            else
            {
                await DisabledCommandHandler.Remove(command.Name, Context.Guild.Id, DisabledCommandType.Command);
                await ReplyAsync($":star: Command **{command.Name}** re-enabled.");
                return;
            }
        }

        [UserPermission(GuildPermission.Administrator)]
        [Command("ToggleReactionImages"), Alias("tri"), Description("Disables or enables reaction images.\n**Usage**: `!tri`")]
        public async Task ToggleReactionImages()
        {
            if (await DisabledCommandHandler.AddNew("ReactionImages", Context.Guild.Id, DisabledCommandType.Images))
            {
                await ReplyAsync($":star: **Reaction images** disabled... Use the same command again to re-enable");
                return;
            }
            else
            {
                await DisabledCommandHandler.Remove("ReactionImages", Context.Guild.Id, DisabledCommandType.Images);
                await ReplyAsync($":star: **Reaction images** re-enabled.");
                return;
            }
        }

        [Command("ListDisabledCommands"), Alias("ldc"), Description("Lists all disabled modules and commands.\n**Usage**: `!ldc`")]
        public async Task ListDisabledCommands([Remainder] string name = "")
        {
            string modules = "-";
            string commands = "-";

            if (DisabledCommandHandler.DisabledCommands.TryGetValue(Context.Guild.Id, out var set) && set.Any())
            {
                commands = string.Join(" | ", set);
            }
            if (DisabledCommandHandler.DisabledModules.TryGetValue(Context.Guild.Id, out set) && set.Any())
            {
                modules = string.Join(" | ", set);
            }

            var eb = new EmbedBuilderPrepared(Context.User);
            eb.AddField("Disabled Modules", modules);
            eb.AddField("Disabled Commands", commands);
            if (DisabledCommandHandler.DisabledImages.Contains(Context.Guild.Id))
            {
                eb.AddField("Reaction Images", "All Disabled");
            }
            await ReplyAsync(embed: eb.Build());
        }
    }
}
