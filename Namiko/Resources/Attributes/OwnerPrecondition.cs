using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Namiko.Resources.Attributes
{
    class OwnerPrecondition : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User.Id == StaticSettings.owner)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
            {
                //context.Channel.SendMessageAsync("You can't do thaaaaaaat! This one's only for tai >_<");
                return Task.FromResult(PreconditionResult.FromError("Not bot owner"));
            }
        }
    }
}
