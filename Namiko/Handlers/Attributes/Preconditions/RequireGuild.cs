using System;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class RequireGuildAttribute : PreconditionAttribute
    {
        public override string ErrorMessage => "You can only use this command in a server, Senpai...";
        public override string Name => "RequireGuild";

        public override Attribute ReturnAttribute(Handler handler)
            => handler switch
            {
                Handler.Interactions => new Discord.Interactions.RequireContextAttribute(Discord.Interactions.ContextType.Guild),
                Handler.Commands => new Discord.Commands.RequireContextAttribute(Discord.Commands.ContextType.Guild) { ErrorMessage = ErrorMessage },
                _ => throw new NotImplementedException(),
            };
    }
}
