using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class RedditDb
    {
        public static bool Exists(string permalink, int upvotes = 0)
        {
            using var db = new SqliteDbContext();
            return db.RedditPosts.Any(x => x.PermaLink == permalink && x.Upvotes > upvotes);
        }
        public static async Task AddPost(string permalink, int upvotes)
        {
            using var db = new SqliteDbContext();
            var post = db.RedditPosts.FirstOrDefault(x => x.PermaLink == permalink);

            if (post == null)
            {
                db.RedditPosts.Add(new RedditPost() { PermaLink = permalink, Upvotes = upvotes });
            }
            else
            {
                post.Upvotes = upvotes;
                db.Update(post);
            }
            await db.SaveChangesAsync();
        }
        public static int GetUpvotes(string permalink)
        {
            using var db = new SqliteDbContext();
            var res = db.RedditPosts.FirstOrDefault(x => x.PermaLink == permalink);
            if (res == null)
                return 0;

            return res.Upvotes;
        }
    }
}
