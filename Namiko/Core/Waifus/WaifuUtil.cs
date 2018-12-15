using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Core.Basic;
using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Namiko.Core.Waifus
{
    public static class WaifuUtil
    {
        public static async Task<List<ShopWaifu>> GetShopWaifus()
        {
            var contents = WaifuShopDb.GetWaifuStores();
            var date = contents[0].GeneratedDate;

            if (date.Date < System.DateTime.Now.Date)
            {
                var list = GenerateWaifuList();
                await WaifuShopDb.NewList(list);
                return list;
            }

            return contents;

        }
        public static List<ShopWaifu> GenerateWaifuList()
        {
            var date = System.DateTime.Now.Date;
            var waifus = new List<ShopWaifu>();
            var rand = new Random();
            int r = 0;

            var tier = WaifuDb.GetWaifusByTier(1);
            tier.AddRange(WaifuDb.GetWaifusByTier(0));
            ShopWaifu item = null;
            for (int i = 0; i < 1; i++)
            {
                r = rand.Next(0, tier.Count);
                item = new ShopWaifu { Waifu = tier.ElementAt(r), GeneratedDate = date, Discount = GenerateDiscount(), Limited = 1, BoughtBy = 0 };
                waifus.Add(item);
                tier.RemoveAt(r);
            }

            for (int i = 0; i < 3; i++)
            {
                r = rand.Next(0, tier.Count);
                item = new ShopWaifu { Waifu = tier.ElementAt(r), GeneratedDate = date, Limited = -1, BoughtBy = 0 };
                waifus.Add(item);
                tier.RemoveAt(r);
            }

            tier = WaifuDb.GetWaifusByTier(2);
            for (int i = 0; i < 4; i++)
            {
                r = rand.Next(0, tier.Count);
                item = new ShopWaifu { Waifu = tier.ElementAt(r), GeneratedDate = date, Limited = -1, BoughtBy = 0 };
                waifus.Add(item);
                tier.RemoveAt(r);
            }

            tier = WaifuDb.GetWaifusByTier(3);
            for (int i = 0; i < 5; i++)
            {
                r = rand.Next(0, tier.Count);
                item = new ShopWaifu { Waifu = tier.ElementAt(r), GeneratedDate = date, Limited = -1, BoughtBy = 0 };
                waifus.Add(item);
                tier.RemoveAt(r);
            }

            return waifus;
        }
        public static EmbedBuilder GetShopEmbed(List<ShopWaifu> waifus, SocketCommandContext Context)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor("Waifu Store", Context.Client.CurrentUser.GetAvatarUrl());
            bool split = false;
            foreach (var x in waifus)
            {
                var efb = new EmbedFieldBuilder();
                efb.IsInline = true;
                efb.Name = x.Waifu.Name;
                efb.Value = $"TIER: **{x.Waifu.Tier}**\nPrice: {GetPriceString(x.Waifu.Tier, x.Discount)}";

                if (x.Limited > -1)
                {
                    efb.IsInline = false;
                    efb.Value += "\n" + "**Limited!**";
                    if (x.Limited > 0)
                        efb.Value += $" {x.Limited} in stock!";
                    else
                        efb.Value += $" OUT OF STOCK! Bought by: {Context.Client.GetUser(x.BoughtBy).Mention}";
                }

                eb.AddField(efb);
            }

            ///////////////////////////
            //INSERT THUMBNAIL
            ///////////////////////////
            eb.WithFooter($"{StaticSettings.prefix}buywaifu [name]");
            eb.Color = BasicUtil.RandomColor();
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
                return GetPrice(tier).ToString();

            else
                return $"~~{GetPrice(tier)}~~ {GetPrice(tier, discount)} (-{discount}%)";
        }
        public static int GetPrice(int tier, int discount = 0)
        {
            int price = tier == 1 ? 20000 :
                tier == 2 ? 10000 :
                tier == 3 ? 5000 :
                tier == 0 ? 100000 : 0;

            if (discount >= 0)
                price = price - (price / 100 * discount);

            return price;
        }
        public static EmbedBuilder WaifuEmbedBuilder(Waifu waifu, bool footerDetails = false, SocketCommandContext context = null)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithAuthor(waifu.Name);

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

            if (footerDetails)
            {
                string footer = ($"Tier: {waifu.Tier}");
                footer += WaifuOwnerMentionsString(waifu, context);
                eb.WithFooter(footer);
            }
            

            eb.Color = BasicUtil.RandomColor();
            return eb;
        }
        public static string WaifuOwnerMentionsString(Waifu waifu, SocketCommandContext context)
        {
            var userIds = UserInventoryDb.GetOwners(waifu);
            string str = null;
            if (userIds.Count > 0)
            {
                str = ", Owned by: ";
                foreach (ulong id in userIds)
                {
                    try
                    {
                        var mention = context.Client.GetUser(id).Username;
                        str += $"`{mention}` ";
                    }
                    catch(Exception ex) { }
                }
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
        public static EmbedBuilder UserInventoryEmbed(IUser user)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithThumbnailUrl(user.GetAvatarUrl());

            var waifus = UserInventoryDb.GetWaifus(user.Id);
            waifus = waifus.OrderBy(x => x.Name).ToList();
            int waifuprice = WaifuValue(waifus);

            // string desc = $"**Toasties**: {ToastieDb.GetToasties(user.Id)}\n";
            // desc += $"**Waifu worth**: {waifuprice}";
            // eb.WithDescription(desc);

            var daily = DailyDb.GetDaily(user.Id);
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            eb.AddField("Toasties", $"Amount: {ToastieDb.GetToasties(user.Id)} <:toastie3:454441133876183060>\nDaily: {(daily == null ? "0" : ((daily.Date + 172800000) < timeNow ? "0" : daily.Streak.ToString()))} :calendar_spiral:", true);
            eb.AddField("Waifus", $"Amount: {waifus.Count} :two_hearts:\nValue: {waifuprice} <:toastie3:454441133876183060>", true);

            if (waifus.Count > 0)
            {
                string wstr = "";
                // wstr += $"Total: {waifus.Count} :two_hearts:\n";
                // wstr += $"Value: {waifuprice} <:toastie3:454441133876183060>";
                foreach (var x in waifus)
                {
                    if (x.LongName.Length > 35)
                        x.LongName = x.LongName.Substring(0, 33) + "...";
                    x.LongName = x.LongName == null ? "" : x.LongName;
                    wstr += String.Format("**{0}** - *{1}*\n", x.Name, x.LongName);
                }
                eb.AddField("Waifus", wstr);
            }

            var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id);
            if(waifu != null)
                eb.WithThumbnailUrl(waifu.ImageUrl);
            
            string footer = "";
            footer += $"Status: '{user.Status.ToString()}'";
            if (user.Game.HasValue)
                footer += $", Playing: '{user.Game.Value.Name}'";
            eb.WithFooter(footer);

            eb.Color = BasicUtil.RandomColor();
            return eb;
        }
        public static string FoundWaifusCodeBlock(List<Waifu> waifus)
        {
            string text = "```WAIFUS FOUND:\n\n";
            foreach (var x in waifus)
            {
                text += String.Format("{0, 10} - {1}\n", x.Name, x.LongName);
            }
            text += "```";
            return text;
        }
        public static async Task<Waifu> ProcessWaifuListAndRespond(List<Waifu> waifus, string inputName, ISocketMessageChannel channel = null)
        {
          //  if (waifus.Count == 1)
          //  {
          //      if (waifus[0].Name.Equals(inputName, StringComparison.InvariantCultureIgnoreCase))
          //          return waifus[0];
          //  }

            if (waifus.Count == 1)
                return waifus[0];

            if (waifus.Count > 0 && channel != null)
            {
                await channel.SendMessageAsync(WaifuUtil.FoundWaifusCodeBlock(waifus));
            }

            return null;
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
    }
}
