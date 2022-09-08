using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Namiko.Handlers.Services;
using Namiko.Handlers.TypeConverters;
using Namiko.Modules.Leaderboard;
using Namiko.Modules.Pro;
using Sentry;

namespace Namiko.Addons.Handlers;

public class SlashCommandService
{
    public readonly InteractionService Interaction;
    private readonly IServiceProvider _services;
    private readonly DiscordShardedClient _client;
    private readonly DiscordService _discordService;
    private readonly List<string> _reactionCommands;

    private bool CommandsRegistered { get; set; } = false;

    public SlashCommandService(IServiceProvider services)
    {
        _reactionCommands = GetImageCommands();
        _services = services;
        _client = services.GetService<DiscordShardedClient>();
        _discordService = services.GetService<DiscordService>();
        var logger = services.GetService<Logger>();
        Interaction = new InteractionService(_client, services.GetService<InteractionServiceConfig>());

        Interaction.AddTypeConverter<Waifu>(new WaifuConverter());
        Interaction.AddTypeConverter<ShopWaifu>(new ShopWaifuConverter());


        Interaction.SlashCommandExecuted += AfterSlashCommandExecuted;
        Interaction.Log += logger.Console_Log;

        _client.InteractionCreated += ExecuteInteraction;
        _client.ShardReady += RegisterCommands;
        _client.ShardReady += RegisterReactionImageCommands;
    }

    private int _reactionImageCommandsRegistered = 0;
    private async Task RegisterReactionImageCommands(DiscordSocketClient client)
    {
        // Making sure this part only runs once, unless an exception is thrown. Thread safe.
        if (_client.Shards.All(x => x.ConnectionState == ConnectionState.Connected) &&
            Interlocked.Exchange(ref _reactionImageCommandsRegistered, 1) == 0)
        {
            try
            {
                var sharedCommands = _reactionCommands.Select(x => new SlashCommandBuilder()
                    .WithName(x)
                    .WithDescription($"Send a random reaction image/gif")
                    .Build()
                ).ToArray();

                foreach (var guild in client.Guilds)
                {
                    if (Images.ReactionImageCommands.ContainsKey(guild.Id))
                    {
                        
                    }
                    guild.BulkOverwriteApplicationCommandAsync(sharedCommands);
                }
            }
            catch (Exception ex)
            {
                _reactionImageCommandsRegistered = 0;
                SentrySdk.CaptureException(ex);
            }
        }
    }

    private int _commandsRegistered = 0;
    private async Task RegisterCommands(DiscordSocketClient client)
    {
        // Making sure this part only runs once, unless an exception is thrown. Thread safe.
        if (Interlocked.Exchange(ref _commandsRegistered, 1) == 0)
        {
            try
            {
                // await Interaction.AddModuleAsync(typeof(Banroulettes), _services);
                // await Interaction.AddModuleAsync(typeof(Banroyales), _services);
                Interaction.AddModuleAsync(typeof(Basic), _services);
                Interaction.AddModuleAsync(typeof(Leaderboards), _services);
                Interaction.AddModuleAsync(typeof(Currency), _services);
                Interaction.AddModuleAsync(typeof(Images), _services);
                Interaction.AddModuleAsync(typeof(Roles), _services);
                Interaction.AddModuleAsync(typeof(ServerModule), _services);
                Interaction.AddModuleAsync(typeof(User), _services);
                Interaction.AddModuleAsync(typeof(Waifus), _services);
                Interaction.AddModuleAsync(typeof(WaifuEditing), _services);
                Interaction.AddModuleAsync(typeof(Web), _services);
                Interaction.AddModuleAsync(typeof(Music), _services);
                Interaction.AddModuleAsync(typeof(Pro), _services);

                if (_discordService.Development)
                {
                    Interaction.AddModulesToGuildAsync(418900885079588884, true, Interaction.Modules.ToArray());
                }
                else
                {
                    Interaction.AddModulesGloballyAsync(true,
                        Interaction.Modules.Where(x => x.Name != nameof(WaifuEditing)).ToArray());
                    Interaction.AddModulesToGuildAsync(418900885079588884, true,
                        Interaction.Modules.FirstOrDefault(x => x.Name == nameof(WaifuEditing)));
                }
            }
            catch (Exception ex)
            {
                _commandsRegistered = 0;
                SentrySdk.CaptureException(ex);
            }
        }
    }


    private async Task ExecuteInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new CustomInteractionContext(_client, interaction);
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                if (((SocketSlashCommand)interaction).CommandName != "waifu-edit")
                {
                    await interaction.DeferAsync();
                }
            }

            var result = await Interaction.ExecuteCommandAsync(context, _services);
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private async Task AfterSlashCommandExecuted(SlashCommandInfo cmd, IInteractionContext con, IResult res)
    {
        var context = (CustomInteractionContext)con;


        if (!res.IsSuccess)
        {
            // prevent doing "Bot is thinking..." for 15min if command fails
            if (await context.Interaction.GetOriginalResponseAsync() == null)
            {
                await context.ReplyAsync(embed: new EmbedBuilderPrepared(":x: Unkown error occured").Build(),
                    ephemeral: true);
            }
        }
    }

    private List<string> GetImageCommands()
    {
        return new List<string>(100)
        {
            "hug",
            "kiss",
            "pat",
            "stop",
            "cuddle",
            "lewdkiss",
            "slap",
            "lick",
            "cry",
            "bonk",
            "nom",
            "punch",
            "kick",
            "love",
            "dance",
            "blush",
            "spank",
            "poke",
            "bite",
            "steal",
            "sex",
            "gay",
            "lewdlick",
            "cheekkiss",
            "squish",
            "lewdcuddle",
            "lewd",
            "fluff",
            "pay",
            "pout",
            "boop",
            "sleep",
            "headkiss",
            "bang",
            "monthly",
            "yeet",
            "ara",
            "nut",
            "run",
            "grape",
            "lewdbite",
            "cryhug",
            "lewdpat",
            "poof",
            "buttouch",
            "sip",
            "highfive",
            "x",
            "wave",
            "banana",
            "test",
            "servermeme",
            "namiko",
            "sudoku",
            "dab",
            "nuzzle",
            "laugh",
            "peek",
            "owo",
            "goodsleepfloof",
            "welcome",
            "furry",
            "holdhand",
            "ora",
            "subonk",
            "nyaa",
            "clap",
            "suhug",
            "step",
            "bad",
            "wasted",
            "muda",
            "smug",
            "vore",
            "pancakes",
            "tickle",
            "yes",
            "zawarudo",
            "thonk",
            "hydrate",
            "ame",
            "rawr",
            "padoru",
            "dodge",
            "arrest",
            "rat",
            "supat",
            "explosion",
            "fistbump",
            "kabedon",
            "yawn",
            "yamero",
            "sneeze",
            "waffles",
            "thundercrosssplitattack",
            "brofist",
            "protecc",
            "pun",
            "ree",
            "creeper",
        };
    }
}