using System;
using System.Globalization;
using System.Windows.Data;

namespace DominoGame.Converters
{
    public class TileSizeConverter : IValueConverter
    {
        // Base size for horizontal orientation
        public double NormalWidth { get; set; } = 60;
        public double NormalHeight { get; set; } = 120;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double rotation))
                return NormalWidth;

            string param = parameter as string ?? "Width";

            // For 0 or 180 degrees, use normal orientation
            if (rotation % 180 == 0)
            {
                return param.Equals("Width", StringComparison.OrdinalIgnoreCase)
                    ? NormalWidth
                    : NormalHeight;
            }
            else
            {
                // Swap width/height for vertical orientation (90 or 270 degrees)
                return param.Equals("Width", StringComparison.OrdinalIgnoreCase)
                    ? NormalHeight
                    : NormalWidth;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
