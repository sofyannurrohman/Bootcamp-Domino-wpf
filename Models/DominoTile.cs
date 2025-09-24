using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace DominoGame.Models
{
    public class DominoTile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int left;
        private int right;
        private double rotationAngle;
        private bool isFlipped;

        public int Left
        {
            get => left;
            set { left = value; OnPropertyChanged(nameof(Left)); }
        }

        public int Right
        {
            get => right;
            set { right = value; OnPropertyChanged(nameof(Right)); }
        }
        public bool Matches(int number) => Left == number || Right == number;

        public int OriginalLeft { get; private set; }
        public int OriginalRight { get; private set; }

        private BitmapImage _originalImage;
        public BitmapImage DisplayImage
        {
            get => _originalImage;
            private set { _originalImage = value; OnPropertyChanged(nameof(DisplayImage)); }
        }

        public bool IsDouble => OriginalLeft == OriginalRight;
        public int TotalPip => Left + Right;

        public double RotationAngle
        {
            get => rotationAngle;
            set { rotationAngle = value; OnPropertyChanged(nameof(RotationAngle)); }
        }

        public bool IsFlipped
        {
            get => isFlipped;
            set { isFlipped = value; OnPropertyChanged(nameof(IsFlipped)); }
        }

        public DominoTile(int left, int right)
        {
            Left = OriginalLeft = left;
            Right = OriginalRight = right;
            DisplayImage = LoadImage(left, right);
            if (IsDouble) RotationAngle = 90;
        }

        public void Flip()
        {
            (Left, Right) = (Right, Left);
            IsFlipped = !IsFlipped;
        }

        public void ResetOrientation()
        {
            Left = OriginalLeft;
            Right = OriginalRight;
            IsFlipped = false;
            RotationAngle = IsDouble ? 90 : 0;
        }

        private BitmapImage LoadImage(int left, int right)
        {
            string fileName = $"{left}_{right}.png";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
            if (!File.Exists(path)) return null;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze(); // ✅ Freeze supaya aman di UI thread
            return bmp;
        }

        private void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
