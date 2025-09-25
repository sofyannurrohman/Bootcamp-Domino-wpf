using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using DominoGame.Models;
using DominoGame.Interfaces;

namespace DominoGame.Converters
{
    public class TileHighlightConverter : IMultiValueConverter
    {
        private static readonly Brush HighlightBrush = CreateFrozenBrush(Color.FromRgb(0x3C, 0x3C, 0x3C)); // Green
        private static readonly Brush NormalBrush = CreateFrozenBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));    // Gray

        private static Brush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Expecting: [DominoTile, Player, Board]
            if (values is null || values.Length != 3)
                return NormalBrush;

            if (values[0] is not DominoTile tile) return NormalBrush;
            if (values[1] is not Player) return NormalBrush; // currently unused, signature for MultiBinding
            if (values[2] is not IBoard board) return NormalBrush;

            var tiles = board.Tiles;
            if (tiles == null || tiles.Count == 0)
                return HighlightBrush; // first move: highlight all tiles

            // Highlight if tile matches either end of the board
            return tile.Matches(board.LeftEnd) || tile.Matches(board.RightEnd)
                ? HighlightBrush
                : NormalBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("TileHighlightConverter does not support ConvertBack.");
    }
}
