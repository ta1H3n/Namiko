using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Resources.Preconditions.Interactions
{
    class CustomBotPermission : RequireBotPermissionAttribute
    {
        public CustomBotPermission(GuildPermission permission) : base(permission)
        {
            //ErrorMessage = $"I need the {permission.ToString()} permission. Life is unfair.";
        }

        public CustomBotPermission(ChannelPermission permission) : base(permission)
        {
            //ErrorMessage = $"I need the {permission.ToString()} permission. Life is unfair.";
        }
    }
}
