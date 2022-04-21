using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko
{
    class CustomBotPermission : RequireBotPermissionAttribute
    {
        public CustomBotPermission(GuildPermission permission) : base(permission)
        {
            ErrorMessage = $"I need the {permission.ToString()} permission. Life is unfair.";
        }

        public CustomBotPermission(ChannelPermission permission) : base(permission)
        {
            ErrorMessage = $"I need the {permission.ToString()} permission. Life is unfair.";
        }
    }
}
