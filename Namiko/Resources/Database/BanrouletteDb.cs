using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Namiko.Resources.Datatypes;
using Namiko.Resources.Database;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Namiko.Resources.Database
{
    public static class BanrouletteDb
    {
        public static async Task NewBanroulette(Banroulette banroulette)
        {
            using (var db = new SqliteDbContext())
            {
                db.Add(banroulette);
                await db.SaveChangesAsync();
            }
        }
        public static Banroulette GetBanroulette(ulong channelId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Banroulettes.LastOrDefault(x => x.ChannelId == channelId && x.Active);
            }
        }
        public static async Task EndBanroulette(int banrouletteId)
        {
            using (var db = new SqliteDbContext())
            {
                var br = db.Banroulettes.FirstOrDefault(x => x.Id == banrouletteId);
                if (br == null)
                    return;
                br.Active = false;
                db.Update(br);
                await db.SaveChangesAsync();
            }
        }
        public static async Task<bool> AddParticipant(ulong userId, Banroulette banroulette)
        {
            using (var db = new SqliteDbContext())
            {
                if (db.BanrouletteParticipants.Count(x => x.UserId == userId && x.Banroulette.Id == banroulette.Id) > 0)
                    return false;

                db.Update(new BanrouletteParticipant { Banroulette = banroulette, UserId = userId });

                if (await db.SaveChangesAsync() > 0)
                    return true;
                return false;
            }
        }
        public static List<ulong> GetParticipants(Banroulette banroulette)
        {
            using (var db = new SqliteDbContext())
            {
                return db.BanrouletteParticipants.Where(x => x.Banroulette.Id == banroulette.Id).Select(x => x.UserId).ToList();
            }
        }
        public static bool IsParticipant(Banroulette banroulette, ulong userId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.BanrouletteParticipants.Count(x => x.Banroulette.Id == banroulette.Id && userId == x.UserId) > 0 ? true : false;
            }
        }
        public static async Task UpdateBanroulette(Banroulette banroulette)
        {
            using (var db = new SqliteDbContext())
            {
                db.Update(banroulette);
                await db.SaveChangesAsync();
            }
        }
    }


    public static class BanDb
    { 
        public static async Task AddBan(BannedUser bannedUser)
        {
            using (var db = new SqliteDbContext())
            {
                db.Add(bannedUser);
                await db.SaveChangesAsync();
            }
        }
        public static async Task EndBan(ulong userId, ulong serverId)
        {
            using (var db = new SqliteDbContext())
            {
                var bans = db.BannedUsers.Where(x => x.UserId == userId && x.ServerId == serverId);
                if (!bans.Any())
                    return;

                foreach (var ban in bans)
                {
                    ban.Active = false;
                    db.BannedUsers.Update(ban);
                }
                await db.SaveChangesAsync();
            }
        }
        public static List<BannedUser> GetBans(bool active = true)
        {
            using (var db = new SqliteDbContext())
            {
                return db.BannedUsers.Where(x => x.Active == active).ToList();
            }
        }
    }
}
