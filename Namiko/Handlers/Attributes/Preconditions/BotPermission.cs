using Discord;
using System;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class BotPermissionAttribute : PreconditionAttribute
    {
        public GuildPermission GuildPermission { get; set; }
        public ChannelPermission ChannelPermission { get; set; }
        private readonly bool Guild = true;

        public BotPermissionAttribute(GuildPermission permission)
        {
            GuildPermission = permission;
        }
        public BotPermissionAttribute(ChannelPermission permission)
        {
            ChannelPermission = permission;
            Guild = false;
        }

        public override string ErrorMessage => $"Senpai, I need the **{Name}** permission to run this command... Life is unfair...";
        public override string Name => Guild ? GuildPermission.ToString() : ChannelPermission.ToString();

        public override Attribute ReturnAttribute(HandlerType handlerType)
            => handlerType switch
            {
                HandlerType.Commands when Guild => new Discord.Commands.RequireBotPermissionAttribute(GuildPermission) { ErrorMessage = ErrorMessage },
                HandlerType.Commands => new Discord.Commands.RequireBotPermissionAttribute(ChannelPermission) { ErrorMessage = ErrorMessage },
                HandlerType.Interactions when Guild => new Discord.Interactions.RequireBotPermissionAttribute(GuildPermission),
                HandlerType.Interactions => new Discord.Interactions.RequireBotPermissionAttribute(ChannelPermission),
                _ => throw new NotImplementedException(),
            };
    }
}
