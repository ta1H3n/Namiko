using System;
using System.Linq;
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
    private readonly BaseSocketClient _client;

    private bool CommandsRegistered { get; set; } = false;

    public SlashCommandService(IServiceProvider services)
    {
        _services = services;
        _client = services.GetService<BaseSocketClient>();
        var logger = services.GetService<Logger>();
        Interaction = new InteractionService(_client, services.GetService<InteractionServiceConfig>());

        Interaction.AddTypeConverter<Waifu>(new WaifuConverter());
        Interaction.AddTypeConverter<ShopWaifu>(new ShopWaifuConverter());

        //await Interactions.AddModuleAsync(typeof(Banroulettes), Services);
        //await Interactions.AddModuleAsync(typeof(Banroyales), Services);
        Interaction.AddModuleAsync(typeof(Basic), services);
        Interaction.AddModuleAsync(typeof(Currency), services);
        Interaction.AddModuleAsync(typeof(Images), services);
        Interaction.AddModuleAsync(typeof(Roles), services);
        Interaction.AddModuleAsync(typeof(ServerModule), services);
        Interaction.AddModuleAsync(typeof(User), services);
        Interaction.AddModuleAsync(typeof(Waifus), services);
        Interaction.AddModuleAsync(typeof(WaifuEditing), services);
        Interaction.AddModuleAsync(typeof(Web), services);
        Interaction.AddModuleAsync(typeof(Music), services);
        Interaction.AddModuleAsync(typeof(Leaderboards), services);
        Interaction.AddModuleAsync(typeof(Pro), services);

        Interaction.SlashCommandExecuted += AfterSlashCommandExecuted;
        Interaction.Log += logger.Console_Log;

        _client.InteractionCreated += ExecuteInteraction;
        _client.LoggedIn += RegisterCommands;
    }

    private async Task RegisterCommands()
    {
        if (!CommandsRegistered)
        {
            if (true)
            {
                await Interaction.AddModulesToGuildAsync(418900885079588884);
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