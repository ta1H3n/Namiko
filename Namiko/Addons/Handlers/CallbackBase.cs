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
        public DisposeLevel DisposeLevel { get; internal set; }

        public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(60);

        public CallbackBase(ICustomContext sourceContext, ICriterion<T> criterion = null, int timeout = 60, DisposeLevel disposeLevel = DisposeLevel.RemoveComponents)
        {
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<T>();
            DisposeLevel = disposeLevel;
            Timeout = TimeSpan.FromSeconds(timeout);
        }

        public abstract Task<IUserMessage> DisplayAsync();
        public abstract Task<object> HandleCallbackAsync(T response);
        public virtual Task TimeoutAsync()
        {
            switch (DisposeLevel)
            {
                case DisposeLevel.Delete:
                    Message.DeleteAsync();
                    break;
                case DisposeLevel.RemoveComponents:
                    Message.ModifyAsync(m => m.Components = null);
                    break;
                case DisposeLevel.RemoveCallback:
                    break;
                default:
                    break;
            }
            return Task.CompletedTask;
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
