using DominoGame.Interfaces;
using DominoGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Models
{
    public class Deck : IDeck
    {
        public List<IDominoTile> DominoTiles { get; set; } = new();

        private readonly Random _random = new(Guid.NewGuid().GetHashCode());

        public Deck()
        {
            // generate 28 domino tiles
            for (byte i = 0; i <= 6; i++)
                for (byte j = i; j <= 6; j++)
                    DominoTiles.Add(new DominoTile(i, j));
        }

        public void Shuffle()
        {
            DominoTiles = DominoTiles.OrderBy(_ => _random.Next()).ToList();
        }

        public List<IDominoTile> DrawTiles(int count)
        {
            var tiles = DominoTiles.Take(count).ToList();
            DominoTiles.RemoveRange(0, tiles.Count);
            return tiles;
        }
    }
}
