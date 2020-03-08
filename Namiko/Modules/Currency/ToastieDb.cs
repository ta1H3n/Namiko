using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Linq;



using System.Threading.Tasks;

namespace Namiko
{
    public static class ToastieDb
    {
        public static int GetToasties(ulong UserId, ulong GuildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Toasties.Where(x => x.UserId == UserId && x.GuildId == GuildId).Select(x => x.Amount).FirstOrDefault();
            }
        }
        public static async Task SetToasties(ulong UserId, int Amount, ulong GuildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var toasties = DbContext.Toasties.FirstOrDefault(x => x.UserId == UserId && x.GuildId == GuildId);

                if (toasties == null)
                {
                    DbContext.Add(new Balance { UserId = UserId, Amount = Amount, GuildId = GuildId });
                } else
                {
                    toasties.Amount = Amount;
                    DbContext.Update(toasties);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task<int> AddToasties(ulong userId, int amount, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                var bal = await db.Toasties.FirstOrDefaultAsync(x => x.UserId == userId && x.GuildId == guildId);

                if (bal == null)
                    bal = new Balance
                    {
                        Amount = 0,
                        UserId = userId,
                        GuildId = guildId
                    };
                bal.Amount += amount;

                if (bal.Amount < 0)
                    throw new Exception("You don't have enough toasties... qq");
                else
                {
                    db.Toasties.Update(bal);
                    await db.SaveChangesAsync();
                }

                return bal.Amount;
            }
        }
        public static async Task<List<LeaderboardEntryId>> GetAllToasties(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Toasties
                    .Where(x => x.Amount > 0 && x.GuildId == guildId)
                    .Select(x => new LeaderboardEntryId
                    {
                        Id = x.UserId,
                        Count = x.Amount
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();
            }
        }
        public static async Task<List<Balance>> GetAllToastiesRaw(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Toasties.Where(x => x.GuildId == guildId).ToListAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Toasties.RemoveRange(db.Toasties.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
        public static async Task<long> TotalToasties(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Toasties.Where(x => x.Amount > 0 && x.GuildId == guildId).SumAsync(x => (long)x.Amount);
            }
        }
    }

    public static class DailyDb
    {
        public static Daily GetDaily(ulong UserId, ulong GuildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Dailies.Where(x => x.UserId == UserId && x.GuildId == GuildId).FirstOrDefault();
            }
        }
        public static async Task SetDaily(Daily Daily)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Update(Daily);
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task<List<LeaderboardEntryId>> GetLeaderboard(ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                return await db.Dailies
                    .Where(x => (x.GuildId == GuildId) && ((x.Date + 48 * 60 * 60 * 1000) > now))
                    .Select(x => new LeaderboardEntryId
                    {
                        Id = x.UserId,
                        Count = x.Streak
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();
            }
        }
        public static async Task<int> GetHighest(ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                try
                {
                    long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    return await db.Dailies.Where(x => x.GuildId == GuildId && ((x.Date + 48 * 60 * 60 * 1000) > now)).MaxAsync(x => x.Streak);
                }
                catch { return 1; }
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Dailies.RemoveRange(db.Dailies.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
    }

    public static class WeeklyDb
    {
        public static Weekly GetWeekly(ulong UserId, ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                var weekly = db.Weeklies.FirstOrDefault(x => x.GuildId == GuildId && x.UserId == UserId);
                return weekly;
            }
        }
        public static async Task SetWeekly(Weekly weekly)
        {
            using (var db = new SqliteDbContext())
            {
                db.Update(weekly);
                await db.SaveChangesAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Weeklies.RemoveRange(db.Weeklies.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
    }

    public static class LootBoxDb
    {
        public static int GetAmount(ulong UserId, LootBoxType type, ulong GuildId = 0)
        {
            using (var db = new SqliteDbContext())
            {
                return db.LootBoxes.Where(x => x.UserId == UserId && x.Type == type && x.GuildId == GuildId).Select(x => x.Amount).FirstOrDefault();
            }
        }
        public static async Task AddLootbox(ulong UserId, LootBoxType type, int amount, ulong GuildId = 0)
        {
            using (var db = new SqliteDbContext())
            {
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
        }
        public static async Task<List<LootBox>> GetAll(ulong UserId, ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                var box = await db.LootBoxes.Where(x => x.UserId == UserId && (x.GuildId == GuildId || x.GuildId == 0) && x.Amount > 0).ToListAsync();
                
                return box;
            }
        }
    }
    
}
