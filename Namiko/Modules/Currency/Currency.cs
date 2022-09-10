using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Criteria;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Namiko.Modules.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    [RequireGuild]
    [Name("Currency & Gambling")]
    public class Currency : CustomModuleBase<ICustomContext>
    {
        [Command("Blackjack"), Alias("bj"), Description("Starts a game of blackjack.\n**Usage**: `!bj [amount]`")]
        [SlashCommand("blackjack", "Start a game of blackjack")]
        public async Task BlackjackCommand(string sAmount)
        {
            var user = (SocketGuildUser)Context.User;
            var ch = (SocketTextChannel)Context.Channel;

            int amount = CurrencyUtil.ParseAmount(sAmount, user);
            if (amount < 0)
            {
                await ReplyAsync("Pick an amount! some number, 'all', 'half', or x/y.");
                return;
            }
            if (amount == 0)
            {
                await ReplyAsync("You have no toasties...");
                return;
            }

            try
            {
                await BalanceDb.AddToasties(user.Id, -amount, Context.Guild.Id);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
                return;
            }

            if (BalanceDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id) < amount)
            {
                string prefix = GetPrefix();
                await ReplyAsync("Tch, I don't have enough toasties... I will recover eventually by stealing toasties from all of you. :fox:" +
                    $"But if you want to speed up the process you can give me some using the `{prefix}give` or `{prefix}at` commands.");
                await BalanceDb.AddToasties(user.Id, amount, Context.Guild.Id);
                return;
            }

            BlackjackGame game = new BlackjackGame(amount, Context, Interactive);

            var box = new DialogueBox(Blackjack.StartEmbed(Context, game).Build());
            box.Options.Add("hit", new DialogueBoxOption(new ButtonBuilder("Hit", "hit", ButtonStyle.Primary), x => Blackjack.Hit(Context, game), DisposeLevel.Continue));
            box.Options.Add("stand", new DialogueBoxOption(new ButtonBuilder("Stand", "stand", ButtonStyle.Primary), x => Blackjack.Stand(Context, game), DisposeLevel.RemoveComponents));
            box.Options.Add("forfeit", new DialogueBoxOption(new ButtonBuilder("Forfeit", "forfeit", ButtonStyle.Danger), x => Blackjack.Forfeit(Context, game), DisposeLevel.RemoveComponents));
            box.Options.Add("double", new DialogueBoxOption(new ButtonBuilder("Double", "double", ButtonStyle.Success), x => Blackjack.DoubleDown(Context, game), DisposeLevel.RemoveComponents));

            var msg = await DialogueReplyAsync(box, timeout: 0);
            game.Message = msg;
        }

        [Command("Daily"), Alias("dailies", "daywy", "daiwy"), Description("Gives daily toasties.")]
        [SlashCommand("daily", "Claim a daily reward")]
        public async Task DailyCmd()
        {
            bool newDaily = false;
            Daily daily = DailyDb.GetDaily(Context.User.Id, Context.Guild.Id);
            if (daily == null)
            {
                daily = new Daily
                {
                    UserId = Context.User.Id,
                    GuildId = Context.Guild.Id,
                    Date = 0
                };
                newDaily = true;
            }

            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int ms = 72000000;
            if (PremiumDb.IsPremium(Context.User.Id, ProType.ProPlus))
                ms /= 2;

            long dayslate = 0;
            if ((daily.Date + ms) < timeNow)
            {
                if ((daily.Date + 172800000) < timeNow && !newDaily)
                {
                    long mslate = timeNow - (daily.Date + 172800000);
                    dayslate = (mslate / (1000 * 60 * 60 * 24)) + 1;
                    double multiplier = dayslate > 3 ? 0 : 1 - dayslate * 0.25;
                    daily.Streak = (int)(daily.Streak * multiplier);
                }

                daily.Streak++;
                daily.Date = timeNow;
                int amount = CurrencyUtil.DailyAmount(daily.Streak);
                int tax = CurrencyUtil.DailyTax(amount, BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id), BalanceDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id), await BalanceDb.TotalToasties(Context.Guild.Id));
                amount -= tax / 2;
                //int cap = Constants.dailycap;
                //amount = amount > cap ? cap : amount;

                await DailyDb.SetDaily(daily);
                await BalanceDb.AddToasties(Context.User.Id, amount, Context.Guild.Id);
                await BalanceDb.AddToasties(Context.Client.CurrentUser.Id, tax, Context.Guild.Id);

                await ReplyAsync(dayslate == 0 ? "" : $"You are **{dayslate}** days late to claim your daily. For every day missed, you lose 25% of your streak.", false, CurrencyUtil.DailyGetEmbed(Context.User, daily.Streak, amount, BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id), GetPrefix()).Build());
            }
            else
            {
                long wait = (daily.Date + ms - timeNow) / 1000;
                int hours = (int)wait / 3600;
                int minutes = (int)wait % 3600 / 60;
                int seconds = (int)wait % 60;
                await ReplyAsync("", false, CurrencyUtil.DailyWaitEmbed(Context.User, hours, minutes, seconds, GetPrefix()).Build());
            }
        }

        [Command("Weekly"), Alias("weeklies", "weekwy"), Description("Gives weekly toasties.")]
        [SlashCommand("weekly", "Claim a weekly reward")]
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

            int hours = 164;
            if (PremiumDb.IsPremium(Context.Guild.Id, ProType.Guild) || PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
                hours /= 2;

            if (weekly.Date.AddHours(hours).CompareTo(DateTime.Now) < 0)
            {
                int streak = await DailyDb.GetHighest(Context.Guild.Id) + 15;
                int amount = CurrencyUtil.DailyAmount(streak);
                int tax = CurrencyUtil.DailyTax(amount, BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id), BalanceDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id), await BalanceDb.TotalToasties(Context.Guild.Id));
                amount -= tax / 2;
                if (PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
                    amount += 1000;

                string text = "";
                if (PremiumDb.IsPremium(Context.User.Id, ProType.ProPlus))
                {
                    await LootBoxDb.AddLootbox(Context.User.Id, LootBoxType.WaifuT1, 1, Context.Guild.Id);
                    text = "You receive a T1 Waifu lootbox! :star2:";
                }

                await BalanceDb.AddToasties(Context.User.Id, amount, Context.Guild.Id);
                await BalanceDb.AddToasties(Context.Client.CurrentUser.Id, tax / 2, Context.Guild.Id);
                weekly.Date = DateTime.Now;
                await WeeklyDb.SetWeekly(weekly);
                await ReplyAsync(text, false, CurrencyUtil.WeeklyGetEmbed(amount, BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id), Context.User, GetPrefix()).Build());
                return;
            }

            await ReplyAsync("", false, CurrencyUtil.WeeklyWaitEmbed(weekly.Date.AddHours(hours), Context.User, GetPrefix()).Build());
        }

        [Command("Flip"), Alias("f", "fwip"), Description("Flip a coin for toasties, defaults to tails.\n**Usage**: `!flip [amount] [heads_or_tails]`")]
        [SlashCommand("flip", "Flip a coin for toasties, defaults to tails")]
        public async Task Flip(string sAmount, string side = "t")
        {
            var user = (SocketGuildUser)Context.User;
            var rnd = new Random();
            side = side.ToLower();

            if (!(side.Equals("t") || side.Equals("h") || side.Equals("tails") || side.Equals("heads")))
            {
                await ReplyAsync("Pick heads or tails!");
                return;
            }

            int amount = CurrencyUtil.ParseAmount(sAmount, user);
            if (amount < 0)
            {
                await ReplyAsync("Pick an amount! number, all, half, or x/y.");
                return;
            }
            if (amount == 0)
            {
                await ReplyAsync("You have no toasties...");
                return;
            }

            try
            {
                await BalanceDb.AddToasties(user.Id, -amount, Context.Guild.Id);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
                return;
            }

            if (BalanceDb.GetToasties(Context.Client.CurrentUser.Id, Context.Guild.Id) < amount)
            {
                string prefix = GetPrefix();
                await ReplyAsync("Tch, I don't have enough toasties... I will recover eventually by stealing toasties from all of you. :fox:" +
                    $"But if you want to speed up the process you can give me some using the `{prefix}give` or `{prefix}at` commands.");
                await BalanceDb.AddToasties(user.Id, amount, Context.Guild.Id);
                return;
            }

            if (CurrencyUtil.FiftyFifty())
            {
                await BalanceDb.AddToasties(user.Id, amount * 2, Context.Guild.Id);
                await BalanceDb.AddToasties(Context.Client.CurrentUser.Id, -amount, Context.Guild.Id);
                string resp = "";
                if (amount >= 50000 && rnd.Next(5) == 1)
                    resp = "Tch... I'll get you next time.";
                await ReplyAsync(resp, false, CurrencyUtil.FlipWinEmbed(user, amount).Build());
            }
            else
            {
                await BalanceDb.AddToasties(Context.Client.CurrentUser.Id, amount, Context.Guild.Id);
                string resp = "";
                if (amount >= 50000 && rnd.Next(5) == 1)
                    resp = "I'll take those.";
                await ReplyAsync(resp, false, CurrencyUtil.FlipLoseEmbed(user, amount).Build());
            }
        }

        [Command("Balance"), Alias("toastie", "bal", "toasties"), Description("Shows amount of toasties.\n**Usage**: `!bal [user_optional]`")]
        [SlashCommand("balance", "See how many toasties are had")]
        public async Task Toastie([Remainder] IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }
            await ReplyAsync("", false, CurrencyUtil.ToastieEmbed(user, BalanceDb.GetToasties(user.Id, Context.Guild.Id)).Build());
        }

        [Command("Give"), Description("Give a user some of your toasties.\n**Usage**: `!give [user] [amount]`")]
        [SlashCommand("give", "Give someone toasties")]
        public async Task Give(IUser recipient, string sAmount)
        {
            int amount = CurrencyUtil.ParseAmount(sAmount, (SocketGuildUser)Context.User);
            if (amount < 0)
            {
                await ReplyAsync("Pick an amount! number, all, half, or x/y.");
                return;
            }
            if (amount == 0)
            {
                await ReplyAsync("You have no toasties...");
                return;
            }

            try
            {
                await BalanceDb.AddToasties(Context.User.Id, -amount, Context.Guild.Id);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
                return;
            }

            await BalanceDb.AddToasties(recipient.Id, amount, Context.Guild.Id);

            await ReplyAsync("", false, CurrencyUtil.GiveEmbed(Context.User, recipient, amount).Build());
        }
        
        [Command("Give"), Description("Give a user some of your toasties.\n**Usage**: `!give [user] [amount]`")]
        public async Task Give(string sAmount, IUser recipient)
        {
            await Give(recipient, sAmount);
        }

        [UserPermission(GuildPermission.Administrator)]
        [Command("SetToasties"), Alias("st", "sett"), Description("Sets the amount of toasties.\n**Usage**: `!st [user] [amount]`")]
        [SlashCommand("set-toasties", "Set balance")]
        public async Task Set(IUser user, int amount)
        {
            await BalanceDb.SetToasties(user.Id, amount, Context.Guild.Id);
            await ReplyAsync("", false, CurrencyUtil.ToastieEmbed(user, BalanceDb.GetToasties(user.Id, Context.Guild.Id)).Build());
        }

        [UserPermission(GuildPermission.Administrator)]
        [Command("AddToasites"), Alias("at", "addt"), Description("Adds toasties to a user.\n**Usage**: `!at [user] [amount]`")]
        [SlashCommand("add-toasties", "Add balance")]
        public async Task Add(IUser user, int amount)
        {
            await BalanceDb.AddToasties(user.Id, amount, Context.Guild.Id);
            await ReplyAsync("", false, CurrencyUtil.ToastieEmbed(user, BalanceDb.GetToasties(user.Id, Context.Guild.Id)).Build());
        }

        [Command("LootboxShop"), Alias("ls"), Description("Lootbox shop.\n**Usage**: `!ls`")]
        [SlashCommand("lootbox-shop", "Lootbox shop")]
        public async Task LootboxShop()
        {
            await ReplyAsync(embed: CurrencyUtil.BoxShopEmbed(Context.User).Build());
        }

        [Command("BuyLootbox"), Alias("bl"), Description("Buy a lootbox.\n**Usage**: `!bl [name]`")]
        [SlashCommand("buy-lootbox", "Buy a lootbox")]
        public async Task BuyLootbox()
        {
            var prefix = GetPrefix();

            if (!PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ This command requires Pro Guild+ ~*\n" +
                    $"Try `{prefix}lootboxstats` to see drop rates.")
                    .WithFooter($"`{prefix}pro`")
                    .Build());
                return;
            }

            LootboxStat box = null;
            var boxes = LootboxStats.Lootboxes.Where(x => x.Value.Price >= 0).Select(x => x.Value).ToList();

            var eb = CurrencyUtil.BoxShopEmbed(Context.User).Build();
            Func<LootboxStat, SelectMenuOptionBuilder> func = x => new SelectMenuOptionBuilder(x.TypeId.ToString(), x.TypeId.ToString(), emote: Emote.Parse(x.Emote));
            box = await Select(boxes, "Lootbox", eb, x => x.Name, null, x => Emote.Parse(x.Emote));

            var type = LootboxStats.Lootboxes[box.TypeId];
            int amount = await Select(new List<int> { 1, 2, 3, 4, 5, 10, 20, 50, 100 }, $"How many {type.Emote} **{box.Name}** lootboxes do you wish to buy?", "Select amount");

            try
            {
                await BalanceDb.AddToasties(Context.User.Id, -box.Price * amount, Context.Guild.Id);
            } catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
                return;
            }

            await LootBoxDb.AddLootbox(Context.User.Id, (LootBoxType)box.TypeId, amount, Context.Guild.Id);
            await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                .WithDescription($"You bought {amount}x **{box.Name}**!\nType `{GetPrefix()}open` to open it!")
                .Build());
        }

        [Command("LootboxStats"), Description("Shows the stats of all lootboxes.\n**Usage**: `!lootboxstats`")]
        [SlashCommand("lootbox-stats", "See lootbox drop rates")]
        public async Task ShowLootboxStats()
        {
            await ReplyAsync(embed: CurrencyUtil.BoxStatsEmbed(Context.User).Build());
        }

        [Command("Beg"), Description("Beg Namiko for toasties.\n**Usage**: `!beg`")]
        [SlashCommand("beg", "Beg Namiko for toasties, loser")]
        public async Task Beg()
        {
            var amount = BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id);
            if (amount > 0)
            {
                await ReplyAsync("You already have toasties, you snob.");
                return;
            }

            if (!Constants.beg)
            {
                await ReplyAsync(CurrencyUtil.GetFalseBegMessage());
                return;
            }

            amount = Constants.begAmount;
            await BalanceDb.AddToasties(Context.User.Id, amount, Context.Guild.Id);

            await ReplyAsync("Fine. Just leave me alone.", false, CurrencyUtil.GiveEmbed(Context.Client.CurrentUser, Context.User, amount).Build());
        }

        [Command("Open"), Alias("OpenLootbox", "Lootbox", "Lootwox"), Description("Open a lootbox if you have one.\n**Usage**: `!open`")]
        public async Task Open()
        {
            var boxes = await LootBoxDb.GetAll(Context.User.Id, Context.Guild.Id);
            if(boxes.Count == 0)
            {
                await ReplyAsync("", false, CurrencyUtil.NoBoxEmbed(Context.User).Build());
                return;
            }

            var box = await this.SelectLootbox(boxes);
            var type = LootboxStats.Lootboxes[box.Type];

            try
            {
                await LootBoxDb.AddLootbox(Context.User.Id, box.Type, -1, box.GuildId);
            } catch
            {
                await ReplyAsync("You tried.");
                return;
            }
            var msg = await ReplyAsync("", false, CurrencyUtil.BoxOpeningEmbed(Context.User).Build());
            await ProfileDb.IncrementLootboxOpened(Context.User.Id);
            int waitms = 4200;

            if (type.IsWaifu())
            {
                bool isPremium = PremiumDb.IsPremium(Context.User.Id, ProType.Pro);
                var waifu = await CurrencyUtil.UnboxWaifu(type, isPremium, Context.User.Id, Context.Guild.Id);
                while(UserInventoryDb.OwnsWaifu(Context.User.Id, waifu, Context.Guild.Id))
                    waifu = await CurrencyUtil.UnboxWaifu(type, isPremium, Context.User.Id, Context.Guild.Id);

                await UserInventoryDb.AddWaifu(Context.User.Id, waifu, Context.Guild.Id);
                await Task.Delay(waitms);
                var embed = WaifuUtil.WaifuEmbedBuilder(waifu, Context).Build();
                await msg.ModifyAsync(x => {
                    x.Embed = embed;
                    x.Content = $"{Context.User.Mention} Congratulations! You found **{waifu.Name}**!";
                });
                return;
            }

            var amountWon = type.GetRandomToasties();
            await BalanceDb.AddToasties(Context.User.Id, amountWon, Context.Guild.Id);
            var bal = BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id);
            await Task.Delay(waitms);
            await msg.ModifyAsync(x => {
                x.Embed = new EmbedBuilder()
                .WithAuthor($"{Context.User} | {box.Type.ToString()}", Context.User.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-lootbox"))
                .WithColor(BasicUtil.RandomColor())
                .WithThumbnailUrl("https://i.imgur.com/4JQmxa6.png")
                .WithDescription($"Congratulations! You found **{amountWon.ToString("n0")}** {CurrencyUtil.RandomEmote()}!\nNow you have **{bal.ToString("n0")}** {CurrencyUtil.RandomEmote()}!")
                .Build();
            });
        }

        [Command("BulkOpen"), Description("Open multiple lootboxes at a time.\n**Usage**: `!open`")]
        [SlashCommand("open", "Open lootboxes")]
        public async Task BulkOpen()
        {
            var boxes = await LootBoxDb.GetAll(Context.User.Id, Context.Guild.Id);
            if (boxes.Count == 0)
            {
                await ReplyAsync("", false, CurrencyUtil.NoBoxEmbed(Context.User).Build());
                return;
            }

            LootBox box = await Select(boxes, "Lootbox", CurrencyUtil.BoxListEmbed(boxes, Context.User).Build());

            var type = LootboxStats.Lootboxes[box.Type];
            var options = new List<int>{ 1, 2, 3, 4, 5, 10, 20, 50, 100 }.Where(x => x < box.Amount).ToList();
            options.Add(box.Amount);
            int amount = await Select(options.Distinct(), $"How many {type.Emote} **{type.Name}** lootboxes do you wish to open?", "Amount");

            try
            {
                await LootBoxDb.AddLootbox(Context.User.Id, box.Type, -amount, box.GuildId);
            }
            catch
            {
                await ReplyAsync("You tried.");
                return;
            }

            var msg = await ReplyAsync("", false, CurrencyUtil.BoxOpeningEmbed(Context.User).Build());
            _ = ProfileDb.IncrementLootboxOpened(Context.User.Id, amount);
            int waitms = 4200;

            int toasties = 0;
            var waifusFound = new List<Waifu>();
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            bool isPremium = PremiumDb.IsPremium(Context.User.Id, ProType.Pro);

            for (int i = 0; i < amount; i++)
            {
                if (type.IsWaifu())
                {
                    var waifu = await CurrencyUtil.UnboxWaifu(type, isPremium, Context.User.Id, Context.Guild.Id);
                    while (waifu == null || waifus.Any(x => x.Name.Equals(waifu.Name)) || waifusFound.Any(x => x.Name.Equals(waifu.Name)))
                        waifu = await CurrencyUtil.UnboxWaifu(type, isPremium, Context.User.Id, Context.Guild.Id);

                    waifusFound.Add(waifu);
                    await UserInventoryDb.AddWaifu(Context.User.Id, waifu, Context.Guild.Id);
                }
                else
                {
                    toasties += type.GetRandomToasties();
                }
            }

            await BalanceDb.AddToasties(Context.User.Id, toasties, Context.Guild.Id);
            var bal = BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id);

            await Task.Delay(waitms);
            var eb = new EmbedBuilder()
                .WithAuthor($"{Context.User} | {box.Type.ToString()} x{amount}", Context.User.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-lootbox"))
                .WithColor(BasicUtil.RandomColor())
                .WithThumbnailUrl("https://i.imgur.com/4JQmxa6.png");

            string desc = $"You found **{toasties.ToString("n0")}** {CurrencyUtil.RandomEmote()}!\nNow you have **{bal.ToString("n0")}** {CurrencyUtil.RandomEmote()}!\n\n";
            if (waifusFound.Any())
            {
                desc += "**Waifus Found:**\n";
                foreach (var w in waifusFound)
                {
                    desc += $"`T{w.Tier}` **{w.Name}** - *{(w.Source.Length > 37 ? w.Source.Substring(0, 33) + "..." : w.Source)}*\n";
                }
            }

            eb.WithDescription(desc.Length > 2000 ? desc.Substring(0, 1970) + "\n*And more...*" : desc);
            await msg.ModifyAsync(x => {
                x.Embed = eb.Build();
            });
        }

        [Command("Lootboxes"), Description("Lists your lootboxes.\n**Usage**: `!Lootboxes`")]
        [SlashCommand("lootboxes", "Your lootboxes")]
        public async Task Lootboxes([Remainder] IUser user = null)
        {
            user ??= Context.User;

            var boxes = await LootBoxDb.GetAll(user.Id, Context.Guild.Id);
            if (boxes.Count == 0)
            {
                await ReplyAsync("", false, CurrencyUtil.NoBoxEmbed(user).Build());
                return;
            }

            await ReplyAsync(embed: CurrencyUtil.BoxListEmbed(boxes, user).WithDescription("").Build());
        }
    }
}
