using DominoGame.Interfaces;
using System.Collections;
using System.Collections.ObjectModel;

namespace DominoGame.Models
{
    public class Board : IBoard, IEnumerable<IDominoTile>
    {
        public ObservableCollection<IDominoTile> Tiles { get; } = new();
        IReadOnlyList<IDominoTile> IBoard.Tiles => Tiles;
        public int LeftEnd => Tiles.Count > 0 ? Tiles[0].PipLeft : 0;
        public int RightEnd => Tiles.Count > 0 ? Tiles[^1].PipRight : 0;
        public void Clear() => Tiles.Clear();
        public void AddTileToBoard(IDominoTile tile, bool atStart)
        {
            if (atStart)
                Tiles.Insert(0, tile);
            else
                Tiles.Add(tile);
        }
        public IEnumerator<IDominoTile> GetEnumerator() => Tiles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
