using DominoGame.Interfaces;
using DominoGame.Interfaces.Services;
using DominoGame.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DominoGame.Controllers
{
    public class DominoGameController : IGameController, INotifyPropertyChanged
    {
        // Models
        public IBoard Board { get; private set; }
        public IDeck Deck { get; }
        public List<IPlayer> Players { get; } = new();

        // Services
        private readonly IBoardService _boardService;
        private readonly IPlayerService _playerService;
        private readonly ITurnService _turnService;

        private int currentPlayerIndex;
        private readonly Random _random = new(Guid.NewGuid().GetHashCode());
        public IPlayer CurrentPlayer => Players.Count > 0 ? Players[currentPlayerIndex] : null!;

        // PropertyChanged for WPF
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Game Events
        public event Action<IPlayer, IDominoTile, bool>? OnTilePlayed;
        public event Action<IPlayer?>? OnRoundOver;
        public event Action<IPlayer>? OnGameOver;
        public event Action<IPlayer>? OnPlayerSkipped;

        // Round tracking
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

        #region Constructor
        public DominoGameController(
            IBoardService boardService,
            IPlayerService playerService,
            ITurnService turnService,
            IBoard board,
            IDeck deck)
        {
            _boardService = boardService;
            _playerService = playerService;
            _turnService = turnService;
            Board = board;
            Deck = deck;
        }
        #endregion

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
                player.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(IPlayer.Score))
                        OnPropertyChanged(nameof(Players));
                };
            }

            StartNewRound();
        }

        private void StartNewRound()
        {
            CurrentRound++;
            _boardService.ClearBoard(Board);
            currentPlayerIndex = _random.Next(Players.Count);

            foreach (var player in Players)
                player.Hand.Clear();

            Deck.Reset();
            Deck.Shuffle();

            const int handSize = 7;
            foreach (var player in Players)
                player.Hand.AddRange(Deck.DrawTiles(handSize));

            // Make sure starting player can play
            if (!_boardService.HasPlayableTile(CurrentPlayer, Board))
            {
                var playableIndex = Players.FindIndex(p => _boardService.HasPlayableTile(p, Board));
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
            if (!_boardService.PlaceTile(Board, tile, placeLeft)) return false;

            _playerService.RemoveTileFromHand(player, tile);
            OnTilePlayed?.Invoke(player, tile, placeLeft);
            return true;
        }
        #endregion

        #region Turn Handling
        public void NextTurn()
        {
            if (Players.Count == 0) return;

            currentPlayerIndex = _turnService.NextTurn(
                Players, currentPlayerIndex, _boardService, Board, out var nextPlayer);
        }
        public void TriggerSkip(IPlayer player)
        {
            OnPlayerSkipped?.Invoke(player);
        }
        public void SkipCurrentPlayer() => NextTurn();
        #endregion

        #region Helpers
        public bool HasPlayableTile(IPlayer player) =>
            _boardService.HasPlayableTile(player, Board);

        public (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player) =>
            _boardService.GetNextPlayableTile(player, Board);
        #endregion

        #region Game Flow
        public bool IsRoundOver() =>
            Players.Any(p => !p.Hand.Any()) || Players.All(p => !_boardService.HasPlayableTile(p, Board));

        public IPlayer? GetRoundWinner() =>
            _playerService.GetRoundWinner(Players);

        public void EndRound()
        {
            var winner = GetRoundWinner();
            if (winner != null)
            {
                int score = Players.Where(p => p != winner)
                                   .Sum(p => p.Hand.Sum(t => t.Left + t.Right));
                score -= winner.Hand.Sum(t => t.Left + t.Right);
                winner.Score += Math.Max(score, 0);
                // ✅ Explicitly tell UI winner's score changed
                OnPropertyChanged(nameof(Players));
            }

            OnRoundOver?.Invoke(winner);

            if (Players.Any(p => p.Score >= 30) || CurrentRound > MaxRounds)
            {
                var gameWinner = _playerService.GetGameWinner(Players);
                if (gameWinner != null)
                    OnGameOver?.Invoke(gameWinner);
            }
        }

        public bool IsGameOver() => Players.Any(p => p.Score >= 30) || CurrentRound > MaxRounds;
        public IPlayer? GetWinner() => _playerService.GetGameWinner(Players);
        public void ResetScores() => _playerService.ResetScores(Players);
        #endregion
    }
}
