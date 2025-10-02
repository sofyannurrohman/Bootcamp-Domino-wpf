using System;
using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IGameController
    {
        IBoard Board { get; }
        List<IPlayer> Players { get; }
        // Current player in turn
        IPlayer CurrentPlayer { get; }
        // Current round tracking
        int CurrentRound { get; }
        int MaxRounds { get; }
        #region Game Setup
        /// Start a new round without restarting the whole game
        void StartNextRound();

        #endregion

        #region Game Actions
        /// Play a tile for a player; returns true if successful
        bool PlayTile(IPlayer player, IDominoTile tile, bool placeLeft);
        
        /// Move to the next player's turn, skipping if they cannot play
        void NextTurn();
        
        /// Check if a player has at least one playable tile
        bool HasPlayableTile(IPlayer player);

        /// Get the next playable tile for a player, null if none
        (IDominoTile tile, bool placeLeft)? GetNextPlayableTile(IPlayer player);

        #endregion

        #region Round & Game State
        /// Check if the current round is over        
        bool IsRoundOver();

        IPlayer? GetRoundWinner();

        /// End the current round and update scores
        void EndRound();
        
        /// Check if the game is over
        bool IsGameOver();

        /// Get the overall game winner
        IPlayer? GetWinner();

        #endregion

        #region Events
        /// Fired when a tile is successfully played
        event Action<IPlayer, IDominoTile, bool>? OnTilePlayed;
        
        /// Fired when a round ends        
        event Action<IPlayer?>? OnRoundOver;

        /// Fired when the game ends        
        event Action<IPlayer>? OnGameOver;

        /// Fired when a player is skipped because they cannot play
        event Action<IPlayer>? OnPlayerSkipped;

        #endregion
    }
}
