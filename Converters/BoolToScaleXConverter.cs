﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace DominoGame.Converters
{
    public class BoolToScaleXConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? -1.0 : 1.0;
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return d < 0;
            return false;
        }
    }
}
