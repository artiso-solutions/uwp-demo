namespace MyWhiteboard.Service.Controllers
{
    public class BackgroundImageDescription
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public string ThumbnailUri { get; set; }

        public string ImageUri { get; set; }
    }
}