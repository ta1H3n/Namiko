using System;
using Discord;

using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;


using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Discord.Addons.Interactive;
using System.Linq;

namespace Namiko
{
    public class Currency : InteractiveBase<ShardedCommandContext>
    {
        [Command("Blackjack"), Alias("bj"), Summary("Starts a game of blackjack.\n**Usage**: `!bj [amount]`")]
        public async Task BlackjackCommand(string sAmount, [Remainder] string str = "")
        {
            var user = (SocketGuildUser)Context.User;
            var ch = (SocketTextChannel)Context.Channel;

            if (Blackjack.games.ContainsKey(Context.User))
            {
                await Context.Channel.SendMessageAsync("You are already in a game of blackjack. #" + Blackjack.games[user].Channel.Name);
                return;
            }

            int amount = ToastieUtil.ParseAmount(sAmount, user);
            if (amount < 0)
            {
                await Context.Channel.SendMessageAsync("Pick an amount! number, all, half, or x/y.");
                return;
            }
            if (amount == 0)
            {
                await Context.Channel.SendMessageAsync("You have no toasties...");
                return;
            }

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
                string prefix = Program.GetPrefix(Context);
                await Context.Channel.SendMessageAsync("Tch, I don't have enough toasties... I will recover eventually by stealing toasties from all of you. :fox:" +
                    $"But if you want to speed up the process you can give me some using the `{prefix}give` or `{prefix}at` commands.");
                await ToastieDb.AddToasties(user.Id, amount, Context.Guild.Id);
                return;
            }

            BlackjackGame game = new BlackjackGame(amount, ch);
            Blackjack.games[user] = game;
            await Blackjack.GameContinue(Context, game);
        }

        [Command("Daily"), Alias("dailies", "daywy", "daiwy"), Summary("Gives daily toasties.")]
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
            int ms = 72000000;
            if (PremiumDb.IsPremium(Context.Guild.Id, PremiumType.ServerT1) || PremiumDb.IsPremium(Context.Guild.Id, PremiumType.ServerT2))
                ms /= 2;

            if ((daily.Date + ms) < timeNow)
            {
                if ((daily.Date + 172800000) < timeNow)
                {
                    daily.Streak = 0;
                }

                daily.Streak++;
                daily.Date = timeNow;
                int amount = ToastieUtil.DailyAmount(daily.Streak);
                int tax = ToastieUtil.DailyTax(amount, ToastieDb.GetToasties(Context.User.Id, Context.Guild.Id), ToastieDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id), await ToastieDb.TotalToasties(Context.Guild.Id));
                amount -= tax / 2;
                //int cap = Constants.dailycap;
                //amount = amount > cap ? cap : amount;

