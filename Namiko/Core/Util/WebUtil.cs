using Discord;
using IqdbApi;
using IqdbApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Namiko.Core.Util
{
    public static class WebUtil
    {
        // IQDB

        public static async Task<SearchResult> IqdbUrlSearchAsync(string url)
        {
            IIqdbClient api = new IqdbClient();
            return await api.SearchUrl(url);
        }
        public static async Task<SearchResult> IqdbFileSearchAsync(FileStream file)
        {
            IIqdbClient api = new IqdbClient();
            return await api.SearchFile(file);
        }

        public static EmbedBuilder IqdbSourceResultEmbed(SearchResult result, string searchedUrl)
        {
            var eb = new EmbedBuilder();
            string desc = "";

            if(result.Matches[0].MatchType == IqdbApi.Enums.MatchType.Best)
            {
                desc += $"**Best Match:**\n{IqdbListingLine(result.Matches[0])}";
            }

            if (result.Matches.Any(x => x.MatchType == IqdbApi.Enums.MatchType.Additional))
            {
                desc += $"**Additional Matches:**\n";
                foreach (var x in result.Matches.Where(x => x.MatchType == IqdbApi.Enums.MatchType.Additional))
                {
                    if(!x.Tags.Any(y => y.Contains("loli") || y.Contains("shota")) || x.Rating == IqdbApi.Enums.Rating.Safe)
                        desc += IqdbListingLine(x);
                }
            }

            if (result.Matches.Any(x => x.MatchType == IqdbApi.Enums.MatchType.Possible))
            {
                desc += $"**Possible Matches:**\n";
                foreach (var x in result.Matches.Where(x => x.MatchType == IqdbApi.Enums.MatchType.Possible))
                {
                    if (!x.Tags.Any(y => y.Contains("loli") || y.Contains("shota")) || x.Rating == IqdbApi.Enums.Rating.Safe)
                        desc += IqdbListingLine(x);
                }
            }

            eb.WithDescription(desc);
            eb.WithThumbnailUrl(searchedUrl);
            eb.WithAuthor("IQDB", "https://i.imgur.com/lX13yov.png", "https://iqdb.org/");
            eb.WithFooter($"Images searched: {result.SearchedImagesCount} | Took {result.SearchedInSeconds*1000}ms");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
        public static string IqdbListingLine(Match x)
        {
            return $"{x.Resolution.Width}x{x.Resolution.Height} Rating:{x.Rating} [{x.Source}]({IqdbFixUrl(x.Url)}) {x.Similarity}%\n";
        }
        public static string IqdbFixUrl(string url)
        {
            return url.StartsWith(@"//") ? url.Insert(0, "https:") : url;
        }

        // BOORU

        public async static void BooruTest()
        {
            var booru = new Booru.Net.BooruClient();
            var kona = await booru.GetKonaChanImagesAsync("zerotwo", "order:score");
            foreach (var x in kona)
            {
                Console.WriteLine($"PostURL:   {x.PostUrl}");
                Console.WriteLine($"Rating:    {x.Rating}");
                Console.WriteLine($"Score:     {x.Score}");
                Console.WriteLine($"Tags:      ");
                foreach (var t in x.Tags)
                    Console.Write(t + " ");

                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("Count: " + kona.Count);
        }
    }
}
