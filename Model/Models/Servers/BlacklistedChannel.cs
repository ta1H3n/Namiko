using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class BlacklistedChannel
    {
        [Key]
        public ulong ChannelId { get; set; }
    }
}