                await DailyDb.SetDaily(daily);
                await ToastieDb.AddToasties(Context.User.Id, amount, Context.Guild.Id);
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, tax, Context.Guild.Id);

                await Context.Channel.SendMessageAsync("", false, ToastieUtil.DailyGetEmbed(Context.User, daily.Streak, amount, ToastieDb.GetToasties(Context.User.Id, Context.Guild.Id), this.Prefix()).Build());
            }
            else
            {
                long wait = ((daily.Date + ms) - timeNow) / 1000;
                int hours = (int)wait / 3600;
                int minutes = (int)wait % 3600 / 60;
                int seconds = (int)wait % 60;
                await Context.Channel.SendMessageAsync("", false, ToastieUtil.DailyWaitEmbed(Context.User, hours, minutes, seconds, this.Prefix()).Build());
            }
        }

        [Command("Weekly"), Alias("weeklies", "weekwy"), Summary("Gives weekly toasties.")]
        public async Task Weekly()
        {
            var weekly = WeeklyDb.GetWeekly(Context.User.Id, Context.Guild.Id);

            if (weekly == null)
            {
                weekly = new Weekly
                {
                    GuildId = Context.Guild.Id,
                    UserId = Context.User.Id
                };
            }

            int hours = 168;
            if (PremiumDb.IsPremium(Context.User.Id, PremiumType.Toastie))
                hours /= 2;

            if (weekly.Date == null ? true : weekly.Date.AddHours(hours).CompareTo(DateTime.Now) < 0)
            {
                int streak = await DailyDb.GetHighest(Context.Guild.Id) + 15;
                int amount = ToastieUtil.DailyAmount(streak);
                int tax = ToastieUtil.DailyTax(amount, ToastieDb.GetToasties(Context.User.Id, Context.Guild.Id), ToastieDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id), await ToastieDb.TotalToasties(Context.Guild.Id));
                amount -= tax / 2;
                //int cap = Constants.weeklycap;
                //if (PremiumDb.IsPremium(Context.Guild.Id, PremiumType.ServerT1))
                //    cap += 1000;
                //amount = amount > cap ? cap : amount;
                if (PremiumDb.IsPremium(Context.Guild.Id, PremiumType.ServerT1))
                    amount += 1000;

                string text = "";
                if (PremiumDb.IsPremium(Context.User.Id, PremiumType.Waifu))
                {
                    if (PremiumDb.IsPremium(Context.User.Id, PremiumType.Toastie))
                    {
                        await LootBoxDb.AddLootbox(Context.User.Id, LootBoxType.Premium, 1, Context.Guild.Id);
                        text = "You receive a lootbox! :star2:";
                    }
                    else
                    {
                        await LootBoxDb.AddLootbox(Context.User.Id, LootBoxType.Vote, 1, Context.Guild.Id);
                        text = "You receive a lootbox! :star:";
                    }
                }

                await ToastieDb.AddToasties(Context.User.Id, amount, Context.Guild.Id);
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, tax / 2, Context.Guild.Id);
                weekly.Date = DateTime.Now;
                await WeeklyDb.SetWeekly(weekly);
                await Context.Channel.SendMessageAsync(text, false, ToastieUtil.WeeklyGetEmbed(amount, ToastieDb.GetToasties(Context.User.Id, Context.Guild.Id), Context.User, this.Prefix()).Build());
                return;
            }

            await Context.Channel.SendMessageAsync("", false, ToastieUtil.WeeklyWaitEmbed(weekly.Date.AddHours(hours), Context.User, this.Prefix()).Build());
        }

        [Command("Flip"), Alias("f", "fwip"), Summary("Flip a coin for toasties, defaults to tails.\n**Usage**: `!flip [amount] [heads_or_tails]`")]
        public async Task Flip(string sAmount, string side = "t", [Remainder] string str = "")
        {
            var user = (SocketGuildUser)Context.User;
            var rnd = new Random();
            side = side.ToLower();

            if (!(side.Equals("t") || side.Equals("h") || side.Equals("tails") || side.Equals("heads")))
            {
                await Context.Channel.SendMessageAsync("Pick heads or tails!");
                return;
            }

            int amount = ToastieUtil.ParseAmount(sAmount, user);
            if (amount < 0)
            {
                await Context.Channel.SendMessageAsync("Pick an amount! number, all, half, or x/y.");
                return;
            }
            if (amount == 0)
            {
                await Context.Channel.SendMessageAsync("You have no toasties...");
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

            if (ToastieDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id) < amount)
            {
                string prefix = Program.GetPrefix(Context);
                await Context.Channel.SendMessageAsync("Tch, I don't have enough toasties... I will recover eventually by stealing toasties from all of you. :fox:" +
                    $"But if you want to speed up the process you can give me some using the `{prefix}give` or `{prefix}at` commands.");
                await ToastieDb.AddToasties(user.Id, amount, Context.Guild.Id);
                return;
            }

            if (ToastieUtil.FiftyFifty())
            {
                await ToastieDb.AddToasties(user.Id, amount * 2, Context.Guild.Id);
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, -amount, Context.Guild.Id);
                string resp = "";
                if (amount >= 50000 && rnd.Next(5) == 1)
                    resp = "Tch... I'll get you next time.";
                await Context.Channel.SendMessageAsync(resp, false, ToastieUtil.FlipWinEmbed(user, amount).Build());
            }
            else
            {
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, amount, Context.Guild.Id);
                string resp = "";
                if (amount >= 50000 && rnd.Next(5) == 1)
                    resp = "I'll take those.";
                await Context.Channel.SendMessageAsync(resp, false, ToastieUtil.FlipLoseEmbed(user, amount).Build());
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

        [Command("Give"), Summary("Give a user some of your toasties.\n**Usage**: `!give [user] [amount]`")]
        public async Task Give(IUser recipient, string sAmount, [Remainder] string str = "")
        {
            int amount = ToastieUtil.ParseAmount(sAmount, (SocketGuildUser)Context.User);
            if (amount < 0)
            {
                await Context.Channel.SendMessageAsync("Pick an amount! number, all, half, or x/y.");
                return;
            }
            if (amount == 0)
            {
                await Context.Channel.SendMessageAsync("You have no toasties...");
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

            await Context.Channel.SendMessageAsync("", false, ToastieUtil.GiveEmbed(Context.User, recipient, amount).Build());
        }
        
        [Command("Give"), Summary("Give a user some of your toasties.\n**Usage**: `!give [user] [amount]`")]
        public async Task Give(string sAmount, IUser recipient, [Remainder] string str = "")
        {
            await Give(recipient, sAmount, str);
        }

        [Command("SetToasties"), Alias("st", "sett"), Summary("Sets the amount of toasties.\n**Usage**: `!st [user] [amount]`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task Set(IUser user, int amount, [Remainder] string str = "")
        {
            await ToastieDb.SetToasties(user.Id, amount, Context.Guild.Id);
            await Context.Channel.SendMessageAsync("", false, ToastieUtil.ToastieEmbed(user, ToastieDb.GetToasties(user.Id, Context.Guild.Id)).Build());
        }

        [Command("AddToasites"), Alias("at", "addt"), Summary("Adds toasties to a user.\n**Usage**: `!at [user] [amount]`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task Add(IUser user, int amount, [Remainder] string str = "")
        {
            await ToastieDb.AddToasties(user.Id, amount, Context.Guild.Id);
            await Context.Channel.SendMessageAsync("", false, ToastieUtil.ToastieEmbed(user, ToastieDb.GetToasties(user.Id, Context.Guild.Id)).Build());
        }

        [Command("LootboxShop"), Alias("ls"), Summary("Lootbox shop.\n**Usage**: `!ls`")]
        public async Task LootboxShop([Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync(embed: ToastieUtil.BoxShopEmbed(Context.User).Build());
        }

        [Command("BuyLootbox"), Alias("bl"), Summary("Buy a lootbox.\n**Usage**: `!bl [name]`")]
        public async Task BuyLootbox([Remainder] string str = "")
        {
            LootboxStat box = null;
            var boxes = LootboxStats.Lootboxes.Where(x => x.Value.Price >= 0).Select(x => x.Value).ToList();
            if (str != "")
            {
                var matches = boxes.Where(x => x.Name.Contains(str, StringComparison.OrdinalIgnoreCase));
                if (matches.Count() == 1)
                    box = matches.First();
            }
            if (box == null)
            {
                var listMsg = await Context.Channel.SendMessageAsync(embed: ToastieUtil.BoxShopEmbed(Context.User)
                    .WithFooter("Times out in 23 seconds. Try the `lootboxstats` command")
                    .WithDescription("Enter the number of the lootbox you wish to purchase.")
                    .Build());
                var response = await NextMessageAsync(
                    new Criteria<IMessage>()
                    .AddCriterion(new EnsureSourceUserCriterion())
                    .AddCriterion(new EnsureSourceChannelCriterion())
                    .AddCriterion(new EnsureRangeCriterion(boxes.Count(), Program.GetPrefix(Context))),
                    new TimeSpan(0, 0, 23));

                _ = listMsg.DeleteAsync();
                int i = 0;
                try
                {
                    i = int.Parse(response.Content);
                }
                catch
                {
                    _ = Context.Message.DeleteAsync();
                    return;
                }
                _ = response.DeleteAsync();

                box = boxes[i - 1];
            }

            try
            {
                await ToastieDb.AddToasties(Context.User.Id, -box.Price, Context.Guild.Id);
            } catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync(ex.Message);
                return;
            }

            await LootBoxDb.AddLootbox(Context.User.Id, (LootBoxType)box.TypeId, 1, Context.Guild.Id);
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                .WithDescription($"You bought a **{box.Name}**!\nType `{Program.GetPrefix(Context)}open` to open it!")
                .Build());
        }

        [Command("LootboxStats"), Summary("Shows the stats of all lootboxes.\n**Usage**: `!lootboxstats`")]
        public async Task ShowLootboxStats([Remainder] string str = "")
        {
            await ReplyAsync(embed: ToastieUtil.BoxStatsEmbed(Context.User).Build());
        }

        [Command("ToastieLeaderboard"), Alias("tlb"), Summary("Toastie Leaderboard.\n**Usage**: `!tlb`")]
        public async Task ToastieLeaderboard([Remainder] string str = "")
        {
            var toasties = await ToastieDb.GetAllToasties(Context.Guild.Id);
            var parsed = toasties.Select(x => 
                {
                    try
                    {
                        return new UserAmountView()
                        {
                            User = Context.Guild.GetUser(x.Id),
                            Amount = x.Count
                        };
                    }
                    catch
                    { return null; }
                })
                .Where(x => x != null && x.User != null);

            var msg = new CustomPaginatedMessage();
            
            msg.Title = "User Leaderboards";
            var fields = new List<FieldPages>();
            fields.Add(new FieldPages
            {
                Title = "Toasties <:toastie3:454441133876183060>",
                Pages = CustomPaginatedMessage.PagesArray(parsed, 10),
                Inline = true
            });
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }

        [Command("DailyLeaderboard"), Alias("dlb"), Summary("Daily Leaderboard.\n**Usage**: `!dlb`")]
        public async Task DailyLeaderboard([Remainder] string str = "")
        {
            var dailies = await DailyDb.GetLeaderboard(Context.Guild.Id);
            var parsed = dailies.Select(x =>
                {
                    try
                    {
                        return new UserAmountView()
                        {
                            User = Context.Guild.GetUser(x.Id),
                            Amount = x.Count
                        };
                    }
                    catch
                    { return null; }
                })
                .Where(x => x != null && x.User != null);

            var msg = new CustomPaginatedMessage();

            msg.Author = new EmbedAuthorBuilder() { Name = "User Leaderboards" };
            msg.Title = "Daily Streak :calendar_spiral:";
            msg.Pages = CustomPaginatedMessage.PagesArray(parsed, 10);

            await PagedReplyAsync(msg);
        }

        [Command("Beg"), Summary("Beg Namiko for toasties.\n**Usage**: `!beg`")]
        public async Task Beg([Remainder] string str = "")
        {
            var amount = ToastieDb.GetToasties(Context.User.Id, Context.Guild.Id);
            if (amount > 0)
            {
                await Context.Channel.SendMessageAsync("You already have toasties, you snob.");
                return;
            }

            if (!Constants.beg)
            {
                await Context.Channel.SendMessageAsync(ToastieUtil.GetFalseBegMessage());
                return;
            }

            amount = Constants.begAmount;
            await ToastieDb.AddToasties(Context.User.Id, amount, Context.Guild.Id);

            await Context.Channel.SendMessageAsync("Fine. Just leave me alone.", false, ToastieUtil.GiveEmbed(Context.Client.CurrentUser, Context.User, amount).Build());
        }

        [Command("Open"), Alias("OpenLootbox", "Lootbox", "Lootwox"), Summary("Open a lootbox if you have one.\n**Usage**: `!open`"), RequireContext(ContextType.Guild)]
        public async Task Open([Remainder] string str = "")
        {
            var boxes = await LootBoxDb.GetAll(Context.User.Id, Context.Guild.Id);
            if(boxes.Count == 0)
            {
                await Context.Channel.SendMessageAsync("", false, ToastieUtil.NoBoxEmbed(Context.User).Build());
                return;
            }

            LootBox box = null;
            if (boxes.Count == 1)
                box = boxes[0];

            else
            {
                var listMsg = await Context.Channel.SendMessageAsync(embed: ToastieUtil.BoxListEmbed(boxes, Context.User)
                    .WithFooter("Times out in 23 seconds")
                    .WithDescription("Enter the number of the Lootbox you wish to open.")
                    .Build());
                var response = await NextMessageAsync(
                    new Criteria<IMessage>()
                    .AddCriterion(new EnsureSourceUserCriterion())
                    .AddCriterion(new EnsureSourceChannelCriterion())
                    .AddCriterion(new EnsureRangeCriterion(boxes.Count, Program.GetPrefix(Context))),
                    new TimeSpan(0, 0, 23));

                _ = listMsg.DeleteAsync();
                int i = 0;
                try
                {
                    i = int.Parse(response.Content);
                }
                catch
                {
                    _ = Context.Message.DeleteAsync();
                    return;
                }
                _ = response.DeleteAsync();

                box = boxes[i - 1];
            }

            var type = LootboxStats.Lootboxes[box.Type];

            try
            {
                await LootBoxDb.AddLootbox(Context.User.Id, box.Type, -1, box.GuildId);
            } catch
            {
                await Context.Channel.SendMessageAsync("You tried.");
            }
            var msg = await Context.Channel.SendMessageAsync("", false, ToastieUtil.BoxOpeningEmbed(Context.User).Build());
            await UserDb.IncrementLootboxOpened(Context.User.Id);
            int waitms = 4200;

            if (type.IsWaifu())
            {
                bool isPremium = PremiumDb.IsPremium(Context.User.Id, PremiumType.Waifu);
                var waifu = await ToastieUtil.UnboxWaifu(type, isPremium, Context.User.Id, Context.Guild.Id);
                while(UserInventoryDb.OwnsWaifu(Context.User.Id, waifu, Context.Guild.Id))
                    waifu = await ToastieUtil.UnboxWaifu(type, isPremium, Context.User.Id, Context.Guild.Id);

                await UserInventoryDb.AddWaifu(Context.User.Id, waifu, Context.Guild.Id);
                await Task.Delay(waitms);
                var embed = WaifuUtil.WaifuEmbedBuilder(waifu).Build();
                await msg.ModifyAsync(x => {
                    x.Embed = embed;
                    x.Content = $"{Context.User.Mention} Congratulations! You found **{waifu.Name}**!";
                });
                return;
            }

            var amountWon = type.GetRandomToasties();
            await ToastieDb.AddToasties(Context.User.Id, amountWon, Context.Guild.Id);
            var bal = ToastieDb.GetToasties(Context.User.Id, Context.Guild.Id);
            await Task.Delay(waitms);
            await msg.ModifyAsync(x => {
                x.Embed = new EmbedBuilder()
                .WithAuthor($"{Context.User} | {box.Type.ToString()}", Context.User.GetAvatarUrl(), BasicUtil._patreon)
                .WithColor(BasicUtil.RandomColor())
                .WithThumbnailUrl("https://i.imgur.com/4JQmxa6.png")
                .WithDescription($"Congratulations! You found **{amountWon.ToString("n0")}** {ToastieUtil.RandomEmote()}!\nNow you have **{bal.ToString("n0")}** {ToastieUtil.RandomEmote()}!")
                //.AddField($"<:toastie3:454441133876183060> {Context.User} | Lootbox", $"Congratulations! You found **{amountWon.ToString("n0")}** {ToastieUtil.RandomEmote()}!\nNow you have **{bal.ToString("n0")}** {ToastieUtil.RandomEmote()}!")
                .Build();
            });
        }

        [Command("Lootboxes"), Summary("Lists your lootboxes.\n**Usage**: `!Lootboxes`")]
        public async Task Lootboxes(IUser user = null, [Remainder] string str = "")
        {
            user = user ?? Context.User;

            var boxes = await LootBoxDb.GetAll(user.Id, Context.Guild.Id);
            if (boxes.Count == 0)
            {
                await Context.Channel.SendMessageAsync("", false, ToastieUtil.NoBoxEmbed(user).Build());
                return;
            }

            await Context.Channel.SendMessageAsync(embed: ToastieUtil.BoxListEmbed(boxes, user).WithDescription("").Build());
        }
    }
}
