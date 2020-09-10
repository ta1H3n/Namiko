using System;
using System.Collections.Generic;
using System.Linq;

namespace Website.Models
{
    public class GuildUserView
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }

        public string Quote { get; set; }
        public string ImageUrl { get; set; }

        public int Rep { get; set; }
        public int LootboxesOpened { get; set; }

        public int Daily { get; set; }
        public int Balance { get; set; }

        public WaifuView Waifu { get; set; }

        public List<WaifuView> Waifus { get; set; }
        public int WaifuAmount { get { return Waifus.Count; } }
        public int WaifuValue { get { return Waifus.Sum(x => x.GetPrice()); } }

        public GuildSummaryView Guild { get; set; }
        public DateTimeOffset? JoinedAt { get; set; }
    }
}
