using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Model.Exceptions;
using Namiko.Addons.Handlers;
using Namiko.Data;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Namiko.Handlers.Services;
using Namiko.Handlers.TypeConverters;
using Namiko.Modules.Leaderboard;
using Namiko.Modules.Pro;

#pragma warning disable CS1998

namespace Namiko
{
    public class Program
    {
        private static DiscordShardedClient Client;
        private static IServiceProvider Services;
        private static readonly CancellationTokenSource Cts = new();
        private static readonly CancellationToken Ct = Cts.Token;

        public static bool Development;

        static void Main(string[] args)
        {
            using Mutex mutex = new Mutex(true, "Global-NamikoBot", out bool createdNew);
            if (!createdNew)
            {
                Console.WriteLine("Instance Already Running!");
                return;
            }

            using (SentrySdk.Init(options =>
            {
                options.Dsn = new Dsn(AppSettings.SentryWebhook);
                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                options.Environment = env == null || env == "" ? "Production" : env;
            }))
            {
                SetUp();
                new Program().MainAsync().GetAwaiter().GetResult();
            }
        }
        
        
        private async Task MainAsync()
        {
            var logger = new Logger();
            
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Warning,
                DefaultRetryMode = RetryMode.Retry502,
                AlwaysDownloadUsers = false,
                MessageCacheSize = 0,
                LargeThreshold = 250,
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildVoiceStates |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildMessageReactions |
                    GatewayIntents.DirectMessages |
                    GatewayIntents.DirectMessageReactions
            });

            var discord = new DiscordService(Client, logger);

            var commandConfig = new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = Discord.Commands.RunMode.Async,
                LogLevel = LogSeverity.Warning
            };

            var interactionConfig = new InteractionServiceConfig
            {
                LogLevel = LogSeverity.Warning,
                WildCardExpression = "*",
            };
            
            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton<BaseSocketClient>(Client)
                .AddSingleton<DiscordService>(discord)
                .AddSingleton(interactionConfig)
                .AddSingleton<SlashCommandService>()
                .AddSingleton(commandConfig)
                .AddSingleton<TextCommandService>()
                .AddSingleton<InteractiveService>()
                .AddSingleton(logger)
                .AddSingleton<MusicService>(new MusicService(Client, logger))
                .AddSingleton<TimerService>(new TimerService(discord))
                .BuildServiceProvider();

            // fetch the command services so that their constructor is called and they are activated
            Services.GetService<SlashCommandService>();
            Services.GetService<TextCommandService>();

            await Client.LoginAsync(TokenType.Bot, AppSettings.Token);
            await Client.StartAsync();
            _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
                $"------------------------------\n" +
                $"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `Logged in`\n" +
                $"------------------------------");

            try
            {
                await Task.Delay(-1, Ct);
            }
            catch { }
            Cts.Dispose();
            Console.WriteLine("Shutting down...");
            await Client.LogoutAsync();
        }

        
        
        // SET-UP  

        private static void SetUp()
        {
            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Development":
                    Development = true;
                    break;
                default:
                    Development = false;
                    _ = ImgurAPI.ImgurSetup();
                    break;
            }
            NamikoDbContext.ConnectionString = AppSettings.ConnectionString;
            _ = LootboxStats.Reload(Locations.LootboxStatsJSON);
            Waifu.Path = AppSettings.ImageUrlPath + "waifus/";
        }
        

        // RANDOM
        
        public static CancellationTokenSource GetCts()
        {
            return Cts;
        }
    }
}
