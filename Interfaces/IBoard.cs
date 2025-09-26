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

        /// <summary>
        /// Place a tile on the board, on left or right.
        /// </summary>
        bool PlaceTile(IDominoTile tile, bool placeLeft);

        /// <summary>
        /// Clear all tiles from the board.
        /// </summary>
        void Clear();
    }
}
