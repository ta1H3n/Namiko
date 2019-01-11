using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Namiko.Resources.Datatypes
{
    public class ReactionImage
    {
        [Key]
        public int Id { set; get; }
        public string Name { set; get; }
        public string Url { get; set; }
    }

    public class ImgurAlbumLink
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string AlbumId { get; set; }
    }
}
