using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class SpecialChannelDb
    {
        public static bool IsType(ulong channelId, ChannelType type)
        {
            using var db = new NamikoDbContext();
            return db.SpecialChannels.Any(x => x.ChannelId == channelId && x.Type == type);
        }
        public static List<ulong> GetIdsByType(ChannelType type)
        {
            using var db = new NamikoDbContext();
            return db.SpecialChannels.Where(x => x.Type == type).Select(x => x.ChannelId).ToList();
        }
        public static List<SpecialChannel> GetChannelsByType(ChannelType type)
        {
            using var db = new NamikoDbContext();
            return db.SpecialChannels.Where(x => x.Type == type).ToList();
        }
        public static async Task Delete(ulong channelId, ChannelType type)
        {
            using var db = new NamikoDbContext();
            db.SpecialChannels.RemoveRange(db.SpecialChannels.Where(x => x.ChannelId == channelId && x.Type == type));
            await db.SaveChangesAsync();
        }
        public static async Task Delete(ulong channelId)
        {
            using var db = new NamikoDbContext();
            db.SpecialChannels.RemoveRange(db.SpecialChannels.Where(x => x.ChannelId == channelId));
            await db.SaveChangesAsync();
        }
        public static async Task Delete(SpecialChannel ch)
        {
            using var db = new NamikoDbContext();
            db.Remove(ch);
            await db.SaveChangesAsync();
        }
        public static async Task<int> AddChannel(ulong channelId, ChannelType type, ulong guildId, string args = "")
        {
            using var db = new NamikoDbContext();
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
        public static List<SpecialChannel> GetChannelsByGuild(ulong guildId, ChannelType? type = null)
        {
            using var db = new NamikoDbContext();
            if (type.HasValue)
            {
                return db.SpecialChannels.Where(x => x.GuildId == guildId && x.Type == type.Value).ToList();
            }
            return db.SpecialChannels.Where(x => x.GuildId == guildId).ToList();
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.SpecialChannels.RemoveRange(db.SpecialChannels.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
    }
}
