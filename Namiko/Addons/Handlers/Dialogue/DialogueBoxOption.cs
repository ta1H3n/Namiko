using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers.Dialogue
{
    public class DialogueBoxOption
    {
        public ButtonBuilder ButtonBuilder { get; set; }
        public Func<IUserMessage, Task> Action { get; set; }
        public DisposeLevel After { get; set; } = DisposeLevel.RemoveComponents;
    }
}
