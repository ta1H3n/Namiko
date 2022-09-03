using Discord;
using Discord.WebSocket;
using Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class HomeOrT1GuildPrecondition : PreconditionAttribute
    {
        public override string ErrorMessage => null;
        public override string Name => "Guild+";

        public override Attribute ReturnAttribute(HandlerType handlerType)
            => handlerType switch
            {
                HandlerType.Commands => new HomeOrT1GuildCommandsAttribute(ErrorMessage),
                HandlerType.Interactions => new HomeOrT1GuildInteractionsAttribute(),
                _ => throw new NotImplementedException(),
            };
        private class HomeOrT1GuildCommandsAttribute : Discord.Commands.PreconditionAttribute
        {
            public override Task<Discord.Commands.PreconditionResult> CheckPermissionsAsync(Discord.Commands.ICommandContext context, Discord.Commands.CommandInfo command, IServiceProvider services)
            {
                try
                {
                    if (context.User.Id == AppSettings.OwnerId || ((SocketGuildUser)context.User).Roles.Any(x => x.Id == AppSettings.InsiderRoleId))
                        return Task.FromResult(Discord.Commands.PreconditionResult.FromSuccess());
                    else if (PremiumDb.IsPremium(context.Guild.Id, ProType.GuildPlus))
                        return Task.FromResult(Discord.Commands.PreconditionResult.FromSuccess());
                    else
                    {
                        return Task.FromResult(Discord.Commands.PreconditionResult.FromError("This command is only for my insiders or Pro Guild+ Servers."));
                    }
                }
                catch
                {
                    if (PremiumDb.IsPremium(context.Guild.Id, ProType.GuildPlus))
                        return Task.FromResult(Discord.Commands.PreconditionResult.FromSuccess());
                    return Task.FromResult(Discord.Commands.PreconditionResult.FromError("This command is only for my insiders or Pro Guild+ Servers."));
                }
            }
            public HomeOrT1GuildCommandsAttribute(string errorMessage)
            {
                ErrorMessage = errorMessage;
            }
        }
        private class HomeOrT1GuildInteractionsAttribute : Discord.Interactions.PreconditionAttribute
        {
            public override Task<Discord.Interactions.PreconditionResult> CheckRequirementsAsync(IInteractionContext context, Discord.Interactions.ICommandInfo commandInfo, IServiceProvider services)
            {
                try
                {
                    if (context.User.Id == AppSettings.OwnerId || ((SocketGuildUser)context.User).Roles.Any(x => x.Id == AppSettings.InsiderRoleId))
                        return Task.FromResult(Discord.Interactions.PreconditionResult.FromSuccess());
                    else if (PremiumDb.IsPremium(context.Guild.Id, ProType.GuildPlus))
                        return Task.FromResult(Discord.Interactions.PreconditionResult.FromSuccess());
                    else
                    {
                        return Task.FromResult(Discord.Interactions.PreconditionResult.FromError("This command is only for my insiders or Pro Guild+ Servers."));
                    }
                }
                catch
                {
                    if (PremiumDb.IsPremium(context.Guild.Id, ProType.GuildPlus))
                        return Task.FromResult(Discord.Interactions.PreconditionResult.FromSuccess());
                    return Task.FromResult(Discord.Interactions.PreconditionResult.FromError("This command is only for my insiders or Pro Guild+ Servers."));
                }
            }
            public HomeOrT1GuildInteractionsAttribute()
            {
            }
        }
    }
}