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

        [Command("Marry"), Alias("Propose"), Summary("Propose to a user.\n**Usage**:  `!m [user]`")]
        public async Task Marriage(IUser wife = null, [Remainder] string str = "") {

            //commonly used variables + embed basics
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(UserDb.GetHex(out string colour, Context.User.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());
            ulong guildID = Context.Guild.Id;
            IUser user = Context.User;
            eb.WithAuthor(user);

             // checks
            //making sure u cant do anything weird 
            if ( wife == null || wife == user || wife.IsBot) {
                eb.WithDescription($"You can't propose to { ((wife == null) ? "no one" : (wife.IsBot) ? "bots" : "yourself ") } unfortunately.");
                await Context.Channel.SendMessageAsync("", false, eb.Build());
                return;
            }
            
            if (MarriageDb.GetProposalsSent(Context.User.Id, Context.Guild.Id).Any(x => x.WifeId == wife.Id))
            {
                eb.WithAuthor(user);
                eb.WithDescription($"You have already proposed to **{wife}**.");
                await Context.Channel.SendMessageAsync($"", false, eb.Build());
                return;
            }

            if (MarriageDb.GetMarriages(user.Id, Context.Guild.Id).Any(x => x.WifeId == wife.Id || x.UserId == wife.Id))
            {
                eb.WithDescription($"You're already married to **{wife}**.");
                await Context.Channel.SendMessageAsync("", false, eb.Build());
                return;
            }

            //checking marriage status
            Marriage proposal = MarriageDb.GetMarriageOrProposal(wife.Id, user.Id, Context.Guild.Id);
            if (proposal == null) {
                await MarriageDb.Propose(user.Id, wife.Id, guildID);

                eb.WithAuthor(wife);
                eb.WithDescription($"**{ user.Mention }** has proposed to you.");
                eb.WithFooter($"`{this.Prefix()}marry [user]` or `{this.Prefix()}decline [user]`");
                await Context.Channel.SendMessageAsync($"", false, eb.Build());
                return;
            }

            //if already married
            
            //checking marriage cap
            if (MarriageDb.GetMarriages(Context.User.Id, Context.Guild.Id).Count >= UserUtil.GetMarriageLimit(Context.User.Id) 
                || MarriageDb.GetMarriages(wife.Id, Context.Guild.Id).Count >= UserUtil.GetMarriageLimit(wife.Id)) {
                eb.WithDescription($"One of you has reached the maximum number of marriages.");
                eb.WithFooter($"Limit can be increased to {Constants.PremiumMarriageLimit} with Waifu Premium.");
                await Context.Channel.SendMessageAsync("", false, eb.Build());
                return;
            }

            // Marry em'
            //if the user has already proposed to you
            proposal.IsMarried = true;
            proposal.Date = System.DateTime.Now;
            await MarriageDb.UpdateMarriage(proposal);
            eb.WithDescription($"**Congratulations**! You and **{ wife }** are now married!");
            await Context.Channel.SendMessageAsync($"", false, eb.Build());
        }

        [Command("Decline"), Alias("DeclineMarriage", "dm"), Summary("Decline marriage proposal.\n**Usage**: `!decline`")]
        public async Task Decline([Remainder] string str = "")
        {

            //common variables
            IUser user = Context.User;
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithColor(UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());

            var proposals = MarriageDb.GetProposalsReceived(user.Id, Context.Guild.Id);
            proposals.AddRange(MarriageDb.GetProposalsSent(user.Id, Context.Guild.Id));
            var proposal = await UserUtil.SelectMarriage(proposals, this);

            if(proposal == null)
            {
                eb.WithDescription("~ You have no proposals ~");
                await Context.Channel.SendMessageAsync($"", false, eb.Build());
                return;
            }

            ulong wife = UserUtil.GetWifeId(proposal, user.Id);
            // Swagness
            //creating decline action
            var decline = new DialogueBoxOption
            {
                Action = async (IUserMessage message) => {
                    await MarriageDb.DeleteMarriageOrProposal(proposal);

                    //embed
                    eb.WithAuthor(user);
                    eb.WithDescription($"You declined the proposal.\nBetter luck next time **{ BasicUtil.IdToMention(wife) }**.");
                    await message.ModifyAsync(x => x.Embed = eb.Build());

                    //execution condition
                },
                After = OnExecute.RemoveReactions
            };

            //creating cancel 
            var cancel = new DialogueBoxOption { After = OnExecute.Delete };

            //making dialog embed
            var dia = new DialogueBox();
            dia.Options.Add(Emote.Parse("<:TickYes:577838859107303424>"), decline);
            dia.Options.Add(Emote.Parse("<:TickNo:577838859077943306>"), cancel);
            dia.Timeout = new TimeSpan(0, 1, 0);
            dia.Embed = new EmbedBuilderPrepared(user)
                .WithDescription($"Are you sure you wish to Decline **{ BasicUtil.IdToMention(wife) }**?").Build();

            //
            await DialogueReplyAsync(dia);
        }

        [Command("Divorce"), Summary("Divorce a user.\n**Usage**: `!divorce`")]
        public async Task Divorce([Remainder] string str = "") {

            //common variables
            IUser user = Context.User;
            EmbedBuilder eb = new EmbedBuilder();
            Discord.Color userColour = UserDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor();
            eb.WithColor(userColour);
            eb.WithAuthor(user);

            var marriages = MarriageDb.GetMarriages(user.Id, Context.Guild.Id);
            var marriage = await UserUtil.SelectMarriage(marriages, this);

            if (marriage == null)
            {
                eb.WithDescription("~ You are not married ~");
                await Context.Channel.SendMessageAsync($"", false, eb.Build());
                return;
            }

            ulong wife = UserUtil.GetWifeId(marriage, user.Id);
             // 
            //creating divorce action
            var divorce = new DialogueBoxOption {
                Action = async (IUserMessage message) => {
                    await MarriageDb.DeleteMarriageOrProposal(marriage);
                    await message.ModifyAsync((x) => x.Embed = eb.WithDescription($"You divorced **{ BasicUtil.IdToMention(wife) }**.\n*~ May you both find happiness elsewhere ~*").Build());

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
            dia.Embed = new EmbedBuilderPrepared(user)
                .WithColor(userColour)
                .WithDescription($"Are you sure you wish to Divorce **{ BasicUtil.IdToMention(wife) }**?").Build();

            //
            await DialogueReplyAsync(dia);
        }
        
        [Command("Proposals"), Alias("ShowProposals", "Proposal"), Summary("Displays sent & received proposals.\n**Usage**: `!proposals`")]
        public async Task Proposals(IUser user = null, [Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync("", false, UserUtil.ProposalsEmbed(user ?? Context.User, Context.Guild).Build());
        }

        [Command("Marriages"), Alias("ShowMarriages", "Marraiges", "Marriage", "sm"), Summary("Displays marriages.\n**Usage**: `!sm`")]
        public async Task Marriages(IUser user = null, [Remainder] string str = "")
        {
            await Context.Channel.SendMessageAsync("", false, UserUtil.MarriagesEmbed(user ?? Context.User, Context.Guild).Build());
        }

        [Command("SetColour"), Alias("SetColor", "sc"), Summary("Set your profile colour.\n**Usage**: `!sc [colour_name or hex_value]`")]
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
            await Context.Channel.SendMessageAsync("Quote set!", false, UserUtil.QuoteEmbed(Context.User).Build());
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
            EmbedBuilder embed = UserUtil.QuoteEmbed(Context.User);
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
            embed = UserUtil.QuoteEmbed(user);
            if(embed == null){
                await Context.Channel.SendMessageAsync($"{((isMe)? "You don't" : $"{ user.Username } doesn't") } have an image or a quote. Set one with `sq` and `si` commands.");
                return;

            //sending quote
            } await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("SetFeaturedWaifu"), Alias("sfw"), Summary("Sets your waifu image on your profile.\n**Usage**: `!sfw [waifu_name]`")]
        public async Task SetFeaturedWaifu([Remainder] string str = "")
        {
            if(str == "")
            {
                await FeaturedWaifuDb.Delete(Context.User.Id, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("Removed your featured waifu. Now your last bought will appear! The betrayal...");
                return;
            }

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

        [Command("UndoColour"), Alias("uc", "UndoColor"), Summary("Switch back to a previous color.\n**Usage**: `!scp`")]
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

        [Command("ColourHistory"), Alias("clrh", "ColorHistory"), Summary("List of your previous colors.\n**Usage**: `!scpl`")]
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
            embed.WithAuthor("Colour History");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Avatar"), Alias("pfp"), Summary("View a users profile picture.\n**Usage**: `!pfp [user]`")]
        public async Task Avatar(IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }
            string avatar = user.GetAvatarUrl(ImageFormat.Auto, 2048);
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(user)
                .WithAuthor(user.Username + "'s Profile Picture", avatar, avatar)
                .WithImageUrl(avatar)
                .Build());
        }

        [Command("Rep"), Summary("Gives rep to a user.\n**Usage**: `!rep [user]`")]
        public async Task Rep(IUser user)
        {
            if(user == Context.User)
            {
                await Context.Channel.SendMessageAsync("Nice try, Mr. Perfect.");
                return;
            }

            var author = await UserDb.GetProfile(Context.User.Id);

            var cooldown = author.RepDate.AddHours(20);
            var now = System.DateTime.Now;
            if (cooldown > now)
            {
                var span = cooldown.Subtract(now);
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription("You already repped someone today. If everyone is cool then no one is cool.\n" +
                    $"You must wait `{span.Hours} hours {span.Minutes} minutes {span.Seconds} seconds`")
                    .WithColor(Color.DarkRed)
                    .Build());
                return;
            }

            author.RepDate = now;
            int rep = await UserDb.IncrementRep(user.Id);
            await UserDb.UpdateProfile(author);

            await Context.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                .WithDescription($"You repped {user.Mention}\nNow they have **{rep}** rep!")
                .WithThumbnailUrl("https://i.imgur.com/xxobSIH.png")
                .Build());

            var bot = Program.GetClient().CurrentUser;
            if (user.Id == bot.Id)
            {
                await ToastieDb.AddToasties(Context.User.Id, 50, Context.Guild.Id);
                await Context.Channel.SendMessageAsync($"Thank you {Context.User.Mention}! <a:loveme:536705504798441483>", embed: ToastieUtil.GiveEmbed(bot, Context.User, 50).Build());
            }
        }

        [Command("ActivatePremium"), Alias("ap"), Summary("Activates premium subscriptions associated with this account.\n**Usage**: `!ap`")]
        public async Task ActivatePremium([Remainder] string GuildId = "0")
        {
            var ntr = Context.Client.GetGuild((ulong)PremiumType.HomeGuildId_NOTAPREMIUMTYPE);
            SocketGuildUser user = ntr.GetUser(Context.User.Id);

            if (user == null)
            {
                await Context.Channel.SendMessageAsync("You are not in my server! https://discord.gg/W6Ru5sM");
                return;
            }

            var current = PremiumDb.GetUserPremium(user.Id);
            var roles = user.Roles;

            bool log = false;
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
                        log = true;
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
                        log = true;
                    }
                }
            }
            if (text == "")
                text += $"You have no user premium... Try `{Program.GetPrefix(Context)}donate`";

            await Context.Channel.SendMessageAsync(text);
            if(log)
            {
                await ((SocketTextChannel)Program.GetClient().GetChannel(StaticSettings._premiumLogChannel)).SendMessageAsync(embed: new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"{Context.User.Mention} `{Context.User.Id}`\n{text}")
                    .WithFooter(System.DateTime.Now.ToLongDateString())
                    .Build());
            }
        }
    }
}
