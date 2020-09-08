using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class WaifuDb
    {
        public static async Task<Waifu> GetWaifu(string name)
        {
            using var db = new NamikoDbContext();
            return await db.Waifus.Include(x => x.Mal).FirstOrDefaultAsync(x => x.Name == name);
        }
        public static async Task<int> AddWaifu(Waifu waifu)
        {
            using var DbContext = new NamikoDbContext();
            DbContext.Add(waifu);
            return await DbContext.SaveChangesAsync();
        }
        public static async Task<int> UpdateWaifu(Waifu waifu)
        {
            using var DbContext = new NamikoDbContext();
            DbContext.Update(waifu);
            return await DbContext.SaveChangesAsync();
        }
        public static async Task<List<Waifu>> SearchWaifus(string query, bool primaryName = false, IEnumerable<Waifu> from = null, bool includeMAL = false, int perPage = 0, int page = 0)
        {
            using var DbContext = new NamikoDbContext();
            List<Waifu> waifus = new List<Waifu>();

            var waifuQuery = from == null ? DbContext.Waifus : from.AsQueryable();
            if (includeMAL)
                waifuQuery.Include(x => x.Mal);

            if (primaryName)
            {
                Waifu waifu = waifuQuery.Where(x => x.Name.ToUpper().Equals(query.ToUpper())).FirstOrDefault();
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
                    x.Name.ToUpper().Contains(word.ToUpper()) ||
                    (x.LongName == null ? false : x.LongName.ToUpper().Contains(word.ToUpper())) ||
                    (x.Source == null ? false : x.Source.ToUpper().Contains(word.ToUpper())));
            }

            if (perPage > 0)
            {
                waifuQuery = waifuQuery.Skip(perPage * page).Take(perPage);
            }

            waifus = from == null ? await waifuQuery.ToListAsync() : waifuQuery.ToList();

            return waifus;

        }
        public static async Task<List<Waifu>> AllWaifus()
        {
            using var DbContext = new NamikoDbContext();
            return await DbContext.Waifus.ToListAsync();

        }
        public static async Task<int> DeleteWaifu(string name)
        {
            using var DbContext = new NamikoDbContext();
            Waifu waifu = DbContext.Waifus.Where(x => x.Name == name).FirstOrDefault();
            if (waifu == null)
                return 0;

            await UserInventoryDb.CompletelyDeleteWaifu(waifu);
            await WaifuShopDb.CompletelyDeleteWaifu(waifu);
            DbContext.Waifus.Remove(waifu);
            return await DbContext.SaveChangesAsync();
        }
        public static async Task<List<Waifu>> GetWaifusByTier(int tier)
        {
            using var DbContext = new NamikoDbContext();
            return await DbContext.Waifus.Where(x => x.Tier == tier).ToListAsync();
        }
        public static async Task<List<Waifu>> RandomWaifus(int tier, int amount, List<string> includeSource = null, List<string> excludeSource = null)
        {
            using var db = new NamikoDbContext();
            return (await db.Waifus.Where(x => x.Tier == tier &&
                (includeSource == null || includeSource.Contains(x.Source)) &&
                (excludeSource == null || !excludeSource.Contains(x.Source)))
                .ToListAsync())
                .OrderBy(r => Guid.NewGuid())
                .Take(amount).ToList();
        }
        public static async Task<int> RenameWaifu(string oldName, string newName)
        {
            using var db = new NamikoDbContext();
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
            foreach (var x in actualL)
                x.Name = newName;

            db.Waifus.AddRange(actualL);

            int res = await db.SaveChangesAsync();

            db.UserInventories.AddRange(invL);
            db.WaifuWishlist.AddRange(wishL);
            db.ShopWaifus.AddRange(storeL);
            db.FeaturedWaifus.AddRange(featL);

            return res + await db.SaveChangesAsync();
        }

        public static async Task<int> AddMalWaifu(MalWaifu waifu)
        {
            using var db = new NamikoDbContext();
            db.MalWaifus.Add(waifu);
            return await db.SaveChangesAsync();
        }
        public static async Task<int> UpdateMalWaifu(MalWaifu waifu)
        {
            using var db = new NamikoDbContext();
            db.MalWaifus.Update(waifu);
            return await db.SaveChangesAsync();
        }
        public static async Task<MalWaifu> GetMalWaifu(string waifuName)
        {
            using var db = new NamikoDbContext();
            return await db.MalWaifus.Include(x => x.Waifu).FirstOrDefaultAsync(x => x.WaifuName == waifuName);
        }
        public static async Task<MalWaifu> GetMalWaifu(long malId)
        {
            using var db = new NamikoDbContext();
            return await db.MalWaifus.Include(x => x.Waifu).FirstOrDefaultAsync(x => x.MalId == malId);
        }
    }
}
