using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IBoard : IEnumerable<IDominoTile>
    {
        IReadOnlyList<IDominoTile> Tiles { get; }
        int LeftEnd { get; }
        int RightEnd { get; }
        void Clear();
    }
}
