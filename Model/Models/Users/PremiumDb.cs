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
            return db.Premiums.Where(x => x.UserId == UserId).ToList();
        }
        public static List<Premium> GetGuildPremium(ulong GuildId)
        {
            using var db = new NamikoDbContext();
            return db.Premiums.Where(x => x.GuildId == GuildId).ToList();
        }
        public static bool IsPremium(ulong Id, ProType type)
        {
            using var db = new NamikoDbContext();
            return db.Premiums.Any(x => x.Type == type && (x.GuildId == Id || x.UserId == Id));
        }
        public static List<Premium> GetAllPremiums()
        {
            using var db = new NamikoDbContext();
            return db.Premiums.ToList();
        }
        public static List<Premium> GetAllPremiums(DateTime ClaimedBefore)
        {
            using var db = new NamikoDbContext();
            return db.Premiums.Where(x => x.ClaimDate < ClaimedBefore).ToList();
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
        public async static Task<int> AddPremium(ulong userId, ProType type, ulong guildId = 0)
        {
            using var db = new NamikoDbContext();
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
