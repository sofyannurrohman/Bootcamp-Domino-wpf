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
        private const double TileSpacing = 5;
        private const double MaxRowWidth = 800;

        public DominoGameController Game { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

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
            // Ranking berdasarkan sisa point tangan
            var ranking = Game.Players
                .Select(p => new { p.Name, Points = p.Hand.Sum(t => t.TotalPip) })
                .OrderBy(p => p.Points)
                .ToList();

            string message = $"Game Over! Winner: {winner.Name}\n\nRanking:\n";
            int rank = 1;
            foreach (var p in ranking)
            {
                message += $"{rank}. {p.Name} - {p.Points} points\n";
                rank++;
            }

            MessageBox.Show(message, "Domino Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

            InitializeGame();
        }

        private void RefreshUI()
        {
            PlayerHand.ItemsSource = null;
            PlayerHand.ItemsSource = Game.CurrentPlayer.Hand;
            RenderBoard();
            StatusText.Text = $"{Game.CurrentPlayer.Name}'s turn";
        }

        #region Board Rendering

        private void RenderBoard()
        {
            BoardTiles.Children.Clear();
            if (!Game.Board.Any()) return;

            double x = 0, y = 0, rowHeight = 0;
            int? prevRight = null;

            foreach (var tile in Game.Board)
            {
                SetTileRotationAndFlip(tile, prevRight);
                prevRight = tile.Right;

                var (tileWidth, tileHeight) = GetTileDimensions(tile);

                if (x + tileWidth > MaxRowWidth)
                {
                    x = 0;
                    y += rowHeight + TileSpacing;
                    rowHeight = 0;
                }

                rowHeight = Math.Max(rowHeight, tileHeight);

                // Gunakan Image langsung untuk board agar tidak clickable
                var boardTile = CreateBoardTile(tile, tileWidth, tileHeight);
                Canvas.SetLeft(boardTile, x);
                Canvas.SetTop(boardTile, y);
                BoardTiles.Children.Add(boardTile);

                x += tileWidth + TileSpacing;
            }

            BoardTiles.Height = y + rowHeight + TileSpacing;
        }

        private (double width, double height) GetTileDimensions(DominoTile tile)
            => tile.RotationAngle == 90 ? (60, 120) : (120, 60);

        private void SetTileRotationAndFlip(DominoTile tile, int? prevRight)
        {
            tile.IsFlipped = false;

            if (tile.IsDouble)
                tile.RotationAngle = 90;
            else if (prevRight.HasValue)
            {
                if (tile.Left == prevRight.Value)
                    tile.RotationAngle = 0;
                else if (tile.Right == prevRight.Value)
                {
                    tile.RotationAngle = 0;
                    tile.Flip();
                }
                else
                    tile.RotationAngle = 180;
            }
            else
                tile.RotationAngle = 0;
        }

        private Image CreateBoardTile(DominoTile tile, double width, double height)
        {
            return new Image
            {
                Source = tile.DisplayImage,
                Width = width,
                Height = height,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        new RotateTransform(tile.RotationAngle),
                        new ScaleTransform { ScaleX = tile.IsFlipped ? -1 : 1, ScaleY = 1 }
                    }
                }
            };
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
            // Jika board kosong, game belum selesai
            if (!Game.Board.Any()) return false;

            // Jika semua player tidak bisa main, game over
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
