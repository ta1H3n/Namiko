using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class ReactionImage
    {
        [Key]
        public int Id { set; get; }
        public string Name { set; get; }
        public string Url { get; set; }
        public ulong GuildId { get; set; }
    }
}
