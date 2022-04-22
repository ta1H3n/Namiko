using Discord;
using System;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class UserPermissionAttribute : PreconditionAttribute
    {
        public GuildPermission GuildPermission { get; set; }
        public ChannelPermission ChannelPermission { get; set; }
        private readonly bool Guild = true;

        public UserPermissionAttribute(GuildPermission permission)
        {
            GuildPermission = permission;
        }
        public UserPermissionAttribute(ChannelPermission permission)
        {
            ChannelPermission = permission;
            Guild = false;
        }

        public override string ErrorMessage => $"You don't have the {Name} permission. Getting ahead of yourself, huh?";
        public override string Name => Guild ? GuildPermission.ToString() : ChannelPermission.ToString();

        public override Attribute ReturnAttribute(Handler handler)
            => handler switch
            {
                Handler.Interactions when Guild => new Discord.Interactions.RequireUserPermissionAttribute(GuildPermission),
                Handler.Interactions => new Discord.Interactions.RequireUserPermissionAttribute(ChannelPermission),
                Handler.Commands when Guild => new Discord.Commands.RequireUserPermissionAttribute(GuildPermission) { ErrorMessage = ErrorMessage },
                Handler.Commands => new Discord.Commands.RequireUserPermissionAttribute(ChannelPermission) { ErrorMessage = ErrorMessage },
                _ => throw new NotImplementedException(),
            };
    }
}
