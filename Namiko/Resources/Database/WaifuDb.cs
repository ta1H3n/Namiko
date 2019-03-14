using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Namiko.Resources.Datatypes;
using Namiko.Resources.Database;
using System.Threading.Tasks;

namespace Namiko.Resources.Database
{
    public class WaifuDb
    {
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
        public static List<Waifu> SearchWaifus(string name)
        {
            using (var DbContext = new SqliteDbContext())
            {
                List<Waifu> waifus = new List<Waifu>();

                Waifu waifu = DbContext.Waifus.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                if(waifu != null)
                {
                    waifus.Add(waifu);
                    return waifus;
                }

                waifus.AddRange(DbContext.Waifus.Where(x => x.LongName == null ? false : x.LongName.Contains(name, StringComparison.InvariantCultureIgnoreCase)).ToList());
                if(waifus.Count == 0)
                {
                    waifus.AddRange(DbContext.Waifus.Where(x => x.Source == null ? false : x.Source.Contains(name, StringComparison.InvariantCultureIgnoreCase)).ToList());
                }
               // if (waifus.Count == 0)
               // {
               //     waifus.AddRange(DbContext.Waifus.Where(x => x.Description == null ? false : x.Description.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList());
               // }

                return waifus;
            }

        }
        public static List<Waifu> GetWaifus()
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
        public static List<Waifu> RandomWaifus(int tier, int amount)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Waifus.Where(x => x.Tier == tier).OrderBy(r => Guid.NewGuid()).Take(amount).ToList();
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
                return await DbContext.SaveChangesAsync();
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

                var items = db.UserInventories;

                var waifus = items.Select(x => x.Waifu).ToList();
                var stores = items.ToList();
                for (int i = 0; i < stores.Count; i++)
                {
                    stores[i].Waifu = waifus[i];
                }
                return stores;
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

                var items = db.UserInventories.Where(x => x.GuildId == GuildId);

                var waifus = items.Select(x => x.Waifu).ToList();
                var stores = items.ToList();
                for (int i = 0; i < stores.Count; i++)
                {
                    stores[i].Waifu = waifus[i];
                }
                return stores;
            }
        }
        public static long TotalToasties(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.WaifuStores.Where(x => x.GuildId == guildId).Sum(x => Convert.ToInt64(Namiko.Core.Util.WaifuUtil.GetPrice(x.Waifu.Tier, 0)));
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
        public static async Task AddWaifu(ShopWaifu shopWaifu)
        {
            using(var dbContext = new SqliteDbContext())
            {
                dbContext.Update(shopWaifu);
                await dbContext.SaveChangesAsync();
            }
        }
        public static async Task NewList(IEnumerable<ShopWaifu> waifuStores)
        {
            using (var dbContext = new SqliteDbContext())
            {
                if(waifuStores.Any())
                {
                    var oldShop = dbContext.WaifuStores.Where(x => x.GuildId == waifuStores.First().GuildId);
                    dbContext.WaifuStores.RemoveRange(oldShop);
                    await dbContext.SaveChangesAsync();
                    foreach(var x in waifuStores)
                    {
                        await AddWaifu(x);
                    }
                }
            }
        }
        public static ShopWaifu GetLastWaifuStore(ulong guildId)
        {
            using (var dbContext = new SqliteDbContext())
            {
                return dbContext.WaifuStores.LastOrDefault(x => x.GuildId == guildId);
            }
        }
        public static List<ShopWaifu> GetWaifuStores(ulong guildId)
        {
            using (var dbContext = new SqliteDbContext())
            {
                var waifu = dbContext.WaifuStores.LastOrDefault(x => x.GuildId == guildId);
                if (waifu == null)
                    return null;
                var storesque = dbContext.WaifuStores.Where(x => x.GeneratedDate.Equals(waifu.GeneratedDate) && x.GuildId == guildId).OrderBy(x => x.Id);

                var waifus = storesque.Select(x => x.Waifu).ToList();
                var stores = storesque.ToList();
                for(int i=0; i<stores.Count; i++)
                {
                    stores[i].Waifu = waifus[i];
                }
                return stores;
            }
        }
        public static async Task UpdateWaifu(ShopWaifu shopWaifu)
        {
            using (var dbContext = new SqliteDbContext())
            {
                dbContext.WaifuStores.Update(shopWaifu);
                await dbContext.SaveChangesAsync();
            }
        }
        public static async Task CompletelyDeleteWaifu(Waifu waifu)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var userWaifu = DbContext.WaifuStores.Where(x => x.Waifu.Equals(waifu));
                if (userWaifu == null)
                    return;

                DbContext.WaifuStores.RemoveRange(userWaifu);
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.WaifuStores.RemoveRange(db.WaifuStores.Where(x => x.GuildId == guildId));
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
    }
}
