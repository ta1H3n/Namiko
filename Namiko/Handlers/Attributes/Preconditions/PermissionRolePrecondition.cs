using Discord;
using Discord.WebSocket;
using Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class PermissionRoleAttribute : PreconditionAttribute
    {
        private readonly RoleType Type;

        public PermissionRoleAttribute(RoleType type)
        {
            Type = type;
        }

        public override string ErrorMessage => null;
        public override string Name => Type.ToString();

        public override Attribute ReturnAttribute(Handler handler)
            => handler switch
            {
                Handler.Commands => new Discord.Commands.RequireOwnerAttribute() { ErrorMessage = ErrorMessage },
                Handler.Interactions => new Discord.Interactions.RequireOwnerAttribute(),
                _ => throw new NotImplementedException(),
            };

        private class PermissionRoleCommandsAttribute : Discord.Commands.PreconditionAttribute
        {
            RoleType Type;
            public override Task<Discord.Commands.PreconditionResult> CheckPermissionsAsync(Discord.Commands.ICommandContext context, Discord.Commands.CommandInfo command, IServiceProvider services)
            {
                var ids = PermissionRoleDb.Get(context.Guild.Id, Type);
                if (!ids.Any())
                {
                    return Task.FromResult(Discord.Commands.PreconditionResult.FromSuccess());
                }

                if (((SocketGuildUser)context.User).Roles.Any(x => ids.Contains(x.Id)))
                    return Task.FromResult(Discord.Commands.PreconditionResult.FromSuccess());

                string roles = "";
                string again = "";
                foreach (var id in ids)
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

                return Task.FromResult(Discord.Commands.PreconditionResult.FromError("You need one of these roles to use this command, Senpai...\n" + roles + "\n" + again));
            }
            public PermissionRoleCommandsAttribute(RoleType type)
            {
                Type = type;
            }
        }
        private class PermissionRoleInteractionsAttribute : Discord.Interactions.PreconditionAttribute
        {
            RoleType Type;
            public override Task<Discord.Interactions.PreconditionResult> CheckRequirementsAsync(IInteractionContext context, Discord.Interactions.ICommandInfo commandInfo, IServiceProvider services)
            {
                var ids = PermissionRoleDb.Get(context.Guild.Id, Type);
                if (!ids.Any())
                {
                    return Task.FromResult(Discord.Interactions.PreconditionResult.FromSuccess());
                }

                if (((SocketGuildUser)context.User).Roles.Any(x => ids.Contains(x.Id)))
                    return Task.FromResult(Discord.Interactions.PreconditionResult.FromSuccess());

                string roles = "";
                string again = "";
                foreach (var id in ids)
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

                return Task.FromResult(Discord.Interactions.PreconditionResult.FromError("You need one of these roles to use this command, Senpai...\n" + roles + "\n" + again));
            }
            public PermissionRoleInteractionsAttribute(RoleType type)
            {
                Type = type;
            }
        }
    }
}
