using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class WaifuShopDb
    {
        public static async Task RemoveItem(ShopWaifu waifu)
        {
            using var db = new SqliteDbContext();
            db.ShopWaifus.Remove(waifu);
            await db.SaveChangesAsync();
        }
        public static async Task<WaifuShop> AddShop(WaifuShop shop, bool overwrite)
        {
            using var db = new SqliteDbContext();
            var items = shop.ShopWaifus ?? new List<ShopWaifu>();

            if (overwrite)
            {
                var old = db.WaifuShops.Where(x => x.GuildId == shop.GuildId && x.Type == shop.Type).Include(x => x.ShopWaifus).ToList();
                db.WaifuShops.RemoveRange(old);
                foreach (var oldshop in old)
                {
                    db.ShopWaifus.RemoveRange(oldshop.ShopWaifus);
                }

                shop.ShopWaifus = null;
                db.WaifuShops.Add(shop);
                await db.SaveChangesAsync();
            }

            foreach (var item in items)
            {
                item.WaifuShop = shop;
                await UpdateShopWaifu(item);
            }

            return shop;
        }
        public static async Task<WaifuShop> GetWaifuShop(ulong guildId, ShopType type)
        {
            using var db = new SqliteDbContext();
            var shop = await db.WaifuShops.OrderByDescending(x => x.Id).Include(x => x.ShopWaifus).ThenInclude(x => x.Waifu).FirstOrDefaultAsync(x => x.GuildId == guildId && x.Type == type);
            return shop;
        }
        public static async Task UpdateShopWaifu(ShopWaifu shopWaifu)
        {
            using var dbContext = new SqliteDbContext();
            dbContext.ShopWaifus.Update(shopWaifu);
            await dbContext.SaveChangesAsync();
        }
        public static async Task CompletelyDeleteWaifu(Waifu waifu)
        {
            using var DbContext = new SqliteDbContext();
            var userWaifu = DbContext.ShopWaifus.Where(x => x.Waifu.Equals(waifu));
            if (userWaifu == null)
                return;

            DbContext.ShopWaifus.RemoveRange(userWaifu);
            await DbContext.SaveChangesAsync();
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new SqliteDbContext();
            var shops = db.WaifuShops.Where(x => x.GuildId == guildId);
            db.ShopWaifus.RemoveRange(db.ShopWaifus.Where(x => shops.Any(y => y.Id == x.WaifuShop.Id)));
            db.WaifuShops.RemoveRange(shops);

            await db.SaveChangesAsync();
        }
    }
}
