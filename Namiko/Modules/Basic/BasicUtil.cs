﻿using Discord;
using Discord.WebSocket;
using Namiko.Modules.Basic;
using System;
using System.Collections.Generic;
using System.Linq;

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
                new Color(90, 161, 255),
                new Color(255, 69, 0),
                new Color(123, 104, 238)
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
        public static List<SocketUser> UserList(DiscordShardedClient client, List<ulong> ids)
        {
            var users = new List<SocketUser>();
            foreach(var x in ids)
            {
                var user = client.GetUser(x);
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
        public static EmbedBuilder InfoEmbed()
        {
            var eb = new EmbedBuilder();
            var client = Program.GetClient();

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
        public static EmbedBuilder DonateEmbed(string prefix)
        {
            var eb = new EmbedBuilder();
            var client = Program.GetClient();

            string desc = "Support the development of Namiko and get rewards!";
            eb.WithDescription(desc);

            string field = "";
            field += $"**Namiko Pro** -  *5$*\n `{prefix}InfoPro`\n";
            field += $"**Namiko Pro+** - *10$*\n `{prefix}InfoProPlus`\n";
            eb.AddField("User Upgrades <:Pro:632544044643516416>", field, true);

            field = "";
            field += $"**Pro Guild** -  *5$*\n `{prefix}InfoGuild`\n";
            field += $"**Pro Guild+** - *10$*\n `{prefix}InfoGuildPlus`\n";
            eb.AddField("Server Wide Upgrades <:Guild:632544044660031498>", field, true);

            field = "";
            field += $":star: [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-pro")}) - includes Pro.\n";
            eb.AddField("Donation Links", field);

            eb.WithAuthor(client.CurrentUser);
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithFooter("-What are you? Twelve?");
            eb.WithImageUrl(AppSettings.NamikoBannerUrl);
            return eb;
        }
        public static EmbedBuilder GuildJoinEmbed(string prefix)
        {
            var eb = new EmbedBuilder();
            var client = Program.GetClient();

            string desc = "";
            desc += $"`{prefix}info` - learn more about me.\n" +
                $"`{prefix}help` - list of my commands.\n" +
                $"Or check out my usage guide [here]({LinkHelper.Guide}) :star: \n\n" +
                $"You can change my prefix by typing `{prefix}sp [prefix]` and replacing [prefix] with your prefix!\n" +
                $"Mentioning me {client.CurrentUser.Mention} can also be used as a prefix!";
            eb.WithDescription(desc);

            eb.WithAuthor(client.CurrentUser);
            eb.WithColor(BasicUtil.RandomColor());
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
