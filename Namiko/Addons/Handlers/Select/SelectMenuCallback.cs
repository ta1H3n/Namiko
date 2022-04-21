using Discord;
using Discord.WebSocket;
using Namiko.Addons.Handlers.Criteria;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers.Select
{
    public class SelectMenuCallback<T> : CallbackBase<SocketMessageComponent>
    {
        public readonly SelectMenu<T> _selectMenu;

        public SelectMenuCallback(SelectMenu<T> selectMenu, ICustomContext context, ICriterion<SocketMessageComponent> criterion = null) : base(context, criterion)
        {
            _selectMenu = selectMenu;
        }

        public override async Task<IUserMessage> DisplayAsync()
        {
            var builder = new ComponentBuilder();
            builder.WithSelectMenu(nameof(SelectMenu<T>), _selectMenu.Options.Values.Select(x => x.OptionBuilder).ToList());

            var message = await Context.ReplyAsync(embed: _selectMenu.Embed, components: builder.Build()).ConfigureAwait(false);
            Message = message;
            return message;
        }

        public override async Task<object> HandleCallbackAsync(SocketMessageComponent comp)
        {
            if (comp.Data.CustomId != nameof(SelectMenu<T>))
            {
                return null;
            }

            var res = comp.Data.Values.First();
            await _selectMenu.Continue(_selectMenu.Options[res].Item);
            return true;
        }
    }
}
