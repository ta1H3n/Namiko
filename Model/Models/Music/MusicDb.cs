﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class MusicDb
    {
        public static async Task<int> AddPlaylist(Playlist playlist)
        {
            using var db = new NamikoDbContext();
            db.Playlists.Add(playlist);
            return await db.SaveChangesAsync();
        }

        public static bool IsPlaylist(string name, ulong guildId)
        {
            using var db = new NamikoDbContext();
            return db.Playlists.Any(x => x.Name.ToUpper().Equals(name.ToUpper()) && x.GuildId == guildId);
        }

        public static async Task<List<Playlist>> GetPlaylists(ulong guildId)
        {
            using var db = new NamikoDbContext();
            return await db.Playlists.Where(x => x.GuildId == guildId).ToListAsync();
        }

        public static async Task<Playlist> GetPlaylist(int id)
        {
            using var db = new NamikoDbContext();
            return await db.Playlists.Where(x => x.Id == id)
.Include(x => x.Tracks)
.FirstOrDefaultAsync();
        }

        public static async Task DeletePlaylist(int id)
        {
            using var db = new NamikoDbContext();
            db.Playlists.RemoveRange(db.Playlists.Where(x => x.Id == id));
            await db.SaveChangesAsync();
        }
    }
}
