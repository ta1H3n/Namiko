using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class MalWaifu
    {
        [Key]
        [ForeignKey(nameof(Model.Waifu))]
        public string WaifuName { get; set; }
        public long MalId { get; set; }
        public bool MalConfirmed { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual Waifu Waifu { get; set; }
    }
}
