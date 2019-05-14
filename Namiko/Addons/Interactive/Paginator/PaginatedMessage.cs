using System.Collections.Generic;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessage
    {
        public IEnumerable<object> Pages { get; set; }
        public IEnumerable<FieldPages> Fields { get; set; }

        public string Content { get; set; } = "";

        public EmbedAuthorBuilder Author { get; set; } = null;
        public Color Color { get; set; } = Color.Default;
        public string ImageUrl { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public string Title { get; set; } = "";
        public int PageCount { get; set; } = -1;

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}
