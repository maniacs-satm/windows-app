using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace wallabag.Converter
{
    public sealed class FavoriteToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            System.Diagnostics.Debug.WriteLine("Convert favorite to icon: " + value.ToString());
            return (value is bool && (bool)value) ? new FontIcon() { Glyph = "", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets") } : new FontIcon() { Glyph = "", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets") };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
