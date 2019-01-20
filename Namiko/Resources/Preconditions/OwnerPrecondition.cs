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
                return Task.FromResult(PreconditionResult.FromError("This command is only for my owner."));
            }
        }

        public override string GetName()
        {
            return "BotOwner";
        }
    }
}
