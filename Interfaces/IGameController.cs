using DominoGame.Models;
using System;
using System.Collections.Generic;

namespace DominoGame.Interfaces
{
    public interface IGameController
    {
        // Current board state
        IBoard Board { get; }

        // Players participating
        List<Player> Players { get; }

        // Current player in turn
        Player CurrentPlayer { get; }

        // Current round tracking
        int CurrentRound { get; }
        int MaxRounds { get; }

        #region Game Actions

        // Attempt to play a tile (returns true if successful)
        bool PlayTile(Player player, DominoTile tile, bool placeLeft);

        // Move to the next player's turn
        void NextTurn();

        // Draw a tile from the boneyard for a player
        DominoTile? DrawTile(Player player);

        // Check if a player has at least one playable tile
        bool HasPlayableTile(Player player);

        // Auto-play or draw until a player can play (returns true if player can play)
        DominoTile? TryPlayOrDraw(Player player);

        #endregion

        #region Round & Game State

        // Check if the current round is over
        bool IsRoundOver();

        // Get the winner of the current round
        Player GetRoundWinner();

        // Check if the game is over
        bool IsGameOver();

        // Get the overall game winner
        Player GetWinner();

        #endregion

        #region Events

        // Fired when a tile is successfully played
        event Action<Player, DominoTile, bool>? OnTilePlayed;

        // Fired when a round ends
        event Action<Player>? OnRoundOver;

        // Fired when the game ends
        event Action<Player>? OnGameOver;

        #endregion
    }
}
