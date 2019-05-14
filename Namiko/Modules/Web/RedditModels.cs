using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Namiko
{
    public class RedditPost
    {
        [Key]
        public string PermaLink { get; set; }
        public int Upvotes { get; set; }
    }
}
