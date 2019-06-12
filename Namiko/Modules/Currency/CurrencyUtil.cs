using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;

using System.Threading.Tasks;

using System.Collections.Generic;

namespace Namiko
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
            if (sAmount.Equals("all", StringComparison.InvariantCultureIgnoreCase))
            {
                amount = ToastieDb.GetToasties(user.Id, user.Guild.Id);
                if (amount > 10000)
                    amount = 10000;
                return amount;
            }
            if (sAmount.Equals("half", StringComparison.InvariantCultureIgnoreCase))
            {
                amount = ToastieDb.GetToasties(user.Id, user.Guild.Id) / 2;
                return amount;
            }
            var div = sAmount.Split('/');
            if(div.Length == 2)
            {
                try
                {
                    double x = double.Parse(div[0]);
                    double y = double.Parse(div[1]);
                    if (x / y > 0 && x / y <= 1)
                    {
                        amount = (int)(ToastieDb.GetToasties(user.Id, user.Guild.Id) * x / y);
                        return amount;
                    }
                } catch { }
            }

            if (!Int32.TryParse(sAmount, out amount))
            {
                amount = -1;
            }
            return amount == 0 ? -1 : amount;
        }
        public static EmbedBuilder ToastieEmbed(IUser user, int amount)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithDescription($"You have **{amount.ToString("n0")}** {ToastieUtil.RandomEmote()}!");
            eb.WithColor(Color.Gold);
            return eb;
        }
        public static async Task<EmbedBuilder> ToastieLeaderboardEmbedAsync(List<Balance> toasties, SocketCommandContext context, int page)
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
        public static async Task<EmbedBuilder> BothLeaderboardEmbedAsync(List<Balance> toasties, List<Daily> dailies, SocketCommandContext context, int page)
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
            double amount = 100 + 3000 * Math.Log10(streak / 30 + 1);
            double multiplier = (double)(90 + new Random().Next(20)) / 100;
            amount = amount * multiplier;

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
        public static EmbedBuilder DailyGetEmbed(IUser user, int streak, int amount, int balance, string prefix)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user.ToString(), user.GetAvatarUrl(), BasicUtil._patreon);
            eb.WithDescription($"You're on a **{streak.ToString("n0")}** day streak. You receive **{amount.ToString("n0")}** {ToastieUtil.RandomEmote()}\n" +
                $"Now you have **{balance.ToString("n0")}** {ToastieUtil.RandomEmote()}\n\n" +
                $"Vote for me on [Discord Bots](https://discordbots.org/bot/418823684459855882/vote) every day to receive a lootbox!");
            eb.WithColor(BasicUtil.RandomColor());

            if (!(PremiumDb.IsPremium(user.Id, PremiumType.Waifu) || PremiumDb.IsPremium(user.Id, PremiumType.Toastie)))
                eb.WithFooter($"Namiko now has Premium! `{prefix}Premium`");
            return eb;
        }
        public static EmbedBuilder DailyWaitEmbed(IUser user, int hours, int minutes, int seconds, string prefix)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user.ToString(), user.GetAvatarUrl(), BasicUtil._patreon);
            eb.WithDescription($"You already claimed your daily reward today.\nYou must wait `{hours} hours {minutes} minutes {seconds} seconds`");
            if (new Random().Next(20) == 1)
                eb.WithImageUrl("https://i.imgur.com/LcqpKmo.png");
            eb.WithColor(BasicUtil.RandomColor());

            if (!(PremiumDb.IsPremium(user.Id, PremiumType.Waifu) || PremiumDb.IsPremium(user.Id, PremiumType.Toastie)))
                eb.WithFooter($"Namiko now has Premium! `{prefix}Premium`");
            return eb;
        }

        // WEEKLIES

        public static EmbedBuilder WeeklyWaitEmbed(DateTime date, IUser user, string prefix)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user.ToString(), user.GetAvatarUrl(), BasicUtil._patreon);
            DateTime now = DateTime.Now;
            string wait = $"{(date - now).Days} Days {(date - now).Hours} Hours {(date - now).Minutes} Minutes";
            eb.WithDescription($"You already claimed your weekly reward.\nYou must wait `{wait}`");
            eb.WithColor(BasicUtil.RandomColor());

            if (!(PremiumDb.IsPremium(user.Id, PremiumType.Waifu) || PremiumDb.IsPremium(user.Id, PremiumType.Toastie)))
                eb.WithFooter($"Namiko now has Premium! `{prefix}Premium`");
            return eb;
        }
        public static EmbedBuilder WeeklyGetEmbed(int amount, int current, IUser user, string prefix)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user.ToString(), user.GetAvatarUrl(), BasicUtil._patreon);
            eb.WithDescription($"You received **{amount.ToString("n0")}** {RandomEmote()}\nNow you have **{current.ToString("n0")}** {RandomEmote()}");
            eb.WithColor(BasicUtil.RandomColor());

            if (!(PremiumDb.IsPremium(user.Id, PremiumType.Waifu) || PremiumDb.IsPremium(user.Id, PremiumType.Toastie)))
                eb.WithFooter($"Namiko now has Premium! `{prefix}Premium`");
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
                "Life may be temporary, but your stupidity is eternal.",
                "If I agreed with you, then we'd both be wrong.",
                "Could you stop talking?",
                "Do you know what a tsundere is? That's me except I actually don't like you.",
                "The ideal heart rate? For you... 0.",
                "If humans are 70% water, how are you 100% bullshit?",
                "Toasties? It's your life you should be begging for...",
                "Jokes aside, you're pathetic.",
                "STOP... SPAMMING... ME!",
                "You should've been aborted by your mother."
            };

            return msgs[new Random().Next(msgs.Length)];
        }

        // LOOTBOX

        public static bool IsWaifu(LootBoxType type)
        {
            var rnd = new Random();

            switch (type)
            {
                case LootBoxType.Vote:
                    return rnd.Next(Constants.voteWaifuChance) == 0;

                case LootBoxType.Premium:
                    return rnd.Next(Constants.premiumWaifuChance) == 0;

                default:
                    return false;
            }
        }
        public static Waifu BoxWaifu(LootBoxType type, IEnumerable<PremiumType> premiums = null, ulong userId = 0, ulong guildId = 0)
        {
            var rnd = new Random();
            List<Waifu> waifus = new List<Waifu>();
            int randomizerAmount = 7;

            int tier = 3;
            switch (type)
            {
                case LootBoxType.Vote:
                    waifus.AddRange(WaifuDb.RandomWaifus(3, randomizerAmount));
                    break;

                case LootBoxType.Premium:
                    int r = rnd.Next(100);

                    if (r < 5)
                        tier = 1;
                    else if (r < 30)
                        tier = 2;
                    else
                        tier = 3;

                    waifus.AddRange(WaifuDb.RandomWaifus(tier, randomizerAmount));
                    break;

                default:
                    waifus.AddRange(WaifuDb.RandomWaifus(3, randomizerAmount));
                    break;
            }

            if(premiums != null && premiums.Contains(PremiumType.Waifu))
            {
                waifus.AddRange(WaifuWishlistDb.GetWishlist(userId, guildId).Where(x => x.Tier == tier));
            }

            return waifus[rnd.Next(waifus.Count)];
        }
        public static int BoxToasties(LootBoxType type)
        {
            switch (type)
            {
                case LootBoxType.Vote:
                    return Constants.voteToastieAmount;

                case LootBoxType.Premium:
                    return Constants.premiumToastieAmount;

                default:
                    return Constants.voteToastieAmount;
            }
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
        public static EmbedBuilder BoxListEmbed(List<LootBox> boxes, IUser author)
        {
            var eb = new EmbedBuilderPrepared(author);

            string field = "";
            for(int i = 0; i < boxes.Count; i++)
            {
                string location = boxes[i].GuildId == 0 ? "Global" : "This Server";
                field += $"`#{i+1}` **x{boxes[i].Amount}** {boxes[i].Type.ToString()} - *{location}*\n";
            }

            eb.WithDescription("Enter the number of the Lootbox you wish to open.");
            eb.AddField("Lootboxes :star:", field);
            return eb;
        }
    }
}
