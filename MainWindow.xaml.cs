using DominoGame.Controllers;
using DominoGame.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            Game.OnGameOver += Game_OnGameOver;

            DataContext = Game; // ✅ Bind once here

            Game.StartGame();
            RefreshUI();

            _ = GameLoopAsync(); // start game loop
        }

        private void Game_OnTilePlayed(Player player, DominoTile tile, bool placedLeft)
        {
            StatusText.Text = $"{player.Name} played [{tile.Left}|{tile.Right}] on {(placedLeft ? "left" : "right")}.";
            RefreshUI();
        }

        private void Game_OnGameOver(Player winner)
        {
            ShowGameOverMessage(winner);
            InitializeGame();
        }

        private void ShowGameOverMessage(Player winner)
        {
            var ranking = Game.Players
                .Select(p => new { p.Name, Points = p.Hand.Sum(t => t.TotalPip) })
                .OrderBy(p => p.Points)
                .ToList();

            string message = $"Game Over! Winner: {winner.Name}\n\nRanking:\n";
            for (int i = 0; i < ranking.Count; i++)
                message += $"{i + 1}. {ranking[i].Name} - {ranking[i].Points} points\n";

            MessageBox.Show(message, "Domino Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region UI Refresh

        private void RefreshUI()
        {
            // Only update dynamic parts (like player's hand).
            PlayerHand.ItemsSource = null;
            PlayerHand.ItemsSource = Game.CurrentPlayer.Hand;

            // ❌ No need to reset BoardItemsControl anymore,
            // since Board.Tiles is ObservableCollection and bound in XAML
        }

        #endregion

        #region Player Actions

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsHumanTurn()) return;
            if (sender is not Button { DataContext: DominoTile tile }) return;

            bool played = Game.PlayTile(tile, true) || Game.PlayTile(tile, false);

            if (!played)
            {
                StatusText.Text = "Cannot play this tile!";
                return;
            }

            _humanTurnTcs?.TrySetResult(true); // signal human turn done
        }

        #endregion

        #region Game Loop

        private async Task GameLoopAsync()
        {
            while (!CheckGameOver())
            {
                RefreshUI();

                if (!HasPlayableTile(Game.CurrentPlayer))
                {
                    StatusText.Text = IsComputerTurn()
                        ? "Computer cannot play, skipping turn..."
                        : $"{Game.CurrentPlayer.Name} cannot play, skipping turn...";

                    await Task.Delay(800);
                    Game.NextTurn();
                    continue;
                }

                if (IsComputerTurn())
                {
                    await ComputerTurnAsync();
                    Game.NextTurn();
                }
                else
                {
                    _humanTurnTcs = new TaskCompletionSource<bool>();
                    StatusText.Text = $"{Game.CurrentPlayer.Name}'s turn";

                    await _humanTurnTcs.Task; // wait for player
                    Game.NextTurn();
                }
            }
        }

        private async Task ComputerTurnAsync()
        {
            await Task.Delay(500);
            var tile = GetPlayableTile(Game.CurrentPlayer);

            if (tile != null)
            {
                bool played = Game.PlayTile(tile, true) || Game.PlayTile(tile, false);
                if (played)
                {
                    StatusText.Text = $"Computer played [{tile.Left}|{tile.Right}]";
                    await Task.Delay(500);
                }
                else
                {
                    Game.CurrentPlayer.Hand.Remove(tile); // prevent stuck loop
                    StatusText.Text = $"Computer cannot play [{tile.Left}|{tile.Right}], skipping...";
                    await Task.Delay(500);
                }
            }
        }

        #endregion

        #region Helpers

        private DominoTile? GetPlayableTile(Player player)
        {
            if (!Game.Board.Any()) return player.Hand.FirstOrDefault();
            int leftEnd = Game.Board.LeftEnd;
            int rightEnd = Game.Board.RightEnd;
            return player.Hand.FirstOrDefault(t => t.Matches(leftEnd) || t.Matches(rightEnd));
        }

        private bool HasPlayableTile(Player player)
        {
            if (!player.Hand.Any()) return false;
            if (!Game.Board.Any()) return true;
            int leftEnd = Game.Board.LeftEnd;
            int rightEnd = Game.Board.RightEnd;
            return player.Hand.Any(t => t.Matches(leftEnd) || t.Matches(rightEnd));
        }

        private bool CheckGameOver()
        {
            if (!Game.Board.Any()) return false;

            if (Game.Players.All(p => !HasPlayableTile(p)))
            {
                var winner = Game.Players.OrderBy(p => p.Hand.Sum(t => t.TotalPip)).First();
                Game_OnGameOver(winner);
                return true;
            }

            return false;
        }

        private bool IsComputerTurn() => Game.CurrentPlayer.Name == "Computer";
        private bool IsHumanTurn() => !IsComputerTurn();

        #endregion
    }
}
