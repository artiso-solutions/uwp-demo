using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MyWhiteboard
{
    public class IsPresentToBrushConverter : IValueConverter
    {
        private readonly Brush presentBrush;
        private readonly Brush awayBrush;

        public IsPresentToBrushConverter()
        {
            presentBrush = new SolidColorBrush(Colors.PaleGreen);
            awayBrush = new SolidColorBrush(Colors.IndianRed);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isPresent = (bool)value;
            return isPresent ? presentBrush : awayBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}