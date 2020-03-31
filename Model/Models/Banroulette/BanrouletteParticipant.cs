using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class BanrouletteParticipant
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public Banroulette Banroulette { get; set; }
    }
}
