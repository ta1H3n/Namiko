using Discord;
using System;
using System.Linq;
using Discord.WebSocket;
using System.Globalization;
using System.Collections.Generic;

namespace Namiko {
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
                case "red":     color = (shade.Equals("dark")) ? Color.DarkRed :new Color(255, 50, 50); break;
                case "blue":    color = (shade.Equals("dark")) ? Color.DarkBlue : new Color(90, 161, 255); break;
                case "purple":  color = (shade.Equals("dark")) ? color = Color.DarkPurple : new Color(123, 104, 238); break;
                case "green":   color = (shade.Equals("dark")) ? color = Color.DarkGreen : new Color(112, 255, 65); break;

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


        // Embeds
        //Embed Method: profile
        public static EmbedBuilder ProfileEmbed(SocketGuildUser user) {
            var eb = new EmbedBuilder();

            string name = user.Username;
            bool toastiePremium = PremiumDb.IsPremium(user.Id, PremiumType.Toastie);
            bool waifuPremium = PremiumDb.IsPremium(user.Id, PremiumType.Waifu);
            if (toastiePremium && waifuPremium)
                name += " | Toastie and Waifu 🌟";
            else if (waifuPremium)
                name += " | Waifu ⭐";
            else if (toastiePremium)
                name += " | Toastie ⭐";
            eb.WithAuthor(name, user.GetAvatarUrl(), BasicUtil._patreon);

            var waifus = UserInventoryDb.GetWaifus(user.Id, user.Guild.Id);
            int waifucount = waifus.Count();
            int waifuprice = WaifuUtil.WaifuValue(waifus);

            var daily = DailyDb.GetDaily(user.Id, user.Guild.Id);
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            eb.AddField("Toasties", $"Amount: {ToastieDb.GetToasties(user.Id, user.Guild.Id).ToString("n0")} <:toastie3:454441133876183060>\nDaily: {(daily == null ? "0" : ((daily.Date + 172800000) < timeNow ? "0" : daily.Streak.ToString()))} :calendar_spiral:", true);
            eb.AddField("Waifus", $"Amount: {waifucount} :two_hearts:\nValue: {waifuprice.ToString("n0")} <:toastie3:454441133876183060>", true);

            var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, user.Guild.Id);
            if (waifu != null)
            {
                eb.WithImageUrl(waifu.ImageUrl);
                eb.AddField("Featured Waifu", $"**{waifu.Name}** - *{waifu.Source}* <:MiaHug:536580304018735135>");
            }

            string footer = "";
            footer += $"Status: '{user.Status.ToString()}'";
            if (user.Activity != null)
                footer += $", Playing: '{user.Activity.Name}'";
            eb.WithFooter(footer);

            //quote 
            string quote = UserDb.GetQuote(user.Id);
            if ( !String.IsNullOrEmpty(quote) & !WebUtil.IsValidUrl(quote)) eb.WithDescription(quote);

            //image
            string image = UserDb.GetImage(user.Id); 
            if ( WebUtil.IsValidUrl(image) ) eb.WithThumbnailUrl(image);


            eb.Color = UserDb.GetHex(out string colour, user.Id)? (Discord.Color) HexToColor(colour) : BasicUtil.RandomColor();
            return eb;
        }
        public static EmbedBuilder ProposalsEmbed(IUser user, SocketGuild guild) {

            //embed basics
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(user);
            embed.WithColor(UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());

            //proposals
            List<Marriage> sent = MarriageDb.GetProposalsSent(user.Id, guild.Id);
            List<Marriage> received = MarriageDb.GetProposalsReceived(user.Id, guild.Id);

            //fields aggregation 
            int users = 1;
            string field1 = "", field2 = "";
            foreach (Marriage proposals in sent)
                field1 += $"#{users++} {guild.GetUser(proposals.WifeId) }\n";

            //
            users = 1;
            foreach (Marriage proposals in received)
                field2 += $"#{users++} {guild.GetUser(proposals.UserId) }\n";

            //if this dude is #ForeverAlone
            if( String.IsNullOrEmpty(field1) && String.IsNullOrEmpty(field2)) 
                embed.WithDescription("You have not sent or Received any Proposals");
            
            //do columns, sent on the left received on the right (or some shit)
            if( !String.IsNullOrEmpty(field1) ) embed.AddField("Sent", field1, true);
            if( !String.IsNullOrEmpty(field2) ) embed.AddField("Received", field2, true);
            return embed;
        }
        public static EmbedBuilder MarriagesEmbed(IUser user, SocketGuild guild) {

            //embed basics
            int numUsers = 1;
            string partners = "";
            List<Marriage> marriages = MarriageDb.GetMarriages(user.Id, guild.Id);
            foreach (Marriage marriage in marriages)
                partners += $"#{numUsers++} {guild.GetUser(marriage.WifeId) }\n";
            
            //embed
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());
            if (!String.IsNullOrEmpty(partners)) embed.AddField("Current Marriages", partners, true);
            else embed.WithDescription("You are currently Unmarried.");
            embed.WithAuthor(user);
            return embed;
        }
        public static EmbedBuilder WaifusEmbed(SocketGuildUser user)
        {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            var waifus = UserInventoryDb.GetWaifus(user.Id, user.Guild.Id).OrderBy(x => x.Source).ThenBy(x => x.Name);

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
                eb.AddField("Waifus", wstr);

                var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, user.Guild.Id);
                if (waifu != null)
                    eb.WithThumbnailUrl(waifu.ImageUrl);
                else
                    eb.WithThumbnailUrl(user.GetAvatarUrl());
            }
            else
            {
                string desc = "You find yourself in a strange place. You are all alone in the darkness. You have no waifus, no love, no purpose.\n\nBut perhaps all is not lost?\n\n"
                    + $"*A pillar of light reveals strange texts*\n"
                    + $"```{Program.GetPrefix(user)}lootbox\n{Program.GetPrefix(user)}daily\n{Program.GetPrefix(user)}weekly\n{Program.GetPrefix(user)}waifushop```";
                eb.WithDescription(desc);
            }

            eb.WithColor(UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)HexToColor(colour) : BasicUtil.RandomColor());
            return eb;
        }
        public static EmbedBuilder PostEmbed(IUser User) {

            //necessary string variables 
            string quote = UserDb.GetQuote(User.Id);
            string image = UserDb.GetImage(User.Id);
            EmbedBuilder embed = new EmbedBuilder();

            //quote embed
            if ( String.IsNullOrEmpty(quote) && !WebUtil.IsValidUrl(image)) return null;
            embed.WithColor(UserDb.GetHex(out string colour, User.Id)? (Discord.Color) HexToColor(colour) : BasicUtil.RandomColor());
            embed.WithAuthor(User);
            embed.WithDescription(quote);
            embed.WithImageUrl(image);
            return embed;
        }
        public static EmbedBuilder StitchedQuoteEmbed(IReadOnlyCollection<SocketUser> users) {

            //creating variables
            int i = 0;
            string footer = "";
            string aggregate_quote = "";
            int aggregateCap = Constants.aggregateCap;

            //building quote and footer
            foreach (SocketUser user in users) {
                string quote = UserDb.GetQuote(user.Id);
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
        public static EmbedBuilder SetColourEmbed(IUser user) {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Profile Colour");
            embed.WithDescription($"{ user.Username } changed primary colour!");
            embed.WithColor(UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)HexToColor(colour) : BasicUtil.RandomColor());
            return embed;
        }
    }
}
