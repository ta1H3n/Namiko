using Discord;
using Discord.WebSocket;
using Namiko.Modules.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    static class BasicUtil
    {
        public readonly static List<Color> colors;
        public readonly static Random rnd;

        static BasicUtil()
        {
            colors = new List<Color>
            {
                new Color(250, 202, 48),
                new Color(255, 165, 229),
                new Color(112, 255, 65),
                new Color(85, 147, 255),
                new Color(130, 79, 250),
                new Color(185, 12, 69)
            };
            rnd = new Random();
        }

        public static Color RandomColor()
        {
            return colors.ElementAt(rnd.Next(colors.Count));
        }
        public static Color GetColor(SocketGuildUser user)
        {
            var roles = user.Guild.Roles;
            for (int i = 0; i < roles.Count; i++)
            {
                if (user.Roles.Contains(roles.ElementAt(i)))
                {
                    Color color = roles.ElementAt(i).Color;
                    if (color.ToString() != "#0")
                        return color;
                }
            }
            return new Color(255, 255, 255);
        }
        public static async Task<List<IUser>> UserListAsync(IDiscordClient client, List<ulong> ids)
        {
            var users = new List<IUser>();
            foreach(var x in ids)
            {
                var user = await client.GetUserAsync(x);
                if (user != null)
                    users.Add(user);
            }
            return users;
        }
        public static string ShortenString(this string str, int max = 33, int cut = 28, string suffix = "...")
        {
            if (str.DefaultIfEmpty() == null)
                return "";

            return str.Length > max ? (str.Substring(0, cut) + suffix) : str;
        }
        public static EmbedBuilder InfoEmbed(BaseSocketClient client)
        {
            var eb = new EmbedBuilder();

            string desc = "Discord bot made in C#, featuring innovations like waifu shops, a banroulette, teams and extensive currency and economy features.";
            eb.WithDescription(desc);

            string field = "Creator: taiHen#2839\n";
            field += $"Support Server: [Namiko Test Realm]({LinkHelper.SupportServerInvite})\n";
            field += $"Usage Guide: [Guide]({LinkHelper.Guide})\n";
            field += $"Origin Server: [AMFWT]({LinkHelper.AmfwtServerInvite})\n";
            field += $"Invite Link: [Namiko]({LinkHelper.GetRedirectUrl(LinkHelper.BotInvite, "BotInvite", "cmd-info")})\n";
            field += $"Repository: [Github]({LinkHelper.Repository})\n";
            field += $"Get Pro: [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info")}) :star:";
            eb.AddField("References", field);

            eb.WithAuthor(client.CurrentUser);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter("-What are you? Twelve?");
            eb.WithImageUrl(AppSettings.NamikoBannerUrl);
            return eb;
        }
        public static EmbedBuilder DonateEmbed(string prefix, BaseSocketClient client)
        {
            var eb = new EmbedBuilder();

            string desc =
                "Support the development of Namiko and get rewards!\n" +
                "\n" +
                $":star: [More info]({LinkHelper.GetRedirectUrl(LinkHelper.Pro, "Pro", "cmd-pro")})";

            eb.WithDescription(desc);
            eb.WithAuthor(client.CurrentUser);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter("-What are you? Twelve?");
            eb.WithImageUrl(AppSettings.NamikoBannerUrl);
            return eb;
        }
        public static EmbedBuilder GuildJoinEmbed(BaseSocketClient client)
        {
            var eb = new EmbedBuilder();

            string desc = "";
            desc += $"`/help` - find my commands.\n\n" +
                $"Or check out my usage guide [here]({LinkHelper.Guide}) :star: \n\n" +
            eb.WithDescription(desc);

            eb.WithAuthor(client.CurrentUser);
            eb.WithColor(RandomColor());
            eb.WithFooter("-What are you? Twelve?");
            eb.WithImageUrl(AppSettings.NamikoBannerUrl);
            return eb;
        }
        public static string IdToMention(this ulong id)
        {
            return $"<@!{id}>";
        }
    }
}
