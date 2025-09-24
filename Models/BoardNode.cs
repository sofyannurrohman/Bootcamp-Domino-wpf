using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoGame.Models
{
    public class BoardNode
    {
        public DominoTile Tile { get; set; }
        public BoardNode? Left { get; set; }
        public BoardNode? Right { get; set; }

        public BoardNode(DominoTile tile)
        {
            Tile = tile;
        }
    }

}
