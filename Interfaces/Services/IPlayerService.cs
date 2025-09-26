using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoGame.Interfaces.Services
{
    public interface IPlayerService
    {
        void RemoveTileFromHand(IPlayer player, IDominoTile tile);
        IPlayer? GetRoundWinner(IEnumerable<IPlayer> players);
        IPlayer? GetGameWinner(IEnumerable<IPlayer> players);
        void ResetScores(IEnumerable<IPlayer> players);
        bool CanPlayTile(IPlayer player, IBoard board, IBoardService boardService);
    }
}
