using System.Threading.Tasks;

namespace Namiko.Addons.Handlers.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(ICustomContext sourceContext, T parameter);
    }
}
