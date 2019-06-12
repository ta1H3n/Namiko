![](https://i.imgur.com/Iw3SsqS.png)
<a href="https://discordbots.org/bot/418823684459855882" >
  <img src="https://discordbots.org/api/widget/status/418823684459855882.svg" alt="Namiko" />
</a>

# Namiko
You can find all the features and commands of Namiko here, and how to use them. This page **does not** contain instructions on how to set-up and host Namiko on your own.

<a href="https://giant.gfycat.com/PlushPrestigiousAsiaticmouflon.webm">
  <img src="https://thumbs.gfycat.com/PlushPrestigiousAsiaticmouflon-size_restricted.gif"><br>
</a>

*Click on the gif to view it in higher quality!*

# Getting Started
[Invite Namiko to your server](https://discordapp.com/oauth2/authorize?client_id=418823684459855882&scope=bot&permissions=268707844)  

Namiko's prefix can be changed by using the `!sp [new_prefix]` command and replacing `[new_prefix]` with your desired prefix. The default is `!` and it will be used in this wiki. Mentioning Namiko can also be used as a prefix e.g. `@Namiko sp [new_prefix]`.  

Type `!help` or `!help [command]` in a server with Namiko for a list of commands and command usage instructions.

### Premium
Namiko now has premium upgrades! Both for users and for servers! You can find them [on Patreon](https://www.patreon.com/taiHen).  

## Contents
1. [Waifus](#waifus)
1. [Banroulette](#banroulette)
1. [User Profile](#user-profile)
1. [Currency & Gambling](#currency-and-gambling)
1. [Reaction Images](#reaction-images)
1. [Teams](#teams)
1. [Server Settings](#server-settings)
1. [Web Commands](#web-commands)
1. [Full Command List](#full-command-list)
## Waifus
Namiko has ~1000 handmade and tiered waifus from anime, games and more. They were all carefully made by our users, per request and per popularity. You can ask our insiders to create a waifu for you in [Namiko Test Realm.](https://discord.gg/W6Ru5sM)

Useful commands:
* **!waifu** - Searches for a waifu in the database, based on their name or source. If more than one match is found it returns a list. Searching for the short ID name will always return the specific waifu.
* **!waifushop** - Shows a list of waifus currently for sale, resets every 12 hours.
* **!givewaifu** - You can trade waifus with other users.
* **!waifus** - shows a list of the waifus you have.
* **!aww** - add a waifu to your wishlist. Namiko will dm you if the waifu goes for sale, and you will be listed on the waifu card. Limited to 5. Increase the limit to 12 with Waifu Premium.
* **!wwl** - shows yours or another users waifu wishlist.

Your last bought waifu will also appear on your profile. To see your profile use the **!profile** command. To change the waifu previewed on your profile you can use the **!sfw** command.

## Banroulette
Create a banroulette in a channel where users can join for a chance to be banned, while the others share rewards from a pool of Toasties.
The banroulette can be fully customized: 
* The ban duration in hours (0 - no ban)
* The minimum amount of users required to finish the banroulette
* The maximum amount of users
* The toastie reward pool (admin only)
* And optionally setting a required role to join  

Useful commands:
* **!nbr** - starts a new banroulette.
* **!jbr** - join the banroulette.
* **!br** - view the details.
* **!ebr** - end the banroulette.
* **!cbr** - cancel the banroulette.

Namiko will guide you through the process, it's really simple!

## User Profile
Full profile customization. You can set an image, quote and a color for yourself. You can also display your favorite waifu on your profile, it's the last you bought by default. The quote, color and image is shared across all servers, but the waifu is not.  

Useful Commands:
* **!profile** - view yours or someone's profile.
* **!quote** - view yours or someone's unique quote and image.
* **!si** - set image.
* **!sq** - set quote.
* **!sc** - set color. e.g. `!sc dark blue` or `!sc #121212` - hex value.
* **!sfw** - set your featured waifu on your profile.
* **!waifus** - view yours or someone's waifus.
* **!marry** - marry another user.

## Currency and Gambling
The currency is Toasties, and is server exclusive. Toasties can be used to buy waifus, or trade with other users. Toasties can be gambled, and Namiko's balance acts as the casino balance - meaning that if you gamble, you win or lose toasties from Namiko's balance. Namiko starts off with a balance of 1 million. This stops absurd of amounts of toasties being won - preventing ruining of the game experience. Namiko gains a small amount of your dailies and weeklies, based on how much you have, how much she has, and the whole net amount in the server.  

Useful Commands:
* **!bal** - view a users balance.
* **!daily** - claim a daily toastie reward. The value is randomized and increases with your streak. 
* **!weekly** - claim a weekly reward. The weekly is like a daily of the person with the highest streak in the server, +15 days.
* **!flip** - flip a coin for a 50/50 chance to win or lose toasties.
* **!blackjack** - play blackjack with a bet of toasties.
* **!give** - give toasties to someone else.

If Namiko has less than 200,000 toasties, every hour she has a 1/5 chance to steal 1% of toasties from every user.

## Reaction Images
There a lot of reaction images you can use by typing an image command. Such as pat, hug, kick, hydrate and much more. Use them by simply typing the prefix `!` and the image name after it e.g. `!pat`. View the list of all the image commands by typing `!listall`. You can suggest adding new images and more image commands in [Namiko Test Realm](https://discord.gg/W6Ru5sM), we always appreciate new additions!  

Useful Commands:
* **![image_name]** - sends a random image from all the images under the name.
* **!listall** - lists all the image commands Namiko currently has.
* **!all [image_name]** - gives a link to an imgur album with all the images in the command, and their IDs.
* **!image** - sends a specific image by it's ID.
* **!newimage** - add a server exclusive reaction image. (Requires T1 Server Premium)

## Teams
Role based teams can be made, where a *leader* can invite users to their team, and if they accept they will be given the teams role. Invites expire in 24 hours. The leader roles must be assigned manually.  

Useful Commands:
* **!nt** - creates a new team with selected roles.
* **!invite** - invite a user to your team. **Team Leader only.**
* **!join** - accept an invitation.
* **!leaveteam** - leave your team.
* **!kick** - kick a user from your team. **Team Leader only.**
* **!team** - view details about a team.

## Server Settings
Various settings for the server, such as welcome messages, server join logs, team join logs, public roles and blacklisting channels. 

Useful Commands:
* **!spr** - sets a role as public, where anyone can gain the role by typing: `!role [role_name]`.
* **!role** - gives or removes a public role from you.
* **!prl** - lists the public roles.
* **!wch** - sets a **Welcome Channel**.
* **!blch** - blacklists a channel, disabling Namiko commands in that channel.
* **!jch** - sets a **Server Join Log** channel.
* **!tch** - sets a **Team Join Log** channel.

## Web Commands
Commands using various internet services.  

Useful Commands:
* **!source** - tries to find the source of an image using SauceNao.
* **!iqdb** - tries to find the source of an image using IQDB.
* **!anime** - looks up an anime on MAL.
* **!manga** - looks up a manga on MAL.
* **!subreddit** - subscribe to hop posts from a subreddit that reach a certain upvote limit.

# Full Command List
All of Namiko's commands, excluding some technical, host only commands.

## Banroulettes
* **NewBanroulette** - Starts a new game of ban roulette, where one participant is randomly banned from the server. Winners split toasties from the reward pool.
**Usage**: `!nbr [ban_length_in_hours] [required_role_name-optional]`.
* **JoinBanroulette** - Join the current Ban Roulette. Must be in the same channel.
**Usage**: `!jbr`.
* **CancelBanroulette** - Cancels the current Ban Roulette.
**Usage**: `!cbr`.
* **EndBanroulette** - Ends the current Ban Roulette, banning a random participant and splitting the reward pool between the others.
**Usage**: `!ebr`.
* **BRRewardPool** - Add toasties to the Ban Roulette reward pool from your account.
**Usage**: `!brrp [amount]`.
* **SetBRRewardPool** - Set the reward pool.
**Usage**: `!sbrrp [amount]`.
* **SetBRMinParticipants** - Set minimum participants.
**Usage**: `!sbrmin [amount]`.
* **SetBRMaxParticipants** - Set maximum participants.
**Usage**: `!sbrmax [amount]`.

## Currency
* **Blackjack** - Starts a game of blackjack.
**Usage**: `!bj [amount]`.
* **Daily** - Gives daily toasties..
* **Weekly** - Gives weekly toasties..
* **Flip** - Flip a coin for toasties, defaults to tails.
**Usage**: `!flip [amount] [heads_or_tails]`.
* **Balance** - Shows amount of toasties.
**Usage**: `!bal [user_optional]`.
* **SetToasties** - Sets the amount of toasties.
**Usage**: `!st [user] [amount]`.
* **AddToasites** - Adds toasties to a user.
**Usage**: `!at [user] [amount]`.
* **ToastieLeaderboard** - Toastie Leaderboard.
**Usage**: `!tlb [page_number]`.
* **DailyLeaderboard** - Daily Leaderboard.
**Usage**: `!dlb [page_number]`.
* **Beg** - Beg Namiko for toasties.
**Usage**: `!beg`.
* **Open** - Open a lootbox if you have one.
**Usage**: `!open`.
* **Lootboxes** - Lists your lootboxes.
**Usage**: `!Lootboxes`.

## Images
* **List** - List of all image commands and how many images there are.
**Usage**: `listall`.
* **Album** - All reaction images from a single command.
**Usage**: `!all [image_name]`.
* **Image** - Sends a reaction image by id.
**Usage**: `!i [id]`.
* **NewImage** - Adds a new image to the database.
**Usage**: `!ni [name] [url_or_attachment]`.
* **DeleteImage** - Deletes image from the database using the id.
**Usage**: `di [id]`.

## Roles
* **Role** - Adds or removes a public role from the user.
**Usage**: `!r [name]`.
* **SetPublicRole** - Sets or unsets a role as a public role.
**Usage**: `!spr [name]`.
* **ClearRole** - Removes all users from a role.
**Usage**: `cr [name]`.
* **Invite** - Invites a user to your team.
**Usage**: `!inv [user]`.
* **Join** - Accept an invite to a team.
**Usage**: `!join [team_name]`.
* **LeaveTeam** - Leave your team.
**Usage**: `!lt`.
* **TeamKick** - Kicks a user from your team.
**Usage**: `!tk [user]`.
* **NewTeam** - Creates a new team.
**Usage**: `!nt [LeaderRoleName] [MemberRoleName]`.
* **DeleteTeam** - Deletes a team.
**Usage**: `!dt [Leader or Team RoleName]`.
* **TeamList** - Lists all teams.
**Usage**: `!tl`.
* **PublicRoleList** - Lists all public roles.
**Usage**: `!prl`.

## Server
* **Server** - Stats about the server.
**Usage**: `!server`.
* **SetPrefix** - Sets a prefix for the bot in the server.
**Usage**: `!sp [prefix]`.
* **SetJoinLogChannel** - Sets a channel to log users joining/leaving the guild.
**Usage**: `!jlch`.
* **SetTeamLogChannel** - Sets a channel to log users joining/leaving teams.
**Usage**: `!tlch`.
* **SetWelcomeChannel** - Sets a channel to welcome members.
**Usage**: `!wch`.
* **BlacklistChannel** - Disables or enables bot commands in a channel.
**Usage**: `!blch [optional_channel_id]`.
* **ListWelcomes** - Lists all welcomes and their IDs..
* **ActivateServerPremium** - Activates server premium in the current server.
**Usage**: `!asp [tier]`.

## User
* **Profile** - Showsa a users profile.
**Usage**: `!profile [user_optional]`.
* **Waifus** - Shows a users waifu list.
**Usage**: `!waifus [user_optional]`.
* **Marry** - Propose to a user.
**Usage**:  `!m [user]`.
* **Decline** - Decline marriage proposal.
**Usage**: `!decline`.
* **Divorce** - Divorce a user.
**Usage**: `!divorce`.
* **Proposals** - Displays sent & received proposals.
**Usage**: `!proposals`.
* **Marriages** - Displays marriages.
**Usage**: `!sm`.
* **SetColour** - Set your profile colour.
**Usage**: `!sc [colour_name or hex_value]`.
* **SetQuote** - Sets your quote on profile.
**Usage**: `!sq [quote]`.
* **SetImage** - Sets thumbnail Image on profile. 
**Usage**: `!si [image_url_or_attachment]`.
* **SetFeaturedWaifu** - Sets your waifu image on your profile.
**Usage**: `!sfw [waifu_name]`.
* **UndoColour** - Switch back to a previous color.
**Usage**: `!scp`.
* **ColourHistory** - List of your previous colors.
**Usage**: `!scpl`.
* **ActivatePremium** - Activates premium subscriptions associated with this account.
**Usage**: `!ap`.

## Waifus
* **WaifuShop** - Opens the waifu shop..
* **WaifuShopSlides** - Opens the waifu shop slides..
* **BuyWaifu** - Buys a waifu, must be in a shop.
**Usage**: `!bw [name]`.
* **SellWaifu** - Sells a waifu you already own for a discounted price.
**Usage**: `!sw [name]`.
* **GiveWaifu** - Transfers waifu to another user.
**Usage**: `!gw [user] [waifu_name]`.
* **TopWaifus** - Shows most popular waifus.
**Usage**: `!tw`.
* **ServerTopWaifus** - Shows most popular waifus in the server.
**Usage**: `!stw`.
* **WaifuLeaderboard** - Shows waifu worth of each person.
**Usage**: `!wlb`.
* **AddWaifuWish** - Add a waifu to your wishlist to be notified when it appears in shop.
Limited to 5.
**Usage**: `!ww [waifu]`.
* **WaifuWishlist** - Shows yours or someone's waifu wishlist.
**Usage**: `!wwl [user_optional]`.
* **RemoveWaifuWish** - Removes a waifu from your wishlist.
**Usage**: `!rww [waifu]`.

## WaifuEditing
* **NewWaifu** - Adds a waifu to the database.
**Usage**: `!nw [name] [tier(1-3)] [image_url]`. **Insider only.**
* **DeleteWaifu** - Removes a waifu from the database.
**Usage**: `!dw [name]`. **Insider only.**
* **WaifuFullName** - Changes the full name of a waifu.
**Usage**: `!wfn [name] [fullname]`. **Insider only.**
* **WaifuDescription** - Changes the description of a waifu.
**Usage**: `!wd [name] [description]`. **Insider only.**
* **WaifuSource** - Changes the source of a waifu.
**Usage**: `!ws [name] [source]`. **Insider only.**
* **WaifuTier** - Changes the tier of a waifu.
**Usage**: `!wt [name] [tier(1-3)]`. **Insider only.**
* **WaifuImage** - Changes the image of a waifu.
**Usage**: `!wi [name] [image_url]`. **Insider only.**
* **RenameWaifu** - Change a waifu's primary name.
**Usage**: `!rw [oldName] [newName]`. **Insider only.**
* **AutocompleteWaifu** - Auto completes a waifu using MAL.
**Usage**: `!acw [name] [MAL_ID]`. **Insider only.**
* **ResetWaifuShop** - Resets the waifu shop contents.. **Insider only.**

## Web
* **IQDB** - Finds the source of an image in iqdb.
**Usage**: `!iqdb [image_url]` or `!iqdb` with attached image..
* **Source** - Finds the source of an image with SauceNao.
**Usage**: `!source [image_url]` or `!source` with attached image..
* **Anime** - Searchs MAL for an anime and the following details.
**Usage**: `!Anime [anime_title]`.
* **Manga** - Searchs MAL for an manga and the following details.
**Usage**: `!Manga [manga_title]`.
* **MALWaifu** - Searches MAL for characters.
**Usage**: `!malw [query]`. **Insider only.**
* **Subreddit** - Set a subreddit for Namiko to post hot posts from.
**Usage**: `!sub [subreddit_name] [min_upvotes]`.
* **Unsubscribe** - Unsubscribe from a subreddit.
**Usage**: `!unsub [subreddit_name]`.
* **SubList** - Subreddits you are subscribed to.
**Usage**: `!sublist`.
