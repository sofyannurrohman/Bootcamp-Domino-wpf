using System;
using System.Globalization;
using System.Windows.Data;
using DominoGame.Models;
using DominoGame.Controllers;

namespace DominoGame.Converters
{
    public class TilePlayableConverter : IMultiValueConverter
    {
        // values[0] = DominoTile
        // values[1] = DominoGameController (DataContext)
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is not DominoTile tile) return false;
            if (values[1] is not Player player) return false;
            if (values[2] is not List<DominoTile> board) return false;

            if (board.Count == 0) return true; // first tile is always playable
            int leftEnd = board.First().Left;
            int rightEnd = board.Last().Right;

            return tile.Matches(leftEnd) || tile.Matches(rightEnd);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
