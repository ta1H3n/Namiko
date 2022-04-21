using Discord;
using System.Collections.Generic;

namespace Namiko.Addons.Handlers.Dialogue
{
    public class DialogueBox
    {
        public Embed Embed { get; set; }
        public IDictionary<string, DialogueBoxOption> Options { get; set; } = new Dictionary<string, DialogueBoxOption>();

        public DialogueBox(Embed embed)
        {
            Embed = embed;
        }

        public DialogueBox(string text, IUser user = null)
        {
            var eb = new EmbedBuilder().WithDescription(text);
            if (user != null)
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
}
