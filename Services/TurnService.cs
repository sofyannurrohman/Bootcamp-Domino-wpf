using DominoGame.Interfaces;
using DominoGame.Interfaces.Services;
using System.Collections.Generic;

namespace DominoGame.Services
{
    public class TurnService : ITurnService
    {
        // Use IBoardService, not BoardService
        public int NextTurn(List<IPlayer> players, int currentPlayerIndex, IBoardService boardService, IBoard board, out IPlayer nextPlayer)
        {
            if (players.Count == 0)
            {
                nextPlayer = null!;
                return -1;
            }

            int startIndex = currentPlayerIndex;

            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

                if (boardService.HasPlayableTile(players[currentPlayerIndex], board))
                {
                    nextPlayer = players[currentPlayerIndex];
                    return currentPlayerIndex;
                }

                // Looped through all players without finding a playable tile
                if (currentPlayerIndex == startIndex)
                    break;

            } while (true);

            // No one can play, keep current player
            nextPlayer = players[startIndex];
            return startIndex;
        }
    }
}
