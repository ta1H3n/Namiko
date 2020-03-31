using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class BanroyaleMessage
    {
        [Key]
        public int Id { get; set; }
        public ulong MessageId { get; set; }
        public ulong EmoteId { get; set; }
        public bool Active { get; set; }
        public int BanroyaleId { get; set; }
        public Banroyale Banroyale { get; set; }
    }
}
