using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class PremiumDb
    {
        public static List<Premium> GetUserPremium(ulong UserId)
        {
            using var db = new SqliteDbContext();
            return db.Premiums.Where(x => x.UserId == UserId).ToList();
        }
        public static List<Premium> GetGuildPremium(ulong GuildId)
        {
            using var db = new SqliteDbContext();
            return db.Premiums.Where(x => x.GuildId == GuildId).ToList();
        }
        public static bool IsPremium(ulong Id, PremiumType type)
        {
            using var db = new SqliteDbContext();
            return db.Premiums.Any(x => x.Type == type && (x.GuildId == Id || x.UserId == Id));
        }
        public static List<Premium> GetAllPremiums()
        {
            using var db = new SqliteDbContext();
            return db.Premiums.ToList();
        }
        public static List<Premium> GetAllPremiums(DateTime ClaimedBefore)
        {
            using var db = new SqliteDbContext();
            return db.Premiums.Where(x => x.ClaimDate < ClaimedBefore).ToList();
        }

        public async static Task<int> UpdatePremium(Premium premium)
        {
            using var db = new SqliteDbContext();
            db.Premiums.Update(premium);
            return await db.SaveChangesAsync();
        }
        public async static Task<int> DeletePremium(Premium premium)
        {
            using var db = new SqliteDbContext();
            db.Premiums.Remove(premium);
            return await db.SaveChangesAsync();
        }
        public async static Task<int> AddPremium(ulong userId, PremiumType type, ulong guildId = 0)
        {
            using var db = new SqliteDbContext();
            db.Premiums.Add(new Premium
            {
                GuildId = guildId,
                UserId = userId,
                Type = type,
                ClaimDate = System.DateTime.Now
            });
            return await db.SaveChangesAsync();
        }
    }
}
