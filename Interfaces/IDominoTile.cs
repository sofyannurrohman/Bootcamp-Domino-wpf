using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DominoGame.Models.Enums;
namespace DominoGame.Interfaces
{
    public interface IDominoTile
    {
        int Left { get; set; }
        int Right { get; set; }
        int TotalPip { get; }
        Orientation Orientation { get; set; }
        double RotationAngle { get; }
        bool Matches(int value);
        void Flip();
        bool IsDouble { get; }
    }
}
