using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace Namiko
{
    public class RequireGuild : RequireContextAttribute
    {
        public RequireGuild() : base(ContextType.Guild)
        {
            ErrorMessage = "You can only use this command in a server, Senpai...";
        }
    }
}
