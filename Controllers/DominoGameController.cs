using DominoGame.Interfaces;
using DominoGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Controllers
{
    public class DominoGameController : IGameController
    {
        public BoardNode? Root { get; private set; }
        public List<Player> Players { get; } = new();
        private int currentPlayerIndex = 0;

        public Player CurrentPlayer => Players[currentPlayerIndex];

        // ✅ Event pakai versi lama supaya kompatibel
        public event Action<Player, DominoTile, bool>? OnTilePlayed;
        public event Action<Player>? OnGameOver;

        // ✅ Tambahin wrapper Board (list semua tile) supaya UI lama tetap jalan
        public List<DominoTile> Board
        {
            get
            {
                var result = new List<DominoTile>();
                if (Root != null) CollectTiles(Root, result);
                return result;
            }
        }

        private void CollectTiles(BoardNode node, List<DominoTile> list)
        {
            if (node.Left != null) CollectTiles(node.Left, list);
            list.Add(node.Tile);
            if (node.Right != null) CollectTiles(node.Right, list);
        }

        public void StartGame()
        {
            var tiles = ShuffleTiles(GenerateTiles());

            var player1 = new Player("You");
            var player2 = new Player("Computer");

            DealTiles(tiles, player1, player2);

            Players.Clear();
            Players.Add(player1);
            Players.Add(player2);

            Root = null;
            currentPlayerIndex = 0;
        }

        private List<DominoTile> GenerateTiles()
        {
            var tiles = new List<DominoTile>();
            for (int i = 0; i <= 6; i++)
            {
                for (int j = i; j <= 6; j++)
                {
                    tiles.Add(new DominoTile(i, j));
                }
            }
            return tiles;
        }

        private List<DominoTile> ShuffleTiles(List<DominoTile> tiles)
        {
            var rnd = new Random();
            return tiles.OrderBy(_ => rnd.Next()).ToList();
        }

        private void DealTiles(List<DominoTile> tiles, Player player1, Player player2)
        {
            player1.Hand.AddRange(tiles.Take(7));
            player2.Hand.AddRange(tiles.Skip(7).Take(7));
        }

        public bool PlayTile(DominoTile tile, bool placeLeft)
        {
            if (Root == null)
            {
                Root = new BoardNode(tile)
                {
                    Tile = { RotationAngle = tile.IsDouble ? 90 : 0 }
                };
                CurrentPlayer.Hand.Remove(tile);
                OnTilePlayed?.Invoke(CurrentPlayer, tile, placeLeft);
                return true;
            }

            // cari ujung board
            var targetNode = placeLeft ? GetLeftMost(Root) : GetRightMost(Root);
            int numberToMatch = placeLeft ? targetNode.Tile.Left : targetNode.Tile.Right;

            if (!tile.Matches(numberToMatch))
                return false;

            var playTile = (placeLeft && tile.Right == numberToMatch) ||
                           (!placeLeft && tile.Left == numberToMatch)
                ? tile
                : tile.FlippedTile();

            playTile.RotationAngle = playTile.IsDouble ? 90 : 0;

            var newNode = new BoardNode(playTile);
            if (placeLeft) targetNode.Left = newNode;
            else targetNode.Right = newNode;

            CurrentPlayer.Hand.Remove(tile);
            OnTilePlayed?.Invoke(CurrentPlayer, playTile, placeLeft);

            if (IsGameOver())
                OnGameOver?.Invoke(GetWinner());

            return true;
        }

        private BoardNode GetLeftMost(BoardNode node)
        {
            while (node.Left != null) node = node.Left;
            return node;
        }

        private BoardNode GetRightMost(BoardNode node)
        {
            while (node.Right != null) node = node.Right;
            return node;
        }

        public void NextTurn()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
        }

        public bool IsGameOver()
        {
            if (Players.Any(p => p.Hand.Count == 0))
                return true;

            var ends = GetBoardEnds();
            return Players.All(p => !p.HasPlayableTile(ends.left, ends.right));
        }

        private (int left, int right) GetBoardEnds()
        {
            if (Root == null) return (0, 0);
            return (GetLeftMost(Root).Tile.Left, GetRightMost(Root).Tile.Right);
        }

        public Player GetWinner()
        {
            return Players.OrderBy(p => p.Hand.Sum(t => t.Left + t.Right)).First();
        }
    }
}
