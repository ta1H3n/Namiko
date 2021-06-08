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
        public ProType Type { get; set; }
        public DateTime ClaimDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool ExpireSent { get; set; }
    }
}
