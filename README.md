# Domino Game WPF MVVM

A classic **Domino Block Game** implemented in **C# WPF** using **MVVM architecture** with **Dependency Injection** for clean design, testability, and maintainability.

---

## Table of Contents

- [Features](#features)  
- [Architecture](#architecture)  
- [Installation](#installation)  
- [Usage](#usage)  
- [Project Structure](#project-structure)  
- [Gameplay Rules](#gameplay-rules)  
- [Quick Visual Diagram](#quick-visual-diagram)  
- [Contributing](#contributing)  
- [License](#license)  

---

## Features

- Two-player Domino game (Human vs Computer)  
- Block Domino rules with doubles and pip sums  
- MVVM architecture for clean separation of concerns  
- Dependency Injection for services and controller  
- ObservableCollections for real-time UI updates  
- Highlight playable tiles using WPF converters  
- Multi-round scoring with round and game winner detection  

---

## Architecture

This project uses **MVVM + DI pattern**:

MainWindow (View)
->
GameViewModel
->
DominoGameController
->
Services:
- BoardService
- PlayerService
- TurnService
Models:
- Board
- Deck
- Player
- DominoTile
Interfaces:
- IBoard, IDeck, IPlayer, IDominoTile
- IBoardService, IPlayerService, ITurnService
Converters:
- TilePlayableConverter (highlight playable tiles)


**Highlights:**

- **Dependency Injection**: All services and controller are injected, no manual instantiation in `MainWindow` or `ViewModel`.
- **ObservableCollection**: Automatically updates UI when board or hand changes.
- **Converters**: Encapsulate tile highlighting rules, use interfaces for flexibility.

---

## Installation

1. Clone the repository:

```bash
git clone https://github.com/yourusername/domino-wpf.git
```
2. Open in Visual Studio 2022+.

3. Restore NuGet packages.

4. Build the solution.

---

## Usage

1. Run the project (F5)

2. The main window shows your hand, the board, and computer moves.

3. Click a playable tile to play it. Tiles you cannot play are grayed out.

4. Game will notify when round is over and display the winner.

5. The game continues until the maximum number of rounds or a player reaches the target score.

---
## Gameplay Rules

1. Opening Move: If player has doubles, only the largest double can start. Otherwise, the tile with the highest pip sum starts.
   
2. Normal Play: Tiles must match either left or right end of the board. Tiles are flipped if necessary to match board ends.
   
3. Round End: When no player can play or all hands are empty. Scores are calculated based on remaining tiles in opponent hands.
   
4. Game End: Maximum rounds reached or a player reaches target score.
