using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Team
    {
        [Key]
        public ulong LeaderRoleId { get; set; }
        public ulong MemberRoleId { get; set; }
        public ulong GuildId { get; set; }
    }
}
