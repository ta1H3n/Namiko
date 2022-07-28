using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Namiko
{
    [RequireGuild]
    public class Waifus : CustomModuleBase<ICustomContext>
    {
        private static readonly Dictionary<ulong, Object> slideLock = new Dictionary<ulong, Object>();

        [Command("WaifuShop"), Alias("ws"), Description("Opens the waifu shop.")]
        public async Task WaifuShop([Remainder] string str = "")
        {
            WaifuShop shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Waifu);
            int count = Constants.shoplimitedamount + Constants.shopt1amount + Constants.shopt2amount + Constants.shopt3amount;
            string prefix = Program.GetPrefix(Context);
            var waifus = shop.ShopWaifus.OrderByDescending(x => x.Limited).ThenBy(x => x.Waifu.Tier).ThenBy(x => x.Waifu.Source).ToList();

            if (waifus.Count <= count)
            {
                var eb = WaifuUtil.NewShopEmbed(waifus, prefix, Context.Guild.Id);
                await ReplyAsync("", false, eb.Build());
                return;
            }

            await PagedReplyAsync(WaifuUtil.PaginatedShopMessage(waifus, count, prefix, Context.Guild.Id));
        }

        [Command("GachaShop"), Alias("gs"), Description("Opens the gacha shop.")]
        public async Task GachaShop([Remainder] string str = "")
        {
            WaifuShop shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Gacha);
            string prefix = Program.GetPrefix(Context);
            var waifus = shop.ShopWaifus.OrderByDescending(x => x.Limited).ThenBy(x => x.Waifu.Tier).ThenBy(x => x.Waifu.Source).ToList();

            var eb = WaifuUtil.NewShopEmbed(waifus, prefix, Context.Guild.Id, ShopType.Gacha);
            await ReplyAsync("", false, eb.Build());
        }

        [Command("ModShop"), Alias("ms"), Description("Opens the mod shop. A waifu shop controlled by server moderators.")]
        public async Task ModShop([Remainder] string str = "")
        {
            WaifuShop shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Mod);
            int count = Constants.shoplimitedamount + Constants.shopt1amount + Constants.shopt2amount + Constants.shopt3amount;
            string prefix = Program.GetPrefix(Context);
            List<ShopWaifu> waifus = new List<ShopWaifu>();
            if (shop != null)
                waifus = shop.ShopWaifus;

            waifus = waifus.OrderBy(x => x.Waifu.Tier).ThenBy(x => x.Waifu.Source).ThenBy(x => x.Waifu.Name).ToList();

            if (waifus.Count <= count)
            {
                var eb = WaifuUtil.NewShopEmbed(waifus, prefix, Context.Guild.Id, ShopType.Mod);
                await ReplyAsync("", false, eb.Build());
                return;
            }

            await PagedReplyAsync(WaifuUtil.PaginatedShopMessage(waifus, count, prefix, Context.Guild.Id, ShopType.Mod));
        }

        [Command("WaifuShopSlides"), Alias("wss"), Description("Opens the waifu shop slides.")]
        public async Task WaifuShopSlides([Remainder] string str = "")
        {
            List<ShopWaifu> waifus = (await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Waifu)).ShopWaifus;
            waifus.RemoveAt(0);

            var lockObj = new Object();
            if (slideLock.ContainsKey(Context.Channel.Id))
                slideLock.Remove(Context.Channel.Id);
            slideLock.Add(Context.Channel.Id, lockObj);

            var eb = WaifuUtil.WaifuShopSlideEmbed(waifus[0].Waifu);
            var msg = await ReplyAsync("", false, eb.Build());

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

        [Command("BuyWaifu"), Alias("bw"), Description("Buys a waifu, must be in a shop.\n**Usage**: `!bw [name]`")]
        public async Task BuyWaifu([Remainder] string str = "")
        {
            var shopwaifus = (await WaifuShopDb.GetAllShopWaifus(Context.Guild.Id)).DistinctBy(x => x.WaifuName);

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str, false, shopwaifus.Select(x => x.Waifu)), this);

            if (waifu == null)
            {
                return;
            }
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            if (waifus.Any(x => x.Name.Equals(waifu.Name)))
            {
                await ReplyAsync("You already have **" + waifu.Name + "**.");
                return;
            }

            ShopWaifu shopWaifu = shopwaifus.FirstOrDefault(x => x.Waifu.Equals(waifu) && x.Limited != 0);
            if (shopWaifu == null)
            {
                await ReplyAsync($"**{waifu.Name}** is not currently for sale! Try the `waifushop` command.");
                return;
            }

            var price = WaifuUtil.GetPrice(waifu.Tier, shopWaifu.Discount);

            try
            {
                await BalanceDb.AddToasties(Context.User.Id, -price, Context.Guild.Id);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
                return;
            }

            await UserInventoryDb.AddWaifu(Context.User.Id, waifu, Context.Guild.Id);
            await ReplyAsync($"Congratulations! You bought **{waifu.Name}**!", false, WaifuUtil.WaifuEmbedBuilder(waifu).Build());
            await BalanceDb.AddToasties(Context.Client.CurrentUser.Id, price / 13, Context.Guild.Id);

            if (shopWaifu.Limited > 0)
            {
                shopWaifu.BoughtBy = Context.User.Id;
                shopWaifu.Limited -= 1;
                await WaifuShopDb.UpdateItem(shopWaifu);
            }
        }

        [Command("SellWaifu"), Alias("sw"), Description("Sells a waifu you already own for a discounted price.\n**Usage**: `!sw [name]`")]
        public async Task SellWaifu([Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str, false, UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id)), this);

            //waifus existance
            if (waifu == null)
            {
                return;
            }

            int worth = WaifuUtil.GetSalePrice(waifu.Tier);

            if (await Confirm($"Sell **{waifu.Name}** for **{worth.ToString("n0")}** toasties?"))
            {
                if (!UserInventoryDb.OwnsWaifu(Context.User.Id, waifu, Context.Guild.Id))
                {
                    await ReplyAsync(new EmbedBuilderPrepared(Context.User).WithDescription("You tried :star:").Build());
                    return;
                }

                try { await BalanceDb.AddToasties(Context.User.Id, worth, Context.Guild.Id); }
                catch (Exception ex) { await ReplyAsync(ex.Message); }

                //removing waifu + confirmation
                await UserInventoryDb.DeleteWaifu(Context.User.Id, waifu, Context.Guild.Id);
                await ReplyAsync($"You sold **{waifu.Name}** for **{worth.ToString("n0")}** toasties.", embed: CurrencyUtil.ToastieEmbed(Context.User, BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id)).Build());
            }
        }

        [Command("GiveWaifu"), Alias("gw"), Description("Transfers waifu to another user.\n**Usage**: `!gw [user] [waifu_name]`")]
        public async Task GiveWaifu(IUser recipient, [Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str, false, UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id)), this);

            if (waifu == null)
            {
                return;
            }
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            if (!waifus.Any(x => x.Name.Equals(waifu.Name)))
            {
                await ReplyAsync($"**{waifu.Name}** is just like my love - you don't have it.");
                return;
            }
            waifus = UserInventoryDb.GetWaifus(recipient.Id, Context.Guild.Id);
            if (waifus.Any(x => x.Name.Equals(waifu.Name)))
            {
                await ReplyAsync($"They already have **{waifu.Name}**.");
                return;
            }

            await UserInventoryDb.AddWaifu(recipient.Id, waifu, Context.Guild.Id);
            await UserInventoryDb.DeleteWaifu(Context.User.Id, waifu, Context.Guild.Id);
            await ReplyAsync($"{recipient.Mention} You received **{waifu.Name}** from {Context.User.Mention}!", false, WaifuUtil.WaifuEmbedBuilder(waifu).Build());
        }

        [Command("Waifu"), Alias("Husbando", "Trap", "w"), Description("Shows waifu details.\n**Usage**: `!waifu [search]`")]
        public async Task ShowWaifu([Remainder] string name)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name), this);
            if (waifu == null)
            {
                //await ReplyAsync($"Can't find '{name}'. I know they are not real, but this one *really is* just your imagination >_>");
                return;
            }

            var eb = WaifuUtil.WaifuEmbedBuilder(waifu, Context);

            await ReplyAsync("", false, eb.Build());

        }

        [Command("Wish"), Alias("ww", "aww", "WishWaifu", "AddWaifuWish"), Description("Add a waifu to your wishlist to be notified when it appears in shop.\nLimited to 5.\n**Usage**: `!ww [waifu]`")]
        public async Task WishWaifu([Remainder] string str = "")
        {
            var user = Context.User;
            Waifu waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str), this);
            if (waifu == null)
            {
                return;
            }

            var waifus = await WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);
            int cap = 5;
            if (PremiumDb.IsPremium(Context.User.Id, ProType.ProPlus))
                cap = 12;

            string prefix = Program.GetPrefix(Context);
            if (waifus.Count >= cap)
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"You have reached your wishlist limit of **{cap}**.\n" +
                        $"Try `{prefix}rww` to remove a waifu.")
                    .WithFooter($"Increase the limit: `{prefix}pro`")
                    .Build());
                return;
            }

            if (waifus.Any(x => x.Name == waifu.Name))
            {
                await ReplyAsync($"**{waifu.Name}** is already in your wishlist. Baka.");
                return;
            }

            if (UserInventoryDb.OwnsWaifu(user.Id, waifu, Context.Guild.Id))
            {
                await ReplyAsync($"You already own **{waifu.Name}**. Baka.");
                return;
            }

            await WaifuWishlistDb.AddWaifuWish(Context.User.Id, waifu, Context.Guild.Id);

            waifus = await WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);
            await ReplyAsync($"Added **{waifu.Name}** to your wishlist!", false, WaifuUtil.WishlistEmbed(waifus, (SocketGuildUser)user).Build());
        }

        [Command("WaifuWishlist"), Alias("wwl"), Description("Shows yours or someone's waifu wishlist.\n**Usage**: `!wwl [user_optional]`")]
        public async Task WaifuWishlist([Remainder] IUser user = null)
        {
            user ??= Context.User;
            var waifus = await WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);

            await ReplyAsync(null, false, WaifuUtil.WishlistEmbed(waifus, (SocketGuildUser)user).Build());
        }

        [Command("RemoveWish"), Alias("rww", "RemoveWaifuWish"), Description("Removes a waifu from your wishlist.\n**Usage**: `!rww [waifu]`")]
        public async Task RemoveWaifuWish([Remainder] string str = "")
        {
            var user = Context.User;
            Waifu waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str, false, await WaifuWishlistDb.GetWishlist(Context.User.Id, Context.Guild.Id)), this);
            if (waifu == null)
            {
                return;
            }
            var waifus = await WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);
            if (!waifus.Any(x => x.Name == waifu.Name))
            {
                await ReplyAsync($"**{waifu.Name}** is not in your wishlist. Baka.");
                return;
            }

            await WaifuWishlistDb.DeleteWaifuWish(user.Id, waifu, Context.Guild.Id);
            await ReplyAsync("You don't want her anymore, huh...");
        }

        [Command("ModShopAddWaifu"), Alias("msaddwaifu", "msaw"), Description("Adds a waifu to the mod shop. Available for everyone to purchase.\n**Usage**: `!msaw [waifu]`"), UserPermission(GuildPermission.ManageGuild)]
        public async Task ModShopAddWaifu([Remainder] string name)
        {
            var prefix = Program.GetPrefix(Context);

            if (!PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ This command requires Pro Guild+ ~*")
                    .WithFooter($"`{prefix}pro`")
                    .Build());
                return;
            }

            var shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Mod);
            var waifus = shop.ShopWaifus.Select(x => x.Waifu);

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name), this);
            if (waifu == null)
                return;

            if (waifu.Tier < 1 || waifu.Tier > 3)
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ You can only add Tier 1-3 waifus ~*")
                    .Build());
                return;
            }
            if (shop.ShopWaifus.Count >= 15)
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ The Mod Shop is limited to 15 waifus. `{prefix}msrw` to remove ~*")
                    .Build());
                return;
            }
            if (waifus.Any(x => x.Name.Equals(waifu.Name)))
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ **{waifu.Name}** is already in the mod shop ~*")
                    .Build());
                return;
            }

            await WaifuShopDb.UpdateItem(new ShopWaifu
            {
                Discount = 0,
                Limited = -1,
                WaifuShop = shop,
                Waifu = waifu
            });

            await ReplyAsync($"Added **{waifu.Name}** to the Mod Shop", embed: WaifuUtil.WaifuEmbedBuilder(waifu).Build());
            return;
        }

        [Command("ModShopRemoveWaifu"), Alias("msremovewaifu", "msrw"), Description("Removes a waifu from the mod shop.\n**Usage**: `!msrw [waifu]`"), UserPermission(GuildPermission.ManageGuild)]
        public async Task ModShopRemoveWaifu([Remainder] string name = "")
        {
            var shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Mod);
            var waifus = shop.ShopWaifus.Select(x => x.Waifu);

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, false, waifus), this);
            if (waifu == null)
                return;

            await WaifuShopDb.RemoveItem(shop.ShopWaifus.FirstOrDefault(x => x.Waifu.Name.Equals(waifu.Name)));

            await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ **{waifu.Name}** removed from the Mod Shop ~*")
                    .Build());
            return;
        }

        [Command("ShipWaifu"), Description("Gives any waifu to a user.\n**Usage**: `!shipwaifu [user] [waifu_search]`"), HomeOrT1GuildPrecondition, UserPermission(GuildPermission.Administrator)]
        public async Task ShipWaifu(IUser user, [Remainder] string name)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name), this);
            if (waifu == null)
                return;

            if (waifu.Tier < 1 || waifu.Tier > 3)
            {
                if (!(Context.Guild.Id == 738291903770132646 && waifu.Tier == 825))
                {
                    await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                        .WithDescription($"*~ You can only ship Tier 1-3 waifus ~*")
                        .Build());
                    return;
                }
            }

            if (UserInventoryDb.OwnsWaifu(user.Id, waifu, Context.Guild.Id))
            {
                await ReplyAsync($"They already own **{waifu.Name}**");
                return;
            }

            await UserInventoryDb.AddWaifu(user.Id, waifu, Context.Guild.Id);
            await ReplyAsync($"**{waifu.Name}** shipped!", embed: WaifuUtil.WaifuEmbedBuilder(waifu).Build());
        }
    }

    public class WaifuEditing : CustomModuleBase<ICustomContext>
    {
        [Insider]
        [Command("NewWaifu"), Alias("nw"), Description("Adds a waifu to the database.\n**Usage**: `!nw [name] [tier(1-3)] [image_url]`")]
        public async Task NewWaifuCommand(string name, int tier, string url = "")
            => await NewWaifu(name, tier, url ?? ((ICommandContext)Context).Message.Attachments.FirstOrDefault()?.Url);
        public async Task NewWaifu(string name, int tier, string url)
        {
            var exists = await WaifuDb.GetWaifu(name);
            if (exists != null)
            {
                await ReplyAsync($"**{exists.Name}** is already a waifu.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            if (url != null)
            {
                url = url.EndsWith(".gifv") ? url.Replace(".gifv", ".gif") : url;
                url = url.EndsWith(".mp4") ? url.Replace(".mp4", ".gif") : url;

                if (ImgurAPI.RateLimit.ClientRemaining < 15)
                {
                    await ReplyAsync("Not enough imgur credits to upload. Please try again later.");
                    return;
                }

                string albumId;
                if (!ImageDb.AlbumExists("Waifus"))
                {
                    albumId = (await ImgurAPI.CreateAlbumAsync("Waifus")).Id;
                    await ImageDb.CreateAlbum("Waifus", albumId);
                }
                else albumId = ImageDb.GetAlbum("Waifus").AlbumId;

                var iImage = await ImgurAPI.UploadImageAsync(url, albumId, null, name);
                url = iImage.Link;
            }

            var waifu = new Waifu { Name = name, Tier = tier, ImageUrl = url, Description = null, LongName = null };
            await WaifuUtil.UploadWaifuImage(waifu, Context.Channel);

            if (await WaifuDb.AddWaifu(waifu) > 0)
                await ReplyAsync($"{name} added.");
            else
                await ReplyAsync($"Failed to add {name}");

            await Context.Channel.TriggerTypingAsync();
            await WaifuUtil.FindAndUpdateWaifuImageSource(waifu, Context.Channel);
        }

        [Command("DeleteWaifu"), Alias("dw"), Description("Removes a waifu from the database.\n**Usage**: `!dw [name]`"), OwnerPrecondition]
        public async Task DeleteWaifu(string name)
        {
            if (await WaifuDb.DeleteWaifu(name) > 0)
                await ReplyAsync($"{name} deleted.");
            else
                await ReplyAsync($"Failed to delete {name}");
        }

        [Command("WaifuFullName"), Alias("wfn"), Description("Changes the full name of a waifu.\n**Usage**: `!wfn [name] [fullname]`"), Insider]
        public async Task WaifuFullName(string name, [Remainder] string fullname = null)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            string old = waifu.LongName;
            waifu.LongName = fullname;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await SendWaifuUpdatedMessage(waifu, "Full Name", old, waifu.LongName);
            else
                await ReplyAsync($":x: Failed to update {name}");
        }

        [Command("WaifuDescription"), Alias("wd"), Description("Changes the description of a waifu.\n**Usage**: `!wd [name] [description]`"), Insider]
        public async Task WaifuDescription(string name, [Remainder] string description = null)
        {

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            string old = waifu.Description;
            waifu.Description = description;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await SendWaifuUpdatedMessage(waifu, "Description", old, waifu.Description);
            else
                await ReplyAsync($":x: Failed to update {name}");
        }

        [Command("WaifuSource"), Alias("wsrc"), Description("Changes the source of a waifu.\n**Usage**: `!ws [name] [source]`"), Insider]
        public async Task WaifuSource(string name, [Remainder] string source = null)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            string old = waifu.Source;
            waifu.Source = source;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await SendWaifuUpdatedMessage(waifu, "Source", old, waifu.Source);
            else
                await ReplyAsync($":x: Failed to update {name}");
        }

        [Command("WaifuTier"), Alias("wt"), Description("Changes the tier of a waifu.\n**Usage**: `!wt [name] [tier(1-3)]`"), Insider]
        public async Task WaifuTier(string name, int tier)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            string old = waifu.Tier.ToString();
            waifu.Tier = tier;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
                await SendWaifuUpdatedMessage(waifu, "Tier", old, waifu.Tier.ToString());
            else
                await ReplyAsync($":x: Failed to update {name}");
        }

        [Insider]
        [Command("WaifuImage"), Alias("wi"), Description("Changes the image of a waifu.\n**Usage**: `!wi [name] [image_url]`")]
        public async Task WaifuImageCommand(string name, string url = null)
            => await WaifuImage(name, url ?? ((ICommandContext)Context).Message.Attachments.FirstOrDefault()?.Url);
        public async Task WaifuImage(string name, string url)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            if (url == null)
            {
                await ReplyAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            url = url.EndsWith(".gifv") ? url.Replace(".gifv", ".gif") : url;
            url = url.EndsWith(".mp4") ? url.Replace(".mp4", ".gif") : url;

            if (ImgurAPI.RateLimit.ClientRemaining < 15)
            {
                await ReplyAsync("Not enough imgur credits to upload. Please try again later.");
                return;
            }

            string albumId;
            if (!ImageDb.AlbumExists("Waifus"))
            {
                albumId = (await ImgurAPI.CreateAlbumAsync("Waifus")).Id;
                await ImageDb.CreateAlbum("Waifus", albumId);
            }
            else albumId = ImageDb.GetAlbum("Waifus").AlbumId;

            var iImage = await ImgurAPI.UploadImageAsync(url, albumId, null, name);
            string old = waifu.ImageUrl;
            waifu.ImageUrl = iImage.Link;
            await WaifuUtil.UploadWaifuImage(waifu, Context.Channel);

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
            {
                await SendWaifuUpdatedMessage(waifu, "ImageUrl", old, waifu.ImageUrl);
            }
            else
            {
                await ReplyAsync($":x: Failed to update {name}");
            }

            await Context.Channel.TriggerTypingAsync();
            await WaifuUtil.FindAndUpdateWaifuImageSource(waifu, Context.Channel);
        }

        [Command("WaifuImageSource"), Alias("wis"), Description("Set waifu image source.\n**Usage**: `!wis [name] [image_sauce]`"), Insider]
        public async Task WaifuImageSource(string name, string url = null)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            string old = waifu.ImageSource;
            waifu.ImageSource = url;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
            {
                await SendWaifuUpdatedMessage(waifu, "Image Source", old, waifu.ImageSource);
            }
            else
            {
                await ReplyAsync($":x: Failed to update {name}");
                return;
            }
        }

        [Command("ImageSauceRequest"), Alias("isr"), Description("Get a request for missing image sauce.\n**Usage**: `!isr`"), Insider]
        public async Task ImageSauceRequest(string url = null)
        {
            await Timers.Timer_RequestSauce(null, null);
            await ReplyAsync("<#728729035986829342>");
        }

        [Command("GetWaifu"), Description("Get details about a waifu.\n**Usage**: `!wi [name] [image_url]`"), Insider]
        public async Task WaifuImageSource([Remainder] string name)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true, null, true), this);
            if (waifu == null)
            {
                return;
            }

            string str = "```yaml\n";

            str += $"Name: {waifu.Name}\n";
            str += $"FullName: {waifu.LongName}\n";
            str += $"Source: {waifu.Source}\n";
            str += $"Description: {waifu.Description}\n";
            str += $"ImgurUrl: {waifu.ImageUrl}\n";
            str += $"NamikoMoeUrl: {waifu.HostImageUrl}\n";
            str += $"ImageSource: {waifu.ImageSource}\n";
            str += $"Tier: {waifu.Tier}\n";
            str += $"MalId: {waifu.Mal?.MalId}\n";

            str += "```";

            await ReplyAsync(str);
        }


        [Command("RenameWaifu"), Alias("rw"), Description("Change a waifu's primary name.\n**Usage**: `!rw [oldName] [newName]`"), Insider]
        public async Task RenameWaifu(string oldName, string newName)
        {
            var waifu = await WaifuDb.GetWaifu(oldName);
            if (waifu == null)
            {
                await ReplyAsync($"**{oldName}** doesn't exist. *BAAAAAAAAAAAAAAAAAAKA*");
                return;
            }

            waifu = await WaifuDb.GetWaifu(newName);
            if (waifu != null)
            {
                await ReplyAsync($"**{newName}** already exists! *BAAAAAAAAAAAAAAAAAAKA*");
                return;
            }

            int res = await WaifuDb.RenameWaifu(oldName, newName);
            await ReplyAsync($"Replaced **{res}** **{oldName}** with their **{newName}** clones and burned the originals. *That was wild...*");
        }

        [Command("AutocompleteWaifu"), Alias("acw"), Description("Auto completes a waifu using MAL.\n**Usage**: `!acw [name] [MAL_ID]`"), Insider]
        public async Task AutocompleteWaifu(string name, long malId)
        {
            var waifu = await WaifuDb.GetWaifu(name);
            if (waifu == null)
            {
                await ReplyAsync($"**{name}** doesn't exist. *BAAAAAAAAAAAAAAAAAAKA*");
                return;
            }

            var mal = await WebUtil.GetWaifu(malId);
            waifu.LongName = $"{mal.Name} ({mal.NameKanji})";
            var about = mal.About;
            var lines = about.Split('\n');
            string desc = "";
            foreach (var line in lines)
            {
                if (line.Split(' ')[0].EndsWith(':'))
                    continue;
                if (line.StartsWith('('))
                    continue;

                var l = Regex.Replace(line, @"\t|\n|\r|\\n|\\t|\\r", "");
                if (l != "")
                    desc += l + "\n\n";
            }
            //desc.Replace(@"\r", @"\n\n");
            waifu.Description = desc;
            waifu.Source = mal.Animeography.FirstOrDefault() == null ? "" : mal.Animeography.FirstOrDefault().Name;

            await WaifuDb.UpdateWaifu(waifu);
            await ReplyAsync($"Autocompleted **{waifu.Name}**. Has **{mal.MemberFavorites}** favorites.", false, WaifuUtil.WaifuEmbedBuilder(waifu, Context).Build());
        }

        [Command("NewWaifuAutocomplete"), Alias("nwac"), Description("Creates a new waifu and auto completes using MAL.\n**Usage**: `!nwac [name] [MAL_ID] [image_url_optional]`"), Insider]
        public async Task NewWaifuAutocompleteCommand(string name, long malId, string url = null)
            => await NewWaifuAutocompleteCommand(name, malId, url ?? ((ICommandContext)Context).Message.Attachments.FirstOrDefault()?.Url);
        public async Task NewWaifuAutocomplete(string name, long malId, string url)
        {
            var exists = await WaifuDb.GetWaifu(name);
            if (exists != null)
            {
                await ReplyAsync($"**{exists.Name}** is already a waifu.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            if (url != null)
            {
                url = url.EndsWith(".gifv") ? url.Replace(".gifv", ".gif") : url;
                url = url.EndsWith(".mp4") ? url.Replace(".mp4", ".gif") : url;

                if (ImgurAPI.RateLimit.ClientRemaining < 15)
                {
                    await ReplyAsync("Not enough imgur credits to upload. Please try again later.");
                    return;
                }

                string albumId;
                if (!ImageDb.AlbumExists("Waifus"))
                {
                    albumId = (await ImgurAPI.CreateAlbumAsync("Waifus")).Id;
                    await ImageDb.CreateAlbum("Waifus", albumId);
                }
                else albumId = ImageDb.GetAlbum("Waifus").AlbumId;

                var iImage = await ImgurAPI.UploadImageAsync(url, albumId, null, name);
                url = iImage.Link;
            }

            var waifu = new Waifu { Name = name, Tier = 404, ImageUrl = url, Description = null, LongName = null };
            await WaifuUtil.UploadWaifuImage(waifu, Context.Channel);

            var mal = await WebUtil.GetWaifu(malId);
            waifu.LongName = $"{mal.Name} ({mal.NameKanji})";
            var about = mal.About;
            var lines = about.Split('\n');
            string desc = "";
            foreach (var line in lines)
            {
                if (line.Split(' ')[0].EndsWith(':'))
                    continue;
                if (line.StartsWith('('))
                    continue;

                var l = Regex.Replace(line, @"\t|\n|\r|\\n|\\t|\\r", "");
                if (l != "")
                    desc += l + "\n\n";
            }
            waifu.Description = desc;
            waifu.Source = mal.Animeography.FirstOrDefault() == null ? "" : mal.Animeography.FirstOrDefault().Name;
            try
            {
                waifu.Tier = WaifuUtil.FavoritesToTier(mal.MemberFavorites.Value);
            }
            catch { }

            if (waifu.Tier == 0)
            {
                waifu.Tier = 3;
                await ReplyAsync($"Not enough favorites! Are you sure you wish to create this waifu? Remove - `!dw {waifu.Name}`");
            }

            if (await WaifuDb.AddWaifu(waifu) > 0)
            {
                await ReplyAsync
                (
                    $"Autocompleted **{waifu.Name}**. Has **{mal.MemberFavorites}** favorites.",
                    embed: WaifuUtil.WaifuEmbedBuilder(waifu, Context).Build()
                );

                await WaifuDb.AddMalWaifu(new MalWaifu
                {
                    MalId = malId,
                    WaifuName = waifu.Name,
                    LastUpdated = DateTime.Now,
                    MalConfirmed = true
                });
            }
            else
            {
                await ReplyAsync("Rip");
            }

            await Context.Channel.TriggerTypingAsync();
            await WaifuUtil.FindAndUpdateWaifuImageSource(waifu, Context.Channel);
        }

        [Command("WaifuMal"), Alias("wm"), Description("Sets waifus MAL Id.\n**Usage**: `!wm [name] [MAL_ID]`"), Insider]
        public async Task WaifuMal(string name, long malId)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true, includeMAL: true), this);
            if (waifu == null)
            {
                return;
            }

            var mal = waifu.Mal ?? new MalWaifu { WaifuName = waifu.Name };
            mal.LastUpdated = DateTime.Now;
            mal.MalConfirmed = true;
            mal.MalId = malId;

            if ((await WaifuDb.UpdateMalWaifu(mal)) > 0)
                await ReplyAsync($":white_check_mark: {waifu.Name} updated.");
            else
                await ReplyAsync($":x: Failed to update {name}");
        }

        [Command("ResetWaifuShop"), Alias("rws"), Description("Resets the waifu shop contents."), Insider]
        public async Task ResetWaifuShop()
        {
            await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Waifu, true);
            await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Gacha, true);
            await ReplyAsync("Waifu Shop and Gacha Shop reset.");
        }


        private async Task SendWaifuUpdatedMessage(Waifu waifu, string field, string oldVal, string newVal)
        {
            var author = Context.User;

            newVal = newVal == null || newVal == "" ? "-" : newVal.ShortenString(1000, 1000, " ...");
            oldVal = oldVal == null || oldVal == "" ? "-" : oldVal.ShortenString(1000, 1000, " ...");

            var eb = new EmbedBuilder()
                .WithAuthor($"{waifu.Name} - {field} updated", waifu.HostImageUrl)
                .AddField("New", newVal, true)
                .AddField("Old", oldVal, true)
                .WithColor(BasicUtil.RandomColor())
                .WithFooter(author.Username + "#" + author.Discriminator, author.GetAvatarUrl());

            await ReplyAsync(embed: eb.Build());
        }
    }
}