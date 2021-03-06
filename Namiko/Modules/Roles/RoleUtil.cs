﻿using Discord;
using Discord.WebSocket;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    public static class RoleUtil
    {
        public static async Task<EmbedBuilder> TeamListEmbed(List<Team> teams, SocketGuild guild)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());

            string teamstr = "";
            string leaderstr = "";
            int count = 1;
            foreach(var x in teams)
            {
                var teamRole = guild.GetRole(x.MemberRoleId);
                var leaderRole = guild.GetRole(x.LeaderRoleId);

                if (teamRole != null && leaderRole != null)
                {
                    teamstr += $"\n`#{count}` {teamRole.Mention}";
                    leaderstr += $"\n`#{count}` {leaderRole.Mention}";
                    count++;
                }
                else
                {
                    await TeamDb.DeleteTeam(x);
                }
            }

            try {
                eb.AddField("Teams :shield:", teamstr, true);
                eb.AddField("Leaders :crown:", leaderstr, true);
                eb.WithColor(Color.DarkRed);
            } catch
            {
                eb.WithDescription("*~ There are no teams ~*");
            }
            eb.WithTitle("Teams :european_castle:");
            return eb;
        }
        public static EmbedBuilder PublicRoleListEmbed(List<SocketRole> roles)
        {
            var eb = new EmbedBuilder();

            string rolestr = "";
            foreach (var x in roles)
            {
                if (x != null)
                    rolestr += $"\n{x.Mention}";
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

                totalToasties += BalanceDb.GetToasties(x.Id, x.Guild.Id);
                var waifus = UserInventoryDb.GetWaifus(x.Id, x.Guild.Id);
                totalWaifus += waifus.Count;
                totalWaifuValue += WaifuUtil.WaifuValue(waifus);
            }

            string leaderList = "";
            foreach(var x in leaderUsers)
            {
                leaderList += $"\n`{x.Username}`";

                totalToasties += BalanceDb.GetToasties(x.Id, x.Guild.Id);
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
