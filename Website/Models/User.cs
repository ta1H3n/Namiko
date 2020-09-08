namespace Website.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Discriminator { get; set; }
        public string AvatarHash { get; set; }
        public string AvatarUrl { get; set; }
        public bool LoggedIn { get; set; }
    }
}
