using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Daily
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public long Date { get; set; }
        public int Streak { get; set; }
    }
}
