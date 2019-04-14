![](https://i.imgur.com/Iw3SsqS.png)
<a href="https://discordbots.org/bot/418823684459855882" >
  <img src="https://discordbots.org/api/widget/status/418823684459855882.svg" alt="Namiko" />
</a>
# Namiko
You can find all the features and commands of Namiko here, and how to use them. This page **does not** contain instructions on how to set-up and host Namiko on your own.

# Getting Started
[Invite Namiko to your server](https://discordapp.com/oauth2/authorize?client_id=418823684459855882&scope=bot&permissions=268707844)  

Namiko's prefix can be changed by using the `!sp [new_prefix]` command and replacing `[new_prefix]` with your desired prefix. The default is `!` and it will be used in this wiki. Mentioning Namiko can also be used as a prefix e.g. `@Namiko sp [new_prefix]`.  

Type `!help` or `!help [command]` in a server with Namiko for a list of commands and command usage instructions.

The following is a list of the main Namiko's features and details on how to use them.  
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
* **!ww** - add a waifu to your wishlist. Namiko will dm you if the waifu goes for sale, and you will be listed on the waifu card. Limited to 5.
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

# Full Command List
-to be added-
