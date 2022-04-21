using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Addons.Handlers.Criteria;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Addons.Handlers.Select;
using System;
using System.Collections.Generic;
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

        public async Task<IUserMessage> SendDialogueBoxAsync(ICustomContext context, DialogueBox dialogue, ICriterion<SocketMessageComponent> criterion = null)
            => await ExecuteCallbackAsync(new DialogueBoxCallback(dialogue, context, criterion));

        public async Task<IUserMessage> SendSelectMenuAsync<T>(ICustomContext context, SelectMenu<T> select, ICriterion<SocketMessageComponent> criterion = null)
            => await ExecuteCallbackAsync(new SelectMenuCallback<T>(select, context, criterion));

        public async Task<IUserMessage> ExecuteCallbackAsync(CallbackBase<SocketMessageComponent> callback)
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

            _ = Task.Run(async () =>
            {
                var res = await callback.HandleCallbackAsync(arg).ConfigureAwait(false);
                if (res is bool && (bool)res == true)
                {
                    DisposeCallback(callback);
                }
            });
        }


        private void RegisterCallback(CallbackBase<SocketMessageComponent> callback)
        {
            AddCallback(callback.Message, callback);
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
        private void DisposeCallback(CallbackBase<SocketMessageComponent> callback)
        {
            var timer = _timers[callback.Message.Id];
            _timers.Remove(callback.Message.Id);
            timer.Dispose();
            RemoveCallback(callback.Message);
            callback.TimeoutAsync().ConfigureAwait(false);
        }

        private void AddCallback(IMessage message, CallbackBase<SocketMessageComponent> callback)
            => _callbacks[message.Id] = callback;
        private void RemoveCallback(SocketMessageComponent arg)
            => RemoveCallback(arg.Message.Id);
        private void RemoveCallback(IMessage message)
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
