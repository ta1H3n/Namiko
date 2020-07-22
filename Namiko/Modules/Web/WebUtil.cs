using Discord;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;
using IqdbApi;
using IqdbApi.Models;
using JikanDotNet;
using Model;
using Namiko.Data;
using Reddit.Controllers;
using SauceNET;
using SauceNET.Model;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Namiko
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

            if (result.Matches[0].MatchType == IqdbApi.Enums.MatchType.Best)
            {
                desc += $"**Best Match:**\n{IqdbListingLine(result.Matches[0])}";
            }

            if (result.Matches.Any(x => x.MatchType == IqdbApi.Enums.MatchType.Additional))
            {
                desc += $"**Additional Matches:**\n";
                foreach (var x in result.Matches.Where(x => x.MatchType == IqdbApi.Enums.MatchType.Additional))
                {
                    if (!x.Tags.Any(y => y.Contains("loli") || y.Contains("shota")) || x.Rating == IqdbApi.Enums.Rating.Safe)
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
            eb.WithFooter($"Images searched: {result.SearchedImagesCount} | Took {result.SearchedInSeconds * 1000}ms");
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

        public static async Task<Sauce> SauceNETSearchAsync(string url)
        {
            SauceNETClient client = new SauceNETClient(Config.SauceNaoApi);
            return await client.GetSauceAsync(url);
        }
        public static EmbedBuilder SauceEmbed(Sauce sauce, string requestUrl)
        {
            var eb = new EmbedBuilder();

            string desc = "**Results:**\n";
            foreach (var x in sauce.Results)
            {
                desc += $"• {x.Similarity}% - [{x.DatabaseName}]({x.SourceURL ?? x.InnerSource})";

                if (x.InnerSource != null && x.SourceURL != null)
                {
                    if (IsValidUrl(x.InnerSource))
                    {
                        desc += $" -> [{GetDomainFromUrl(x.InnerSource)}]({x.InnerSource})\n";
                    }
                    else
                    {
                        desc += $" -> {x.InnerSource}\n";
                    }
                }

                if (!desc.EndsWith("\n"))
                {
                    desc += "\n";
                }

                x.ExtUrls.Remove(x.InnerSource);
                x.ExtUrls.Remove(x.SourceURL);
                if (x.ExtUrls.Count > 0)
                {
                    desc += $"Extra: ";
                    foreach (var url in x.ExtUrls)
                    {
                        if (IsValidUrl(url))
                        {
                            desc += $"[{GetDomainFromUrl(url)}]({url}) ";
                        }
                    }
                    desc += "\n";
                }
            }

            eb.WithDescription(desc);
            eb.WithThumbnailUrl(requestUrl);
            eb.WithAuthor("SauceNao", "http://www.userlogos.org/files/logos/zoinzberg/SauceNAO_2.png", "https://saucenao.com/");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }

        // GLOBAL

        public static string GetDomainFromUrl(string url)
        {
            Uri myUri = new Uri(url);
            string host = myUri.Host;
            host = host.Replace("www.", "");
            host = host.Replace(".com", "");
            host = host.Replace(".net", "");
            return host;
        }

        public static bool IsImageUrl(string url)
        {

            try {
                var req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "HEAD";
                using var resp = req.GetResponse();
                return resp.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            } catch (UriFormatException){ return false; }
        }
        public static bool IsValidUrl(string url)
        {
            return  Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps); 

            //return Uri.IsWellFormedUriString("https://www.google.com", UriKind.Absolute);
        }

        // MAL

        // Anime
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
            eb.WithFooter("Times out in 23 seconds");
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
            string desc = "**Type:** " + anime.Type +
                "\n**Anime score:** " + anime.Score +
                "\n**Rated:** " + anime.Rating +
                "\n**Episodes:** " + anime.Episodes +
                "\n**Genres:** " + string.Join('/', anime.Genres) + "\n" + "\n" +
                anime.Synopsis;
            eb.WithDescription(desc.Length > 2040 ? desc.Substring(0, 2030) + "..." : desc);
            eb.ThumbnailUrl = anime.ImageURL;
            return eb;
        }

        // Manga
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
            eb.WithFooter("Times out in 23 seconds");
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
            string MangaState;
            if (manga.Status == "Publishing") MangaState = "^"; else MangaState = manga.Chapters;

            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithAuthor($"{manga.Title} ({manga.TitleJapanese})", MalIconUrl, manga.LinkCanonical);
            eb.WithFooter($"Results of manga search");
            eb.WithCurrentTimestamp();
            string desc = "**Type:** " + manga.Type +
                "\n**Manga score:** " + manga.Score +
                "\n**Status:** " + manga.Status +
                "\n**Chapters:** " + MangaState +
                "\n**Genres:** " + string.Join('/', manga.Genres) + "\n" + "\n" +
                manga.Synopsis;
            eb.WithDescription(desc.Length > 2040 ? desc.Substring(0, 2030) + "..." : desc);
            eb.ThumbnailUrl = manga.ImageURL;
            return eb;
        }

        //Character
        public static async Task<Character> GetWaifu(long malId)
        {
            return await Jikan.GetCharacter(malId);
        }
        public static async Task<CharacterSearchResult> GetWaifus(string name)
        {
            return await Jikan.SearchCharacter(name);
        }
        
        // DISCORD BOTS ORG

        public static async Task<IList<IDblEntity>> GetVotersAsync()
        {
            try
            {
                return await DblApi.GetVotersAsync();
            } catch (Exception ex)
            {
                SetUpDbl(Program.GetClient().CurrentUser.Id);
                SentrySdk.CaptureException(ex);
                throw ex;
            }
        }
        public static void SetUpDbl(ulong id)
        {
            try
            {
                string token = File.ReadAllText(Locations.DblTokenTxt);
                DblApi = new AuthDiscordBotListApi(id, token);
            } catch
            {
                Console.WriteLine("No DBL token");
            }
        }
        public static void UpdateGuildCount(int amount)
        {
            Task.Run(() => DblApi.UpdateStats(amount));
        }

        // REDDIT

        public static EmbedBuilder SubredditSubscribedEmbed(Subreddit sub, int upvotes)
        {
            var eb = new EmbedBuilder();

            eb.WithAuthor(sub.Title, sub.CommunityIcon, "https://www.reddit.com" + sub.URL);
            try { eb.WithImageUrl(sub.BannerImg); } catch { }
            eb.WithDescription($"Subscribed to hot posts from **{sub.Name}** that reach **{upvotes}** or more upvotes.");
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter($"Type `!unsub {sub.Name}` to unsubscribe.");

            return eb;
        }

        public static EmbedBuilder SubListEmbed(ulong guildId)
        {
            var eb = new EmbedBuilder();
            var subs = SpecialChannelDb.GetChannelsByGuild(guildId, Model.ChannelType.Reddit);

            string desc = "";
            int i = 1;
            foreach(var sub in subs)
            {
                string[] args = sub.Args.Split(",");
                desc += $"{i++}. *{args[0]}* - **{args[1]}** upvotes\n";
            }

            eb.WithDescription(desc == "" ? "-" : desc);
            eb.WithAuthor("Subreddits subscribed in this server");
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter($"Type `{Program.GetPrefix(guildId)}unsub [name]` to unsubscribe.");
            return eb;
        }
    }
}
