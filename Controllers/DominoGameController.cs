using DominoGame.Interfaces;
using DominoGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Controllers
{
    public class DominoGameController : IGameController
    {
        public List<DominoTile> Board { get; private set; } = new();
        public List<Player> Players { get; } = new();
        private int currentPlayerIndex = 0;

        public Player CurrentPlayer => Players[currentPlayerIndex];

        // Events
        public event Action<Player, DominoTile, bool>? OnTilePlayed;
        public event Action<Player>? OnGameOver;

        /// <summary>
        /// Start a new game with shuffled tiles and 2 players.
        /// </summary>
        public void StartGame()
        {
            var tiles = ShuffleTiles(GenerateTiles());

            var player1 = new Player("You");
            var player2 = new Player("Computer");

            DealTiles(tiles, player1, player2);

            Players.Clear();
            Players.Add(player1);
            Players.Add(player2);

            Board.Clear();
            currentPlayerIndex = 0;
        }

        /// <summary>
        /// Generate standard double-six domino tiles (0-0 up to 6-6).
        /// </summary>
        private List<DominoTile> GenerateTiles()
        {
            var tiles = new List<DominoTile>();
            for (int i = 0; i <= 6; i++)
            {
                for (int j = i; j <= 6; j++)
                {
                    tiles.Add(new DominoTile(i, j));
                }
            }
            return tiles;
        }

        private List<DominoTile> ShuffleTiles(List<DominoTile> tiles)
        {
            var rnd = new Random();
            return tiles.OrderBy(_ => rnd.Next()).ToList();
        }

        private void DealTiles(List<DominoTile> tiles, Player player1, Player player2)
        {
            player1.Hand.AddRange(tiles.Take(7));
            player2.Hand.AddRange(tiles.Skip(7).Take(7));
        }

        /// <summary>
        /// Attempt to play a tile on the board.
        /// </summary>
        public bool PlayTile(DominoTile tile, bool placeLeft)
        {
            bool played = false;

            if (Board.Count == 0)
            {
                played = PlaceFirstTile(tile);
            }
            else
            {
                int leftEnd = Board.First().Left;
                int rightEnd = Board.Last().Right;

                played = placeLeft
                    ? TryPlaceLeft(tile, leftEnd)
                    : TryPlaceRight(tile, rightEnd);
            }

            if (played)
            {
                OnTilePlayed?.Invoke(CurrentPlayer, tile, placeLeft);

                if (IsGameOver())
                {
                    OnGameOver?.Invoke(GetWinner());
                }
            }

            return played;
        }

        private bool PlaceFirstTile(DominoTile tile)
        {
            tile.RotationAngle = 0;
            Board.Add(tile);
            CurrentPlayer.Hand.Remove(tile);
            return true;
        }

        private bool TryPlaceLeft(DominoTile tile, int leftEnd)
        {
            if (!tile.Matches(leftEnd)) return false;

            var playTile = tile.Left == leftEnd ? tile.FlippedTile() : tile;
            playTile.RotationAngle = 0;
            Board.Insert(0, playTile);
            CurrentPlayer.Hand.Remove(tile);
            return true;
        }

        private bool TryPlaceRight(DominoTile tile, int rightEnd)
        {
            if (!tile.Matches(rightEnd)) return false;

            var playTile = tile.Right == rightEnd ? tile.FlippedTile() : tile;
            playTile.RotationAngle = 0;
            Board.Add(playTile);
            CurrentPlayer.Hand.Remove(tile);
            return true;
        }

        public void NextTurn()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
        }

        public bool IsGameOver()
        {
            // Game ends if any player has no tiles
            if (Players.Any(p => p.Hand.Count == 0))
                return true;

            if (Board.Count == 0)
                return false;

            int leftEnd = Board.First().Left;
            int rightEnd = Board.Last().Right;

            // Game ends if all players cannot play any tile
            return Players.All(p => !p.HasPlayableTile(leftEnd, rightEnd));
        }

        public Player GetWinner()
        {
            // Player with least sum of tile pips wins
            return Players.OrderBy(p => p.Hand.Sum(t => t.Left + t.Right)).First();
        }
    }
}
