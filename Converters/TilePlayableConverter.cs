using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using DominoGame.Models;
using DominoGame.Interfaces;

namespace DominoGame.Converters
{
    public class TilePlayableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 3 ||
                values[0] is not IDominoTile tile ||
                values[1] is not IPlayer player ||
                values[2] is not IBoard board)
                return false;

            return TilePlayRules.CanPlay(tile, player, board);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("TilePlayableConverter does not support ConvertBack.");
    }

    public static class TilePlayRules
    {
        public static bool CanPlay(IDominoTile tile, IPlayer player, IBoard board)
        {
            if (tile == null || player == null || board == null)
                return false;

            return !board.Tiles.Any()
                ? CanPlayOpeningTile(tile, player)
                : CanPlayRegularTile(tile, board);
        }

        private static bool CanPlayOpeningTile(IDominoTile tile, IPlayer player)
        {
            if (player.Hand == null || !player.Hand.Any())
                return false;

            var doubles = player.Hand.Where(t => t.IsDouble).ToList();
            if (doubles.Any())
            {
                int maxDouble = doubles.Max(t => t.PipLeft);
                return tile.IsDouble && tile.PipLeft == maxDouble;
            }

            int maxPip = player.Hand.Max(t => t.TotalPip);
            return tile.TotalPip == maxPip;
        }

        private static bool CanPlayRegularTile(IDominoTile tile, IBoard board)
        {
            if (board.Tiles == null || !board.Tiles.Any())
                return false;

            return tile.Matches(board.LeftEnd) || tile.Matches(board.RightEnd);
        }
    }
}
