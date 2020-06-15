using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class BlacklistedChannelDb
    {
        public static HashSet<ulong> BlacklistedChannelIds;

        static BlacklistedChannelDb()
        {
            BlacklistedChannelIds = GetAll();
        }

        public static HashSet<ulong> GetAll()
        {
            using NamikoDbContext db = new NamikoDbContext();
            return db.BlacklistedChannels.Select(x => x.ChannelId).ToHashSet();
        }

        public static async Task UpdateBlacklistedChannel(BlacklistedChannel ch)
        {
            using NamikoDbContext db = new NamikoDbContext();
            var res = db.BlacklistedChannels.Where(x => x.ChannelId == ch.ChannelId).FirstOrDefault();
            if (res == null)
                db.Add(ch);

            else
                db.Update(ch);

            BlacklistedChannelIds.Add(ch.ChannelId);
            await db.SaveChangesAsync();
        }
        public static async Task DeleteBlacklistedChannel(ulong channelId)
        {
            using NamikoDbContext db = new NamikoDbContext();
            var ch = db.BlacklistedChannels.Where(x => x.ChannelId == channelId).FirstOrDefault();
            if (ch != null)
            {
                db.Remove(ch);
                BlacklistedChannelIds.Remove(ch.ChannelId);
                await db.SaveChangesAsync();
            }
        }
        public static bool IsBlacklisted(ulong channelId)
        {
            if (BlacklistedChannelIds == null)
            {
                using var db = new NamikoDbContext();
                BlacklistedChannelIds = GetAll();
            }

            return BlacklistedChannelIds.Contains(channelId);
        }
    }
}
