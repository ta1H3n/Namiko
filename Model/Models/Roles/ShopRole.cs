using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class ShopRole
    {
        [Key]
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
        public int Price { get; set; }
    }
}
