using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace wallabag.Converter
{
    public sealed class BooleanNullableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
