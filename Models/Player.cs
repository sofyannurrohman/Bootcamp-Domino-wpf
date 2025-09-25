using DominoGame.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DominoGame.Models
{
    public class Player : IPlayer, INotifyPropertyChanged
    {
        private int _score;

        public string Name { get; private set; }
        public List<DominoTile> Hand { get; } = new();
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

        public bool HasPlayableTile(int leftEnd, int rightEnd)
        {
            return Hand.Any(tile => tile.Matches(leftEnd) || tile.Matches(rightEnd));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
