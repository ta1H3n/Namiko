using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Invite
    {
        [Key]
        public int Id { get; set; }
        public ulong TeamId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
