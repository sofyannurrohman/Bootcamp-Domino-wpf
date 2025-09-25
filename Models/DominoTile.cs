using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using DominoGame.Interfaces;
using DominoGame.Models.Enums;

namespace DominoGame.Models
{
    public class DominoTile : INotifyPropertyChanged,IDominoTile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int left;
        private int right;
        private bool isFlipped;
        private Orientation orientation;
        private double rotationAngle;
        private BitmapImage _originalImage;

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

        public int OriginalLeft { get; private set; }
        public int OriginalRight { get; private set; }

        public bool IsDouble => OriginalLeft == OriginalRight;
        public int TotalPip => Left + Right;

        public bool IsFlipped
        {
            get => isFlipped;
            set { isFlipped = value; OnPropertyChanged(nameof(IsFlipped)); }
        }

        public Orientation Orientation
        {
            get => orientation;
            set
            {
                orientation = value;
                OnPropertyChanged(nameof(Orientation));

                // Keep RotationAngle in sync with Orientation
                RotationAngle = orientation == Orientation.VERTICAL ? 90 : 0;
            }
        }

        public double RotationAngle
        {
            get => rotationAngle;
            set { rotationAngle = value; OnPropertyChanged(nameof(RotationAngle)); }
        }

        public BitmapImage DisplayImage
        {
            get => _originalImage;
            private set { _originalImage = value; OnPropertyChanged(nameof(DisplayImage)); }
        }

        public DominoTile(int left, int right)
        {
            Left = OriginalLeft = left;
            Right = OriginalRight = right;
            DisplayImage = LoadImage(left, right);

            // Default orientation
            Orientation = IsDouble ? Orientation.VERTICAL : Orientation.HORIZONTAL;
        }

        /// <summary>
        /// Checks if this tile matches a given pip value.
        /// </summary>
        public bool Matches(int number) => Left == number || Right == number;

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
            Orientation = IsDouble ? Orientation.VERTICAL : Orientation.HORIZONTAL;
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
            bmp.Freeze(); // Safe for cross-thread UI
            return bmp;
        }

        private void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
