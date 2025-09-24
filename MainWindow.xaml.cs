using DominoGame.Controllers;
using DominoGame.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DominoGameWPF
{
    public partial class MainWindow : Window
    {
        public DominoGameController game { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            game = new DominoGameController();

            // Subscribe to game events
            game.OnTilePlayed += Game_OnTilePlayed;
            game.OnGameOver += Game_OnGameOver;

            game.StartGame();
            RefreshUI();

            // Jika giliran pertama AI → langsung jalan
            if (game.CurrentPlayer.Name == "Computer")
                _ = ComputerMoveAsync();
        }

        private void Game_OnTilePlayed(Player player, DominoTile tile, bool placedLeft)
        {
            StatusText.Text = $"{player.Name} played [{tile.Left}|{tile.Right}] on {(placedLeft ? "left" : "right")}.";
            RefreshUI();
        }

        private void Game_OnGameOver(Player winner)
        {
            MessageBox.Show($"{winner.Name} wins!", "Domino Game", MessageBoxButton.OK, MessageBoxImage.Information);
            InitializeGame();
        }

        private void RefreshUI()
        {
            // Update board
            BoardTiles.ItemsSource = null;
            BoardTiles.ItemsSource = game.Board;

            // Update player hand
            PlayerHand.ItemsSource = null;
            PlayerHand.ItemsSource = game.CurrentPlayer.Hand;

            StatusText.Text = $"{game.CurrentPlayer.Name}'s turn";
        }

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            if (game.CurrentPlayer.Name == "Computer") return;

            if (sender is System.Windows.Controls.Button btn && btn.DataContext is DominoTile tile)
                PlayerMove(tile);
        }

        private void PlayerMove(DominoTile tile)
        {
            bool played = game.PlayTile(tile, true) || game.PlayTile(tile, false);

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

            game.NextTurn();
            RefreshUI();

            if (game.CurrentPlayer.Name == "Computer")
                _ = ComputerMoveAsync();
        }

        private async Task ComputerMoveAsync()
        {
            await Task.Delay(500);

            while (game.CurrentPlayer.Name == "Computer")
            {
                var tile = GetPlayableTile(game.CurrentPlayer);

                if (tile != null)
                {
                    game.PlayTile(tile, true);
                    RefreshUI();

                    if (CheckGameOver()) return;

                    game.NextTurn();
                    RefreshUI();
                    await Task.Delay(500);
                }
                else
                {
                    StatusText.Text = "Computer cannot play, skipping turn...";
                    game.NextTurn();
                    RefreshUI();

                    if (CheckGameOver()) return;
                    await Task.Delay(500);
                }
            }
        }

        private DominoTile GetPlayableTile(Player player)
        {
            if (game.Board.Count == 0)
                return player.Hand.FirstOrDefault();

            int leftEnd = game.Board.First().Left;
            int rightEnd = game.Board.Last().Right;

            return player.Hand.FirstOrDefault(t => t.Matches(leftEnd) || t.Matches(rightEnd));
        }

        private bool CheckGameOver()
        {
            bool anyPlayable = game.CurrentPlayer.Hand.Any(t =>
                game.Board.Count == 0 || t.Matches(game.Board.First().Left) || t.Matches(game.Board.Last().Right));

            if (!anyPlayable && game.Board.Count > 0)
            {
                Game_OnGameOver(game.GetWinner());
                return true;
            }

            return false;
        }

        private void SkipTurnIfNoPlayableTile()
        {
            bool cannotPlay = game.CurrentPlayer.Hand.All(t =>
                game.Board.Count > 0 && !t.Matches(game.Board.First().Left) && !t.Matches(game.Board.Last().Right));

            if (cannotPlay)
            {
                StatusText.Text = $"{game.CurrentPlayer.Name} cannot play, skipping turn...";
                game.NextTurn();
                RefreshUI();

                if (CheckGameOver()) return;

                if (game.CurrentPlayer.Name == "Computer")
                    _ = ComputerMoveAsync();
            }
        }
    }
}
