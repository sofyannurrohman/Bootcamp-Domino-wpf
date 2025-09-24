using System;
using System.Globalization;
using System.Windows.Data;

namespace DominoGame.Converters
{
    public class TileSizeConverter : IValueConverter
    {
        public double NormalWidth { get; set; } = 60;
        public double NormalHeight { get; set; } = 120;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double rotation)) return NormalWidth;

            string param = parameter as string;
            if (rotation % 180 == 0)
            {
                return param == "Width" ? NormalWidth : NormalHeight;
            }
            else
            {
                return param == "Width" ? NormalHeight : NormalWidth;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
