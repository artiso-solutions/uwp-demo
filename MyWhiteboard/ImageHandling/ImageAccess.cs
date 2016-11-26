using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace MyWhiteboard.ImageHandling
{
    public static class ImageAccess
    {
        public static BitmapImage LoadImage(string url)
        {
            return new BitmapImage(new Uri(url, UriKind.Absolute));
        }

        public static async Task<BitmapImage> LoadCurrentBackgroundImageAsync()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(new Uri($"{Consts.ApiControllerBaseUrl}GetCurrentBackgroundImage"));
            var uri = await response.Content.ReadAsAsync<string>();
            if (uri == null)
            {
                return null;
            }

            return LoadImage(uri);
        }
    }
}