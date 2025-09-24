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
        List<DominoTile> Board { get; }
        Player CurrentPlayer { get; }
        void StartGame();
        bool PlayTile(DominoTile tile, bool placeLeft);
        void NextTurn();
        bool IsGameOver();
        Player GetWinner();
    }

}
