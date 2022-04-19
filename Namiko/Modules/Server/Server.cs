using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using Model;
using Namiko.Modules.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    [RequireGuild]
    [Name("Server")]
    public class ServerModule : InteractiveBase<ShardedCommandContext>
    {
        [Command("Server"), Alias("serverinfo", "guild", "stats"), Summary("Stats about the server.\n**Usage**: `!server`")] 
        public async Task ServerInfo([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync("", false, (await ServerUtil.ServerInfo(Context.Guild)).Build());
        }

        [Command("SetPrefix"), Alias("sp", "sbp", "setbotprefix"), Summary("Sets a prefix for the bot in the server.\n**Usage**: `!sp [prefix]`"), CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task SetBotPrefix(string prefix)
        {
            if (prefix.Length < 1)
                return;

            var server = ServerDb.GetServer(Context.Guild.Id);

            server.Prefix = prefix;
            await ServerDb.UpdateServer(server);
            Program.UpdatePrefix(Context.Guild.Id, prefix);
            await Context.Channel.SendMessageAsync($"My prefix is now `{prefix}`");
        }

        [Command("Prefix"), Alias("sp", "sbp", "setbotprefix"), Summary("View the current prefix.\n**Usage**: `!sp [prefix]`")]
        public async Task Prefix([Remainder] string str = "")
        {
            var prefix = Program.GetPrefix(Context);
            await Context.Channel.SendMessageAsync($"My current prefix is `{prefix}`.\nYou can change it by typing `{prefix}sp [new_prefix]` without the brackets :fox:");
        }

        [Command("SetJoinLogChannel"), Alias("jch", "jlch", "sjch", "sjlch"), Summary("Sets a channel to log users joining/leaving the guild.\n**Usage**: `!jlch`"), CustomUserPermission(GuildPermission.ManageChannels)]
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

        [Command("SetTeamLogChannel"), Alias("tch"), Summary("Sets a channel to log users joining/leaving teams.\n**Usage**: `!tlch`"), CustomUserPermission(GuildPermission.ManageChannels)]
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
            await Context.Channel.SendMessageAsync($"Channel blacklisted. Use `{Program.GetPrefix(Context)}blch [channel_id]` in another channel to undo.\n The ID of this channel is `{Context.Channel.Id}`.");
        }

        //[Command("ListWelcomes"), Alias("lw"), Summary("Lists all welcomes and their IDs.")]
        //public async Task ListWelcome()
        //{

        //    List<WelcomeMessage> messages = WelcomeMessageDb.GetMessages();
        //    string list = @"```";
        //    foreach (WelcomeMessage x in messages)
        //    {
        //        list += x.Id + ". ";
        //        list += x.Message;
        //        list += '\n';
        //    }
        //    list += "```";
        //    await Context.Channel.SendMessageAsync(list);
        //}

        [Command("ActivateProGuild"), Alias("asp", "ActivateServerPremium", "apg"), Summary("Activates pro guild in the current server.\n**Usage**: `!asp [tier]`")]
        public async Task ActivateServerPremium([Remainder] string str = "")
        {
            var ntr = Context.Client.GetGuild((ulong)ProType.HomeGuildId_NOTAPREMIUMTYPE);
            SocketGuildUser user = ntr.GetUser(Context.User.Id);

            if (user == null)
            {
                await Context.Channel.SendMessageAsync($"You are not in my server! {LinkHelper.SupportServerInvite}");
                return;
            }

            var current = PremiumDb.GetGuildPremium(Context.Guild.Id);
            var roles = user.Roles;

            bool log = false;
            string text = "";
            foreach (var role in roles)
            {
                if (role.Id == (ulong)ProType.GuildPlus)
                {
                    if (current.Any(x => x.Type == ProType.GuildPlus))
                    {
                        text += "**Pro Guild+** is already activated in this server!\n";
                    }
                    else
                    {
                        if (PremiumDb.IsPremium(user.Id, ProType.GuildPlus))
                            text += "You used your **Pro Guild+** premium upgrade in another server...\n";

                        else
                        {
                            await PremiumDb.AddPremium(user.Id, ProType.GuildPlus, Context.Guild.Id);
                            try
                            {
                                await PremiumDb.DeletePremium(PremiumDb.GetGuildPremium(Context.Guild.Id).FirstOrDefault(x => x.Type == ProType.Guild));
                            }
                            catch { }
                            text += "**Pro Guild+** activated!\n";
                            log = true;
                        }
                    }
                    break;
                }
                if (role.Id == (ulong)ProType.Guild)
                {
                    if (current.Any(x => x.Type == ProType.GuildPlus))
                        text += "**Pro Guild+** is already activated in this server!\n";
                    else if (current.Any(x => x.Type == ProType.Guild))
                        text += "**Pro Guild** is already activated in this server!\n";
                    else
                    {
                        if (PremiumDb.IsPremium(user.Id, ProType.Guild))
                            text += "You used your **Pro Guild** premium upgrade in another server...\n";

                        else
                        {
                            await PremiumDb.AddPremium(user.Id, ProType.Guild, Context.Guild.Id);
                            text += "**Pro Guild** activated!\n";
                            log = true;
                        }
                    }
                    break;
                }
            }
            if (text == "")
                text += $"You have no Pro Guild... Try `{Program.GetPrefix(Context)}Pro`";

            await Context.Channel.SendMessageAsync(text);
            if (log)
            {
                await WebhookClients.PremiumLogChannel.SendMessageAsync(embeds: new List<Embed>
                    {
                        new EmbedBuilderPrepared(Context.User)
                            .WithDescription($"{Context.User.Mention} `{Context.User.Id}`\n{Context.Guild.Name} `{Context.Guild.Id}`\n{text}")
                            .WithFooter(System.DateTime.Now.ToLongDateString())
                            .Build()
                    });
            }
        }

        [Command("ToggleModule"), Alias("tm"), Summary("Disables or enables a command module.\n**Usage**: `!tm [module_name]`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task ToggleModule([Remainder] string name)
        {
            var cmdService = Program.GetCommands();
            var module = cmdService.Modules.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

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

        [Command("ToggleCommand"), Alias("tc"), Summary("Disables or enables a command.\n**Usage**: `!tc [command_name]`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task ToggleCommand([Remainder] string name)
        {
            var cmdService = Program.GetCommands();
            var command = cmdService.Commands.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

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

        [Command("ToggleReactionImages"), Alias("tri"), Summary("Disables or enables reaction images.\n**Usage**: `!tri`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task ToggleReactionImages([Remainder] string name = "")
        {
            if (await DisabledCommandHandler.AddNew(name, Context.Guild.Id, DisabledCommandType.Images))
            {
                await ReplyAsync($":star: **Reaction images** disabled... Use the same command again to re-enable");
                return;
            }
            else
            {
                await DisabledCommandHandler.Remove(name, Context.Guild.Id, DisabledCommandType.Images);
                await ReplyAsync($":star: **Reaction images** re-enabled.");
                return;
            }
        }

        [Command("ListDisabledCommands"), Alias("ldc"), Summary("Lists all disabled modules and commands.\n**Usage**: `!ldc`")]
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
