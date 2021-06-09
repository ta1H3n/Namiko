using System;
using System.ComponentModel.DataAnnotations;

namespace Model.Models.Users
{
    public class PremiumCodeUse
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime ClaimedAt { get; set; }

        public string PremiumCodeId { get; set; }
        public PremiumCode PremiumCode { get; set; }
    }
}
