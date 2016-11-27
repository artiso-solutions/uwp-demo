using System;
using System.Collections.Generic;
using System.Web.Http;

namespace MyWhiteboard.Service.Controllers
{
    public class ReviewController : ApiController
    {
        [HttpGet]
        public List<BackgroundImageDescription> GetImageDescriptions()
        {
            return new List<BackgroundImageDescription>
            {
                GetImageDescriptionForImage(1, "Car 1", "car1.png"),
                GetImageDescriptionForImage(2, "Car 2", "car2.png"),
                GetImageDescriptionForImage(3, "Machine 1", "machine1.png"),
                GetImageDescriptionForImage(4, "Machine 1", "machine2.png")
            };
        }

        private BackgroundImageDescription GetImageDescriptionForImage(int id, string description, string fileName)
        {
            var baseUri = new Uri(Request.RequestUri, RequestContext.VirtualPathRoot);
            return new BackgroundImageDescription { Id = id, Description = description, ThumbnailUri = $"{baseUri}thumbnails/{fileName}", ImageUri = $"{baseUri}{fileName}" };
        }
    }
}