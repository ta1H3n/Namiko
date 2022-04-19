using Reddit;
using Reddit.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Namiko
{
    public static class RedditAPI
    {
        private static RedditClient Client;

        static RedditAPI()
        {
            ApiSettings settings = AppSettings.Reddit;
            Client = new RedditClient(settings.ClientId, settings.RefreshToken, settings.ClientSecret);
        }

        public static async Task<List<Post>> GetHot(string subredditName, int limit = 15)
        {
            try
            {
                var sub = Client.Subreddit(subredditName);
                List<Post> posts = null;
                await Task.Run(() => posts = sub.Posts.GetHot(limit: limit));
                return posts;
            }
            catch { return null; }
        }

        public static async Task<Subreddit> GetSubreddit(string subredditName)
        {
            var sub = Client.Subreddit(subredditName);
            await Task.Run(() => sub = sub.About());
            return sub;
        }
    }
}
