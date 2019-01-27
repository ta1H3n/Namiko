using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Namiko.Resources.Preconditions
{
    class OwnerPrecondition : CustomPrecondition
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User.Id == StaticSettings.owner)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
            {
                return Task.FromResult(PreconditionResult.FromError("My will is my master's, and you're clearly not him."));
            }
        }

        public override string GetName()
        {
            return "BotOwner";
        }
    }
}
