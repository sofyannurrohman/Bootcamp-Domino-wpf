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
        public Player CurrentPlayer => Players[currentPlayerIndex];

        public int CurrentRound { get; private set; } = 0;
        public int MaxRounds { get; private set; } = 5;

        public event Action<Player, DominoTile, bool>? OnTilePlayed;
        public event Action<Player>? OnRoundOver;
        public event Action<Player>? OnGameOver;

        private readonly List<DominoTile> boneyard = new();

        #region Game Setup

        public void StartGame(int maxRounds = 5)
        {
            Players.Clear();
            Players.Add(new Player("You"));
            Players.Add(new Player("Computer"));

            MaxRounds = maxRounds;
            CurrentRound = 0;

            foreach (var player in Players)
                player.Score = 0;

            StartNewRound();
        }

        private void StartNewRound()
        {
            CurrentRound++;
            Board.Clear();
            currentPlayerIndex = 0;

            foreach (var player in Players)
                player.Hand.Clear();

            boneyard.Clear();

            var tiles = ShuffleTiles(GenerateTiles());
            DealTiles(tiles, Players);
            boneyard.AddRange(tiles.Skip(Players.Count * 7));
        }

        public void StartNextRound() => StartNewRound();

        #endregion

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

        private void DealTiles(List<DominoTile> tiles, List<Player> players)
        {
            const int handSize = 7;
            for (int i = 0; i < players.Count; i++)
                players[i].Hand.AddRange(tiles.Skip(i * handSize).Take(handSize));
        }

        public DominoTile? DrawTile(Player player)
        {
            if (!boneyard.Any()) return null;

            var tile = boneyard[0];
            boneyard.RemoveAt(0);
            player.Hand.Add(tile);
            return tile;
        }

        public bool PlayTile(Player player, DominoTile tile, bool placeLeft)
        {
            if (player != CurrentPlayer)
                return false;

            if (!Board.PlaceTile(tile, placeLeft))
                return false;

            player.Hand.Remove(tile);
            OnTilePlayed?.Invoke(player, tile, placeLeft);
            return true;
        }

        public DominoTile? GetPlayableTile(Player player)
        {
            if (!Board.Any())
                return player.Hand.FirstOrDefault();

            int leftEnd = Board.LeftEnd;
            int rightEnd = Board.RightEnd;
            return player.Hand.FirstOrDefault(t => t.Matches(leftEnd) || t.Matches(rightEnd));
        }

        #endregion

        #region Game Flow

        public void NextTurn() => currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;

        public bool HasPlayableTile(Player player)
        {
            if (!player.Hand.Any()) return false;
            if (!Board.Any()) return true;

            int leftEnd = Board.LeftEnd;
            int rightEnd = Board.RightEnd;
            return player.Hand.Any(t => t.Matches(leftEnd) || t.Matches(rightEnd));
        }

        public bool IsRoundOver()
        {
            if (Players.Any(p => !p.Hand.Any())) return true;

            int leftEnd = Board.LeftEnd;
            int rightEnd = Board.RightEnd;
            return Players.All(p => !p.HasPlayableTile(leftEnd, rightEnd)) && !boneyard.Any();
        }

        public Player GetRoundWinner() => Players.OrderBy(p => p.Hand.Sum(t => t.Left + t.Right)).First();

        public bool IsGameOver() => CurrentRound >= MaxRounds;

        public Player GetWinner() => Players.OrderByDescending(p => p.Score).First();

        /// <summary>
        /// Attempt to play or draw until player can play.
        /// Returns the first playable tile or null if cannot play.
        /// </summary>
        public DominoTile? TryPlayOrDraw(Player player)
        {
            var tile = GetPlayableTile(player);
            if (tile != null)
                return tile;

            while (boneyard.Any())
            {
                DrawTile(player);
                tile = GetPlayableTile(player);
                if (tile != null)
                    return tile;
            }

            return null;
        }

        #endregion
    }
}
