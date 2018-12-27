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
