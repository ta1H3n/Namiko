using System.Threading.Tasks;
using Namiko.Core.Util;
using Discord;
using System;
using Discord.Commands;
using Namiko.Resources.Database;
using Discord.WebSocket;
using System.Linq;

namespace Namiko.Core.Modules {
    public class User : ModuleBase<SocketCommandContext> {

        [Command("Inventory"), Alias("waifus", "profile"), Summary("Shows user waifus.\n**Usage**: `!inventory [user_optional]`")]
        public async Task Inventory(IUser user = null, [Remainder] string str = "") {
            if (user == null) user = Context.User;
            await Context.Channel.SendMessageAsync("", false, UserUtil.ProfileEmbed((SocketGuildUser)user).Build());
        }

        [Command("setcolour"), Alias("setcolor", "sc"), Summary("Allows user to set profile colour for 150 toasties.\n**Usage**: `!sc [dark/light optional] [colour name or hex value]`")]
        public async Task CustomColour(string shade, string colour = "",[Remainder] string str = "") {

             //
            //way to set it back to default
            shade = shade.ToLower();
            if (shade.Equals("default")) {
                await UserDb.HexDefault(Context.User.Id);                    
                
                //creating comfermation embed
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor("Profile Colour");
                embed.WithDescription($"{ Context.User.Username } set colour to **Default**\nSwitching to Dafault incurs no additional costs");
                embed.WithColor(BasicUtil.RandomColor());

                //sending embed + exception & error confermations 
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;


            //if no shade specified but colour value exists
            } if (colour.Equals("")) colour = shade;
            else colour = colour.ToLower();



             //
            //setting start values
            System.Drawing.Color color;

            //checking for possible name or hex value, 
            if (colour.StartsWith('#')) colour = colour.Remove(0, 1);
            if (UserUtil.GetNamedColour(ref color, colour, shade) || UserUtil.GetHexColour(ref color, colour)) {

                //toastie + saving hex color try
                try { await ToastieDb.AddToasties(Context.User.Id, -Cost.colour, Context.Guild.Id);
                    await UserDb.SetHex(color, Context.User.Id);

                    //creating comfermation embed
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithAuthor("Profile Colour");
                    embed.WithDescription($"{ Context.User.Username } changed primary colour!");
                    embed.WithColor((Discord.Color)color); 

                    //sending embed + exception & error confermations 
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                } catch (Exception ex) {await Context.Channel.SendMessageAsync(ex.Message); }
            } 
        }

        [Command("setquote"), Alias("sq"), Summary("Sets your personal quote on your profile.\n**Usage**: `!sq [quote]`")]
        public async Task CustomQuote([Remainder] string quote = null) {

            if(quote == null)
            {
                await UserDb.SetQuote(Context.User.Id, null);
                await Context.Channel.SendMessageAsync("Quote removed.");
                return;
            }

            if (quote.Length > 200)
            {
                await Context.Channel.SendMessageAsync("Quotes have a 200 character limit.");
                return;
            }

            //possible image validity check
            bool isPic = WebUtil.IsImageUrl(quote);
            if (WebUtil.IsValidUrl(quote) && !isPic) {
                await Context.Channel.SendMessageAsync("Gomen... I only accept image links (ᗒᗩᗕ)");
                return;
            }

            //setting quote 
            await UserDb.SetQuote(Context.User.Id, quote);

            //getting embed + re-getting quote
            //quote = ;
            EmbedBuilder embed = UserUtil.QuoteEmbed(Context.User, true);
            embed.WithAuthor($"Personal { ((isPic)? "Picture" : "Quote") } Updated");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("quote"), Alias("q"), Summary("Allows user to see their personal quote.\n**Usage**: `!q`")]
        public async Task PersonalQuote(IUser iuser = null, [Remainder] string str = "") {

            //variables
            bool isMe = iuser == null;
            IUser user = iuser ?? Context.User;

            //checking quote
            if(UserDb.GetQuote(user.Id) == null){
                await Context.Channel.SendMessageAsync($"Gomen... { ((isMe)? "You haven't" : $"{ user.Username } hasn't") } added a Quote yet qq ");
                return;
            }

            //sending quote
            EmbedBuilder embed = UserUtil.QuoteEmbed(user, isMe);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("SetFeaturedWaifu"), Alias("sfw"), Summary("Sets your waifu image on your profile.\n**Usage**: `!sfw [waifu_name]`")]
        public async Task SetFeaturedWaifu(string name, [Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(name), name, Context.Channel);
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync($"{name} not found.");
                return;
            }

            if (UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id).Any(x => x.Name.Equals(waifu.Name)))
            {
                await FeaturedWaifuDb.SetFeaturedWaifu(Context.User.Id, waifu, Context.Guild.Id);
                await Context.Channel.SendMessageAsync($"{waifu.Name} set as your featured waifu!", false, UserUtil.ProfileEmbed((SocketGuildUser)Context.User).Build());
                return;
            }
            await Context.Channel.SendMessageAsync($":x: You don't have {waifu.Name}");
        }

        [Command("FeaturedWaifu"), Alias("fw"), Summary("Views Featured Waifu.\n**Usage**: `!fw`")]
        public async Task FeaturedWaifu(IUser iuser = null, [Remainder] string str = "")
        {

            //using variables for names
            bool isMe = iuser == null;
            IUser user = iuser ?? Context.User;
            var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, Context.Guild.Id);

            //checking featured exists
            if (waifu == null)
            {
                await Context.Channel.SendMessageAsync(((isMe) ? "You Have No Featured Waifu qq" : $"{ user.Username } has No Featured waifu"));
                return;

            }
            await Context.Channel.SendMessageAsync(((isMe) ? $"You have { waifu.Name } as your" : $"{ user.Username } has { waifu.Name } as his") + " Featured waifu!", false, WaifuUtil.WaifuEmbedBuilder(waifu, true, Context).Build());
        }
    }
}
