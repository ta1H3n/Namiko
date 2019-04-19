﻿using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Core.Modules;
using System.Threading.Tasks;
using Namiko.Resources.Database;
using System.Collections.Generic;
using Namiko.Resources.Datatypes;
namespace Namiko.Core.Util
{
    public static class ToastieUtil
    {
        public static string RandomEmote()
        {
            return "Toasties";
        }
        public static int ParseAmount(string sAmount, SocketGuildUser user)
        {
            int amount = 0;
            if (sAmount.Equals("all"))
            {
                amount = ToastieDb.GetToasties(user.Id, user.Guild.Id);
                if (amount > 10000)
                    amount = 10000;
            }
            else if (!Int32.TryParse(sAmount, out amount))
            {
                amount = 0;
            }
            return amount;
        }
        public static EmbedBuilder ToastieEmbed(IUser user, int amount)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithDescription($"You have **{amount.ToString("n0")}** {ToastieUtil.RandomEmote()}!");
            eb.WithColor(Color.Gold);
            return eb;
        }
        public static async Task<EmbedBuilder> ToastieLeaderboardEmbedAsync(List<Toastie> toasties, SocketCommandContext context, int page)
        {
            var eb = new EmbedBuilder();
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var tList = new Dictionary<SocketUser, int>();

            //Getting users and sorting
            await Task.Run(() =>
             {
                 foreach (var x in toasties)
                 {
                     var user = context.Guild.GetUser(x.UserId);
                     if (user != null && !user.IsBot)
                         tList.Add(user, x.Amount);
                 }
             });

            var ordtList = tList.OrderByDescending(x => x.Value);

            string users = "";
            page = page * 10;
            for (int i = page; i < page + 10; i++)
            {
                var x = ordtList.ElementAtOrDefault(i);
                if (x.Key != null)
                    users += $"#{i + 1} {x.Key.Mention} - {x.Value.ToString("n0")}\n";
            }
            if (users == "")
                users = "-";

            eb.WithTitle(":star: Toastie Leaderboard");
            eb.AddField("Toasties <:toastie3:454441133876183060>", users, false);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter($"Page: {page / 10 + 1}");
            return eb;
        }
        public static async Task<EmbedBuilder> DailyLeaderboardEmbedAsync(List<Daily> dailies, SocketCommandContext context, int page)
        {
            var eb = new EmbedBuilder();
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var dList = new Dictionary<SocketUser, int>();

            //Getting users and sorting
            await Task.Run(() =>
            {
                foreach (var x in dailies)
                {
                    var user = context.Guild.GetUser(x.UserId);
                    if (user != null && !user.IsBot)
                        dList.Add(user, (x.Date + 172800000) < timeNow ? 0 : x.Streak);
                }
            });

            var orddList = dList.OrderByDescending(x => x.Value);

            string daily = "";
            page = page * 10;
            for (int i = page; i < page + 10; i++)
            {
                var y = orddList.ElementAtOrDefault(i);
                if (y.Key != null)
                {
                    daily += $"#{i + 1} {y.Key.Mention} - {y.Value.ToString("n0")}\n";
                }
            }
            if (daily == "")
                daily = "-";

            eb.WithTitle(":star: Toastie Leaderboard");
            eb.AddField("Daily Streak :calendar_spiral:", daily, true);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter($"Page: {page / 10 + 1}");
            return eb;
        }
        public static async Task<EmbedBuilder> BothLeaderboardEmbedAsync(List<Toastie> toasties, List<Daily> dailies, SocketCommandContext context, int page)
        {
            var eb = new EmbedBuilder();
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var tList = new Dictionary<SocketUser, int>();
            var dList = new Dictionary<SocketUser, int>();

            //Getting users and sorting
            await Task.Run(() =>
            {
                foreach (var x in toasties)
                {
                    var user = context.Guild.GetUser(x.UserId);
                    if (user != null && !user.IsBot)
                        tList.Add(user, x.Amount);
                }

                foreach (var x in dailies)
                {
                    var user = context.Guild.GetUser(x.UserId);
                    if (user != null && !user.IsBot)
                        dList.Add(user, (x.Date + 172800000) < timeNow ? 0 : x.Streak);
                }
            });

            var ordtList = tList.OrderByDescending(x => x.Value);
            var orddList = dList.OrderByDescending(x => x.Value);

            string users = "";
            string daily = "";
            page = page * 10;
            for (int i = page; i < page + 10; i++)
            {
                var x = ordtList.ElementAtOrDefault(i);
                if (x.Key != null)
                    users += $"#{i + 1} {x.Key.Mention} - {x.Value.ToString("n0")}\n";

                var y = orddList.ElementAtOrDefault(i);
                if (y.Key != null)
                {
                    daily += $"#{i + 1} {y.Key.Mention} - {y.Value.ToString("n0")}\n";
                }
            }
            if (users == "")
                users = "-";
            if (daily == "")
                daily = "-";

            eb.WithTitle(":star: Toastie Leaderboard");
            eb.AddField("Toasties <:toastie3:454441133876183060>", users, true);
            eb.AddField("Daily Streak :calendar_spiral:", daily, true);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter($"Page: {page / 10 + 1}");
            return eb;
        }
        public static EmbedBuilder GiveEmbed(IUser from, IUser to, int amount)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.AddField("Toasties <:toastie3:454441133876183060>", $"{from.Username} gave {to.Username} **{amount.ToString("n0")}** {ToastieUtil.RandomEmote()}!");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }



        // FLIPS

        public static Boolean FiftyFifty()
        {
            Random rand = new Random();
            int x = rand.Next(2);
            if (x == 0)
                return false;
            return true;
        }
        public static EmbedBuilder FlipWinEmbed(SocketGuildUser user, int amount)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithDescription($"**You win!** {amount.ToString("n0")} {ToastieUtil.RandomEmote()} received!\nNow you have {ToastieDb.GetToasties(user.Id, user.Guild.Id).ToString("n0")} {ToastieUtil.RandomEmote()}!");
            eb.WithColor(Color.Gold);
            return eb;
        }
        public static EmbedBuilder FlipLoseEmbed(SocketGuildUser user, int amount)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithDescription($"**You lose!** {amount.ToString("n0")} {ToastieUtil.RandomEmote()} lost...\nNow you have {ToastieDb.GetToasties(user.Id, user.Guild.Id).ToString("n0")} {ToastieUtil.RandomEmote()}!");
            eb.WithColor(Color.DarkRed);
            return eb;
        }

        // DAILIES

        public static int DailyAmount(double streak)
        {
            double amount = 0;
            double multiplier = 1.0 + streak / 5;
            int random = new Random().Next(10) + 5;
            amount = random * multiplier * 10;

            return (int)amount;
        }
        public static int DailyTax(double daily, double user, double bank, double all)
        {
            double tax = 0;
            double x = 0;

            x = bank / all;
            double bankMultiplier = (1 / ( 20*x + 1.2 )) - 0.05;

            x = user / all;
            double userMultiplier = Math.Log10((5 * x) + 0.3) + 0.1;

            double multiplier = bankMultiplier + userMultiplier;
            multiplier = multiplier > 0.5 ? 0.5 :
                         multiplier < 0.05 ? 0.05 :
                         multiplier;
            tax = daily * multiplier;

            return (int)tax;
        }
        public static EmbedBuilder DailyGetEmbed(IUser user, int streak, int amount, int balance)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user.ToString(), user.GetAvatarUrl(), BasicUtil.Patreon);
            eb.WithDescription($"You're on a **{streak.ToString("n0")}** day streak. You receive **{amount.ToString("n0")}** {ToastieUtil.RandomEmote()}\n" +
                $"Now you have **{balance}** {ToastieUtil.RandomEmote()}\n\n" +
                $"Vote for me on [Discord Bots](https://discordbots.org/bot/418823684459855882/vote) every day to receive a lootbox!");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
        public static EmbedBuilder DailyWaitEmbed(IUser user, int hours, int minutes, int seconds)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user.ToString(), user.GetAvatarUrl(), BasicUtil.Patreon);
            eb.WithDescription($"You already claimed your daily reward today.\nYou must wait `{hours} hours {minutes} minutes {seconds} seconds`");
            if (new Random().Next(20) == 1)
                eb.WithImageUrl("https://i.imgur.com/LcqpKmo.png");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }

        // WEEKLIES

        public static EmbedBuilder WeeklyWaitEmbed(DateTime date, IUser user)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user.ToString(), user.GetAvatarUrl(), BasicUtil.Patreon);
            DateTime now = DateTime.Now;
            date = date.AddDays(7);
            string wait = $"{(date - now).Days} Days {(date - now).Hours} Hours {(date - now).Minutes} Minutes";
            eb.WithDescription($"You already claimed your weekly reward.\nYou must wait `{wait}`");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
        public static EmbedBuilder WeeklyGetEmbed(int amount, int current, IUser user)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user.ToString(), user.GetAvatarUrl(), BasicUtil.Patreon);
            eb.WithDescription($"You received **{amount}** {RandomEmote()}\nNow you have **{current}** {RandomEmote()}");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
        
        // BEG

        public static string GetFalseBegMessage()
        {
            string[] msgs = {
                "Go away.",
                "Go away, you're annoying.",
                "Ask someone else.",
                "*Picks up a knife.*",
                "Have you not had enough?",
                "It's your own fault for losing it all.",
                "Your ideal heart rate is 0.",
                "You need a high-five... on the face... with a chair.",
                "If you were orphaned when you were a child, I feel sorry for you, but not for your parents.",
                "I bet your brain feels as good as new, seeing that you've never used it.",
                "Don't let your mind wander, it's far too small to be out by itself.",
                "People can't say that you have absolutely nothing! After all, you have inferiority!",
                "If we were to kill everybody who hates you, it wouldn't be murder; it would be genocide!",
                "I called your boyfriend gay and he hit me with his purse!",
                "I'd slap you, but that would be animal abuse.",
                "The next time you shave, could you stand a little closer to the razor?",
                "Everyone is entitled to be stupid, but you abuse the privilege.",
                "Don't piss me off today, I'm running out of places to hide the bodies.",
                "Until you called me I couldn't remember the last time I wanted to break somebody's fingers so badly.",
                "Beauty is skin deep, but ugly is to the bone.",
                "Sorry I can't think of an insult stupid enough for you.",
                "Let's see... I've walked the dog, cleaned my room, gone shopping and gossiped with my friends... Nope, this list doesn't say that I'm required to talk to you.",
                "Earth is full. Go home.",
                "If I could be one person for a day, it sure as hell wouldn't be you.",
                "Roses are red violets are blue, God made me pretty, what the hell happened to you?",
                "I am not anti-social, I just don't like you.",
                "There are some stupid people in this world. You just helped me realize it.",
                "A-are you talking? Did I give you permission to talk...?",
                "OK, and that's supposed to make me feel what?",
                "Damn not you again.",
                "I'm sorry I'm busy right now, can I ignore you some other time?",
                "Oh please help me, I'm sooo hurt by your hurtful comments!",
                "Before you came along we were hungry. Now we are fed up.",
                "Cancel my subscriptions, I'm tired of your issues!",
                "You have your whole life to be a jerk... So why don't you take a day off?",
                "You don't know me, you just wish you did.",
                "My Mom said never talk to strangers and well, since you're really strange... I guess that means I can't talk to you!",
                "People like you are the reason I'm on medication.",
                "If you're gonna act like a dick you should wear a condom on your head so you can at least look like one!",
                "I don't discriminate, I hate everyone.",
                "You know the drill... You leave a message and I ignore it!",
                "Hey, I am away from my phone but in the meantime, why don't you go play in traffic?!"
            };

            return msgs[new Random().Next(msgs.Length)];
        }

        // LOOTBOX

        public static bool IsWaifu(LootBoxType type)
        {
            var rnd = new Random();

            if (type == LootBoxType.Vote)
                return rnd.Next(Cost.voteWaifuChance) == 0;

            return false;
        }
        public static Waifu BoxWaifu(LootBoxType type)
        {
            return WaifuDb.RandomWaifus(3, 1)[0];
        }
        public static int BoxToasties(LootBoxType type)
        {
            var rnd = new Random();

            if (type == LootBoxType.Vote)
                return Cost.voteToastieAmount;

            return 0;
        }
        public static EmbedBuilder NoBoxEmbed(IUser author)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(author);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithDescription("You have no lootboxes!\nVote for me on [Discord Bots](https://discordbots.org/bot/418823684459855882/vote) to get one!");
            return eb;
        }
        public static EmbedBuilder BoxOpeningEmbed(IUser author)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(author);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithTitle("Opening your lootbox!");
            eb.WithImageUrl("https://data.whicdn.com/images/109950962/original.gif");
            return eb;
        }
    }
}
