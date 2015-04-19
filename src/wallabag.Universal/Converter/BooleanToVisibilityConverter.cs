using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace wallabag.Converter
{
    /// <summary>
    /// Wertkonverter, der TRUE in <see cref="Visibility.Visible"/> und FALSE in
    /// <see cref="Visibility.Collapsed"/> übersetzt.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
