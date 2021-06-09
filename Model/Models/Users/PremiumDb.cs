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
            using var db = new NamikoDbContext();
            return db.Premiums.Where(x => x.UserId == UserId && x.ExpiresAt > DateTime.Now).ToList();
        }
        public static List<Premium> GetGuildPremium(ulong GuildId)
        {
            using var db = new NamikoDbContext();
            return db.Premiums.Where(x => x.GuildId == GuildId && x.ExpiresAt > DateTime.Now).ToList();
        }
        public static bool IsPremium(ulong Id, ProType type)
        {
            using var db = new NamikoDbContext();
            return db.Premiums.Any(x => x.Type == type && (x.GuildId == Id || x.UserId == Id) && x.ExpiresAt > DateTime.Now);
        }
        public static List<Premium> GetNewlyExpired()
        {
            using var db = new NamikoDbContext();
            return db.Premiums.Where(x => x.ExpiresAt < DateTime.Now && x.ExpireSent == false).ToList();
        }

        public async static Task<int> UpdatePremium(Premium premium)
        {
            using var db = new NamikoDbContext();
            db.Premiums.Update(premium);
            return await db.SaveChangesAsync();
        }
        public async static Task<int> DeletePremium(Premium premium)
        {
            using var db = new NamikoDbContext();
            db.Premiums.Remove(premium);
            return await db.SaveChangesAsync();
        }
        public async static Task<int> AddPremium(ulong userId, ProType type, ulong guildId = 0, DateTime? expiresAt = null)
        {
            if (expiresAt == null)
            {
                expiresAt = DateTime.Now.AddMonths(1);
            }

            using var db = new NamikoDbContext();
            db.Premiums.Add(new Premium
            {
                GuildId = guildId,
                UserId = userId,
                Type = type,
                ClaimDate = System.DateTime.Now,
                ExpiresAt = expiresAt.Value,
                ExpireSent = false
            });
            return await db.SaveChangesAsync();
        }
    }
}
