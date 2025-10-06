using System;
using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IGameController
    {
        IBoard Board { get; }
        List<IPlayer> Players { get; }
        IPlayer CurrentPlayer { get; }
        int CurrentRound { get; }
        int MaxRounds { get; }
        void StartNextRound();
        bool PlayTile(IPlayer player, IDominoTile tile, bool placeLeft);
        void NextTurn();
        bool HasPlayableTile(IPlayer player);
        (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player);
        bool IsRoundOver();
        IPlayer? GetRoundWinner();
        void EndRound();
        bool IsGameOver();
        IPlayer? GetWinner();
        event Action<IPlayer, IDominoTile, bool>? OnTilePlayed;
        event Action<IPlayer?>? OnRoundOver;
        event Action<IPlayer>? OnGameOver;
        event Action<IPlayer>? OnPlayerSkipped;
    }
}
