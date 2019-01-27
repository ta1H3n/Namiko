using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
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
                if (context.User.Id == StaticSettings.owner || ((SocketGuildUser) context.User).Roles.Any(x => x.Id == StaticSettings.home_server))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                {
                    return Task.FromResult(PreconditionResult.FromError("This command is only for my insiders."));
                }
            } catch 
            {
                return Task.FromResult(PreconditionResult.FromError("This command is only for my insiders."));
            }
        }

        public override string GetName()
        {
            return "HomeServer";
        }
    }
}
