using DominoGame.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace DominoGame.Models
{
    public class Player : IPlayer, INotifyPropertyChanged
    {
        private int _score;

        public string Name { get; private set; }
        public List<IDominoTile> Hand { get; } = new();

        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged(nameof(Score));
                }
            }
        }

        public Player(string name)
        {
            Name = name;
            Score = 0;
        }
        public bool PlayDominoTile(IDominoTile dominoTile)
        {
            return Hand.Contains(dominoTile);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
