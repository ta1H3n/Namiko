using System.Collections.Generic;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessage
    {
        public IEnumerable<object> Pages { get; set; } = new List<FieldPages>();
        public IEnumerable<FieldPages> Fields { get; set; } = new List<FieldPages>();

        public string MessageText { get; set; } = "";

        public EmbedAuthorBuilder Author { get; set; } = null;
        public Color Color { get; set; } = Color.Default;
        public string ImageUrl { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public string Title { get; set; } = "";
        public int PageCount { get; set; } = -1;
        public string Footer { get; set; } = "";

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}
