using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Resources.Preconditions
{
    class CustomUserPermission : RequireUserPermissionAttribute
    {
        public CustomUserPermission(GuildPermission permission) : base(permission)
        {
            ErrorMessage = $"You don't have the {permission.ToString()} permission.";
        }

        public CustomUserPermission(ChannelPermission permission) : base(permission)
        {
            ErrorMessage = $"You don't have the {permission.ToString()} permission.";
        }
    }
}
