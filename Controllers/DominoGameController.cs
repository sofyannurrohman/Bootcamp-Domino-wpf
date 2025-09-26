using DominoGame.Interfaces;
using DominoGame.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DominoGame.Controllers
{
    public class DominoGameController : IGameController, INotifyPropertyChanged
    {
        public IBoard Board { get; private set; } = new Board();
        public List<Player> Players { get; } = new();

        private int currentPlayerIndex = 0;
        public Player CurrentPlayer => Players.Count > 0 ? Players[currentPlayerIndex] : null!;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _currentRound = 0;
        public int CurrentRound
        {
            get => _currentRound;
            private set
            {
                if (_currentRound != value)
                {
                    _currentRound = value;
                    OnPropertyChanged(nameof(CurrentRound));
                }
            }
        }

        private int _maxRounds = 5;
        public int MaxRounds
        {
            get => _maxRounds;
            private set
            {
                if (_maxRounds != value)
                {
                    _maxRounds = value;
                    OnPropertyChanged(nameof(MaxRounds));
                }
            }
        }

        public event Action<Player, DominoTile, bool>? OnTilePlayed;
        public event Action<Player>? OnRoundOver;
        public event Action<Player>? OnGameOver;

        #region Game Setup

        public void StartGame(int maxRounds = 5)
        {
            Players.Clear();
            Players.Add(new Player("You"));
            Players.Add(new Player("Computer"));

            MaxRounds = maxRounds;
            CurrentRound = 0;

            foreach (var player in Players)
            {
                player.Score = 0;
                player.PropertyChanged += Player_PropertyChanged;
            }

            StartNewRound();
        }

        private void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.Score))
                OnPropertyChanged(nameof(Players));
        }

        private void StartNewRound()
        {
            CurrentRound++;
            Board.Clear();

            // Tentukan giliran pertama secara acak
            currentPlayerIndex = new Random().Next(Players.Count);

            foreach (var player in Players)
                player.Hand.Clear();

            var tiles = ShuffleTiles(GenerateTiles());
            DealTiles(tiles, Players);
        }

        public void StartNextRound() => StartNewRound();

        #endregion

        #region Tile Management

        private static List<DominoTile> GenerateTiles()
        {
            var tiles = new List<DominoTile>();
            for (int i = 0; i <= 6; i++)
                for (int j = i; j <= 6; j++)
                    tiles.Add(new DominoTile(i, j));
            return tiles;
        }

        private static List<DominoTile> ShuffleTiles(List<DominoTile> tiles)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            return tiles.OrderBy(_ => rnd.Next()).ToList();
        }

        private static void DealTiles(List<DominoTile> tiles, List<Player> players)
        {
            const int handSize = 7;
            for (int i = 0; i < players.Count; i++)
                players[i].Hand.AddRange(tiles.Skip(i * handSize).Take(handSize));
        }

        public bool PlayTile(Player player, DominoTile tile, bool placeLeft)
        {
            if (player != CurrentPlayer) return false;
            if (!Board.PlaceTile(tile, placeLeft)) return false;

            player.Hand.Remove(tile);
            OnTilePlayed?.Invoke(player, tile, placeLeft);
            return true;
        }

        public (DominoTile tile, bool placeLeft)? GetNextPlayableTile(Player player)
        {
            if (!Board.Any())
            {
                var firstTile = player.Hand.FirstOrDefault();
                return firstTile != null ? (firstTile, true) : null;
            }

            int leftEnd = Board.LeftEnd;
            int rightEnd = Board.RightEnd;

            foreach (var tile in player.Hand)
            {
                if (tile.Matches(leftEnd)) return (tile, true);
                if (tile.Matches(rightEnd)) return (tile, false);
            }

            return null;
        }

        public bool HasPlayableTile(Player player) => GetNextPlayableTile(player) != null;

        #endregion

        #region Game Flow

        public void NextTurn()
        {
            int attempts = 0;
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
                attempts++;
                if (attempts > Players.Count) break; // semua player di-blok
            } while (!HasPlayableTile(CurrentPlayer));
        }

        public bool IsRoundOver()
        {
            // Ronde berakhir jika ada player habis tile atau semua player di-blok
            if (Players.Any(p => !p.Hand.Any())) return true;
            return Players.All(p => !HasPlayableTile(p));
        }

        public Player? GetRoundWinner()
        {
            // Cek siapa habis tile → otomatis menang
            var playerEmpty = Players.FirstOrDefault(p => !p.Hand.Any());
            if (playerEmpty != null) return playerEmpty;

            // Semua di-blok → cari jumlah pips terkecil
            var minSum = Players.Min(p => p.Hand.Sum(t => t.Left + t.Right));
            var winners = Players.Where(p => p.Hand.Sum(t => t.Left + t.Right) == minSum).ToList();

            if (winners.Count == 1) return winners[0]; // pemenang tunggal
            return null; // draw
        }

        public void EndRound()
        {
            var winner = GetRoundWinner();
            if (winner != null)
            {
                int score = 0;
                foreach (var player in Players)
                {
                    if (player != winner)
                        score += player.Hand.Sum(t => t.Left + t.Right);
                }

                score -= winner.Hand.Sum(t => t.Left + t.Right); // sesuai aturan
                winner.Score += Math.Max(score, 0); // skor tidak boleh negatif
            }

            OnRoundOver?.Invoke(winner!);

            // Cek game over
            var gameWinner = Players.FirstOrDefault(p => p.Score >= 100);
            if (gameWinner != null)
                OnGameOver?.Invoke(gameWinner);
        }

        public bool IsGameOver() => Players.Any(p => p.Score >= 100);

        public Player? GetWinner() => Players.OrderByDescending(p => p.Score).FirstOrDefault();

        #endregion
    }
}
