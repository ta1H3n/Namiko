using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class LootBox
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public LootBoxType Type { get; set; }
        public int Amount { get; set; }
        public ulong GuildId { get; set; }
    }
}
