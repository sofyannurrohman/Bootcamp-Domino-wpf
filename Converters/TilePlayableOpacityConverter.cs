using System;
using System.Globalization;
using System.Windows.Data;
using DominoGame.Models;
using DominoGame.Interfaces;

namespace DominoGame.Converters
{
    public class TilePlayableOpacityConverter : IMultiValueConverter
    {
        private const double FullyVisible = 1.0;
        private const double Dimmed = 0.5;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 3)
                return Dimmed;

            if (values[0] is not IDominoTile tile) return Dimmed;
            if (values[1] is not IPlayer) return Dimmed;
            if (values[2] is not IBoard board) return Dimmed;

          
            if (!board.Tiles.Any())
                return FullyVisible;

            
            return tile.Matches(board.LeftEnd) || tile.Matches(board.RightEnd)
                ? FullyVisible
                : Dimmed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("TilePlayableOpacityConverter does not support ConvertBack.");
    }
}
