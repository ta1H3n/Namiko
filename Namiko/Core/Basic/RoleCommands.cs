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

namespace Namiko.Core.Basic
{
    public class Roles : ModuleBase<SocketCommandContext>
    {
        [Command("Role"), Alias("r"), Summary("Adds or removes a public role from the user.\n`!r [name]`")]
        public async Task Role([Remainder] string name)
        {
            var role = GetRole(Context.Guild, name);
            var guildUser = Context.Guild.GetUser(Context.User.Id);
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
                await guildUser.RemoveRoleAsync(role);
                await Context.Channel.SendMessageAsync($"Removed the `{role.Name}` role from you. Don't come back, BAAAAAAAAAAAAAAKA!");
            }
            else
            {
                await guildUser.AddRoleAsync(role);
                await Context.Channel.SendMessageAsync($"Gave you the `{role.Name}` role! Welcome to the club! o7");
            }
        }

        [Command("SetPublicRole"), Alias("spr"), Summary("Sets or unsets a role as a public role.\n`!spr [name]`"), RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task NewRole([Remainder] string name)
        {
            var role = GetRole(Context.Guild, name);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"There's no role called `{name}` :bangbang:");
                return;
            }
            if (!PublicRoleDb.IsPublic(role.Id))
            {
                await PublicRoleDb.Add(role.Id);
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

        [Command("ClearRole"), Alias("cr"), Summary("Removes all users from a role.\n`cr [name]`"), RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task ClearRole([Remainder] string name)
        {
            var role = GetRole(Context.Guild, name);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"There's no role called `{name}` :bangbang:");
                return;
            }

            int count = 0;
            var users = Context.Guild.Users.Where(x => x.Roles.Contains(role)).ToList();
            foreach (var user in users)
            {
                await user.RemoveRoleAsync(role);
                count++;
            }

            await Context.Channel.SendMessageAsync($"There are now {count} less weebs in the `{role.Name}` role. Can I have them?");
        }

        [Command("Invite"), Alias("inv"), Summary("Invites a user to your team.\n`!inv [user]`")]
        public async Task Invite(IUser user = null)
        {
            var channel = Context.Channel;
            var leaderRole = GetLeader(Context.User);

            if (leaderRole == null)
            {
                await channel.SendMessageAsync("You're not a team leader, silly.");
                return;
            }

            if (IsTeamed(user))
            {
                await channel.SendMessageAsync($@"{user.Username} is already in a team, I guess you're too late ¯\_(ツ)_/¯");
                return;
            }

            var team = TeamDb.TeamByLeader(leaderRole.Id);
            var teamRole = Context.Guild.GetRole(team.MemberRoleId);

            if (InviteDb.IsInvited(team.MemberRoleId, user.Id))
            {
                await channel.SendMessageAsync($"You already invited {user.Username}, baaaaaka.");
                return;
            }

            await InviteDb.NewInvite(team.MemberRoleId, user.Id);
            await channel.SendMessageAsync($"{user.Mention} You're invited to join {teamRole.Name}! Type {StaticSettings.prefix}join {teamRole.Name}");

            ISocketMessageChannel ch = (ISocketMessageChannel) Context.Client.GetChannel(StaticSettings.log_channel);
            await ch.SendMessageAsync($"{Context.User.Mention} invited {user} to {teamRole.Name}.");
        }

