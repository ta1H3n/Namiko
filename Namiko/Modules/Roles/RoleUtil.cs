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
            try {
                eb.AddField("Teams :shield:", teamstr, true);
                eb.AddField("Leaders :crown:", leaderstr, true);
            } catch
            {
                eb.WithDescription("*~ There are no teams ~*");
            }
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

            List<SocketGuildUser> leaderUsers = new List<SocketGuildUser>(leaderRole.Members);
            List<SocketGuildUser> teamUsers = new List<SocketGuildUser>(teamRole.Members);
            teamUsers.RemoveAll(x => leaderUsers.Any(y => y.Equals(x)));

            int totalToasties = 0;
            int totalWaifus = 0;
            int totalWaifuValue = 0;

            string memberList = "";
            foreach(var x in teamUsers)
            {
                memberList += $"\n`{x.Username}`";

                totalToasties += ToastieDb.GetToasties(x.Id, x.Guild.Id);
                var waifus = UserInventoryDb.GetWaifus(x.Id, x.Guild.Id);
                totalWaifus += waifus.Count;
                totalWaifuValue += WaifuUtil.WaifuValue(waifus);
            }

            string leaderList = "";
            foreach(var x in leaderUsers)
            {
                leaderList += $"\n`{x.Username}`";

                totalToasties += ToastieDb.GetToasties(x.Id, x.Guild.Id);
                var waifus = UserInventoryDb.GetWaifus(x.Id, x.Guild.Id);
                totalWaifus += waifus.Count;
                totalWaifuValue += WaifuUtil.WaifuValue(waifus);
            }

            eb.WithDescription($"Total Toasties: {totalToasties.ToString("n0")} <:toastie3:454441133876183060>\n" +
                $"Total Waifus: {totalWaifus.ToString("n0")} :two_hearts:\n" +
                $"Waifu Value: {totalWaifuValue.ToString("n0")} <:toastie3:454441133876183060>");
            eb.AddField($":shield: Members - {teamUsers.Count}", memberList == "" ? "-" : memberList, true);
            eb.AddField($":crown: Leaders - {leaderUsers.Count}", leaderList == "" ? "-" : leaderList, true);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithTitle(teamRole.Name);
            return eb;
        }

        public static SocketRole GetLeaderRole(SocketGuild guild, IUser user)
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
        public static SocketRole GetMemberRole(SocketGuild guild, IUser user)
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
            var role = roles.FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                role = roles.Count(x => x.Name.Contains(roleName, StringComparison.OrdinalIgnoreCase)) == 1 ? 
                    roles.First(x => x.Name.Contains(roleName, StringComparison.OrdinalIgnoreCase)) : null;
            }
            return role;
        }
        public static bool IsTeamed(SocketGuild guild, IUser user)
        {
            if (GetMemberRole(guild, user) != null)
                return true;
            if (GetLeaderRole(guild, user) != null)
                return true;
            return false;
        }
        public static bool HasRole(SocketGuildUser user, SocketRole role)
        {
            return user.Roles.Contains(role);
        }
    }
}
