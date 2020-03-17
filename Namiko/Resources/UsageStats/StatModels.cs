using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Namiko
{
    public class ServerStat
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class CommandStat
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class UsageStat
    {
        [Key]
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class CommandLog
    {
        [Key]
        public int Id { get; set; }
        public string Command { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Date { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
        public bool Success { get; set; }
    }
}
