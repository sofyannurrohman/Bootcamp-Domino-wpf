using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DominoGame.Models;
using DominoGame.Interfaces;

namespace DominoGame.Converters
{
    /// <summary>
    /// Returns a highlight brush if the tile is playable; otherwise normal brush.
    /// Expects MultiBinding: [DominoTile, Player, Board]
    /// </summary>
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

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 3)
                return NormalBrush;

            if (values[0] is not DominoTile tile) return NormalBrush;
            if (values[1] is not Player player) return NormalBrush;
            if (values[2] is not IBoard board) return NormalBrush;

            // ✅ Correct call (use the shared rule class)
            bool isPlayable = TilePlayRules.CanPlay(tile, player, board);

            return isPlayable ? HighlightBrush : NormalBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("TileHighlightConverter does not support ConvertBack.");
    }
}
