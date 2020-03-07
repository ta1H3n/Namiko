using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Namiko
{
    public static class ImageDb
    {
        public static async Task AddImage(string name, string url, ulong guildId = 0, int id = -1)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if(id == -1)
                    DbContext.Add(new ReactionImage { Name = name.ToLowerInvariant(), Url = url, GuildId = guildId });
                else
                    DbContext.Add(new ReactionImage { Id = id, Name = name.ToLowerInvariant(), Url = url, GuildId = guildId });
                await DbContext.SaveChangesAsync();
            }
        }

        public static List<ReactionImage> GetImages(string name = null, ulong guildId = 0)
        {
            using (var db = new SqliteDbContext())
            {
                if(name == null)
                    return db.Images.Where(x => x.GuildId == guildId).ToList();
                return db.Images.Where(x => x.Name == name && x.GuildId == guildId).ToList();
            }

        }

        public static async Task DeleteImage(int id)
        {
            using (var DbContext = new SqliteDbContext())
            {
                ReactionImage image = DbContext.Images.Where(x => x.Id == id).FirstOrDefault();
                DbContext.Images.Remove(image);
                await DbContext.SaveChangesAsync();
            }
        }

        public static ReactionImage GetImage(int id)
        {
            using (var DbContext = new SqliteDbContext())
            {
                ReactionImage message = DbContext.Images.Where(x => x.Id == id).FirstOrDefault();
                return message;
            }
        }

        public static ReactionImage GetRandomImage(string name, ulong guildId = 0)
        {
            using (var DbContext = new SqliteDbContext())
            {
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
        }

        public static ReactionImage GetLastImage()
        {
            using (var DbContext = new SqliteDbContext())
            {
                ReactionImage message = DbContext.Images.OrderByDescending(x => x.Id).FirstOrDefault();
                return message;
            }
        }

        public static async Task ToLower()
        {
            using(var db = new SqliteDbContext())
            {
                var images = db.Images;
                foreach(var x in images)
                {
                    x.Name = x.Name.ToLowerInvariant();
                }
                db.Images.UpdateRange(images);
                await db.SaveChangesAsync();
            }
        }

        public static bool AlbumExists(string name)
        {
            using (var db = new SqliteDbContext())
            {
                bool res = db.ImgurAlbums.Any(x => x.Name.ToUpper().Equals(name.ToUpper()));
                return res;
            }
        }

        public static async Task UpdateImage(ReactionImage image)
        {
            using (var db = new SqliteDbContext())
            {
                db.Images.Update(image);
                await db.SaveChangesAsync();
            }
        }

        public static ImgurAlbumLink GetAlbum(string name)
        {
            using (var db = new SqliteDbContext())
            {
                return db.ImgurAlbums.FirstOrDefault(x => x.Name.ToUpper().Equals(name.ToUpper()));
            }
        }

        public static async Task CreateAlbum(string name, string albumId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Add(new ImgurAlbumLink { AlbumId = albumId, Name = name.ToLowerInvariant() });
                await db.SaveChangesAsync();
            }
        }
    }
}
