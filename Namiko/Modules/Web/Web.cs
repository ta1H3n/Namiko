﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Reddit.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    [Name("Web Services")]
    public class Web : InteractiveBase<ShardedCommandContext>
    {
        [Command("IQDB"), Summary("Finds the source of an image in iqdb.\n**Usage**: `!iqdb [image_url]` or `!iqdb` with attached image.")]
        public async Task Iqdb(string url = null, [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();

            url ??= Context.Message.Attachments.FirstOrDefault()?.Url;

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
        public async Task SauceNao(string url = null, [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();

            url ??= Context.Message.Attachments.FirstOrDefault()?.Url;

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

        [Command("Anime"), Alias("AnimeSearch", "SearchAnime"), Summary("Searches MAL for an anime and the following details.\n**Usage**: `!Anime [anime_title]`")]
        public async Task AnimeSearch([Remainder]string Query)
        {
            await Context.Channel.TriggerTypingAsync();
            var animeSearch = await WebUtil.AnimeSearch(Query);

            //Quick If to see if manga had results
            if (animeSearch == null || animeSearch.Results == null || animeSearch.Results.Count <= 0)
            {
                await Context.Channel.SendMessageAsync($"Gomen, Senpai... No results.");
                return;
            }

            //Sends embed of manga titles from results
            var listMsg = await ReplyAsync("", false, WebUtil.AnimeListEmbed(animeSearch).Build());

            //Sets a timeout of 20 seconds, changeable if needed
            var response = await NextMessageAsync(
                new Criteria<IMessage>()
                    .AddCriterion(new EnsureSourceUserCriterion())
                    .AddCriterion(new EnsureSourceChannelCriterion())
                    .AddCriterion(new EnsureRangeCriterion(7, Program.GetPrefix(Context))),
                new TimeSpan(0, 0, 23));

            long id;
            try
            {
                int i = int.Parse(response.Content);
                id = animeSearch.Results.Skip(i - 1).FirstOrDefault().MalId;
            }
            catch
            {
                _ = Context.Message.DeleteAsync();
                return;
            }
            _ = response.DeleteAsync();

            if (response != null)
            {
                await Context.Channel.TriggerTypingAsync();
                var endAnime = await WebUtil.GetAnime(id); //EndManga becomes the manga, it uses ID to get propa page umu
                await listMsg.ModifyAsync(x => x.Embed = WebUtil.AnimeEmbed(endAnime).Build());
            }
        }

        [Command("Manga"), Alias("MangaSearch", "SearchManga"), Summary("Searches MAL for an manga and the following details.\n**Usage**: `!Manga [manga_title]`")]
        public async Task MangaSearch([Remainder]string Query)
        {
            await Context.Channel.TriggerTypingAsync();
            var mangaSearch = await WebUtil.MangaSearch(Query);

            //Quick If to see if manga had results
            if (mangaSearch == null || mangaSearch.Results == null || mangaSearch.Results.Count <= 0)
            {
                await Context.Channel.SendMessageAsync($"Gomen, Senpai... No results.");
                return;
            }

            //Sends embed of manga titles from results
            var listMsg = await ReplyAsync("", false, WebUtil.MangaListEmbed(mangaSearch).Build());

            var response = await NextMessageAsync(
                new Criteria<IMessage>()
                    .AddCriterion(new EnsureSourceUserCriterion())
                    .AddCriterion(new EnsureSourceChannelCriterion())
                    .AddCriterion(new EnsureRangeCriterion(7, Program.GetPrefix(Context))),
                new TimeSpan(0, 0, 23));

            long id;
            try
            {
                int i = int.Parse(response.Content);
                id = mangaSearch.Results.Skip(i - 1).FirstOrDefault().MalId;
            }
            catch
            {
                _ = Context.Message.DeleteAsync();
                return;
            }
            _ = response.DeleteAsync();

            if (mangaSearch != null)
            {
                await Context.Channel.TriggerTypingAsync();
                var endManga = await WebUtil.GetManga(id); //EndManga becomes the manga, it uses ID to get propa page umu
                await listMsg.ModifyAsync(x => x.Embed = WebUtil.MangaEmbed(endManga).Build());
            }
        }
        
        [Command("MALWaifu"), Alias("malw"), Summary("Searches MAL for characters.\n**Usage**: `!malw [query]`"), Insider]
        public async Task AutocompleteWaifu([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var eb = new EmbedBuilderPrepared(Context.User);

            var waifus = await WebUtil.GetWaifus(name);
            string list = "";
            foreach (var w in waifus.Results.Take(10))
            {
                list += $"`{w.MalId}` [{w.Name}]({w.URL}) - *{(w.Animeography.FirstOrDefault() == null ? w.Mangaography.FirstOrDefault().Name : w.Animeography.FirstOrDefault().Name)}*\n";
            }
            eb.WithDescription(list);

            await Context.Channel.SendMessageAsync(embed: eb.Build());
        }

        [RequireGuild]
        [Command("Subreddit"), Alias("sub"), Summary("Set a subreddit for Namiko to post hot posts from.\n**Usage**: `!sub [subreddit_name] [min_upvotes]`"), CustomUserPermission(GuildPermission.ManageChannels)]
        public async Task Subreddit(string name, int upvotes)
        {
            var subs = SpecialChannelDb.GetChannelsByGuild(Context.Guild.Id, Model.ChannelType.Reddit);
            int limit = 1;
            if (PremiumDb.IsPremium(Context.Guild.Id, ProType.Guild))
                limit = 5;
            if (PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
                limit = 10;

            if (subs.Count() >= limit)
            {
                await Context.Channel.SendMessageAsync($"Limit {limit} subscription per guild. Upgrade server to increase the limit! `{Program.GetPrefix(Context)}Pro`", embed: WebUtil.SubListEmbed(Context.Guild.Id).Build());
                return;
            }

            if (upvotes < 100)
            {
                await Context.Channel.SendMessageAsync("Upvote minimum must be at least 100.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();
            Subreddit sub = null;
            try
            {
                sub = await RedditAPI.GetSubreddit(name);
            } catch
            {
                await Context.Channel.SendMessageAsync($"Subreddit **{name}** not found.");
                return;
            }
            if (sub.Subscribers < 20000)
            {
                await Context.Channel.SendMessageAsync("The Subreddit must have at least **20,000** subscribers.\n" +
                    $"**{sub.Name}** has **{sub.Subscribers?.ToString("n0")}**.");
                return;
            }
            try
            {
                if (sub.Over18.Value && !((SocketTextChannel)Context.Channel).IsNsfw)
                {
                    await Context.Channel.SendMessageAsync($"**{sub.Name}** is a NSFW subreddit. This channel must be marked as NSFW.");
                    return;
                }
            } catch { }

            var old = subs.FirstOrDefault(x => x.ChannelId == Context.Channel.Id && x.Args.Split(",")[0].Equals(sub.Name));
            if(old != null)
            {
                await SpecialChannelDb.Delete(old);
            }

            await SpecialChannelDb.AddChannel(Context.Channel.Id, Model.ChannelType.Reddit, Context.Guild.Id, sub.Name + "," + upvotes);
            await Context.Channel.SendMessageAsync(embed: WebUtil.SubredditSubscribedEmbed(sub, upvotes).Build());
        }

        [RequireGuild]
        [Command("Unsubscribe"), Alias("unsub"), Summary("Unsubscribe from a subreddit.\n**Usage**: `!unsub [subreddit_name]`"), CustomUserPermission(GuildPermission.ManageChannels)]
        public async Task Unsubscribe(string name)
        {
            await Context.Channel.TriggerTypingAsync();

            var subs = SpecialChannelDb.GetChannelsByGuild(Context.Guild.Id, Model.ChannelType.Reddit);
            var olds = subs.Where(x => x.Args.Split(",")[0].Equals(name, StringComparison.OrdinalIgnoreCase));
            if (!olds.Any())
            {
                await Context.Channel.SendMessageAsync($"Subreddit **{name}** not found. Try `{Program.GetPrefix(Context)}sublist` for a list of your subreddits.");
            }
            foreach (var old in olds)
            {
                await SpecialChannelDb.Delete(old);
                await Context.Channel.SendMessageAsync($"Unsubscribed from **{name}**. Were their posts not good enough?");
            }
        }

        [RequireGuild]
        [Command("SubList"), Summary("Subreddits you are subscribed to.\n**Usage**: `!sublist`")]
        public async Task SubList()
        {
            await Context.Channel.SendMessageAsync(embed: WebUtil.SubListEmbed(Context.Guild.Id).Build());
        }
    }
}