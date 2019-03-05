using System;
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
            eb.WithDescription($"You're on a **{streak.ToString("n0")}** day streak. You receive **{amount.ToString("n0")}** {ToastieUtil.RandomEmote()}\nYou now have **{balance}** {ToastieUtil.RandomEmote()}");
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
    }
}
