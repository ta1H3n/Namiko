using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class DailyDb
    {
        public static Daily GetDaily(ulong UserId, ulong GuildId)
        {
            using var DbContext = new NamikoDbContext();
            return DbContext.Dailies.Where(x => x.UserId == UserId && x.GuildId == GuildId).FirstOrDefault();
        }
        public static async Task SetDaily(Daily Daily)
        {
            using var DbContext = new NamikoDbContext();
            DbContext.Update(Daily);
            await DbContext.SaveChangesAsync();
        }
        public static async Task<List<LeaderboardEntryId>> GetLeaderboard(ulong GuildId)
        {
            using var db = new NamikoDbContext();
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return await db.Dailies
                .Where(x => (x.GuildId == GuildId) && ((x.Date + 48 * 60 * 60 * 1000) > now))
                .Select(x => new LeaderboardEntryId
                {
                    Id = x.UserId,
                    Count = x.Streak
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }
        public static async Task<int> GetHighest(ulong GuildId)
        {
            using var db = new NamikoDbContext();
            try
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                return await db.Dailies.Where(x => x.GuildId == GuildId && ((x.Date + 48 * 60 * 60 * 1000) > now)).MaxAsync(x => x.Streak);
            }
            catch { return 1; }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.Dailies.RemoveRange(db.Dailies.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
    }
}
