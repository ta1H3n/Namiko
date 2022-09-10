using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Model.Exceptions;
using Namiko.Handlers.Services;
using Namiko.Modules.Leaderboard;
using Namiko.Modules.Pro;
using Sentry;

namespace Namiko.Addons.Handlers;

public class TextCommandService
{
    public readonly CommandService Commands;
    private readonly IServiceProvider _services;
    private readonly BaseSocketClient _client;
    private static Dictionary<ulong, string> Prefixes;
    public static HashSet<ulong> Blacklist;
    private static bool Pause = false;
    private HashSet<int> _shardsDownloadingUsers = new HashSet<int>();
    
    private bool CommandsRegistered { get; set; } = false;

    public TextCommandService(IServiceProvider services)
    {
        _services = services;
        _client = services.GetService<BaseSocketClient>();
        var logger = services.GetService<Logger>();
        Commands = new CommandService(services.GetService<CommandServiceConfig>());

        Commands.CommandExecuted += AfterCommandExecuted;
        Commands.Log += logger.Console_Log;
        Commands.Log += Error_Log;

        _client.MessageReceived += ReadMessage;
        _client.LoggedIn += RegisterCommands;
        _client.LoggedIn += InitialisePrefixes;
    }

    private async Task InitialisePrefixes()
    {
        ServerDb.Prefixes = await ServerDb.GetPrefixes(AppSettings.DefaultPrefix);
    }

