using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Criteria;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Reddit.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;

namespace Namiko
{
    [Name("Web Services")]
    public class Web : CustomModuleBase<ICustomContext>
    {
        [Command("IQDB"), Description("Finds the source of an image in iqdb.\n**Usage**: `!iqdb [image_url]` or `!iqdb` with attached image.")]
        public async Task Iqdb(string url)
            => await Iqdb(url ?? ((ICommandContext)Context).Message.Attachments.FirstOrDefault()?.Url);
        //[SlashCommand("iqdb", "Finds the source of an image in iqdb")]
        public async Task Iqdb2(string url)
        {
            await Context.TriggerTypingAsync();
            if (url == null)
            {
                await ReplyAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            var result = await WebUtil.IqdbUrlSearchAsync(url);

            if (!result.IsFound)
            {
                await ReplyAsync("No results. Too bad.");
                return;
            }

            await ReplyAsync("", false, WebUtil.IqdbSourceResultEmbed(result, url).Build());
        }

        [SlashCommand("sauce", "Finds the source of an image with SauceNao")]
        public Task SauceNaoAttachment(IAttachment file) =>
            SauceNaoCmd(file.Url);

        [Command("Source"), Alias("SauceNao", "Sauce"), Description("Finds the source of an image with SauceNao.\n**Usage**: `!source [image_url]` or `!source` with attached image.")]
        public Task SauceNao(string url = null) => 
            SauceNaoCmd(url ?? ((ICommandContext)Context)?.Message?.Attachments?.FirstOrDefault()?.Url);

        public async Task SauceNaoCmd(string url)
        {
            await Context.TriggerTypingAsync();

            if (url == null)
            {
                await ReplyAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            var sauce = await WebUtil.SauceNETSearchAsync(url);

            if (sauce.Request.Status != 0)
            {
                await ReplyAsync($"An error occured. Server response: `{sauce.Message}`");
                return;
            }

            for (int i = sauce.Results.Count - 1; i >= 0; i--)
            {
                if (Double.Parse(sauce.Results[i].Similarity) < 42)
                    sauce.Results.RemoveAt(i);
            }

            if (sauce.Results.Count == 0)
            {
                await ReplyAsync("No matches. Sorry~");
                return;
            }

            await ReplyAsync("", false, WebUtil.SauceEmbed(sauce, url).Build());
        }
        

        public enum Mal { Anime, Manga, Waifu }

        [SlashCommand("myanimelist", "Search MyAnimeList database")]
        public Task MalSearch(Mal searchType, string search) => searchType switch
        {
            Mal.Anime => AnimeSearch(search),
            Mal.Manga => MangaSearch(search),
            Mal.Waifu => WaifuSearch(search)
        };
        
        [Command("Anime"), Alias("AnimeSearch", "SearchAnime"), Description("Searches MAL for an anime and the following details.\n**Usage**: `!Anime [anime_title]`")]
        public async Task AnimeSearch([Remainder]string Query)
        {
            await Context.TriggerTypingAsync();
            var animeSearch = await WebUtil.AnimeSearch(Query);

            //Quick If to see if manga had results
            if (animeSearch == null || animeSearch.Results == null || animeSearch.Results.Count <= 0)
            {
                await ReplyAsync($"Gomen, senpai... No results.");
                return;
            }

            var response = await Select(animeSearch.Results, "Result", WebUtil.AnimeListEmbed(animeSearch).Build());

            await Context.TriggerTypingAsync();
            var anime = await WebUtil.GetAnime(response.MalId); //EndManga becomes the manga, it uses ID to get propa page umu

            await ReplyAsync(WebUtil.AnimeEmbed(anime).Build());
        }

        [Command("Manga"), Alias("MangaSearch", "SearchManga"), Description("Searches MAL for an manga and the following details.\n**Usage**: `!Manga [manga_title]`")]
        public async Task MangaSearch([Remainder]string Query)
        {
            await Context.TriggerTypingAsync();
            var mangaSearch = await WebUtil.MangaSearch(Query);

            //Quick If to see if manga had results
            if (mangaSearch == null || mangaSearch.Results == null || mangaSearch.Results.Count <= 0)
            {
                await ReplyAsync($"Gomen, Senpai... No results.");
                return;
            }

            var response = await Select(mangaSearch.Results, "Result", WebUtil.MangaListEmbed(mangaSearch).Build());

            await Context.TriggerTypingAsync();
            var manga = await WebUtil.GetManga(response.MalId); //EndManga becomes the manga, it uses ID to get propa page umu

            await ReplyAsync(WebUtil.MangaEmbed(manga).Build());
        }
        
        [Command("MALWaifu"), Alias("malw"), Description("Searches MAL for characters.\n**Usage**: `!malw [query]`")]
        public async Task WaifuSearch([Remainder] string name)
        {
            await Context.TriggerTypingAsync();
            var eb = new EmbedBuilderPrepared(Context.User);

            var waifus = await WebUtil.GetWaifus(name);
            string list = "";
            foreach (var w in waifus.Results.Take(10))
            {
                list += $"`{w.MalId}` [{w.Name}]({w.URL}) - *{(w.Animeography.FirstOrDefault() == null ? w.Mangaography.FirstOrDefault().Name : w.Animeography.FirstOrDefault().Name)}*\n";
            }
            eb.WithDescription(list);

            await ReplyAsync(embed: eb.Build());
        }


        [RequireGuild]
        [UserPermission(GuildPermission.ManageChannels)]
        [Command("Subreddit"), Alias("sub"), Description("Set a subreddit for Namiko to post hot posts from.\n**Usage**: `!sub [subreddit_name] [min_upvotes]`")]
        [SlashCommand("reddit-subscribe", "Set a subreddit for Namiko to post hot posts from")]
        public async Task Subreddit(string subredditName, int minimumUpvotes)
        {
            var subs = SpecialChannelDb.GetChannelsByGuild(Context.Guild.Id, Model.ChannelType.Reddit);
            int limit = 1;
            if (PremiumDb.IsPremium(Context.Guild.Id, ProType.Guild))
                limit = 5;
            if (PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
                limit = 10;

            if (subs.Count() >= limit)
            {
                await ReplyAsync($"Limit {limit} subscription per guild. Upgrade server to increase the limit! `{TextCommandService.GetPrefix(Context)}Pro`", embed: WebUtil.SubListEmbed(Context.Guild.Id).Build());
                return;
            }

            if (minimumUpvotes < 100)
            {
                await ReplyAsync("Upvote minimum must be at least 100.");
                return;
            }

            await Context.TriggerTypingAsync();
            Subreddit sub = null;
            try
            {
                sub = await RedditAPI.GetSubreddit(subredditName);
            } catch
            {
                await ReplyAsync($"Subreddit **{subredditName}** not found.");
                return;
            }
            if (sub.Subscribers < 20000)
            {
                await ReplyAsync("The Subreddit must have at least **20,000** subscribers.\n" +
                    $"**{sub.Name}** has **{sub.Subscribers?.ToString("n0")}**.");
                return;
            }
            try
            {
                if (sub.Over18.Value && !((SocketTextChannel)Context.Channel).IsNsfw)
                {
                    await ReplyAsync($"**{sub.Name}** is a NSFW subreddit. This channel must be marked as NSFW.");
                    return;
                }
            } catch { }

            var old = subs.FirstOrDefault(x => x.ChannelId == Context.Channel.Id && x.Args.Split(",")[0].Equals(sub.Name));
            if(old != null)
            {
                await SpecialChannelDb.Delete(old);
            }

            await SpecialChannelDb.AddChannel(Context.Channel.Id, Model.ChannelType.Reddit, Context.Guild.Id, sub.Name + "," + minimumUpvotes);
            await ReplyAsync(embed: WebUtil.SubredditSubscribedEmbed(sub, minimumUpvotes).Build());
        }

        [RequireGuild]
        [UserPermission(GuildPermission.ManageChannels)]
        [Command("Unsubscribe"), Alias("unsub"), Description("Unsubscribe from a subreddit.\n**Usage**: `!unsub [subreddit_name]`")]
        [SlashCommand("reddit-unsubscribe", "Unsubscribe from a subreddit")]
        public async Task Unsubscribe(string subredditName)
        {
            await Context.TriggerTypingAsync();

            var subs = SpecialChannelDb.GetChannelsByGuild(Context.Guild.Id, Model.ChannelType.Reddit);
            var olds = subs.Where(x => x.Args.Split(",")[0].Equals(subredditName, StringComparison.OrdinalIgnoreCase));
            if (!olds.Any())
            {
                await ReplyAsync($"Subreddit **{subredditName}** not found. Try `{TextCommandService.GetPrefix(Context)}sublist` for a list of your subreddits.");
            }
            foreach (var old in olds)
            {
                await SpecialChannelDb.Delete(old);
                await ReplyAsync($"Unsubscribed from **{subredditName}**. Were their posts not good enough?");
            }
        }

        [RequireGuild]
        [Command("SubList"), Description("Subreddits you are subscribed to.\n**Usage**: `!sublist`")]
        [SlashCommand("reddit-list", "Subreddits you are subscribed to")]
        public async Task SubList()
        {
            await ReplyAsync(embed: WebUtil.SubListEmbed(Context.Guild.Id).Build());
        }
    }
}