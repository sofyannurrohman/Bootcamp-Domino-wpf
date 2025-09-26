using DominoGame.Interfaces;
using System.Collections;
using System.Collections.ObjectModel;

namespace DominoGame.Models
{
    public class Board : IBoard, IEnumerable<IDominoTile>
    {
        // ObservableCollection for UI binding
        public ObservableCollection<IDominoTile> Tiles { get; } = new();

        // Explicit IReadOnlyList implementation for IBoard
        IReadOnlyList<IDominoTile> IBoard.Tiles => Tiles;

        // Ends for gameplay logic (delegated to BoardService)
        public int LeftEnd => Tiles.Count > 0 ? Tiles[0].Left : 0;
        public int RightEnd => Tiles.Count > 0 ? Tiles[^1].Right : 0;

        public void Clear() => Tiles.Clear();

        // === IEnumerable Implementation ===
        public IEnumerator<IDominoTile> GetEnumerator() => Tiles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void AddTileToBoard(IDominoTile tile, bool atStart)
        {
            if (atStart)
                Tiles.Insert(0, tile);
            else
                Tiles.Add(tile);
        }
    }
}
