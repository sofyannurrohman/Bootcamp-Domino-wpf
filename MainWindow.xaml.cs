using DominoGame.Controllers;
using DominoGame.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DominoGameWPF
{
    public partial class MainWindow : Window
    {
        private readonly DominoGameController _game;
        private TaskCompletionSource<bool>? _humanTurnTcs;

        // ✅ Controller is injected — no manual instantiation
        public MainWindow(DominoGameController gameController)
        {
            try
            {
                InitializeComponent();
                _game = gameController;
                SetupGame();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup error:\n{ex}", "Error");
                throw; // keep this so the debugger still stops where it happens
            }
        }

        private void SetupGame()
        {
            // Subscribe to events
            _game.OnTilePlayed += OnTilePlayed;
            _game.OnRoundOver += OnRoundOver;
            _game.OnGameOver += OnGameOver;
            _game.OnPlayerSkipped += OnPlayerSkipped;

            // ✅ Bind UI to controller
            DataContext = _game;

            // Start the game
            _game.StartGame(maxRounds: 5);

            // Kick off the loop
            _ = GameLoopAsync();
        }

        private void OnTilePlayed(IPlayer player, IDominoTile tile, bool placedLeft)
        {
            StatusText.Text = $"{player.Name} played [{tile.Left}|{tile.Right}] on {(placedLeft ? "left" : "right")}.";
            RefreshUI();
        }

        private void OnPlayerSkipped(IPlayer player)
        {
            StatusText.Text = $"{player.Name} cannot play, skipping turn...";
        }

        private void OnRoundOver(IPlayer? winner)
        {
            RefreshUI();
            string msg = winner != null ? $"Winner: {winner.Name}" : "Draw! No winner this round.";

            MessageBox.Show(
                $"Round {_game.CurrentRound} over!\n{msg}\nScores:\n{GetScoresString()}",
                "Round Over", MessageBoxButton.OK, MessageBoxImage.Information);

            if (!_game.IsGameOver())
            {
                _game.StartNextRound();
                RefreshUI();
            }
        }

        private void OnGameOver(IPlayer winner)
        {
            RefreshUI();
            MessageBox.Show(
                $"Game Over!\nWinner: {winner.Name}\nFinal Scores:\n{GetScoresString()}",
                "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

            _game.ResetScores();
            _game.StartGame(maxRounds: 5);

            DataContext = null;
            DataContext = _game;
            RefreshUI();
        }

        private void RefreshUI()
        {
            var player = _game.CurrentPlayer;
            if (player == null) return;

            PlayerHand.ItemsSource = null;
            PlayerHand.ItemsSource = player.Hand;

            foreach (var item in PlayerHand.Items)
            {
                if (PlayerHand.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi &&
                    lbi.Content is IDominoTile tile &&
                    FindVisualChild<Button>(lbi) is Button btn)
                {
                    btn.IsEnabled = IsHumanTurn() &&
                                    (!_game.Board.Any() ||
                                     tile.Matches(_game.Board.LeftEnd) ||
                                     tile.Matches(_game.Board.RightEnd));
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                if (FindVisualChild<T>(child) is T result) return result;
            }
            return null;
        }

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsHumanTurn()) return;
            if (sender is not Button { DataContext: IDominoTile tile }) return;

            bool played = _game.PlayTile(_game.CurrentPlayer, tile, true) ||
                          _game.PlayTile(_game.CurrentPlayer, tile, false);

            if (!played)
            {
                StatusText.Text = "Cannot play this tile!";
                return;
            }

            _humanTurnTcs?.TrySetResult(true);
        }

        private async Task GameLoopAsync()
        {
            while (!_game.IsGameOver())
            {
                RefreshUI();

                var player = _game.CurrentPlayer;
                if (player == null)
                {
                    await Task.Delay(100);
                    continue;
                }

                if (!_game.HasPlayableTile(player))
                {
                    OnPlayerSkipped(player);
                    _game.NextTurn();
                    await Task.Delay(800);
                    continue;
                }

                if (IsComputerTurn())
                    await ComputerTurnAsync();
                else
                    await HumanTurnAsync();

                if (_game.IsRoundOver())
                {
                    _game.EndRound();
                    RefreshUI();
                    await Task.Delay(400);
                }

                await Task.Delay(50);
            }

            _humanTurnTcs?.TrySetResult(true);
            RefreshUI();
        }

        private async Task ComputerTurnAsync()
        {
            var player = _game.CurrentPlayer;
            var nextTileInfo = _game.GetNextPlayableTile(player);

            if (nextTileInfo is (IDominoTile tile, bool placeLeft))
            {
                _game.PlayTile(player, tile, placeLeft);
                await Task.Delay(500);
            }
            else
            {
                OnPlayerSkipped(player);
                _game.NextTurn();
                await Task.Delay(800);
            }
        }

        private async Task HumanTurnAsync()
        {
            if (_game.IsGameOver() || _game.IsRoundOver())
            {
                _humanTurnTcs?.TrySetResult(true);
                return;
            }

            _humanTurnTcs = new TaskCompletionSource<bool>();
            StatusText.Text = $"{_game.CurrentPlayer.Name}'s turn";
            RefreshUI();
            await _humanTurnTcs.Task;
        }

        private string GetScoresString() =>
            string.Join("\n", _game.Players.Select(p => $"{p.Name}: {p.Score} points"));

        private bool IsComputerTurn() => _game.CurrentPlayer?.Name == "Computer";
        private bool IsHumanTurn() => !IsComputerTurn();
    }
}
