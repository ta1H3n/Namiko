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
            using var db = new NamikoDbContext();
            return db.Servers.Where(x => x.GuildId == GuildId).FirstOrDefault();
        }
        public static Dictionary<ulong, string> GetPrefixes()
        {
            using (var db = new NamikoDbContext())
            {
                return db.Servers.ToDictionary(x => x.GuildId, x => x.Prefix);
            }
        }
        public static async Task UpdateServer(Server server)
        {
            using var db = new NamikoDbContext();
            if (!db.Servers.Any(x => x.GuildId == server.GuildId))
                db.Add(server);

            else
                db.Update(server);

            await db.SaveChangesAsync();
        }
        public static async Task DeleteServer(ulong GuildId)
        {
            using var db = new NamikoDbContext();
            var server = db.Servers.Where(x => x.GuildId == GuildId).FirstOrDefault();
            if (server != null)
            {
                db.Remove(server);
                await db.SaveChangesAsync();
            }
        }
        public static async Task<int> AddNewServers(IEnumerable<ulong> guildsIds, string prefix)
        {
            using var db = new NamikoDbContext();
            db.Servers.RemoveRange(db.Servers.Where(x => guildsIds.Contains(x.GuildId)));

            var zerotime = new DateTime(0);
            var servers = guildsIds.Select(x => new Server
            {
                GuildId = x,
                JoinDate = DateTime.Now,
                LeaveDate = zerotime,
                Prefix = prefix
            });

            db.Servers.AddRange(servers);
            return await db.SaveChangesAsync();
        }
        public static HashSet<ulong> GetAll()
        {
            using var db = new NamikoDbContext();
            return db.Servers.Select(x => x.GuildId).ToHashSet();
        }
        public static List<Server> GetLeft()
        {
            using var db = new NamikoDbContext();
            var time = new DateTime(0);
            return db.Servers.Where(x => x.LeaveDate != null && x.LeaveDate > time).ToList();
        }
        public static HashSet<ulong> GetNotLeft()
        {
            using var db = new NamikoDbContext();
            return db.Servers.Where(x => x.LeaveDate == null).Select(x => x.GuildId).ToHashSet();
        }
        public static List<Server> GetOld()
        {
            using var db = new NamikoDbContext();
            return db.Servers.Where(x => x.LeaveDate != null && x.LeaveDate.Value.AddDays(3) < DateTime.Now).ToList();
        }
    }
}
