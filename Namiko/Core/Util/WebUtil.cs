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

        private static Jikan jikan = new Jikan();

        public static AnimeSearchResult AnimeSearch(string Query)
        {
            AnimeSearchResult animeSearch = jikan.SearchAnime(Query).Result; //animeSearch becomes a "list" of the results of query
            return animeSearch;
        }

        public static EmbedBuilder SearchedAnimeList(AnimeSearchResult animeSearch)
        {
            List<string> AnimeList = new List<string>();
            foreach (var anime in animeSearch.Results) { AnimeList.Add(anime.Title); }

            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            eb.Title = "List of anime found. Respond with 1-7 to search title";
            eb.WithFooter("Results");

            var QuiccList = new List<string>();
            for (int i = 0; i < 7; i++)
            {
                int Speciali = i + 1; //Otherwise it would show 0-6
                QuiccList.Add($"{Speciali}. " + AnimeList[i]);
            }
            string listboo = string.Join('\n', QuiccList);
            eb.WithDescription(listboo);
            return eb;
        }

        public static Anime GetAnime(long id)
        {
            return jikan.GetAnime(id).Result; //Gets anime machine
        }

        public static EmbedBuilder AnimeSearchEmbed(Anime EndAnime)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());

            eb.Title = $"**{EndAnime.Title}** ({EndAnime.TitleJapanese})  *{EndAnime.LinkCanonical}*"; // LinkCanonical is page link
            eb.WithFooter($"Results of anime search");
            eb.WithCurrentTimestamp(); //automatically put in footer
            eb.WithDescription("Type: " + EndAnime.Type +
                "\nAnime score: " + EndAnime.Score +
                "\nRated: " + EndAnime.Rating +
                "\nEpisodes: " + EndAnime.Episodes +
                "\nGenres: " + string.Join('/', EndAnime.Genres) + "\n" + "\n" +
                EndAnime.Synopsis);
            eb.ImageUrl = EndAnime.ImageURL;
            return eb;
        }

        // MANGA SEARCH

        public static MangaSearchResult MangaSearch(string Query)
        {
            MangaSearchResult mangaSearch = jikan.SearchManga(Query).Result; //mangaSearch becomes a "list" of the results of query
            return mangaSearch;
        }

        public static EmbedBuilder SearchedMangaList(MangaSearchResult mangaSearch)
        {
            List<string> MangaList = new List<string>();
            foreach (var manga in mangaSearch.Results) { MangaList.Add(manga.Title); }

            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            eb.Title = "List of manga found. Respond with 1-7 to search title";
            eb.WithFooter("Results | Respond with `x` to cancel search.");

            var QuiccList = new List<string>();
            for (int i = 0; i < 7; i++)
            {
                int Speciali = i + 1; //Otherwise it would show 0-6
                QuiccList.Add($"{Speciali}. " + MangaList[i]);
            }
            string listboo = string.Join('\n', QuiccList);
            eb.WithDescription(listboo);
            return eb;
        }

        public static Manga GetManga(long id)
        {
            return jikan.GetManga(id).Result; //Gets manga machine
        }

        public static EmbedBuilder MangaSearchEmbed(Manga EndManga)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            string MangaState = "";
            if (EndManga.Status == "Publishing") MangaState = "^"; else MangaState = EndManga.Chapters;

            eb.Title = $"**{EndManga.Title}** ({EndManga.TitleJapanese})  *{EndManga.LinkCanonical}*";
            eb.WithFooter($"Results of manga search");
            eb.WithCurrentTimestamp();
            eb.WithDescription("Type: " + EndManga.Type +
                "\nManga score: " + EndManga.Score +
                "\nStatus: " + EndManga.Status +
                "\nChapters: " + MangaState +
                "\nGenres: " + string.Join('/', EndManga.Genres) + "\n" + "\n" +
                EndManga.Synopsis);
            eb.ImageUrl = EndManga.ImageURL;
            return eb;
        }
    }
}
