using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Balance
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int Amount { get; set; }
    }
}
