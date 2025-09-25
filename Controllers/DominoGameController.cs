using DominoGame.Interfaces;
using DominoGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Controllers
{
    public class DominoGameController : IGameController
    {
        public IBoard Board { get; private set; } = new Board();
        public List<Player> Players { get; } = new();
        private int currentPlayerIndex = 0;

        public Player? CurrentPlayer => Players.Count > 0 ? Players[currentPlayerIndex] : null;


        public event Action<Player, DominoTile, bool>? OnTilePlayed;
        public event Action<Player>? OnGameOver;

        public void StartGame()
        {
            Board.Clear();
            currentPlayerIndex = 0;

            var tiles = ShuffleTiles(GenerateTiles());
            var player1 = new Player("You");
            var player2 = new Player("Computer");

            DealTiles(tiles, player1, player2);

            Players.Clear();
            Players.Add(player1);
            Players.Add(player2);
        }

        #region Tile Management

        private List<DominoTile> GenerateTiles()
        {
            var tiles = new List<DominoTile>();
            for (int i = 0; i <= 6; i++)
                for (int j = i; j <= 6; j++)
                    tiles.Add(new DominoTile(i, j));
            return tiles;
        }

        private List<DominoTile> ShuffleTiles(List<DominoTile> tiles)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            return tiles.OrderBy(_ => rnd.Next()).ToList();
        }

        private void DealTiles(List<DominoTile> tiles, Player player1, Player player2)
        {
            const int handSize = 7;
            player1.Hand.AddRange(tiles.Take(handSize));
            player2.Hand.AddRange(tiles.Skip(handSize).Take(handSize));
        }

        public bool PlayTile(DominoTile tile, bool placeLeft)
        {
            bool success = Board.PlaceTile(tile, placeLeft);
            if (!success) return false;

            CurrentPlayer.Hand.Remove(tile);
            OnTilePlayed?.Invoke(CurrentPlayer, tile, placeLeft);

            if (IsGameOver())
                OnGameOver?.Invoke(GetWinner());

            return true;
        }

        #endregion

        #region Game Flow

        public void NextTurn()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
        }

        public bool IsGameOver()
        {
            if (Players.Any(p => p.Hand.Count == 0))
                return true;

            var leftEnd = Board.LeftEnd;
            var rightEnd = Board.RightEnd;

            return Players.All(p => !p.HasPlayableTile(leftEnd, rightEnd));
        }

        public Player GetWinner()
        {
            return Players.OrderBy(p => p.Hand.Sum(t => t.Left + t.Right)).First();
        }

        #endregion
    }
}
