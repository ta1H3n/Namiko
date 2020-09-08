namespace Website.Models
{
    public class WaifuView
    {
        public string Name { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public int Tier { get; set; }
        public int Bought { get; set; }
        public string ImageSource { get; set; }

        public string ImageMedium { get; set; }
        public string ImageLarge { get; set; }
        public string ImageRaw { get; set; }
    }
}
