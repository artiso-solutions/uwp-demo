using Microsoft.WindowsAzure.Storage.Table;

namespace ImageReview.Service.Controllers
{
    public class BackgroundImageEntity : TableEntity
    {
        public const string AnonymousPartitionKey = "Anonymous";

        public BackgroundImageEntity()
        {
        }

        public BackgroundImageEntity(BackgroundImageDescription backgroundImageDescription)
        {
            PartitionKey = AnonymousPartitionKey;
            RowKey = backgroundImageDescription.Id.ToString();
            Description = backgroundImageDescription.Description;
            ImageUri = backgroundImageDescription.ImageUri;
            ThumbnailUri = backgroundImageDescription.ThumbnailUri;
        }

        public string Description { get; set; }

        public string ImageUri { get; set; }

        public string ThumbnailUri { get; set; }

        public BackgroundImageDescription ToBackgroundImageDescription()
        {
            return new BackgroundImageDescription
            {
                Id = int.Parse(RowKey),
                Description = Description,
                ImageUri = ImageUri,
                ThumbnailUri = ThumbnailUri
            };
        }
    }
}