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
            Game.OnTilePlayed += Game_OnTilePlayed;
            Game.OnRoundOver += Game_OnRoundOver;
            Game.OnGameOver += Game_OnGameOver;

            DataContext = Game;
            Game.StartGame(maxRounds: 5);

            _ = GameLoopAsync();
        }

        private void Game_OnTilePlayed(IPlayer player, DominoTile tile, bool placedLeft)
        {
            StatusText.Text = $"{player.Name} played [{tile.Left}|{tile.Right}] on {(placedLeft ? "left" : "right")}.";
            RefreshUI();
        }

        private void Game_OnRoundOver(IPlayer winner)
        {
            RefreshUI();
            MessageBox.Show($"Round {Game.CurrentRound} over!\nWinner: {winner.Name}\nScores:\n{GetScoresString()}",
                            "Round Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Game_OnGameOver(IPlayer winner)
        {
            RefreshUI();
            MessageBox.Show($"Game Over!\nWinner: {winner.Name}\nFinal Scores:\n{GetScoresString()}",
                            "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

            InitializeGame(); // Restart automatically
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

            // Dynamically enable only playable tiles for human
            foreach (var item in PlayerHand.Items)
            {
                if (PlayerHand.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi)
                {
                    if (lbi.Content is DominoTile tile)
                    {
                        var btn = FindVisualChild<Button>(lbi);
                        if (btn != null)
                            btn.IsEnabled = IsHumanTurn() && (BoardIsEmpty() || tile.Matches(Game.Board.LeftEnd) || tile.Matches(Game.Board.RightEnd));
                    }
                }
            }
        }

        private bool BoardIsEmpty() => !Game.Board.Any();

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
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

            bool played = Game.PlayTile(Game.CurrentPlayer, tile, true) || Game.PlayTile(Game.CurrentPlayer, tile, false);

            if (!played)
            {
                StatusText.Text = "Cannot play this tile!";
                return;
            }

            _humanTurnTcs?.TrySetResult(true); // Signal human turn done
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

                // Auto-draw until player can play or boneyard empty
                while (!HasPlayableTile(player) && Game.DrawTile(player) != null)
                {
                    StatusText.Text = $"{player.Name} draws a tile...";
                    RefreshUI();
                    await Task.Delay(400);
                }

                // Skip if still cannot play
                if (!HasPlayableTile(player))
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
            var tile = Game.GetPlayableTile(Game.CurrentPlayer);
            if (tile != null)
            {
                bool played = Game.PlayTile(Game.CurrentPlayer, tile, true) || Game.PlayTile(Game.CurrentPlayer, tile, false);
                StatusText.Text = played
                    ? $"Computer played [{tile.Left}|{tile.Right}]"
                    : $"Computer cannot play [{tile.Left}|{tile.Right}], skipping...";
            }
            await Task.Delay(500);
        }

        private async Task HandleRoundEndIfNeeded()
        {
            if (!Game.IsRoundOver()) return;

            var roundWinner = Game.GetRoundWinner();
            roundWinner.Score++;
            Game_OnRoundOver(roundWinner);

            if (Game.CurrentRound >= Game.MaxRounds)
            {
                var gameWinner = Game.GetWinner();
                Game_OnGameOver(gameWinner);
            }
            else
            {
                Game.StartNextRound();
                await Task.Delay(300);
            }
        }

        #endregion

        #region Helpers

        private bool HasPlayableTile(IPlayer player)
        {
            if (!player.Hand.Any()) return false;
            if (BoardIsEmpty()) return true;

            int leftEnd = Game.Board.LeftEnd;
            int rightEnd = Game.Board.RightEnd;
            return player.Hand.Any(t => t.Matches(leftEnd) || t.Matches(rightEnd));
        }

        private bool IsComputerTurn() => Game.CurrentPlayer?.Name == "Computer";
        private bool IsHumanTurn() => !IsComputerTurn();

        #endregion
    }
}
