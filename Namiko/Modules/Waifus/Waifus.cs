using System.Threading.Tasks;

using System.Linq;
using Discord;
using System;
using Discord.Commands;
using Discord.WebSocket;



using System.Collections.Generic;
using Discord.Addons.Interactive;

namespace Namiko
{
    public class Waifus : InteractiveBase<ShardedCommandContext>
    {
        private static Dictionary<ulong, Object> slideLock = new Dictionary<ulong, Object>();

        [Command("WaifuShop"), Alias("ws"), Summary("Opens the waifu shop.")]
        public async Task WaifuShop([Remainder] string str = "")
        {
            List<ShopWaifu> waifus = await WaifuUtil.GetShopWaifus(Context.Guild.Id);
            int count = Constants.shoplimitedamount + Constants.shopt1amount + Constants.shopt2amount + Constants.shopt3amount;
            string prefix = Program.GetPrefix(Context);

            if (waifus.Count <= count)
            {
                var eb = WaifuUtil.NewShopEmbed(waifus, prefix);
                await Context.Channel.SendMessageAsync("", false, eb.Build());
                return;
            }

            await PagedReplyAsync(WaifuUtil.PaginatedShopMessage(waifus, count, prefix));
        }

        [Command("WaifuShopSlides"), Alias("wss"), Summary("Opens the waifu shop slides.")]
        public async Task WaifuShopSlides([Remainder] string str = "")
        {
            List<ShopWaifu> waifus = await WaifuUtil.GetShopWaifus(Context.Guild.Id);
            waifus.RemoveAt(0);

            var lockObj = new Object();
            if (slideLock.ContainsKey(Context.Channel.Id))
                slideLock.Remove(Context.Channel.Id);
            slideLock.Add(Context.Channel.Id, lockObj);

            var eb = WaifuUtil.WaifuShopSlideEmbed(waifus[0].Waifu);
            var msg = await Context.Channel.SendMessageAsync("", false, eb.Build());

            await Task.Delay(5000);

            for (int i = 1; i < waifus.Count; i++)
            {
                if (slideLock.GetValueOrDefault(Context.Channel.Id) != lockObj)
                    break;
                eb = WaifuUtil.WaifuShopSlideEmbed(waifus[i].Waifu);
                await msg.ModifyAsync(x => x.Embed = eb.Build());
                await Task.Delay(5000);
            }

            eb = WaifuUtil.WaifuShopSlideEmbed(waifus[new Random().Next(waifus.Count)].Waifu);
            eb.WithFooter("Slideshow ended.");
            await msg.ModifyAsync(x => x.Embed = eb.Build());

            if (slideLock.GetValueOrDefault(Context.Channel.Id) == lockObj)
                slideLock.Remove(Context.Channel.Id);
        }

        [Command("BuyWaifu"), Alias("bw"), Summary("Buys a waifu, must be in a shop.\n**Usage**: `!bw [name]`")]
        public async Task BuyWaifu([Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(str, false, WaifuShopDb.GetWaifuStores(Context.Guild.Id).Select(x => x.Waifu)), this);

            if (waifu == null)
            {
                return;
            }
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            if (waifus.Any(x => x.Name.Equals(waifu.Name)))
            {
                await Context.Channel.SendMessageAsync("You already have **" + waifu.Name + "**.");
                return;
            }
            var shopwaifus = WaifuShopDb.GetWaifuStores(Context.Guild.Id);
            ShopWaifu shopWaifu = null;
            foreach (var x in shopwaifus)
            {
                if (x.Waifu.Name.Equals(waifu.Name) && x.Limited != 0)
                {
                    shopWaifu = x;
                }
            }
            if (shopWaifu == null)
            {
                await Context.Channel.SendMessageAsync($"**{waifu.Name}** is not currently for sale! Try the `waifushop` command.");
                return;
            }

            var price = WaifuUtil.GetPrice(waifu.Tier, shopWaifu.Discount);

            try
            {
                await ToastieDb.AddToasties(Context.User.Id, -price, Context.Guild.Id);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync(ex.Message);
                return;
            }

            if (shopWaifu.Limited > 0)
            {
                shopWaifu.BoughtBy = Context.User.Id;
                shopWaifu.Limited -= 1;
                await WaifuShopDb.UpdateWaifu(shopWaifu);
            }

            await UserInventoryDb.AddWaifu(Context.User.Id, waifu, Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"Congratulations! You bought **{waifu.Name}**!", false, WaifuUtil.WaifuEmbedBuilder(waifu).Build());
            await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, price / 13, Context.Guild.Id);
        }

