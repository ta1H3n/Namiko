using System.ComponentModel.DataAnnotations;

namespace Namiko.Resources.Datatypes
{
    public class Toastie
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        [Key]
        public ulong UserId { get; set; }
        public int Amount { get; set; }
    }

    public class Daily
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        [Key]
        public ulong UserId { get; set; }
        public long Date { get; set; }
        public int Streak { get; set; }
    }
 
}
