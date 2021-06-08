using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.Models.Users
{
    public class PremiumCode
    {
        [Key]
        public string Id { get; set; }
        public ProType Type { get; set; }
        public int DurationDays { get; set; }
        public int UsesLeft { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public IEnumerable<PremiumCodeUse> Uses { get; set; }
    }
}
