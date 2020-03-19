using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class LootBoxDb
    {
        public static int GetAmount(ulong UserId, LootBoxType type, ulong GuildId = 0)
        {
            using var db = new SqliteDbContext();
            return db.LootBoxes.Where(x => x.UserId == UserId && x.Type == type && x.GuildId == GuildId).Select(x => x.Amount).FirstOrDefault();
        }
        public static async Task AddLootbox(ulong UserId, LootBoxType type, int amount, ulong GuildId = 0)
        {
            using var db = new SqliteDbContext();
            var box = db.LootBoxes.Where(x => x.UserId == UserId && x.Type == type && x.GuildId == GuildId).FirstOrDefault();

            if (box == null)
            {
                db.LootBoxes.Add(new LootBox
                {
                    UserId = UserId,
                    GuildId = GuildId,
                    Amount = amount,
                    Type = type
                });
            }
            else
            {
                box.Amount += amount;
                if (box.Amount < 0)
                    throw new Exception("Lootbox amount < 0");
                db.LootBoxes.Update(box);
            }

            await db.SaveChangesAsync();
        }
        public static async Task<List<LootBox>> GetAll(ulong UserId, ulong GuildId)
        {
            using var db = new SqliteDbContext();
            var box = await db.LootBoxes.Where(x => x.UserId == UserId && (x.GuildId == GuildId || x.GuildId == 0) && x.Amount > 0).ToListAsync();

            return box;
        }
    }
}
