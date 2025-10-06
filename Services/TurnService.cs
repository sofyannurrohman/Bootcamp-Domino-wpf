using DominoGame.Interfaces;
using DominoGame.Interfaces.Services;
using System.Collections.Generic;

namespace DominoGame.Services
{
    public class TurnService : ITurnService
    {
        public int NextTurn(List<IPlayer> players, int currentPlayerIndex, IBoardService boardService, IBoard board, out IPlayer nextPlayer)
        {
            if (players.Count == 0)
            {
                nextPlayer = null!;
                return -1;
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            nextPlayer = players[currentPlayerIndex];

            return currentPlayerIndex;
        }
    }
}
