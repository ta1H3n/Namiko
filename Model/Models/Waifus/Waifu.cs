using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
        public string ImageSource { get; set; }

        public virtual MalWaifu Mal { get; set; }

        [NotMapped]
        private string ImgurId { get { return ImageUrl?.Split('/').Last().Split('.').First(); } }
        [NotMapped]
        private string ImageFileType { get { return ImageUrl?.Split('.').Last(); } }
        [NotMapped]
        public string ImageMedium { get { return Name + "+" + ImgurId + "m." + ImageFileType; } }
        [NotMapped]
        public string ImageLarge { get { return Name + "+" + ImgurId + "l." + ImageFileType; } }
        [NotMapped]
        public string ImageRaw { get { return Name + "+" + ImgurId + "." + ImageFileType; } }

        public int GetPrice()
        {
            return Tier == 1 ? 20000 :
                Tier == 2 ? 10000 :
                Tier == 3 ? 5000 :
                Tier == 0 ? 100000 :

                //default
                0;
        }
    }
}
