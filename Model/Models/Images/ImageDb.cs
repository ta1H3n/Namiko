using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class ImageDb
    {
        public static async Task<ReactionImage> AddImage(string name, string url, ulong guildId = 0, int id = -1)
        {
            using var DbContext = new NamikoDbContext();
            ReactionImage img;
            if (id == -1)
            {
                img = new ReactionImage { Name = name.ToLowerInvariant(), Url = url, GuildId = guildId };
            }
            else
            {
                img = new ReactionImage { Id = id, Name = name.ToLowerInvariant(), Url = url, GuildId = guildId };
            }
            DbContext.Images.Add(img);
            await DbContext.SaveChangesAsync();

            return img;
        }

        public static async Task<List<ReactionImage>> GetImages(string name = null, ulong guildId = 0)
        {
            using var db = new NamikoDbContext();
            if (name == null)
                return await db.Images.Where(x => x.GuildId == guildId).ToListAsync();
            return await db.Images.Where(x => x.Name == name && x.GuildId == guildId).ToListAsync();

        }

        public static async Task DeleteImage(int id)
        {
            using var DbContext = new NamikoDbContext();
            ReactionImage image = DbContext.Images.Where(x => x.Id == id).FirstOrDefault();
            DbContext.Images.Remove(image);
            await DbContext.SaveChangesAsync();
        }

        public static ReactionImage GetImage(int id)
        {
            using var DbContext = new NamikoDbContext();
            ReactionImage message = DbContext.Images.Where(x => x.Id == id).FirstOrDefault();
            return message;
        }

        public static ReactionImage GetRandomImage(string name, ulong guildId = 0)
        {
            using var DbContext = new NamikoDbContext();
            int count = DbContext.Images.Where(x => x.Name.ToUpper().Equals(name.ToUpper()) && x.GuildId == guildId).Count();
            if (count < 1)
            {
                return null;
            }
            else
            {
                Random rand = new Random();
                int random = rand.Next(count);
                return DbContext.Images.Where(x => x.Name.ToUpper().Equals(name.ToUpper()) && x.GuildId == guildId).ToArray<ReactionImage>().ElementAt(random);
            }
        }

        public static ReactionImage GetLastImage()
        {
            using var DbContext = new NamikoDbContext();
            ReactionImage message = DbContext.Images.OrderByDescending(x => x.Id).FirstOrDefault();
            return message;
        }

        public static HashSet<string> GetReactionImageCommandHashSet()
        {
            using (var db = new NamikoDbContext())
            {
                return db.Images.Select(x => x.Name).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        public static async Task<Dictionary<ulong, HashSet<string>>> GetReactionImageDictionary()
        {
            using (var db = new NamikoDbContext())
            {
                var images = db.Images.Select(x => new { x.GuildId, x.Name }).AsEnumerable().GroupBy(x => x.GuildId, x => x.Name.ToLower());
                return new Dictionary<ulong, HashSet<string>>(images.Select(x => new KeyValuePair<ulong, HashSet<string>>(x.Key, x.ToHashSet())));
            }
        }

        public static async Task ToLower()
        {
            using var db = new NamikoDbContext();
            var images = db.Images;
            foreach (var x in images)
            {
                x.Name = x.Name.ToLowerInvariant();
            }
            db.Images.UpdateRange(images);
            await db.SaveChangesAsync();
        }

        public static bool AlbumExists(string name)
        {
            using var db = new NamikoDbContext();
            bool res = db.ImgurAlbums.Any(x => x.Name.ToUpper().Equals(name.ToUpper()));
            return res;
        }

        public static async Task UpdateImage(ReactionImage image)
        {
            using var db = new NamikoDbContext();
            db.Images.Update(image);
            await db.SaveChangesAsync();
        }

        public static ImgurAlbumLink GetAlbum(string name)
        {
            using var db = new NamikoDbContext();
            return db.ImgurAlbums.FirstOrDefault(x => x.Name.ToUpper().Equals(name.ToUpper()));
        }

        public static async Task CreateAlbum(string name, string albumId)
        {
            using var db = new NamikoDbContext();
            db.Add(new ImgurAlbumLink { AlbumId = albumId, Name = name.ToLowerInvariant() });
            await db.SaveChangesAsync();
        }
    }
}
