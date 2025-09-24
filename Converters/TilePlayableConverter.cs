using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using DominoGame.Models;
using System.Collections.Generic;

namespace DominoGame.Converters
{
    public class TilePlayableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null || values.Length < 3)
                return false;

            if (values[0] is not DominoTile tile ||
                values[1] is not Player player ||
                values[2] is not List<DominoTile> board)
                return false;

            return board.Count == 0
                ? CanPlayOpeningTile(tile, player)
                : CanPlayRegularTile(tile, board);
        }

        /// <summary>
        /// Rule pembukaan (Domino Block):
        /// - Kalau ada double → hanya double terbesar yang playable.
        /// - Kalau tidak ada double → tile dengan pip terbesar playable.
        /// </summary>
        private static bool CanPlayOpeningTile(DominoTile tile, Player player)
        {
            if (player.Hand is null || player.Hand.Count == 0)
                return false;

            var doubles = player.Hand.Where(t => t.IsDouble).ToList();
            if (doubles.Count > 0)
            {
                int maxDouble = doubles.Max(t => t.Left);
                return tile.IsDouble && tile.Left == maxDouble;
            }

            int maxPip = player.Hand.Max(t => t.TotalPip);
            return tile.TotalPip == maxPip;
        }

        /// <summary>
        /// Rule reguler: tile harus cocok dengan salah satu ujung board.
        /// </summary>
        private static bool CanPlayRegularTile(DominoTile tile, List<DominoTile> board)
        {
            if (board is null || board.Count == 0)
                return false;

            int leftEnd = board.First().Left;
            int rightEnd = board.Last().Right;

            return tile.Matches(leftEnd) || tile.Matches(rightEnd);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
