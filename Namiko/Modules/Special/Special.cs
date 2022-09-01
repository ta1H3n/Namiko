using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Models.Users;
using Namiko.Addons.Handlers;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Namiko.Modules.Basic;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PreconditionAttribute = Namiko.Handlers.Attributes.PreconditionAttribute;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Namiko
{
    public class Special : CustomModuleBase<ICustomContext>
    {
        public DiscordShardedClient Client { get; set; }
        public TextCommandService TextCommands { get; set; }
        static ISocketMessageChannel ch;

        [Command("SetSayCh"), Alias("ssch"), OwnerPrecondition]
        public async Task SetSayChannel(ulong id)
        {
            ch = Context.Client.GetChannel(id) as ISocketMessageChannel;
            await ReplyAsync($"{ch.Name} set as say channel.");
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
            await ReplyAsync($"Saying in {ch.Name}:\n\n{str}");
        }

        [Command("Playing"), Description("Sets the playing status."), OwnerPrecondition]
        public async Task Playing([Remainder] string str)
        {
            await Context.Client.SetGameAsync(str);
        }

        // [Command("Freeze"), Description("Pauses or Unpauses the bot"), OwnerPrecondition]
        // public async Task Pause()
        // {
        //     var pause = Program.SetPause();
        //     await ReplyAsync($"Pause = {pause}");
        // }

        [Command("SQL"), Description("Executes an SQL query. DANGEROUS"), OwnerPrecondition]
        public async Task Sql([Remainder] string str = "")
        {
            try
            {
                int res = await NamikoDbContext.ExecuteSQL(str);
                await ReplyAsync($"{res} rows affected.");
            } catch (Exception ex)
            {
                string err = $"Thrown: `{ex.Message}`\n";
                if (ex.InnerException != null)
                    err += $"Inner: `{ex.InnerException.Message}`";
                await ReplyAsync(err);
            }
        }

        [Command("SSQL"), Description("Executes an SQL query on the stats db. DANGEROUS"), OwnerPrecondition]
        public async Task SSql([Remainder] string str = "")
        {
            try
            {
                int res = await StatsDbContext.ExecuteSQL(str);
                await ReplyAsync($"{res} rows affected.");
            }
            catch (Exception ex)
            {
                string err = $"Thrown: `{ex.Message}`\n";
                if (ex.InnerException != null)
                    err += $"Inner: `{ex.InnerException.Message}`";
                await ReplyAsync(err);
            }
        }

        [Command("SQLGET"), Description("Executes an SQL GET query. DANGEROUS"), OwnerPrecondition]
        public async Task SqlGet([Remainder] string str = "")
        {
            try
            {
                using var db = new NamikoDbContext();
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
                await ReplyAsync(text);
            }
            catch (Exception ex)
            {
                string err = $"Thrown: `{ex.Message}`\n";
                if (ex.InnerException != null)
                    err += $"Inner: `{ex.InnerException.Message}`";
                await ReplyAsync(err);
            }
        }

        [Command("SSQLGET"), Description("Executes an SQL GET query on the stats db. DANGEROUS"), OwnerPrecondition]
        public async Task SSqlGet([Remainder] string str = "")
        {
            try
            {
                using var db = new StatsDbContext();
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
                await ReplyAsync(text);
            }
            catch (Exception ex)
            {
                string err = $"Thrown: `{ex.Message}`\n";
                if (ex.InnerException != null)
                    err += $"Inner: `{ex.InnerException.Message}`";
                await ReplyAsync(err);
            }
        }

        [Command("Die"), Description("Kills Namiko"), Insider]
        public async Task Die()
        {
            var tasks = new List<Task>();
            tasks.Add(WebhookClients.NamikoLogChannel.SendMessageAsync($"`{DateTime.Now.ToString("HH:mm:ss")}` {Context.Client.CurrentUser.Username} killed by {Context.User.Mention} :gun:"));

            foreach (var player in Music.Node.Players)
            {
                try
                {
                    await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava().WithDescription("Gomen, Senpai. I have to Disconnect the player due to server restart.\n" +
                        "You can restart the player once I am back online in a few minutes.").Build());
                    await Music.Node.LeaveAsync(player.VoiceChannel);
                } 
                catch (Exception ex) { SentrySdk.CaptureException(ex); }
            }

            var cts = Program.GetCts();
            await Context.Client.StopAsync();
            cts.Cancel();
        }

        [Command("GetInvite"), Description("Gets an invite to a server"), OwnerPrecondition]
        public async Task GetInvite(ulong id)
        {
            var guild = Context.Client.GetGuild(id);
            var invite = (await guild.GetInvitesAsync()).FirstOrDefault();
            await ReplyAsync(invite == null ? "Nada." : invite.Url);
        }

        [Command("CreateInvite"), Description("Creates an invite to a server"), OwnerPrecondition]
        public async Task CreateInvite(ulong id)
        {
            var guild = Context.Client.GetGuild(id);
            var invite = guild.TextChannels.FirstOrDefault();
            await ReplyAsync(invite == null ? "Nada." : (await invite.CreateInviteAsync()).Url);
        }

        [Command("NewWelcome"), Alias("nwlc"), Description("Adds a new welcome message. @_ will be replaced with a mention.\n**Usage**: `!nw [welcome]`"), Insider]
        public async Task NewWelcome([Remainder] string message)
        {
            if (message.Length < 20)
            {
                await ReplyAsync("Message must be longer than 20 characters.");
                return;
            }
            await WelcomeMessageDb.AddMessage(message);
            await ReplyAsync("Message added: '" + message.Replace("@_", Context.User.Mention) + "'");
        }

        [Command("DeleteWelcome"), Alias("dw", "delwelcome"), Description("Deletes a welcome message by ID.\n**Usage**: `!dw [id]`"), Insider]
        public async Task DeleteWelcome(int id)
        {

            WelcomeMessage message = WelcomeMessageDb.GetMessage(id);
            if (message == null)
                await ReplyAsync($"Message with id: {id} not found");
            else
            {
                await WelcomeMessageDb.DeleteMessage(id);
                await ReplyAsync($"Deleted welcome message with id: {id}");
            }
        }

        [Command("StartLavalink"), Description("Starts Lavalink.\n**Usage**: `!join`"), Insider]
        public async Task Init([Remainder]string str = "")
        {
            await Music.Initialize(Client);
            await ReplyAsync("Done.");
        }

        [Command("SendLootboxes"), OwnerPrecondition]
        public async Task SendLootboxes()
        {
            var voters = (await WebUtil.GetVotersAsync(Client)).Select(x => x.Id).Distinct();
            int sent = await Timers.SendRewards(voters);

            ReplyAsync($"Broadcasted to {sent}/{voters.Count()} users.");
        }

        [Command("MessageVoters"), OwnerPrecondition]
        public async Task MessageVoters([Remainder] string msg = "")
        {
            var voters = (await WebUtil.GetVotersAsync(Client)).Select(x => x.Id).Distinct();

            int sent = 0;
            foreach (var x in voters)
            {
                try
                {
                    var ch = await Client.GetUser(x).CreateDMChannelAsync();
                    await ch.SendMessageAsync(msg);
                    sent++;
                }
                catch { }
            }

            ReplyAsync($"Broadcasted to {sent}/{voters.Count()} users.");
        }

        [Command("ImgurAuth"), OwnerPrecondition]
        public async Task ImgurAuth([Remainder] string msg = "")
        {
            await ReplyAsync(ImgurAPI.GetAuthorizationUrl());
        }

        [Command("SetImgurRefreshToken"), Alias("sirt"), OwnerPrecondition]
        public async Task SetImgurRefreshToken(string refreshToken, [Remainder] string msg = "")
        {
            ImgurAPI.SetRefreshToken(refreshToken);
            await ReplyAsync("Done.");
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
            using var db = new NamikoDbContext();
            var voters = db.Voters.AsQueryable().Where(x => x.Date > DateTime.Now.AddDays(-days)).Select(x => x.UserId).ToHashSet();
            int votes = db.Voters.AsQueryable().Where(x => x.Date > DateTime.Now.AddDays(-days)).Count();
            await ReplyAsync($"Sending this to {voters.Count} users. Votes - {votes}");
            await ReplyAsync(embed: eb.Build());
        }

        [Command("SendAnnouncement"), OwnerPrecondition]
        public async Task SendAnnouncement(string name, int days)
        {
            var eb = await JsonHelper.ReadJson<EmbedBuilder>(Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", $@"embeds/{name}"));
            eb.WithColor(BasicUtil.RandomColor());
            using var db = new NamikoDbContext();
            var voters = db.Voters.AsQueryable().Where(x => x.Date > DateTime.Now.AddDays(-days)).Select(x => x.UserId).ToHashSet();
            int votes = db.Voters.AsQueryable().Where(x => x.Date > DateTime.Now.AddDays(-days)).Count();
            await ReplyAsync($"Sending this to {voters.Count} users. Votes - {votes}");
            await ReplyAsync(embed: eb.Build());
            var client = Client;
            var embed = eb.Build();

            int i = 0;
            foreach (var id in voters)
            {
                try
                {
                    var ch = await client.GetUser(id).CreateDMChannelAsync();
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
            await ReplyAsync($"Saying in {ch.Name}", false, embed: eb.Build());
        }

        [Command("DownloadFiles"), Description("Downloads files from a channel.\n**Usage**: `!DownloadImages [path] [amount] [skip] [channel_id}`"), OwnerPrecondition]
        public async Task DownloadFiles(string path, int amount, int skip = 0, ulong channelId = 0)
        {
            var context = (ICommandContext)Context;

            await context.Message.DeleteAsync();
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

        [Command("CreateCommandSchema"), Description("Copies command info to the database"), OwnerPrecondition]
        public async Task CreateCommandSchema()
        {
            var cmds = TextCommands;

            var modules = new List<Model.Module>();

            foreach (var module in TextCommands.Commands.Modules)
            {
                var m = new Model.Module
                {
                    Name = module.Name,
                    Commands = new List<Command>()
                };

                foreach (var command in module.Commands)
                {
                    var cmd = new Model.Command();
                    var split = command.Summary == null ? null : command.Summary.Split(new string[] { "\n**Usage**:" }, StringSplitOptions.None);

                    cmd.ModuleName = module.Name;
                    cmd.Name = Regex.Replace(command.Name, "[a-z][A-Z]", m => $"{m.Value[0]} {m.Value[1]}"); ;
                    cmd.Aliases = command.Aliases.Aggregate((x, y) => x + ',' + y);
                    cmd.Conditions = "";

                    if (split != null && split.Count() > 0)
                    {
                        cmd.Description = split[0];
                        if (split.Count() > 1)
                        {
                            cmd.Example = split[1].Replace("`", "");
                        }
                    }

                    foreach (var x in command.Attributes)
                    {
                        if (x is UserPermissionAttribute)
                        {
                            var prec = x as UserPermissionAttribute;
                            cmd.Conditions += $"User:{prec.Name},";
                        }
                        else if (x is BotPermissionAttribute)
                        {
                            var prec = x as BotPermissionAttribute;
                            cmd.Conditions += $"Bot:{prec.Name},";
                        }
                        else if (x is PreconditionAttribute)
                        {
                            var prec = x as PreconditionAttribute;
                            cmd.Conditions += $"{prec.Name},";
                        }
                    }

                    cmd.Conditions = cmd.Conditions.Trim(',');

                    m.Commands.Add(cmd);
                }

                modules.Add(m);
            }

            using var db = new NamikoDbContext();

            db.Modules.RemoveRange(db.Modules.Include(x => x.Commands));
            var res = await db.SaveChangesAsync();

            db.Modules.AddRange(modules);
            res += await db.SaveChangesAsync();

            await ReplyAsync($"Updated db command list. {res} rows affected.");
        }

        [Command("LeaveInactiveGuildsTest"), Description("Rundown of how many guilds are inactive.\n**Usage**: `!LeaveInactiveGuildsTest [inactive_days] [new_servers_days]`"), OwnerPrecondition]
        public async Task LeaveInactiveGuildsTest(int days, int newDays)
        {
            string desc = "";
            using (var statsdb = new StatsDbContext())
            using (var db = new NamikoDbContext())
            {
                var now = DateTime.Now;
                var newServers = db.Servers.AsQueryable().Where(x => x.JoinDate > now.AddDays(-newDays)).Select(x => x.GuildId).Distinct().ToHashSet();
                desc += $"New servers: {newServers.Count}\n" +
                    $"AMFWT: {newServers.Contains(417064769309245471)}\n" +
                    $"Personal: {newServers.Contains(231113616911237120)}\n" +
                    $"NTR: {newServers.Contains(418900885079588884)}\n\n";

                var active = statsdb.CommandLogs.AsQueryable().Where(x => x.Date > now.AddDays(-days)).Select(x => x.GuildId).Distinct().ToHashSet();
                desc += $"Active servers: {active.Count}\n" +
                    $"AMFWT: {active.Contains(417064769309245471)}\n" +
                    $"Personal: {active.Contains(231113616911237120)}\n" +
                    $"NTR: {active.Contains(418900885079588884)}\n\n";

                var guilds = Client.Guilds.ToList();
                desc += $"Joined servers: {guilds.Count}\n" +
                    $"AMFWT: {guilds.Any(x => x.Id == 417064769309245471)}\n" +
                    $"Personal: {guilds.Any(x => x.Id == 231113616911237120)}\n" +
                    $"NTR: {guilds.Any(x => x.Id == 418900885079588884)}\n\n";

                guilds = guilds.Where(x => !active.Contains(x.Id) && !newServers.Contains(x.Id)).ToList();
                desc += $"Filtered servers: {guilds.Count}\n" +
                    $"AMFWT: {guilds.Any(x => x.Id == 417064769309245471)}\n" +
                    $"Personal: {guilds.Any(x => x.Id == 231113616911237120)}\n" +
                    $"NTR: {guilds.Any(x => x.Id == 418900885079588884)}\n\n";
            }

            await ReplyAsync(desc);
        }

        [Command("LeaveInactiveGuilds"), Description("Leave all inactive guilds.\n**Usage**: `!LeaveInactiveGuilds [inactive_days] [new_servers_days] [ms_delay_per_task]`"), OwnerPrecondition]
        public async Task LeaveInactiveGuilds(int days, int newDays, int delay)
        {
            if (days < 14)
            {
                await ReplyAsync("Less than 14 days illegal");
                return;
            }

            string desc = "";
            using (var statsdb = new StatsDbContext())
            using (var db = new NamikoDbContext())
            {
                var now = DateTime.Now;
                var newServers = db.Servers.AsQueryable().Where(x => x.JoinDate > now.AddDays(-newDays)).Select(x => x.GuildId).Distinct().ToHashSet();
                desc += $"New servers: {newServers.Count}\n" +
                    $"AMFWT: {newServers.Contains(417064769309245471)}\n" +
                    $"Personal: {newServers.Contains(231113616911237120)}\n" +
                    $"NTR: {newServers.Contains(418900885079588884)}\n\n";

                var active = statsdb.CommandLogs.AsQueryable().Where(x => x.Date > now.AddDays(-days)).Select(x => x.GuildId).Distinct().ToHashSet();
                desc += $"Active servers: {active.Count}\n" +
                    $"AMFWT: {active.Contains(417064769309245471)}\n" +
                    $"Personal: {active.Contains(231113616911237120)}\n" +
                    $"NTR: {active.Contains(418900885079588884)}\n\n";

                var guilds = Client.Guilds.ToList();
                desc += $"Joined servers: {guilds.Count}\n" +
                    $"AMFWT: {guilds.Any(x => x.Id == 417064769309245471)}\n" +
                    $"Personal: {guilds.Any(x => x.Id == 231113616911237120)}\n" +
                    $"NTR: {guilds.Any(x => x.Id == 418900885079588884)}\n\n";

                guilds = guilds.Where(x => !active.Contains(x.Id) && !newServers.Contains(x.Id)).ToList();
                desc += $"Filtered servers: {guilds.Count}\n" +
                    $"AMFWT: {guilds.Any(x => x.Id == 417064769309245471)}\n" +
                    $"Personal: {guilds.Any(x => x.Id == 231113616911237120)}\n" +
                    $"NTR: {guilds.Any(x => x.Id == 418900885079588884)}\n\n";

                desc += "Leaving filtered guilds...";
                await ReplyAsync(desc);

                Program.GuildLeaveEvent = false;

                if (guilds.Count >= Client.Guilds.Count)
                {
                    await ReplyAsync("Filtered same or higher than all. Cancelling.");
                    return;
                }
                int s = 0;
                int f = 0;
                int dm = 0;
                foreach (var guild in guilds)
                {
                    try
                    {
                        await guild.Owner.SendMessageAsync($"I am leaving **{guild.Name}** due to {days}+ days of inactivity. All data like user balances related to that server will be deleted in 3 days.\n" +
                            $"You can re-invite me using this link: {LinkHelper.BotInvite}");
                        dm++;
                    }
                    catch { }
                    try
                    {
                        await guild.LeaveAsync();
                        s++;
                    }
                    catch { }

                    if ((s + f) % 100 == 0)
                    {
                        try
                        {
                            _ = ReplyAsync($"Left: {s}\n" +
                                $"Failed: {f}\n" +
                                $"Dms: {dm}\n" +
                                $"Remaining: {guilds.Count - s - f}");
                        }
                        catch { }
                    }

                    await Task.Delay(delay);
                }

                await ReplyAsync($"Left: {s}\n" +
                    $"Failed: {f}\n" +
                    $"Dms: {dm}\n" +
                    $"Done.");

                Program.GuildLeaveEvent = true;
            }
        }

        [Command("GuildLeaveEvent"), Description("Set guild leave tracking."), OwnerPrecondition]
        public async Task GuildLeaveEvent()
        {
            Program.GuildLeaveEvent = !Program.GuildLeaveEvent;
            await ReplyAsync(Program.GuildLeaveEvent.ToString());
        }

        [Command("TestCode"), Description("Test code."), OwnerPrecondition]
        public async Task GenerateCode(string code)
        {
            string desc = "```yaml\n";
            var example = await PremiumCodeDb.TestCode(code);
            foreach (var prop in example.GetProperties())
            {
                desc += prop.Key + ": " + prop.Value + "\n";
            }
            desc += "```";

            await ReplyAsync(desc);
        }

        [Command("GenerateCodes"), Description("Generate trial codes. 0 to pass null into optionals.\n**Usage**: `!GenerateCodes ProType type, int durationDays, int useAmount, int codeAmount, string prefix, string id, int codeExpiresInDays`"), OwnerPrecondition]
        public async Task GenerateCode(ProType type, int durationDays, int useAmount, int codeAmount, string prefix, string id, int codeExpiresInDays)
        {
            prefix = prefix == "0" ? null : prefix;
            id = id == "0" ? null : id;
            DateTime? expires = null;
            if (codeExpiresInDays != 0)
                expires = DateTime.Now.AddDays(codeExpiresInDays);

            var res = await PremiumCodeDb.GenerateCodes(type, durationDays, useAmount, codeAmount, prefix, id, expires);
            var example = res.FirstOrDefault();
            string desc = "```yaml\n";
            foreach (var prop in example.GetProperties())
            {
                desc += prop.Key + ": " + prop.Value + "\n";
            }
            desc += "```\n```yaml\n";
            foreach(var code in res.Select(x => x.Id))
            {
                desc += code + "\n";
            }
            desc += "```";

            await ReplyAsync(desc);
        }

        [Command("GetVoters")]
        public async Task GetVoters(ulong botId = 0)
        {
            string er = "```\n";
            WebUtil.SetUpDbl(botId == 0 ? Context.Client.CurrentUser.Id : botId);
            var voters = await WebUtil.GetVotersAsync(Client);
            foreach (var id in voters.Take(10))
            {
                er += $"{id.Id}\n";
            }
            er += "...\n";
            foreach (var id in voters.Skip(voters.Count - 10))
            {
                er += $"{id.Id}\n";
            }
            er += "```";
            await ReplyAsync($"Found {voters.Count} new voters.\n {er}");
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
