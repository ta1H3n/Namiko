using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Premium
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public PremiumType Type { get; set; }
        public DateTime ClaimDate { get; set; }
    }
}
