using Namiko.Data;
using Newtonsoft.Json;
using Reddit;
using Reddit.Controllers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Namiko
{
    public static class RedditAPI
    {
        private static RedditClient Client;

        public static void RedditSetup()
        {
            string JSON = "";
            string JSONLocation = Locations.RedditJSON;
            using (var Stream = new FileStream(JSONLocation, FileMode.Open, FileAccess.Read))
            using (var ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            ApiLogin settings = JsonConvert.DeserializeObject<ApiLogin>(JSON);

            Client = new Reddit.RedditClient(settings.ClientId, settings.RefreshToken, settings.ClientSecret);
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
