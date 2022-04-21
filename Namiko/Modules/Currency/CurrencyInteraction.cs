using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Namiko
{
    public class CurrencyInteraction : InteractionModuleBase
    {
        [SlashCommand("echo", "Echo an input")]
        public async Task Echo(string input)
        {
            await Context.Channel.SendMessageAsync(input);
            var res = await FollowupAsync(input);

            await Task.Delay(3000);

            var builder = new ComponentBuilder().WithButton("asdf", "123");
            await FollowupAsync(components: builder.Build());
            
        }
    }
}
