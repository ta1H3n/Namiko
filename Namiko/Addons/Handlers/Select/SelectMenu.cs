using Discord;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers.Select
{
    public class SelectMenu<T>
    {
        public Embed Embed { get; }
        public IDictionary<string, SelectMenuOption<T>> Options { get; } = new Dictionary<string, SelectMenuOption<T>>();
        public Func<T, Task> Continue { get; }

        public SelectMenu(Embed embed, IDictionary<string, SelectMenuOption<T>> options, Func<T, Task> continueWith)
        {
            Embed = embed;
            Options = options;
            Continue = continueWith;
        }
    }
}
