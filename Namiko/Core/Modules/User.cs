using System.Threading.Tasks;
using Namiko.Core.Util;
using Discord;
using System;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Resources.Database;
using Discord.Addons.Interactive;
using System.Collections.Generic;

namespace Namiko.Core.Modules {
    public class User : InteractiveBase<ShardedCommandContext> {

        [Command("Profile"), Summary("Showsa a users profile.\n**Usage**: `!profile [user_optional]`")]
        public async Task Profile(IUser user = null, [Remainder] string str = "")
        {
            if (user == null) user = Context.User;
            await Context.Channel.SendMessageAsync("", false, UserUtil.ProfileEmbed((SocketGuildUser)user).Build());
        }

        [Command("Waifus"), Summary("Shows a users waifu list.\n**Usage**: `!waifus [user_optional]`")]
        public async Task Waifus(IUser user = null, [Remainder] string str = "")
        {
            user = user ?? Context.User;

            var waifus = UserInventoryDb.GetWaifus(user.Id, Context.Guild.Id);

            if (waifus.Count <= 21)
            {
                await Context.Channel.SendMessageAsync("", false, UserUtil.WaifusEmbed((SocketGuildUser)user).Build());
                return;
            }

            var ordwaifus = waifus.OrderBy(x => x.Source).ThenBy(x => x.Name);
            var msg = new CustomPaginatedMessage();

            var author = new EmbedAuthorBuilder()
            {
                IconUrl = user.GetAvatarUrl(),
                Name = user.ToString()
            };
            msg.Author = author;

            msg.Title = "Waifus :revolving_hearts:";
            msg.ThumbnailUrl = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, Context.Guild.Id).ImageUrl;
            msg.Pages = CustomPaginatedMessage.PagesArray(ordwaifus, 15, (x) => String.Format("**{0}** - *{1}*\n", x.Name, x.Source.Length > 33 ? x.Source.Substring(0, 33) + "..." : x.Source), false);

            await PagedReplyAsync(msg);
        }

        [Command("SetColour"), Alias("SetColor", "sc"), Summary("Allows user to set profile colour.\n**Usage**: `!sc [colour_name or hex_value]`")]
        public async Task SetPersonalColour(string shade = "", string colour = "",[Remainder] string str = "") {

             //
            //way to set it back to default
            if ( shade.Equals("") ) {
                await UserDb.HexDefault(Context.User.Id);
                
                //sending embed + exception & error confermations 
                EmbedBuilder embed = UserUtil.SetColourEmbed(Context.User);
                embed.WithDescription($"{ Context.User.Username } set colour to **Default**");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;


            //if no shade specified but colour value exists
            } if ( colour.Equals("") ) colour = shade;



             //
            //setting start values & checking for possible name or hex value
            System.Drawing.Color color;
            if (UserUtil.GetNamedColour(ref color, colour, shade) || UserUtil.GetHexColour(ref color, colour)) {

                //toastie + saving hex colour
                try {
                    await ToastieDb.AddToasties(Context.User.Id, -Cost.colour, Context.Guild.Id);
                    await UserDb.SetHex(color, Context.User.Id);
                    await Context.Channel.SendMessageAsync("", false, UserUtil.SetColourEmbed(Context.User).Build());
                } catch (Exception ex) { await Context.Channel.SendMessageAsync(ex.Message); }
            } 
        }

        [Command("SetQuote"), Alias("sq"), Summary("Sets your quote on profile.\n**Usage**: `!sq [quote]`")]
        public async Task SetPersonalQuote([Remainder] string quote = null) {

            //null me babi
            if(quote == null) {
                await UserDb.SetQuote(Context.User.Id, null);
                await Context.Channel.SendMessageAsync("Quote removed.");
                return;
            }

            //length check
            if (quote.Length > Cost.quoteCap) {
                await Context.Channel.SendMessageAsync($"Quotes have a { Cost.quoteCap } character limit. {quote.Length}/{Cost.quoteCap}");
                return;
            }
            
            //setting quote + getting embed & quote
            await UserDb.SetQuote(Context.User.Id, quote);
            await Context.Channel.SendMessageAsync("Quote set!", false, UserUtil.PostEmbed(Context.User).Build());
        }

