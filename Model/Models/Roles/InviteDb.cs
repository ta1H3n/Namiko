using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class InviteDb
    {
        public static async Task NewInvite(ulong teamId, ulong userId)
        {
            using var DbContext = new NamikoDbContext();
            DbContext.Add(new Invite { TeamId = teamId, UserId = userId, Date = DateTime.Now });
            await DbContext.SaveChangesAsync();
        }
        public static async Task DeleteInvite(ulong teamId, ulong userId)
        {
            using var DbContext = new NamikoDbContext();
            var invite = DbContext.Invites.Where(x => x.TeamId == teamId && x.UserId == userId).FirstOrDefault();
            DbContext.Remove(invite);
            await DbContext.SaveChangesAsync();
        }
        public static Boolean IsInvited(ulong teamId, ulong userId)
        {
            using var DbContext = new NamikoDbContext();
            return DbContext.Invites.Any(x => x.TeamId == teamId && x.UserId == userId);
        }
        public static async Task<int> DeleteOlder(DateTime date)
        {
            using var db = new NamikoDbContext();
            db.RemoveRange(db.Invites.Where(x => x.Date < date));
            return await db.SaveChangesAsync();
        }
    }
}
