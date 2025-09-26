using DominoGame.Interfaces;

namespace DominoGame.Models
{
    public class BoardNode
    {
        public IDominoTile Tile { get; set; }
        public BoardNode? Left { get; set; }
        public BoardNode? Right { get; set; }
        public BoardNode(IDominoTile tile)
        {
            Tile = tile;
        }
    }
}
