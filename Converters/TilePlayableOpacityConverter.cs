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
            // Expecting: [DominoTile, Player, Board]
            if (values is null || values.Length != 3)
                return Dimmed;

            if (values[0] is not DominoTile tile) return Dimmed;
            if (values[1] is not Player currentPlayer) return Dimmed;
            if (values[2] is not IBoard board) return Dimmed;

            // Opening move: all tiles fully visible
            if (board.Tiles.Count == 0)
                return FullyVisible;

            // Regular move: fully visible if playable, otherwise dimmed
            return tile.Matches(board.LeftEnd) || tile.Matches(board.RightEnd)
                ? FullyVisible
                : Dimmed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("TilePlayableOpacityConverter does not support ConvertBack.");
    }
}
