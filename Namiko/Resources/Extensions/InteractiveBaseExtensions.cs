using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko
{
    public static class InteractiveBaseExtensions
    {
        public static string Prefix(this InteractiveBase<ShardedCommandContext> interactive)
        {
            return Program.GetPrefix(interactive.Context);
        }
    }
}
