using DominoGame.Controllers;
using DominoGame.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DominoGame.Helpers;

namespace DominoGameWPF.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly DominoGameController _game;
        private TaskCompletionSource<bool>? _humanTurnTcs;

        public ObservableCollection<IDominoTile> PlayerHand { get; } = new();
        public ObservableCollection<IDominoTile> BoardTiles { get; } = new();
        public ObservableCollection<IPlayer> Players { get; } = new();

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }

        public int PlayerScore => Players.Count > 0 ? Players[0].Score : 0;
        public int ComputerScore => Players.Count > 1 ? Players[1].Score : 0;
        public int CurrentRound => _game.CurrentRound;
        public int MaxRounds => _game.MaxRounds;
        public IPlayer? CurrentPlayer => _game.CurrentPlayer;
        public IBoard Board => _game.Board;

        public ICommand PlayTileCommand { get; }

        public GameViewModel(DominoGameController gameController)
        {
            _game = gameController;

            _game.OnTilePlayed += OnTilePlayed;
            _game.OnPlayerSkipped += OnPlayerSkipped;
            _game.OnRoundOver += OnRoundOver;
            _game.OnGameOver += OnGameOver;

            PlayTileCommand = new RelayCommand<IDominoTile>(PlayTile);

            StartGame();
        }

        private void StartGame()
        {
            _game.StartGame(maxRounds: 5);
            BindPlayers();
            _ = GameLoopAsync();
            RefreshAll();
        }

        #region Event Handlers
        private void OnTilePlayed(IPlayer player, IDominoTile tile, bool placedLeft)
        {
            StatusText = $"{player.Name} played [{tile.Left}|{tile.Right}] on {(placedLeft ? "left" : "right")}.";
            RefreshAll();
        }

        private void OnPlayerSkipped(IPlayer player)
        {
            StatusText = $"{player.Name} cannot play, skipping turn...";
            RefreshAll();
        }

        private void OnRoundOver(IPlayer? winner)
        {
            var msg = winner != null
                ? $"Winner of this round: {winner.Name}\n"
                : "Draw! No winner this round.\n";

            msg += string.Join("\n", _game.Players.Select(p => $"{p.Name}: {p.Score} points"));
            StatusText = $"Round {CurrentRound} over!";
            RefreshAll();

            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(msg, $"Round {CurrentRound} Over", MessageBoxButton.OK, MessageBoxImage.Information));
        }

        private void OnGameOver(IPlayer winner)
        {
            var msg = $"Game Over! Winner: {winner.Name}\n" +
                      string.Join("\n", _game.Players.Select(p => $"{p.Name}: {p.Score} points"));

            StatusText = msg;
            RefreshAll();

            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(msg, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information));

            ResetUI();
            StartGame();
        }
        #endregion

        #region Refresh Methods
        private void RefreshHand()
        {
            PlayerHand.Clear();
            var player = CurrentPlayer;
            if (player == null) return;

            foreach (var tile in player.Hand)
                PlayerHand.Add(tile);

            OnPropertyChanged(nameof(CurrentPlayer));
        }

        private void RefreshBoard()
        {
            BoardTiles.Clear();
            foreach (var tile in Board.Tiles)
                BoardTiles.Add(tile);

            OnPropertyChanged(nameof(Board));
        }

        private void RefreshScores()
        {
            OnPropertyChanged(nameof(PlayerScore));
            OnPropertyChanged(nameof(ComputerScore));
        }

        private void RefreshAll()
        {
            RefreshHand();
            RefreshBoard();
            RefreshScores();
            OnPropertyChanged(nameof(CurrentRound));
            OnPropertyChanged(nameof(MaxRounds));
        }
        #endregion

        #region Commands
        private void PlayTile(IDominoTile tile)
        {
            if (!IsHumanTurn() || _humanTurnTcs == null) return;

            // Enforce first-move double rule for human
            if (_game.Board.Tiles.Count == 0)
            {
                var hasDouble = CurrentPlayer!.Hand.Any(t => t.Left == t.Right);
                if (hasDouble && tile.Left != tile.Right)
                {
                    StatusText = "You must play a double tile on the first move!";
                    return;
                }
            }

            bool played = _game.PlayTile(CurrentPlayer, tile, true) || _game.PlayTile(CurrentPlayer, tile, false);

            if (!played)
            {
                StatusText = "Cannot play this tile!";
                return;
            }

            _humanTurnTcs.TrySetResult(true);
            RefreshAll();
        }
        #endregion

        #region Game Loop
        private async Task GameLoopAsync()
        {
            while (!_game.IsGameOver())
            {
                var player = CurrentPlayer;
                if (player == null)
                {
                    await Task.Delay(50);
                    continue;
                }

                if (!_game.HasPlayableTile(player))
                {
                    _game.TriggerSkip(player);
                    RefreshAll();
                    await Task.Delay(1000);
                    _game.NextTurn();
                    continue;
                }

                if (IsComputerTurn())
                    await ComputerTurnAsync();
                else
                    await HumanTurnAsync();

                _game.NextTurn();

                if (_game.IsRoundOver())
                {
                    _game.EndRound();
                    RefreshAll();
                    await Task.Delay(1000);

                    if (!_game.IsGameOver())
                    {
                        _game.StartNextRound();
                        RefreshAll();
                    }
                }

                await Task.Delay(50);
            }

            OnGameOver(_game.GetWinner());
        }

        private async Task ComputerTurnAsync()
        {
            var player = CurrentPlayer;
            if (player == null) return;

            var next = _game.GetNextPlayableTile(player);

            if (next is (IDominoTile tileToPlay, bool placeLeft))
            {
                // Enforce first-move double rule for computer
                if (_game.Board.Tiles.Count == 0)
                {
                    var hasDouble = player.Hand.Any(t => t.Left == t.Right);
                    if (hasDouble && tileToPlay.Left != tileToPlay.Right)
                    {
                        // Skip non-double if double exists
                        _game.TriggerSkip(player);
                        RefreshAll();
                        await Task.Delay(800);
                        return;
                    }
                }

                _game.PlayTile(player, tileToPlay, placeLeft);
                StatusText = $"{player.Name} played [{tileToPlay.Left}|{tileToPlay.Right}]";
            }
            else
            {
                _game.TriggerSkip(player);
            }

            RefreshAll();
            await Task.Delay(800);
        }


        private async Task HumanTurnAsync()
        {
            if (_game.IsGameOver() || _game.IsRoundOver()) return;

            _humanTurnTcs = new TaskCompletionSource<bool>();
            StatusText = $"{CurrentPlayer?.Name}'s turn";
            RefreshHand();

            await _humanTurnTcs.Task;
        }
        #endregion

        #region Helpers
        private bool IsComputerTurn() => CurrentPlayer?.Name == "Computer";
        private bool IsHumanTurn() => !IsComputerTurn();

        private void ResetUI()
        {
            PlayerHand.Clear();
            BoardTiles.Clear();
            StatusText = "";
            OnPropertyChanged(nameof(CurrentRound));
            OnPropertyChanged(nameof(MaxRounds));
            OnPropertyChanged(nameof(CurrentPlayer));
            RefreshScores();
        }

        private void BindPlayers()
        {
            Players.Clear();
            foreach (var player in _game.Players)
            {
                Players.Add(player);
                player.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(IPlayer.Score))
                        RefreshScores();
                };
            }

            RefreshScores();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
