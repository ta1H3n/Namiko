using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class BalanceDb
    {
        public static int GetToasties(ulong UserId, ulong GuildId)
        {
            using var DbContext = new SqliteDbContext();
            return DbContext.Toasties.Where(x => x.UserId == UserId && x.GuildId == GuildId).Select(x => x.Amount).FirstOrDefault();
        }
        public static async Task SetToasties(ulong UserId, int Amount, ulong GuildId)
        {
            using var DbContext = new SqliteDbContext();
            var toasties = DbContext.Toasties.FirstOrDefault(x => x.UserId == UserId && x.GuildId == GuildId);

            if (toasties == null)
            {
                DbContext.Add(new Balance { UserId = UserId, Amount = Amount, GuildId = GuildId });
            }
            else
            {
                toasties.Amount = Amount;
                DbContext.Update(toasties);
            }
            await DbContext.SaveChangesAsync();
        }
        public static async Task<int> AddToasties(ulong userId, int amount, ulong guildId)
        {
            using var db = new SqliteDbContext();
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
        public static async Task<List<LeaderboardEntryId>> GetAllToasties(ulong guildId)
        {
            using var db = new SqliteDbContext();
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
        public static async Task<List<Balance>> GetAllToastiesRaw(ulong guildId)
        {
            using var db = new SqliteDbContext();
            return await db.Toasties.Where(x => x.GuildId == guildId).ToListAsync();
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new SqliteDbContext();
            db.Toasties.RemoveRange(db.Toasties.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
        public static async Task<long> TotalToasties(ulong guildId)
        {
            using var db = new SqliteDbContext();
            return await db.Toasties.Where(x => x.Amount > 0 && x.GuildId == guildId).SumAsync(x => (long)x.Amount);
        }
        public static async Task<int> AddNewServerBotBalance(IEnumerable<ulong> guildsIds, ulong clientId)
        {
            using var db = new SqliteDbContext();
            db.Toasties.RemoveRange(db.Toasties.Where(x => guildsIds.Contains(x.GuildId) && x.UserId == clientId));

            var balances = guildsIds.Select(x => new Balance
            {
                Amount = 1000000,
                GuildId = x,
                UserId = clientId
            });

            db.Toasties.AddRange(balances);
            return await db.SaveChangesAsync();
        }
    }
}
