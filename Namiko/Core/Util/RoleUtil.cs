using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Namiko.Core.Util
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

        public static SocketRole GetLeader(SocketCommandContext Context)
        {
            var teams = TeamDb.Teams();

            foreach (Team x in teams)
            {
                var role = Context.Guild.GetRole(x.LeaderRoleId);
                if (HasRole((SocketGuildUser)Context.User, role))
                    return role;
            }

            return null;
        }
        public static SocketRole GetMember(SocketCommandContext Context)
        {
            var teams = TeamDb.Teams();

            foreach (Team x in teams)
            {
                var role = Context.Guild.GetRole(x.MemberRoleId);
                if (HasRole((SocketGuildUser)Context.User, role))
                    return role;
            }

            return null;
        }
        public static SocketRole GetRoleByName(SocketGuild guild, string roleName)
        {
            var roles = guild.Roles;
            foreach (SocketRole x in roles)
            {
                if (x.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase))
                    return x;
            }
            return null;
        }
        public static bool IsTeamed(SocketCommandContext Context)
        {
            if (GetMember(Context) != null)
                return true;
            if (GetLeader(Context) != null)
                return true;
            return false;
        }
        public static bool HasRole(SocketGuildUser user, SocketRole role)
        {
            return user.Roles.Contains(role);
        }
    }
}
