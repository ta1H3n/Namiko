using Discord;

namespace Namiko.Addons.Handlers.Select
{
    public class SelectMenuOption<T>
    {
        public SelectMenuOptionBuilder OptionBuilder { get; set; }
        public T Item { get; set; }

        public SelectMenuOption(SelectMenuOptionBuilder builder, T item)
        {
            OptionBuilder = builder;
            Item = item;
        }
    }
}
