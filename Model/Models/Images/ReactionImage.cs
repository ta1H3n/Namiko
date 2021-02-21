using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Model
{
    public class ReactionImage
    {
        [Key]
        public int Id { set; get; }
        public string Name { set; get; }
        public string Url { get; set; }
        public ulong GuildId { get; set; }

        [NotMapped]
        public string ImageFileType { get { return Url?.Split('.').Last(); } }
    }
}
