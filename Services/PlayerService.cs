using DominoGame.Interfaces;
using DominoGame.Models;
using DominoGame.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Services
{
    public class PlayerService : IPlayerService
    {
        public bool CanPlayTile(IPlayer player, IBoard board, IBoardService boardService)
        {
            return boardService.HasPlayableTile(player, board);
        }

        public void RemoveTileFromHand(IPlayer player, IDominoTile tile)
        {
            player.Hand.Remove(tile);
        }

        public void ResetScores(IEnumerable<IPlayer> players)
        {
            foreach (var player in players)
                player.Score = 0;
        }

        public IPlayer? GetRoundWinner(IEnumerable<IPlayer> players)
        {
            var playerEmpty = players.FirstOrDefault(p => !p.Hand.Any());
            if (playerEmpty != null) return playerEmpty;

            var minSum = players.Min(p => p.Hand.Sum(t => t.PipLeft + t.PipRight));
            var winners = players.Where(p => p.Hand.Sum(t => t.PipLeft + t.PipRight) == minSum).ToList();
            return winners.Count == 1 ? winners[0] : null;
        }

        public IPlayer? GetGameWinner(IEnumerable<IPlayer> players)
        {
            return players.OrderByDescending(p => p.Score).FirstOrDefault();
        }
    }
}
