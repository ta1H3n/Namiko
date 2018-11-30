using System.ComponentModel.DataAnnotations;

namespace Namiko.Resources.Datatypes
{
    public class Toastie
    {
        [Key]
        public ulong UserId { get; set; }
        public int Amount { get; set; }
    }

    public class Daily
    {
        [Key]
        public ulong UserId { get; set; }
        public long Date { get; set; }
        public int Streak { get; set; }
    }
 
}
