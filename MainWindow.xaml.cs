using DominoGame.Controllers;
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

            Game.StartGame();
            RefreshUI();

            if (IsComputerTurn())
                _ = ComputerMoveAsync();
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
            // Bind player hand
            PlayerHand.ItemsSource = null;
            PlayerHand.ItemsSource = Game.CurrentPlayer.Hand;

            // Bind board items
            BoardItemsControl.ItemsSource = null;
            BoardItemsControl.ItemsSource = Game.Board;

            StatusText.Text = $"{Game.CurrentPlayer.Name}'s turn";
        }

        #endregion

        #region Player Actions

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsComputerTurn()) return;

            if (sender is Button btn && btn.DataContext is DominoTile tile)
                PlayTileForCurrentPlayer(tile);
        }

        private void PlayTileForCurrentPlayer(DominoTile tile)
        {
            // Coba mainkan di kiri atau kanan
            bool played = Game.PlayTile(tile, true) || Game.PlayTile(tile, false);
            if (!played)
            {
                StatusText.Text = "Cannot play this tile!";
                SkipTurnIfNoPlayableTile();
                return;
            }

            ProcessTurn();
        }

        private void ProcessTurn()
        {
            if (CheckGameOver()) return;

            Game.NextTurn();
            RefreshUI();

            if (IsComputerTurn())
                _ = ComputerMoveAsync();
        }

        private async Task ComputerMoveAsync()
        {
            await Task.Delay(1000);

            while (IsComputerTurn())
            {
                var tile = GetPlayableTile(Game.CurrentPlayer);

                if (tile != null)
                {
                    PlayTileForCurrentPlayer(tile);
                    await Task.Delay(500);
                }
                else
                {
                    StatusText.Text = "Computer cannot play, skipping turn...";
                    Game.NextTurn();
                    RefreshUI();
                    if (CheckGameOver()) return;
                    await Task.Delay(500);
                }
            }
        }

        private DominoTile GetPlayableTile(Player player)
        {
            if (!Game.Board.Any()) return player.Hand.FirstOrDefault();

            int leftEnd = Game.Board.First().Left;
            int rightEnd = Game.Board.Last().Right;

            return player.Hand.FirstOrDefault(t => t.Matches(leftEnd) || t.Matches(rightEnd));
        }

        private bool HasPlayableTile(Player player)
        {
            if (!Game.Board.Any()) return true;

            int leftEnd = Game.Board.First().Left;
            int rightEnd = Game.Board.Last().Right;

            return player.Hand.Any(t => t.Matches(leftEnd) || t.Matches(rightEnd));
        }

        private bool CheckGameOver()
        {
            if (!Game.Board.Any()) return false;

            if (Game.Players.All(p => !HasPlayableTile(p)))
            {
                var winner = GetWinnerByPoints();
                Game_OnGameOver(winner);
                return true;
            }

            return false;
        }

        private Player GetWinnerByPoints()
            => Game.Players.OrderBy(p => p.Hand.Sum(t => t.TotalPip)).First();

        private void SkipTurnIfNoPlayableTile()
        {
            if (HasPlayableTile(Game.CurrentPlayer)) return;

            StatusText.Text = $"{Game.CurrentPlayer.Name} cannot play, skipping turn...";
            Game.NextTurn();
            RefreshUI();

            if (CheckGameOver()) return;

            if (IsComputerTurn())
                _ = ComputerMoveAsync();
        }

        private bool IsComputerTurn() => Game.CurrentPlayer.Name == "Computer";

        #endregion
    }
}
