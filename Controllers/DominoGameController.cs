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
        public List<IPlayer> Players { get; } = new();

        private int currentPlayerIndex;
        private readonly Random _random = new(Guid.NewGuid().GetHashCode());

        public IPlayer CurrentPlayer => Players.Count > 0 ? Players[currentPlayerIndex] : null!;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int _currentRound;
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

        // events
        public event Action<IPlayer, IDominoTile, bool>? OnTilePlayed;
        public event Action<IPlayer?>? OnRoundOver;
        public event Action<IPlayer>? OnGameOver;
        public event Action<IPlayer>? OnPlayerSkipped;

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
            if (e.PropertyName == nameof(IPlayer.Score))
                OnPropertyChanged(nameof(Players));
        }

        private void StartNewRound()
        {
            CurrentRound++;
            Board.Clear();

            currentPlayerIndex = _random.Next(Players.Count);

            foreach (var player in Players)
                player.Hand.Clear();

            var tiles = ShuffleTiles(GenerateTiles());
            DealTiles(tiles, Players);
        }

        public void StartNextRound() => StartNewRound();
        #endregion

        #region Tile Management
        private static List<IDominoTile> GenerateTiles()
        {
            var tiles = new List<IDominoTile>();
            for (int i = 0; i <= 6; i++)
                for (int j = i; j <= 6; j++)
                    tiles.Add(new DominoTile(i, j));
            return tiles;
        }

        private List<IDominoTile> ShuffleTiles(IEnumerable<IDominoTile> tiles) =>
            tiles.OrderBy(_ => _random.Next()).ToList();

        private static void DealTiles(List<IDominoTile> tiles, List<IPlayer> players)
        {
            const int handSize = 7;
            for (int i = 0; i < players.Count; i++)
            {
                var handTiles = tiles.Skip(i * handSize).Take(handSize);
                players[i].Hand.AddRange(handTiles);
            }
        }

        public bool PlayTile(IPlayer player, IDominoTile tile, bool placeLeft)
        {
            if (player != CurrentPlayer) return false;
            if (!Board.PlaceTile(tile, placeLeft)) return false;

            player.Hand.Remove(tile);
            OnTilePlayed?.Invoke(player, tile, placeLeft);

            // tidak lagi memanggil EndRound di sini
            NextTurn();
            return true;
        }
        #endregion

        #region Turn Handling
        public void NextTurn()
        {
            if (Players.Count == 0) return;

            int startIndex = currentPlayerIndex;
            bool skipped = false;

            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;

                if (!HasPlayableTile(CurrentPlayer) && !skipped)
                {
                    skipped = true;
                    OnPlayerSkipped?.Invoke(CurrentPlayer);
                    // break supaya UI bisa baca event sebelum lanjut
                    return;
                }

                if (HasPlayableTile(CurrentPlayer))
                {
                    skipped = false;
                    break; // ada playable tile, turn valid
                }

                // jika kembali ke startIndex, berarti semua player buntu
                if (currentPlayerIndex == startIndex)
                {
                    skipped = false;
                    break;
                }

            } while (true);
        }


        // wrapper supaya kompatibel dengan MainWindow lama
        public void SkipCurrentPlayer() => NextTurn();
        #endregion

        #region Helpers (Playable Tile)
        public (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player)
        {
            if (player == null || !player.Hand.Any()) return null;

            if (!Board.Any())
            {
                var firstTile = player.Hand.FirstOrDefault();
                return firstTile != null ? (firstTile, true) : null;
            }

            int? left = Board.LeftEnd;
            int? right = Board.RightEnd;

            foreach (var tile in player.Hand)
            {
                if (left != null && (tile.Left == left || tile.Right == left))
                    return (tile, true);
                if (right != null && (tile.Left == right || tile.Right == right))
                    return (tile, false);
            }
            return null;
        }

        public bool HasPlayableTile(IPlayer player) =>
            GetNextPlayableTile(player) != null;
        #endregion

        #region Game Flow
        public bool IsRoundOver() =>
            Players.Any(p => !p.Hand.Any()) || Players.All(p => !HasPlayableTile(p));

        public IPlayer? GetRoundWinner()
        {
            var playerEmpty = Players.FirstOrDefault(p => !p.Hand.Any());
            if (playerEmpty != null) return playerEmpty;

            var minSum = Players.Min(p => p.Hand.Sum(t => t.Left + t.Right));
            var winners = Players.Where(p => p.Hand.Sum(t => t.Left + t.Right) == minSum).ToList();
            return winners.Count == 1 ? winners[0] : null;
        }

        public void EndRound()
        {
            var winner = GetRoundWinner();
            if (winner != null)
            {
                int score = Players
                    .Where(p => p != winner)
                    .Sum(p => p.Hand.Sum(t => t.Left + t.Right));

                score -= winner.Hand.Sum(t => t.Left + t.Right);
                winner.Score += Math.Max(score, 0);
            }

            OnRoundOver?.Invoke(winner);

            var gameWinner = Players.FirstOrDefault(p => p.Score >= 100);
            if (gameWinner != null)
                OnGameOver?.Invoke(gameWinner);
        }

        public bool IsGameOver() => Players.Any(p => p.Score >= 30);
        public IPlayer? GetWinner() => Players.OrderByDescending(p => p.Score).FirstOrDefault();
        #endregion
    }
}
