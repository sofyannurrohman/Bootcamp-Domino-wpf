using DominoGame.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace DominoGame.Interfaces
{
    public interface IPlayer : INotifyPropertyChanged
    {
        string Name { get; }
        List<IDominoTile> Hand { get; }
        int Score { get; set; }
    }
}
