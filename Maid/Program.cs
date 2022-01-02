using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Maid
{

    class Program
    {
        private static DiscordSocketClient Client;
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static readonly CancellationToken ct = cts.Token;


        static void Main(string[] args)
        {
            using Mutex mutex = new Mutex(true, "Global-MaidBot", out bool createdNew);
            if (!createdNew)
            {
                Console.WriteLine("Instance Already Running!");
                return;
            }

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.Retry502,
                ExclusiveBulkDelete = true,
                AlwaysDownloadUsers = false,
                MessageCacheSize = 0,
                LargeThreshold = 50,
                GatewayIntents = GatewayIntents.Guilds |
                    GatewayIntents.GuildMessages
            });

            Client.Ready += Client_Ready;
            Client.MessageReceived += Client_MessageReceived;

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();

            try
            {
                await Task.Delay(-1, ct);
            }
            catch { }
            cts.Dispose();
            Console.WriteLine("Shutting down...");
            await Client.LogoutAsync();
        }

        private static Regex r = new Regex(@"([a-zA-Z\d]+://)?((\w+:\w+@)?([a-zA-Z\d.-]+\.[A-Za-z]{2,4})(:\d+)?(/.*)?)");
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task Client_MessageReceived(SocketMessage msg)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (!Config.ImageChannelIds.Contains(msg.Channel.Id))
                return;

            if (r.IsMatch(msg.Content))
            {
                _ = msg.DeleteAsync();
                var res = msg.Channel.SendMessageAsync(":x: URLs are not allowed, Senpai. Please upload your image/gif directly to the channel.");
                _ = Task.Delay(15 * 1000).ContinueWith(a => res.Result.DeleteAsync());
                return;
            }

            if (msg.Attachments.Any())
            {
                _ = msg.AddReactionAsync(Emote.Parse("<:TickYes:577838859107303424>"));
                _ = msg.AddReactionAsync(Emote.Parse("<:TickNo:577838859077943306>"));
            }
        }

        private async Task Client_Ready()
        {
            var ch = Client.GetChannel(Config.LogChannelId) as SocketTextChannel;
            string msg = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")) switch
            {
                "Development" => "<:NekoHi:620711213826834443> Dev mode >_<",
                _ => $"<:NekoHi:620711213826834443> `{DateTime.Now.ToString("HH:mm:ss")}` Reporting uwu"
            };

            await ch.SendMessageAsync(msg);
        }

        private async Task Giveaway()
        {
            var ch = Client.GetChannel(549341713542414336) as SocketTextChannel;
            var m = await ch.GetMessageAsync(926466939168108574);

            var users = (await m.GetReactionUsersAsync(Emote.Parse("<:NamikoYes:806196730440974376>"), 100).FlattenAsync()).OrderBy(x => Guid.NewGuid()).ToList();
            var me = users.First(x => x.Id == 113677776417980419);

            var proplus = new List<string>
            {
            };

            var guild = new List<string>
            {
            };

            string res = "";

            foreach (var code in proplus)
            {
                try
                {
                    var user = users.FirstOrDefault();
                    users.Remove(user);

                    res += $"{user} - {user.Mention} - `{code}`\n";
                    await user.SendMessageAsync($"Congratulations! You won the giveaway for **Namiko Pro+**!\nYour code is `{code}`.\n\nTo activate it, use Namiko's redeem command: `!redeem {code}`\n\nMore info: <https://namiko.moe/Pro>");
                }
                catch { }
            }

            res += "\n\n";

            foreach (var code in guild)
            {
                try
                {
                    var user = users.FirstOrDefault();
                    users.Remove(user);

                    res += $"{user} - {user.Mention} - `{code}`\n";
                    await user.SendMessageAsync($"Congratulations! You won the giveaway for **Namiko Pro Guild**!\nYour code is `{code}`.\n\nUse Namiko's redeem command in the server that you wish to activate it: `!redeem {code}`\n\nMore info: <https://namiko.moe/Pro>");
                }
                catch { }
            }

            Console.WriteLine(res);
            await me.SendMessageAsync(res);
        }
    }
}
