using Discord;
using IqdbApi;
using IqdbApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Namiko.Core.Web
{
    public static class IqdbUtil
    {

        public static async Task<SearchResult> SearchUrl(string url)
        {
            IIqdbClient api = new IqdbClient();
            return await api.SearchUrl(url);
        }
        public static async Task<SearchResult> SearchFile(FileStream file)
        {
            IIqdbClient api = new IqdbClient();
            return await api.SearchFile(file);
        }

        public static EmbedBuilder SourceResultEmbed(SearchResult result, string searchedUrl)
        {
            var eb = new EmbedBuilder();
            string desc = "";

            if(result.Matches[0].MatchType == IqdbApi.Enums.MatchType.Best)
            {
                desc += $"**Best Match:**\n{ListingLine(result.Matches[0])}";
            }

            if (result.Matches.Any(x => x.MatchType == IqdbApi.Enums.MatchType.Additional))
            {
                desc += $"**Additional Matches:**\n";
                foreach (var x in result.Matches.Where(x => x.MatchType == IqdbApi.Enums.MatchType.Additional))
                {
                    if(!x.Tags.Any(y => y.Contains("loli") || y.Contains("shota")) || x.Rating == IqdbApi.Enums.Rating.Safe)
                        desc += ListingLine(x);
                }
            }

            if (result.Matches.Any(x => x.MatchType == IqdbApi.Enums.MatchType.Possible))
            {
                desc += $"**Possible Matches:**\n";
                foreach (var x in result.Matches.Where(x => x.MatchType == IqdbApi.Enums.MatchType.Possible))
                {
                    if (!x.Tags.Any(y => y.Contains("loli") || y.Contains("shota")) || x.Rating == IqdbApi.Enums.Rating.Safe)
                        desc += ListingLine(x);
                }
            }

            eb.WithDescription(desc);
            eb.WithThumbnailUrl(searchedUrl);
            eb.WithAuthor("IQDB", "https://i.imgur.com/lX13yov.png", "https://iqdb.org/");
            eb.WithFooter($"Images searched: {result.SearchedImagesCount} | Took {result.SearchedInSeconds*1000}ms");
            eb.WithColor(Basic.BasicUtil.RandomColor());
            return eb;
        }
        public static string ListingLine(Match x)
        {
            return $"{x.Resolution.Width}x{x.Resolution.Height} Rating:{x.Rating} [{x.Source}]({FixUrl(x.Url)}) {x.Similarity}%\n";
        }
        public static string FixUrl(string url)
        {
            return url.StartsWith(@"//") ? url.Insert(0, "https:") : url;
        }
    }
}
