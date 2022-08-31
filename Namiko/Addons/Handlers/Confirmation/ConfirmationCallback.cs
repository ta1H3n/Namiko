using Discord;
using Discord.WebSocket;
using Namiko.Addons.Handlers.Criteria;
using System;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers.Confirmation
{
    internal class ConfirmationCallback : CallbackBase<SocketMessageComponent>
    {
        string Question { get; }
        public TaskCompletionSource<bool> _taskSource;

        public ConfirmationCallback(ICustomContext context, string question = null, ICriterion<SocketMessageComponent> criterion = null) : base(context, criterion)
        {
            DisposeLevel = DisposeLevel.Delete;
            Question = question ?? "Are you sure?";
            _taskSource = new TaskCompletionSource<bool>();
        }

        public async override Task<IUserMessage> DisplayAsync()
        {
            var eb = new EmbedBuilderPrepared(Question);
            var cb = new ComponentBuilder()
                .WithButton(customId: "no", emote: Emote.Parse("<:TickNo:577838859077943306>"), style: ButtonStyle.Secondary)
                .WithButton(customId: "yes", emote: Emote.Parse("<:TickYes:577838859107303424>"), style: ButtonStyle.Secondary);

            var message = await Context.ReplyAsync(embed: eb.Build(), components: cb.Build()).ConfigureAwait(false);
            Message = message;
            return message;
        }

        public async override Task<object> HandleCallbackAsync(SocketMessageComponent comp)
        {
            var res = comp.Data.CustomId switch
            {
                "yes" => true,
                "no" => false,
                _ => true
            };
            
            DisposeLevel = comp.Data.CustomId switch
            {
                "yes" => DisposeLevel.Continue,
                "no" => DisposeLevel.Delete,
                _ => DisposeLevel.Delete
            };

            await TimeoutAsync();
            _taskSource.SetResult(res);
            return true;
        }
    }
}
