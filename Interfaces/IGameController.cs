using DominoGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoGame.Interfaces
{
    public interface IGameController
    {
        BoardNode? Root { get; }
        List<Player> Players { get; }
        Player CurrentPlayer { get; }
        bool PlayTile(DominoTile tile, bool placeLeft);
        void NextTurn();
        bool IsGameOver();
        Player GetWinner();
    }

}
