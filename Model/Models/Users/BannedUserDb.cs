using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class BanDb
    {
        public static async Task AddBan(BannedUser bannedUser)
        {
            using var db = new NamikoDbContext();
            db.Add(bannedUser);
            await db.SaveChangesAsync();
        }
        public static async Task EndBan(ulong userId, ulong serverId)
        {
            using var db = new NamikoDbContext();
            var bans = db.BannedUsers.Where(x => x.UserId == userId && x.ServerId == serverId);
            if (!bans.Any())
                return;

            foreach (var ban in bans)
            {
                ban.Active = false;
                db.BannedUsers.Update(ban);
            }
            await db.SaveChangesAsync();
        }
        public static async Task<List<BannedUser>> ToUnban(bool active = true)
        {
            using var db = new NamikoDbContext();
            return await db.BannedUsers.Where(x => x.Active == active && x.DateBanEnd < System.DateTime.Now).ToListAsync();
        }
    }
}