        [Command("SetImage"), Alias("si"), Summary("Sets thumbnail Image on profile. \n**Usage**: `!si [image_url_or_attachment]`")]
        public async Task SetPersonalImage([Remainder] string image = null) {

            image = image ?? Context.Message.Attachments.FirstOrDefault()?.Url;

            //to delete image
            if (image == null)
            {
                await UserDb.SetImage(Context.User.Id, null);
                await Context.Channel.SendMessageAsync("Image removed.");
                return;
            }

            //url check
            if (!WebUtil.IsValidUrl(image))
            {
                await Context.Channel.SendMessageAsync("This URL is just like you... Invalid.");
                return;
            }

            //image validity check
            if (!WebUtil.IsImageUrl(image))
            {
                await Context.Channel.SendMessageAsync("This URL is not an image, what do you want me to do with it?");
                return;
            }

            //building embed
            await UserDb.SetImage(Context.User.Id, image);
            EmbedBuilder embed = UserUtil.PostEmbed(Context.User);
            await Context.Channel.SendMessageAsync("Image set!", false, embed.Build());
        }

        [Command("Quote"), Alias("q"), Summary("Allows user to see their personal quote and Image.\n**Usage**: `!q` [user(s)_optional]")]
        public async Task DisplayPersonalQuote(IUser iuser = null, [Remainder] string str = "") {

            //variables
            EmbedBuilder embed;
            bool isMe = iuser == null;
            IUser user = iuser ?? Context.User;

            //
            IReadOnlyCollection<SocketUser> users = Context.Message.MentionedUsers;
            if( users != null && users.Count > 1) {
                embed = UserUtil.StitchedQuoteEmbed(users);
                if (embed == null) await Context.Channel.SendMessageAsync("No one had a proper \"Quote\" qq");
                else await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            //checking quote
            embed = UserUtil.PostEmbed(user);
            if(embed == null){
                await Context.Channel.SendMessageAsync($"{((isMe)? "You don't" : $"{ user.Username } doesn't") } have an image or a quote. Set one with `sq` and `si` commands.");
                return;

            //sending quote
            } await Context.Channel.SendMessageAsync("", false, embed.Build());
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
        public async Task DisplayFeaturedWaifu(IUser iuser = null, [Remainder] string str = "")
        {

            //using variables for names
            bool isMe = iuser == null;
            IUser user = iuser ?? Context.User;
            var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, Context.Guild.Id);

            //checking featured exists
            if (waifu == null) {
                await Context.Channel.SendMessageAsync(((isMe) ? "You Have" : $"{ user.Username } Has") + " No Featured Waifu qq");
                return;

            }
            await Context.Channel.SendMessageAsync(((isMe) ? $"You have { waifu.Name } as your" : $"{ user.Username } has { waifu.Name } as his") + " Featured waifu!", false, WaifuUtil.WaifuEmbedBuilder(waifu, true, Context).Build());
        }

        [Command("SetColourPrior"), Alias("scp"), Summary("Basically `CTRL + Z` for previous profile colours.\n**Usage**: `!scp`")]
        public async Task SetColourPrior([Remainder] string str = "")
        {

            //previous colour command names are pretty dumb
            //reason: named for ease of use i.e 
            //!sc is colour, so !scp & !scpl are easy mental additions
            //apposed to seperate command letters, looks kind of dumb, but simpler for users

            //stack check, dumb as fuck imo
            IUser user = Context.User;
            string stack = UserDb.GetHexStack(user.Id);
            if (stack.Length < 6)
            {
                await Context.Channel.SendMessageAsync("You have no colours in the stack.");
                return;

            //if they do
            } await UserDb.PopStack(user.Id);

            //creating comfermation embed
            EmbedBuilder embed = UserUtil.SetColourEmbed(user);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("ShowColourPriorList"), Alias("scpl"), Summary("Allows user to access previous profile colours.\n**Usage**: `!scpl`")]
        public async Task ColourPriorList([Remainder] string str = "")
        {

            //stack check, dumb as fuck imo
            string stack = UserDb.GetHexStack(Context.User.Id);
            if (stack.Length < 6)
            {
                await Context.Channel.SendMessageAsync("You have no Colour List.");
                return;
            }

            //making message
            string message = "";
            string[] stackItems = stack.Split(";");
            foreach (string item in stackItems) message += item + "\n";

            //getting embed
            EmbedBuilder embed = UserUtil.SetColourEmbed(Context.User);
            embed.WithDescription(message);
            embed.WithAuthor("Previous Colours");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
