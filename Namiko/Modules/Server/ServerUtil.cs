﻿using Discord;
using System;
using System.Linq;
using System.Globalization;

using Discord.WebSocket;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Namiko
{
    public static class ServerUtil
    {
        public static async Task<EmbedBuilder> ServerInfo(SocketGuild guild)
        {
            var eb = new EmbedBuilder();

            string name = guild.Name;
            if (PremiumDb.IsPremium(guild.Id, PremiumType.ServerT1))
                name += " | T1 Guild 🌟";
            else if (PremiumDb.IsPremium(guild.Id, PremiumType.ServerT2))
                name += " | T2 Guild ⭐";
            eb.WithAuthor(name, guild.IconUrl, BasicUtil._patreon);

            var toasties = (await ToastieDb.GetAllToastiesRaw(guild.Id)).OrderByDescending(x => x.Amount);
            var waifus = await UserInventoryDb.GetAllWaifuItems(guild.Id);

            string field = "";
            field += $"Total toasties: **{toasties.Sum(x => x.Amount).ToString("n0")}**\n";
            SocketGuildUser user = guild.GetUser(toasties.FirstOrDefault(x => x.UserId != Program.GetClient().CurrentUser.Id).UserId);
            if(user != null)
                field += $"Richest user: {user.Mention} - **{toasties.FirstOrDefault(x => x.UserId == user.Id).Amount.ToString("n0")}**\n";
            var bank = toasties.FirstOrDefault(x => x.UserId == Program.GetClient().CurrentUser.Id);
            field += $"Bank balance: **{(bank == null ? "0" : bank.Amount.ToString("n0"))}**\n";
            eb.AddField("Toasties <:toastie3:454441133876183060>", field);

            field = "";
            field += $"Total waifus: **{waifus.Count.ToString("n0")}**\n";
            field += $"Total waifu value: **{waifus.Sum(x => Convert.ToInt64(WaifuUtil.GetPrice(x.Waifu.Tier, 0))).ToString("n0")}**\n";
            var groupedwaifus = waifus.GroupBy(x => x.UserId).OrderByDescending(x => x.Count());
            var most = groupedwaifus.FirstOrDefault();
            user = guild.GetUser(most.Key);
            if (most != null && user != null)
            {
                field += $"Most waifus: {user.Mention} - **{most.Count().ToString("n0")}**\n";
            }
            most = groupedwaifus.OrderByDescending(x => x.Sum(y => WaifuUtil.GetPrice(y.Waifu.Tier, 0))).FirstOrDefault();
            user = guild.GetUser(most.Key);
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
