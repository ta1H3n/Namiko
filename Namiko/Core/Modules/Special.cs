using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;
using Namiko.Resources.Preconditions;
using Namiko.Resources.Database;
using Discord.WebSocket;
using Discord.Rest;
using Namiko.Core.Util;
using Discord.Addons.Interactive;

namespace Namiko.Core.Modules
{
    public class Special : InteractiveBase<SocketCommandContext>
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

        [Command("Say"), Alias("s"), OwnerPrecondition]
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

        [Command("GetInvite"), Summary("Gets invite to a server"), OwnerPrecondition]
        public async Task GetInvite(ulong id, [Remainder] string str = "")
        {
            var guild = Context.Client.GetGuild(id);
            var invite = (await guild.GetInvitesAsync()).FirstOrDefault().Url;
            await Context.Channel.SendMessageAsync(invite);
        }
    }
}
