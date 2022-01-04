using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class WaifuWishlistDb
    {
        public static async Task AddWaifuWish(ulong userId, Waifu waifu, ulong guildId)
        {
            using var db = new NamikoDbContext();
            var entry = new WaifuWish { UserId = userId, Waifu = waifu, GuildId = guildId };

            int cap = 5;
            if (PremiumDb.IsPremium(userId, ProType.ProPlus))
                cap = 12;

            db.WaifuWishlist.Update(entry);
            if (db.WaifuWishlist.Where(x => x.UserId == userId && x.GuildId == guildId).Count() >= cap)
                db.WaifuWishlist.Remove(db.WaifuWishlist.Where(x => x.UserId == userId && x.GuildId == guildId).First());

            await db.SaveChangesAsync();
        }
        public static async Task<List<Waifu>> GetWishlist(ulong userId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            var waifus = await db.WaifuWishlist.Where(x => x.UserId == userId && x.GuildId == guildId).Select(x => x.Waifu).ToListAsync();
            return waifus;
        }
        public static async Task<List<WaifuWish>> GetWishlist(ulong guildId)
        {
            using var db = new NamikoDbContext();
            var wishes = await db.WaifuWishlist.Include(x => x.Waifu).Where(x => x.GuildId == guildId).ToListAsync();
            return wishes;
        }
        public static List<WaifuWish> GetWishlist(ulong guildId, string waifuName)
        {
            using var db = new NamikoDbContext();
            var wishes = db.WaifuWishlist.Include(x => x.Waifu).Where(x => x.GuildId == guildId && x.Waifu.Name == waifuName).ToList();
            return wishes;
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.WaifuWishlist.RemoveRange(db.WaifuWishlist.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
        public static async Task DeleteWaifuWish(ulong userId, Waifu waifu, ulong guildId)
        {
            using var DbContext = new NamikoDbContext();
            var userWaifu = DbContext.WaifuWishlist.Where(x => x.UserId == userId && x.Waifu.Equals(waifu) && x.GuildId == guildId).FirstOrDefault();
            if (userWaifu != null)
            {
                DbContext.WaifuWishlist.Remove(userWaifu);
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task<List<WaifuWish>> GetAllPremiumWishlists(ulong guildId, ProType premium)
        {
            using var db = new NamikoDbContext();
            var users = db.Premiums.Where(x => x.Type == premium).Select(x => x.UserId);
            var wishlists = db.WaifuWishlist.Where(x => x.GuildId == guildId && users.Contains(x.UserId)).Include(x => x.Waifu);
            return await wishlists.ToListAsync();
        }
    }
}
