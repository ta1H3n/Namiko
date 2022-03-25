using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;

namespace Discord.Addons.Interactive
{
    public class DialogueBoxCallback : IReactionCallback
    {
        public SocketCommandContext Context { get; }
        public InteractiveService Interactive { get; private set; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion { get; }
        public TimeSpan? Timeout => _dialogue.Timeout;

        public readonly DialogueBox _dialogue;

        public DialogueBoxCallback(
            InteractiveService interactive,
            SocketCommandContext sourceContext,
            DialogueBox Dialogue,
            ICriterion<SocketReaction> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            _dialogue = Dialogue;
        }

        public async Task DisplayAsync()
        {
            var message = await Context.Channel.SendMessageAsync(embed: _dialogue.Embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);

            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                foreach(var emote in _dialogue.Options)
                {
                    await message.AddReactionAsync(emote.Key);
                }
            });

            if (Timeout.HasValue)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(message);
                    switch (_dialogue.OnTimeout)
                    {
                        case TimeoutOptions.Delete:
                            message.DeleteAsync();
                            break;
                        case TimeoutOptions.RemoveReactions:
                            message.RemoveAllReactionsAsync();
                            break;
                        default:
                            break;
                    }
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            _dialogue.Options.TryGetValue(reaction.Emote, out var option);
            bool end = false;
            if(option != null)
            {
                switch (option.After)
                {
                    case OnExecute.Delete:
                        _ = Message.DeleteAsync();
                        end = true;
                        break;
                    case OnExecute.RemoveReactions:
                        _ = Message.RemoveAllReactionsAsync();
                        end = true;
                        break;
                    case OnExecute.RemoveCallback:
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
