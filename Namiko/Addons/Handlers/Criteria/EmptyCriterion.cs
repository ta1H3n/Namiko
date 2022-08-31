using System.Threading.Tasks;

namespace Namiko.Addons.Handlers.Criteria
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(ICustomContext sourceContext, T parameter)
            => Task.FromResult(true);
    }
}
