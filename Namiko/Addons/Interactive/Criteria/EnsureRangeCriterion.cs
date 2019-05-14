using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Discord.Addons.Interactive
{
    public class EnsureRangeCriterion : ICriterion<IMessage>
    {
        private readonly int _range;

        public EnsureRangeCriterion(int range)
            => _range = range;

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            bool ok = false;
            try
            {
                int x = Int32.Parse(parameter.Content);
                ok = _range >= x && x > 0;
            } catch { }
            return Task.FromResult(ok);
        }
    }
}
