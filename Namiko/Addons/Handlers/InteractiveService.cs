using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Addons.Handlers.Confirmation;
using Namiko.Addons.Handlers.Criteria;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Addons.Handlers.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Namiko.Addons.Handlers
{
    public class InteractiveService : IDisposable
    {
        public BaseSocketClient Discord { get; }

        private Dictionary<ulong, CallbackBase<SocketMessageComponent>> _callbacks;
        private Dictionary<ulong, Timer> _timers;

        public InteractiveService(DiscordShardedClient discord)
        {
            Discord = discord;
            Discord.SelectMenuExecuted += HandleComponentExecuted;
            Discord.ButtonExecuted += HandleComponentExecuted;

            _callbacks = new();
            _timers = new();
        }


        public async Task<IUserMessage> SendPaginatedMessageAsync(ICustomContext context, PaginatedMessage pager, ICriterion<SocketMessageComponent> criterion = null)
            => await ExecuteCallbackAsync(new PaginatedMessageCallback(pager, context, criterion));

        public async Task<IUserMessage> SendDialogueBoxAsync(ICustomContext context, DialogueBox dialogue, ICriterion<SocketMessageComponent> criterion = null, int timeout = 60)
            => await ExecuteCallbackAsync(new DialogueBoxCallback(dialogue, context, criterion, timeout));

        public Task<T> SendSelectMenuAsync<T>(ICustomContext context, SelectMenu<T> select, ICriterion<SocketMessageComponent> criterion = null)
        {
            if (select.Options.Count == 0)
                return Task.FromResult(default(T));
            if (select.Options.Count == 1)
                return Task.FromResult(select.Options.Values.First().Item);

            var callback = new SelectMenuCallback<T>(select, context, criterion);
            var message = callback.DisplayAsync().ContinueWith(x => 
            {
                RegisterCallback(callback);
            });
            return callback._taskSource.Task;
        }

        public Task<bool> Confirm(ICustomContext context, string question = null, ICriterion<SocketMessageComponent> criterion = null)
        {
            var callback = new ConfirmationCallback(context, question, criterion);
            var message = callback.DisplayAsync().ContinueWith(x =>
            {
                RegisterCallback(callback);
            });
            return callback._taskSource.Task;
        }





        private async Task<IUserMessage> ExecuteCallbackAsync(CallbackBase<SocketMessageComponent> callback)
        {
            await callback.DisplayAsync().ConfigureAwait(false);
            RegisterCallback(callback);
            return callback.Message;
        }
        private async Task HandleComponentExecuted(SocketMessageComponent arg)
        {
            _ = arg.DeferAsync();
            if (!_callbacks.TryGetValue(arg.Message.Id, out var callback)) return;
            if (!await callback.Criterion.JudgeAsync(callback.Context, arg).ConfigureAwait(false))
                return;

            var res = await callback.HandleCallbackAsync(arg).ConfigureAwait(false);
            if (res is bool && (bool)res == true)
            {
                DisposeCallback(callback);
            }
        }

        private void RegisterCallback(CallbackBase<SocketMessageComponent> callback)
        {
            AddCallback(callback.Message, callback);
            if (callback.Timeout.Value.TotalMilliseconds > 0)
            {
                var timer = new Timer()
                {
                    Interval = callback.Timeout.Value.TotalMilliseconds,
                    AutoReset = false,
                    Enabled = true
                };
                timer.Elapsed += (sender, args) => DisposeCallback(callback);
                timer.Start();
                _timers.Add(callback.Message.Id, timer);
            }
        }
        private void DisposeCallback(CallbackBase<SocketMessageComponent> callback)
        {
            if (_timers.TryGetValue(callback.Message.Id, out var timer))
            {
                _timers.Remove(callback.Message.Id);
                timer.Dispose();
            }
            RemoveCallback(callback.Message);
            callback.TimeoutAsync();
        }

        private void AddCallback(IMessage message, CallbackBase<SocketMessageComponent> callback)
            => _callbacks[message.Id] = callback;
        private void RemoveCallback(SocketMessageComponent arg)
            => RemoveCallback(arg.Message.Id);
        public void RemoveCallback(IMessage message)
            => RemoveCallback(message.Id);
        private void RemoveCallback(ulong id)
            => _callbacks.Remove(id);
        private void ClearCallbacks()
            => _callbacks.Clear();

        public void Dispose()
        {
            Discord.SelectMenuExecuted -= HandleComponentExecuted;
            Discord.ButtonExecuted -= HandleComponentExecuted;
        }
    }
}
