﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Namiko.Resources.Datatypes;
using Namiko.Resources.Database;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Namiko.Resources.Database
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
                var res = db.Servers.Where(x => x.GuildId == server.GuildId).FirstOrDefault();
                if (res == null)
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
}