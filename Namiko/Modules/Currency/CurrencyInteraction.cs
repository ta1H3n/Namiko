using Discord;
using Discord.Commands;
using Discord.Interactions;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Namiko
{
    public class CurrencyInteraction : CustomModuleBase<ICustomContext>
    {
        [SlashCommand("echo", "Echo an input")]
        public async Task Echo(string input)
        {
            await ReplyAsync(input);
        }
        [SlashCommand("select", "asdf")]
        public async Task Select([Remainder] string search)
        {
            var waifus = await WaifuDb.SearchWaifus(search);

            var menu = new SelectMenu<Waifu>(
                new EmbedBuilderPrepared("Select waifu loosser").Build(),
                waifus.ToDictionary(
                    x => x.Name, 
                    x => new SelectMenuOption<Waifu>(
                        new SelectMenuOptionBuilder(x.LongName, x.Name),
                        x
            )));

            var task = SelectMenuReplyAsync(menu);

            var res = await task;

            await ReplyAsync(embed: WaifuUtil.WaifuEmbedBuilder(res).Build());
        }
    }
}
