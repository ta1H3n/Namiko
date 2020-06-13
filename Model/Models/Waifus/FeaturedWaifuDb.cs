using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class FeaturedWaifuDb
    {
        public static async Task SetFeaturedWaifu(ulong userId, Waifu waifu, ulong guildId)
        {
            using var dbContext = new NamikoDbContext();
            var entry = new FeaturedWaifu { UserId = userId, Waifu = waifu, GuildId = guildId };

            dbContext.FeaturedWaifus.RemoveRange(dbContext.FeaturedWaifus.Where(x => x.UserId == userId & x.GuildId == guildId));
            dbContext.FeaturedWaifus.Update(entry);
            await dbContext.SaveChangesAsync();
        }
        public static Waifu GetFeaturedWaifu(ulong userId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            var waifu = db.FeaturedWaifus.Where(x => x.UserId == userId && x.GuildId == guildId).OrderByDescending(x => x.Id).Select(x => x.Waifu).FirstOrDefault();
            if (waifu == null)
                waifu = db.UserInventories.Where(x => x.UserId == userId && x.GuildId == guildId).OrderByDescending(x => x.Id).Select(x => x.Waifu).FirstOrDefault();
            return waifu;
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.FeaturedWaifus.RemoveRange(db.FeaturedWaifus.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
        public static async Task Delete(ulong userId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.FeaturedWaifus.RemoveRange(db.FeaturedWaifus.Where(x => x.UserId == userId && x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
    }
}
