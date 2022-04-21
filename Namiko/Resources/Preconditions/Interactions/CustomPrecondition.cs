using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Namiko.Resources.Preconditions.Interactions
{
    public abstract class CustomPrecondition : PreconditionAttribute
    {
        public abstract string GetName();
    }
}
