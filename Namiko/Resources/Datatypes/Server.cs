using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Namiko.Resources.Datatypes
{
    public class Server
    {
        [Key]
        public ulong GuildId { get; set; }
        public ulong WelcomeChannelId { get; set; }
        public ulong JoinLogChannelId { get; set; }
        public ulong TeamLogChannelId { get; set; }
        public string Prefix { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LeaveDate { get; set; }
    }

    public class BlacklistedChannel
    {
        [Key]
        public ulong ChannelId { get; set; }
    }
}
