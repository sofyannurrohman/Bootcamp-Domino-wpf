using DominoGame.Controllers;
using DominoGame.Interfaces;
using DominoGame.Models;
using DominoGame.Services;
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
            var boardService = new BoardService();
            var playerService = new PlayerService();
            var turnService = new TurnService();

            var board = new Board();

            // DI Services
            Game = new DominoGameController(boardService, playerService, turnService, board);

            // Subscribe to events
            Game.OnTilePlayed += OnTilePlayed;
            Game.OnRoundOver += OnRoundOver;
            Game.OnGameOver += OnGameOver;
            Game.OnPlayerSkipped += OnPlayerSkipped;

            // DataContext for WPF binding
            DataContext = Game;

            // Start the game
            Game.StartGame(maxRounds: 5);

            // Start the game loop
            _ = GameLoopAsync();
        }
        #endregion

        #region Event Handlers

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
                $"Round {Game.CurrentRound} over!\n{msg}\nScores:\n{GetScoresString()}",
                "Round Over", MessageBoxButton.OK, MessageBoxImage.Information);

            // Start next round if game not over
            if (!Game.IsGameOver())
            {
                Game.StartNextRound();
                RefreshUI();
            }
        }

        private void OnGameOver(IPlayer winner)
        {
            RefreshUI();
            MessageBox.Show(
                $"Game Over!\nWinner: {winner.Name}\nFinal Scores:\n{GetScoresString()}",
                "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reset skor
            Game.ResetScores();

            // Restart game otomatis
            Game.StartGame(maxRounds: 5);

            // Pastikan binding UI di-refresh
            DataContext = null;
            DataContext = Game;
            RefreshUI();
        }


        #endregion

        #region UI Refresh

        private void RefreshUI()
        {
            var player = Game.CurrentPlayer;
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
                                    (!Game.Board.Any() ||
                                     tile.Matches(Game.Board.LeftEnd) ||
                                     tile.Matches(Game.Board.RightEnd));
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

        #endregion

        #region Player Actions

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsHumanTurn()) return;
            if (sender is not Button { DataContext: IDominoTile tile }) return;

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
                if (player == null)
                {
                    await Task.Delay(100);
                    continue;
                }

                if (!Game.HasPlayableTile(player))
                {
                    OnPlayerSkipped(player);
                    Game.NextTurn();
                    await Task.Delay(800);
                    continue;
                }

                if (IsComputerTurn())
                    await ComputerTurnAsync();
                else
                    await HumanTurnAsync();

                // cek ronde selesai
                if (Game.IsRoundOver())
                {
                    Game.EndRound();
                    RefreshUI();
                    await Task.Delay(400);
                }

                await Task.Delay(50);
            }

            // pastikan human turn tidak stuck
            _humanTurnTcs?.TrySetResult(true);
            RefreshUI();
        }

        private async Task ComputerTurnAsync()
        {
            var player = Game.CurrentPlayer;
            var nextTileInfo = Game.GetNextPlayableTile(player);

            if (nextTileInfo is (IDominoTile tile, bool placeLeft))
            {
                Game.PlayTile(player, tile, placeLeft);
                await Task.Delay(500);
            }
            else
            {
                OnPlayerSkipped(player);
                Game.NextTurn();
                await Task.Delay(800);
            }
        }

        private async Task HumanTurnAsync()
        {
            if (Game.IsGameOver() || Game.IsRoundOver())
            {
                _humanTurnTcs?.TrySetResult(true);
                return;
            }

            _humanTurnTcs = new TaskCompletionSource<bool>();
            StatusText.Text = $"{Game.CurrentPlayer.Name}'s turn";
            RefreshUI();
            await _humanTurnTcs.Task;
        }

        #endregion

        #region Helpers

        private string GetScoresString() =>
            string.Join("\n", Game.Players.Select(p => $"{p.Name}: {p.Score} points"));

        private bool IsComputerTurn() => Game.CurrentPlayer?.Name == "Computer";
        private bool IsHumanTurn() => !IsComputerTurn();
        
        #endregion
    }
}
