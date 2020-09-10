using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class UserInventoryDb
    {
        public static async Task<int> AddWaifu(ulong userId, Waifu waifu, ulong guildId)
        {
            using var DbContext = new NamikoDbContext();
            var inv = new UserInventory { UserId = userId, Waifu = waifu, GuildId = guildId, DateBought = DateTime.Now };
            DbContext.Update(inv);
            int res = await DbContext.SaveChangesAsync();

            await WaifuWishlistDb.DeleteWaifuWish(userId, waifu, guildId);
            return res;
        }
        public static List<Waifu> GetWaifus(ulong userId, ulong guildId)
        {
            using var DbContext = new NamikoDbContext();
            return DbContext.UserInventories.Where(x => x.UserId == userId && x.GuildId == guildId).Select(x => x.Waifu).ToList();
        }
        public static async Task<List<Waifu>> GetWaifusAsync(ulong userId, ulong guildId)
        {
            using var DbContext = new NamikoDbContext();
            return await DbContext.UserInventories.Where(x => x.UserId == userId && x.GuildId == guildId).Select(x => x.Waifu).ToListAsync();
        }
        public static async Task DeleteWaifu(ulong userId, Waifu waifu, ulong guildId)
        {
            using var DbContext = new NamikoDbContext();
            var userWaifu = DbContext.UserInventories.Where(x => x.UserId == userId && x.Waifu.Equals(waifu) && x.GuildId == guildId).FirstOrDefault();
            if (userWaifu != null)
            {
                DbContext.UserInventories.Remove(userWaifu);
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.UserInventories.RemoveRange(db.UserInventories.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
        public static async Task CompletelyDeleteWaifu(Waifu waifu)
        {
            using var DbContext = new NamikoDbContext();
            var userWaifu = DbContext.UserInventories.Where(x => x.Waifu.Equals(waifu));
            if (userWaifu == null)
                return;

            DbContext.UserInventories.RemoveRange(userWaifu);
            await DbContext.SaveChangesAsync();
        }
        public static List<ulong> GetOwners(Waifu waifu, ulong guildId)
        {
            using var db = new NamikoDbContext();
            return db.UserInventories.Where(x => x.Waifu.Equals(waifu) && x.GuildId == guildId).Select(x => x.UserId).ToList();
        }
        public static List<UserInventory> GetAllWaifuItems()
        {
            using var db = new NamikoDbContext();
            var items = db.UserInventories.Include(x => x.Waifu).ToList();
            return items;
        }
        public static async Task<List<UserInventory>> GetAllWaifuItems(ulong GuildId)
        {
            using var db = new NamikoDbContext();
            var items = await db.UserInventories.Include(x => x.Waifu).Where(x => x.GuildId == GuildId).ToListAsync();
            return items;
        }
        public static bool OwnsWaifu(ulong userId, Waifu waifu, ulong guildId)
        {
            using var db = new NamikoDbContext();
            return db.UserInventories.Any(x => x.Waifu == waifu && x.UserId == userId && x.GuildId == guildId);
        }
        public static async Task<Dictionary<string, int>> CountWaifus(ulong guildId = 0, string[] filter = null)
        {
            using var db = new NamikoDbContext();

            var query = db.UserInventories.AsQueryable();

            if (guildId != 0)
                query = query.Where(x => x.GuildId == guildId);

            if (filter.DefaultIfEmpty() != null)
            {
                foreach (var word in filter)
                {
                    query = query.Where(x =>
                        x.Waifu.Name.ToUpper().Contains(word.ToUpper()) ||
                        (x.Waifu.LongName == null ? false : x.Waifu.LongName.ToUpper().Contains(word.ToUpper())) ||
                        (x.Waifu.Source == null ? false : x.Waifu.Source.ToUpper().Contains(word.ToUpper())));
                }
            }

            var res = await query
                .GroupBy(x => x.WaifuName)
                .Select(x => new { Name = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ToDictionaryAsync(x => x.Name, x => x.Count);
            return res;
        }
    }
}
