using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;


using System.Threading.Tasks;

namespace Namiko
{
    public class WaifuDb
    {
        public static Waifu GetWaifu(string name)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Waifus.FirstOrDefault(x => x.Name == name);
            }
        }
        public static async Task<int> AddWaifu(Waifu waifu)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Add(waifu);
                return await DbContext.SaveChangesAsync();
            }
        }
        public static async Task<int> UpdateWaifu(Waifu waifu)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Update(waifu);
                return await DbContext.SaveChangesAsync();
            }
        }
        public static List<Waifu> SearchWaifus(string query, bool primaryName = false, IEnumerable<Waifu> from = null)
        {
            using (var DbContext = new SqliteDbContext())
            {
                List<Waifu> waifus = new List<Waifu>();

                var waifuQuery = from == null ? DbContext.Waifus : from.AsQueryable();

                if (primaryName)
                {
                    Waifu waifu = waifuQuery.Where(x => x.Name.Equals(query, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (waifu != null)
                    {
                        waifus.Add(waifu);
                        return waifus;
                    }
                }

                var words = query.Split(' ');

                foreach (var word in words)
                {
                    waifuQuery = waifuQuery.Where(x =>
                        (x.Name.Contains(word, StringComparison.InvariantCultureIgnoreCase)) ||
                        (x.LongName == null ? false : x.LongName.Contains(word, StringComparison.InvariantCultureIgnoreCase)) ||
                        (x.Source == null ? false : x.Source.Contains(word, StringComparison.InvariantCultureIgnoreCase)));
                }

                waifus = waifuQuery.ToList();

                return waifus;
            }

        }
        public static List<Waifu> AllWaifus()
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Waifus.ToList();
            }

        }
        public static async Task<int> DeleteWaifu(string name)
        {
            using (var DbContext = new SqliteDbContext())
            {
                Waifu waifu = DbContext.Waifus.Where(x => x.Name == name).FirstOrDefault();
                if (waifu == null)
                    return 0;

                await UserInventoryDb.CompletelyDeleteWaifu(waifu);
                await WaifuShopDb.CompletelyDeleteWaifu(waifu);
                DbContext.Waifus.Remove(waifu);
                return await DbContext.SaveChangesAsync();
            }
        }
        public static List<Waifu> GetWaifusByTier(int tier)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Waifus.Where(x => x.Tier == tier).ToList();
            }
        }
        public static List<Waifu> RandomWaifus(int tier, int amount, List<string> includeSource = null, List<string> excludeSource = null)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Waifus.Where(x => x.Tier == tier && 
                (includeSource == null || includeSource.Contains(x.Source)) && 
                (excludeSource == null || !excludeSource.Contains(x.Source)))
                    .OrderBy(r => Guid.NewGuid())
                    .Take(amount).ToList();
            }
        }
        public static async Task<int> RenameWaifu(string oldName, string newName)
        {
            using (var db = new SqliteDbContext())
            {
                var inv = db.UserInventories.Where(x => x.Waifu.Name == oldName);
                var wish = db.WaifuWishlist.Where(x => x.Waifu.Name == oldName);
                var store = db.ShopWaifus.Where(x => x.Waifu.Name == oldName);
                var feat = db.FeaturedWaifus.Where(x => x.Waifu.Name == oldName);
                var actual = db.Waifus.Where(x => x.Name == oldName);
                
                var invL = inv.ToList();
                var wishL = wish.ToList();
                var storeL = store.ToList();
                var featL = feat.ToList();
                var actualL = actual.ToList();

                db.UserInventories.RemoveRange(inv);
                db.WaifuWishlist.RemoveRange(wish);
                db.ShopWaifus.RemoveRange(store);
                db.FeaturedWaifus.RemoveRange(feat);
                db.Waifus.RemoveRange(actual);
                
                await db.SaveChangesAsync();

                foreach (var x in invL)
                    x.Waifu.Name = newName;
                foreach (var x in wishL)
                    x.Waifu.Name = newName;
                foreach (var x in storeL)
                    x.Waifu.Name = newName;
                foreach (var x in featL)
                    x.Waifu.Name = newName;
                foreach(var x in actualL)
                    x.Name = newName;

                db.Waifus.AddRange(actualL);

                int res = await db.SaveChangesAsync();

                db.UserInventories.AddRange(invL);
                db.WaifuWishlist.AddRange(wishL);
                db.ShopWaifus.AddRange(storeL);
                db.FeaturedWaifus.AddRange(featL);

                return res + await db.SaveChangesAsync();
            }
        }
    }

    
    public class UserInventoryDb
    {
        public static async Task<int> AddWaifu(ulong userId, Waifu waifu, ulong guildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var inv = new UserInventory{ UserId = userId, Waifu = waifu, GuildId = guildId, DateBought = DateTime.Now };
                DbContext.Update(inv);
                int res = await DbContext.SaveChangesAsync();

                await WaifuWishlistDb.DeleteWaifuWish(userId, waifu, guildId);
                return res;
            }
        }
        public static List<Waifu> GetWaifus(ulong userId, ulong guildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.UserInventories.Where(x => x.UserId == userId && x.GuildId == guildId).Select(x => x.Waifu).ToList();
            }

        }
        public static async Task DeleteWaifu(ulong userId, Waifu waifu, ulong guildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var userWaifu = DbContext.UserInventories.Where(x => x.UserId == userId && x.Waifu.Equals(waifu) && x.GuildId == guildId).FirstOrDefault();
                if (userWaifu != null)
                {
                    DbContext.UserInventories.Remove(userWaifu);
                    await DbContext.SaveChangesAsync();
                }
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.UserInventories.RemoveRange(db.UserInventories.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
        public static async Task CompletelyDeleteWaifu(Waifu waifu)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var userWaifu = DbContext.UserInventories.Where(x => x.Waifu.Equals(waifu));
                if (userWaifu == null)
                    return;

                DbContext.UserInventories.RemoveRange(userWaifu);
                await DbContext.SaveChangesAsync();
            }
        }
        public static List<ulong> GetOwners(Waifu waifu, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.UserInventories.Where(x => x.Waifu.Equals(waifu) && x.GuildId == guildId).Select(x => x.UserId).ToList();
            }
        }
        public static List<UserInventory> GetAllWaifuItems()
        {
            using (var db = new SqliteDbContext())
            {
              //  var items = db.UserInventories;
              //  foreach (var x in items)
              //      x.Waifu = x.Waifu;
              //  return items.ToList();

                var items = db.UserInventories.Include(x => x.Waifu).ToList();
                return items;
            }
        }
        public static List<UserInventory> GetAllWaifuItems(ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                //  var items = db.UserInventories;
                //  foreach (var x in items)
                //      x.Waifu = x.Waifu;
                //  return items.ToList();
                
                var items = db.UserInventories.Include(x => x.Waifu).Where(x => x.GuildId == GuildId).ToList();
                return items;
            }
        }
        public static bool OwnsWaifu(ulong userId, Waifu waifu, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.UserInventories.Any(x => x.Waifu == waifu && x.UserId == userId && x.GuildId == guildId);
            }
        }
    }

    
    public class WaifuShopDb
    {
        public static async Task RemoveItem(ShopWaifu waifu)
        {
            using (var db = new SqliteDbContext())
            {
                db.ShopWaifus.Remove(waifu);
                await db.SaveChangesAsync();
            }
        }
        public static async Task<WaifuShop> AddShop(WaifuShop shop)
        {
            using (var db = new SqliteDbContext())
            {
                var old = db.WaifuShops.Where(x => x.GuildId == shop.GuildId && x.Type == shop.Type).Include(x => x.ShopWaifus).ToList();
                db.WaifuShops.RemoveRange(old);
                foreach(var oldshop in old)
                {
                    db.ShopWaifus.RemoveRange(oldshop.ShopWaifus);
                }

                var items = shop.ShopWaifus ?? new List<ShopWaifu>();

                shop.ShopWaifus = null;
                db.WaifuShops.Add(shop);
                await db.SaveChangesAsync();

                foreach (var item in items)
                {
                    item.WaifuShop = shop;
                    await UpdateShopWaifu(item);
                }

                return shop;
            }
        }
        public static WaifuShop GetWaifuShop(ulong guildId, ShopType type)
        {
            using (var db = new SqliteDbContext())
            {
                var shop = db.WaifuShops.Include(x => x.ShopWaifus).ThenInclude(x => x.Waifu).LastOrDefault(x => x.GuildId == guildId && x.Type == type);
                return shop;
            }
        }
        public static async Task UpdateShopWaifu(ShopWaifu shopWaifu)
        {
            using (var dbContext = new SqliteDbContext())
            {
                dbContext.ShopWaifus.Update(shopWaifu);
                await dbContext.SaveChangesAsync();
            }
        }
        public static async Task CompletelyDeleteWaifu(Waifu waifu)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var userWaifu = DbContext.ShopWaifus.Where(x => x.Waifu.Equals(waifu));
                if (userWaifu == null)
                    return;

                DbContext.ShopWaifus.RemoveRange(userWaifu);
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                var shops = db.WaifuShops.Where(x => x.GuildId == guildId);

                foreach (var shop in shops)
                {
                    db.ShopWaifus.RemoveRange(db.ShopWaifus.Where(x => x.WaifuShop.Id == shop.Id));
                }
                db.WaifuShops.RemoveRange(shops);

                await db.SaveChangesAsync();
            }
        }
    }


    public class FeaturedWaifuDb
    {
        public static async Task SetFeaturedWaifu(ulong userId, Waifu waifu, ulong guildId)
        {
            using (var dbContext = new SqliteDbContext())
            {
                var entry = new FeaturedWaifu { UserId = userId, Waifu = waifu, GuildId = guildId };

                dbContext.FeaturedWaifus.RemoveRange(dbContext.FeaturedWaifus.Where(x => x.UserId == userId & x.GuildId == guildId));
                dbContext.FeaturedWaifus.Update(entry);
                await dbContext.SaveChangesAsync();
            }
        }
        public static Waifu GetFeaturedWaifu(ulong userId, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                var waifu = db.FeaturedWaifus.Where(x => x.UserId == userId && x.GuildId == guildId).Select(x => x.Waifu).LastOrDefault();
                if (waifu == null)
                    waifu = UserInventoryDb.GetWaifus(userId, guildId).LastOrDefault();
                return waifu;
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.FeaturedWaifus.RemoveRange(db.FeaturedWaifus.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
        public static async Task Delete(ulong userId, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.FeaturedWaifus.RemoveRange(db.FeaturedWaifus.Where(x => x.UserId == userId && x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
    }

    public class WaifuWishlistDb
    {
        public static async Task AddWaifuWish(ulong userId, Waifu waifu, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                var entry = new WaifuWish { UserId = userId, Waifu = waifu, GuildId = guildId };

                int cap = 5;
                if (PremiumDb.IsPremium(userId, PremiumType.Waifu))
                    cap = 12;

                db.WaifuWishlist.Update(entry);
                if (db.WaifuWishlist.Where(x => x.UserId == userId && x.GuildId == guildId).Count() >= cap)
                    db.WaifuWishlist.Remove(db.WaifuWishlist.Where(x => x.UserId == userId && x.GuildId == guildId).First());

                await db.SaveChangesAsync();
            }
        }
        public static List<Waifu> GetWishlist(ulong userId, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                var waifus = db.WaifuWishlist.Where(x => x.UserId == userId && x.GuildId == guildId).Select(x => x.Waifu).ToList();
                return waifus;
            }
        }
        public static List<WaifuWish> GetWishlist(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                var wishes = db.WaifuWishlist.Include(x => x.Waifu).Where(x => x.GuildId == guildId).ToList();
                return wishes;
            }
        }
        public static List<WaifuWish> GetWishlist(ulong guildId, string waifuName)
        {
            using (var db = new SqliteDbContext())
            {
                var wishes = db.WaifuWishlist.Include(x => x.Waifu).Where(x => x.GuildId == guildId && x.Waifu.Name == waifuName).ToList();
                return wishes;
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.WaifuWishlist.RemoveRange(db.WaifuWishlist.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
        public static async Task DeleteWaifuWish(ulong userId, Waifu waifu, ulong guildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var userWaifu = DbContext.WaifuWishlist.Where(x => x.UserId == userId && x.Waifu.Equals(waifu) && x.GuildId == guildId).FirstOrDefault();
                if (userWaifu != null)
                {
                    DbContext.WaifuWishlist.Remove(userWaifu);
                    await DbContext.SaveChangesAsync();
                }
            }
        }
        public static List<WaifuWish> GetAllPremiumWishlists(ulong guildId, PremiumType premium)
        {
            using (var db = new SqliteDbContext())
            {
                var users = db.Premiums.Where(x => x.Type == premium).Select(x => x.UserId);
                var wishlists = db.WaifuWishlist.Where(x => x.GuildId == guildId && users.Contains(x.UserId)).Include(x => x.Waifu);
                return wishlists.ToList();
            }
        }
    }
}
