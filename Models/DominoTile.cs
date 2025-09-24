using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace DominoGame.Models
{
    public class DominoTile
    {
        public int Left { get; }
        public int Right { get; }
        public BitmapImage Image { get; }
        public bool IsDouble => Left == Right;

        public double RotationAngle { get; set; } = 0; // default 0° untuk Board, 90° untuk PlayerHand

        // Constructor utama (load image berdasarkan nilai Left & Right)
        public DominoTile(int left, int right)
        {
            Left = left;
            Right = right;

            var fileName = $"{left}_{right}.png";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);

            if (File.Exists(path))
            {
                Image = new BitmapImage();
                Image.BeginInit();
                Image.UriSource = new Uri(path, UriKind.Absolute);
                Image.CacheOption = BitmapCacheOption.OnLoad;
                Image.EndInit();
            }
            else
            {
                Console.WriteLine($"⚠️ Asset not found: {path}");
            }
        }

        // Constructor dari image yang sudah ada
        public DominoTile(int left, int right, BitmapImage image)
        {
            Left = left;
            Right = right;
            Image = image;
        }

        // Flip tile tanpa kehilangan image
        public DominoTile FlippedTile()
        {
            return new DominoTile(Right, Left, this.Image)
            {
                RotationAngle = this.RotationAngle
            };
        }

        // Cek apakah tile cocok dengan number
        public bool Matches(int number) =>
            Left == number || Right == number;

        public override string ToString() => $"[{Left}|{Right}]";
    }
}
