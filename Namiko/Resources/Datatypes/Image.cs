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
}
