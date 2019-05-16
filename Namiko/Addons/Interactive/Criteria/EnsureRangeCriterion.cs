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
        private readonly string _except;

        public EnsureRangeCriterion(int range, string exceptStartsWith = null)
        { 
            _range = range;
            _except = exceptStartsWith;
        }


        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            bool ok = false;

            if (_except != null && parameter.Content.StartsWith(_except))
                return Task.FromResult(true);

            try
            {
                int x = Int32.Parse(parameter.Content);
                ok = _range >= x && x > 0;
            } catch { }
            return Task.FromResult(ok);
        }
    }
}
