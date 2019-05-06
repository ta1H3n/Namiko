using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Namiko.Resources.Datatypes
{
    public class RedditPost
    {
        [Key]
        public string FullName { get; set; }
        public string PermaLink { get; set; }
    }
}
