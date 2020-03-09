using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Voter
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
