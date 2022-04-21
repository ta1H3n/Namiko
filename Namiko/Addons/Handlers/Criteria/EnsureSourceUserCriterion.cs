using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers.Criteria
{
    public class EnsureSourceUserCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(ICustomContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.User.Id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
    public class EnsureComponentFromSourceUser : ICriterion<SocketMessageComponent>
    {
        public Task<bool> JudgeAsync(ICustomContext sourceContext, SocketMessageComponent parameter)
        {
            var ok = sourceContext.User.Id == parameter.User.Id;
            return Task.FromResult(ok);
        }
    }
}
