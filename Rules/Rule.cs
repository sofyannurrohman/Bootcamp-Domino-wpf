using System.Collections.Generic;
using System.Linq;
using DominoGame.Models;

namespace DominoGame.Rules
{
    /// <summary>
    /// Static helper berisi aturan Block Domino.
    /// </summary>
    public static class Rule
    {
        /// <summary>
        /// Cek apakah tile bisa dimainkan pada ujung board.
        /// </summary>
        public static bool CanPlay(DominoTile tile, int leftEnd, int rightEnd)
        {
            return tile.Left == leftEnd ||
                   tile.Right == leftEnd ||
                   tile.Left == rightEnd ||
                   tile.Right == rightEnd;
        }

        /// <summary>
        /// Ambil semua tile dari tangan yang bisa dimainkan.
        /// </summary>
        public static List<DominoTile> GetPlayableTiles(IEnumerable<DominoTile> hand, int leftEnd, int rightEnd)
        {
            return hand.Where(tile => CanPlay(tile, leftEnd, rightEnd)).ToList();
        }

        /// <summary>
        /// Hitung total pip dari semua tile (scoring akhir).
        /// </summary>
        public static int CalculateScore(IEnumerable<DominoTile> hand)
        {
            return hand.Sum(tile => tile.Left + tile.Right);
        }
    }
}
