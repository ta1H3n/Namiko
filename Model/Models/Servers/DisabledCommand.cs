using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class DisabledCommand
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public DisabledCommandType Type { get; set; }
    }
}
