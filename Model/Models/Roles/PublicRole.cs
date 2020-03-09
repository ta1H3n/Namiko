using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class PublicRole
    {
        [Key]
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
    }
}
