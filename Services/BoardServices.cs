using DominoGame.Interfaces;
using DominoGame.Interfaces.Services;
using DominoGame.Models;

namespace DominoGame.Services
{
    public class BoardService : IBoardService
    {
        public int? LeftEnd(IBoard board) => board.Tiles.Count > 0 ? board.LeftEnd : null;
        public int? RightEnd(IBoard board) => board.Tiles.Count > 0 ? board.RightEnd : null;

        public void ClearBoard(IBoard board) => board.Clear();

        public bool PlaceTile(IBoard board, IDominoTile tile, bool placeLeft)
        {
            var concreteBoard = board as Board ?? throw new InvalidOperationException("Board must be Board type");

            if (concreteBoard.Tiles.Count == 0)
            {
                // First move only allow doubles
                if (tile.Left != tile.Right)
                    return false;

                concreteBoard.AddTileToBoard(tile, false);
                return true;
            }

            int left = concreteBoard.LeftEnd;
            int right = concreteBoard.RightEnd;
            int matchValue = placeLeft ? left : right;

            if (!tile.Matches(matchValue))
                return false;

            if ((placeLeft && tile.Right != matchValue) || (!placeLeft && tile.Left != matchValue))
                tile.Flip();

            concreteBoard.AddTileToBoard(tile, placeLeft);

            return true;
        }

        public (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player, IBoard board)
        {
            if (player.Hand.Count == 0) return null;

            // First move: pick first double
            if (board.Tiles.Count == 0)
            {
                var firstDouble = player.Hand.FirstOrDefault(t => t.Left == t.Right);
                if (firstDouble != null)
                    return (firstDouble, true); // placeLeft doesn't matter
                return null;
            }

            int? left = LeftEnd(board);
            int? right = RightEnd(board);

            // Normal move: try left end first
            foreach (var tile in player.Hand)
            {
                if (left.HasValue && tile.Matches(left.Value))
                    return (tile, true);
                if (right.HasValue && tile.Matches(right.Value))
                    return (tile, false);
            }

            return null;
        }

        public bool HasPlayableTile(IPlayer player, IBoard board) =>
            GetNextPlayableTile(player, board) != null;
    }
}
