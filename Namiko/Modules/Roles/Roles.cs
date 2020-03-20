using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    public class Roles : InteractiveBase<ShardedCommandContext>
    {
        [Command("Role"), Alias("r", "iam"), Summary("Adds or removes a public role from the user.\n**Usage**: `!r [name]`"), CustomBotPermission(GuildPermission.ManageRoles)]
        public async Task Role([Remainder] string name)
        {
            var role = RoleUtil.GetRoleByName(Context.Guild, name);
            var guildUser = Context.User as SocketGuildUser;
            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"There's no role called `{name}` :bangbang:");
                return;
            }
            if (!PublicRoleDb.IsPublic(role.Id))
            {
                await Context.Channel.SendMessageAsync($"`{role.Name}` isn't a public role. Trying something funny you?");
                return;
            }

            if (guildUser.Roles.Contains(role))
            {
                try
                {
                    await guildUser.RemoveRoleAsync(role);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync($"I tried removing a role that is above my own role. *Coughs blood.* Discord wouldn't let me...");
                    return;
                }
                await Context.Channel.SendMessageAsync($"Removed the `{role.Name}` role from you. Don't come back, BAAAAAAAAAAAAAAKA!");
            }
            else
            {
                try
                {
                    await guildUser.AddRoleAsync(role);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync($"I tried giving a role that is above my own role. *Coughs blood.* Discord wouldn't let me...");
                    return;
                }
                await Context.Channel.SendMessageAsync($"Gave you the `{role.Name}` role! Welcome to the club! o7");
            }
        }

        [Command("SetPublicRole"), Alias("spr"), Summary("Sets or unsets a role as a public role.\n**Usage**: `!spr [name]`"), CustomUserPermission(GuildPermission.ManageRoles)]
        public async Task NewRole([Remainder] string name)
        {
            var role = RoleUtil.GetRoleByName(Context.Guild, name);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"There's no role called `{name}` :bangbang:");
                return;
            }
            if (!PublicRoleDb.IsPublic(role.Id))
            {
                await PublicRoleDb.Add(role.Id, Context.Guild.Id);
                await Context.Channel.SendMessageAsync($"`{role.Name}` set as a public role.");
                return;
            }
            else
            {
                await PublicRoleDb.Delete(role.Id);
                await Context.Channel.SendMessageAsync($"`{role.Name}` removed from public roles.");
                return;
            }
        }

        [Command("ClearRole"), Alias("cr"), Summary("Removes all users from a role.\n**Usage**: `cr [name]`"), CustomUserPermission(GuildPermission.ManageRoles), CustomBotPermission(GuildPermission.ManageRoles)]
        public async Task ClearRole([Remainder] string name)
        {
            var role = RoleUtil.GetRoleByName(Context.Guild, name);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"There's no role called `{name}` :bangbang:");
                return;
            }

            var users = Context.Guild.Users.Where(x => x.Roles.Contains(role)).ToList();
            if (users.Count >= 3)
                await Context.Channel.SendMessageAsync($"Clearing **{role.Name}**... This might take a while, Senpai.");

            foreach (var user in users)
            {
                await user.RemoveRoleAsync(role);
            }

            await Context.Channel.SendMessageAsync($"There are now **{users.Count}** less weebs in the `{role.Name}` role. Can I have them?");
        }

        [Command("Invite"), Summary("Invites a user to your team.\n**Usage**: `!inv [user]`")]
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
            await channel.SendMessageAsync($"{user.Mention} You're invited to join **{teamRole.Name}**! Type `{Program.GetPrefix(Context)}join {teamRole.Name}`");

            ISocketMessageChannel ch = (ISocketMessageChannel) Context.Client.GetChannel(ServerDb.GetServer(Context.Guild.Id).TeamLogChannelId);
            await ch.SendMessageAsync($"<:KannaHype:571690048001671238> `{Context.User}` invited `{user}` to **{teamRole.Name}**.");
        }

        [Command("Join"), Summary("Accept an invite to a team.\n**Usage**: `!join [team_name]`"), CustomBotPermission(GuildPermission.ManageRoles)]
        public async Task Join([Remainder] string teamName)
        {
            var role = RoleUtil.GetRoleByName(Context.Guild, teamName);

            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"You're not invited to **{teamName}**! You sure they exist?");
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
                    await Context.Channel.SendMessageAsync($"I tried giving a role that is above my own role. *Coughs blood.* Discord wouldn't let me...");
                    return;
                }
                await InviteDb.DeleteInvite(role.Id, user.Id);
                await Context.Channel.SendMessageAsync($"Congratulations! You joined **{role.Name}**!");
                ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(ServerDb.GetServer(Context.Guild.Id).TeamLogChannelId);
                await ch.SendMessageAsync($"<:TickYes:577838859107303424> `{Context.User}` joined **{role.Name}**.");
                return;
            }
            await Context.Channel.SendMessageAsync($"You're not invited to **{teamName}**! You sure they exist?");
        }

        [Command("LeaveTeam"), Alias("lt"), Summary("Leave your team.\n**Usage**: `!lt`"), CustomBotPermission(GuildPermission.ManageRoles)]
        public async Task Leave([Remainder] string str = "")
        {
            var role = RoleUtil.GetMemberRole(Context.Guild, Context.User);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync("Buuut... You're not in a team.");
                return;
            }

            try
            {
                await Context.Guild.GetUser(Context.User.Id).RemoveRoleAsync(role);
            }
            catch
            {
                await Context.Channel.SendMessageAsync($"I tried removing a role that is above my own role. *Coughs blood.* Discord wouldn't let me...");
                return;
            }

            await Context.Channel.SendMessageAsync($@"Ha! You left **{role.Name}**! Too bad for them ¯\_(ツ)_/¯");

            ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(ServerDb.GetServer(Context.Guild.Id).TeamLogChannelId);
            await ch.SendMessageAsync($"<:TickNo:577838859077943306> `{Context.User}` left **{role.Name}**.");
        }

        [Command("TeamKick"), Alias("tk"), Summary("Kicks a user from your team.\n**Usage**: `!tk [user]`"), CustomBotPermission(GuildPermission.ManageRoles)]
        public async Task TeamKick(IUser user, [Remainder] string str = "")
        {
            var leader = RoleUtil.GetLeaderRole(Context.Guild, Context.User);
            if (leader == null)
            {
                await Context.Channel.SendMessageAsync("You're not a leader! Getting ahead of yourself eh?~");
                return;
            }

            var team = TeamDb.TeamByLeader(leader.Id);
            var userteam = RoleUtil.GetMemberRole(Context.Guild, user);
            var leaderteam = Context.Guild.GetRole(team.MemberRoleId);
            if (!userteam.Equals(leaderteam))
            {
                await Context.Channel.SendMessageAsync($"They're not in your team! If you just want to kick them normaly use `{Program.GetPrefix(Context)}kick`");
                return;
            }

            await Context.Guild.GetUser(user.Id).RemoveRoleAsync(userteam);
            await Context.Channel.SendMessageAsync("Ha! One less to take care of!");

            ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(ServerDb.GetServer(Context.Guild.Id).TeamLogChannelId);
            await ch.SendMessageAsync($":hammer: `{Context.User}` kicked `{user}` from **{userteam.Name}**.");
        }

        [Command("NewTeam"), Alias("nt"), Summary("Creates a new team.\n**Usage**: `!nt [LeaderRoleName] [MemberRoleName]`\n Note: if a role has a space in it's name it has to be put in quotes. \n For example: `!nt \"Role One\" \"Role Two\"`"), CustomUserPermission(GuildPermission.ManageRoles)]
        public async Task NewTeam(string leader, string member)
        {
            var leaderR = RoleUtil.GetRoleByName(Context.Guild, leader);
            var memberR = RoleUtil.GetRoleByName(Context.Guild, member);
            if (leaderR == null)
            {
                await Context.Channel.SendMessageAsync($"Leader role {leader} not found.");
                return;
            }
            if (memberR == null)
            {
                await Context.Channel.SendMessageAsync($"Member role {member} not found.");
                return;
            }

            await TeamDb.AddTeam(leaderR.Id, memberR.Id, Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"Added Leader role: '{leaderR.Name}' and Team role: '{memberR.Name}'");
        }

        [Command("DeleteTeam"), Alias("dt"), Summary("Deletes a team.\n**Usage**: `!dt [Leader or Team RoleName]`"), CustomUserPermission(GuildPermission.ManageRoles)]
        public async Task DeleteTeam([Remainder] string teamName)
        {
            var role = RoleUtil.GetRoleByName(Context.Guild, teamName);

            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"`{teamName}` is not a role.");
                return;
            }

            var team = TeamDb.TeamByMember(role.Id);
            if (team == null)
                team = TeamDb.TeamByLeader(role.Id);

            if (team == null)
            {
                await Context.Channel.SendMessageAsync($"`{role.Name}` is not a team. Create a team using `!newteam`");
                return;
            }

            await TeamDb.DeleteTeam(team);
            await Context.Channel.SendMessageAsync($"Deleted team {role.Name}. The weak fall, big deal.");
        }

        [Command("TeamList"), Alias("tl", "teams"), Summary("Lists all teams.\n**Usage**: `!tl`")]
        public async Task TeamList([Remainder] string str = "")
        {
            var eb = await RoleUtil.TeamListEmbed(TeamDb.Teams(Context.Guild.Id), Context.Guild);
            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("PublicRoleList"), Alias("prl", "roles"), Summary("Lists all public roles.\n**Usage**: `!prl`")]
        public async Task PublicRoleList([Remainder] string str = "")
        {
            var roles = new List<SocketRole>();
            foreach (var x in PublicRoleDb.GetAll(Context.Guild.Id))
            {
                roles.Add(Context.Guild.GetRole(x.RoleId));
            }

            var eb = RoleUtil.PublicRoleListEmbed(roles);
            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("Team"), Summary("Info about a team.\n**Usage**: `!team [team_role_name]`")]
        public async Task Team([Remainder] string teamName)
        {
            var role = RoleUtil.GetRoleByName(Context.Guild, teamName);

            if(role == null)
            {
                await Context.Channel.SendMessageAsync($"`{teamName}` is not a role.");
                return;
            }

            var team = TeamDb.TeamByMember(role.Id);
            if (team == null)
                team = TeamDb.TeamByLeader(role.Id);

            if(team == null)
            {
                await Context.Channel.SendMessageAsync($"`{role.Name}` is not a team. Create a team using `!newteam`");
                return;
            }

            var teamRole = Context.Guild.GetRole(team.MemberRoleId);
            var leaderRole = Context.Guild.GetRole(team.LeaderRoleId);

            var eb = RoleUtil.TeamEmbed(teamRole, leaderRole);
            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }
    }
}
