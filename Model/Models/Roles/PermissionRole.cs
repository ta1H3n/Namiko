using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class PermissionRole
    {
        [Key]
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
        public RoleType Type { get; set; }
    }
}
