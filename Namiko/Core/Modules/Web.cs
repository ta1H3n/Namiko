using System.Threading.Tasks;
using System.Linq;

using Discord.Commands;
using Discord;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System.Collections.Generic;
using System;
using Namiko.Resources.Preconditions;
using Discord.WebSocket;
using Namiko.Core.Util;
using Discord.Addons.Interactive;

namespace Namiko.Core.Modules
{
    public class Web : InteractiveBase<SocketCommandContext>
    {
        [Command("IQDB"), Summary("Finds the source of an image in iqdb.\n**Usage**: `!iqdb [image_url]` or `!iqdb` with attached image.")]
        public async Task Iqdb(string url = "", [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();

            url = !url.Equals("") ? url : Context.Message.Attachments.FirstOrDefault()?.Url;

            if (url == null)
            {
                await Context.Channel.SendMessageAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            var result = await WebUtil.IqdbUrlSearchAsync(url);

            if (!result.IsFound)
            {
                await Context.Channel.SendMessageAsync("No results. Too bad.");
                return;
            }

            await Context.Channel.SendMessageAsync("", false, WebUtil.IqdbSourceResultEmbed(result, url).Build());
        }

        [Command("Source"), Alias("SauceNao", "Sauce"), Summary("Finds the source of an image with SauceNao.\n**Usage**: `!source [image_url]` or `!source` with attached image.")]
        public async Task SaceNao(string url = null, [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();

            url = !url.Equals("") ? url : Context.Message.Attachments.FirstOrDefault()?.Url;

            if (url == null)
            {
                await Context.Channel.SendMessageAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            var sauce = await WebUtil.SauceNETSearchAsync(url);

            if (sauce.Request.Status != 0)
            {
                await Context.Channel.SendMessageAsync($"An error occured. Server response: `{sauce.Message}`");
                return;
            }

            for (int i = sauce.Results.Count - 1; i >= 0; i--)
            {
                if (Double.Parse(sauce.Results[i].Similarity) < 42)
                    sauce.Results.RemoveAt(i);
            }

            if (sauce.Results.Count == 0)
            {
                await Context.Channel.SendMessageAsync("No matches. Sorry~");
                return;
            }

            await Context.Channel.SendMessageAsync("", false, WebUtil.SauceEmbed(sauce, url).Build());
        }

        [Command("Anime", RunMode = RunMode.Async), Alias("AnimeSearch", "SearchAnime"), Summary("Searchs MAL for an anime and the following details.\n**Usage**: `!Anime [anime_title]`")]
        public async Task AnimeSearch([Remainder]string Query)
        {
            var searchmsg = await Context.Channel.SendMessageAsync("`I'll humour your request...`");  //Informs the user it is being searched
            var animeSearch = WebUtil.AnimeSearch(Query); //animeSearch becomes the result of Query (will hold a list of searched results)

            if (animeSearch == null) { await searchmsg.DeleteAsync(); await Context.Channel.SendMessageAsync($"Even after humouring your request, you made **me** search for {Query}?! You're lucky there was no results..."); return; } //Quick If to see if anime had results

            await searchmsg.DeleteAsync(); //Unclutter chat
            var ResponseQuery = await ReplyAsync("", false, WebUtil.SearchedAnimeList(animeSearch).Build()); //Sends embed of anime titles from results
            SocketMessage UserResponse = null; //umu
            UserResponse = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20)); //Sets a timeout of 20 seconds, changable if needed
            long id = 0;
            if (UserResponse.Content == "x" || UserResponse.Content == "X") { await ReplyAndDeleteAsync($"I've cancelled your request for {Query}", timeout: TimeSpan.FromSeconds(5)); await ResponseQuery.DeleteAsync(); return; };

            try
            {
                List<long> AnimeListId = new List<long>();
                foreach (var anime in animeSearch.Results) { AnimeListId.Add(anime.MalId); }
                for (int i = 0; i < 50; i++) //50 since thats how many results MAL gives
                {
                    if (Int64.Parse(UserResponse.Content) == i + 1)
                    {
                        id = AnimeListId[i];
                    }
                }
            }
            catch
            {
                //response is not a number
                await ResponseQuery.DeleteAsync();
                await ReplyAsync("I underestimated your ability to respond to simple options, sorry... I'll just cancel the search...");
                return;
            }

            var EndAnime = WebUtil.GetAnime(id); //EndAnime becomes the anime, it uses ID to get propa page umu
            if (UserResponse != null)
            {
                await Context.Channel.SendMessageAsync("", false, WebUtil.AnimeSearchEmbed(EndAnime).Build());
            }
            else
            {
                //If the user did not respond before the timeout
                await ReplyAsync("Can you not even think about which option to pick? I'll just cancel the search...");
                await ResponseQuery.DeleteAsync();
            }

        }

        [Command("Manga", RunMode = RunMode.Async), Alias("MangaSearch", "SearchManga"), Summary("Searchs MAL for an manga and the following details.\n**Usage**: `!Manga [manga_title]`")]
        public async Task MangaSearch([Remainder]string Query)
        {
            var searchmsg = await Context.Channel.SendMessageAsync("`I'll humour your request...`");  //Informs the user it is being searched
            var mangaSearch = WebUtil.MangaSearch(Query); //mangaSearch becomes the result of Query (will hold a list of searched results)

            if (mangaSearch == null) { await searchmsg.DeleteAsync(); await Context.Channel.SendMessageAsync($"Even after humouring your request, you made **me** search for {Query}?! You're lucky there was no results..."); return; } //Quick If to see if manga had results

            await searchmsg.DeleteAsync(); //Unclutter chat
            var ResponseQuery = await ReplyAsync("", false, WebUtil.SearchedMangaList(mangaSearch).Build()); //Sends embed of manga titles from results
            SocketMessage UserResponse = null; //umu
            UserResponse = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20)); //Sets a timeout of 20 seconds, changable if needed
            long id = 0;
            if (UserResponse.Content == "x" || UserResponse.Content == "X") { await ReplyAndDeleteAsync($"I've cancelled your request for {Query}", timeout: TimeSpan.FromSeconds(5)); await ResponseQuery.DeleteAsync(); return; };
            try
            {
                List<long> MangaListId = new List<long>();
                foreach (var manga in mangaSearch.Results) { MangaListId.Add(manga.MalId); }
                for (int i = 0; i < 50; i++) //50 since thats how many results MAL gives
                {
                    if (Int64.Parse(UserResponse.Content) == i + 1)
                    {
                        id = MangaListId[i];
                    }
                }
            }
            catch
            {
                //response is not a number
                await ResponseQuery.DeleteAsync();
                await ReplyAsync("I underestimated your ability to respond to simple options, sorry... I'll just cancel the search...");
                return;
            }

            var EndManga = WebUtil.GetManga(id); //EndManga becomes the manga, it uses ID to get propa page umu
            if (UserResponse != null)
            {
                await Context.Channel.SendMessageAsync("", false, WebUtil.MangaSearchEmbed(EndManga).Build());
            }
            else
            {
                //If the user did not respond before the timeout
                await ReplyAsync("Can you not even think about which option to pick? I'll just cancel the search...");
                await ResponseQuery.DeleteAsync();
            }
        }
    }
}