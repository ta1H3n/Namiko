﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    class UserUtil {
        
        //Method: basic named colours i.e "white"
        public static bool GetNamedColour(ref System.Drawing.Color color, string colour, string shade) {
            
            //checking named colours
            switch (colour) {
                case "gray":
                case "grey":
                    switch (shade) {
                        case "lite":
                        case "light": color = Color.LighterGrey; break;
                        case "dark": color = Color.DarkerGrey; break;
                        default: color = new Color(128, 128, 128);  break;
                    } break; 
                case "orange":
                    switch (shade) {
                        case "lite":
                        case "light": color = Color.LightOrange; break;
                        case "dark": color = Color.DarkOrange; break;
                        default: color = new Color(255, 69, 0); break;
                    } break;
                case "yellow":  
                case "gold":    color = new Color(255, 215, 0); break;
                case "white":   color = new Color(255, 255, 255); break;
                case "pink":    color = new Color(255, 165, 229); break;
                case "black":   color = new Color(0, 0, 0); break;
                case "red":     color = shade.Equals("dark") ? Color.DarkRed :new Color(255, 50, 50); break;
                case "blue":    color = shade.Equals("dark") ? Color.DarkBlue : new Color(90, 161, 255); break;
                case "purple":  color = shade.Equals("dark") ? Color.DarkPurple : new Color(123, 104, 238); break;
                case "green":   color = shade.Equals("dark") ? Color.DarkGreen : new Color(112, 255, 65); break;

                //default stooffz
                default: return false;
            } return true;
        }
        //Methods: Hex Code Operations - *only* utility classes have access to HexToColor
        public static bool GetHexColour(ref System.Drawing.Color color, string colour) {
            if (colour.StartsWith('#')) colour = colour.Remove(0, 1);
            if (System.Text.RegularExpressions.Regex.IsMatch(colour, @"\A\b[0-9a-fA-F]+\b\Z")) {
                color = HexToColor(colour);
                return true;
            } return false;
        }
        public static System.Drawing.Color HexToColor(string colour) {
            return new Color(   int.Parse(colour.Substring(0, 2), NumberStyles.AllowHexSpecifier), 
                                int.Parse(colour.Substring(2, 2), NumberStyles.AllowHexSpecifier),
                                int.Parse(colour.Substring(4, 2), NumberStyles.AllowHexSpecifier));

        }
        public static int GetMarriageLimit(ulong userId)
        {
            int limit = Constants.MarriageLimit;
            if (PremiumDb.IsPremium(userId, ProType.Pro))
                limit = Constants.ProMarriageLimit;
            if (PremiumDb.IsPremium(userId, ProType.ProPlus))
                limit = Constants.ProPlusMarriageLimit;

            return limit;
        }


        // Embeds
        //Embed Method: profile
        public static async Task<EmbedBuilder> ProfileEmbed(SocketGuildUser user)
        {
            var eb = new EmbedBuilder();

            string name = user.Username;

            var role = RoleUtil.GetMemberRole(user.Guild, user) ?? RoleUtil.GetLeaderRole(user.Guild, user);
            if (role != null)
            {
                var team = TeamDb.TeamByMember(role.Id) ?? TeamDb.TeamByLeader(role.Id);
                if (team != null)
                {
                    role = user.Roles.FirstOrDefault(x => x.Id == team.LeaderRoleId);
                    if (role == null)
                        role = user.Roles.FirstOrDefault(x => x.Id == team.MemberRoleId);

                    name += $" | {role.Name}";
                }
            }

            if (PremiumDb.IsPremium(user.Id, ProType.ProPlus))
                name += " | Pro+ 🌟";
            else if (PremiumDb.IsPremium(user.Id, ProType.Pro))
                name += " | Pro ⭐";
            eb.WithAuthor(name, user.GetAvatarUrl(), $"https://namiko.moe/Guild/{user.Guild.Id}/{user.Id}");

            var waifus = UserInventoryDb.GetWaifus(user.Id, user.Guild.Id);
            int waifucount = waifus.Count();
            int waifuprice = WaifuUtil.WaifuValue(waifus);

            var daily = DailyDb.GetDaily(user.Id, user.Guild.Id);
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string text = "";
            text += $"Amount: {BalanceDb.GetToasties(user.Id, user.Guild.Id).ToString("n0")}\n" +
                $"Daily: {(daily == null ? "0" : ((daily.Date + 172800000) < timeNow ? "0" : daily.Streak.ToString()))}\n" +
                $"Boxes Opened: {ProfileDb.GetLootboxOpenedAmount(user.Id)}\n";
            eb.AddField("Toasties <:toastie3:454441133876183060>", text, true);

            text = $"Amount: {waifucount}\n" +
                $"Value: {waifuprice.ToString("n0")}\n";
            foreach(var x in MarriageDb.GetMarriages(user.Id, user.Guild.Id))
            {
                try
                {
                    if (!text.Contains("Married: "))
                        text += "Married: ";
                    text += $"{BasicUtil.IdToMention(GetWifeId(x, user.Id))}\n";
                } catch { }
            }
            eb.AddField("Waifus :two_hearts:", text, true);

            var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, user.Guild.Id);
            if (waifu != null)
            {
                eb.WithImageUrl(waifu.HostImageUrl);
                eb.AddField("Featured Waifu <:MiaHug:536580304018735135>", $"**{waifu.Name}** - *{waifu.Source}*");
            }

            var rep = ProfileDb.GetRepAmount(user.Id);
            string footer = $"Votes: {await VoteDb.VoteCount(user.Id)} • ";
            footer += $"Rep: {rep} • ";
            // Activities require guildpresence
            //footer += $"Status: '{user.Status}'";
            //var activity = user.Activities.FirstOrDefault();
            //if (activity != null)
            //    footer += $", {activity.Type}: '{activity.Name}'";
            eb.WithFooter(footer);

            //quote 
            string quote = ProfileDb.GetQuote(user.Id);
            if (!String.IsNullOrEmpty(quote) & !WebUtil.IsValidUrl(quote))
                eb.WithDescription(quote);

            //image
            string image = ProfileDb.GetImage(user.Id); 
            if (WebUtil.IsValidUrl(image))
                eb.WithThumbnailUrl(image);

            eb.Color = ProfileDb.GetHex(out string colour, user.Id)? (Discord.Color) HexToColor(colour) : BasicUtil.RandomColor();
            return eb;
        }
        public static EmbedBuilder ProposalsEmbed(IUser user, SocketGuild guild)
        {

            //embed basics
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(user);
            embed.WithColor(ProfileDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());

            //proposals
            List<Marriage> sent = MarriageDb.GetProposalsSent(user.Id, guild.Id);
            List<Marriage> received = MarriageDb.GetProposalsReceived(user.Id, guild.Id);

            //fields aggregation 
            int users = 1;
            string field1 = "", field2 = "";
            foreach (Marriage proposals in sent)
                field1 += $"#{users++} {guild.GetUser(proposals.WifeId) }\n";

            //
            foreach (Marriage proposals in received)
                field2 += $"#{users++} {guild.GetUser(proposals.UserId) }\n";

            //if this dude is #ForeverAlone
            if( String.IsNullOrEmpty(field1) && String.IsNullOrEmpty(field2)) 
                embed.WithDescription("You have not sent or received any Proposals.");
            
            //do columns, sent on the left received on the right (or some shit)
            if( !String.IsNullOrEmpty(field1) ) embed.AddField("Proposals Sent :sparkling_heart:", field1, true);
            if( !String.IsNullOrEmpty(field2) ) embed.AddField("Proposals Received :sparkling_heart:", field2, true);
            return embed;
        }
        public static EmbedBuilder MarriagesEmbed(IUser user, SocketGuild guild, BaseSocketClient client)
        {

            //embed basics
            string partners = "";
            List<Marriage> marriages = MarriageDb.GetMarriages(user.Id, guild.Id);
            foreach (Marriage marriage in marriages)
                partners += $"{BasicUtil.IdToMention(GetWifeId(marriage, user.Id))} :revolving_hearts: {marriage.Date.ToString("yyyy/MM/dd")}\n";
            
            //embed
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(ProfileDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());
            if (!String.IsNullOrEmpty(partners))
            {
                embed.AddField("Married To", partners, true);
                try
                {
                    embed.WithImageUrl(client.GetUser(GetWifeId(marriages[0], user.Id)).GetAvatarUrl());
                } catch { }
            }
            else embed.WithDescription("You are not Married.");
            embed.WithAuthor(user);
            return embed;
        }
        public static EmbedBuilder ListMarriageEmbed(List<Marriage> marriages, IUser author)
        {
            var eb = new EmbedBuilderPrepared(author);
            string list = "";
            int i = 1;
            foreach(var x in marriages)
            {
                list += $"`#{i++}` {BasicUtil.IdToMention(GetWifeId(x, author.Id))}\n";
            }
            eb.AddField("User List <:MiaHug:536580304018735135>", list);
            return eb;
        }
        public static EmbedBuilder WaifusEmbed(SocketGuildUser user, string prefix)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            var waifus = UserInventoryDb.GetWaifus(user.Id, user.Guild.Id).OrderBy(x => x.Source).ThenBy(x => x.Name);

            if (waifus.Any())
            {
                string wstr = "";
                foreach (var x in waifus)
                {
                    string row = String.Format("**{0}** - *{1}*", x.Name, x.Source);
                    if (row.Length > 47)
                        row = row.Substring(0, 43) + "...";
                    wstr += row + "\n";
                }
                eb.AddField("Waifus :revolving_hearts:", wstr);

                var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, user.Guild.Id);
                if (waifu != null)
                    eb.WithThumbnailUrl(waifu.HostImageUrl);
                else
                    eb.WithThumbnailUrl(user.GetAvatarUrl());

                eb.WithDescription($"Open in [browser](https://namiko.moe/Guild/{user.Guild.Id}/{user.Id})");
            }
            else
            {
                string desc = "You find yourself in a strange place. You are all alone in the darkness. You have no waifus, no love, no purpose.\n\nBut perhaps all is not lost?\n\n"
                    + $"*A pillar of light reveals strange texts*\n"
                    + $"```{prefix}lootbox\n{prefix}daily\n{prefix}weekly\n{prefix}waifushop```";
                eb.WithDescription(desc);
            }

            eb.WithColor(ProfileDb.GetHex(out string colour, user.Id) ? (Discord.Color)HexToColor(colour) : BasicUtil.RandomColor());
            return eb;
        }
        public static EmbedBuilder QuoteEmbed(IUser User)
        {

            //necessary string variables 
            string quote = ProfileDb.GetQuote(User.Id);
            string image = ProfileDb.GetImage(User.Id);
            EmbedBuilder embed = new EmbedBuilder();

            //quote embed
            if ( String.IsNullOrEmpty(quote) && !WebUtil.IsValidUrl(image)) return null;
            embed.WithColor(ProfileDb.GetHex(out string colour, User.Id)? (Discord.Color) HexToColor(colour) : BasicUtil.RandomColor());
            embed.WithAuthor(User);
            embed.WithDescription(quote);
            embed.WithImageUrl(image);
            return embed;
        }
        public static EmbedBuilder StitchedQuoteEmbed(IReadOnlyCollection<SocketUser> users)
        {

            //creating variables
            int i = 0;
            string footer = "";
            string aggregate_quote = "";
            int aggregateCap = Constants.aggregateCap;

            //building quote and footer
            foreach (SocketUser user in users) {
                string quote = ProfileDb.GetQuote(user.Id);
                if( String.IsNullOrEmpty(quote) )
                    continue;
                
                //if quote exists
                footer += ((i == 0)? "- " : ", " ) + user.Username;
                aggregate_quote += quote + "\n";
                if (++i >= aggregateCap) break;
            }

            //making sure quote exists
            if( i == 0 )
                return null;
            

            //making embed
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithDescription(aggregate_quote);
            embed.WithColor(BasicUtil.RandomColor());
            embed.WithAuthor($"{ (( i == 1 )? "Failed " : "") }Aggregate Quote");
            embed.WithFooter(footer + ((i == 1) ? " had the only \"Quote\"" : ""));
            return embed;
        }
        public static EmbedBuilder SetColourEmbed(IUser user)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Profile Colour");
            embed.WithDescription($"{ user.Username } changed primary colour!");
            embed.WithColor(ProfileDb.GetHex(out string colour, user.Id) ? (Discord.Color)HexToColor(colour) : BasicUtil.RandomColor());
            return embed;
        }


        public static ulong GetWifeId(Marriage marriage, ulong userId)
        {
            return marriage.UserId == userId ? marriage.WifeId : marriage.UserId;
        }
        public async static Task<Marriage> SelectMarriage(List<Marriage> marriages, CustomModuleBase<ICustomContext> module)
        {
            return await module.Select(marriages, "Spouse", embed: ListMarriageEmbed(marriages, module.Context.User).Build());
        }
    }
}
