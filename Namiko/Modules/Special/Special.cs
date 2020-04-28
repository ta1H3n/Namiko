using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Namiko
{
    public class Special : InteractiveBase<ShardedCommandContext>
    {
        static ISocketMessageChannel ch;

        [Command("SetSayCh"), Alias("ssch"), OwnerPrecondition]
        public async Task SetSayChannel(ulong id)
        {
            ch = Context.Client.GetChannel(id) as ISocketMessageChannel;
            await Context.Channel.SendMessageAsync($"{ch.Name} set as say channel.");
        }

        [Command("SayCh"), Alias("sch"), OwnerPrecondition]
        public async Task Say([Remainder] string str)
        {
            if (ch == null)
            {
                ch = Context.Client.GetChannel(417064769309245475) as ISocketMessageChannel;
            }
            await ch.SendMessageAsync(str);
        }

        [Command("Say"), OwnerPrecondition]
        public async Task SayChannel(ulong id, [Remainder] string str)
        {
            ISocketMessageChannel ch = Context.Client.GetChannel(id) as ISocketMessageChannel;
            await ch.SendMessageAsync(str);
            await Context.Channel.SendMessageAsync($"Saying in {ch.Name}:\n\n{str}");
        }

        [Command("Sayd"), Alias("sd"), OwnerPrecondition]
        public async Task SayDelete([Remainder] string str)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(str);
        }

        [Command("Playing"), Summary("Sets the playing status."), OwnerPrecondition]
        public async Task Playing([Remainder] string str)
        {
            await Context.Client.SetGameAsync(str);
        }

        [Command("Freeze"), Summary("Pauses or Unpauses the bot"), OwnerPrecondition]
        public async Task Pause([Remainder] string str = "")
        {
            var pause = Program.SetPause();
            await Context.Channel.SendMessageAsync($"Pause = {pause}");
        }

        [Command("SQL"), Summary("Executes an SQL query. DANGEROUS"), OwnerPrecondition]
        public async Task Sql([Remainder] string str = "")
        {
            try
            {
                int res = await SqliteDbContext.ExecuteSQL(str);
                await Context.Channel.SendMessageAsync($"{res} rows affected.");
            } catch (Exception ex)
            {
                string err = $"Thrown: `{ex.Message}`\n";
                if (ex.InnerException != null)
                    err += $"Inner: `{ex.InnerException.Message}`";
                await Context.Channel.SendMessageAsync(err);
            }
        }

        [Command("SSQL"), Summary("Executes an SQL query on the stats db. DANGEROUS"), OwnerPrecondition]
        public async Task SSql([Remainder] string str = "")
        {
            try
            {
                int res = await SqliteStatsDbContext.ExecuteSQL(str);
                await Context.Channel.SendMessageAsync($"{res} rows affected.");
            }
            catch (Exception ex)
            {
                string err = $"Thrown: `{ex.Message}`\n";
                if (ex.InnerException != null)
                    err += $"Inner: `{ex.InnerException.Message}`";
                await Context.Channel.SendMessageAsync(err);
            }
        }

        [Command("SQLGET"), Summary("Executes an SQL GET query. DANGEROUS"), OwnerPrecondition]
        public async Task SqlGet([Remainder] string str = "")
        {
            try
            {
                using var db = new SqliteDbContext();
                var list = db.DynamicListFromSql(str, new Dictionary<string, object>());

                string text = $"Results: {list.Count()}\n";
                text += $"```yaml\n";
                foreach (var item in list)
                {
                    foreach (var row in item)
                    {
                        text += row.Key + ": " + row.Value + "\n";
                    }
                    text += "\n";
                }
                text += "```";

                if (text.Length > 2000)
                    text = text.Substring(0, 1990) + "\n...```";
                await Context.Channel.SendMessageAsync(text);
            }
            catch (Exception ex)
            {
                string err = $"Thrown: `{ex.Message}`\n";
                if (ex.InnerException != null)
                    err += $"Inner: `{ex.InnerException.Message}`";
                await Context.Channel.SendMessageAsync(err);
            }
        }

        [Command("SSQLGET"), Summary("Executes an SQL GET query on the stats db. DANGEROUS"), OwnerPrecondition]
        public async Task SSqlGet([Remainder] string str = "")
        {
            try
            {
                using var db = new SqliteStatsDbContext();
                var list = db.DynamicListFromSql(str, new Dictionary<string, object>());

                string text = $"Results: {list.Count()}\n";
                text += $"```yaml\n";
                foreach (var item in list)
                {
                    foreach (var row in item)
                    {
                        text += row.Key + ": " + row.Value + "\n";
                    }
                    text += "\n";
                }
                text += "```";

                if (text.Length > 2000)
                    text = text.Substring(0, 1990) + "\n...```";
                await Context.Channel.SendMessageAsync(text);
            }
            catch (Exception ex)
            {
                string err = $"Thrown: `{ex.Message}`\n";
                if (ex.InnerException != null)
                    err += $"Inner: `{ex.InnerException.Message}`";
                await Context.Channel.SendMessageAsync(err);
            }
        }

        [Command("Die"), Summary("Kills Namiko"), Insider]
        public async Task Die()
        {
            var tasks = new List<Task>();
            tasks.Add(WebhookClients.NamikoLogChannel.SendMessageAsync($"`{DateTime.Now.ToString("HH:mm:ss")}` {Context.Client.CurrentUser.Username} killed by {Context.User.Mention} :gun:"));

            foreach (var player in Music.Node.Players)
            {
                try
                {
                    tasks.Add(player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava().WithDescription("Gomen, Senpai. I have to Disconnect the player due to server restart.\n" +
                        "You can restart the player once I am back online in a few minutes.").Build()));
                    tasks.Add(Music.Node.LeaveAsync(player.VoiceChannel));
                } 
                catch (Exception ex) { SentrySdk.CaptureException(ex); }
            }

            await Task.WhenAll(tasks);

            var cts = Program.GetCts();
            await Context.Client.StopAsync();
            cts.Cancel();
        }

        [Command("GetInvite"), Summary("Gets an invite to a server"), OwnerPrecondition]
        public async Task GetInvite(ulong id, [Remainder] string str = "")
        {
            var guild = Context.Client.GetGuild(id);
            var invite = (await guild.GetInvitesAsync()).FirstOrDefault();
            await Context.Channel.SendMessageAsync(invite == null ? "Nada." : invite.Url);
        }

        [Command("CreateInvite"), Summary("Creates an invite to a server"), OwnerPrecondition]
        public async Task CreateInvite(ulong id, [Remainder] string str = "")
        {
            var guild = Context.Client.GetGuild(id);
            var invite = guild.TextChannels.FirstOrDefault();
            await Context.Channel.SendMessageAsync(invite == null ? "Nada." : (await invite.CreateInviteAsync()).Url);
        }

        [Command("NewWelcome"), Alias("nwlc"), Summary("Adds a new welcome message. @_ will be replaced with a mention.\n**Usage**: `!nw [welcome]`"), Insider]
        public async Task NewWelcome([Remainder] string message)
        {
            if (message.Length < 20)
            {
                await Context.Channel.SendMessageAsync("Message must be longer than 20 characters.");
                return;
            }
            await WelcomeMessageDb.AddMessage(message);
            await Context.Channel.SendMessageAsync("Message added: '" + message.Replace("@_", Context.User.Mention) + "'");
        }

        [Command("DeleteWelcome"), Alias("dw", "delwelcome"), Summary("Deletes a welcome message by ID.\n**Usage**: `!dw [id]`"), Insider]
        public async Task DeleteWelcome(int id)
        {

            WelcomeMessage message = WelcomeMessageDb.GetMessage(id);
            if (message == null)
                await Context.Channel.SendMessageAsync($"Message with id: {id} not found");
            else
            {
                await WelcomeMessageDb.DeleteMessage(id);
                await Context.Channel.SendMessageAsync($"Deleted welcome message with id: {id}");
            }
        }

        [Command("StartLavalink"), Summary("Starts Lavalink.\n**Usage**: `!join`"), Insider]
        public async Task Init([Remainder]string str = "")
        {
            await Music.Initialize(Program.GetClient());
            await ReplyAsync("Done.");
        }

        [Command("SendLootboxes"), OwnerPrecondition]
        public async Task SendLootboxes()
        {
            var voters = (await WebUtil.GetVotersAsync()).Select(x => x.Id).Distinct();
            int sent = await Timers.SendRewards(voters);

            Context.Channel.SendMessageAsync($"Broadcasted to {sent}/{voters.Count()} users.");
        }

        [Command("MessageVoters"), OwnerPrecondition]
        public async Task MessageVoters([Remainder] string msg = "")
        {
            var voters = (await WebUtil.GetVotersAsync()).Select(x => x.Id).Distinct();

            int sent = 0;
            foreach (var x in voters)
            {
                try
                {
                    var ch = await Program.GetClient().GetUser(x).GetOrCreateDMChannelAsync();
                    await ch.SendMessageAsync(msg);
                    sent++;
                }
                catch { }
            }

            Context.Channel.SendMessageAsync($"Broadcasted to {sent}/{voters.Count()} users.");
        }

        [Command("Debug"), OwnerPrecondition]
        public async Task Debug([Remainder] string msg = "")
        {
            var commands = Program.GetCommands();
            var processed = System.DateTime.Now.AddMinutes(-10);

            async Task listen(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
            {
                if (arg2.Message.Equals(Context.Message) && arg1.Value.Name != "Debug")
                {
                    processed = System.DateTime.Now;
                    await Task.Delay(1);
                }
                Console.WriteLine("Command debugger");
            }
            commands.CommandExecuted += listen;

            var received = System.DateTime.Now;
            string prefix = Program.GetPrefix(Context);
            int ArgPos = 0;
            bool isPrefixed = Context.Message.HasStringPrefix(prefix + "debug ", ref ArgPos);
            var result = await commands.ExecuteAsync(Context, ArgPos, Program.GetServices());

            //var processed = System.DateTime.Now;
            var message = await Context.Channel.SendMessageAsync("`Counting...`");
            await Task.Delay(5000);
            commands.CommandExecuted -= listen;

            //var receivedms = received - Context.Message.CreatedAt;
            var processedms = processed - received;
            //var sentms = message.CreatedAt - processed;

            message.ModifyAsync(x => x.Content = $"" +
            //$"Discord -> Namiko: `{receivedms.TotalMilliseconds}ms`\n" +
            $"Random Number: `{processedms.TotalMilliseconds.ToString("n0")}ms`\n" +
            //$"Namiko -> Discord: `{sentms.TotalMilliseconds}ms`" +
            "");
        }

        [Command("ImgurAuth"), OwnerPrecondition]
        public async Task ImgurAuth([Remainder] string msg = "")
        {
            await Context.Channel.SendMessageAsync(ImgurAPI.GetAuthorizationUrl());
        }

        [Command("SetImgurRefreshToken"), Alias("sirt"), OwnerPrecondition]
        public async Task SetImgurRefreshToken(string refreshToken, [Remainder] string msg = "")
        {
            ImgurAPI.SetRefreshToken(refreshToken);
            await Context.Channel.SendMessageAsync("Done.");
        }

        [Command("TestEmbed"), OwnerPrecondition] 
        public async Task TestEmbed(string name)
        {
            var eb = await JsonHelper.ReadJson<EmbedBuilder>(Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", $@"embeds/{name}"));
            eb.WithColor(BasicUtil.RandomColor());
            await ReplyAsync(embed: eb.Build());
        }

        [Command("GetEmbedJson"), OwnerPrecondition]
        public async Task GetEmbedJson(string name)
        {
            var eb = await JsonHelper.ReadJson<EmbedBuilder>(Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", $@"embeds/{name}"));
            string str = JsonConvert.SerializeObject(eb, Formatting.Indented);
            await ReplyAsync($"```json\n{str}```");
        }

        [Command("SaveEmbedJson"), OwnerPrecondition]
        public async Task SaveEmbedJson(string name, [Remainder] string json)
        {
            EmbedBuilder eb = JsonConvert.DeserializeObject<EmbedBuilder>(json, new JsonSerializerSettings { Formatting = Formatting.Indented });
            await JsonHelper.SaveJson(eb, Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", $@"embeds/{name}"));
            await ReplyAsync(embed: eb.Build());
        }

        [Command("ListEmbeds"), OwnerPrecondition]
        public async Task ListEmbeds([Remainder] string json = "")
        {
            string root = Assembly.GetEntryAssembly().Location.Replace("Namiko.dll", "embeds/");
            string[] paths = Directory.GetFiles(root);
            string str = "";
            foreach (string s in paths)
            {
                var split = s.Split("embeds/");
                str += split.LastOrDefault() + "\n";
            }
            await ReplyAsync(str);
        }

        [Command("TestAnnouncement"), OwnerPrecondition]
        public async Task TestAnnouncement(string name, int days)
        {
            var eb = await JsonHelper.ReadJson<EmbedBuilder>(Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", $@"embeds/{name}"));
            eb.WithColor(BasicUtil.RandomColor());
            using var db = new SqliteDbContext();
            var voters = db.Voters.Where(x => x.Date > DateTime.Now.AddDays(-days)).Select(x => x.UserId).ToHashSet();
            int votes = db.Voters.Where(x => x.Date > DateTime.Now.AddDays(-days)).Count();
            await ReplyAsync($"Sending this to {voters.Count} users. Votes - {votes}");
            await ReplyAsync(embed: eb.Build());
        }

        [Command("SendAnnouncement"), OwnerPrecondition]
        public async Task SendAnnouncement(string name, int days)
        {
            var eb = await JsonHelper.ReadJson<EmbedBuilder>(Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", $@"embeds/{name}"));
            eb.WithColor(BasicUtil.RandomColor());
            using var db = new SqliteDbContext();
            var voters = db.Voters.Where(x => x.Date > DateTime.Now.AddDays(-days)).Select(x => x.UserId).ToHashSet();
            int votes = db.Voters.Where(x => x.Date > DateTime.Now.AddDays(-days)).Count();
            await ReplyAsync($"Sending this to {voters.Count} users. Votes - {votes}");
            await ReplyAsync(embed: eb.Build());
            var client = Program.GetClient();
            var embed = eb.Build();

            int i = 0;
            foreach (var id in voters)
            {
                try
                {
                    var ch = await client.GetUser(id).GetOrCreateDMChannelAsync();
                    await ch.SendMessageAsync(embed: embed);
                    i++;
                }
                catch { }
            }

            await ReplyAsync($"Delivered to {i} users.");
        }

        [Command("SendEmbed"), OwnerPrecondition]
        public async Task SendEmbed(string name, ulong id)
        {
            var eb = await JsonHelper.ReadJson<EmbedBuilder>(Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", $@"embeds/{name}"));
            eb.WithColor(BasicUtil.RandomColor());

            ISocketMessageChannel ch = Context.Client.GetChannel(id) as ISocketMessageChannel;
            await ch.SendMessageAsync(embed: eb.Build());
            await Context.Channel.SendMessageAsync($"Saying in {ch.Name}", false, embed: eb.Build());
        }

        [Command("DownloadFiles"), Summary("Downloads files from a channel.\n**Usage**: `!DownloadImages [path] [amount] [skip] [channel_id}`"), OwnerPrecondition]
        public async Task DownloadFiles(string path, int amount, int skip = 0, ulong channelId = 0)
        {
            await Context.Message.DeleteAsync();
            var ch = channelId == 0 ? Context.Channel : (ISocketMessageChannel) Context.Client.GetChannel(channelId);
            var messages = await ch.GetMessagesAsync(amount, CacheMode.AllowDownload).FlattenAsync();

            int total = 0;
            int downloaded = 0;

            foreach (var msg in messages.Skip(skip))
            {
                if (msg.Type != MessageType.Default)
                {
                    continue;
                }

                foreach (var attachment in msg.Attachments.Where(x => x.Height != null && x.Width != null))
                {
                    if (WebUtil.IsValidUrl(attachment.ProxyUrl))
                    {
                        Task.Run(() => DownloadFile(attachment.ProxyUrl, 
                            path + @"\" 
                            + msg.Timestamp.UtcDateTime.ToString("yyyy-MM-dd_HHmm") + "_" 
                            + attachment.Filename))
                            .ContinueWith(x =>
                             {
                                 if (x.Result == true)
                                 {
                                     downloaded++;
                                     if (downloaded % 5 == 0)
                                     {
                                         Console.WriteLine("Downloaded: " + downloaded);
                                     }
                                 }
                             });
                        total++;
                        if (total % 50 == 0)
                        {
                            Console.WriteLine("Total: " + total);
                        }
                    }
                }

                foreach (var word in msg.Content.Split(' '))
                {
                    if (WebUtil.IsValidUrl(word) && WebUtil.IsImageUrl(word))
                    {
                        Task.Run(() => DownloadFile(word,
                            path + @"\"
                            + msg.Timestamp.UtcDateTime.ToString("yyyy-MM-dd_HHmm") + "_"
                            + word.Split(@"/").Last()))
                            .ContinueWith(x =>
                            {
                                if (x.Result == true)
                                {
                                    downloaded++;
                                    if (downloaded % 5 == 0)
                                    {
                                        Console.WriteLine("Downloaded: " + downloaded);
                                    }
                                }
                            });
                        total++;
                        if (total % 50 == 0)
                        {
                            Console.WriteLine("Total: " + total);
                        }
                    }
                }
            }
            Console.WriteLine("Total: " + total);
        }

        public bool DownloadFile(string url, string path)
        {
            if (File.Exists(path))
                return false;

            try
            {
                using var client = new WebClient
                {
                    Credentials = new NetworkCredential("UserName", "Password")
                };
                client.DownloadFile(url, path);
                return true;
            } catch
            {
                Console.WriteLine("Failed: " + url);
                return false;
            }
        }
    }
}
