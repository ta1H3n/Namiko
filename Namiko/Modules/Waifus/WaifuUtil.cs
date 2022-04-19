using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Model;
using Namiko.Modules.Basic;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Namiko
{
    public static class WaifuUtil
    {
        // Waifus Read/Write
        public static async Task<WaifuShop> GetShop(ulong guildId, ShopType type, bool overrideNew = false)
        {
            WaifuShop shop = null;
            try
            {
                shop = await WaifuShopDb.GetWaifuShop(guildId, type);
            }
            catch { }

            if (type == ShopType.Mod)
            {
                // Create mod shop if doesn't exist
                if (shop == null)
                {
                    shop = new WaifuShop
                    {
                        GeneratedDate = DateTime.Now,
                        GuildId = guildId,
                        Type = type,
                        ShopWaifus = new List<ShopWaifu>()
                    };
                    shop = await WaifuShopDb.AddShop(shop);
                }
                return shop;
            }

            if (overrideNew || shop == null || shop.GeneratedDate.AddHours(12) < DateTime.Now)
            {
                var newShop = await CreateNewShop(guildId, type);
                await WaifuShopDb.DeleteShop(guildId, type);
                await WaifuShopDb.AddShop(newShop);
                if (Program.Development == false)
                    _ = Task.Run(() => NotifyWishlist(newShop.ShopWaifus.Select(x => x.Waifu), guildId));
                return newShop;
            }

            return shop;
        }
        public static async Task<WaifuShop> CreateNewShop(ulong guildId, ShopType type)
        {
            var date = System.DateTime.Now.Date;
            if (DateTime.Now.Hour >= 12)
                date = date.AddHours(12);

            WaifuShop shop = new WaifuShop
            {
                GeneratedDate = date,
                GuildId = guildId,
                Type = type
            };

            List<ShopWaifu> waifus = null;

            switch (type)
            {
                case ShopType.Waifu:
                    waifus = await GenerateWaifuShopList(guildId);
                    break;
                case ShopType.Gacha:
                    waifus = await GenerateGachaShopList(guildId);
                    break;
                default:
                    return null;
            }

            shop.ShopWaifus = waifus;
            return shop;
        }
        public static async Task<List<ShopWaifu>> GenerateWaifuShopList(ulong guildId)
        {
            int limitedamount = Constants.shoplimitedamount;
            int t1amount = Constants.shopt1amount;
            int t2amount = Constants.shopt2amount;
            int t3amount = Constants.shopt3amount;
            int pages = 1;
            if (PremiumDb.IsPremium(guildId, ProType.GuildPlus) || PremiumDb.IsPremium(guildId, ProType.Guild))
                pages = 3;
            int randomizerMultiplier = 7 - pages;

            var gachaSource = GetGachaSources();
            var tier0 = await WaifuDb.GetWaifusByTier(0);
            var tier1 = await WaifuDb.RandomWaifus(1, (limitedamount + t1amount) * pages * randomizerMultiplier, excludeSource: gachaSource);
            var tier2 = await WaifuDb.RandomWaifus(2, t2amount * pages * randomizerMultiplier, excludeSource: gachaSource);
            var tier3 = await WaifuDb.RandomWaifus(3, t3amount * pages * randomizerMultiplier, excludeSource: gachaSource);

            var wishlists = await WaifuWishlistDb.GetAllPremiumWishlists(guildId, ProType.Pro);
            wishlists.RemoveAll(x => gachaSource.Contains(x.Waifu.Source));
            var ids = wishlists.Select(x => x.UserId).Distinct().ToArray();
            var guild = Program.GetClient().GetGuild(guildId);
            foreach (var id in ids)
            {
                SocketGuildUser user = null;
                try
                {
                    user = guild.GetUser(id);
                }
                catch
                {
                    wishlists.RemoveAll(x => x.UserId == id);
                }
                if (user == null)
                    wishlists.RemoveAll(x => x.UserId == id);
            }

            tier1.AddRange(wishlists.Where(x => x.Waifu.Tier == 1).Select(x => x.Waifu));
            tier2.AddRange(wishlists.Where(x => x.Waifu.Tier == 2).Select(x => x.Waifu));
            tier3.AddRange(wishlists.Where(x => x.Waifu.Tier == 3).Select(x => x.Waifu));

            ShopWaifu item = null;
            List<ShopWaifu> waifus = new List<ShopWaifu>();
            var rnd = new Random();
            int r = 0;

            for (int k = 0; k < pages; k++)
            {
                // LIMITED WAIFU
                for (int i = 0; i < limitedamount; i++)
                {
                    r = rnd.Next(0, tier1.Count);
                    item = new ShopWaifu { Waifu = tier1.ElementAt(r), Discount = GenerateDiscount(), Limited = 1, BoughtBy = 0 };
                    waifus.Add(item);
                    tier1.RemoveAll(x => x.Name.Equals(tier1[r].Name));
                }

                // TIER 0 AND 1 WAIFUS
                for (int i = 0; i < t1amount; i++)
                {
                    r = rnd.Next(0, tier1.Count + tier0.Count);

                    if (r < tier1.Count)
                    {
                        item = new ShopWaifu { Waifu = tier1.ElementAt(r), Limited = -1, BoughtBy = 0 };
                        tier1.RemoveAll(x => x.Name.Equals(tier1[r].Name));
                    }
                    else
                    {
                        r -= tier1.Count;
                        item = new ShopWaifu { Waifu = tier0[r], Limited = -1, BoughtBy = 0 };
                        tier0.RemoveAll(x => x.Name.Equals(tier0[r].Name));
                    }

                    waifus.Add(item);
                }

                // TIER 2 WAIFUS
                for (int i = 0; i < t2amount; i++)
                {
                    r = rnd.Next(0, tier2.Count);
                    item = new ShopWaifu { Waifu = tier2.ElementAt(r), Limited = -1, BoughtBy = 0 };
                    waifus.Add(item);
                    tier2.RemoveAll(x => x.Name.Equals(tier2[r].Name));
                }

                // TIER 3 WAIFUS
                for (int i = 0; i < t3amount; i++)
                {
                    r = rnd.Next(0, tier3.Count);
                    item = new ShopWaifu { Waifu = tier3.ElementAt(r), Limited = -1, BoughtBy = 0 };
                    waifus.Add(item);
                    tier3.RemoveAll(x => x.Name.Equals(tier3[r].Name));
                }
            }

            return waifus;
        }
        public static async Task<List<ShopWaifu>> GenerateGachaShopList(ulong guildId)
        {
            int t1amount = Constants.gachat1amount;
            int t2amount = Constants.gachat2amount;
            int t3amount = Constants.gachat3amount;
            int pages = 1;
            if (PremiumDb.IsPremium(guildId, ProType.GuildPlus) || PremiumDb.IsPremium(guildId, ProType.Guild))
                pages = 3;
            int randomizerMultiplier = 7 - pages;

            var waifus = new List<Waifu>();

            var gachaSource = GetGachaSources();
            waifus.AddRange(await WaifuDb.RandomWaifus(1, t1amount * pages * randomizerMultiplier, includeSource: gachaSource));
            waifus.AddRange(await WaifuDb.RandomWaifus(2, t2amount * pages * randomizerMultiplier, includeSource: gachaSource));
            waifus.AddRange(await WaifuDb.RandomWaifus(3, t3amount * pages * randomizerMultiplier, includeSource: gachaSource));

            var wishlists = await WaifuWishlistDb.GetAllPremiumWishlists(guildId, ProType.Pro);
            wishlists.RemoveAll(x => !gachaSource.Contains(x.Waifu.Source));
            var ids = wishlists.Select(x => x.UserId).Distinct().ToArray();
            var guild = Program.GetClient().GetGuild(guildId);
            foreach (var id in ids)
            {
                SocketGuildUser user = null;
                try
                {
                    user = guild.GetUser(id);
                }
                catch
                {
                    wishlists.RemoveAll(x => x.UserId == id);
                }
                if (user == null)
                    wishlists.RemoveAll(x => x.UserId == id);
            }
            waifus.AddRange(wishlists.Select(x => x.Waifu));

            ShopWaifu item = null;
            List<ShopWaifu> finalWaifus = new List<ShopWaifu>();
            var rnd = new Random();
            int r = 0;

            for (int k = 0; k < Constants.gachatotal * pages; k++)
            {
                r = rnd.Next(0, waifus.Count);
                item = new ShopWaifu { Waifu = waifus.ElementAt(r), Limited = -1, BoughtBy = 0 };
                finalWaifus.Add(item);
                waifus.RemoveAll(x => x.Name.Equals(waifus[r].Name));
            }

            return finalWaifus.OrderBy(x => x.Waifu.Tier).ToList();
        }
        public static List<ShopWaifu> OrderWaifuShop(List<ShopWaifu> waifus, ShopType type)
        {
            int count = 0;

            switch (type)
            {
                case ShopType.Waifu:
                    count = Constants.shoplimitedamount + Constants.shopt1amount + Constants.shopt2amount + Constants.shopt3amount;
                    break;
                case ShopType.Gacha:
                    count = Constants.gachalimitedamount + Constants.gachat1amount + Constants.gachat2amount + Constants.gachat3amount;
                    break;
                default:
                    break;
            }

            if (waifus.Count <= count)
            {
                waifus = waifus.OrderByDescending(x => x.Limited).ThenBy(x => x.Waifu.Tier).ToList();
            }

            return waifus;
        }
        public static WaifuShop OrderWaifuShop(this WaifuShop shop)
        {
            shop.ShopWaifus = OrderWaifuShop(shop.ShopWaifus, shop.Type);
            return shop;
        }
        public static List<string> GetGachaSources()
        {
            var par = ParamDb.GetParam(name: "GachaSource");
            return par.Select(x => x.Args).ToList();
        }

        public static async Task NotifyWishlist(IEnumerable<Waifu> waifus, ulong guildId)
        {
            var wishes = await WaifuWishlistDb.GetWishlist(guildId);
            foreach (var wish in wishes)
            {
                if (waifus.Any(x => x.Name == wish.Waifu.Name))
                {
                    try
                    {
                        var guild = Program.GetClient().GetGuild(guildId);
                        var ch = await guild.GetUser(wish.UserId).CreateDMChannelAsync();
                        await ch.SendMessageAsync($"**{wish.Waifu.Name}** is now for sale in **{guild.Name}**", false, WaifuEmbedBuilder(wish.Waifu).Build());
                    }
                    catch { }
                }
            }
        }

        // Shop Message generators
        /*public static EmbedBuilder GetShopEmbed(List<ShopWaifu> waifus, string prefix)
        {
            var client = Program.GetClient();
            var eb = new EmbedBuilder();
            eb.WithAuthor("Waifu Store", client.CurrentUser.GetAvatarUrl());
            foreach (var x in waifus)
            {

                var efb = new EmbedFieldBuilder{
                    IsInline = true,
                    Name = x.Waifu.Name,
                    Value = $"TIER: **{x.Waifu.Tier}**\nPrice: {GetPriceString(x.Waifu.Tier, x.Discount)}"
                };
                //efb.IsInline = true;
                //efb.Name = x.Waifu.Name;
                //efb.Value = $"TIER: **{x.Waifu.Tier}**\nPrice: {GetPriceString(x.Waifu.Tier, x.Discount)}";

                if (x.Limited > -1)
                {
                    efb.IsInline = false;
                    efb.Value += "\n" + "**Limited!**";
                    if (x.Limited > 0)
                        efb.Value += $" {x.Limited} in stock!";
                    else
                        efb.Value += $" OUT OF STOCK! Bought by: {client.GetUser(x.BoughtBy).Mention}";
                }

                eb.AddField(efb);
            }

            ///////////////////////////
            //INSERT THUMBNAIL
            ///////////////////////////
            eb.WithFooter($"{prefix}buywaifu [name] | Resets in {23 - DateTime.Now.Hour} Hours {60 - DateTime.Now.Minute} Minutes");
            eb.Color = BasicUtil.RandomColor();
            return eb;
        }
        public static EmbedBuilder WaifuShopEmbed(List<ShopWaifu> waifus, string prefix)
        {
            var client = Program.GetClient();
            var eb = new EmbedBuilder();
            eb.WithAuthor("Waifu Store", client.CurrentUser.GetAvatarUrl());
            
            string list = "";
            foreach (var x in waifus)
            {
                string src = x.Waifu.Source.Length > 33 ? (x.Waifu.Source.Substring(0, 30) + "...") : x.Waifu.Source;
                string listing = $"T{x.Waifu.Tier} {GetPriceString(x.Waifu.Tier, x.Discount)}: **{x.Waifu.Name}** - *{src}*\n";

                if (x.Limited > -1)
                {
                    listing += "    **-Limited!**";
                    if (x.Limited > 0)
                        listing += $" {x.Limited} in stock!\n";
                    else
                        listing += $" OUT OF STOCK! Bought by: {client.GetUser(x.BoughtBy).Mention}\n";
                    list += listing;
                }
                else
                    list += listing;
            }

            eb.WithDescription(list);
            eb.WithFooter($"`{prefix}buywaifu [name]` `{prefix}waifu [name]` | Resets in {23 - DateTime.Now.Hour} Hours {60 - DateTime.Now.Minute} Minutes");
            eb.Color = BasicUtil.RandomColor();
            return eb;
        }*/
        public static EmbedBuilder NewShopEmbed(List<ShopWaifu> waifus, string prefix, ulong guildId = 0, ShopType type = ShopType.Waifu)
        {
            var client = Program.GetClient();
            var eb = new EmbedBuilder();

            if (type == ShopType.Gacha)
                eb.WithAuthor("Gacha Shop", client.CurrentUser.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-shop"));
            else
            if (type == ShopType.Mod)
            {
                eb.WithAuthor("Mod Shop", client.CurrentUser.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-shop"));
                eb.WithDescription($"A waifu shop controlled by server moderators.\n`{prefix}MsAddWaifu` | `{prefix}MsRemoveWaifu` - requires Namiko Pro Guild+.");
            }
            else
            {
                eb.WithAuthor("Waifu Shop", client.CurrentUser.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-shop"));
                if (guildId != 0)
                    eb.WithDescription($"Open in [browser](https://namiko.moe/WaifuShop/{guildId})");
            }

            string list = WaifuShopWaifuList(waifus);

            eb.AddField("\n:books: Commands", $"`{prefix}BuyWaifu [name]` | `{prefix}Waifu [search]` | `{prefix}Ws` | `{prefix}Gs` | `{prefix}Ms`", true);

            list = list.Any() ? list : "~ Shop Empty ~";
            eb.AddField("<:MiaHug:536580304018735135> Waifus", list, false);
            //eb.WithThumbnailUrl(waifus[0].Waifu.ImageUrl);
            if (type != ShopType.Mod)
                eb.WithFooter($"Resets in {11 - DateTime.Now.Hour % 12} Hours {60 - DateTime.Now.Minute} Minutes");
            eb.Color = BasicUtil.RandomColor();
            return eb;
        }
        public static CustomPaginatedMessage PaginatedShopMessage(IEnumerable<ShopWaifu> waifus, int pageSize, string prefix, ulong guildId = 0, ShopType type = ShopType.Waifu)
        {
            CustomPaginatedMessage paginatedMessage = new CustomPaginatedMessage();
            var fieldList = new List<FieldPages>();
            var splitWaifus = CustomPaginatedMessage.Split(waifus, pageSize);
            int pages = splitWaifus.Count();

            var fieldInfo = new FieldPages();
            var pagelist = new List<string>();
            fieldInfo.Title = ":books: Commands";
            for (int i = 0; i < pages; i++)
            {
                pagelist.Add($"`{prefix}BuyWaifu [name]` | `{prefix}Waifu [search]` | `{prefix}Ws` | `{prefix}Gs` | `{prefix}Ms`");
            }
            fieldInfo.Inline = true;
            fieldInfo.Pages = pagelist;
            fieldList.Add(fieldInfo);

            var fieldWaifus = new FieldPages();
            var pagelist2 = new List<string>();
            fieldWaifus.Title = "<:MiaHug:536580304018735135> Waifus";
            foreach (var w in splitWaifus)
            {
                pagelist2.Add(WaifuUtil.WaifuShopWaifuList(w));
            }
            fieldWaifus.Pages = pagelist2;
            fieldList.Add(fieldWaifus);

            paginatedMessage.Fields = fieldList;
            paginatedMessage.Footer = $"Resets in {11 - DateTime.Now.Hour % 12} Hours {60 - DateTime.Now.Minute} Minutes | ";
            paginatedMessage.Color = BasicUtil.RandomColor();
            paginatedMessage.PageCount = pages;
            if (guildId != 0)
                paginatedMessage.Pages = new List<string> { $"Open in [browser](https://namiko.moe/WaifuShop/{guildId})" };
            paginatedMessage.Author = new EmbedAuthorBuilder()
            {
                Name = type switch
                {
                    ShopType.Waifu => "Waifu Shop",
                    ShopType.Gacha => "Gacha Shop",
                    ShopType.Mod => "Mod Shop",
                    _ => "Waifu Shop"
                },
                IconUrl = Program.GetClient().CurrentUser.GetAvatarUrl(),
                Url = LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-shop")
            };

            return paginatedMessage;
        }
        public static string WaifuShopWaifuList(IEnumerable<ShopWaifu> waifus)
        {
            string list = "";
            foreach (var x in waifus)
            {
                string price = GetPriceString(x.Waifu.Tier, x.Discount).Count() > 5 ? GetPriceString(x.Waifu.Tier, x.Discount) : GetPriceString(x.Waifu.Tier, x.Discount) + "​‏ ‏‏‎";
                string src = x.Waifu.Source.Length > 33 ? (x.Waifu.Source.Substring(0, 30) + "...") : x.Waifu.Source;
                string listing = $"`T{x.Waifu.Tier}` `{price}` **{x.Waifu.Name}** - *{src}*\n";

                if (x.Limited > -1)
                {
                    listing += "    **-Limited!**";
                    if (x.Limited > 0)
                        listing += $" {x.Limited} in stock!\n";
                    else
                    {
                        try
                        {
                            listing += $" OUT OF STOCK! Bought by: {Program.GetClient().GetUser(x.BoughtBy).Mention}\n";
                        }
                        catch
                        {
                            listing += $" OUT OF STOCK!\n";
                        }
                    }
                    list += listing;
                }
                else
                    list += listing;
            }
            list += "";

            return list;
        }
        public static EmbedBuilder WaifuShopSlideEmbed(Waifu waifu)
        {
            var eb = new EmbedBuilder();
            eb.Description = $"**{waifu.LongName}**\nT{waifu.Tier} - `{GetPrice(waifu.Tier)}` {ToastieUtil.RandomEmote()}";
            eb.WithImageUrl(waifu.HostImageUrl);
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }

        //Waifu Prices
        public static int GenerateDiscount()
        {
            var rand = new Random();
            return rand.Next(1, 6) * 5;
        }
        public static string GetPriceString(int tier, int discount = 0)
        {
            if (discount == 0)
                return GetPrice(tier).ToString("n0");

            else
                // return $"~~{GetPrice(tier).ToString("n0")}~~ {GetPrice(tier, discount).ToString("n0")} (-{discount}%)";
                return $"{GetPrice(tier, discount).ToString("n0")} (-{discount}%)";
        }
        public static int GetPrice(int tier, int discount = 0)
        {
            int price = tier == 1 ? Constants.tier1 :
                        tier == 2 ? Constants.tier2 :
                        tier == 3 ? Constants.tier3 :
                        tier == 0 ? Constants.tier0 :

                        //default
                        0;

            if (discount >= 0)
                price -= price / 100 * discount;

            return price;
        }
        public static int GetSalePrice(int tier)
        {
            int worth = (int)(GetPrice(tier) * ((tier == 3) ? 0.70 :   //70%
                                                (tier == 2) ? 0.60 :    //60%

                                                //default, also 55%
                                                0.55));
            return worth;
        }
        public static int FavoritesToTier(int fav)
        {
            int tier = fav > 7000 ? 1 :
                        fav > 800 ? 2 :
                        fav < 100 ? 0 :
                        3;

            return tier;
        }
        public static int WaifuValue(IEnumerable<Waifu> waifus)
        {
            return waifus.Sum(x => GetPrice(x.Tier));
        }

        //Random embeds
        public static EmbedBuilder WaifuEmbedBuilder(Waifu waifu, SocketCommandContext context = null)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithAuthor(waifu.Name, null, LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-waifu"));

            string desc = "";
            if (waifu.LongName != null)
                desc += $"**{waifu.LongName}**\n";

            if (waifu.Source != null)
                desc += $"*{waifu.Source}*\n";

            if (waifu.Description != null)
            {
                if (waifu.LongName != null || waifu.Description != null)
                    desc += "\n";
                desc += waifu.Description;
            }
            if (desc != "")
            {
                if (desc.Length > 2000)
                    desc = desc.Substring(0, 1995) + "...";
                eb.WithDescription(desc);
            }

            if (waifu.HostImageUrl != null)
                eb.WithImageUrl(waifu.HostImageUrl);

            string footer = $"Tier: {waifu.Tier}";
            if (context != null)
            {
                footer += WaifuOwnerString(waifu, context);

                var wishes = WaifuWishlistDb.GetWishlist(context.Guild.Id, waifu.Name);
                if (wishes.Any())
                {
                    try
                    {
                        eb.AddField("Wanted By", WaifuWantedString(wishes, context));
                    }
                    catch { }
                }
            }
            eb.WithFooter(footer);


            eb.Color = BasicUtil.RandomColor();
            return eb;
        }
        public static string WaifuOwnerString(Waifu waifu, SocketCommandContext context)
        {
            var userIds = UserInventoryDb.GetOwners(waifu, context.Guild.Id);
            string str = null;
            if (userIds.Count > 0)
            {
                str = ", Owned by: ";
                foreach (ulong id in userIds)
                {
                    try
                    {
                        var mention = context.Guild.GetUser(id).Username;
                        str += $"`{mention}` ";
                    }
                    catch { }
                }
            }
            return str;
        }
        public static string WaifuWantedString(IEnumerable<WaifuWish> wishes, SocketCommandContext context)
        {
            string str = "";
            foreach (var wish in wishes)
            {
                try
                {
                    str += $"`{context.Guild.GetUser(wish.UserId).Username}` ";
                }
                catch { }
            }
            return str;
        }
        public static async Task<EmbedBuilder> WaifuListEmbedBuilder(int tier = 0)
        {
            var eb = new EmbedBuilder();

            if (tier == 0)
            {
                for (int i = 1; i < 4; i++)
                {
                    string field = "";
                    var waifus = await WaifuDb.GetWaifusByTier(i);
                    waifus = waifus.OrderBy(x => x.Name).ToList();
                    foreach (Waifu x in waifus)
                        field += "`" + x.Name + "` ";
                    if (field != "")
                        eb.AddField("Tier " + i, field);
                }
            }

            else
            {
                string field = "";
                var waifus = await WaifuDb.GetWaifusByTier(tier);
                waifus = waifus.OrderBy(x => x.Name).ToList();
                foreach (Waifu x in waifus)
                    field += "`" + x.Name + "` ";
                eb.WithDescription(field);
            }
            eb.Color = BasicUtil.RandomColor();
            return eb;
        }
        public static string FoundWaifusCodeBlock(List<Waifu> waifusRaw)
        {
            var waifus = waifusRaw.OrderBy(x => x.Source).ThenBy(x => x.Name);
            var grouped = waifus.GroupBy(x => x.Source);

            string text = "```json\nWAIFUS FOUND:\n\n";
            // foreach (var x in waifus)
            // {
            //     text += String.Format("{0, 11} - {1}- {2}\n", x.Name, BasicUtil.ShortenString(x.Source, 35, 35, ""), BasicUtil.ShortenString(x.LongName, 35, 34, "-"));
            // }
            foreach (var x in grouped)
            {
                text += String.Format("\"{0}\"\n", x.Key);
                foreach (var waifu in x)
                {
                    text += String.Format("{0, 11} - {1}\n", waifu.Name, BasicUtil.ShortenString(waifu.LongName, 35, 34, "-"));
                }
            }
            text += "```";
            return text;
        }
        public static EmbedBuilder FoundWaifusEmbedBuilder(IEnumerable<IGrouping<string, Waifu>> waifus, SocketGuildUser user = null)
        {
            var eb = new EmbedBuilderPrepared();

            int i = 1;
            foreach (var x in waifus)
            {
                if (x.Count() > 15)
                {
                    var split = x.GroupBy(10);
                    foreach (var y in split)
                    {
                        string list = "";
                        foreach (var waifu in y)
                        {
                            list += $"`#{i++}` ";
                            try
                            {
                                if (user != null && UserInventoryDb.OwnsWaifu(user.Id, waifu, user.Guild.Id))
                                    list += "✓ ";
                            }
                            catch { }
                            list += $"**{waifu.Name}** - *{BasicUtil.ShortenString(waifu.LongName, 28, 27, "-")}*\n";
                        }
                        eb.AddField(x.Key + "#" + (y.Key + 1), list);
                    }
                }
                else
                {
                    string list = "";
                    foreach (var waifu in x)
                    {
                        list += $"`#{i++}` ";
                        try
                        {
                            if (user != null && UserInventoryDb.OwnsWaifu(user.Id, waifu, user.Guild.Id))
                                list += "✓ ";
                        }
                        catch { }
                        list += $"**{waifu.Name}** - *{BasicUtil.ShortenString(waifu.LongName, 28, 27, "-")}*\n";
                    }
                    eb.AddField(x.Key, list);
                }
            }

            eb.WithAuthor("Waifus Found", user?.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-waifulist"));
            eb.WithDescription("Enter the number of the waifu you wish to select.");
            eb.WithFooter("Times out in 23 seconds.");
            return eb;
        }
        public static EmbedBuilder WaifuLeaderboardEmbed(IOrderedEnumerable<KeyValuePair<Waifu, int>> waifus, IOrderedEnumerable<KeyValuePair<SocketUser, int>> users, int page)
        {
            var eb = new EmbedBuilder();
            eb.WithTitle(":star: Waifu Leaderboard");
            page = page == 0 ? 0 : page * 10;

            string waifu = "";
            string user = "";
            for (int i = page; i < page + 10; i++)
            {
                var x = waifus.ElementAtOrDefault(i);
                if (x.Key != null)
                    waifu += $"#{i + 1} {x.Key.Name} - {x.Value}\n";

                var y = users.ElementAtOrDefault(i);
                if (y.Key != null)
                    user += $"#{i + 1} {y.Key.Mention} - {y.Value}\n";
            }
            if (waifu == "")
                waifu = "-";
            if (user == "")
                user = "-";

            eb.AddField("Waifus :two_hearts:", waifu, true);
            eb.AddField("Users <:toastie3:454441133876183060>", user, true);

            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter($"Page: {page / 10 + 1}");
            return eb;
        }
        public static IEnumerable<IGrouping<int, TSource>> GroupBy<TSource>(this IEnumerable<TSource> source, int itemsPerGroup)
        {
            return source.Zip(Enumerable.Range(0, source.Count()),
                              (s, r) => new { Group = r / itemsPerGroup, Item = s })
                         .GroupBy(i => i.Group, g => g.Item)
                         .ToList();
        }
        public static EmbedBuilder WishlistEmbed(IEnumerable<Waifu> waifus, SocketGuildUser user)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);

            if (waifus.Any())
            {
                string wstr = "";
                foreach (var x in waifus)
                {
                    if (x.Source.Length > 45)
                        x.Source = x.Source.Substring(0, 33) + "...";
                    x.Source ??= "";
                    wstr += String.Format("**{0}** - *{1}*\n", x.Name, x.Source);
                }
                eb.AddField("Wishlist", wstr);
                eb.WithImageUrl(waifus.Last().HostImageUrl);
            }
            else
            {
                string desc = "Your mind is empty, you have no desires. You do not wish for any waifus.\n\n"
                    + $"*A pillar of light reveals strange texts*\n"
                    + $"```{Program.GetPrefix(user)}waifu\n{Program.GetPrefix(user)}wishwaifu```";
                eb.WithDescription(desc);
            }

            eb.WithColor(ProfileDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());
            return eb;
        }

        //Selector
        public static async Task<Waifu> ProcessWaifuListAndRespond(List<Waifu> waifus, InteractiveBase<ShardedCommandContext> interactive = null)
        {
            if (waifus.Count == 1)
                return waifus[0];

            if (interactive != null)
            {
                if (waifus.Count > 0)
                {
                    var ordered = waifus.OrderBy(x => x.Source).ThenBy(x => x.Name).ToList();
                    var grouped = ordered.GroupBy(x => x.Source);
                    RestUserMessage msg = null;
                    try
                    {
                        msg = await interactive.Context.Channel.SendMessageAsync(embed: FoundWaifusEmbedBuilder(grouped, (SocketGuildUser)interactive?.Context.User).Build());
                    }
                    catch
                    {
                        _ = interactive.Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared()
                            .WithAuthor("Waifus Found", interactive?.Context.User.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-waifulist"))
                            .WithDescription("*~ Too many results ~*")
                            .WithColor(255, 255, 255)
                            .Build());
                        return null;
                    }
                    var response = await interactive.NextMessageAsync(
                        new Criteria<IMessage>()
                        .AddCriterion(new EnsureSourceUserCriterion())
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureRangeCriterion(waifus.Count, Program.GetPrefix(interactive.Context))),
                        new TimeSpan(0, 0, 23));

                    _ = msg.DeleteAsync();
                    int i = 0;
                    try
                    {
                        i = int.Parse(response.Content);
                    }
                    catch
                    {
                        _ = interactive.Context.Message.DeleteAsync();
                        return null;
                    }
                    _ = response.DeleteAsync();

                    return ordered[i - 1];
                }

                _ = interactive.Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared()
                    .WithAuthor("Waifus Found", interactive?.Context.User.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-waifulist"))
                    .WithDescription("*~ No results ~*")
                    .WithColor(201, 0, 16)
                    .Build());
            }

            return null;
        }

        //Image handlers
        public static async Task UploadWaifuImage(Waifu waifu, ISocketMessageChannel ch)
        {
            try
            {
                if (waifu.ImageUrl == null || waifu.ImageUrl == "")
                    return;

                using WebClient client = new WebClient();

                string filetype = waifu.ImageUrl.Split('.').Last();
                string imgurId = waifu.ImageUrl.Split('/').Last().Split('.').First();
                string domain = "https://i.imgur.com/";
                string path = "waifus";

                var tasks = new List<Task>();
                tasks.Add(ImageUtil.UploadImage(path, waifu.ImageRaw,    domain + imgurId + "."  + filetype));
                tasks.Add(ImageUtil.UploadImage(path, waifu.ImageLarge,  domain + imgurId + "l." + filetype));
                tasks.Add(ImageUtil.UploadImage(path, waifu.ImageMedium, domain + imgurId + "m." + filetype));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                await ch.SendMessageAsync($"{Program.GetClient().GetUser(AppSettings.OwnerId).Mention} Error while downloading waifu image variants to server.");
                SentrySdk.WithScope(scope =>
                {
                    scope.SetExtras(waifu.GetProperties());
                    SentrySdk.CaptureException(ex);
                });
            }
        }
        public static async Task FindAndUpdateWaifuImageSource(Waifu waifu, ISocketMessageChannel ch)
        {
            try
            {
                if (waifu.ImageUrl == null || waifu.ImageUrl == "")
                    return;

                var res = await WebUtil.SauceNETSearchAsync(waifu.ImageUrl);
                string sauce = "";

                foreach (var result in res.Results.OrderByDescending(x => Double.Parse(x.Similarity)))
                {
                    if (Double.Parse(result.Similarity) > 80)
                    {
                        sauce = result.SourceURL;
                        await ch.SendMessageAsync($"<:TickYes:577838859107303424> **{waifu.Name}** - {result.DatabaseName} {result.Similarity}% ({result.SourceURL})");
                        break;
                    }
                    else if ((result.DatabaseName == "Pixiv" ||
                        result.DatabaseName == "Danbooru" ||
                        result.DatabaseName == "Gelbooru" ||
                        result.DatabaseName == "AniDb" ||
                        result.DatabaseName == "Twitter") &&
                        Double.Parse(result.Similarity) > 60)
                    {
                        sauce = result.SourceURL;
                        await ch.SendMessageAsync($":question: **{waifu.Name}** - {result.DatabaseName} {result.Similarity}% ({result.SourceURL})\n" +
                            $"Please verify if image sauce is correct.\n" +
                            $"`!getwaifu {waifu.Name}` - check waifu details.\n" +
                            $"`!wis {waifu.Name} [correct_source]` - change waifu image source.\n" +
                            $"If you can't find the correct source, set waifu image source to `missing`.",
                            embed: WebUtil.SauceEmbed(res, waifu.ImageUrl).Build());
                        break;
                    }
                    else if (result.DatabaseName == "AniDb" && Double.Parse(result.Similarity) > 40)
                    {
                        sauce = result.SourceURL;
                        await ch.SendMessageAsync($":question: **{waifu.Name}** - {result.DatabaseName} {result.Similarity}% ({result.SourceURL})\n" +
                            $"Please verify if image sauce is correct.\n" +
                            $"`!getwaifu {waifu.Name}` - check waifu details.\n" +
                            $"`!wis {waifu.Name} [correct_source]` - change waifu image source.\n" +
                            $"If you can't find the correct source, set waifu image source to `missing`.",
                            embed: WebUtil.SauceEmbed(res, waifu.ImageUrl).Build());
                        break;
                    }
                }

                if (sauce != "")
                {
                    waifu.ImageSource = sauce;
                    await WaifuDb.UpdateWaifu(waifu);
                }
                else
                {
                    waifu.ImageSource = "retry";
                    await ch.SendMessageAsync($":x: Could not find sauce! Please find the correct image source!\n" +
                        $"`!getwaifu {waifu.Name}` - check waifu details.\n" +
                        $"`!wis {waifu.Name} [correct_source]` - change waifu image source.\n" +
                        $"If you can't find the correct source, set waifu image source to `missing`.",
                        embed: WebUtil.SauceEmbed(res, waifu.ImageUrl).Build());
                }
            }
            catch (Exception ex)
            {
                await ch.SendMessageAsync($":x: Error searching for image sauce! Please find the correct image source!\n" +
                    $"`!getwaifu {waifu.Name}` - check waifu details.\n" +
                    $"`!wis {waifu.Name} [correct_source]` - change waifu image source.\n" +
                    $"If you can't find the correct source, set waifu image source to `missing`.");
                SentrySdk.WithScope(scope =>
                {
                    scope.SetExtras(waifu.GetProperties());
                    SentrySdk.CaptureException(ex);
                });
            }
        }
    }
}
