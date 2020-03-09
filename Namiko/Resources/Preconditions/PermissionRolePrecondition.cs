using Discord.Commands;
using Discord.WebSocket;
using Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    public class PermissionRoleAttribute : CustomPrecondition
    {
        private readonly RoleType Type;

        public PermissionRoleAttribute(RoleType type)
        {
            this.Type = type;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ids = PermissionRoleDb.Get(context.Guild.Id, Type);
            if (!ids.Any())
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            if (((SocketGuildUser)context.User).Roles.Any(x => ids.Contains(x.Id)))
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError("You do not have the required role to use this command, Senpai... Ask your server admins for help."));
        }

        public override string GetName()
        {
            return Type.ToString();
        }
    }
}
