using System;
using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IGameController
    {
        // Current board state
        IBoard Board { get; }

        // Players participating
        List<IPlayer> Players { get; }

        // Current player in turn
        IPlayer CurrentPlayer { get; }

        // Current round tracking
        int CurrentRound { get; }
        int MaxRounds { get; }

        #region Game Setup

        // Start a new game with optional maxRounds
        void StartGame(int maxRounds = 5);

        // Start a new round
        void StartNextRound();

        #endregion

        #region Game Actions

        // Attempt to play a tile (returns true if successful)
        bool PlayTile(IPlayer player, IDominoTile tile, bool placeLeft);

        // Move to the next player's turn (skip if no playable tile)
        void NextTurn();

        // Check if a player has at least one playable tile
        bool HasPlayableTile(IPlayer player);

        // Get the next playable tile for a player (null if none)
        (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player);

        #endregion

        #region Round & Game State

        // Check if the current round is over
        bool IsRoundOver();

        // Get the winner of the current round (null if draw)
        IPlayer? GetRoundWinner();

        // End the round and update scores
        void EndRound();

        // Check if the game is over (any player score >= 100)
        bool IsGameOver();

        // Get the overall game winner (first player with highest score)
        IPlayer? GetWinner();

        #endregion

        #region Events

        // Fired when a tile is successfully played
        event Action<IPlayer, IDominoTile, bool>? OnTilePlayed;

        // Fired when a round ends
        event Action<IPlayer?>? OnRoundOver;

        // Fired when the game ends
        event Action<IPlayer>? OnGameOver;

        #endregion
    }
}
