using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class WaifuShopDb
    {
        public static async Task<WaifuShop> AddShop(WaifuShop shop)
        {
            using var db = new NamikoDbContext();
            var waifus = new Dictionary<string, Waifu>();

            // Detach waifus from list so they could be added to database
            foreach (var x in shop.ShopWaifus)
            {
                x.WaifuName = x.Waifu.Name;
                waifus.Add(x.Waifu.Name, x.Waifu);
                x.Waifu = null;
            }

            db.WaifuShops.Add(shop);
            await db.SaveChangesAsync();

            // Re-attach after submitting on the database and return
            foreach (var x in shop.ShopWaifus)
            {
                x.Waifu = waifus.GetValueOrDefault(x.WaifuName);
            }
            return shop;
        }
        public static async Task DeleteShop(ulong guildId, ShopType type)
        {
            using var db = new NamikoDbContext();
            db.WaifuShops.RemoveRange(db.WaifuShops.Where(x => x.GuildId == guildId && x.Type == type));
            await db.SaveChangesAsync();
        }
        public static async Task<WaifuShop> GetWaifuShop(ulong guildId, ShopType type)
        {
            using var db = new NamikoDbContext();
            var shop = await db.WaifuShops
                .OrderByDescending(x => x.Id)
                .Include(x => x.ShopWaifus)
                .ThenInclude(x => x.Waifu)
                .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Type == type);
            return shop;
        }
        public static async Task<ShopWaifu> GetWaifuFromShop(ulong guildId, string name)
        {
            using var db = new NamikoDbContext();
            var shop = await db.WaifuShops
                .Include(x => x.ShopWaifus)
                    .ThenInclude(x => x.Waifu)
                .Where(x => x.GuildId == guildId)
                .SelectMany(x => x.ShopWaifus)
                .FirstOrDefaultAsync(x => x.WaifuName == name);
            return shop;
        }
        public static async Task<List<Waifu>> GetWaifus(ulong guildId)
        {
            using var db = new NamikoDbContext();
            var waifus = await db.WaifuShops
                .Where(x => x.GuildId == guildId)
                .OrderByDescending(x => x.Id)
                .SelectMany(x => x.ShopWaifus.Select(x => x.Waifu))
                .ToListAsync();
            return waifus;
        }
        public static async Task<List<ShopWaifu>> GetAllShopWaifus(ulong guildId)
        {
            using var db = new NamikoDbContext();
            var waifus = await db.WaifuShops
                .Where(x => x.GuildId == guildId)
                .OrderByDescending(x => x.Id)
                .SelectMany(x => x.ShopWaifus)
                .Include(x => x.Waifu)
                .ToListAsync();
            return waifus;
        }
        public static async Task AddItem(ShopWaifu shopWaifu)
        {
            using var dbContext = new NamikoDbContext();
            dbContext.ShopWaifus.Add(shopWaifu);
            await dbContext.SaveChangesAsync();
        }
        public static async Task UpdateItem(ShopWaifu shopWaifu)
        {
            using var dbContext = new NamikoDbContext();
            dbContext.ShopWaifus.Update(shopWaifu);
            await dbContext.SaveChangesAsync();
        }
        public static async Task RemoveItem(ShopWaifu shopWaifu)
        {
            using var db = new NamikoDbContext();
            db.ShopWaifus.Remove(shopWaifu);
            await db.SaveChangesAsync();
        }
        public static async Task CompletelyDeleteWaifu(Waifu waifu)
        {
            using var DbContext = new NamikoDbContext();
            var userWaifu = DbContext.ShopWaifus.Where(x => x.Waifu.Equals(waifu));
            if (userWaifu == null)
                return;

            DbContext.ShopWaifus.RemoveRange(userWaifu);
            await DbContext.SaveChangesAsync();
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            var shops = db.WaifuShops.Where(x => x.GuildId == guildId);
            db.ShopWaifus.RemoveRange(db.ShopWaifus.Where(x => shops.Any(y => y.Id == x.WaifuShop.Id)));
            db.WaifuShops.RemoveRange(shops);

            await db.SaveChangesAsync();
        }
    }
}
