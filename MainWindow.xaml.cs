using DominoGame.Controllers;
using DominoGame.Interfaces;
using DominoGame.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DominoGameWPF
{
    public partial class MainWindow : Window
    {
        public DominoGameController Game { get; private set; }
        private TaskCompletionSource<bool>? _humanTurnTcs;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        #region Initialization

        private void InitializeGame()
        {
            Game = new DominoGameController();
            Game.OnTilePlayed += OnTilePlayed;
            Game.OnRoundOver += OnRoundOver;
            Game.OnGameOver += OnGameOver;

            DataContext = Game;
            Game.StartGame(maxRounds: 5);

            _ = GameLoopAsync();
        }

        private void OnTilePlayed(Player player, DominoTile tile, bool placedLeft)
        {
            StatusText.Text = $"{player.Name} played [{tile.Left}|{tile.Right}] on {(placedLeft ? "left" : "right")}.";
            RefreshUI();
        }

        private void OnRoundOver(Player? winner)
        {
            RefreshUI();
            string msg = winner != null
                ? $"Winner: {winner.Name}"
                : "Draw! No winner this round.";

            MessageBox.Show(
                $"Round {Game.CurrentRound} over!\n{msg}\nScores:\n{GetScoresString()}",
                "Round Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnGameOver(Player winner)
        {
            RefreshUI();
            MessageBox.Show(
                $"Game Over!\nWinner: {winner.Name}\nFinal Scores:\n{GetScoresString()}",
                "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

            InitializeGame(); // Automatically restart
        }

        private string GetScoresString() =>
            string.Join("\n", Game.Players.Select(p => $"{p.Name}: {p.Score} points"));

        #endregion

        #region UI Refresh

        private void RefreshUI()
        {
            if (Game.CurrentPlayer == null) return;

            PlayerHand.ItemsSource = null;
            PlayerHand.ItemsSource = Game.CurrentPlayer.Hand;

            foreach (var item in PlayerHand.Items)
            {
                if (PlayerHand.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi &&
                    lbi.Content is DominoTile tile)
                {
                    var btn = FindVisualChild<Button>(lbi);
                    if (btn != null)
                    {
                        btn.IsEnabled = IsHumanTurn() &&
                                        (Game.Board.Any() ? tile.Matches(Game.Board.LeftEnd) || tile.Matches(Game.Board.RightEnd) : true);
                    }
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        #endregion

        #region Player Actions

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsHumanTurn()) return;
            if (sender is not Button { DataContext: DominoTile tile }) return;

            // Try placing left first, then right
            bool played = Game.PlayTile(Game.CurrentPlayer, tile, true) ||
                          Game.PlayTile(Game.CurrentPlayer, tile, false);

            if (!played)
            {
                StatusText.Text = "Cannot play this tile!";
                return;
            }

            _humanTurnTcs?.TrySetResult(true);
        }

        #endregion

        #region Game Loop

        private async Task GameLoopAsync()
        {
            while (!Game.IsGameOver())
            {
                RefreshUI();
                var player = Game.CurrentPlayer;
                if (player == null) { await Task.Delay(300); continue; }

                if (!Game.HasPlayableTile(player))
                {
                    StatusText.Text = $"{player.Name} cannot play, skipping turn...";
                    await Task.Delay(800);
                    Game.NextTurn();
                    await HandleRoundEndIfNeeded();
                    continue;
                }

                if (IsComputerTurn())
                {
                    await ComputerTurnAsync();
                    Game.NextTurn();
                    await HandleRoundEndIfNeeded();
                    continue;
                }

                // Human turn
                _humanTurnTcs = new TaskCompletionSource<bool>();
                StatusText.Text = $"{player.Name}'s turn";
                RefreshUI();
                await _humanTurnTcs.Task;
                Game.NextTurn();
                await HandleRoundEndIfNeeded();
            }
        }

        private async Task ComputerTurnAsync()
        {
            await Task.Delay(500);
            var player = Game.CurrentPlayer;

            // Ambil next playable tile (nullable tuple)
            var nextTileInfo = Game.GetNextPlayableTile(player);

            if (nextTileInfo is (DominoTile tile, bool placeLeft))
            {
                bool played = Game.PlayTile(player, tile, placeLeft);
                StatusText.Text = played
                    ? $"Computer played [{tile.Left}|{tile.Right}]"
                    : $"{player.Name} cannot play, skipping turn...";
            }
            else
            {
                StatusText.Text = $"{player.Name} cannot play, skipping turn...";
            }

            await Task.Delay(500);
        }

        private async Task HandleRoundEndIfNeeded()
        {
            if (!Game.IsRoundOver()) return;

            Game.EndRound(); // controller akan update skor dan panggil OnRoundOver

            if (!Game.IsGameOver())
            {
                Game.StartNextRound();
                await Task.Delay(300);
            }
        }

        #endregion

        #region Helpers

        private bool IsComputerTurn() => Game.CurrentPlayer?.Name == "Computer";
        private bool IsHumanTurn() => !IsComputerTurn();

        #endregion
    }
}
