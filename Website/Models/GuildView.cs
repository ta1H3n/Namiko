using System.Collections.Generic;

namespace Website.Models
{
    public class GuildView : GuildSummaryView
    {
        public Dictionary<string, int> WaifusOwned { get; set; }
        public Dictionary<string, int> Toasties { get; set; }
        public Dictionary<string, int> WaifuValue { get; set; }
    }
}
