using Discord;
using Discord.Commands;
using Discord.Interactions;
using Namiko.Addons.Handlers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Namiko.Modules.Currency
{
    public class CurrencyTestModule : CustomModuleBase<ICustomContext>
    {
        [Command("epiccommand"), Alias("bj"), Discord.Commands.Summary("Starts a game of blackjack.\n**Usage**: `!bj [amount]`")]
        [SlashCommand("epiccommand", "Do something")]
        public async Task EpicCommand(string input)
        {
            await Context.Channel.SendMessageAsync(input);
            var res = await ReplyAsync(input);

            await Task.Delay(3000);

            var builder = new ComponentBuilder().WithButton("asdf", "123");

            var b = new SelectMenuBuilder();

            await ReplyAsync(components: builder.Build());
        }
    }
}
