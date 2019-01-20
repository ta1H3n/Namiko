using System;
using Discord;
using Namiko.Core.Util;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Namiko.Resources.Preconditions;
namespace Namiko.Core.Modules
{
    public class Currency : ModuleBase<SocketCommandContext>
    {
        [Command("Blackjack"), Alias("bj"), Summary("Starts a game of blackjack.\n**Usage**: `!bj [amount]`")]
        public async Task BlackjackCommand(string sAmount, [Remainder] string str = "")
        {
            var user = (SocketGuildUser) Context.User;
            var ch = (SocketTextChannel) Context.Channel;

            if (Blackjack.games.ContainsKey(Context.User))
            {
                await Context.Channel.SendMessageAsync("You are already in a game of blackjack. #" + Blackjack.games[user].Channel.Name);
                return;
            }
            
            int amount = ToastieUtil.ParseAmount(sAmount, user);
            try
            {
                await ToastieDb.AddToasties(user.Id, -amount, Context.Guild.Id);
            }
            catch (Exception e)
            {
                await ch.SendMessageAsync(e.Message);
                return;
            }

            if (ToastieDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id) < amount)
            {
                await Context.Channel.SendMessageAsync("I don't have enough toasties to gamble with... You can give me some using the `!give` command, and view who has the most toasties with `!tlb`.");
                await ToastieDb.AddToasties(user.Id, amount, Context.Guild.Id);
                return;
            }

            BlackjackGame game = new BlackjackGame(amount, ch);
            Blackjack.games[user] = game;
            await Blackjack.GameContinue(Context, game);
        }

