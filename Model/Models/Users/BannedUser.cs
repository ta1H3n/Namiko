using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class BannedUser
    {
        [Key]
        public int Id { get; set; }
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public DateTime DateBanStart { get; set; }
        public DateTime DateBanEnd { get; set; }
        public bool Active { get; set; }
    }
}
