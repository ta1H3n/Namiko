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
            await FollowupAsync(input);
        }
    }
}
