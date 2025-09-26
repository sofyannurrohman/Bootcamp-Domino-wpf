using System;
using System.Globalization;
using System.Windows.Data;
using DominoGame.Models;
using DominoGame.Interfaces;

namespace DominoGame.Converters
{
    /// <summary>
    /// Returns full opacity for tiles that can be played; dimmed otherwise.
    /// Expects MultiBinding: [DominoTile, Player, Board]
    /// </summary>
    public class TilePlayableOpacityConverter : IMultiValueConverter
    {
        private const double FullyVisible = 1.0;
        private const double Dimmed = 0.5;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 3)
                return Dimmed;

            if (values[0] is not DominoTile tile) return Dimmed;
            if (values[1] is not Player) return Dimmed; // Player is unused, only needed for signature
            if (values[2] is not IBoard board) return Dimmed;

            // First move: all tiles are playable
            if (!board.Tiles.Any())
                return FullyVisible;

            // Block Domino: fully visible only if tile matches left or right end
            return tile.Matches(board.LeftEnd) || tile.Matches(board.RightEnd)
                ? FullyVisible
                : Dimmed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("TilePlayableOpacityConverter does not support ConvertBack.");
    }
}
