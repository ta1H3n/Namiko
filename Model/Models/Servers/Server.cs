using System;
using System.ComponentModel.DataAnnotations;

namespace Model
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
}
