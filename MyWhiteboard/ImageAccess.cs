using System;
using Windows.UI.Xaml.Media.Imaging;

namespace MyWhiteboard
{
    public static class ImageAccess
    {
        public static BitmapImage LoadImage(string url)
        {
            return new BitmapImage(new Uri(url, UriKind.Absolute));
        }
    }
}