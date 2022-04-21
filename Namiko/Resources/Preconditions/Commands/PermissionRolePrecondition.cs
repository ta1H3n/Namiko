using Discord.Commands;
using Discord.WebSocket;
using Model;
using System;
using System.ComponentModel.DataAnnotations;
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

            string roles = "";
            string again = "";
            foreach(var id in ids)
            {
                var role = context.Guild.GetRole(id);
                if (role != null)
                    roles += $"• **{role.Name}**\n";
                else
                {
                    roles += $"• Deleted role - id: {id}\n";
                    again = "Removed deleted roles from requirements... Please try again, Senpai...";
                    _ = PermissionRoleDb.Delete(id);
                }
            }

            return Task.FromResult(PreconditionResult.FromError("You need one of these roles to use this command, Senpai...\n" + roles + "\n" + again));
        }

        public override string GetName()
        {
            return Type.ToString();
        }
    }
}
