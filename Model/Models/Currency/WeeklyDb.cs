using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class WeeklyDb
    {
        public static Weekly GetWeekly(ulong UserId, ulong GuildId)
        {
            using var db = new SqliteDbContext();
            var weekly = db.Weeklies.FirstOrDefault(x => x.GuildId == GuildId && x.UserId == UserId);
            return weekly;
        }
        public static async Task SetWeekly(Weekly weekly)
        {
            using var db = new SqliteDbContext();
            db.Update(weekly);
            await db.SaveChangesAsync();
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new SqliteDbContext();
            db.Weeklies.RemoveRange(db.Weeklies.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
    }
}
