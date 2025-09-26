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
            if (board.Tiles.Count == 0)
                return (player.Hand[0], true);

            int? left = LeftEnd(board);
            int? right = RightEnd(board);

            foreach (var tile in player.Hand)
            {
                if (left.HasValue && (tile.Left == left || tile.Right == left)) return (tile, true);
                if (right.HasValue && (tile.Left == right || tile.Right == right)) return (tile, false);
            }
            return null;
        }
        public bool HasPlayableTile(IPlayer player, IBoard board) =>
            GetNextPlayableTile(player, board) != null;
    }
}
