using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace Namiko.Resources.Preconditions
{
    public class HomePrecondition : CustomPrecondition
    {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            try
            {
                if (context.User.Id == StaticSettings.owner || context.Guild.Id == StaticSettings.home_server)
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                {
                    return Task.FromResult(PreconditionResult.FromError("Not bot owner"));
                }
            } catch 
            {
                return Task.FromResult(PreconditionResult.FromError("Not bot owner"));
            }
        }

        public override string GetName()
        {
            return "HomeServer";
        }
    }
}
