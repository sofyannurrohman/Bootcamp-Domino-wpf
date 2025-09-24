using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DominoGame.Models
{
    public class DominoTile
    {
        // === PROPERTIES ===
        public int Left { get; }
        public int Right { get; }

        public BitmapImage Image { get; }

        // Tile double = vertikal, total pip = jumlah angka
        public bool IsDouble => Left == Right;
        public int TotalPip => Left + Right;

        // Rotasi default 0°; bisa diubah (board = 0, player hand = 0, double tile = 90)
        public double RotationAngle { get; set; } = 0;

        // === CONSTRUCTORS ===
        public DominoTile(int left, int right)
        {
            Left = left;
            Right = right;

            Image = LoadImage(left, right);

            // Jika double, default rotation di board 90°
            if (IsDouble)
                RotationAngle = 90;
        }

        public DominoTile(int left, int right, BitmapImage image)
        {
            Left = left;
            Right = right;
            Image = image;

            if (IsDouble)
                RotationAngle = 90;
        }

        // === METHODS ===

        // Flip tile (swap sisi) tanpa kehilangan image
        public DominoTile FlippedTile()
        {
            return new DominoTile(Right, Left, this.Image)
            {
                RotationAngle = this.RotationAngle
            };
        }

        // Cek apakah tile cocok dengan angka tertentu
        public bool Matches(int number) => Left == number || Right == number;

        // Animasi rotasi Image
        public void RotateWithAnimation(Image image, double toAngle)
        {
            var rotateTransform = new RotateTransform(RotationAngle);
            image.RenderTransform = rotateTransform;
            image.RenderTransformOrigin = new Point(0.5, 0.5);

            var animation = new DoubleAnimation
            {
                From = RotationAngle,
                To = toAngle,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);

            RotationAngle = toAngle;
        }

        // Load image dari folder Assets
        private BitmapImage LoadImage(int left, int right)
        {
            try
            {
                var fileName = $"{left}_{right}.png";
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);

                if (!File.Exists(path))
                    throw new FileNotFoundException($"⚠️ Asset not found: {path}");

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                return bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public override string ToString() => $"[{Left}|{Right}]";
    }
}
