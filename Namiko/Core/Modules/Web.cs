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
    public class Web : InteractiveBase<ShardedCommandContext>
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

        [Command("Anime"), Alias("AnimeSearch", "SearchAnime"), Summary("Searchs MAL for an anime and the following details.\n**Usage**: `!Anime [anime_title]`")]
        public async Task AnimeSearch([Remainder]string Query)
        {
            var searchmsg = await Context.Channel.SendMessageAsync("`I'll humour your request...`");        //Informs the user it is being searched
            var animeSearch = await WebUtil.AnimeSearch(Query);                                             //mangaSearch becomes the result of Query (will hold a list of searched results)
            await searchmsg.DeleteAsync();

            //Quick If to see if manga had results
            if (animeSearch == null)
            {
                await Context.Channel.SendMessageAsync($"No results. Try harder.");
                return;
            }

            //Sends embed of manga titles from results
            var ResponseQuery = await ReplyAsync("", false, WebUtil.AnimeListEmbed(animeSearch).Build());

            //Sets a timeout of 20 seconds, changeable if needed
            SocketMessage UserResponse = null;
            UserResponse = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20));

            if (UserResponse.Content == "x" || UserResponse.Content == "X")
            {
                await ReplyAndDeleteAsync($"Cancelling...", timeout: TimeSpan.FromSeconds(5));
                await ResponseQuery.DeleteAsync();
                return;
            };

            long id = 0;
            try
            {
                int i = int.Parse(UserResponse.Content);
                id = animeSearch.Results.Skip(i - 1).FirstOrDefault().MalId;
            }
            catch
            {
                //response is not a number
                await ResponseQuery.DeleteAsync();
                await ReplyAsync("That's not 1-7, try harder next time.");
                return;
            }

            if (UserResponse != null)
            {
                var endAnime = await WebUtil.GetAnime(id); //EndManga becomes the manga, it uses ID to get propa page umu
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync("", false, WebUtil.AnimeEmbed(endAnime).Build());
            }

            await ResponseQuery.DeleteAsync();
        }

        [Command("Manga"), Alias("MangaSearch", "SearchManga"), Summary("Searchs MAL for an manga and the following details.\n**Usage**: `!Manga [manga_title]`")]
        public async Task MangaSearch([Remainder]string Query)
        {
            var searchmsg = await Context.Channel.SendMessageAsync("`I'll humour your request...`");        //Informs the user it is being searched
            var mangaSearch = await WebUtil.MangaSearch(Query);                                             //mangaSearch becomes the result of Query (will hold a list of searched results)
            await searchmsg.DeleteAsync();

            //Quick If to see if manga had results
            if (mangaSearch == null)
            {
                await Context.Channel.SendMessageAsync($"No results. Try harder.");
                return;
            }

            //Sends embed of manga titles from results
            var ResponseQuery = await ReplyAsync("", false, WebUtil.MangaListEmbed(mangaSearch).Build());

            //Sets a timeout of 20 seconds, changeable if needed
            SocketMessage UserResponse = null;
            UserResponse = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20));                       

            if (UserResponse.Content == "x" || UserResponse.Content == "X")
            {
                await ReplyAndDeleteAsync($"Cancelling...", timeout: TimeSpan.FromSeconds(5));
                await ResponseQuery.DeleteAsync();
                return;
            };

            long id = 0;
            try
            {
                int i = int.Parse(UserResponse.Content);
                id = mangaSearch.Results.Skip(i - 1).FirstOrDefault().MalId;
            }
            catch 
            {
                //response is not a number
                await ResponseQuery.DeleteAsync();
                await ReplyAsync("That's not 1-7, try harder next time.");
                return;
            }

            if (UserResponse != null)
            {
                await Context.Channel.TriggerTypingAsync();
                var EndManga = await WebUtil.GetManga(id); //EndManga becomes the manga, it uses ID to get propa page umu
                await Context.Channel.SendMessageAsync("", false, WebUtil.MangaEmbed(EndManga).Build());
            }

            await ResponseQuery.DeleteAsync();
        }

        [Command("Animemes"), Summary("Sets a channel where Namiko will post popular anime memes.\n**Usage**: `!Animemes`"), CustomUserPermission(GuildPermission.ManageChannels), RequireContext(ContextType.Guild)]
        public async Task Animemes([Remainder] string str = "")
        {
            if(SpecialChannelDb.IsType(Context.Channel.Id, ChannelType.Animemes))
            {
                await SpecialChannelDb.Delete(Context.Channel.Id, ChannelType.Animemes);
                await Context.Channel.SendMessageAsync("I'll stop posting anime memes here. You're no fun...");
            }
            else
            {
                await SpecialChannelDb.AddChannel(Context.Channel.Id, ChannelType.Animemes, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("Channel set as an anime memes channel! I will post popular memes here!");
            }
        }
    }
}