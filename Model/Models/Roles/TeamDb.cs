using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class TeamDb
    {
        public static Team TeamByLeader(ulong roleId)
        {
            using var DbContext = new SqliteDbContext();
            return DbContext.Teams.Where(x => x.LeaderRoleId == roleId).FirstOrDefault();
        }
        public static Team TeamByMember(ulong roleId)
        {
            using var DbContext = new SqliteDbContext();
            return DbContext.Teams.Where(x => x.MemberRoleId == roleId).FirstOrDefault();
        }
        public static async Task AddTeam(ulong leaderId, ulong memberId, ulong guildId)
        {
            using var DbContext = new SqliteDbContext();
            DbContext.Add(new Team { LeaderRoleId = leaderId, MemberRoleId = memberId, GuildId = guildId });
            await DbContext.SaveChangesAsync();
        }
        public static async Task DeleteTeam(Team team)
        {
            using var DbContext = new SqliteDbContext();
            DbContext.Teams.Remove(team);
            await DbContext.SaveChangesAsync();
        }

        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new SqliteDbContext();
            db.Teams.RemoveRange(db.Teams.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
        public static List<Team> Teams(ulong guildId)
        {
            using var DbContext = new SqliteDbContext();
            return DbContext.Teams.Where(x => x.GuildId == guildId).ToList();
        }
    }
}
