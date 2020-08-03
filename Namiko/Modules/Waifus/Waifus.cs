using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    [RequireGuild]
    public class Waifus : InteractiveBase<ShardedCommandContext>
    {
        private static readonly Dictionary<ulong, Object> slideLock = new Dictionary<ulong, Object>();

        [Command("WaifuShop"), Alias("ws"), Summary("Opens the waifu shop.")]
        public async Task WaifuShop([Remainder] string str = "")
        {
            WaifuShop shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Waifu);
            int count = Constants.shoplimitedamount + Constants.shopt1amount + Constants.shopt2amount + Constants.shopt3amount;
            string prefix = Program.GetPrefix(Context);
            var waifus = shop.ShopWaifus.OrderByDescending(x => x.Limited).ThenBy(x => x.Waifu.Tier).ThenBy(x => x.Waifu.Source).ToList();

            if (waifus.Count <= count)
            {
                var eb = WaifuUtil.NewShopEmbed(waifus, prefix);
                await Context.Channel.SendMessageAsync("", false, eb.Build());
                return;
            }

            await PagedReplyAsync(WaifuUtil.PaginatedShopMessage(waifus, count, prefix));
        }

        [Command("GachaShop"), Alias("gs"), Summary("Opens the gacha shop.")]
        public async Task GachaShop([Remainder] string str = "")
        {
            WaifuShop shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Gacha);
            string prefix = Program.GetPrefix(Context);
            var waifus = shop.ShopWaifus.OrderByDescending(x => x.Limited).ThenBy(x => x.Waifu.Tier).ThenBy(x => x.Waifu.Source).ToList();

            var eb = WaifuUtil.NewShopEmbed(waifus, prefix, ShopType.Gacha);
            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("ModShop"), Alias("ms"), Summary("Opens the mod shop. A waifu shop controlled by server moderators.")]
        public async Task ModShop([Remainder] string str = "")
        {
            WaifuShop shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Mod);
            string prefix = Program.GetPrefix(Context);
            List<ShopWaifu> waifus = new List<ShopWaifu>();
            if (shop != null)
                waifus = shop.ShopWaifus;

            waifus = waifus.OrderBy(x => x.Waifu.Tier).ThenBy(x => x.Waifu.Source).ThenBy(x => x.Waifu.Name).ToList();
            var eb = WaifuUtil.NewShopEmbed(waifus, prefix, ShopType.Mod);
            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("WaifuShopSlides"), Alias("wss"), Summary("Opens the waifu shop slides.")]
        public async Task WaifuShopSlides([Remainder] string str = "")
        {
            List<ShopWaifu> waifus = (await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Waifu)).ShopWaifus;
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
            var shopwaifus = (await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Waifu)).ShopWaifus;
            shopwaifus.AddRange((await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Gacha)).ShopWaifus);

            var modshop = await WaifuShopDb.GetWaifuShop(Context.Guild.Id, ShopType.Mod);
            if (modshop != null)
                shopwaifus.AddRange(modshop.ShopWaifus);

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str, false, shopwaifus.Select(x => x.Waifu)), this);

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
                await BalanceDb.AddToasties(Context.User.Id, -price, Context.Guild.Id);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync(ex.Message);
                return;
            }

            await UserInventoryDb.AddWaifu(Context.User.Id, waifu, Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"Congratulations! You bought **{waifu.Name}**!", false, WaifuUtil.WaifuEmbedBuilder(waifu).Build());
            await BalanceDb.AddToasties(Context.Client.CurrentUser.Id, price / 13, Context.Guild.Id);

            if (shopWaifu.Limited > 0)
            {
                shopWaifu.BoughtBy = Context.User.Id;
                shopWaifu.Limited -= 1;
                await WaifuShopDb.UpdateShopWaifu(shopWaifu);
            }
        }

        [Command("SellWaifu"), Alias("sw"), Summary("Sells a waifu you already own for a discounted price.\n**Usage**: `!sw [name]`")]
        public async Task SellWaifu([Remainder] string str = "") {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str, false, UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id)), this);

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
                    try { await BalanceDb.AddToasties(Context.User.Id, worth, Context.Guild.Id); }
                    catch (Exception ex) { await Context.Channel.SendMessageAsync(ex.Message); }

                    //removing waifu + confirmation
                    await UserInventoryDb.DeleteWaifu(Context.User.Id, waifu, Context.Guild.Id);
                    await message.ModifyAsync(x => {
                        x.Content = $"You sold **{waifu.Name}** for **{worth.ToString("n0")}** toasties.";
                        x.Embed = ToastieUtil.ToastieEmbed(Context.User, BalanceDb.GetToasties(Context.User.Id, Context.Guild.Id)).Build();
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
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str, false, UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id)), this);

            if (waifu == null)
            {
                return;
            }
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            if (!waifus.Any(x => x.Name.Equals(waifu.Name)))
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
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name), this);
            if (waifu == null)
            {
                //await Context.Channel.SendMessageAsync($"Can't find '{name}'. I know they are not real, but this one *really is* just your imagination >_>");
                return;
            }

            var eb = WaifuUtil.WaifuEmbedBuilder(waifu, Context);

            await Context.Channel.SendMessageAsync("", false, eb.Build());

        }

        [Command("TopWaifus"), Alias("tw"), Summary("Shows most popular waifus.\n**Usage**: `!tw`")]
        public async Task TopWaifus([Remainder] string str = "")
        {
            var waifus = await UserInventoryDb.CountWaifus(0, str.Split(' '));
            var msg = new CustomPaginatedMessage();

            msg.Title = ":two_hearts: Waifu Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Globally Bought",
                    Pages = CustomPaginatedMessage.PagesArray(waifus, 10, (x) => $"**{x.Key}** - {x.Value}\n")
                }
            };
            msg.Fields = fields;
            msg.ThumbnailUrl = WaifuDb.GetWaifu(waifus.First().Key).ImageUrl;

            await PagedReplyAsync(msg);
        }

        [Command("ServerTopWaifus"), Alias("stw"), Summary("Shows most popular waifus in the server.\n**Usage**: `!stw`")]
        public async Task ServerTopWaifus([Remainder] string str = "")
        {
            var waifus = await UserInventoryDb.CountWaifus(Context.Guild.Id, str.Split(' '));
            var msg = new CustomPaginatedMessage();

            msg.Title = ":two_hearts: Waifu Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Bought Here",
                    Pages = CustomPaginatedMessage.PagesArray(waifus, 10, (x) => $"**{x.Key}** - {x.Value}\n")
                }
            };
            msg.Fields = fields;
            msg.ThumbnailUrl = WaifuDb.GetWaifu(waifus.First().Key).ImageUrl;

            await PagedReplyAsync(msg);
        }

        [Command("WaifuLeaderboard"), Alias("wlb"), Summary("Shows waifu worth of each person.\n**Usage**: `!wlb`")]
        public async Task WaifuLeaderboard([Remainder] string str = "")
        {
            var AllWaifus = await UserInventoryDb.GetAllWaifuItems(Context.Guild.Id);
            var users = new Dictionary<SocketUser, int>();

            foreach (var x in AllWaifus)
            {
                var user = Context.Guild.GetUser(x.UserId);
                if (user != null)
                    if (!users.ContainsKey(user))
                        users.Add(user, WaifuUtil.WaifuValue(AllWaifus.Where(x => x.UserId == user.Id).Select(x => x.Waifu)));
            }

            var ordUsers = users.OrderByDescending(x => x.Value);

            var msg = new CustomPaginatedMessage();

            msg.Title = "User Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Waifu Value <:toastie3:454441133876183060>",
                    Pages = CustomPaginatedMessage.PagesArray(ordUsers, 10, (x) => $"{x.Key.Mention} - {x.Value}\n")
                }
            };
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }

        [Command("Wish"), Alias("ww", "aww", "WishWaifu", "AddWaifuWish"), Summary("Add a waifu to your wishlist to be notified when it appears in shop.\nLimited to 5.\n**Usage**: `!ww [waifu]`")]
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
            if (PremiumDb.IsPremium(Context.User.Id, PremiumType.ProPlus))
                cap = 12;

            string prefix = Program.GetPrefix(Context);
            if (waifus.Count >= cap)
            {
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"You have reached your wishlist limit of **{cap}**.\n" +
                        $"Try `{prefix}rww` to remove a waifu.")
                    .WithFooter($"Increase the limit: `{prefix}pro`")
                    .Build());
                return;
            }

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
            
            waifus = await WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"Added **{waifu.Name}** to your wishlist!", false, WaifuUtil.WishlistEmbed(waifus, (SocketGuildUser)user).Build());
        }

        [Command("WaifuWishlist"), Alias("wwl"), Summary("Shows yours or someone's waifu wishlist.\n**Usage**: `!wwl [user_optional]`")]
        public async Task WaifuWishlist([Remainder] IUser user = null)
        {
            user ??= Context.User;
            var waifus = await WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);

            await Context.Channel.SendMessageAsync(null, false, WaifuUtil.WishlistEmbed(waifus, (SocketGuildUser)user).Build());
        }

        [Command("RemoveWish"), Alias("rww", "RemoveWaifuWish"), Summary("Removes a waifu from your wishlist.\n**Usage**: `!rww [waifu]`")]
        public async Task RemoveWaifuWish([Remainder] string str = "")
        {
            var user = Context.User;
            Waifu waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(str, false, await WaifuWishlistDb.GetWishlist(Context.User.Id, Context.Guild.Id)), this);
            if (waifu == null)
            {
                return;
            }
            var waifus = await WaifuWishlistDb.GetWishlist(user.Id, Context.Guild.Id);
            if(!waifus.Any(x => x.Name == waifu.Name))
            {
                await Context.Channel.SendMessageAsync($"**{waifu.Name}** is not in your wishlist. Baka.");
                return;
            }

            await WaifuWishlistDb.DeleteWaifuWish(user.Id, waifu, Context.Guild.Id);
            await Context.Channel.SendMessageAsync("You don't want her anymore, huh...");
        }

        [Command("ModShopAddWaifu"), Alias("msaddwaifu", "msaw"), Summary("Adds a waifu to the mod shop. Available for everyone to purchase.\n**Usage**: `!msaw [waifu]`"), CustomUserPermission(GuildPermission.ManageGuild)]
        public async Task ModShopAddWaifu([Remainder] string name)
        {
            var prefix = Program.GetPrefix(Context);

            if (!PremiumDb.IsPremium(Context.Guild.Id, PremiumType.GuildPlus))
            {
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ This command requires Pro Guild+ ~*")
                    .WithFooter($"`{prefix}pro`")
                    .Build());
                return;
            }

            var shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Mod);
            if (shop == null)
            {
                shop = new WaifuShop
                {
                    GeneratedDate = System.DateTime.Now,
                    GuildId = Context.Guild.Id,
                    Type = ShopType.Mod,
                    ShopWaifus = new List<ShopWaifu>()
                };
                shop = await WaifuShopDb.AddShop(shop, true);
            }
            var waifus = shop.ShopWaifus.Select(x => x.Waifu);

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name), this);
            if (waifu == null)
                return;

            if (waifu.Tier < 1 || waifu.Tier > 3)
            {
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ You can only add Tier 1-3 waifus ~*")
                    .Build());
                return;
            }
            if (shop.ShopWaifus.Count >= 15)
            {
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ The Mod Shop is limited to 15 waifus. `{prefix}msrw` to remove ~*")
                    .Build());
                return;
            }
            if (waifus.Any(x => x.Name.Equals(waifu.Name)))
            {
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ **{waifu.Name}** is already in the mod shop ~*")
                    .Build());
                return;
            }

            await WaifuShopDb.UpdateShopWaifu(new ShopWaifu
            {
                Discount = 0,
                Limited = -1,
                WaifuShop = shop,
                Waifu = waifu
            });

            await Context.Channel.SendMessageAsync($"Added **{waifu.Name}** to the Mod Shop", embed: WaifuUtil.WaifuEmbedBuilder(waifu).Build());
            return;
        }

        [Command("ModShopRemoveWaifu"), Alias("msremovewaifu", "msrw"), Summary("Removes a waifu from the mod shop.\n**Usage**: `!msrw [waifu]`"), CustomUserPermission(GuildPermission.ManageGuild)]
        public async Task ModShopRemoveWaifu([Remainder] string name = "")
        {
            var shop = await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Mod);
            var waifus = shop.ShopWaifus.Select(x => x.Waifu);

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, false, waifus), this);
            if (waifu == null)
                return;

            await WaifuShopDb.RemoveItem(shop.ShopWaifus.FirstOrDefault(x => x.Waifu.Name.Equals(waifu.Name)));

            await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"*~ **{waifu.Name}** removed from the Mod Shop ~*")
                    .Build());
            return;
        }

        [Command("ShipWaifu"), Summary("Gives any waifu to a user.\n**Usage**: `!shipwaifu [user] [waifu_search]`"), HomeOrT1GuildPrecondition, CustomUserPermission(GuildPermission.Administrator)]
        public async Task ShipWaifu(IUser user, [Remainder] string name)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name), this);
            if (waifu == null)
                return;

            if (waifu.Tier < 1 || waifu.Tier > 3)
            {
                if (!(Context.Guild.Id == 482974382445035520 && waifu.Tier == 825))
                {
                    await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                        .WithDescription($"*~ You can only ship Tier 1-3 waifus ~*")
                        .Build());
                    return;
                }
            }

            if (UserInventoryDb.OwnsWaifu(user.Id, waifu, Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync($"They already own **{waifu.Name}**");
                return;
            }

            await UserInventoryDb.AddWaifu(user.Id, waifu, Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"**{waifu.Name}** shipped!", embed: WaifuUtil.WaifuEmbedBuilder(waifu).Build());
        }
    }

    public class WaifuEditing : InteractiveBase<ShardedCommandContext>
    {
        [Command("NewWaifu"), Alias("nw"), Summary("Adds a waifu to the database.\n**Usage**: `!nw [name] [tier(1-3)] [image_url]`"), Insider]
        public async Task NewWaifu(string name, int tier, string url = null)
        {
            await Context.Channel.TriggerTypingAsync();

            url ??= Context.Message.Attachments.FirstOrDefault()?.Url;

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

            var waifu = new Waifu { Name = name, Tier = tier, ImageUrl = url, Description = null, LongName = null};

            if (await WaifuDb.AddWaifu(waifu) > 0)
                await Context.Channel.SendMessageAsync($"{name} added.");
            else
                await Context.Channel.SendMessageAsync($"Failed to add {name}");

            await WaifuUtil.DownloadWaifuImageToServer(waifu, Context.Channel);
            await WaifuUtil.FindAndUpdateWaifuImageSource(waifu, Context.Channel);
        }

        [Command("DeleteWaifu"), Alias("dw"), Summary("Removes a waifu from the database.\n**Usage**: `!dw [name]`"), OwnerPrecondition]
        public async Task DeleteWaifu(string name)
        {
            if (await WaifuDb.DeleteWaifu(name) > 0)
                await Context.Channel.SendMessageAsync($"{name} deleted.");
            else
                await Context.Channel.SendMessageAsync($"Failed to delete {name}");
        }

        [Command("WaifuFullName"), Alias("wfn"), Summary("Changes the full name of a waifu.\n**Usage**: `!wfn [name] [fullname]`"), Insider]
        public async Task WaifuFullName(string name, [Remainder] string fullname = null)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
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

        [Command("WaifuDescription"), Alias("wd"), Summary("Changes the description of a waifu.\n**Usage**: `!wd [name] [description]`"), Insider]
        public async Task WaifuDescription(string name, [Remainder] string description = null)
        {

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
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

        [Command("WaifuSource"), Alias("wsrc"), Summary("Changes the source of a waifu.\n**Usage**: `!ws [name] [source]`"), Insider]
        public async Task WaifuSource(string name, [Remainder] string source = null)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
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

        [Command("WaifuTier"), Alias("wt"), Summary("Changes the tier of a waifu.\n**Usage**: `!wt [name] [tier(1-3)]`"), Insider]
        public async Task WaifuTier(string name, int tier)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
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

        [Command("WaifuImage"), Alias("wi"), Summary("Changes the image of a waifu.\n**Usage**: `!wi [name] [image_url]`"), Insider]
        public async Task WaifuImage(string name, string url = null)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            url ??= Context.Message.Attachments.FirstOrDefault()?.Url;

            if (url == null)
            {
                await Context.Channel.SendMessageAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
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
            waifu.ImageUrl = iImage.Link;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
            {
                var rl = ImgurAPI.RateLimit;
                await Context.Channel.SendMessageAsync($":white_check_mark: {waifu.Name} updated. {rl.ClientRemaining}/{rl.ClientLimit} imgur credits remaining.");
            }
            else
                await Context.Channel.SendMessageAsync($":x: Failed to update {name}");

            await Context.Channel.TriggerTypingAsync();
            await WaifuUtil.FindAndUpdateWaifuImageSource(waifu, Context.Channel);
            await WaifuUtil.DownloadWaifuImageToServer(waifu, Context.Channel);
        }

        [Command("WaifuImageSource"), Alias("wis"), Summary("Set waifu image source.\n**Usage**: `!wis [name] [image_sauce]`"), Insider]
        public async Task WaifuImageSource(string name, string url = null)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(name, true), this);
            if (waifu == null)
            {
                return;
            }

            waifu.ImageSource = url;

            if (await WaifuDb.UpdateWaifu(waifu) > 0)
            {
                await Context.Channel.SendMessageAsync($":white_check_mark: {waifu.Name} updated.");
            }
            else
            {
                await Context.Channel.SendMessageAsync($":x: Failed to update {name}");
                return;
            }

            // Request for another sauce
            await Context.Channel.TriggerTypingAsync();
            Timers.Timer_RequestSauce(new SauceRequest
            {
                Channel = Context.Channel,
                Waifu = waifu
            }, null);
        }

        [Command("ImageSourceRequest"), Alias("isr"), Summary("Get an image source request.\n**Usage**: `!isr`"), Insider]
        public async Task ImageSourceRequest([Remainder] string url = null)
        {
            // Request for another sauce
            await Context.Channel.TriggerTypingAsync();
            Timers.Timer_RequestSauce(new SauceRequest
            {
                Channel = Context.Channel
            }, null);
        }

        [Command("MissingSauceList"), Alias("msl"), Summary("List of missing waifu sauce.\n**Usage**: `!isr`"), Insider]
        public async Task MissingSauceList([Remainder] string url = null)
        {
            using var db = new NamikoDbContext();
            var waifus = await db.Waifus.Where(x => x.ImageSource.Equals("missing")).OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();

            await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(String.Join("\n", waifus).Substring(2000)).Build());
        }

        [Command("GetWaifu"), Alias("wis"), Summary("Set waifu image source.\n**Usage**: `!wi [name] [image_url]`"), Insider]
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
            str += $"ImageUrl: {waifu.ImageUrl}\n";
            str += $"ImageSource: {waifu.ImageSource}\n";
            str += $"Tier: {waifu.Tier}\n";
            str += $"MalId: {waifu.Mal?.MalId}\n";

            str += "```";

            await Context.Channel.SendMessageAsync(str);
        }


        [Command("RenameWaifu"), Alias("rw"), Summary("Change a waifu's primary name.\n**Usage**: `!rw [oldName] [newName]`"), Insider]
        public async Task RenameWaifu(string oldName, string newName)
        {
            var waifu = WaifuDb.GetWaifu(oldName);
            if(waifu == null)
            {
                await Context.Channel.SendMessageAsync($"**{oldName}** doesn't exist. *BAAAAAAAAAAAAAAAAAAKA*");
                return;
            }

            waifu = WaifuDb.GetWaifu(newName);
            if (waifu != null)
            {
                await Context.Channel.SendMessageAsync($"**{newName}** already exists! *BAAAAAAAAAAAAAAAAAAKA*");
                return;
            }

            int res = await WaifuDb.RenameWaifu(oldName, newName);
            await Context.Channel.SendMessageAsync($"Replaced **{res}** **{oldName}** with their **{newName}** clones and burned the originals. *That was wild...*");
        }

        [Command("AutocompleteWaifu"), Alias("acw"), Summary("Auto completes a waifu using MAL.\n**Usage**: `!acw [name] [MAL_ID]`"), Insider]
        public async Task AutocompleteWaifu(string name, long malId)
        {
            var waifu = WaifuDb.GetWaifu(name);
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"**{name}** doesn't exist. *BAAAAAAAAAAAAAAAAAAKA*");
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

                desc += line + '\n' + '\n';
            }
            //desc.Replace(@"\r", @"\n\n");
            waifu.Description = desc;
            waifu.Source = mal.Animeography.FirstOrDefault() == null ? "" : mal.Animeography.FirstOrDefault().Name;

            await WaifuDb.UpdateWaifu(waifu);
            await Context.Channel.SendMessageAsync($"Autocompleted **{waifu.Name}**. Has **{mal.MemberFavorites}** favorites.", false, WaifuUtil.WaifuEmbedBuilder(waifu, Context).Build());
        }

        [Command("NewWaifuAutocomplete"), Alias("nwac"), Summary("Creates a new waifu and auto completes using MAL.\n**Usage**: `!acw [name] [MAL_ID] [image_url_optional]`"), Insider]
        public async Task NewWaifuAutocomplete(string name, long malId, string url = null)
        {
            await Context.Channel.TriggerTypingAsync();

            url ??= Context.Message.Attachments.FirstOrDefault()?.Url;

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

                desc += line + '\n' + '\n';
            }
            waifu.Description = desc;
            waifu.Source = mal.Animeography.FirstOrDefault() == null ? "" : mal.Animeography.FirstOrDefault().Name;
            try
            {
                waifu.Tier = WaifuUtil.FavoritesToTier(mal.MemberFavorites.Value);
            } catch { }

            if (waifu.Tier == 0)
            {
                waifu.Tier = 3;
                await Context.Channel.SendMessageAsync($"Not enough favorites! Are you sure you wish to create this waifu? Remove - `!dw {waifu.Name}`");
            }

            if (await WaifuDb.AddWaifu(waifu) > 0)
            {
                await Context.Channel.SendMessageAsync
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
                await Context.Channel.SendMessageAsync("Rip");
            }

            await WaifuUtil.DownloadWaifuImageToServer(waifu, Context.Channel);
            await WaifuUtil.FindAndUpdateWaifuImageSource(waifu, Context.Channel);
        }

        [Command("WaifuMal"), Alias("wm"), Summary("Sets waifus MAL Id.\n**Usage**: `!wm [name] [MAL_ID]`"), Insider]
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
                await Context.Channel.SendMessageAsync($":white_check_mark: {waifu.Name} updated.");
            else
                await Context.Channel.SendMessageAsync($":x: Failed to update {name}");
        }

        [Command("ResetWaifuShop"), Alias("rws"), Summary("Resets the waifu shop contents."), Insider]
        public async Task ResetWaifuShop()
        {
            await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Waifu, true);
            await WaifuUtil.GetShop(Context.Guild.Id, ShopType.Gacha, true);
            await Context.Channel.SendMessageAsync("Waifu Shop and Gacha Shop reset.");
        }
    }
}