using DominoGame.Models;
using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IPlayer
    {
        string Name { get; }
        List<DominoTile> Hand { get; }
        int Score { get; set; }
        bool HasPlayableTile(int leftEnd, int rightEnd);
    }
}
