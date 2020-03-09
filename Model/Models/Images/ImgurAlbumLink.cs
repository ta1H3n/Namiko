using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class ImgurAlbumLink
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string AlbumId { get; set; }
    }
}
