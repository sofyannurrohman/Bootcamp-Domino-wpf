using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IDeck
    {
        List<IDominoTile> DominoTiles { get; }
        void Shuffle();
        List<IDominoTile> DrawTiles(int count);
        void Reset();
    }
}