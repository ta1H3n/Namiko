using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Namiko.Data;
using Newtonsoft.Json;
using Reddit;
using Reddit.Controllers;

namespace Namiko
{
    public static class RedditAPI
    {
        private static Reddit.RedditAPI Client;
        
        static RedditAPI()
        {
            RedditSetup();
        }

        private static void RedditSetup()
        {
            string JSON = "";
            string JSONLocation = Locations.ImgurJSON;
            using (var Stream = new FileStream(JSONLocation, FileMode.Open, FileAccess.Read))
            using (var ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            ApiLogin settings = JsonConvert.DeserializeObject<ApiLogin>(JSON);

            Client = new Reddit.RedditAPI(settings.ClientId, settings.RefreshToken, settings.ClientSecret);
        }

        public static async Task<List<Post>> GetHot(string subredditName, int limit = 15)
        {
            var sub = Client.Subreddit(subredditName);
            List<Post> posts = null;
            await Task.Run(() => posts = sub.Posts.GetHot(limit: limit));

            return posts;
        }

        public static void Poke()
        {

        }
    }
}
