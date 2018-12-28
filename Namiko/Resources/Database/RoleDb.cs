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
        public static async Task Add(ulong roleId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Add(new PublicRole { RoleId = roleId });
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task Delete(ulong roleId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var publicRole = DbContext.PublicRoles.Where(x => x.RoleId == roleId).First();
                DbContext.PublicRoles.Remove(publicRole);
                await DbContext.SaveChangesAsync();
            }
        }
        public static List<PublicRole> GetAll()
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.PublicRoles.ToList();
            }
        }
    }

    public static class TeamDb
    {
        public static Team TeamByLeader(ulong roleId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Teams.Where(x => x.LeaderRoleId == roleId).First();
            }
        }
        public static Team TeamByMember(ulong roleId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Teams.Where(x => x.MemberRoleId == roleId).First();
            }
        }
        public static async Task AddTeam(ulong leaderId, ulong memberId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Add(new Team { LeaderRoleId = leaderId, MemberRoleId = memberId });
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteTeam(ulong leaderId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var team = DbContext.Teams.Where(x => x.LeaderRoleId == leaderId).First();
                DbContext.Teams.Remove(team);
                await DbContext.SaveChangesAsync();
            }
        }
        public static List<Team> Teams()
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Teams.ToList();
            }
        }
    }

    public static class InviteDb
    {
        public static async Task NewInvite(ulong teamId, ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Add(new Invite { TeamId = teamId, UserId = userId });
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteInvite(ulong teamId, ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Invites.Where(x => ((x.TeamId == teamId) && (x.UserId == userId))).Count() < 1)
                    return;

                var invite = DbContext.Invites.Where(x => ((x.TeamId == teamId) && (x.UserId == userId))).First();
                DbContext.Remove(invite);
                await DbContext.SaveChangesAsync();
            }
        }
        public static Boolean IsInvited(ulong teamId, ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Invites.Where(x => ((x.TeamId == teamId) && (x.UserId == userId))).Count() < 1)
                    return false;
                return true;
            }
        }
    }
}
