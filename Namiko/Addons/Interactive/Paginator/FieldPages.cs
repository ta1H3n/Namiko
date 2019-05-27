using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Addons.Interactive
{
    public class FieldPages
    {
        public string Title { get; set; }
        public IEnumerable<string> Pages { get; set; } = new List<string>();
        public bool Inline { get; set; } = false;
    }
}
