using Discord;
using Discord.WebSocket;
using Model;
using Namiko.Modules.Basic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    public static class ServerUtil
    {
        public static async Task<EmbedBuilder> ServerInfo(SocketGuild guild)
        {
            var eb = new EmbedBuilder();

            string name = guild.Name;
            if (PremiumDb.IsPremium(guild.Id, ProType.GuildPlus))
                name += " | T1 Guild 🌟";
            else if (PremiumDb.IsPremium(guild.Id, ProType.Guild))
                name += " | T2 Guild ⭐";
            eb.WithAuthor(name, guild.IconUrl, LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-info"));

            var toasties = (await BalanceDb.GetAllToastiesRaw(guild.Id)).OrderByDescending(x => x.Amount).ToList();
            var waifus = await UserInventoryDb.GetAllWaifuItems(guild.Id);

            string field = "";
            field += $"Total toasties: **{toasties.Sum(x => (long)x.Amount).ToString("n0")}**\n";
            SocketGuildUser user = null;
            int i = 0;
            while (i < toasties.Count && (user == null || user.IsBot))
            {
                user = guild.GetUser(toasties[i++].UserId);
            }
            if(user != null)
                field += $"Richest user: {user.Mention} - **{toasties.FirstOrDefault(x => x.UserId == user.Id).Amount.ToString("n0")}**\n";

            var bank = toasties.FirstOrDefault(x => x.UserId == Program.GetClient().CurrentUser.Id);
            field += $"Bank balance: **{(bank == null ? "0" : bank.Amount.ToString("n0"))}**\n";
            eb.AddField("Toasties <:toastie3:454441133876183060>", field);

            field = "";
            field += $"Total waifus: **{waifus.Count.ToString("n0")}**\n";
            field += $"Total waifu value: **{waifus.Sum(x => Convert.ToInt64(WaifuUtil.GetPrice(x.Waifu.Tier, 0))).ToString("n0")}**\n";
            var groupedwaifus = waifus.GroupBy(x => x.UserId).OrderByDescending(x => x.Count()).ToList();
            IGrouping<ulong, UserInventory> most = null;
            user = null;
            i = 0;
            while (i < groupedwaifus.Count && (user == null || user.IsBot))
            {
                most = groupedwaifus[i];
                user = guild.GetUser(groupedwaifus[i++].Key);
            }
            if (most != null && user != null)
            {
                field += $"Most waifus: {user.Mention} - **{most.Count().ToString("n0")}**\n";
            }

            groupedwaifus = groupedwaifus.OrderByDescending(x => x.Sum(y => WaifuUtil.GetPrice(y.Waifu.Tier, 0))).ToList();
            most = null;
            user = null;
            i = 0;
            while (i < groupedwaifus.Count && (user == null || user.IsBot))
            {
                most = groupedwaifus[i];
                user = guild.GetUser(groupedwaifus[i++].Key);
            }
            if (most != null && user != null)
            {
                field += $"Highest value: {user.Mention} - **{most.Sum(y => WaifuUtil.GetPrice(y.Waifu.Tier, 0)).ToString("n0")}**\n";
            }
            eb.AddField("Waifus :two_hearts:", field);

            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter("To be expanded");
            eb.WithThumbnailUrl(guild.IconUrl);
            return eb;
        }

        public static SocketGuildUser GetGuildUser(SocketGuild guild, ulong[] userIds, int index = 0)
        {
            try
            {
                var user = guild.GetUser(userIds[index]);
                if (user == null || user.IsBot)
                    return GetGuildUser(guild, userIds, index + 1);
                return user;
            }
            catch
            {
                return GetGuildUser(guild, userIds, index + 1);
            }
        }
    }
}
