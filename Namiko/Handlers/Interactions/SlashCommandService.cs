using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Namiko.Handlers.Services;
using Namiko.Handlers.TypeConverters;
using Namiko.Modules.Leaderboard;
using Namiko.Modules.Pro;

namespace Namiko.Addons.Handlers;

public class SlashCommandService
{
    public readonly InteractionService Interaction;
    private readonly IServiceProvider _services;
    private readonly DiscordShardedClient _client;

    private bool CommandsRegistered { get; set; } = false;

    public SlashCommandService(IServiceProvider services)
    {
        _services = services;
        _client = services.GetService<DiscordShardedClient>();
        var logger = services.GetService<Logger>();
        Interaction = new InteractionService(_client, services.GetService<InteractionServiceConfig>());

        Interaction.AddTypeConverter<Waifu>(new WaifuConverter());
        Interaction.AddTypeConverter<ShopWaifu>(new ShopWaifuConverter());


        Interaction.SlashCommandExecuted += AfterSlashCommandExecuted;
        Interaction.Log += logger.Console_Log;

        _client.InteractionCreated += ExecuteInteraction;
        _client.ShardReady += RegisterCommands;
    }

    private async Task RegisterCommands(DiscordSocketClient client)
    {
        if (!CommandsRegistered)
        {
            // await Interaction.AddModuleAsync(typeof(Banroulettes), _services);
            // await Interaction.AddModuleAsync(typeof(Banroyales), _services);
            await Interaction.AddModuleAsync(typeof(Basic), _services);
            await Interaction.AddModuleAsync(typeof(Leaderboards), _services);
            await Interaction.AddModuleAsync(typeof(Currency), _services);
            await Interaction.AddModuleAsync(typeof(Images), _services);
            await Interaction.AddModuleAsync(typeof(Roles), _services);
            await Interaction.AddModuleAsync(typeof(ServerModule), _services);
            await Interaction.AddModuleAsync(typeof(User), _services);
            await Interaction.AddModuleAsync(typeof(Waifus), _services);
            await Interaction.AddModuleAsync(typeof(WaifuEditing), _services);
            await Interaction.AddModuleAsync(typeof(Web), _services);
            await Interaction.AddModuleAsync(typeof(Music), _services);
            await Interaction.AddModuleAsync(typeof(Pro), _services);
            
            if (true)
            {
                await Interaction.AddModulesToGuildAsync(418900885079588884, true, Interaction.Modules.ToArray());
            }
            else
            {
                await Interaction.AddModulesGloballyAsync(true,
                    Interaction.Modules.Where(x => x.Name != nameof(WaifuEditing)).ToArray());
                await Interaction.AddModulesToGuildAsync(418900885079588884, true,
                    Interaction.Modules.FirstOrDefault(x => x.Name == nameof(WaifuEditing)));
            }

            CommandsRegistered = true;
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
}