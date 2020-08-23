using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Profile
    {
        [Key]
        public ulong UserId { get; set; }
        public string ColorHex { get; set; }
        public string PriorColorHexStack { get; set; }
        public string Quote { get; set; }
        public string Image { get; set; }
        public int LootboxesOpened { get; set; }
        public int Rep { get; set; }
        public string Name { get; set; }
        public string Discriminator { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime RepDate { get; set; }
    }
}
