using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoGame.Interfaces.Services
{
    public interface IBoardService
    {
        bool PlaceTile(IBoard board, IDominoTile tile, bool placeLeft, IPlayer? player = null);
        (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player, IBoard board);
        bool HasPlayableTile(IPlayer player, IBoard board);
        void ClearBoard(IBoard board);
    }

}
