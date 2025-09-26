using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IBoard : IEnumerable<IDominoTile>
    {
        /// <summary>
        /// Tiles on the board in order.
        /// </summary>
        IReadOnlyList<IDominoTile> Tiles { get; }

        /// <summary>
        /// Value at the left end of the board.
        /// </summary>
        int LeftEnd { get; }

        /// <summary>
        /// Value at the right end of the board.
        /// </summary>
        int RightEnd { get; }
        void Clear();
    }
}
