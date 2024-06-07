![](https://i.imgur.com/Iw3SsqS.png)
<a href="https://discordbots.org/bot/418823684459855882" >
  <img src="https://discordbots.org/api/widget/status/418823684459855882.svg" alt="Namiko" />
</a>

# Namiko

Includes the C# Namiko discord bot, the accompanying Namiko Moe web app made with ASPNET and React, and Maid - a helper bot that does housekeeping in the Namiko Support Server.

Learn about the bot and it's features on [Namiko Moe](https://namiko.moe)

## Note from the developer

I used this project to teach myself programming as I was studying in my computer science course. Even though it was refactored a few times, it might still be difficult to understand and set-up for anyone who is not me.

I have tried many patterns and C# features while developing this, which you can explore in the codebase. Some of the features are implemented in clever but difficult patterns, such as a method that takes any list of items and turns it into a fully interactive paginated embed on Discord. Some features are implemented in dumb patterns, such as using static methods for almost everything that should be a service. 

Alas, the end product is performant and of high quality, but difficult to maintain and demands some major refactoring, such as introducing proper dependency injection.

The last major effort in the codebase was the conversion to Discord slash command support. Now all the modules work both as text commands and slash commands with the same implementation 🔥👌💯

## References

https://namiko.moe/Guide - bot usage guides.<br>
https://namiko.moe/Commands - list of commands.

## Main libraries

[Discord-net](https://github.com/discord-net/Discord.Net) - main implementation is based on this framework.<br>
[Discord-net-interactive](https://github.com/foxbot/Discord.Addons.Interactive) - some extensions for discord-net, custom modified to suit Namiko.<br>
[Lavalink-Victoria](https://github.com/Yucked/Victoria) - for music, also custom modified.<br>
