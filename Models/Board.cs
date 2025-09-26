using DominoGame.Interfaces;
using System.Collections;
using System.Collections.ObjectModel;

namespace DominoGame.Models
{
    public class Board : IBoard, IEnumerable<IDominoTile>
    {
        private BoardNode? _root;

        // ObservableCollection for UI binding
        public ObservableCollection<IDominoTile> Tiles { get; } = new();

        // Explicit IReadOnlyList implementation for IBoard
        IReadOnlyList<IDominoTile> IBoard.Tiles => Tiles;

        // Ends for gameplay logic
        public int LeftEnd => _root != null ? GetLeftMost(_root).Tile.Left : 0;
        public int RightEnd => _root != null ? GetRightMost(_root).Tile.Right : 0;

        /// <summary>
        /// Place a tile on the board (left or right)
        /// </summary>
        public bool PlaceTile(IDominoTile tile, bool placeLeft)
        {
            if (_root == null)
            {
                _root = new BoardNode(tile);
                Tiles.Add(tile);
                return true;
            }

            var targetNode = placeLeft ? GetLeftMost(_root) : GetRightMost(_root);
            int matchValue = placeLeft ? targetNode.Tile.Left : targetNode.Tile.Right;

            if (!tile.Matches(matchValue))
                return false;

            // Flip tile if orientation does not match
            if ((placeLeft && tile.Right != matchValue) ||
                (!placeLeft && tile.Left != matchValue))
            {
                tile.Flip();
            }

            var newNode = new BoardNode(tile);
            if (placeLeft) targetNode.Left = newNode;
            else targetNode.Right = newNode;

            // Add to ObservableCollection in correct order
            if (placeLeft)
                Tiles.Insert(0, tile);
            else
                Tiles.Add(tile);

            return true;
        }

        /// <summary>
        /// Clears the board
        /// </summary>
        public void Clear()
        {
            _root = null;
            Tiles.Clear();
        }

        // === Helpers ===
        private static BoardNode GetLeftMost(BoardNode node)
        {
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        private static BoardNode GetRightMost(BoardNode node)
        {
            while (node.Right != null)
                node = node.Right;
            return node;
        }

        // === IEnumerable Implementation ===
        public IEnumerator<IDominoTile> GetEnumerator() => Tiles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Inner node now works with IDominoTile
        private class BoardNode
        {
            public IDominoTile Tile { get; }
            public BoardNode? Left { get; set; }
            public BoardNode? Right { get; set; }

            public BoardNode(IDominoTile tile)
            {
                Tile = tile;
            }
        }
    }
}
