using Discord;
using IqdbApi;
using IqdbApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using SauceNET;
using System.Net;
using System.Globalization;
using JikanDotNet;
using DiscordBotsList.Api.Internal;
using DiscordBotsList.Api.Internal.Queries;
using DiscordBotsList.Api.Objects;
using DiscordBotsList.Api;
using Namiko.Data;

namespace Namiko.Core.Util
{
    public static class WebUtil
    {
        public const string MalIconUrl = "https://i.imgur.com/vZL4XkO.png";
        private static readonly Jikan Jikan = new Jikan();
        private static AuthDiscordBotListApi DblApi;

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

        // SAUCENAO

        public static async Task<SauceNET.Model.Sauce> SauceNETSearchAsync(string url)
        {
            SauceNETClient client = new SauceNETClient("ea858ed6c91d925cc7d0af7ca7cfe6651fd9fb94");
            return await client.GetSauceAsync(url);
        }
        public static EmbedBuilder SauceEmbed(SauceNET.Model.Sauce sauce, string requestUrl)
        {
            var eb = new EmbedBuilder();

            string desc = "**Results:**";
            foreach(var x in sauce.Results)
            {
                desc += $"\n{x.Similarity}% - [{x.DatabaseName}]({x.SourceURL})";
            }

            eb.WithDescription(desc);
            eb.WithThumbnailUrl(requestUrl);
            eb.WithAuthor("SauceNao", "http://www.userlogos.org/files/logos/zoinzberg/SauceNAO_2.png", "https://saucenao.com/");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
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

        // GLOBAL

        public static bool IsImageUrl(string url)
        {

            try {
                var req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "HEAD";
                using (var resp = req.GetResponse()) {
                    return resp.ContentType.ToLower(CultureInfo.InvariantCulture).StartsWith("image/");
                }
            } catch (UriFormatException){ return false; }
        }
        public static bool IsValidUrl(string url)
        {
            return  Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps); 

            //return Uri.IsWellFormedUriString("https://www.google.com", UriKind.Absolute);
        }

        // ANIME SEARCH

        public static async Task<AnimeSearchResult> AnimeSearch(string Query)
        {
            //animeSearch becomes a "list" of the results of query
            return await Task.Run(() => Jikan.SearchAnime(Query).Result);
        }
        public static EmbedBuilder AnimeListEmbed(AnimeSearchResult animeSearch)
        {
            int i = 1;
            string listboo = "";
            foreach (var x in animeSearch.Results)
            {
                listboo += $"**{i++}.** {x.Title}\n";
                if (i > 7)
                    break;
            }

            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithAuthor("Anime search results. Select by typing 1-7", MalIconUrl);
            eb.WithFooter("Type `x` to cancel search.");
            eb.WithDescription(listboo);
            eb.WithThumbnailUrl(animeSearch.Results.First().ImageURL);
            return eb;
        }
        public static async Task<Anime> GetAnime(long id)
        {
            return await Task.Run(() => Jikan.GetAnime(id).Result); //Gets anime machine
        }
        public static EmbedBuilder AnimeEmbed(Anime anime)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithAuthor($"{anime.Title} ({anime.TitleJapanese})", MalIconUrl, anime.LinkCanonical);
            eb.WithFooter($"Results of anime search");
            eb.WithCurrentTimestamp(); //automatically put in footer
            eb.WithDescription("**Type:** " + anime.Type +
                "\n**Anime score:** " + anime.Score +
                "\n**Rated:** " + anime.Rating +
                "\n**Episodes:** " + anime.Episodes +
                "\n**Genres:** " + string.Join('/', anime.Genres) + "\n" + "\n" +
                anime.Synopsis);
            eb.ThumbnailUrl = anime.ImageURL;
            return eb;
        }

        // MANGA SEARCH

        public static async  Task<MangaSearchResult> MangaSearch(string Query)
        {
            //mangaSearch becomes a "list" of the results of query
            return await Task.Run(() => Jikan.SearchManga(Query).Result);
        }
        public static EmbedBuilder MangaListEmbed(MangaSearchResult mangaSearch)
        {
            int i = 1;
            string listboo = "";
            foreach(var x in mangaSearch.Results)
            {
                listboo += $"**{i++}.** {x.Title}\n";
                if (i > 7)
                    break;
            }

            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithAuthor("Manga search results. Select by typing 1-7", MalIconUrl);
            eb.WithFooter("Type `x` to cancel search.");
            eb.WithDescription(listboo);
            eb.WithThumbnailUrl(mangaSearch.Results.First().ImageURL);
            return eb;
        }
        public static async Task<Manga> GetManga(long id)
        {
            return await Task.Run(() => Jikan.GetManga(id).Result); //Gets manga machine
        }
        public static EmbedBuilder MangaEmbed(Manga manga)
        {
            string MangaState = "";
            if (manga.Status == "Publishing") MangaState = "^"; else MangaState = manga.Chapters;

            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithAuthor($"{manga.Title} ({manga.TitleJapanese})", MalIconUrl, manga.LinkCanonical);
            eb.WithFooter($"Results of manga search");
            eb.WithCurrentTimestamp();
            eb.WithDescription("**Type:** " + manga.Type +
                "\n**Manga score:** " + manga.Score +
                "\n**Status:** " + manga.Status +
                "\n**Chapters:** " + MangaState +
                "\n**Genres:** " + string.Join('/', manga.Genres) + "\n" + "\n" +
                manga.Synopsis);
            eb.ThumbnailUrl = manga.ImageURL;
            return eb;
        }
        
        // DISCORD BOTS ORG

        public static async Task<IList<IDblEntity>> GetVoters()
        {
            return await DblApi.GetVotersAsync();
        }

        public static void SetUpDbl()
        {
            try
            {
                var id = Program.GetClient().CurrentUser.Id;
                string token = File.ReadAllText(Locations.DblTokenTxt);
                DblApi = new AuthDiscordBotListApi(id, token);
            } catch
            {
                Console.WriteLine("No DBL token");
            }
        }
    }
}
