using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class SpecialChannel
    {
        [Key]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public ChannelType Type { get; set; }
        public string Args { get; set; }
        public ulong GuildId { get; set; }
    }
}
