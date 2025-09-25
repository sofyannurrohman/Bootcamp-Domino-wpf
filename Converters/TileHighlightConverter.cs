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
        // Frozen brushes for performance
        private static readonly Brush HighlightBrush = CreateFrozenBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
        private static readonly Brush NormalBrush = CreateFrozenBrush(Color.FromRgb(0x2D, 0x2D, 0x30));

        private static Brush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// Returns the highlight brush if the tile is playable; otherwise normal brush.
        /// Expects: [DominoTile, Player, Board]
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 3) return NormalBrush;

            if (values[0] is not DominoTile tile) return NormalBrush;
            if (values[1] is not Player) return NormalBrush; // currently unused, kept for signature
            if (values[2] is not IBoard board) return NormalBrush;

            var boardTiles = board.Tiles;
            if (boardTiles == null || boardTiles.Count == 0)
                return HighlightBrush; // First move: highlight all tiles

            // Highlight if tile matches either end of the board
            return tile.Matches(board.LeftEnd) || tile.Matches(board.RightEnd)
                ? HighlightBrush
                : NormalBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("TileHighlightConverter does not support ConvertBack.");
    }
}
