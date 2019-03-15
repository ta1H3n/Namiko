using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;
using Namiko.Resources.Preconditions;
using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using Discord.WebSocket;
using Discord.Rest;
using Namiko.Core.Util;
using Discord.Addons.Interactive;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Namiko.Core.Modules
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

        [Command("Pause"), Summary("Pauses or Unpauses the bot"), OwnerPrecondition]
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
            var voters = await WebUtil.GetVoters();
            var votesNew = new Dictionary<ulong, int>();

            var add = new List<Voter>();
            foreach (var x in voters)
                if (!votesNew.ContainsKey(x.Id))
                {
                    votesNew.Add(x.Id, 1);
                    add.Add(new Voter { UserId = x.Id });
                }

            await Timers.SendRewards(add);
        }
    }
}
