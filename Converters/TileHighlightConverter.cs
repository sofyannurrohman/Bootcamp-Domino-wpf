using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DominoGame.Models;
using DominoGame.Controllers;

namespace DominoGame.Converters
{
    public class TileHighlightConverter : IMultiValueConverter
    {
        private static readonly Brush HighlightBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // Green
        private static readonly Brush NormalBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C)); // Gray

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return NormalBrush;

            var tile = values[0] as DominoTile;
            var controller = values[1] as DominoGameController;

            if (tile == null || controller == null) return NormalBrush;

            if (controller.Board.Count == 0)
                return HighlightBrush;

            int leftEnd = controller.Board.First().Left;
            int rightEnd = controller.Board.Last().Right;

            return tile.Matches(leftEnd) || tile.Matches(rightEnd) ? HighlightBrush : NormalBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
