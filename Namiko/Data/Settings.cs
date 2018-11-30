using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Resources.Datatypes
{
    class Settings
    {
        public string Token { get; set; }
        public ulong Owner { get; set; }
        public ulong home_server { get; set; }
        public ulong log_channel { get; set; }
        public string Version { get; set; }
        public string Prefix { get; set; }
    }
}
