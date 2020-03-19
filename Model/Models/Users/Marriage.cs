using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Marriage
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong WifeId { get; set; }
        public bool IsMarried { get; set; }
        public ulong GuildId { get; set; }
        public DateTime Date { get; set; }
    }
}
