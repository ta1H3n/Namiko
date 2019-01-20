using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Resources.Preconditions
{
    class CustomBotPermission : RequireBotPermissionAttribute
    {
        public CustomBotPermission(GuildPermission permission) : base(permission)
        {
            ErrorMessage = $"I need the {permission.ToString()} permission.";
        }

        public CustomBotPermission(ChannelPermission permission) : base(permission)
        {
            ErrorMessage = $"I need the {permission.ToString()} permission.";
        }
    }
}