        [Command("Daily"), Alias("dailies"), Summary("Gives daily toasties to a user.")]
        public async Task DailyCmd()
        {
            Daily daily = DailyDb.GetDaily(Context.User.Id, Context.Guild.Id);
            if (daily == null)
            {
                daily = new Daily
                {
                    UserId = Context.User.Id,
                    GuildId = Context.Guild.Id,
                    Date = 0
                };
            }

            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if ((daily.Date + 72000000) < timeNow)
            {
                if ((daily.Date + 172800000) < timeNow)
                {
                    daily.Streak = 0;
                }

                daily.Streak++;
                daily.Date = timeNow;
                int amount = ToastieUtil.DailyAmount(daily.Streak);
                await DailyDb.SetDaily(daily);
                await ToastieDb.AddToasties(Context.User.Id, amount, Context.Guild.Id);
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, amount / 20, Context.Guild.Id);

                await Context.Channel.SendMessageAsync("", false, ToastieUtil.DailyGetEmbed(Context.User, daily.Streak, amount, ToastieDb.GetToasties(Context.User.Id, Context.Guild.Id)).Build());
            }
            else
            {
                long wait = ((daily.Date + 72000000) - timeNow) / 1000;
                int hours = (int)wait / 3600;
                int minutes = (int)wait % 3600 / 60;
                int seconds = (int)wait % 60;
                await Context.Channel.SendMessageAsync("", false, ToastieUtil.DailyWaitEmbed(Context.User, hours, minutes, seconds).Build());
            }
        }

        [Command("Flip"), Alias("f"), Summary("Flip a coin for toasties, defaults to tails.\n**Usage**: `!flip [amount] [heads_or_tails]`")]
        public async Task Flip(string sAmount, string side = "t", [Remainder] string str = "")
        {
            var user = (SocketGuildUser) Context.User;

            if (!(side.Equals("t") || side.Equals("h") || side.Equals("tails") || side.Equals("heads")))
            {
                await Context.Channel.SendMessageAsync("Pick heads or tails!");
                return;
            }

            int amount = ToastieUtil.ParseAmount(sAmount, user);
            if (amount <= 0)
            {
                await Context.Channel.SendMessageAsync("Pick an amount!");
                return;
            }

            try
            {
                await ToastieDb.AddToasties(user.Id, -amount, Context.Guild.Id);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync(ex.Message);
                return;
            }

            if(ToastieDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id) < amount)
            {
                await Context.Channel.SendMessageAsync("I don't have enough toasties to gamble with... You can give me some using the `!give` command, and view who has the most toasties with `!tlb`.");
                await ToastieDb.AddToasties(user.Id, amount, Context.Guild.Id);
                return;
            }

            if (ToastieUtil.FiftyFifty())
            {
                await ToastieDb.AddToasties(user.Id, amount * 2, Context.Guild.Id);
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, -amount, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", false, ToastieUtil.FlipWinEmbed(user, amount).Build());
            }
            else
            {
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, amount, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", false, ToastieUtil.FlipLoseEmbed(user, amount).Build());
            }
        }

        [Command("Balance"), Alias("toastie", "bal", "toasties"), Summary("Shows amount of toasties.\n**Usage**: `!bal [user_optional]`")]
        public async Task Toastie(IUser User = null, [Remainder] string str = "")
        {
            if (User == null)
            {
                User = Context.User;
            }
            await Context.Channel.SendMessageAsync("", false, ToastieUtil.ToastieEmbed(User, ToastieDb.GetToasties(User.Id, Context.Guild.Id)).Build());
        }

        [Command("Give"), Summary("Give a user some of you toasties.\n**Usage**: `!give [user] [amount]`")]
        public async Task Give(IUser recipient, string sAmount, [Remainder] string str = "")
        {
            int amount = ToastieUtil.ParseAmount(sAmount, (SocketGuildUser)Context.User);
            if (amount <= 0)
            {
                await Context.Channel.SendMessageAsync("Pick an amount!");
                return;
            }

            try
            {
                await ToastieDb.AddToasties(Context.User.Id, -amount, Context.Guild.Id);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync(ex.Message);
                return;
            }

            await ToastieDb.AddToasties(recipient.Id, amount, Context.Guild.Id);

            EmbedBuilder eb = new EmbedBuilder();
            eb.WithAuthor("Toasties");
            eb.WithDescription($"{Context.User.Username} gave {recipient.Username} **{amount.ToString("n0")}** {ToastieUtil.RandomEmote()}!");
            eb.WithColor(BasicUtil.RandomColor());
            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("SetToasties"), Alias("st", "sett"), Summary("Sets the amount of toasties.\n**Usage**: `!st [user] [amount]`"), CustomUserPermission(GuildPermission.Administrator), CustomBotPermission(GuildPermission.Administrator)]
        public async Task Set(IUser user, int amount, [Remainder] string str = "")
        {
            await ToastieDb.SetToasties(user.Id, amount, Context.Guild.Id);
            await Context.Channel.SendMessageAsync("", false, ToastieUtil.ToastieEmbed(user, ToastieDb.GetToasties(user.Id, Context.Guild.Id)).Build());
        }

        [Command("AddToasites"), Alias("at", "addt"), Summary("Adds toasties to a user.\n**Usage**: `!at [user] [amount]`"), CustomUserPermission(GuildPermission.Administrator), CustomBotPermission(GuildPermission.Administrator)]
        public async Task Add(IUser user, int amount, [Remainder] string str = "")
        {
            await ToastieDb.AddToasties(user.Id, amount, Context.Guild.Id);
            await Context.Channel.SendMessageAsync("", false, ToastieUtil.ToastieEmbed(user, ToastieDb.GetToasties(user.Id, Context.Guild.Id)).Build());
        }

        [Command("ToastieLeaderboard"), Alias("tlb"), Summary("Toastie Leaderboard.\n**Usage**: `!tlb [page_number]`")]
        public async Task ToastieLeaderboard(int page = 1, [Remainder] string str = "")
        {
            var toasties = ToastieDb.GetAllToasties();
            await Context.Channel.SendMessageAsync("", false, (await ToastieUtil.ToastieLeaderboardEmbedAsync(toasties, Context, page-1)).Build());
        }

        [Command("DailyLeaderboard"), Alias("dlb"), Summary("Daily Leaderboard.\n**Usage**: `!dlb [page_number]`")]
        public async Task DailyLeaderboard(int page = 1, [Remainder] string str = "")
        {
            var dailies = DailyDb.GetAll();
            await Context.Channel.SendMessageAsync("", false, (await ToastieUtil.DailyLeaderboardEmbedAsync(dailies, Context, page - 1)).Build());
        }
    }
}
