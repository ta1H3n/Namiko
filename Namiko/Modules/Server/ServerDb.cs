using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;



using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Namiko
{
    public static class ServerDb
    {
        public static Server GetServer(ulong GuildId)
        {
            using (SqliteDbContext db = new SqliteDbContext())
            {
                return db.Servers.Where(x => x.GuildId == GuildId).FirstOrDefault();
            }
        }
        public static async Task UpdateServer(Server server)
        {
            using (SqliteDbContext db = new SqliteDbContext())
            {
                if (!db.Servers.Any(x => x.GuildId == server.GuildId))
                    db.Add(server);

                else
                    db.Update(server);

                await db.SaveChangesAsync();
            }
        }
        public static async Task DeleteServer(ulong GuildId)
        {
            using (SqliteDbContext db = new SqliteDbContext())
            {
                var server = db.Servers.Where(x => x.GuildId == GuildId).FirstOrDefault();
                if(server != null)
                {
                    db.Remove(server);
                    await db.SaveChangesAsync();
                }
            }
        }
        public static List<Server> GetAll()
        {
            using (var db = new SqliteDbContext())
            {
                return db.Servers.ToList();
            }
        }
        public static List<Server> GetLeft()
        {
            using (var db = new SqliteDbContext())
            {
                var time = new DateTime(0);
                return db.Servers.Where(x => x.LeaveDate > time).ToList();
            }
        }
        public static HashSet<ulong> GetNotLeft(int shard = -1, int shardCount = -1)
        {
            using (var db = new SqliteDbContext())
            {
                var zerotime = new DateTime(0);
                return db.Servers.Where(x => x.LeaveDate == zerotime && shard == -1 ? true : (int)(x.GuildId >> 22) % shardCount == shard).Select(x => x.GuildId).ToHashSet();
            }
        }
        public static List<Server> GetOld()
        {
            using (var db = new SqliteDbContext())
            {
                var date = new DateTime(0);
                return db.Servers.Where(x => x.LeaveDate != date && x.LeaveDate.AddDays(3) < DateTime.Now).ToList();
            }
        }
    }

    public static class BlacklistedChannelDb
    {
        public static async Task UpdateBlacklistedChannel(BlacklistedChannel ch)
        {
            using (SqliteDbContext db = new SqliteDbContext())
            {
                var res = db.BlacklistedChannels.Where(x => x.ChannelId == ch.ChannelId).FirstOrDefault();
                if (res == null)
                    db.Add(ch);

                else
                    db.Update(ch);

                await db.SaveChangesAsync();
            }
        }
        public static async Task DeleteBlacklistedChannel(ulong channelId)
        {
            using (SqliteDbContext db = new SqliteDbContext())
            {
                var ch = db.BlacklistedChannels.Where(x => x.ChannelId == channelId).FirstOrDefault();
                if (ch != null)
                {
                    db.Remove(ch);
                    await db.SaveChangesAsync();
                }
            }
        }
        public static bool IsBlacklisted(ulong channelId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.BlacklistedChannels.Any(x => x.ChannelId == channelId);
            }
        }
    }

    public static class SpecialChannelDb
    {
        public static bool IsType(ulong channelId, ChannelType type)
        {
            using (var db = new SqliteDbContext())
            {
                return db.SpecialChannels.Any(x => x.ChannelId == channelId && x.Type == type);
            }
        }
        public static List<ulong> GetIdsByType(ChannelType type)
        {
            using (var db = new SqliteDbContext())
            {
                return db.SpecialChannels.Where(x => x.Type == type).Select(x => x.ChannelId).ToList();
            }
        }
        public static List<SpecialChannel> GetChannelsByType(ChannelType type)
        {
            using (var db = new SqliteDbContext())
            {
                return db.SpecialChannels.Where(x => x.Type == type).ToList();
            }
        }
        public static async Task Delete(ulong channelId, ChannelType type)
        {
            using (var db = new SqliteDbContext())
            {
                var ch = db.SpecialChannels.Where(x => x.ChannelId == channelId && x.Type == type);
                if (ch != null)
                {
                    db.SpecialChannels.RemoveRange(ch);
                    await db.SaveChangesAsync();
                }
            }
        }
        public static async Task Delete(SpecialChannel ch)
        {
            using (var db = new SqliteDbContext())
            {
                db.Remove(ch);
                await db.SaveChangesAsync();
            }
        }
        public static async Task<int> AddChannel(ulong channelId, ChannelType type, ulong guildId, string args = "")
        {
            using (var db = new SqliteDbContext())
            {
                if (db.SpecialChannels.Any(x => x.ChannelId == channelId && x.Type == type && x.Args == args))
                    return -1;

                db.SpecialChannels.Add(new SpecialChannel()
                {
                    ChannelId = channelId,
                    GuildId = guildId,
                    Type = type,
                    Args = args
                });
                return await db.SaveChangesAsync();
            }
        }
        public static List<SpecialChannel> GetChannelsByGuild(ulong guildId, ChannelType? type = null)
        {
            using (var db = new SqliteDbContext())
            {
                if (type.HasValue)
                {
                    return db.SpecialChannels.Where(x => x.GuildId == guildId && x.Type == type.Value).ToList();
                }
                return db.SpecialChannels.Where(x => x.GuildId == guildId).ToList();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.SpecialChannels.RemoveRange(db.SpecialChannels.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
    }
}
