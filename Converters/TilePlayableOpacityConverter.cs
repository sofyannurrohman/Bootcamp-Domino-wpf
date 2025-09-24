using System;
using System.Globalization;
using System.Windows.Data;
using DominoGame.Models;
using DominoGame.Controllers;

namespace DominoGame.Converters
{
    public class TilePlayableOpacityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0.5;

            if (values[0] is DominoTile tile && values[1] is DominoGameController game)
            {
                if (game.Board.Count == 0) return 1.0; // first tile is always playable

                int leftEnd = game.Board.First().Left;
                int rightEnd = game.Board.Last().Right;

                bool playable = tile.Matches(leftEnd) || tile.Matches(rightEnd);
                return playable ? 1.0 : 1.0; // full opacity for playable tiles
            }

            return 1.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
