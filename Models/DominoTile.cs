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
        public int PipLeft
        {
            get => left;
            set { left = value; OnPropertyChanged(nameof(PipLeft)); }
        }

        public int PipRight
        {
            get => right;
            set { right = value; OnPropertyChanged(nameof(PipRight)); }
        }

        public int OriginalLeft { get; private set; }
        public int OriginalRight { get; private set; }

        public bool IsDouble => OriginalLeft == OriginalRight;
        public int TotalPip => PipLeft + PipRight;

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
            PipLeft = OriginalLeft = left;
            PipRight = OriginalRight = right;
            DisplayImage = LoadImage(left, right);
            Orientation = IsDouble ? Orientation.VERTICAL : Orientation.HORIZONTAL;
        }

        public bool Matches(int number) => PipLeft == number || PipRight == number;

        public void Flip()
        {
            (PipLeft, PipRight) = (PipRight, PipLeft);
            IsFlipped = !IsFlipped;
        }

        public void ResetOrientation()
        {
            PipLeft = OriginalLeft;
            PipRight = OriginalRight;
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
            bmp.Freeze();
            return bmp;
        }

        private void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
