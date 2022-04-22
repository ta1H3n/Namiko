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

        public override Attribute ReturnAttribute(Handler handler)
            => handler switch
            {
                Handler.Commands when Guild => new Discord.Commands.RequireBotPermissionAttribute(GuildPermission) { ErrorMessage = ErrorMessage },
                Handler.Commands => new Discord.Commands.RequireBotPermissionAttribute(ChannelPermission) { ErrorMessage = ErrorMessage },
                Handler.Interactions when Guild => new Discord.Interactions.RequireBotPermissionAttribute(GuildPermission),
                Handler.Interactions => new Discord.Interactions.RequireBotPermissionAttribute(ChannelPermission),
                _ => throw new NotImplementedException(),
            };
    }
}
