using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Model;
using Model.Models.Users;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Namiko.Modules.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    [RequireGuild]
    [Name("Users & Profiles")]
    public class User : CustomModuleBase<ICustomContext>
    {
        public BaseSocketClient Client { get; set; }
        

        [Command("Profile"), Description("Shows a users profile.\n**Usage**: `!profile [user_optional]`")]
        [SlashCommand("profile", "Show user profile")]
        public async Task Profile([Remainder] IUser user = null)
        {
            if (user == null) user = Context.User;
            await ReplyAsync("", false, (await UserUtil.ProfileEmbed((SocketGuildUser)user)).Build());
        }

        [Command("Waifus"), Alias("inv"), Description("Shows a users waifu list.\n**Usage**: `!waifus [user_optional]`")]
        [SlashCommand("waifus", "Show waifu inventory")]
        public async Task Waifus([Remainder] IUser user = null)
        {
            user ??= Context.User;

            var waifus = UserInventoryDb.GetWaifus(user.Id, Context.Guild.Id);

            if (waifus.Count <= 21)
            {
                await ReplyAsync("", false, UserUtil.WaifusEmbed((SocketGuildUser)user, GetPrefix()).Build());
                return;
            }

            var ordwaifus = waifus.OrderBy(x => x.Source).ThenBy(x => x.Name);
            var msg = new PaginatedMessage();

            var author = new EmbedAuthorBuilder()
            {
                IconUrl = user.GetAvatarUrl(),
                Name = user.ToString()
            };
            msg.Author = author;

            msg.ThumbnailUrl = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, Context.Guild.Id).HostImageUrl;
            var pages = PaginatedMessage.PagesArray(ordwaifus, 15, (x) => String.Format("**{0}** - *{1}*\n", x.Name, x.Source.Length > 33 ? x.Source.Substring(0, 33) + "..." : x.Source), false);
            msg.Fields = new List<FieldPages> { new FieldPages { Title = "Waifus :revolving_hearts:", Pages = pages } };
            msg.Pages = new List<string> { $"Open in [browser](https://namiko.moe/Guild/{Context.Guild.Id}/{user.Id})" };

            await PagedReplyAsync(msg);
        }

        [Command("Marry"), Alias("Propose"), Description("Propose to a user.\n**Usage**:  `!m [user]`")]
        [SlashCommand("marry", "Propose/marry a user")]
        public async Task Marriage(IUser wife = null)
        {

            //commonly used variables + embed basics
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(ProfileDb.GetHex(out string colour, Context.User.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());
            ulong guildID = Context.Guild.Id;
            IUser user = Context.User;
            eb.WithAuthor(user);

            // checks
            //making sure u cant do anything weird 
            if (wife == null || wife == user || wife.IsBot)
            {
                eb.WithDescription($"You can't propose to { ((wife == null) ? "no one" : wife.IsBot ? "bots" : "yourself ") } unfortunately.");
                await ReplyAsync("", false, eb.Build());
                return;
            }

            if (MarriageDb.GetProposalsSent(Context.User.Id, Context.Guild.Id).Any(x => x.WifeId == wife.Id))
            {
                eb.WithAuthor(user);
                eb.WithDescription($"You have already proposed to **{wife}**.");
                await ReplyAsync($"", false, eb.Build());
                return;
            }

            if (MarriageDb.GetMarriages(user.Id, Context.Guild.Id).Any(x => x.WifeId == wife.Id || x.UserId == wife.Id))
            {
                eb.WithDescription($"You're already married to **{wife}**.");
                await ReplyAsync("", false, eb.Build());
                return;
            }

            //checking marriage status
            Marriage proposal = MarriageDb.GetMarriageOrProposal(wife.Id, user.Id, Context.Guild.Id);
            if (proposal == null)
            {
                await MarriageDb.Propose(user.Id, wife.Id, guildID);

                eb.WithAuthor(wife);
                eb.WithDescription($"**{ user.Mention }** has proposed to you.");
                eb.WithFooter($"`{GetPrefix()}marry [user]` or `{GetPrefix()}decline [user]`");
                await ReplyAsync($"", false, eb.Build());
                return;
            }

            //if already married

            //checking marriage cap
            if (MarriageDb.GetMarriages(Context.User.Id, Context.Guild.Id).Count >= UserUtil.GetMarriageLimit(Context.User.Id)
                || MarriageDb.GetMarriages(wife.Id, Context.Guild.Id).Count >= UserUtil.GetMarriageLimit(wife.Id))
            {
                eb.WithDescription($"One of you has reached the maximum number of marriages.");
                eb.WithFooter($"Limit can be increased to {Constants.ProMarriageLimit} or {Constants.ProPlusMarriageLimit} with Namiko Pro.");
                await ReplyAsync("", false, eb.Build());
                return;
            }

            // Marry em'
            //if the user has already proposed to you
            proposal.IsMarried = true;
            proposal.Date = System.DateTime.Now;
            await MarriageDb.UpdateMarriage(proposal);
            eb.WithDescription($"**Congratulations**! You and **{ wife }** are now married!");
            await ReplyAsync($"", false, eb.Build());
        }

        [Command("Decline"), Alias("DeclineMarriage", "dm"), Description("Decline marriage proposal.\n**Usage**: `!decline`")]
        [SlashCommand("decline", "Decline marriage proposal")]
        public async Task Decline()
        {
            //common variables
            IUser user = Context.User;
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithAuthor(user);
            eb.WithColor(ProfileDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor());

            var proposals = MarriageDb.GetProposalsReceived(user.Id, Context.Guild.Id);
            proposals.AddRange(MarriageDb.GetProposalsSent(user.Id, Context.Guild.Id));
            var proposal = await UserUtil.SelectMarriage(proposals, this);

            if (proposal == null)
            {
                eb.WithDescription("~ You have no proposals ~");
                await ReplyAsync(eb.Build());
                return;
            }

            ulong wife = UserUtil.GetWifeId(proposal, user.Id);

            if (await Confirm($"Are you sure you wish to Decline **{ BasicUtil.IdToMention(wife) }**?"))
            {
                await MarriageDb.DeleteMarriageOrProposal(proposal);
                eb.WithDescription($"You declined the proposal.\nBetter luck next time **{ BasicUtil.IdToMention(wife) }**.");
                await ReplyAsync(eb.Build());
            }
        }

        [Command("Divorce"), Description("Divorce a user.\n**Usage**: `!divorce`")]
        [SlashCommand("divorce", "Divorce a user")]
        public async Task Divorce()
        {
            //common variables
            IUser user = Context.User;
            EmbedBuilder eb = new EmbedBuilder();
            Discord.Color userColour = ProfileDb.GetHex(out string colour, user.Id) ? (Discord.Color)UserUtil.HexToColor(colour) : BasicUtil.RandomColor();
            eb.WithColor(userColour);
            eb.WithAuthor(user);

            var marriages = MarriageDb.GetMarriages(user.Id, Context.Guild.Id);
            var marriage = await UserUtil.SelectMarriage(marriages, this);

            if (marriage == null)
            {
                eb.WithDescription("~ You are not married ~");
                await ReplyAsync($"", false, eb.Build());
                return;
            }

            ulong wife = UserUtil.GetWifeId(marriage, user.Id);

            if (await Confirm($"Are you sure you wish to Divorce **{ BasicUtil.IdToMention(wife) }**?"))
            {
                await MarriageDb.DeleteMarriageOrProposal(marriage);
                await ReplyAsync(eb.WithDescription($"You divorced **{ BasicUtil.IdToMention(wife) }**.\n*~ May you both find happiness elsewhere ~*").Build());
            }
        }

        [Command("Proposals"), Alias("ShowProposals", "Proposal"), Description("Displays sent & received proposals.\n**Usage**: `!proposals`")]
        [SlashCommand("proposals", "Show proposals")]
        public async Task Proposals([Remainder] IUser user = null)
        {
            await ReplyAsync("", false, UserUtil.ProposalsEmbed(user ?? Context.User, Context.Guild).Build());
        }

        [Command("Marriages"), Alias("ShowMarriages", "Marraiges", "Marriage", "sm"), Description("Displays marriages.\n**Usage**: `!sm`")]
        [SlashCommand("marriages", "Show marital details")]
        public async Task Marriages([Remainder] IUser user = null)
        {
            await ReplyAsync("", false, UserUtil.MarriagesEmbed(user ?? Context.User, Context.Guild, Client).Build());
        }

        [Command("SetColour"), Alias("SetColor", "sc"), Description("Set your profile colour.\n**Usage**: `!sc [colour_name or hex_value]`")]
        [SlashCommand("profile-colour", "Set your profile colour")]
        public async Task SetPersonalColour([Discord.Interactions.Summary(description: "e.g. #ffc0cb")] string hexValue = "")
        {
            //way to set it back to default
            if (hexValue.Equals(""))
            {
                await ProfileDb.HexDefault(Context.User.Id);

                EmbedBuilder embed = UserUtil.SetColourEmbed(Context.User);
                embed.WithDescription($"{ Context.User.Username } set colour to **Default**");
                await ReplyAsync("", false, embed.Build());
                return;
            }
            
            System.Drawing.Color color = System.Drawing.Color.White;
            if (UserUtil.GetHexColour(ref color, hexValue))
            {
                try
                {
                    await ProfileDb.SetHex(color, Context.User.Id);
                    await ReplyAsync("", false, UserUtil.SetColourEmbed(Context.User).Build());
                }
                catch (Exception ex) { await ReplyAsync(ex.Message); }
            }
        }

        [Command("SetQuote"), Alias("sq"), Description("Sets your quote on profile.\n**Usage**: `!sq [quote]`")]
        [SlashCommand("profile-quote", "Set your profile quote")]
        public async Task SetPersonalQuote([Remainder] string quote = null)
        {

            //null me babi
            if (quote == null)
            {
                await ProfileDb.SetQuote(Context.User.Id, null);
                await ReplyAsync("Quote removed.");
                return;
            }

            //length check
            if (quote.Length > Constants.quoteCap)
            {
                await ReplyAsync($"Quotes have a { Constants.quoteCap } character limit. {quote.Length}/{Constants.quoteCap}");
                return;
            }

            //setting quote + getting embed & quote
            await ProfileDb.SetQuote(Context.User.Id, quote);
            await ReplyAsync("Quote set!", false, UserUtil.QuoteEmbed(Context.User).Build());
        }

        [Command("SetImage"), Alias("si"), Description("Sets thumbnail image on profile. \n**Usage**: `!si [image_url_or_attachment]`")]
        [SlashCommand("profile-image", "Set your profile image")]
        public async Task SetPersonalImageCommand(string url = null)
        {
            url ??= ((ICommandContext)Context).Message.Attachments.FirstOrDefault()?.Url;
            
            //to delete image
            if (url == null)
            {
                await ProfileDb.SetImage(Context.User.Id, null);
                await ReplyAsync("Image removed.");
                return;
            }

            //url check
            if (!WebUtil.IsValidUrl(url))
            {
                await ReplyAsync("This URL is just like you... Invalid.");
                return;
            }

            //image validity check
            if (!WebUtil.IsImageUrl(url))
            {
                await ReplyAsync("This URL is not an image, what do you want me to do with it?");
                return;
            }

            //building embed
            await ProfileDb.SetImage(Context.User.Id, url);
            EmbedBuilder embed = UserUtil.QuoteEmbed(Context.User);
            await ReplyAsync("Image set!", false, embed.Build());
        }

        [Command("Quote"), Alias("q"), Description("Allows user to see their personal quote and Image.\n**Usage**: `!q [user(s)_optional]`")]
        [SlashCommand("quote", "Show quote and image")]
        public async Task DisplayPersonalQuote(IUser user = null)
        {
            //variables
            EmbedBuilder embed;
            bool isMe = false;
            if (user == null)
            {
                user = Context.User;
                isMe = true;
            }

            if (Context is ICommandContext && ((ICommandContext)Context).Message is SocketUserMessage)
            {
                IReadOnlyCollection<SocketUser> users = ((SocketUserMessage)((ICommandContext)Context).Message).MentionedUsers;
                if (users != null && users.Count > 1)
                {
                    embed = UserUtil.StitchedQuoteEmbed(users);
                    if (embed == null) await ReplyAsync("No one had a proper \"Quote\" qq");
                    else await ReplyAsync("", false, embed.Build());
                    return;
                }
            }

            //checking quote
            embed = UserUtil.QuoteEmbed(user);
            if (embed == null)
            {
                await ReplyAsync($"{(isMe ? "You don't" : $"{ user.Username } doesn't") } have an image or a quote. Set one with `sq` and `si` commands.");
                return;

                //sending quote
            }
            await ReplyAsync("", false, embed.Build());
        }

        [Command("SetFeaturedWaifu"), Alias("sfw"), Description("Sets your waifu image on your profile.\n**Usage**: `!sfw [waifu_name]`")]
        [SlashCommand("set-favorite-waifu", "Choose your favorite waifu to display on your profile")]
        public async Task SetFeaturedWaifu([Remainder] string waifuSearch = "")
        {
            if (waifuSearch == "")
            {
                await FeaturedWaifuDb.Delete(Context.User.Id, Context.Guild.Id);
                await ReplyAsync("Removed your featured waifu. Now your last bought will appear! The betrayal...");
                return;
            }

            var waifu = await WaifuUtil.ProcessWaifuListAndRespond(await WaifuDb.SearchWaifus(waifuSearch, false, UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id)), this);
            if (waifu == null)
            {
                return;
            }

            if (UserInventoryDb.GetWaifus(Context.User.Id, Context.Guild.Id).Any(x => x.Name.Equals(waifu.Name)))
            {
                await FeaturedWaifuDb.SetFeaturedWaifu(Context.User.Id, waifu, Context.Guild.Id);
                await ReplyAsync($"{waifu.Name} set as your featured waifu!", false, (await UserUtil.ProfileEmbed((SocketGuildUser)Context.User)).Build());
                return;
            }
            await ReplyAsync($":x: You don't have {waifu.Name}");
        }

        [Command("FeaturedWaifu"), Alias("fw"), Description("Views Featured Waifu.\n**Usage**: `!fw`")]
        [SlashCommand("favorite-waifu", "Show favorite waifu")]
        public async Task DisplayFeaturedWaifu(IUser user = null)
        {
            //variables
            bool isMe = false;
            if (user == null)
            {
                user = Context.User;
                isMe = true;
            }
            var waifu = FeaturedWaifuDb.GetFeaturedWaifu(user.Id, Context.Guild.Id);

            //checking featured exists
            if (waifu == null)
            {
                await ReplyAsync((isMe ? "You Have" : $"{ user.Username } Has") + " No Featured Waifu qq");
                return;

            }
            await ReplyAsync((isMe ? $"You have { waifu.Name } as your" : $"{ user.Username } has { waifu.Name } as his") + " Featured waifu!", false, WaifuUtil.WaifuEmbedBuilder(waifu, Context).Build());
        }

        [Command("Avatar"), Alias("pfp"), Description("View a users profile picture.\n**Usage**: `!pfp [user]`")]
        [SlashCommand("avatar", "Show someones profile picture")]
        public async Task Avatar([Remainder] IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            string guildAvatar = null;
            if (user is SocketGuildUser)
            {
                var guildUser = user as SocketGuildUser;
                guildAvatar = guildUser.GetGuildAvatarUrl(ImageFormat.Auto, 2048);
            }
            string avatar = user.GetAvatarUrl(ImageFormat.Auto, 2048);

            if (guildAvatar == null)
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(user)
                    .WithAuthor(user.Username + "'s Profile Picture", avatar, avatar)
                    .WithImageUrl(avatar)
                    .Build());
            }
            else
            {
                await ReplyAsync(embed: new EmbedBuilderPrepared(user)
                    .WithAuthor(user.Username + "'s Profile Picture", guildAvatar, guildAvatar)
                    .WithThumbnailUrl(avatar)
                    .WithImageUrl(guildAvatar)
                    .Build());
            }
        }

        [Command("Rep"), Description("Gives rep to a user.\n**Usage**: `!rep [user]`")]
        [SlashCommand("rep", "Give rep to your fren :)")]
        public async Task Rep([Remainder] IUser user = null)
        {
            var author = await ProfileDb.GetProfile(Context.User.Id);
            var cooldown = author.RepDate.AddHours(20);
            var now = System.DateTime.Now;

            if (user == null || cooldown > now)
            {
                if (cooldown > now)
                {
                    var span = cooldown.Subtract(now);
                    await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                        .WithDescription("You already repped someone today. If everyone is cool then no one is cool.\n" +
                        $"You must wait `{span.Hours} hours {span.Minutes} minutes {span.Seconds} seconds`")
                        .WithColor(Color.DarkRed)
                        .Build());
                    return;
                }
                else
                {
                    await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                        .WithDescription("Rep ready!")
                        .WithColor(BasicUtil.RandomColor())
                        .Build());
                    return;
                }
            }

            if (user == Context.User)
            {
                await ReplyAsync("Nice try, Mr. Perfect.");
                return;
            }

            author.RepDate = now;
            int rep = await ProfileDb.IncrementRep(user.Id);
            await ProfileDb.UpdateProfile(author);

            await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User)
                .WithDescription($"You repped {user.Mention}\nNow they have **{rep}** rep!")
                .WithThumbnailUrl("https://i.imgur.com/xxobSIH.png")
                .Build());

            var bot = Client.CurrentUser;
            if (user.Id == bot.Id)
            {
                await BalanceDb.AddToasties(Context.User.Id, 50, Context.Guild.Id);
                await ReplyAsync($"Thank you {Context.User.Mention}! <a:loveme:536705504798441483>", embed: CurrencyUtil.GiveEmbed(bot, Context.User, 50).Build());
            }
        }
    }
}
