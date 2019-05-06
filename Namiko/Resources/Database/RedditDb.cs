using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Namiko.Resources.Datatypes;
using System.Linq;

namespace Namiko.Resources.Database
{
    public static class RedditDb
    {
        public static async Task AddPost(string name, string permalink)
        {
            using (var db = new SqliteDbContext())
            {
                db.RedditPosts.Add(new RedditPost() { FullName = name, PermaLink = permalink });
                await db.SaveChangesAsync();
            }
        }
        public static bool Exists(string name)
        {
            using (var db = new SqliteDbContext())
            {
                return db.RedditPosts.Any(x => x.FullName == name);
            }
        }
    }
}
