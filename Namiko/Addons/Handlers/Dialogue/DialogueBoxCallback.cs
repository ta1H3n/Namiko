using Discord;
using Discord.WebSocket;
using Namiko.Addons.Handlers.Criteria;
using System;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers.Dialogue
{
    public class DialogueBoxCallback : CallbackBase<SocketMessageComponent>
    {
        public readonly DialogueBox _dialogue;

        public DialogueBoxCallback(DialogueBox dialogue, ICustomContext context, ICriterion<SocketMessageComponent> criterion = null) : base(context, criterion)
        {
            _dialogue = dialogue;
        }

        public async override Task<IUserMessage> DisplayAsync()
        {
            var builder = new ComponentBuilder();
            foreach (var option in _dialogue.Options)
            {
                builder = builder.WithButton(option.Value.ButtonBuilder);
            }

            var message = await Context.ReplyAsync(embed: _dialogue.Embed, components: builder.Build()).ConfigureAwait(false);
            Message = message;
            return message;
        }

        public async override Task<object> HandleCallbackAsync(SocketMessageComponent comp)
        {
            _dialogue.Options.TryGetValue(comp.Data.CustomId, out var option);
            object end = null;
            if (option != null)
            {
                switch (option.After)
                {
                    case DisposeLevel.Delete:
                        _ = Message.DeleteAsync();
                        end = true;
                        break;
                    case DisposeLevel.RemoveComponents:
                        _ = Message.ModifyAsync(m => m.Components = null);
                        end = true;
                        break;
                    case DisposeLevel.RemoveCallback:
                        end = true;
                        break;
                    default:
                        break;
                }

                try
                {
                    await option.Action(Message);
                }
                catch { }
            }
            return end;
        }
    }
}
