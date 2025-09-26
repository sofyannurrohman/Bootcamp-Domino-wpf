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
        public IDeck Deck { get; private set; } = new Deck();
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

        // Events
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
            // Jangan otomatis stop, bahkan jika CurrentRound = MaxRounds
            CurrentRound++;
            Board.Clear();
            currentPlayerIndex = _random.Next(Players.Count);

            foreach (var player in Players)
                player.Hand.Clear();

            Deck = new Deck();
            Deck.Shuffle();

            const int handSize = 7;
            foreach (var player in Players)
                player.Hand.AddRange(Deck.DrawTiles(handSize));

            // pastikan CurrentPlayer bisa main
            if (!HasPlayableTile(CurrentPlayer))
            {
                var playableIndex = Players.FindIndex(p => HasPlayableTile(p));
                if (playableIndex >= 0)
                    currentPlayerIndex = playableIndex;
            }
        }

        public void StartNextRound()
        {
            if (!IsGameOver())
                StartNewRound();
        }
        #endregion

        #region Tile Management
        public bool PlayTile(IPlayer player, IDominoTile tile, bool placeLeft)
        {
            if (player != CurrentPlayer) return false;
            if (!Board.PlaceTile(tile, placeLeft)) return false;

            player.Hand.Remove(tile);
            OnTilePlayed?.Invoke(player, tile, placeLeft);

            NextTurn();
            return true;
        }
        #endregion

        #region Turn Handling
        public void NextTurn()
        {
            if (Players.Count == 0) return;

            int startIndex = currentPlayerIndex;
            bool looped = false;

            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;

                if (HasPlayableTile(CurrentPlayer))
                    break;

                OnPlayerSkipped?.Invoke(CurrentPlayer);

                if (currentPlayerIndex == startIndex)
                {
                    looped = true;
                    break; // semua player sudah dicoba
                }

            } while (true);

            // Jika semua player tidak bisa main, biarkan CurrentPlayer tetap
        }

        public void SkipCurrentPlayer() => NextTurn();
        #endregion

        #region Helpers (Playable Tile)
        public (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player)
        {
            if (player == null || !player.Hand.Any()) return null;
            if (!Board.Any())
                return (player.Hand.First(), true);

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

        public bool HasPlayableTile(IPlayer player) => GetNextPlayableTile(player) != null;
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

            // Game over dicek hanya setelah ronde dimainkan
            if (Players.Any(p => p.Score >= 30) || CurrentRound > MaxRounds)
            {
                var gameWinner = GetWinner();
                if (gameWinner != null)
                    OnGameOver?.Invoke(gameWinner);
            }
        }

        public bool IsGameOver() => Players.Any(p => p.Score >= 30) || CurrentRound > MaxRounds;

        public IPlayer? GetWinner() => Players.OrderByDescending(p => p.Score).FirstOrDefault();

        public void ResetScores()
        {
            foreach (var player in Players)
                player.Score = 0;
            OnPropertyChanged(nameof(Players));
        }
        #endregion
    }
}
