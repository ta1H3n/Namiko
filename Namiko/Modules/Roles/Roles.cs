using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;

namespace Namiko
{
    [RequireGuild]
    public class Roles : CustomModuleBase<ICustomContext>
    {
        [BotPermission(GuildPermission.ManageRoles)]
        [Command("Role"), Alias("r", "iam"), Description("Adds or removes a public role from the user.\n**Usage**: `!r [name]`")]
        [SlashCommand("role", "Get or lose a public role")]
        public async Task Role([Remainder] string name = "")
        {
            var role = await this.SelectRole(PublicRoleDb.GetAll(Context.Guild.Id).Select(x => x.RoleId), name);
            if (role == null)
            {
                await ReplyAsync($"No public roles found. {(name == "" ? "" : $"Search: `{name}`")}\n`{TextCommandService.GetPrefix(Context)}spr` - make a role public.\n`{TextCommandService.GetPrefix(Context)}prl` - view a list of public roles.");
                return;
            }

            var guildUser = Context.User as SocketGuildUser;
            if (guildUser.Roles.Contains(role))
            {
                try
                {
                    await guildUser.RemoveRoleAsync(role);
                }
                catch
                {
                    await ReplyAsync($"I tried removing a role that is above my own role. *Coughs blood.* Discord wouldn't let me...");
                    return;
                }
                await ReplyAsync($"Removed the `{role.Name}` role from you. Don't come back, BAAAAAAAAAAAAAAKA!");
            }
            else
            {
                try
                {
                    await guildUser.AddRoleAsync(role);
                }
                catch
                {
                    await ReplyAsync($"I tried giving a role that is above my own role. *Coughs blood.* Discord wouldn't let me...");
                    return;
                }
                await ReplyAsync($"Gave you the `{role.Name}` role! Welcome to the club! o7");
            }
        }

        [Command("PublicRoleList"), Alias("prl", "roles"), Description("Lists all public roles.\n**Usage**: `!prl`")]
        [SlashCommand("role-list", "Lists all public roles that can be received with the role command")]
        public async Task PublicRoleList()
        {
            var roles = new List<SocketRole>();
            foreach (var x in PublicRoleDb.GetAll(Context.Guild.Id))
            {
                roles.Add(Context.Guild.GetRole(x.RoleId));
            }

            var eb = RoleUtil.PublicRoleListEmbed(roles);
            await ReplyAsync("", false, eb.Build());
        }

        [UserPermission(GuildPermission.ManageRoles)]
        [Command("SetPublicRole"), Alias("spr"), Description("Sets or unsets a role as a public role.\n**Usage**: `!spr [name]`")]
        [SlashCommand("role-set-public", "Make or undo a role as public, to add with the role command")]
        public async Task NewRole([Remainder] IRole role)
        {
            if (role == null)
            {
                return;
            }

            int userHigh = ((SocketGuildUser)Context.User).Roles.Max(x => x.Position);
            if (userHigh <= role.Position)
            {
                await ReplyAsync($":x: You do not have permission to modify `{role.Name}`! Make sure you have a role higher in the list.");
                return;
            }

            if (!PublicRoleDb.IsPublic(role.Id))
            {
                await PublicRoleDb.Add(role.Id, Context.Guild.Id);
                await ReplyAsync($"`{role.Name}` set as a public role.");
                return;
            }
            else
            {
                await PublicRoleDb.Delete(role.Id);
                await ReplyAsync($"`{role.Name}` removed from public roles.");
                return;
            }
        }

        [Command("RoleShop"), Alias("rs"), Description("Open a role shop managed by the server admins.\n**Usage**: `!rs`")]
        [SlashCommand("role-shop", "Open the role shop")]
        public async Task RoleShop()
        {
            var roles = await ShopRoleDb.GetRoles(Context.Guild.Id);

            var eb = new EmbedBuilderPrepared()
                .WithTitle(":star: Role Shop");

            if (!roles.Any())
            {
                await ReplyAsync(embed: eb.WithDescription(" *~ No roles in shop ~*\n").WithColor(Color.DarkRed).Build());
                return;
            }

            await ReplyAsync(embed:
                eb.WithDescription(PaginatedMessage.PagesArray(roles.OrderByDescending(x => x.Price), 100, (r) => $"<@&{r.RoleId}> - **{r.Price:n0}**\n").First()).Build());
        }

