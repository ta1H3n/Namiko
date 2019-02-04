using Discord;
using System;
using System.Linq;
using System.Globalization;
using Namiko.Resources.Database;
using Discord.WebSocket;
using Namiko.Resources.Datatypes;
using System.Collections.Generic;

namespace Namiko.Core.Util
{
    public static class ServerUtil
    {
        public static EmbedBuilder ServerInfo(SocketGuild guild)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(guild.Name, guild.IconUrl);
            var toasties = ToastieDb.GetAllToasties(guild.Id).OrderByDescending(x => x.Amount);
            var waifus = UserInventoryDb.GetAllWaifuItems(guild.Id);

            string field = "";
            field += $"Total toasties: **{toasties.Sum(x => x.Amount).ToString("n0")}**\n";
            var user = GetGuildUser(guild, toasties.Select(x => x.UserId).ToArray());
            field += $"Richest user: {user.Mention} - **{toasties.FirstOrDefault(x => x.UserId == user.Id).Amount.ToString("n0")}**\n";
            field += $"Bank balance: **{toasties.FirstOrDefault(x => x.UserId == Program.GetClient().CurrentUser.Id).Amount.ToString("n0")}**\n";
            eb.AddField("Toasties <:toastie3:454441133876183060>", field);

            field = "";
            field += $"Total waifus: **{waifus.Count}**\n";
            field += $"Total waifu value: **{waifus.Sum(x => Convert.ToInt64(Namiko.Core.Util.WaifuUtil.GetPrice(x.Waifu.Tier, 0))).ToString("n0")}**\n";
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
                    throw new Exception();
                return user;
            }
            catch
            {
                return GetGuildUser(guild, userIds, index + 1);
            }
        }
    }
}
