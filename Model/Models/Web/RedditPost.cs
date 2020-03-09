using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class RedditPost
    {
        [Key]
        public string PermaLink { get; set; }
        public int Upvotes { get; set; }
    }
}
