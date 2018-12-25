using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Namiko.Core.Basic;
using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;

namespace Namiko.Core.Currency
{
    public static class ToastieUtil
    {
        public static string RandomEmote()
        {
            return "Toasties";
        }
        public static int GetAmount(string sAmount, SocketUser user)
        {
            int amount = 0;
            if (sAmount.Equals("all"))
            {
                amount = ToastieDb.GetToasties(user.Id);
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
            eb.WithDescription($"You have {amount} {ToastieUtil.RandomEmote()}!");
            eb.WithColor(Color.Gold);
            return eb;
        }
        public static EmbedBuilder ToastieLeaderboardEmbed(List<Toastie> toasties, SocketCommandContext context, int page)
        {
            var eb = new EmbedBuilder();
            
            var list = new Dictionary<SocketUser, int>();
            foreach(var x in toasties)
            {
                var user = context.Client.GetUser(x.UserId);
                if (user != null && !user.IsBot)
                    list.Add(user, x.Amount);
            }

            //var ordToasties = toasties.OrderByDescending(x => x.Amount).Skip(page * 10).Take((page + 1) * 10);
            var ordToasties = list.OrderByDescending(x => x.Value);

            eb.WithTitle(":star: Toastie Leaderboard");
            string users = "";
            page = page*10;
            for (int i = page; i < page + 10; i++)
            {
                //var user = context.Client.GetUser(x.UserId);
                //if(user != null && x != null)
                //    users += $"#{i} {user.Mention} - {x.Amount}\n";
                var x = ordToasties.ElementAtOrDefault(i);
                if(x.Key != null)
                    users += $"#{i+1} {x.Key.Mention} - {x.Value}\n";
            }



            eb.AddField("Users", users, true);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter($"Page: {page+1}");
            return eb;
        }
    }
    public static class FlipUtil
    {
        public static Boolean FiftyFifty()
        {
            Random rand = new Random();
            int x = rand.Next(2);
            if (x == 0)
                return false;
            return true;
        }
        public static EmbedBuilder WinEmbed(SocketUser user, int amount)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithDescription($"**You win!** {amount} {ToastieUtil.RandomEmote()} received!\nNow you have {ToastieDb.GetToasties(user.Id)} {ToastieUtil.RandomEmote()}!");
            eb.WithColor(Color.Gold);
            return eb;
        }
        public static EmbedBuilder LoseEmbed(SocketUser user, int amount)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithDescription($"**You lose!** {amount} {ToastieUtil.RandomEmote()} lost...\nNow you have {ToastieDb.GetToasties(user.Id)} {ToastieUtil.RandomEmote()}!");
            eb.WithColor(Color.DarkRed);
            return eb;
        }
    }
    public static class DailyUtil
    {
        public static int DailyAmount(double streak)
        {
            double amount = 0;
            double multiplier = 1.0 + streak / 5;
            int random = new Random().Next(10) + 5;
            amount = random * multiplier * 10;
            return (int)amount > 2500 ? 2500 : (int)amount;
        }
        public static EmbedBuilder DailyGetEmbed(IUser user, int streak, int amount, int balance)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithDescription($"You're on a **{streak}** day streak. You receive **{amount}** {ToastieUtil.RandomEmote()}\nYou now have **{balance}** {ToastieUtil.RandomEmote()}");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
        public static EmbedBuilder DailyWaitEmbed(IUser user, int hours, int minutes, int seconds)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithDescription($"You already claimed your daily reward today.\nYou must wait `{hours} hours {minutes} minutes {seconds} seconds`");
            eb.WithImageUrl("https://i.imgur.com/LcqpKmo.png");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
    }
}
