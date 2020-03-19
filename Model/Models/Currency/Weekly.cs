using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Weekly
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
