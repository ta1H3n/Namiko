using System;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class OwnerPrecondition : PreconditionAttribute
    {
        public override string ErrorMessage => null;
        public override string Name => "BotOwner";

        public override Attribute ReturnAttribute(Handler handler)
            => handler switch
            {
                Handler.Interactions => new Discord.Interactions.RequireOwnerAttribute(),
                Handler.Commands => new Discord.Commands.RequireOwnerAttribute() { ErrorMessage = ErrorMessage },
                _ => throw new NotImplementedException(),
            };
    }
}
