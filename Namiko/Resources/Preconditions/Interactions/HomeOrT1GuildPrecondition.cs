using Discord.Commands;
using Discord.WebSocket;
using Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko.Resources.Preconditions.Interactions
{
    class HomeOrT1GuildPrecondition : CustomPrecondition
    {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            try
            {
                if (context.User.Id == AppSettings.OwnerId || ((SocketGuildUser)context.User).Roles.Any(x => x.Id == AppSettings.InsiderRoleId))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else if (PremiumDb.IsPremium(context.Guild.Id, ProType.GuildPlus))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                {
                    return Task.FromResult(PreconditionResult.FromError("This command is only for my insiders or Pro Guild+ Servers."));
                }
            }
            catch
            {
                if (PremiumDb.IsPremium(context.Guild.Id, ProType.GuildPlus))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                return Task.FromResult(PreconditionResult.FromError("This command is only for my insiders or Pro Guild+ Servers."));
            }
        }

        public override string GetName()
        {
            return "Guild+";
        }
    }
}