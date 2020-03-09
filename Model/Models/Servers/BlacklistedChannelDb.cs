using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class BlacklistedChannelDb
    {
        public static async Task UpdateBlacklistedChannel(BlacklistedChannel ch)
        {
            using SqliteDbContext db = new SqliteDbContext();
            var res = db.BlacklistedChannels.Where(x => x.ChannelId == ch.ChannelId).FirstOrDefault();
            if (res == null)
                db.Add(ch);

            else
                db.Update(ch);

            await db.SaveChangesAsync();
        }
        public static async Task DeleteBlacklistedChannel(ulong channelId)
        {
            using SqliteDbContext db = new SqliteDbContext();
            var ch = db.BlacklistedChannels.Where(x => x.ChannelId == channelId).FirstOrDefault();
            if (ch != null)
            {
                db.Remove(ch);
                await db.SaveChangesAsync();
            }
        }
        public static bool IsBlacklisted(ulong channelId)
        {
            using var db = new SqliteDbContext();
            return db.BlacklistedChannels.Any(x => x.ChannelId == channelId);
        }
    }
}
