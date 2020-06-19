using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Waifu
    {
        [Key]
        public string Name { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public int Tier { get; set; }
        public string ImageUrl { get; set; }
        public int Bought { get; set; }

        public virtual MalWaifu Mal { get; set; }
    }
}
