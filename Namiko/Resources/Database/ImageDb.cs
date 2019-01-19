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
    public static class ImageDb
    {
        public static async Task AddImage(string name, string url, int id = -1)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if(id == -1)
                    DbContext.Add(new ReactionImage { Name = name.ToLowerInvariant(), Url = url });
                else
                    DbContext.Add(new ReactionImage { Id = id, Name = name.ToLowerInvariant(), Url = url });
                await DbContext.SaveChangesAsync();
            }
        }

        public static List<ReactionImage> GetImages(string name = null)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if(name == null)
                    return DbContext.Images.ToList();
                return DbContext.Images.Where(x => x.Name == name).ToList();
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

        public static ReactionImage GetRandomImage(string name)
        {
            using (var DbContext = new SqliteDbContext())
            {
                int count = DbContext.Images.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count();
                if (count < 1)
                {
                    return null;
                }
                else
                {
                    Random rand = new Random();
                    int random = rand.Next(count);
                    return DbContext.Images.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToArray<ReactionImage>().ElementAt(random);
                }
            }
        }

        public static ReactionImage GetLastImage()
        {
            using (var DbContext = new SqliteDbContext())
            {
                ReactionImage message = DbContext.Images.Last();
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

        public static bool Exists(string name)
        {
            using (var db = new SqliteDbContext())
            {
                bool res = db.ImgurAlbums.Any(x => x.Name == name);
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
                return db.ImgurAlbums.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
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
