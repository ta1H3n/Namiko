using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Addons.Handlers.Paginator
{
    public class FieldPages
    {
        public string Title { get; set; }
        public IEnumerable<string> Pages { get; set; }
        public bool Inline { get; set; } = false;
    }
}
