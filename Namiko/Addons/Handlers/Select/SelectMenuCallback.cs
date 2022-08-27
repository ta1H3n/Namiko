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
        public TaskCompletionSource<T> _taskSource;

        public SelectMenuCallback(SelectMenu<T> selectMenu, ICustomContext context, ICriterion<SocketMessageComponent> criterion = null) : base(context, criterion)
        {
            _selectMenu = selectMenu;
            _taskSource = new TaskCompletionSource<T>();
        }

        public override async Task<IUserMessage> DisplayAsync()
        {
            var builder = new ComponentBuilder()
                .WithSelectMenu(nameof(SelectMenu<T>), _selectMenu.Options.Values.Select(x => x.OptionBuilder).ToList(), _selectMenu.Label);

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

            var id = comp.Data.Values.First();
            var res = _selectMenu.Options[id].Item;
            await TimeoutAsync();
            _taskSource.TrySetResult(res);
            return true;
        }
    }
}
