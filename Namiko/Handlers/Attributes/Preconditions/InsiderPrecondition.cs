using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class InsiderAttribute : PreconditionAttribute
    {
        public override string ErrorMessage => null;
        public override string Name => "BotInsider";

        public override Attribute ReturnAttribute(HandlerType handlerType)
            => handlerType switch
            {
                HandlerType.Interactions => new InsiderInteractionsAttribute(),
                HandlerType.Commands => new InsiderCommandsAttribute(ErrorMessage),
                _ => throw new NotImplementedException(),
            };

        private class InsiderCommandsAttribute : Discord.Commands.PreconditionAttribute
        {
            public override Task<Discord.Commands.PreconditionResult> CheckPermissionsAsync(Discord.Commands.ICommandContext context, Discord.Commands.CommandInfo command, IServiceProvider services)
            {
                try
                {
                    if (context.User.Id == AppSettings.OwnerId || ((IGuildUser)context.User).RoleIds.Any(x => x == AppSettings.InsiderRoleId))
                        return Task.FromResult(Discord.Commands.PreconditionResult.FromSuccess());
                    else
                    {
                        return Task.FromResult(Discord.Commands.PreconditionResult.FromError("This command is only for my insiders."));
                    }
                }
                catch
                {
                    return Task.FromResult(Discord.Commands.PreconditionResult.FromError("This command is only for my insiders."));
                }
            }
            public InsiderCommandsAttribute(string errorMessage)
            {
                ErrorMessage = errorMessage;
            }
        }
        private class InsiderInteractionsAttribute : Discord.Interactions.PreconditionAttribute
        {
            public override Task<Discord.Interactions.PreconditionResult> CheckRequirementsAsync(IInteractionContext context, Discord.Interactions.ICommandInfo commandInfo, IServiceProvider services)
            {
                try
                {
                    if (context.User.Id == AppSettings.OwnerId || ((IGuildUser)context.User).RoleIds.Any(x => x == AppSettings.InsiderRoleId))
                        return Task.FromResult(Discord.Interactions.PreconditionResult.FromSuccess());
                    else
                    {
                        return Task.FromResult(Discord.Interactions.PreconditionResult.FromError("This command is only for my insiders."));
                    }
                }
                catch
                {
                    return Task.FromResult(Discord.Interactions.PreconditionResult.FromError("This command is only for my insiders."));
                }
            }
            public InsiderInteractionsAttribute()
            {
            }
        }
    }
}
