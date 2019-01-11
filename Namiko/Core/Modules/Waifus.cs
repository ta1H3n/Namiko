using System.Threading.Tasks;
using System.Linq;

using Discord.Commands;
using Discord;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System.Collections.Generic;
using System;
using Namiko.Resources.Attributes;
using Namiko.Core.Util;
using Discord.WebSocket;

namespace Namiko.Core.Modules
{
    public class Waifus : ModuleBase<SocketCommandContext>
    {
        [Command("Inventory"), Alias("waifus", "profile"), Summary("Shows user waifus.\n**Usage**: `!inventory [user_optional]`")]
        public async Task Inventory(SocketGuildUser user = null, [Remainder] string str = "")
        {
            if (user == null)
                user = (SocketGuildUser) Context.User;

            await Context.Channel.SendMessageAsync("", false, WaifuUtil.ProfileEmbed(user).Build());
        }

        [Command("WaifuShop"), Alias("ws"), Summary("Opens the waifu shop."),]
        public async Task OpenWaifuShop([Remainder] string str = "")
        {
            List<ShopWaifu> waifus = await WaifuUtil.GetShopWaifus(Context.Guild.Id);
            var eb = WaifuUtil.GetShopEmbed(waifus, Context);
            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        [Command("BuyWaifu"), Alias("bw"), Summary("Buys a waifu, must be in a shop.\n**Usage**: `!bw [name]`")]
        public async Task BuyWaifu(string name, [Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);

            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"Can't find '{name}'. I know they are not real, but this one *really is* just your imagination >_>");
                return;
            }
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            if (waifus.Where(x => x.Name.Equals(waifu.Name)).Count() > 0)
            {
                await Context.Channel.SendMessageAsync("You already have " + name);
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
                await Context.Channel.SendMessageAsync($"{name} is not currently for sale! Try the `waifushop` command.");
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
        }

        [Command("GiveWaifu"), Alias("gw"), Summary("Transfers waifu to another user.\n**Usage**: `!gw [user] [waifu_name]`")]
        public async Task GiveWaifu(IUser recipient, string name, [Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);

            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync("You don't have " + name);
                return;
            }
            var waifus = UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id);
            if (!(waifus.Where(x => x.Name.Equals(waifu.Name)).Count() > 0))
            {
                await Context.Channel.SendMessageAsync("You don't have " + name);
                return;
            }
            waifus = UserInventoryDb.GetWaifus(recipient.Id, Context.Guild.Id);
            if (waifus.Where(x => x.Name.Equals(waifu.Name)).Count() > 0)
            {
                await Context.Channel.SendMessageAsync("They already have " + name);
                return;
            }

            await UserInventoryDb.AddWaifu(recipient.Id, waifu, Context.Guild.Id);
            await UserInventoryDb.DeleteWaifu(Context.User.Id, waifu, Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"{recipient.Mention} You received {waifu.Name} from {Context.User.Mention}!", false, WaifuUtil.WaifuEmbedBuilder(waifu).Build());
        }

        [Command("Waifu"), Alias("Husbando"), Summary("Shows waifu details.\n**Usage**: `!waifu [name]`")]
        public async Task ShowWaifu([Remainder] string name)
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);
            if (waifu == null)
            {
                //await Context.Channel.SendMessageAsync($"Can't find '{name}'. I know they are not real, but this one *really is* just your imagination >_>");
                return;
            }

            var eb = WaifuUtil.WaifuEmbedBuilder(waifu, true, Context);

            await Context.Channel.SendMessageAsync("", false, eb.Build());

        }

        [Command("AllWaifus"), Alias("aw"), Summary("Lists all waifus. Tier Optional.\n**Usage**: `!aw [tier]`")]
        public async Task ListWaifus(int tier = 0)
        {
            await Context.Channel.SendMessageAsync("", false, WaifuUtil.WaifuListEmbedBuilder(tier).Build());
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

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"{name} not found.");
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

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"{name} not found.");
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

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"{name} not found.");
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

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"{name} not found.");
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

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"{name} not found.");
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
            await Context.Channel.SendMessageAsync("I'll try o7. Check if it worked.");
        }
        
        [Command("WaifuLeaderboard"), Alias("wlb"), Summary("Shows most popular waifus.\n**Usage**: `!wlb [page_number]`")]
        public async Task WaifuLeaderboard(int page = 1, [Remainder] string str = "")
        {
            var AllWaifus = UserInventoryDb.GetAllWaifuItems();
            var waifus = new Dictionary<Waifu, int>();
            var users = new Dictionary<SocketUser, int>();

            foreach(var x in AllWaifus)
            {
                if(!waifus.ContainsKey(x.Waifu))
                    waifus.Add(x.Waifu, AllWaifus.Count(y => y.Waifu.Equals(x.Waifu)));

                var user = Context.Guild.GetUser(x.UserId);
                if (user != null)
                    if (!users.ContainsKey(user))
                        users.Add(user, WaifuUtil.WaifuValue(UserInventoryDb.GetWaifus(user.Id, Context.Guild.Id)));
            }

            var ordWaifus = waifus.OrderByDescending(x => x.Value);
            var ordUsers = users.OrderByDescending(x => x.Value);

            page--;
            await Context.Channel.SendMessageAsync("", false, WaifuUtil.WaifuLeaderboardEmbed(ordWaifus, ordUsers, page).Build());
        }

        [Command("SetFeaturedWaifu"), Alias("sfw"), Summary("Sets your waifu image on your profile.\n**Usage**: `!sfw [waifu_name]`")]
        public async Task SetFeaturedWaifu(string name, [Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"{name} not found.");
                return;
            }

            if (UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id).Any(x => x.Name.Equals(waifu.Name)))
            {
                await FeaturedWaifuDb.SetFeaturedWaifu(Context.User.Id, waifu, Context.Guild.Id);
                await Context.Channel.SendMessageAsync($"{waifu.Name} set as your featured waifu!", false, WaifuUtil.ProfileEmbed((SocketGuildUser) Context.User).Build());
                return;
            }
            await Context.Channel.SendMessageAsync($":x: You don't have {waifu.Name}");
        }
    }
}