    private async Task RegisterCommands()
    {
        if (!CommandsRegistered)
        {
            await Commands.AddModuleAsync(typeof(Banroulettes), _services);
            await Commands.AddModuleAsync(typeof(Banroyales), _services);
            await Commands.AddModuleAsync(typeof(Basic), _services);
            await Commands.AddModuleAsync(typeof(Currency), _services);
            await Commands.AddModuleAsync(typeof(Images), _services);
            await Commands.AddModuleAsync(typeof(Roles), _services);
            await Commands.AddModuleAsync(typeof(ServerModule), _services);
            await Commands.AddModuleAsync(typeof(Special), _services);
            await Commands.AddModuleAsync(typeof(SpecialModes), _services);
            await Commands.AddModuleAsync(typeof(User), _services);
            await Commands.AddModuleAsync(typeof(Waifus), _services);
            await Commands.AddModuleAsync(typeof(WaifuEditing), _services);
            await Commands.AddModuleAsync(typeof(Web), _services);
            await Commands.AddModuleAsync(typeof(Music), _services);
            await Commands.AddModuleAsync(typeof(Leaderboards), _services);
            await Commands.AddModuleAsync(typeof(Pro), _services);

            CommandsRegistered = true;
        }
    }
    
    
    private async Task Client_ShardReady_DownloadUsers(DiscordSocketClient arg)
    {
        //method not thread safe, but it's ok.
        if (_shardsDownloadingUsers.Contains(arg.ShardId))
            return;

        try
        {
            _shardsDownloadingUsers.Add(arg.ShardId);

            await Task.Delay(5000);
            await arg.DownloadUsersAsync(arg.Guilds);
            int users = arg.Guilds.Sum(x => x.Users.Count);
            _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
                $":space_invader: `{DateTime.Now:HH:mm:ss}` - `Shard {arg.ShardId} downloaded {users} users.`");
        }
        finally
        {
            _shardsDownloadingUsers.Remove(arg.ShardId);
        }
    }
    
    
    private async Task ReadMessage(SocketMessage messageParam)
    {
        if (!(messageParam is SocketUserMessage message))
            return;

        var context = new CustomCommandContext(_client, message);
        
        
        if (context.User.IsBot)
            return;
        if (context.Message == null || context.Message.Content == "")
            return;
        if (BlacklistedChannelDb.IsBlacklisted(context.Channel.Id))
            return;
        if (RateLimit.InvokeLockout.TryGetValue(context.Channel.Id, out var time) && time > DateTime.Now)
            return;

        
        if (await ExecuteCommand(message, context))
            return;
        
        await SpecialResponse(message);
    }
    private async Task<bool> ExecuteCommand(SocketUserMessage message, CustomCommandContext context)
    {
        int ArgPos = 0;
        string prefix = context?.Guild == null ? AppSettings.DefaultPrefix : GetPrefix(context.Guild.Id);
        bool isPrefixed = message.HasStringPrefix(prefix, ref ArgPos) ||
                          message.HasMentionPrefix(_client.CurrentUser, ref ArgPos);

        if (!isPrefixed)
            return false;

        var cmds = Commands.Search(context, ArgPos);
        if (cmds.IsSuccess)
        {
            if (context.Guild != null && cmds.Commands.Any(x =>
                    DisabledCommandHandler.IsDisabled(x.Command.Name, context.Guild.Id, DisabledCommandType.Command)))
                return false;
            else if (context.Guild != null && cmds.Commands.Any(x =>
                         DisabledCommandHandler.IsDisabled(x.Command.Module.Name, context.Guild.Id,
                             DisabledCommandType.Module)))
                return false;

            else if (!RateLimit.CanExecute(context.Channel.Id))
            {
                await context.Channel.SendMessageAsync(
                    $"Woah there, Senpai, calm down! I locked this channel for **{RateLimit.InvokeLockoutPeriod.Seconds}** seconds <:MeguExploded:627470499278094337>\n" +
                    $"You can only use **{RateLimit.InvokeLimit}** commands per **{RateLimit.InvokeLimitPeriod.Seconds}** seconds per channel.");
                return false;
            }

            else if (Pause && context.User.Id != AppSettings.OwnerId)
            {
                await context.Channel.SendMessageAsync("Commands disabled temporarily. Try again later.");
                return false;
            }

            else if (context.Channel is SocketTextChannel ch
                     && (!ch.Guild.CurrentUser.GetPermissions(ch).Has(ChannelPermission.SendMessages) ||
                         !ch.Guild.CurrentUser.GetPermissions(ch).Has(ChannelPermission.EmbedLinks)))
            {
                var dm = await context.User.CreateDMChannelAsync();
                await dm.SendMessageAsync(embed: new EmbedBuilderPrepared(context.Guild.CurrentUser)
                    .WithDescription($"I don't have permission to reply to you in **{ch.Name}**.\n" +
                                     $"Make sure I have a role that allows me to send messages and embed links in the channels you want to use me in.")
                    .WithImageUrl("https://i.imgur.com/lrPHjyt.png")
                    .Build());
                return false;
            }
        }

        _ = Commands.ExecuteAsync(context, ArgPos, _services);
        return true;
    }
    private async Task AfterCommandExecuted(Optional<CommandInfo> cmd, ICommandContext context, IResult res)
    {
        string cmdName = cmd.IsSpecified ? cmd.Value.Name : null;
        bool success = res.IsSuccess;

        if (!success)
        {
            // Try sending a reaction image if there is no such command
            if (await new Images(_client).SendRandomImage(context))
            {
                cmdName = "ReactionImage";
                success = true;
            }

            // If the command is found but failed then send help message for said command
            else if (!(res.Error == CommandError.UnknownCommand || res.Error == CommandError.Exception))
            {
                string reason = res.ErrorReason + "\n";
                if (res.Error != CommandError.UnmetPrecondition)
                    reason += CommandHelpString(
                        context.Message.Content.Split(null)[0].Replace(GetPrefix(context.Guild.Id), ""),
                        GetPrefix(context.Guild.Id));
                await context.Channel.SendMessageAsync(embed: new EmbedBuilder().WithColor(Color.DarkRed)
                    .WithDescription(":x: " + reason).Build());
            }
        }
        // If the command is found and completed but it is a music command in a guild with no premium - set as failed
        else if ((cmdName == nameof(Music.Join) || cmdName == nameof(Music.Play) || cmdName == nameof(Music.PlayNext) ||
                  cmdName == nameof(Music.PlayFirst))
                 && !(PremiumDb.IsPremium(context.Guild.Id, ProType.GuildPlus) ||
                      PremiumDb.IsPremium(context.Guild.Id, ProType.Guild)))
        {
            success = false;
        }


        // If command is found - save a log of it
        if (cmdName != null && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
            await Stats.LogCommand(cmdName, context, success);
    }
    private async Task Error_Log(LogMessage logMessage)
    {
        if (logMessage.Exception is CommandException cmdException)
        {
            if (cmdException.InnerException is NamikoException ex)
            {
                await cmdException.Context.Channel.SendMessageAsync(":x: " + ex.Message);
            }
            else
            {
                SentrySdk.WithScope(scope =>
                {
                    scope.SetTag("Command", cmdException.Command.Name);
                    scope.SetExtra("GuildId", cmdException.Context.Guild.Id);
                    scope.SetExtra("Guild", cmdException.Context.Guild.Name);
                    scope.SetExtra("GuildOwnerId", cmdException.Context.Guild.OwnerId);
                    scope.SetExtra("ChannelId", cmdException.Context.Channel.Id);
                    scope.SetExtra("Channel", cmdException.Context.Channel.Name);
                    scope.SetExtra("UserId", cmdException.Context.User.Id);
                    scope.SetExtra("User", cmdException.Context.User.Username);
                    scope.SetExtra("MessageId", cmdException.Context.Message.Id);
                    scope.SetExtra("Message", cmdException.Context.Message.Content);
                    if (cmdException.InnerException is HttpException)
                        scope.Level = Sentry.Protocol.SentryLevel.Warning;
                    SentrySdk.CaptureException(cmdException.InnerException);
                });

                if (cmdException.Command.Module.Name.Equals(nameof(WaifuEditing)))
                {
                    await cmdException.Context.Channel.SendMessageAsync(cmdException.InnerException.Message);
                }
            }
        }
    }
    
    
    private async Task SpecialResponse(SocketUserMessage message)
    {
        if (_client == null)
            return;

        if (message.Content.Contains("rep", StringComparison.OrdinalIgnoreCase))
            return;

        if (message.Content.StartsWith("Hi Namiko", StringComparison.OrdinalIgnoreCase))
        {
            await message.Channel.SendMessageAsync($"Hi {message.Author.Mention} :fox:");
            return;
        }

        if (message.Content.StartsWith("Namiko-sama", StringComparison.OrdinalIgnoreCase))
        {
            var msgs = new List<string>
            {
                $"Rise, {message.Author.Mention}",
                $"Yes, {message.Author.Mention}?"
            };
            await message.Channel.SendMessageAsync(msgs[new Random().Next(msgs.Count)]);
            return;
        }

        if (_client?.CurrentUser == null)
            return;

        // string msg = message.Content.Replace("!", "");
        // string mention = _client.CurrentUser.Mention.Replace("!", "");
        // if (msg.Contains(mention) && (!msg.StartsWith(mention) || msg.Equals(mention)))
        // {
        //     await message.Channel.SendMessageAsync($"{message.Author.Mention} <a:loveme:536705504798441483>");
        // }
    }

    
    private string CommandHelpString(string commandName, string prefix)
    {
        try
        {
            var cmd = Commands.Commands
                .Where(x => x.Aliases.Any(y => y.Equals(commandName, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();
            string str = cmd.Summary;
            string result = "**Description**: " + str;
            result = result.Replace("!", prefix);
            return result;
        }
        catch
        {
            return "";
        }
    }

    public static string GetPrefix(ulong guildId)
    {
        var prefix = ServerDb.GetPrefix(guildId);
        if (prefix == null)
        {
            return AppSettings.DefaultPrefix;
        }
        else
        {
            return prefix;
        }
    }
}