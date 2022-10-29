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
    private const string TickYes = "<:TickYes:577838859107303424>";
    private const string TickNo = "<:TickNo:577838859077943306>";
    
    
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
    private async Task<bool> RegisterReactionImageCommands(SocketGuild guild, SlashCommandProperties[] sharedCommands, bool logError = true)
    {
        try
        {
            sharedCommands ??= BuildReactionImageCommands(_reactionCommands);

            if (Images.ReactionImageCommands.ContainsKey(guild.Id))
            {
                var guildImages = Images.ReactionImageCommands[guild.Id];
                var commands = BuildReactionImageCommands(guildImages).ToList();
                commands.AddRange(sharedCommands.Where(x => !guildImages.Contains(x.Name.Value)));

                await guild.BulkOverwriteApplicationCommandAsync(commands.ToArray());
                return true;
            }
            else if (guild.Id == 418900885079588884)
            {
                var commands = await guild.GetApplicationCommandsAsync();

                foreach (var cmd in sharedCommands.Take(100 - commands.Count))
                {
                    await Task.Delay(5000);
                    await guild.CreateApplicationCommandAsync(cmd);
                }
                return true;
            }
            else
            {
                await guild.BulkOverwriteApplicationCommandAsync(sharedCommands);
                return true;
            }
        }
        catch (Exception ex)
        {
            await WebhookClients.ErrorLogChannel.SendMessageAsync(
                "Error when registering reaction commands, guild " + guild.Id, embeds: new List<Embed>()
                {
                    new EmbedBuilder().WithDescription("```cs\n" + ex.Message + "```").Build()
                });
            return false;
        }
    }
    private Task RegisterReactionImageCommands(SocketGuild guild) => RegisterReactionImageCommands(guild, null);
    private async Task RegisterReactionImageCommands(DiscordSocketClient client)
    {
        // Making sure this part only runs once, unless an exception is thrown. Thread safe.
        if (_client.Shards.All(x => x.ConnectionState == ConnectionState.Connected) &&
            Interlocked.Exchange(ref _reactionImageCommandsRegistered, 1) == 0)
        {
            int success = 0;
            int fail = 0;
            int error = 0;
            try
            {
                var sharedCommands = BuildReactionImageCommands(_reactionCommands);
                var param = ParamDb.GetParam(0, "ReactionImageSyncDate").FirstOrDefault() ?? new Param()
                {
                    Name = "ReactionImageSyncDate",
                    Date = DateTime.MinValue
                };
                var guilds = ServerDb.GetGuildsJoinedAfterDate(param.Date);

                foreach (var guild in client.Guilds.Where(x => guilds.Contains(x.Id)))
                {
                    await Task.Delay(5000);
                    try
                    {
                        if (await RegisterReactionImageCommands(guild, sharedCommands, logError: false))
                        {
                            success++;
                        }
                        else
                        {
                            fail++;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (error < 1 || error % 50 == 1)
                        {
                            await WebhookClients.NamikoLogChannel.SendMessageAsync(
                                $"{TickNo} Reaction image command registration threw an exception.\n" +
                                $"GuildId: {(guild == null ? "0" : guild.Id)} Success: {success} Fail: {fail} Error: {error}", embeds: new List<Embed>()
                                {
                                    new EmbedBuilder().WithDescription("```cs\n" + ex.Message.ShortenString(3900, 3900) + "```").Build()
                                });
                            SentrySdk.WithScope(scope => scope.SetExtra("guildId", guild == null ? "0" : guild.Id));
                            SentrySdk.CaptureException(ex);
                        }
                        error++;
                    }
                }

                param.Date = DateTime.Now;
                ParamDb.UpdateParam(param);

                await WebhookClients.NamikoLogChannel.SendMessageAsync(
                    $"{TickYes} Reaction images registered successfully. Success: {success} Fail: {fail} Error: {error}");
            }
            catch (Exception ex)
            {
                _reactionImageCommandsRegistered = 0;
                SentrySdk.CaptureException(ex);
                await WebhookClients.NamikoLogChannel.SendMessageAsync(
                    $"{TickNo} Reaction image command registration stopped execution in the finally block.\n" +
                    $"Success: {success} Fail: {fail} Error: {error}", embeds: new List<Embed>()
                    {
                        new EmbedBuilder().WithDescription("```cs\n" + ex.Message.ShortenString(3900, 3900) + "```").Build()
                    });
            }
        }
    }
    private SlashCommandProperties[] BuildReactionImageCommands(IEnumerable<string> commands)
    {
        return commands.Select(x => new SlashCommandBuilder()
            .WithName(x.ToLower())
            .WithDescription($"Send a random reaction image/gif")
            .AddOption("target", ApplicationCommandOptionType.String, "Add a message to the command")
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
                    Interaction.AddModulesToGuildAsync(231113616911237120, true, Interaction.Modules.ToArray());
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
                        ulong guildId = interaction.GuildId.HasValue && Images.ReactionImageCommands.ContainsKey(interaction.GuildId.Value)
                            ? interaction.GuildId.Value
                            : 0;
                        
                        string msg = slashCommand.Data.Options.FirstOrDefault()?.Value.ToString();

                        var image = ImageDb.GetRandomImage(slashCommand.Data.Name, guildId);
                        var embed = ImageUtil.ToEmbed(image).Build();

                        await interaction.RespondAsync(msg, embed: embed, allowedMentions: new AllowedMentions(AllowedMentionTypes.Users));
                        return;
                    }
                }
                
                // DO NOT DEFER COMMANDS THAT SEND A MODAL !!! = pain :(
                if (((SocketSlashCommand)interaction).CommandName != "waifu-edit")
                {
                    await interaction.DeferAsync();
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