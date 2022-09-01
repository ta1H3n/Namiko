using System;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class OwnerPrecondition : PreconditionAttribute
    {
        public override string ErrorMessage => null;
        public override string Name => "BotOwner";

        public override Attribute ReturnAttribute(HandlerType handlerType)
            => handlerType switch
            {
                HandlerType.Interactions => new Discord.Interactions.RequireOwnerAttribute(),
                HandlerType.Commands => new Discord.Commands.RequireOwnerAttribute() { ErrorMessage = ErrorMessage },
                _ => throw new NotImplementedException(),
            };
    }
}
