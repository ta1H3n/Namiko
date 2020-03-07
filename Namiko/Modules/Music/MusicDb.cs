using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Namiko
{
    public class MusicDb
    {
        public static async Task<int> AddPlaylist(Playlist playlist)
        {
            using (var db = new SqliteDbContext())
            {
                db.Playlists.Add(playlist);
                return await db.SaveChangesAsync();
            }
        }

        public static bool IsPlaylist(string name, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Playlists.Any(x => x.Name.ToUpper().Equals(name.ToUpper()) && x.GuildId == guildId);
            }
        }

        public static async Task<List<Playlist>> GetPlaylists(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Playlists.Where(x => x.GuildId == guildId).ToListAsync();
            }
        }

        public static async Task<Playlist> GetPlaylist(int id)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Playlists.Where(x => x.Id == id)
                    .Include(x => x.Tracks)
                    .FirstOrDefaultAsync();
            }
        }

        public static async Task DeletePlaylist(int id)
        {
            using (var db = new SqliteDbContext())
            {
                db.Playlists.RemoveRange(db.Playlists.Where(x => x.Id == id));
                await db.SaveChangesAsync();
            }
        }
    }
}
