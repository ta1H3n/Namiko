using Discord;
using Discord.Commands;
using Discord.WebSocket;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Namiko
{
    public static class RoleUtil
    {
        public static EmbedBuilder TeamListEmbed(List<SocketRole> teams, List<SocketRole> leaders)
        {
            var eb = new EmbedBuilder();

            string teamstr = "";
            int count = 1;
            foreach(var x in teams)
            {
                if(x != null)
                {
                    teamstr += $"\n`#{count}` **{x.Name}**";
                    count++;
                }
            }

            string leaderstr = "";
            count = 1;
            foreach (var x in leaders)
            {
                if (x != null)
                {
                    leaderstr += $"\n`#{count}` **{x.Name}**";
                    count++;
                }
            }

            eb.AddField("Teams :shield:", teamstr, true);
            eb.AddField("Leaders :crown:", leaderstr, true);
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
        public static EmbedBuilder PublicRoleListEmbed(List<SocketRole> roles)
        {
            var eb = new EmbedBuilder();

            string rolestr = "";
            foreach (var x in roles)
            {
                if (x != null)
                    rolestr += $"\n**{x.Name}**";
            }

            eb.WithDescription(rolestr);
            eb.WithTitle("Public Roles :star:");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
        public static EmbedBuilder TeamEmbed(SocketRole teamRole, SocketRole leaderRole)
        {
            var eb = new EmbedBuilder();

            string memberList = "";
            foreach(var x in teamRole.Members)
            {
                if(!x.Roles.Any(y => y.Id == leaderRole.Id))
                    memberList += $"\n`{x.Username}`";
            }

            string leaderList = "";
            foreach(var x in leaderRole.Members)
            {
                leaderList += $"\n`{x.Username}`";
            }

            eb.AddField($":shield: Members - {teamRole.Members.Count()}", memberList == "" ? "-" : memberList, true);
            eb.AddField($":crown: Leaders - {leaderRole.Members.Count()}", leaderList == "" ? "-" : leaderList, true);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithTitle(teamRole.Name);
            return eb;
        }

        public static SocketRole GetLeader(SocketGuild guild, IUser user)
        {
            var teams = TeamDb.Teams(guild.Id);

            foreach (Team x in teams)
            {
                var role = guild.GetRole(x.LeaderRoleId);
                if (HasRole((SocketGuildUser)user, role))
                    return role;
            }

            return null;
        }
        public static SocketRole GetMember(SocketGuild guild, IUser user)
        {
            var teams = TeamDb.Teams(guild.Id);

            foreach (Team x in teams)
            {
                var role = guild.GetRole(x.MemberRoleId);
                if (HasRole((SocketGuildUser)user, role))
                    return role;
            }

            return null;
        }
        public static SocketRole GetRoleByName(SocketGuild guild, string roleName)
        {
            var roles = guild.Roles;
            var role = roles.FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
            if (role == null)
            {
                role = roles.Count(x => x.Name.Contains(roleName, StringComparison.InvariantCultureIgnoreCase)) == 1 ? 
                    roles.First(x => x.Name.Contains(roleName, StringComparison.InvariantCultureIgnoreCase)) : null;
            }
            return role;
        }
        public static bool IsTeamed(SocketGuild guild, IUser user)
        {
            if (GetMember(guild, user) != null)
                return true;
            if (GetLeader(guild, user) != null)
                return true;
            return false;
        }
        public static bool HasRole(SocketGuildUser user, SocketRole role)
        {
            return user.Roles.Contains(role);
        }
    }
}
