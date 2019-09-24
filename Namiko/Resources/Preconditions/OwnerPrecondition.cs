using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Namiko
{
    class OwnerPrecondition : CustomPrecondition
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User.Id == Config.OwnerId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
            {
                //return Task.FromResult(PreconditionResult.FromError("Only my Master can use that. And you... you're just... *heh*"));
                return Task.FromResult(PreconditionResult.FromError(""));
            }
        }

        public override string GetName()
        {
            return "BotOwner";
        }
    }
}
