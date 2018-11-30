using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace Namiko.Resources.Attributes
{
    class HomePrecondition : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            try
            {
                if (context.User.Id == StaticSettings.owner || context.Guild.Id == StaticSettings.home_server)
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                {
                    context.Channel.SendMessageAsync("Nononono, you can't use this command, you're not in my home guild!");
                    return Task.FromResult(PreconditionResult.FromError("Not bot owner"));
                }
            } catch (Exception ex)
            {
                context.Channel.SendMessageAsync("Nononono, you can't use this command, you're not in my home guild!");
                return Task.FromResult(PreconditionResult.FromError("Not bot owner"));
            }
        }
    }
}
