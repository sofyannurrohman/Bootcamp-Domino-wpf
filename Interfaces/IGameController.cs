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

        /// <summary>
        /// Initialize a new game with optional maximum rounds
        /// </summary>
        void StartGame(int maxRounds = 5);

        /// <summary>
        /// Start a new round without restarting the whole game
        /// </summary>
        void StartNextRound();

        #endregion

        #region Game Actions

        /// <summary>
        /// Play a tile for a player; returns true if successful
        /// </summary>
        bool PlayTile(IPlayer player, IDominoTile tile, bool placeLeft);

        /// <summary>
        /// Move to the next player's turn, skipping if they cannot play
        /// </summary>
        void NextTurn();

        /// <summary>
        /// Check if a player has at least one playable tile
        /// </summary>
        bool HasPlayableTile(IPlayer player);

        /// <summary>
        /// Get the next playable tile for a player, null if none
        /// </summary>
        (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player);

        #endregion

        #region Round & Game State

        /// <summary>
        /// Check if the current round is over
        /// </summary>
        bool IsRoundOver();

        /// <summary>
        /// Get the winner of the current round, or null if draw
        /// </summary>
        IPlayer? GetRoundWinner();

        /// <summary>
        /// End the current round and update scores
        /// </summary>
        void EndRound();

        /// <summary>
        /// Check if the game is over
        /// </summary>
        bool IsGameOver();

        /// <summary>
        /// Get the overall game winner
        /// </summary>
        IPlayer? GetWinner();

        #endregion

        #region Events

        /// <summary>
        /// Fired when a tile is successfully played
        /// </summary>
        event Action<IPlayer, IDominoTile, bool>? OnTilePlayed;

        /// <summary>
        /// Fired when a round ends
        /// </summary>
        event Action<IPlayer?>? OnRoundOver;

        /// <summary>
        /// Fired when the game ends
        /// </summary>
        event Action<IPlayer>? OnGameOver;

        /// <summary>
        /// Fired when a player is skipped because they cannot play
        /// </summary>
        event Action<IPlayer>? OnPlayerSkipped;

        #endregion
    }
}