        [Command("SellWaifu"), Alias("sw"), Summary("Sells a waifu you already own for a discounted price.\n**Usage**: `!sw [name]`")]
        public async Task SellWaifu([Remainder] string str = "") {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(str, false, UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id)), this);

            //waifus existance
            if (waifu == null) {
                return;
            }

            //getting waifus u own
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            if (waifus.Any(x => x.Name.Equals(waifu.Name)))
            {
                int worth = WaifuUtil.GetSalePrice(waifu.Tier);

                var sell = new DialogueBoxOption();
                sell.Action = async (IUserMessage message) =>
                {
                    try { await ToastieDb.AddToasties(Context.User.Id, worth, Context.Guild.Id); }
                    catch (Exception ex) { await Context.Channel.SendMessageAsync(ex.Message); }

                    //removing waifu + confirmation
                    await UserInventoryDb.DeleteWaifu(Context.User.Id, waifu, Context.Guild.Id);
                    await message.ModifyAsync(x => {
                        x.Content = $"You sold **{waifu.Name}** for **{worth.ToString("n0")}** toasties.";
                        x.Embed = ToastieUtil.ToastieEmbed(Context.User, ToastieDb.GetToasties(Context.User.Id, Context.Guild.Id)).Build();
                    });
                };
                sell.After = OnExecute.RemoveReactions;

                var cancel = new DialogueBoxOption();
                cancel.After = OnExecute.Delete;

                var dia = new DialogueBox();
                dia.Options.Add(Emote.Parse("<:TickYes:577838859107303424>"), sell);
                dia.Options.Add(Emote.Parse("<:TickNo:577838859077943306>"), cancel);
                dia.Timeout = new TimeSpan(0, 1, 0);
                dia.Embed = new EmbedBuilder()
                    .WithAuthor(Context.User)
                    .WithColor(BasicUtil.RandomColor())
                    .WithDescription($"Sell **{waifu.Name}** for **{worth.ToString("n0")}** toasties?").Build();

                await DialogueReplyAsync(dia);
                return;
            }

            //doesnt have the waifu
            await Context.Channel.SendMessageAsync($"Sure, here's 100k toasties, as imanigary as the waifu you're trying to sell. You don't own her, baaaaaaaaaka.");
        }

        [Command("GiveWaifu"), Alias("gw"), Summary("Transfers waifu to another user.\n**Usage**: `!gw [user] [waifu_name]`")]
        public async Task GiveWaifu(IUser recipient, [Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(str, false, UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id)), this);

            if (waifu == null)
            {
                return;
            }
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            if (!(waifus.Any(x => x.Name.Equals(waifu.Name))))
            {
                await Context.Channel.SendMessageAsync($"**{waifu.Name}** is just like my love - you don't have it.");
                return;
            }
            waifus = UserInventoryDb.GetWaifus(recipient.Id, Context.Guild.Id);
            if (waifus.Any(x => x.Name.Equals(waifu.Name)))
            {
                await Context.Channel.SendMessageAsync($"They already have **{waifu.Name}**.");
                return;
            }

            await UserInventoryDb.AddWaifu(recipient.Id, waifu, Context.Guild.Id);
            await UserInventoryDb.DeleteWaifu(Context.User.Id, waifu, Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"{recipient.Mention} You received **{waifu.Name}** from {Context.User.Mention}!", false, WaifuUtil.WaifuEmbedBuilder(waifu).Build());
        }

        [Command("Waifu"), Alias("Husbando", "Trap", "w"), Summary("Shows waifu details.\n**Usage**: `!waifu [search]`")]
        public async Task ShowWaifu([Remainder] string name)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), this);
            if (waifu == null)
            {
                //await Context.Channel.SendMessageAsync($"Can't find '{name}'. I know they are not real, but this one *really is* just your imagination >_>");
                return;
            }

            var eb = WaifuUtil.WaifuEmbedBuilder(waifu, true, Context);

            await Context.Channel.SendMessageAsync("", false, eb.Build());

        }

        [Command("TopWaifus"), Alias("tw"), Summary("Shows most popular waifus.\n**Usage**: `!tw`")]
        public async Task TopWaifus([Remainder] string str = "")
        {
            var AllWaifus = UserInventoryDb.GetAllWaifuItems();
            var waifus = new Dictionary<Waifu, int>();

            foreach (var x in AllWaifus)
            {
                if (!waifus.ContainsKey(x.Waifu))
                    waifus[x.Waifu] = 0;
                waifus[x.Waifu]++;
            }

            var ordWaifus = waifus.OrderByDescending(x => x.Value);
            var msg = new CustomPaginatedMessage();

            msg.Title = ":two_hearts: Waifu Leaderboards";
            var fields = new List<FieldPages>();
            fields.Add(new FieldPages
            {
                Title = "Globaly Bought",
                Pages = CustomPaginatedMessage.PagesArray(ordWaifus, 10, (x) => $"**{x.Key.Name}** - {x.Value}\n")
            });
            msg.Fields = fields;
            msg.ThumbnailUrl = ordWaifus.First().Key.ImageUrl;

            await PagedReplyAsync(msg);
        }

        [Command("ServerTopWaifus"), Alias("stw"), Summary("Shows most popular waifus in the server.\n**Usage**: `!stw`")]
        public async Task ServerTopWaifus([Remainder] string str = "")
        {
            var AllWaifus = UserInventoryDb.GetAllWaifuItems(Context.Guild.Id);
            var waifus = new Dictionary<Waifu, int>();

            foreach (var x in AllWaifus)
            {
                if (!waifus.ContainsKey(x.Waifu))
                    waifus.Add(x.Waifu, AllWaifus.Count(y => y.Waifu.Equals(x.Waifu)));
            }

            var ordWaifus = waifus.OrderByDescending(x => x.Value);
            var msg = new CustomPaginatedMessage();

            msg.Title = ":two_hearts: Waifu Leaderboards";
            var fields = new List<FieldPages>();
            fields.Add(new FieldPages
            {
                Title = "Bought Here",
                Pages = CustomPaginatedMessage.PagesArray(ordWaifus, 10, (x) => $"**{x.Key.Name}** - {x.Value}\n")
            });
            msg.Fields = fields;
            msg.ThumbnailUrl = ordWaifus.First().Key.ImageUrl;

            await PagedReplyAsync(msg);
        }

        [Command("WaifuLeaderboard"), Alias("wlb"), Summary("Shows waifu worth of each person.\n**Usage**: `!wlb`")]
        public async Task WaifuLeaderboard([Remainder] string str = "")
        {
            var AllWaifus = UserInventoryDb.GetAllWaifuItems(Context.Guild.Id);
            var users = new Dictionary<SocketUser, int>();

            foreach (var x in AllWaifus)
            {
                var user = Context.Guild.GetUser(x.UserId);
                if (user != null)
                    if (!users.ContainsKey(user))
                        users.Add(user, WaifuUtil.WaifuValue(UserInventoryDb.GetWaifus(user.Id, Context.Guild.Id)));
            }

            var ordUsers = users.OrderByDescending(x => x.Value);

            var msg = new CustomPaginatedMessage();

            msg.Title = "User Leaderboards";
            var fields = new List<FieldPages>();
            fields.Add(new FieldPages
            {
                Title = "Waifu Value <:toastie3:454441133876183060>",
                Pages = CustomPaginatedMessage.PagesArray(ordUsers, 10, (x) => $"{x.Key.Mention} - {x.Value}\n")
            });
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }

        [Command("WishWaifu"), Alias("ww"), Summary("Add a waifu to your wishlist to be notified when it appears in shop.\nLimited to 5.\n**Usage**: `!ww [waifu]`")]
        public async Task WishWaifu([Remainder] string str = "")
        {
            var user = Context.User;
            Waifu waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(str), this);
            if (waifu == null)
            {
                return;
            }

            var waifus = WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);
            if (waifus.Any(x => x.Name == waifu.Name))
            {
                await Context.Channel.SendMessageAsync($"**{waifu.Name}** is already in your wishlist. Baka.");
                return;
            }

            if(UserInventoryDb.OwnsWaifu(user.Id, waifu, Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync($"You already own **{waifu.Name}**. Baka.");
                return;
            }

            await WaifuWishlistDb.AddWaifuWish(Context.User.Id, waifu, Context.Guild.Id);
            
            waifus = WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"Added **{waifu.Name}** to your wishlist!", false, WaifuUtil.WishlistEmbed(waifus, (SocketGuildUser)user).Build());
        }

        [Command("WaifuWishlist"), Alias("wwl"), Summary("Shows yours or someone's waifu wishlist.\n**Usage**: `!wwl [user_optional]`")]
        public async Task WaifuWishlist(IUser user = null, [Remainder] string str = "")
        {
            user = user ?? Context.User;
            var waifus = WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);

            await Context.Channel.SendMessageAsync(null, false, WaifuUtil.WishlistEmbed(waifus, (SocketGuildUser)user).Build());
        }

        [Command("RemoveWaifuWish"), Alias("rww"), Summary("Removes a waifu from your wishlist.\n**Usage**: `!rww [waifu]`")]
        public async Task RemoveWaifuWish([Remainder] string str = "")
        {
            var user = Context.User;
            Waifu waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(str, false, WaifuWishlistDb.GetWishlist(Context.User.Id, Context.Guild.Id)), this);
            if (waifu == null)
            {
                return;
            }
            var waifus = WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);
            if(!waifus.Any(x => x.Name == waifu.Name))
            {
                await Context.Channel.SendMessageAsync($"**{waifu.Name}** is not in your wishlist. Baka.");
                return;
            }

            await WaifuWishlistDb.DeleteWaifuWish(user.Id, waifu, Context.Guild.Id);
            await Context.Channel.SendMessageAsync("You don't want her anymore, huh...");
        }

        [Command("NewWaifu"), Alias("nw"), Summary("Adds a waifu to the database.\n**Usage**: `!nw [name] [tier(1-3)] [image_url]`"), HomePrecondition]
        public async Task NewWaifu(string name, int tier, string url = null)
        {
            if (url != null)
            {
                if (!((url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") || url.EndsWith(".gif") || url.EndsWith(".gifv") || url.EndsWith(".mp4")) && (Uri.TryCreate(url, UriKind.Absolute, out var outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))))
                {
                    await Context.Channel.SendMessageAsync("URL is invalid. Note: URL has to end with .jpg .jpeg .png .gif .gifv or .mp4");
                    return;
                }
            }

            var waifu = new Waifu { Name = name, Tier = tier, ImageUrl = url, Description = null, LongName = null, TimesBought = 0 };

            if (await WaifuDb.AddWaifu(waifu) > 0)
                await Context.Channel.SendMessageAsync($"{name} added.");
            else
                await Context.Channel.SendMessageAsync($"Failed to add {name}");
        }

        [Command("DeleteWaifu"), Alias("dw"), Summary("Removes a waifu from the database.\n**Usage**: `!dw [name]`"), HomePrecondition]
        public async Task DeleteWaifu(string name)
        {
            if (await WaifuDb.DeleteWaifu(name) > 0)
                await Context.Channel.SendMessageAsync($"{name} deleted.");
            else
                await Context.Channel.SendMessageAsync($"Failed to delete {name}");
        }

        [Command("WaifuFullName"), Alias("wfn"), Summary("Changes the full name of a waifu.\n**Usage**: `!wfn [name] [fullname]`"), HomePrecondition]
        public async Task WaifuFullName(string name, [Remainder] string fullname = null)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            waifu.LongName = fullname;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await Context.Channel.SendMessageAsync($":white_check_mark: {waifu.Name} updated.");
            else
                await Context.Channel.SendMessageAsync($":x: Failed to update {name}");
        }

        [Command("WaifuDescription"), Alias("wd"), Summary("Changes the description of a waifu.\n**Usage**: `!wd [name] [description]`"), HomePrecondition]
        public async Task WaifuDescription(string name, [Remainder] string description = null)
        {

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            waifu.Description = description;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await Context.Channel.SendMessageAsync($":white_check_mark: {waifu.Name} updated.");
            else
                await Context.Channel.SendMessageAsync($":x: Failed to update {name}");
        }

        [Command("WaifuSource"), Alias("wsrc"), Summary("Changes the source of a waifu.\n**Usage**: `!ws [name] [source]`"), HomePrecondition]
        public async Task WaifuSource(string name, [Remainder] string source = null)
        {

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            waifu.Source = source;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await Context.Channel.SendMessageAsync($":white_check_mark: {waifu.Name} updated.");
            else
                await Context.Channel.SendMessageAsync($":x: Failed to update {name}");
        }

        [Command("WaifuTier"), Alias("wt"), Summary("Changes the tier of a waifu.\n**Usage**: `!wt [name] [tier(1-3)]`"), HomePrecondition]
        public async Task WaifuTier(string name, int tier)
        {

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            waifu.Tier = tier;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await Context.Channel.SendMessageAsync($":white_check_mark: {waifu.Name} updated.");
            else
                await Context.Channel.SendMessageAsync($":x: Failed to update {name}");
        }

        [Command("WaifuImage"), Alias("wi"), Summary("Changes the image of a waifu.\n**Usage**: `!wi [name] [image_url]`"), HomePrecondition]
        public async Task WaifuImage(string name, string url = null)
        {

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            if (url != null)
            {
                if (!((url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") || url.EndsWith(".gif") || url.EndsWith(".gifv")) && (Uri.TryCreate(url, UriKind.Absolute, out var outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))))
                {
                    await Context.Channel.SendMessageAsync("URL is invalid. Note: URL has to end with .jpg .jpeg .png .gif or .gifv");
                    return;
                }
            }
            waifu.ImageUrl = url;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await Context.Channel.SendMessageAsync($":white_check_mark: {waifu.Name} updated.");
            else
                await Context.Channel.SendMessageAsync($":x: Failed to update {name}");
        }

        [Command("ResetWaifuShop"), Alias("rws"), Summary("Resets the waifu shop contents."), HomePrecondition]
        public async Task ResetWaifuShop()
        {
            await WaifuShopDb.NewList(WaifuUtil.GenerateWaifuList(Context.Guild.Id));
            await WaifuShop();
        }
    }
}