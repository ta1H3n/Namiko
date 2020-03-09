using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class FeaturedWaifu
    {
        [Key]
        public int id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public Waifu Waifu { get; set; }
    }
}
