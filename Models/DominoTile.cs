using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DominoGame.Models
{
    public class DominoTile
    {
        // === PROPERTIES ===
        public int Left { get; private set; }
        public int Right { get; private set; }

        private readonly BitmapImage _originalImage;

        public bool IsDouble => Left == Right;
        public int TotalPip => Left + Right;

        // Rotasi default 0°; double tile default 90°
        public double RotationAngle { get; set; } = 0;

        // Flag untuk flip horizontal (visual)
        public bool IsFlipped { get; set; } = false;

        /// <summary>
        /// Gambar tile (tidak perlu load flipped lagi)
        /// </summary>
        public BitmapImage DisplayImage => _originalImage;

        // === CONSTRUCTORS ===
        public DominoTile(int left, int right)
        {
            Left = left;
            Right = right;
            _originalImage = LoadImage(left, right);

            if (IsDouble)
                RotationAngle = 90;
        }

        public DominoTile(int left, int right, BitmapImage image)
        {
            Left = left;
            Right = right;
            _originalImage = image;

            if (IsDouble)
                RotationAngle = 90;
        }

        // === METHODS ===

        /// <summary>
        /// Mengembalikan tile baru dengan sisi dibalik
        /// </summary>
        public DominoTile FlippedTile() => new DominoTile(Right, Left, _originalImage)
        {
            RotationAngle = this.RotationAngle,
            IsFlipped = !this.IsFlipped
        };

        /// <summary>
        /// Membalik sisi tile saat ini (in-place)
        /// </summary>
        public void Flip()
        {
            (Left, Right) = (Right, Left);
            IsFlipped = !IsFlipped;
        }

        /// <summary>
        /// Cek apakah tile cocok dengan angka tertentu
        /// </summary>
        public bool Matches(int number) => Left == number || Right == number;

        /// <summary>
        /// Rotasi dengan animasi pada Image UI
        /// </summary>
        public void RotateWithAnimation(Image image, double toAngle)
        {
            var rotateTransform = new System.Windows.Media.RotateTransform(RotationAngle);
            image.RenderTransform = rotateTransform;
            image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

            var animation = new DoubleAnimation
            {
                From = RotationAngle,
                To = toAngle,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new System.Windows.Media.Animation.QuadraticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseInOut }
            };

            rotateTransform.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, animation);
            RotationAngle = toAngle;
        }

        /// <summary>
        /// Load image dari folder Assets
        /// </summary>
        private BitmapImage LoadImage(int left, int right)
        {
            try
            {
                string fileName = $"{left}_{right}.png";
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);

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
