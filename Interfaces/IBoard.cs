using DominoGame.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DominoGame.Interfaces
{
    public interface IBoard : IEnumerable<DominoTile>
    {
        /// <summary>
        /// Tiles on the board in order. Use IReadOnlyList for interface compatibility.
        /// </summary>
        IReadOnlyList<DominoTile> Tiles { get; }

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
        bool PlaceTile(DominoTile tile, bool placeLeft);

        /// <summary>
        /// Clear all tiles from the board.
        /// </summary>
        void Clear();
    }
}
