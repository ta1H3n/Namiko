using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class ServerDb
    {
        public static Server GetServer(ulong GuildId)
        {
            using var db = new SqliteDbContext();
            return db.Servers.Where(x => x.GuildId == GuildId).FirstOrDefault();
        }
        public static async Task UpdateServer(Server server)
        {
            using var db = new SqliteDbContext();
            if (!db.Servers.Any(x => x.GuildId == server.GuildId))
                db.Add(server);

            else
                db.Update(server);

            await db.SaveChangesAsync();
        }
        public static async Task DeleteServer(ulong GuildId)
        {
            using var db = new SqliteDbContext();
            var server = db.Servers.Where(x => x.GuildId == GuildId).FirstOrDefault();
            if (server != null)
            {
                db.Remove(server);
                await db.SaveChangesAsync();
            }
        }
        public static async Task<int> AddNewServers(IEnumerable<ulong> guildsIds, string prefix)
        {
            using var db = new SqliteDbContext();
            db.Servers.RemoveRange(db.Servers.Where(x => guildsIds.Contains(x.GuildId)));

            var zerotime = new DateTime(0);
            var servers = guildsIds.Select(x => new Server
            {
                GuildId = x,
                JoinDate = System.DateTime.Now,
                LeaveDate = zerotime,
                Prefix = prefix
            });

            db.Servers.AddRange(servers);
            return await db.SaveChangesAsync();
        }
        public static List<Server> GetAll()
        {
            using var db = new SqliteDbContext();
            return db.Servers.ToList();
        }
        public static List<Server> GetLeft()
        {
            using var db = new SqliteDbContext();
            var time = new DateTime(0);
            return db.Servers.Where(x => x.LeaveDate > time).ToList();
        }
        public static HashSet<ulong> GetNotLeft()
        {
            using var db = new SqliteDbContext();
            var zerotime = new DateTime(0);
            return db.Servers.Where(x => x.LeaveDate == zerotime).Select(x => x.GuildId).ToHashSet();
        }
        public static List<Server> GetOld()
        {
            using var db = new SqliteDbContext();
            var date = new DateTime(0);
            return db.Servers.Where(x => x.LeaveDate != date && x.LeaveDate.AddDays(3) < DateTime.Now).ToList();
        }
    }
}
