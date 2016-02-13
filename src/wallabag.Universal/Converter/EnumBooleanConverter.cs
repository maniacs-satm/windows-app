﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace wallabag.Converter
{
    public class EnumBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string parameterString = parameter as string;
            if (parameterString == null || Enum.IsDefined(value.GetType(), value) == false)
                return false;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);
            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return false;
            return Enum.Parse(targetType, parameterString);
        }
    }
}
