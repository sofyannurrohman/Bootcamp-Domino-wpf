using DominoGame.Models;
using System;
using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IGameController
    {
        IBoard Board { get; }
        List<Player> Players { get; }
        Player CurrentPlayer { get; }
        bool PlayTile(DominoTile tile, bool placeLeft);
        void NextTurn();
        bool IsGameOver();
        Player GetWinner();
    }
}
