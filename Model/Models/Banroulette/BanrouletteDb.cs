using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class BanrouletteDb
    {
        public static async Task NewBanroulette(Banroulette banroulette)
        {
            using var db = new SqliteDbContext();
            db.Add(banroulette);
            await db.SaveChangesAsync();
        }
        public static Banroulette GetBanroulette(ulong channelId)
        {
            using var db = new SqliteDbContext();
            return db.Banroulettes.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ChannelId == channelId && x.Active);
        }
        public static async Task EndBanroulette(int banrouletteId)
        {
            using var db = new SqliteDbContext();
            var br = db.Banroulettes.FirstOrDefault(x => x.Id == banrouletteId);
            if (br == null)
                return;
            br.Active = false;
            db.Update(br);
            await db.SaveChangesAsync();
        }
        public static async Task<bool> AddParticipant(ulong userId, Banroulette banroulette)
        {
            using var db = new SqliteDbContext();
            if (db.BanrouletteParticipants.Count(x => x.UserId == userId && x.Banroulette.Id == banroulette.Id) > 0)
                return false;

            db.Update(new BanrouletteParticipant { Banroulette = banroulette, UserId = userId });

            if (await db.SaveChangesAsync() > 0)
                return true;
            return false;
        }
        public static List<ulong> GetParticipants(Banroulette banroulette)
        {
            using var db = new SqliteDbContext();
            return db.BanrouletteParticipants.Where(x => x.Banroulette.Id == banroulette.Id).Select(x => x.UserId).ToList();
        }
        public static bool IsParticipant(Banroulette banroulette, ulong userId)
        {
            using var db = new SqliteDbContext();
            return db.BanrouletteParticipants.Any(x => x.Banroulette.Id == banroulette.Id && userId == x.UserId);
        }
        public static async Task UpdateBanroulette(Banroulette banroulette)
        {
            using var db = new SqliteDbContext();
            db.Update(banroulette);
            await db.SaveChangesAsync();
        }
    }
}