        [BotPermission(GuildPermission.ManageRoles)]
        [Command("BuyRole"), Description("Buy a role from the role shop.\n**Usage**: `!buyrole [role_name]`")]
        [SlashCommand("role-buy", "Buy a role from the role shop")]
        public async Task RoleShopAddRole([Remainder] string name = "")
        {
            var roles = await ShopRoleDb.GetRoles(Context.Guild.Id);

            var role = await this.SelectRole(roles.Select(x => x.RoleId), name);

            if (role == null)
                return;

            var user = Context.User as SocketGuildUser;
            if (user.Roles.Contains(role))
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared($"You already have **{role.Mention}** !").Build());
                return;
            }

            int price = roles.FirstOrDefault(x => x.RoleId == role.Id).Price;

            try
            {
                await BalanceDb.AddToasties(Context.User.Id, -price, Context.Guild.Id);
                await user.AddRoleAsync(role);
                await ReplyAsync(embed: new EmbedBuilderPrepared($"You bought the **{role.Mention}** role!").Build());
            } catch(Exception ex)
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(ex.Message).WithColor(Color.DarkRed).Build());
            }
        }

        [UserPermission(GuildPermission.ManageRoles), BotPermission(GuildPermission.ManageRoles)]
        [Command("RoleShopAddRole"), Alias("rsar"), Description("Add a role to the role shop.\n**Usage**: `!rsar [price] [role_name]`")]
        [SlashCommand("role-shop-add", "Add a role to the role shop")]
        public async Task RoleShopAddRole(int price, [Remainder] IRole role)
        {
            if (price < 0)
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared($"~ Price can't be negative ~")
                    .WithColor(Color.DarkRed)
                    .Build());
                return;
            }

            if (role == null)
                return;


            int userHigh = ((SocketGuildUser)Context.User).Roles.Max(x => x.Position);
            if (userHigh <= role.Position)
            {
                await ReplyAsync($":x: You do not have permission to modify `{role.Name}`! Make sure you have a role higher in the list.");
                return;
            }

            if (await ShopRoleDb.IsRole(role.Id))
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared($"~ **{role.Name}** is already in the shop ~")
                    .WithColor(Color.DarkRed)
                    .Build());
                return;
            }

            await ShopRoleDb.AddRole(Context.Guild.Id, role.Id, price);
            await ReplyAsync(embed: new EmbedBuilderPrepared($"~ **{role.Name}** added to the shop ~").Build());
        }

        [UserPermission(GuildPermission.Administrator)]
        [Command("RoleShopRemoveRole"), Alias("rsrr"), Description("Remove a role from the role shop.\n**Usage**: `!rsrr [role_name]`")]
        [SlashCommand("role-shop-remove", "Remove a role from the role shop")]
        public async Task RoleShopRemoveRole([Remainder] string name = "")
        {
            var roles = await ShopRoleDb.GetRoles(Context.Guild.Id);

            var eb = new EmbedBuilderPrepared()
                .WithTitle(":star: Role Shop");

            if (!roles.Any())
            {
                await ReplyAsync(embed: eb.WithDescription(" *~ No roles in shop ~*\n").WithColor(Color.DarkRed).Build());
                return;
            }

            int i = 1;
            var role = await Select(roles, "Select which role to remove from the role shop", "Role");

            await ShopRoleDb.RemoveRole(role.RoleId);
            await ReplyAsync(embed: eb.WithDescription($" *~ <@&{role.RoleId}> removed ~*\n").Build());
        }

        [Command("Invite"), Description("Invites a user to your team.\n**Usage**: `!inv [user]`")]
        [SlashCommand("team-invite", "Invite a user to your team")]
        public async Task Invite([Remainder] IUser user = null)
        {
            var channel = Context.Channel;
            var leaderRole = RoleUtil.GetLeaderRole(Context.Guild, Context.User);

            if (leaderRole == null)
            {
                await channel.SendMessageAsync("You're not a team leader, silly.");
                return;
            }

            if (RoleUtil.IsTeamed(Context.Guild, user))
            {
                await channel.SendMessageAsync($@"**{user.Username}** is already in a team, I guess you're too late ¯\_(ツ)_/¯");
                return;
            }

            var team = TeamDb.TeamByLeader(leaderRole.Id);
            var teamRole = Context.Guild.GetRole(team.MemberRoleId);

            if (InviteDb.IsInvited(team.MemberRoleId, user.Id))
            {
                await channel.SendMessageAsync($"You already invited **{user.Username}**, baaaaaka.");
                return;
            }

            await InviteDb.NewInvite(team.MemberRoleId, user.Id);
            await channel.SendMessageAsync($"{user.Mention} You're invited to join **{teamRole.Name}**! Type `{TextCommandService.GetPrefix(Context)}jointeam {teamRole.Name}`");

            ISocketMessageChannel ch = (ISocketMessageChannel) Context.Client.GetChannel(ServerDb.GetServer(Context.Guild.Id).TeamLogChannelId);
            await ch.SendMessageAsync($"<:KannaHype:571690048001671238> `{Context.User}` invited `{user}` to **{teamRole.Name}**.");
        }

        [BotPermission(GuildPermission.ManageRoles)]
        [Command("JoinTeam"), Description("Accept an invite to a team.\n**Usage**: `!jointeam [team_name]`")]
        [SlashCommand("team-join", "Accept a team invite")]
        public async Task Join([Remainder] string teamName)
        {
            var role = await SelectRole(TeamDb.Teams(Context.Guild.Id).Select(x => x.MemberRoleId), teamName);

            if (role == null)
            {
                await ReplyAsync($"*~ No teams found ~*");
                return;
            }

            if (InviteDb.IsInvited(role.Id, Context.User.Id))
            {
                var user = Context.Guild.GetUser(Context.User.Id);
                try
                {
                    await user.AddRoleAsync(role);
                } catch
                {
                    await ReplyAsync($"I tried giving a role that is above my own role. *Coughs blood.* Discord wouldn't let me...");
                    return;
                }
                await InviteDb.DeleteInvite(role.Id, user.Id);
                await ReplyAsync($"Congratulations! You joined **{role.Name}**!");
                ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(ServerDb.GetServer(Context.Guild.Id).TeamLogChannelId);
                await ch.SendMessageAsync($"<:TickYes:577838859107303424> `{Context.User}` joined **{role.Name}**.");
                return;
            }
            await ReplyAsync($"You're not invited to **{role.Name}**! What a shame ^^");
        }

        [BotPermission(GuildPermission.ManageRoles)]
        [Command("LeaveTeam"), Alias("lt"), Description("Leave your team.\n**Usage**: `!lt`")]
        [SlashCommand("team-leave", "Leave your team")]
        public async Task Leave()
        {
            var role = RoleUtil.GetMemberRole(Context.Guild, Context.User);
            if (role == null)
            {
                await ReplyAsync("Buuut... You're not in a team.");
                return;
            }

            try
            {
                await Context.Guild.GetUser(Context.User.Id).RemoveRoleAsync(role);
            }
            catch
            {
                await ReplyAsync($"I tried removing a role that is above my own role. *Coughs blood.* Discord wouldn't let me...");
                return;
            }

            await ReplyAsync($@"Ha! You left **{role.Name}**! Too bad for them ¯\_(ツ)_/¯");

            ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(ServerDb.GetServer(Context.Guild.Id).TeamLogChannelId);
            await ch.SendMessageAsync($"<:TickNo:577838859077943306> `{Context.User}` left **{role.Name}**.");
        }

        [BotPermission(GuildPermission.ManageRoles)]
        [Command("TeamKick"), Alias("tk"), Description("Kicks a user from your team.\n**Usage**: `!tk [user]`")]
        [SlashCommand("team-kick", "Kick a member from your team")]
        public async Task TeamKick(IUser user)
        {
            var leader = RoleUtil.GetLeaderRole(Context.Guild, Context.User);
            if (leader == null)
            {
                await ReplyAsync("You're not a leader! Getting ahead of yourself eh?~");
                return;
            }

            var team = TeamDb.TeamByLeader(leader.Id);
            var userteam = RoleUtil.GetMemberRole(Context.Guild, user);
            var leaderteam = Context.Guild.GetRole(team.MemberRoleId);
            if (!userteam.Equals(leaderteam))
            {
                await ReplyAsync($"They're not in your team! If you just want to kick them normaly use `{TextCommandService.GetPrefix(Context)}kick`");
                return;
            }

            await Context.Guild.GetUser(user.Id).RemoveRoleAsync(userteam);
            await ReplyAsync("Ha! One less to take care of!");

            ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(ServerDb.GetServer(Context.Guild.Id).TeamLogChannelId);
            await ch.SendMessageAsync($":hammer: `{Context.User}` kicked `{user}` from **{userteam.Name}**.");
        }

        [UserPermission(GuildPermission.ManageRoles)]
        [Command("NewTeam"), Alias("nt"), Description("Creates a new team.\n**Usage**: `!nt [LeaderRoleName] [MemberRoleName]`\n Note: if a role has a space in it's name it has to be put in quotes. \n For example: `!nt \"Role One\" \"Role Two\"`")]
        [SlashCommand("team-new", "Create a new team")]
        public async Task NewTeam(IRole leader, [Remainder] IRole member)
        {
            if (leader == null)
            {
                await ReplyAsync($"Leader role {leader} not found.");
                return;
            }
            if (member == null)
            {
                await ReplyAsync($"Member role {member} not found.");
                return;
            }

            await TeamDb.AddTeam(leader.Id, member.Id, Context.Guild.Id);
            await ReplyAsync($"Added Leader role: '{leader.Name}' and Team role: '{member.Name}'");
        }

        [UserPermission(GuildPermission.ManageRoles)]
        [Command("DeleteTeam"), Alias("dt"), Description("Deletes a team.\n**Usage**: `!dt [Leader or Team RoleName]`")]
        [SlashCommand("team-delete", "Delete a team")]
        public async Task DeleteTeam([Remainder] IRole role)
        {
            var team = TeamDb.TeamByMember(role.Id);
            if (team == null)
                team = TeamDb.TeamByLeader(role.Id);

            if (team == null)
            {
                await ReplyAsync($"`{role.Name}` is not a team. Create a team using `!newteam`");
                return;
            }

            await TeamDb.DeleteTeam(team);
            await ReplyAsync($"Deleted team {role.Name}. The weak fall, big deal.");
        }

        [Command("TeamList"), Alias("tl", "teams"), Description("Lists all teams.\n**Usage**: `!tl`")]
        [SlashCommand("teams", "Lists all teams")]
        public async Task TeamList()
        {
            var eb = await RoleUtil.TeamListEmbed(TeamDb.Teams(Context.Guild.Id), Context.Guild);
            await ReplyAsync("", false, eb.Build());
        }

        [Command("Team"), Description("Info about a team.\n**Usage**: `!team [team_role_name]`")]
        [SlashCommand("team", "See team stats")]
        public async Task Team([Remainder] string teamName = "")
        {
            var role = await SelectRole(TeamDb.Teams(Context.Guild.Id).Select(x => x.MemberRoleId), teamName);

            if(role == null)
            {
                await ReplyAsync($"`{teamName}` is not a role.");
                return;
            }

            var team = TeamDb.TeamByMember(role.Id);
            if (team == null)
                team = TeamDb.TeamByLeader(role.Id);

            if(team == null)
            {
                await ReplyAsync($"`{role.Name}` is not a team. Create a team using `!newteam`");
                return;
            }

            var teamRole = Context.Guild.GetRole(team.MemberRoleId);
            var leaderRole = Context.Guild.GetRole(team.LeaderRoleId);

            var eb = RoleUtil.TeamEmbed(teamRole, leaderRole);
            await ReplyAsync("", false, eb.Build());
        }
    }
}
