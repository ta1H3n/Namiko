using System;

namespace Namiko.Handlers.Attributes
{
    public abstract class PreconditionAttribute : Attribute
    {
        public virtual string ErrorMessage { get; } = $"Precondition failed";
        public abstract string Name { get; }
        public abstract Attribute ReturnAttribute(Handler handler);
    }
}
