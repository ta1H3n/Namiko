using System.Collections.Generic;

namespace Website.Models
{
    public class UserView
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public string Discriminator { get; set; }

        public string Quote { get; set; }
        public string ImageUrl { get; set; }

        public int Rep { get; set; }
        public int LootboxesOpened { get; set; }

        public List<GuildSummaryView> Guilds { get; set; }
    }
}
