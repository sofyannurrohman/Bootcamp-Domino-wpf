using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using DominoGame.Models;
using DominoGame.Interfaces;

namespace DominoGame.Converters
{
    /// <summary>
    /// Determines whether a DominoTile is playable for the current player on the current board.
    /// MultiBinding values expected: [DominoTile, Player, Board]
    /// </summary>
    public class TilePlayableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 3)
                return false;

            if (values[0] is not DominoTile tile) return false;
            if (values[1] is not Player player) return false;
            if (values[2] is not IBoard board) return false;

            // First move: opening rule
            return !board.Tiles.Any()
                ? CanPlayOpeningTile(tile, player)
                : CanPlayRegularTile(tile, board);
        }

        /// <summary>
        /// Opening rule (first tile of the round):
        /// - If player has doubles → only the largest double is playable.
        /// - If no doubles → only the tile with the highest pip sum is playable.
        /// </summary>
        private static bool CanPlayOpeningTile(DominoTile tile, Player player)
        {
            if (player.Hand == null || !player.Hand.Any())
                return false;

            var doubles = player.Hand.Where(t => t.IsDouble).ToList();
            if (doubles.Any())
            {
                int maxDouble = doubles.Max(t => t.Left);
                return tile.IsDouble && tile.Left == maxDouble;
            }

            int maxPip = player.Hand.Max(t => t.TotalPip);
            return tile.TotalPip == maxPip;
        }

        /// <summary>
        /// Regular play: tile must match either end of the board.
        /// </summary>
        private static bool CanPlayRegularTile(DominoTile tile, IBoard board)
        {
            if (board.Tiles == null || !board.Tiles.Any())
                return false;

            return tile.Matches(board.LeftEnd) || tile.Matches(board.RightEnd);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("TilePlayableConverter does not support ConvertBack.");
    }
}
