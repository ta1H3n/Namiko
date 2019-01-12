using Discord;
using System;
using System.Linq;
using Namiko.Resources.Database;
using System.Globalization;
namespace Namiko.Core.Util {
    class UserUtil {

         //
        //Method: named colours i.e "white"
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
        
        //Methods: Hex Code Operations
        public static bool GetHexColour(ref System.Drawing.Color color, string colour) {
            if(System.Text.RegularExpressions.Regex.IsMatch(colour, @"\A\b[0-9a-fA-F]+\b\Z")) {
                color = HexToColor(colour);
                return true;
            } return false;
        }
        private static System.Drawing.Color HexToColor(string colour) {
            return new Color(   int.Parse(colour.Substring(0, 2), NumberStyles.AllowHexSpecifier), 
                                int.Parse(colour.Substring(2, 2), NumberStyles.AllowHexSpecifier),
                                int.Parse(colour.Substring(4, 2), NumberStyles.AllowHexSpecifier));

        }


         // Embedz
        //Embed Method: profile
        public static EmbedBuilder ProfileEmbed(IUser user) {
            var eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithThumbnailUrl(user.GetAvatarUrl());

            var waifus = UserInventoryDb.GetWaifus(user.Id).OrderBy(x => x.Source).ThenBy(x => x.Name);
            int waifucount = waifus.Count();
            int waifuprice = WaifuUtil.WaifuValue(waifus);

            // string desc = $"**Toasties**: {ToastieDb.GetToasties(user.Id)}\n";
            // desc += $"**Waifu worth**: {waifuprice}";
            // eb.WithDescription(desc);

            var daily = DailyDb.GetDaily(user.Id);
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            eb.AddField("Toasties", $"Amount: {ToastieDb.GetToasties(user.Id)} <:toastie3:454441133876183060>\nDaily: {(daily == null ? "0" : ((daily.Date + 172800000) < timeNow ? "0" : daily.Streak.ToString()))} :calendar_spiral:", true);
            eb.AddField("Waifus", $"Amount: {waifucount} :two_hearts:\nValue: {waifuprice} <:toastie3:454441133876183060>", true);

            if (waifucount > 0) {
                string wstr = "";
                // wstr += $"Total: {waifus.Count} :two_hearts:\n";
                // wstr += $"Value: {waifuprice} <:toastie3:454441133876183060>";
                foreach (var x in waifus) {
                    if (x.LongName.Length > 35)
                        x.LongName = x.LongName.Substring(0, 33) + "...";
                    x.LongName = x.LongName ?? "";
                    wstr += String.Format("**{0}** - *{1}*\n", x.Name, x.LongName);
                }
                eb.AddField("Waifus", wstr);
            }

            var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id);
            if (waifu != null)
                eb.WithThumbnailUrl(waifu.ImageUrl);

            string footer = "";
            footer += $"Status: '{user.Status.ToString()}'";
            if (user.Activity != null)
                footer += $", Playing: '{user.Activity.Name}'";
            eb.WithFooter(footer);



            eb.Color = UserDb.CheckHex(out string colour, user.Id)? (Discord.Color) HexToColor(colour) : BasicUtil.RandomColor();
            return eb;
        }
        public static EmbedBuilder QuoteEmbed(ulong UserId, string quote) {

            //creating comfermation embed
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Personal Quote");
            embed.WithDescription(quote);
            embed.WithColor(UserDb.CheckHex(out string colour, UserId)? (Discord.Color) HexToColor(colour) : BasicUtil.RandomColor());
            return embed;
        }
    }
}
