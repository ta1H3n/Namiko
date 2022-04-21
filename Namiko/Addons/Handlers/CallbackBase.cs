using Discord;
using Discord.Commands;
using Namiko.Addons.Handlers.Criteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers
{
    public abstract class CallbackBase<T>
    {
        public ICustomContext Context { get; protected set; }
        public ICriterion<T> Criterion { get; protected set; }
        public IUserMessage Message { get; internal set; }

        public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(60);

        public CallbackBase(ICustomContext sourceContext, ICriterion<T> criterion = null)
        {
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<T>();
        }

        public abstract Task<IUserMessage> DisplayAsync();
        public abstract Task<object> HandleCallbackAsync(T response);
        public async virtual Task TimeoutAsync()
        {
            await Message.DeleteAsync().ConfigureAwait(false);
        }
    }

    public enum DisposeLevel
    {
        Delete,
        RemoveComponents,
        RemoveCallback,
        Continue
    }
}
