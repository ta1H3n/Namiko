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
        _client.ShardReady += RegisterCommandModules;
        _client.ShardReady += RegisterReactionImageCommands;
        _client.JoinedGuild += RegisterReactionImageCommands;
    }

    

    private int _reactionImageCommandsRegistered = 0;
    private async Task RegisterReactionImageCommands(SocketGuild guild, SlashCommandProperties[] sharedCommands)
    {
        sharedCommands ??= BuildReactionImageCommands(_reactionCommands);
        
        if (Images.ReactionImageCommands.ContainsKey(guild.Id))
        {
            var guildImages = Images.ReactionImageCommands[guild.Id];
            var commands = BuildReactionImageCommands(guildImages).ToList();
            commands.AddRange(sharedCommands.Where(x => !guildImages.Contains(x.Name.Value)));

            _ = guild.BulkOverwriteApplicationCommandAsync(commands.ToArray());
        }
        else if (guild.Id == 418900885079588884)
        {
            var commands = await guild.GetApplicationCommandsAsync();

            foreach (var cmd in sharedCommands.Take(100 - commands.Count))
            {
                _ = guild.CreateApplicationCommandAsync(cmd);
            }
        }
        else
        {
            _ = guild.BulkOverwriteApplicationCommandAsync(sharedCommands);
        }
    }
    private Task RegisterReactionImageCommands(SocketGuild guild) => RegisterReactionImageCommands(guild, null);
    private async Task RegisterReactionImageCommands(DiscordSocketClient client)
    {
        // Making sure this part only runs once, unless an exception is thrown. Thread safe.
        if (_client.Shards.All(x => x.ConnectionState == ConnectionState.Connected) &&
            Interlocked.Exchange(ref _reactionImageCommandsRegistered, 1) == 0)
        {
            try
            {
                var sharedCommands = BuildReactionImageCommands(_reactionCommands);
                foreach (var guild in client.Guilds)
                {
                    _ = RegisterReactionImageCommands(guild, sharedCommands);
                }
            }
            catch (Exception ex)
            {
                _reactionImageCommandsRegistered = 0;
                SentrySdk.CaptureException(ex);
            }
        }
    }
    private SlashCommandProperties[] BuildReactionImageCommands(IEnumerable<string> commands)
    {
        return commands.Select(x => new SlashCommandBuilder()
            .WithName(x.ToLower())
            .WithDescription($"Send a random reaction image/gif")
            .AddOption("user-1", ApplicationCommandOptionType.User, "Add user to mention")
            .AddOption("user-2", ApplicationCommandOptionType.User, "Add user to mention")
            .AddOption("user-3", ApplicationCommandOptionType.User, "Add user to mention")
            .AddOption("user-4", ApplicationCommandOptionType.User, "Add user to mention")
            .AddOption("user-5", ApplicationCommandOptionType.User, "Add user to mention")
            .Build()
        ).ToArray();
    }

    private int _commandsRegistered = 0;
    private async Task RegisterCommandModules(DiscordSocketClient client)
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
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                if (interaction is ISlashCommandInteraction slashCommand)
                {
                    var cmd = Interaction.SearchSlashCommand(slashCommand);
                    if (!cmd.IsSuccess)
                    {
                        if (Images.ReactionImageCommands[0].Contains(slashCommand.Data.Name) ||
                            (interaction.GuildId.HasValue &&
                             Images.ReactionImageCommands[interaction.GuildId.Value].Contains(slashCommand.Data.Name)))
                        {
                            string mentions = string.Join(' ', slashCommand.Data.Options.Where(x => x?.Value != null && x.Value is IUser).Select(x => (x as IUser).Mention));

                            var image = ImageDb.GetRandomImage(slashCommand.Data.Name, interaction.GuildId.Value);
                            var embed = ImageUtil.ToEmbed(image).Build();

                            interaction.RespondAsync(mentions, embed: embed);
                        }
                    }
                }
                
                // DO NOT DEFER COMMANDS THAT SEND A MODAL !!! = pain :(
                if (((SocketSlashCommand)interaction).CommandName != "waifu-edit")
                {
                    await interaction.DeferAsync(ephemeral: true);
                }
            }

            var context = new CustomInteractionContext(_client, interaction);
            var result = await Interaction.ExecuteCommandAsync(context, _services);
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction.RespondAsync("Whoops, something went wrong... :eyes:", ephemeral: true);
            }
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
            "nyaa",
            "clap",
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
            "ree",
        };
    }
}