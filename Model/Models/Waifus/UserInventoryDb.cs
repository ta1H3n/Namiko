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
        public static async Task<Dictionary<string, int>> CountWaifus(ulong guildId = 0)
        {
            using var cmd = new NamikoDbContext().Database.GetDbConnection().CreateCommand();
            string where = "";
            if (guildId != 0)
                where = $" where \"GuildId\" = {guildId} ";
            cmd.CommandText = $"select \"WaifuName\", count(*) from \"UserInventories\" {where} Group By \"WaifuName\" order by count(*) desc";
            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }

            var res = new Dictionary<string, int>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        res.Add(reader.GetString(0), reader.GetInt32(1));
                    }
                }
            }
            return res;
        }
    }
}
