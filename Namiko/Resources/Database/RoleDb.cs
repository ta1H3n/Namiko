using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Namiko.Resources.Datatypes;
using Namiko.Resources.Database;
using System.Threading.Tasks;
namespace Namiko.Resources.Database
{
    class PublicRoleDb
    {
        public static Boolean IsPublic(ulong roleId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.PublicRoles.Where(x => x.RoleId == roleId).Count() > 0;
            }
        }
        public static async Task Add(ulong roleId, ulong guildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Add(new PublicRole { RoleId = roleId, GuildId = guildId });
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task Delete(ulong roleId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var publicRole = DbContext.PublicRoles.Where(x => x.RoleId == roleId).FirstOrDefault();
                DbContext.PublicRoles.Remove(publicRole);
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.PublicRoles.RemoveRange(db.PublicRoles.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
        public static List<PublicRole> GetAll(ulong guildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.PublicRoles.Where(x => x.GuildId == guildId).ToList();
            }
        }
    }

    public static class TeamDb
    {
        public static Team TeamByLeader(ulong roleId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Teams.Where(x => x.LeaderRoleId == roleId).FirstOrDefault();
            }
        }
        public static Team TeamByMember(ulong roleId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Teams.Where(x => x.MemberRoleId == roleId).FirstOrDefault();
            }
        }
        public static async Task AddTeam(ulong leaderId, ulong memberId, ulong guildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Add(new Team { LeaderRoleId = leaderId, MemberRoleId = memberId, GuildId = guildId });
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteTeam(Team team)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Teams.Remove(team);
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Teams.RemoveRange(db.Teams.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
        public static List<Team> Teams(ulong guildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Teams.Where(x => x.GuildId == guildId).ToList();
            }
        }
    }

    public static class InviteDb
    {
        public static async Task NewInvite(ulong teamId, ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Add(new Invite { TeamId = teamId, UserId = userId, Date = DateTime.Now });
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteInvite(ulong teamId, ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var invite = DbContext.Invites.Where(x => x.TeamId == teamId && x.UserId == userId).FirstOrDefault();
                DbContext.Remove(invite);
                await DbContext.SaveChangesAsync();
            }
        }
        public static Boolean IsInvited(ulong teamId, ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Invites.Any(x => x.TeamId == teamId && x.UserId == userId);
            }
        }

        public static async Task<int> DeleteOlder(DateTime date)
        {
            using (var db = new SqliteDbContext())
            {
                db.RemoveRange(db.Invites.Where(x => x.Date.CompareTo(date) < 0));
                return await db.SaveChangesAsync();
            }
        }
    }
}
