using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class UserInventory
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }

        public string WaifuName { get; set; }
        public Waifu Waifu { get; set; }
        public DateTime DateBought { get; set; }
    }
}
