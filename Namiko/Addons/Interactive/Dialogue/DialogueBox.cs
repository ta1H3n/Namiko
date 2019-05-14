using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Addons.Interactive
{
    public class DialogueBox
    {
        public Embed Embed { get; set; }
        public IDictionary<IEmote, DialogueBoxOption> Options { get; set; } = new Dictionary<IEmote, DialogueBoxOption>();
        public TimeSpan? Timeout { get; set; } = null;
        public TimeoutOptions OnTimeout { get; set; } = TimeoutOptions.RemoveReactions;

        public DialogueBox(Embed embed)
        {
            Embed = embed;
        }

        public DialogueBox(string text, IUser user = null)
        {
            var eb = new EmbedBuilder().WithDescription(text);
            if(user != null)
            {
                eb.WithAuthor(user);
            }
            Embed = eb.Build();
        }
        
        public DialogueBox()
        {
            Embed = new EmbedBuilder().WithDescription("-").Build();
        }
    }

    public enum TimeoutOptions
    {
        Delete,
        RemoveReactions,
        RemoveCallback
    }
}
