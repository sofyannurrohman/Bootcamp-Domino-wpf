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
        public bool PlaceTile(IBoard board, IDominoTile tile, bool placeLeft, IPlayer? player = null)
        {
            var concreteBoard = board as Board ?? throw new InvalidOperationException("Board must be Board type");

            if (concreteBoard.Tiles.Count == 0)
            {
                // First move: if player provided, enforce double rule
                if (player != null)
                {
                    var hasDouble = player.Hand.Any(t => t.PipLeft == t.PipRight);
                    if (hasDouble && tile.PipLeft != tile.PipRight)
                        return false; // Cannot play non-double if a double exists
                }

                // If no double exists or tile is double, allow placement
                concreteBoard.AddTileToBoard(tile, false);
                return true;
            }

            int left = concreteBoard.LeftEnd;
            int right = concreteBoard.RightEnd;
            int matchValue = placeLeft ? left : right;

            if (!tile.Matches(matchValue))
                return false;

            if ((placeLeft && tile.PipRight != matchValue) || (!placeLeft && tile.PipLeft != matchValue))
                tile.Flip();

            concreteBoard.AddTileToBoard(tile, placeLeft);
            return true;
        }



        public (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player, IBoard board)
        {
            if (player.Hand.Count == 0) return null;

            // First move: prioritize first double
            if (board.Tiles.Count == 0)
            {
                var firstDouble = player.Hand.FirstOrDefault(t => t.PipLeft == t.PipRight);
                if (firstDouble != null)
                    return (firstDouble, true); // placeLeft doesn't matter
                return (player.Hand.OrderByDescending(t => t.PipLeft + t.PipRight).First(), true);
            }

            int? left = LeftEnd(board);
            int? right = RightEnd(board);

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
