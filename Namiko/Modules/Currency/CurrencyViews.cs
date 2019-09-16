using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko
{
    public class LeaderboardEntryName
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class LeaderboardEntryId
    {
        public ulong Id { get; set; }
        public int Count { get; set; }
    }
}
