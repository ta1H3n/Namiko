using Discord;
using System;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using System.Collections.Generic;

namespace Namiko {
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

        [Command("Marry"), Alias("m"), Summary("Allows 2 users to marry each other, no more than 1 partner per person is allowed.\n**Usage**:  `!m [user]`")]
        public async Task Marriage(IUser wife = null, [Remainder] string str = "") {

            //commonly used variables + embed basics
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(UserDb.GetHex(out string colour, Context.User.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());
            ulong guildID = Context.Guild.Id;
            IUser user = Context.User;
            embed.WithAuthor(user);

             // checks
            //making sure u cant do anything weird 
            if ( wife == null || wife == user || wife.IsBot) {
                embed.WithDescription($"You can't propose to { ((wife == null) ? "no one" : (wife.IsBot) ? "bots" : "yourself ") } unfortunately.");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            
            //checking marriage status
            Marriage marriage = MarriageDb.GetMarriageOrProposal(wife.Id, Context.User.Id, Context.Guild.Id);
            if (marriage == null) {
                await MarriageDb.Propose(user.Id, wife.Id, guildID);

                embed.WithAuthor(wife);
                embed.WithDescription($"**{ user.Mention }** has proposed to you.");
                embed.WithFooter("`!accept [user]` or `!decline [user]`");
                await Context.Channel.SendMessageAsync($"{ wife.Mention }", false, embed.Build());
                return;
            }

            //if already married
            if (marriage.IsMarried == true) {
                embed.WithDescription($"You're already married to **{ wife }**");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            //checking marriage cap
            if (MarriageDb.GetMarriages(Context.User.Id, Context.Guild.Id).Count >= Constants.MarriageLimit || MarriageDb.GetMarriages(wife.Id, Context.Guild.Id).Count >= Constants.MarriageLimit) {
                embed.WithDescription($"One of you has already reached the maximum number of marriages");
                embed.WithFooter($"Current max: { Constants.MarriageLimit }");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }


            // Marry em'
            //if the user has already proposed to you
            marriage.IsMarried = true;
            await MarriageDb.UpdateMarriage(marriage);
            embed.WithDescription($"**Congratulations**! you and **{ wife }** are now married");
            await Context.Channel.SendMessageAsync($"{ wife.Mention }", false, embed.Build());
        }

        [Command("Accept"), Alias("AcceptMarriage", "AcceptMarraige", "am"), Summary("Allows you to accept proposal.\n**Usage**: `!am`")]
        public async Task AcceptMarriage(IUser wife, [Remainder] string str = "") {

            //variable thangs + embed basics
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(Context.User);
            embed.WithColor(UserDb.GetHex(out string colour, Context.User.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());
            Marriage marriage = MarriageDb.GetMarriageOrProposal(wife.Id, Context.User.Id, Context.Guild.Id);

            //check wifes existence
            if ( wife == null ) {
                embed.WithDescription("You need to accept someone specific.");
                embed.WithFooter("try: `!accept [user]`");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            //checking marriage status
            if ( marriage == null ) {
                embed.WithDescription($"No proposal has been sent too you from **{ wife }**");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            
            //if you're already married
            if( marriage.IsMarried == true ) {
                embed.WithDescription($"You're already married to **{ wife }**");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            //checking marriage cap
            if ( MarriageDb.GetMarriages(Context.User.Id, Context.Guild.Id).Count >= Constants.MarriageLimit  ||  MarriageDb.GetMarriages(wife.Id, Context.Guild.Id).Count >= Constants.MarriageLimit ) {
                embed.WithDescription($"One of you has already reached the maximum number of marriages");
                embed.WithFooter($"Current max: { Constants.MarriageLimit }");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }


             //
            //accepting marriage 
            marriage.IsMarried = true;
            await MarriageDb.UpdateMarriage(marriage);
            embed.WithDescription($"**Congratulations**! you and **{ wife }** are now married");
            await Context.Channel.SendMessageAsync($"{ wife.Mention }", false, embed.Build());
            return;
        }

        [Command("ShowProposals"), Alias("Proposals"),Summary("Display sent & received proposals.\n**Usage**: `!proposals`")]
        public async Task Proposals(IUser user = null, [Remainder] string str = "") {
            await Context.Channel.SendMessageAsync("", false, UserUtil.ProposalsEmbed(user ?? Context.User, Context.Guild).Build());
        }

        [Command("ShowMarriages"), Alias("Marriages", "Marraiges", "sm"), Summary("Displays current marriages.\n**Usage**: `!sm`")]
        public async Task Marriages(IUser user = null, [Remainder] string str = "") {
            await Context.Channel.SendMessageAsync("", false, UserUtil.MarriagesEmbed(user ?? Context.User, Context.Guild).Build());
        }

        [Command("Divorce"), Alias("Devorce", "d"), Summary("Allows you to divorce your partner.\n**Usage**: `!d`")]
        public async Task Divorce(IUser wife = null, [Remainder] string str = "") {

            //common variables
            IUser user = Context.User;
            EmbedBuilder embed = new EmbedBuilder();
            Discord.Color userColour = UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor();
            embed.WithColor(userColour);
            embed.WithAuthor(user);
            

             // checks
            //making sue u have someone selected
            if (wife == null) {
                embed.WithDescription("You need to divorce someone specific.");
                embed.WithFooter("try: `!divorce [user]`");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            //checking marriage status
            Marriage marriage = MarriageDb.GetMarriageOrProposal(user.Id, wife.Id, Context.Guild.Id);
            if ( marriage == null ) {
                embed.WithDescription($"no proposal has been sent to or from **{ wife }**");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            //making sure ur married
            if( marriage.IsMarried == false) {
                embed.WithDescription($"You're not married to **{ wife }** yet.");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            

             // 
            //creating divorce action
            var divorce = new DialogueBoxOption {
                Action = async (IUserMessage message) => {
                    await MarriageDb.DeleteMarriageOrProposal(user.Id, wife.Id, Context.Guild.Id);
                    embed.WithDescription($"You have divorced **{ wife }**\n*~ May you both find happiness elsewhere ~*");
                    await Context.Channel.SendMessageAsync("", false, embed.Build());

                //execution condition
                }, After = OnExecute.RemoveReactions
            };

            //creating cancel 
            var cancel = new DialogueBoxOption { After = OnExecute.Delete };

            //making dialog embed
            var dia = new DialogueBox();
            dia.Options.Add(Emote.Parse("<:TickYes:577838859107303424>"), divorce);
            dia.Options.Add(Emote.Parse("<:TickNo:577838859077943306>"), cancel);
            dia.Timeout = new TimeSpan(0, 1, 0);
            dia.Embed = new EmbedBuilder()
                .WithAuthor(user)
                .WithColor(userColour)
                .WithDescription($"Are you sure you wish Divorce **{ wife }**?").Build();

            //
            await DialogueReplyAsync(dia);
        }
        
        [Command("Decline"), Alias("DeclineMarriage", "DeclineMarraige", "dm"), Summary("Allows you to divorce your partner.\n**Usage**: `!dm`")]
        public async Task Decline(IUser wife = null, [Remainder] string str = "") {

            //common variables
            IUser user = Context.User;
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(user);
            embed.WithColor(UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());


             // checks
            //proposal check
            if ( wife == null ) {
                embed.WithDescription("Need to decline someone specific.");
                embed.WithFooter("try: `!decline [user]`");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            //checking if a user has proposed - only makes sure at least one has proposed
            Marriage marriage = MarriageDb.GetMarriageOrProposal(wife.Id, user.Id, Context.Guild.Id);
            if( marriage == null ) {
                embed.WithDescription($"no proposal has been sent too or from **{ wife }**");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            //if ur already married to that person
            if ( marriage.IsMarried == true ) {
                embed.WithDescription("Usually, if you're married you don't decline your partner");
                embed.WithFooter("try: `divorce [user]`");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            

             // Swagness
            //creating decline action
            var decline = new DialogueBoxOption {
                Action = async (IUserMessage message) => {
                    await MarriageDb.DeleteMarriageOrProposal(marriage);

                    //embed
                    embed.WithAuthor(wife);
                    embed.WithDescription($"You have been declined; Better luck next time **{ wife }**");
                    await Context.Channel.SendMessageAsync($"{ wife.Mention }", false, embed.Build());

                //execution condition
                }, After = OnExecute.RemoveReactions
            };

            //creating cancel 
            var cancel = new DialogueBoxOption { After = OnExecute.Delete };

            //making dialog embed
            var dia = new DialogueBox();
            dia.Options.Add(Emote.Parse("<:TickYes:577838859107303424>"), decline);
            dia.Options.Add(Emote.Parse("<:TickNo:577838859077943306>"), cancel);
            dia.Timeout = new TimeSpan(0, 1, 0);
            dia.Embed = new EmbedBuilder()
                .WithAuthor(user)
                .WithDescription($"Are you sure you wish Decline **{ wife }**?").Build();

            //
            await DialogueReplyAsync(dia);
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
                    await ToastieDb.AddToasties(Context.User.Id, -Constants.colour, Context.Guild.Id);
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
            if (quote.Length > Constants.quoteCap) {
                await Context.Channel.SendMessageAsync($"Quotes have a { Constants.quoteCap } character limit. {quote.Length}/{Constants.quoteCap}");
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
        public async Task SetFeaturedWaifu([Remainder] string str = "")
        {
            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(WaifuDb.SearchWaifus(str, false, UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id)), this);
            if (waifu == null)
            {
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

        [Command("UndoColor"), Alias("uc"), Summary("Switch back to a previous color.\n**Usage**: `!scp`")]
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

        [Command("ColorHistory"), Alias("clrh"), Summary("List of your previous colors.\n**Usage**: `!scpl`")]
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

        [Command("ActivatePremium"), Alias("ap"), Summary("Activates premium subscriptions associated with this account.\n**Usage**: `!ap`")]
        public async Task ActivatePremium([Remainder] string GuildId = "0")
        {
            var ntr = Context.Client.GetGuild((ulong)PremiumType.GuildId_NOTAPREMIUMTYPE);
            SocketGuildUser user = ntr.GetUser(Context.User.Id);

            if (user == null)
            {
                await Context.Channel.SendMessageAsync("You are not in my server! https://discord.gg/W6Ru5sM");
                return;
            }

            var current = PremiumDb.GetUserPremium(user.Id);
            var roles = user.Roles;

            string text = "";
            foreach(var role in roles)
            {
                if(role.Id == (ulong) PremiumType.Toastie)
                {
                    if (current.Any(x => x.Type == PremiumType.Toastie))
                        text += "You already have **Toastie Premium**!\n";
                    else
                    {
                        await PremiumDb.AddPremium(user.Id, PremiumType.Toastie);
                        text += "**Toastie Premium** activated!\n";
                    }
                }
                if (role.Id == (ulong)PremiumType.Waifu)
                {
                    if (current.Any(x => x.Type == PremiumType.Waifu))
                        text += "You already have **Waifu Premium**!\n";
                    else
                    {
                        await PremiumDb.AddPremium(user.Id, PremiumType.Waifu);
                        text += "**Waifu Premium** activated!\n";
                    }
                }
            }
            if (text == "")
                text += $"You have no user premium... Try `{Program.GetPrefix(Context)}donate`";

            await Context.Channel.SendMessageAsync(text);
        }
    }
}
