using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class DialogueBoxOption
    {
        public Func<IUserMessage, Task> Action { get; set; }
        public OnExecute After { get; set; } = OnExecute.RemoveCallback;
    }

    public enum OnExecute
    {
        Delete,
        RemoveReactions,
        RemoveCallback,
        Continue
    }
}
