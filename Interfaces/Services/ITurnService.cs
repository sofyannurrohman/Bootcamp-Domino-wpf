using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoGame.Interfaces.Services
{
    public interface ITurnService
    {
        int NextTurn(List<IPlayer> players, int currentPlayerIndex, IBoardService boardService, IBoard board, out IPlayer nextPlayer);
    }
}