        [Command("Join"), Summary("Accept an invite to a team.\n`!join [team_name]`")]
        public async Task Join(string teamName)
        {
            var role = GetRole(Context.Guild, teamName);

            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"You're not invited to {teamName}! You sure they exist?");
                return;
            }

            if (InviteDb.IsInvited(role.Id, Context.User.Id))
            {
                var user = Context.Guild.GetUser(Context.User.Id);
                await user.AddRoleAsync(role);
                await InviteDb.DeleteInvite(role.Id, user.Id);
                await Context.Channel.SendMessageAsync($"Congratulations! You joined {role.Name}!");
                ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(StaticSettings.log_channel);
                await ch.SendMessageAsync($"{Context.User.Mention} joined {teamName}.");
                return;
            }
            await Context.Channel.SendMessageAsync($"You're not invited to {teamName}! You sure they exist?");
        }

        [Command("LeaveTeam"), Alias("lt"), Summary("Leave your team.\n`!lt`")]
        public async Task Leave()
        {
            var role = GetMember(Context.User);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync("Buuut... You're not in a team.");
                return;
            }

            await Context.Guild.GetUser(Context.User.Id).RemoveRoleAsync(role);
            await Context.Channel.SendMessageAsync($@"Ha! You left {role.Name}! Too bad for them ¯\_(ツ)_/¯");

            ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(StaticSettings.log_channel);
            await ch.SendMessageAsync($"{Context.User.Mention} left {role.Name}.");
        }

        [Command("TeamKick"), Alias("tk"), Summary("Kicks a user from your team.\n`!tk [user]`")]
        public async Task TeamKick(IUser user)
        {
            var leader = GetLeader(Context.User);
            if (leader == null)
            {
                await Context.Channel.SendMessageAsync("You're not a leader! Getting ahead of yourself eh?~");
                return;
            }

            var team = TeamDb.TeamByLeader(leader.Id);
            var userteam = GetMember(user);
            var leaderteam = Context.Guild.GetRole(team.MemberRoleId);
            if (!userteam.Equals(leaderteam))
            {
                await Context.Channel.SendMessageAsync($"They're not in your team! If you just want to kick them normaly use {StaticSettings.prefix}kick");
                return;
            }

            await Context.Guild.GetUser(user.Id).RemoveRoleAsync(userteam);
            await Context.Channel.SendMessageAsync("Ha! One less to take care of!");

            ISocketMessageChannel ch = (ISocketMessageChannel)Context.Client.GetChannel(StaticSettings.log_channel);
            await ch.SendMessageAsync($"{Context.User.Mention} kicked {user} from {userteam.Name}.");
        }

        [Command("NewTeam"), Alias("nt"), Summary("Creates a new team.\n`!newteam [LeaderRoleName] [MemberRoleName]`"), RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task NewTeam(string leader, string member)
        {
            var leaderR = GetRole(Context.Guild, leader);
            var memberR = GetRole(Context.Guild, member);
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

            await TeamDb.AddTeam(leaderR.Id, memberR.Id);
            await Context.Channel.SendMessageAsync($"Added Leader role: '{leaderR.Name}' and Team role: '{memberR.Name}'");
        }



        private SocketRole GetLeader(IUser user)
        {
            var gUser = Context.Guild.GetUser(user.Id);
            var teams = TeamDb.Teams();

            foreach (Team x in teams)
            {
                var role = Context.Guild.GetRole(x.LeaderRoleId);
                if (HasRole(gUser, role))
                    return role;
            }

            return null;
        }
        private SocketRole GetMember(IUser user)
        {
            var gUser = Context.Guild.GetUser(user.Id);
            var teams = TeamDb.Teams();

            foreach (Team x in teams)
            {
                var role = Context.Guild.GetRole(x.MemberRoleId);
                if (HasRole(gUser, role))
                    return role;
            }

            return null;
        }
        public static SocketRole GetRole(SocketGuild guild, string roleName)
        {
            var roles = guild.Roles;
            foreach (SocketRole x in roles)
            {
                if (x.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase))
                    return x;
            }
            return null;
        }
        private bool IsTeamed(IUser user)
        {
            var leader = GetLeader(user);
            var member = GetMember(user);
            if (leader != null || member != null)
                return true;
            return false;
        }
        public static bool HasRole(SocketGuildUser user, SocketRole role)
        {
            /*var roles = user.Roles;
            foreach(SocketRole userRole in roles)
            {
                if (userRole.Equals(role))
                    return true;
            }

            return false;*/

            return user.Roles.Contains(role);
        }
    }
}
