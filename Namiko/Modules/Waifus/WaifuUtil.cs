using Discord;
using System;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;



using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Namiko
{
    public static class WaifuUtil
    {
        public static async Task<List<ShopWaifu>> GetShopWaifus(ulong guildId)
        {
            var contents = WaifuShopDb.GetWaifuStores(guildId);

            if (contents == null || contents[0].GeneratedDate.AddHours(12) < System.DateTime.Now)
            {
                var list = await GenerateWaifuList(guildId);
                await WaifuShopDb.NewList(list);
                return list;
            }

            return contents;

        }
        public static async Task<List<ShopWaifu>> GenerateWaifuList(ulong guildId)
        {
            var date = System.DateTime.Now.Date;
            if (DateTime.Now.Hour >= 12)
                date = date.AddHours(12);

            int limitedamount = Constants.shoplimitedamount;
            int t1amount = Constants.shopt1amount;
            int t2amount = Constants.shopt2amount;
            int t3amount = Constants.shopt3amount;
            int pages = 1;
            if (PremiumDb.IsPremium(guildId, PremiumType.ServerT2))
                pages = 3;
            if (PremiumDb.IsPremium(guildId, PremiumType.ServerT1))
                pages = 5;
            int randomizerMultiplier = 3 + ((5 - pages) / 2);

            var tier0 = WaifuDb.GetWaifusByTier(0);
            var tier1 = WaifuDb.RandomWaifus(1, (limitedamount + t1amount) * pages * randomizerMultiplier);
            var tier2 = WaifuDb.RandomWaifus(2, t2amount * pages * randomizerMultiplier);
            var tier3 = WaifuDb.RandomWaifus(3, t3amount * pages * randomizerMultiplier);

            var wishlists = WaifuWishlistDb.GetAllPremiumWishlists(guildId, PremiumType.Toastie);
            var ids = wishlists.Select(x => x.UserId).Distinct();
            var guild = Program.GetClient().GetGuild(guildId);
            foreach(var id in ids)
            {
                SocketGuildUser user = null;
                try
                {
                    user = guild.GetUser(id);
                } catch
                {
                    wishlists.RemoveAll(x => x.UserId == id);
                }
                if(user == null)
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
                    item = new ShopWaifu { Waifu = tier1.ElementAt(r), GeneratedDate = date, Discount = GenerateDiscount(), Limited = 1, BoughtBy = 0, GuildId = guildId };
                    waifus.Add(item);
                }

                // TIER 0 AND 1 WAIFUS
                for (int i = 0; i < t1amount; i++)
                {
                    r = rnd.Next(0, tier1.Count + tier0.Count);

                    if (r < tier1.Count)
                    {
                        item = new ShopWaifu { Waifu = tier1.ElementAt(r), GeneratedDate = date, Limited = -1, BoughtBy = 0, GuildId = guildId };
                        tier1.RemoveAll(x => x.Name.Equals(tier1[r].Name));
                    }
                    else
                    {
                        r = r - tier1.Count;
                        item = new ShopWaifu { Waifu = tier0[r], GeneratedDate = date, Limited = -1, BoughtBy = 0, GuildId = guildId };
                        tier0.RemoveAll(x => x.Name.Equals(tier0[r].Name));
                    }

                    waifus.Add(item);
                }

                // TIER 2 WAIFUS
                for (int i = 0; i < t2amount; i++)
                {
                    r = rnd.Next(0, tier2.Count);
                    item = new ShopWaifu { Waifu = tier2.ElementAt(r), GeneratedDate = date, Limited = -1, BoughtBy = 0, GuildId = guildId };
                    waifus.Add(item);
                    tier2.RemoveAll(x => x.Name.Equals(tier2[r].Name));
                }

                // TIER 3 WAIFUS
                for (int i = 0; i < t3amount; i++)
                {
                    r = rnd.Next(0, tier3.Count);
                    item = new ShopWaifu { Waifu = tier3.ElementAt(r), GeneratedDate = date, Limited = -1, BoughtBy = 0, GuildId = guildId };
                    waifus.Add(item);
                    tier3.RemoveAll(x => x.Name.Equals(tier3[r].Name));
                }
            }

            _ = Task.Run(() => NotifyWishlist(waifus.Select(x => x.Waifu), guildId));
            return waifus;
        }
        public static async Task NotifyWishlist(IEnumerable<Waifu> waifus, ulong guildId)
        {
            var wishes = WaifuWishlistDb.GetWishlist(guildId);
            foreach(var wish in wishes)
            {
                if(waifus.Any(x => x.Name == wish.Waifu.Name))
                {
                    try
                    {
                        var guild = Program.GetClient().GetGuild(guildId);
                        var ch = await guild.GetUser(wish.UserId).GetOrCreateDMChannelAsync();
                        await ch.SendMessageAsync($"**{wish.Waifu.Name}** is now for sale in **{guild.Name}**", false, WaifuEmbedBuilder(wish.Waifu).Build());
                    }
                    catch { }
                }
            }
        }

        public static EmbedBuilder GetShopEmbed(List<ShopWaifu> waifus, string prefix)
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
        }
        public static EmbedBuilder NewShopEmbed(List<ShopWaifu> waifus, string prefix)
        {
            var client = Program.GetClient();
            var eb = new EmbedBuilder();
            eb.WithAuthor("Waifu Store", client.CurrentUser.GetAvatarUrl(), BasicUtil._patreon);

            string list = WaifuShopWaifuList(waifus);

            eb.AddField(":books: Commands", $"`{prefix}buywaifu [name]` | `{prefix}waifu [search]` | `{prefix}wss`", true);
            eb.AddField(":revolving_hearts: Waifus", list, true);
            //eb.WithThumbnailUrl(waifus[0].Waifu.ImageUrl);
            eb.WithFooter($"Resets in {11 - DateTime.Now.Hour%12} Hours {60 - DateTime.Now.Minute} Minutes");
            eb.Color = BasicUtil.RandomColor();
            return eb;
        }
        public static CustomPaginatedMessage PaginatedShopMessage(IEnumerable<ShopWaifu> waifus, int pageSize, string prefix)
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
                pagelist.Add($"`{prefix}buywaifu [name]` | `{prefix}waifu [search]` | `{prefix}wss`");
            }
            fieldInfo.Inline = true;
            fieldInfo.Pages = pagelist;
            fieldList.Add(fieldInfo);

            var fieldWaifus = new FieldPages();
            var pagelist2 = new List<string>();
            fieldWaifus.Title = ":revolving_hearts: Waifus";
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
            paginatedMessage.Author = new EmbedAuthorBuilder()
            {
                Name = "Waifu Store",
                IconUrl = Program.GetClient().CurrentUser.GetAvatarUrl(),
                Url = BasicUtil._patreon
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
            eb.WithImageUrl(waifu.ImageUrl);
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }

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
                price = price - (price / 100 * discount);

            return price;
        }
        public static int GetSalePrice(int tier) {

            //creating nessacary variables 
            Random rnd = new Random();
            int num = rnd.Next(1, 31);
            int worth = (int)(GetPrice(tier) *  ((tier == 3)? 0.70 :   //70%
                                                (tier == 2)? 0.60 :    //60%

                                                //default, also 55%
                                                0.55));

            //random 3 in 17 chance to have increased sale
            //if (num < 10) return worth + worth / (num * 4);
            return worth;
        }

        public static EmbedBuilder WaifuEmbedBuilder(Waifu waifu, bool guildDetails = false, SocketCommandContext context = null)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithAuthor(waifu.Name, null, BasicUtil._patreon);

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
                eb.WithDescription(desc);

            if (waifu.ImageUrl != null)
                eb.WithImageUrl(waifu.ImageUrl);

            if (guildDetails)
            {
                string footer = ($"Tier: {waifu.Tier}");
                footer += WaifuOwnerString(waifu, context);
                eb.WithFooter(footer);

                var wishes = WaifuWishlistDb.GetWishlist(context.Guild.Id, waifu.Name);
                if(wishes.Any())
                {
                    try
                    {
                        eb.AddField("Wanted By", WaifuWantedString(wishes, context));
                    } catch { }
                }
            }
            

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
                    }catch { }
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
        public static EmbedBuilder WaifuListEmbedBuilder(int tier = 0)
        {
            var eb = new EmbedBuilder();

            if (tier == 0)
            {
                for (int i = 1; i < 4; i++)
                {
                    string field = "";
                    var waifus = WaifuDb.GetWaifusByTier(i);
                    waifus = waifus.OrderBy(x => x.Name).ToList();
                    foreach (Waifu x in waifus)
                        field += "`" + x.Name + "` ";
                    if(field != "")
                        eb.AddField("Tier " + i, field);
                }
            }

            else
            {
                string field = "";
                var waifus = WaifuDb.GetWaifusByTier(tier);
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
                foreach(var waifu in x)
                {
                    text += String.Format("{0, 11} - {1}\n", waifu.Name, BasicUtil.ShortenString(waifu.LongName, 35, 34, "-"));
                }
            }
            text += "```";
            return text;
        }
        public static EmbedBuilder FoundWaifusEmbedBuilder(IEnumerable<IGrouping<string, Waifu>> waifus)
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
                            list += $"`#{i++}` **{waifu.Name}** - *{BasicUtil.ShortenString(waifu.LongName, 28, 27, "-")}*\n";
                        }
                        eb.AddField(x.Key + "#" + (y.Key+1), list);
                    }
                }
                else
                {
                    string list = "";
                    foreach (var waifu in x)
                    {
                        list += $"`#{i++}` **{waifu.Name}** - *{BasicUtil.ShortenString(waifu.LongName, 28, 27, "-")}*\n";
                    }
                    eb.AddField(x.Key, list);
                }
            }

            eb.WithTitle("Waifus Found :sparkling_heart:");
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
            for(int i = page; i < page+10; i++)
            {
                var x = waifus.ElementAtOrDefault(i);
                if (x.Key != null)
                    waifu += $"#{i+1} {x.Key.Name} - {x.Value}\n";

                var y = users.ElementAtOrDefault(i);
                if (y.Key != null)
                    user += $"#{i+1} {y.Key.Mention} - {y.Value}\n";
            }
            if (waifu == "")
                waifu = "-";
            if (user == "")
                user = "-";

            eb.AddField("Waifus :two_hearts:", waifu, true);
            eb.AddField("Users <:toastie3:454441133876183060>", user, true);

            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter($"Page: {page/10+1}");
            return eb;
        }
        public static int WaifuValue(IEnumerable<Waifu> waifus)
        {
            int total = 0;
            foreach(var x in waifus)
            {
                total += GetPrice(x.Tier);
            }
            return total;
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
                    x.Source = x.Source ?? "";
                    wstr += String.Format("**{0}** - *{1}*\n", x.Name, x.Source);
                }
                eb.AddField("Wishlist", wstr);
                eb.WithImageUrl(waifus.Last().ImageUrl);
            }
            else
            {
                string desc = "Your mind is empty, you have no desires. You do not wish for any waifus.\n\n"
                    + $"*A pillar of light reveals strange texts*\n"
                    + $"```{Program.GetPrefix(user)}waifu\n{Program.GetPrefix(user)}wishwaifu```";
                eb.WithDescription(desc);
            }

            eb.WithColor(UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());
            return eb;
        }

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
                    var msg = await interactive.Context.Channel.SendMessageAsync(embed: FoundWaifusEmbedBuilder(grouped).Build());
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
                    .WithAuthor("Waifus Found", null, BasicUtil._patreon)
                    .WithDescription("*~No results~*")
                    .WithColor(201, 0, 16)
                    .Build());
            }
            
            return null;
        }
    }
}
