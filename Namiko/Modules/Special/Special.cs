using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;



using Discord.WebSocket;

using Discord.Addons.Interactive;
using Newtonsoft.Json;

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
            int res = await SqliteDbContext.ExecuteSQL(str);
            await Context.Channel.SendMessageAsync($"{res} rows affected.");
        }

        [Command("SQLGET"), Summary("Executes an SQL GET query. DANGEROUS"), OwnerPrecondition]
        public async Task SqlGet([Remainder] string str = "")
        {
            using (var db = new SqliteDbContext())
            {
                var list = db.DynamicListFromSql(str, new Dictionary<string, object>());

                string text = $"Results: {list.Count()}\n";
                text += $"```json\n{JsonConvert.SerializeObject(list, Formatting.Indented)}```";

                if (text.Length > 2000)
                    text = text.Substring(0, 1990) + "\n...```";
                await Context.Channel.SendMessageAsync(text);
            }
        }

        [Command("Die"), Summary("Kills Namiko"), HomePrecondition]
        public async Task Die()
        {
            var ch = Context.Client.GetChannel(StaticSettings.log_channel) as ISocketMessageChannel;
            Task.Run(() => ch.SendMessageAsync($"Killed by {Context.User.Mention} :gun:"));

            Task.Run(() => Context.Channel.SendMessageAsync("Bye bye... :wave:", false, ImageUtil.ToEmbed(ImageDb.GetRandomImage("sudoku")).Build()));

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

        [Command("NewWelcome"), Alias("nwlc"), Summary("Adds a new welcome message. @_ will be replaced with a mention.\n**Usage**: `!nw [welcome]`"), HomePrecondition]
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

        [Command("DeleteWelcome"), Alias("dw", "delwelcome"), Summary("Deletes a welcome message by ID.\n**Usage**: `!dw [id]`"), HomePrecondition]
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

        [Command("SendLootboxes"), OwnerPrecondition]
        public async Task SendLootboxes()
        {
            var voters = await WebUtil.GetVotersAsync();
            var votesNew = new Dictionary<ulong, int>();

            var add = new List<ulong>();
            foreach (var x in voters)
                if (!votesNew.ContainsKey(x.Id))
                {
                    votesNew.Add(x.Id, 1);
                    add.Add(x.Id);
                }

            await Timers.SendRewards(add);
        }

        [Command("MessageVoters"), OwnerPrecondition]
        public async Task MessageVoters([Remainder] string msg = "")
        {
            var voters = await WebUtil.GetVotersAsync();
            var votesNew = new List<ulong>();

            foreach (var x in voters)
                if (!votesNew.Contains(x.Id))
                {
                    votesNew.Add(x.Id);
                }

            int sent = 0;
            foreach (var x in votesNew)
            {
                try
                {
                    var ch = await Program.GetClient().GetUser(x).GetOrCreateDMChannelAsync();
                    await ch.SendMessageAsync(msg);
                    sent++;
                }
                catch { }
            }

            Context.Channel.SendMessageAsync($"Broadcasted to {sent} users.");
        }

        [Command("Debug"), OwnerPrecondition]
        public async Task Debug([Remainder] string msg = "")
        {
            var commands = Program.GetCommands();
            var processed = System.DateTime.Now.AddMinutes(-10);

            Func<Optional<CommandInfo>, ICommandContext, IResult, Task> listen =  async (arg1, arg2, arg3) =>
            {
                if (arg2.Message.Equals(Context.Message) && arg1.Value.Name != "Debug")
                {
                    processed = System.DateTime.Now;
                    await Task.Delay(1);
                }
                Console.WriteLine("Command debugger");
            };
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
    }
}
