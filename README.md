# CindySolano_Task - Unity Programmer Interview

A 2D adventure prototype where the player explores the world, collects items, and completes delivery requests from NPCs.

## Overview

This prototype demonstrates core gameplay systems including character movement, a slot-based inventory with drag-and-drop functionality, NPC dialogue with quest chains, and a persistent save/load system.

**Built with Unity 6000.2.15f1**

---

## Features

### Gameplay
- Top-down 2D character movement with smooth animations
- Explore the world and collect items from harvest nodes
- Talk to NPCs and complete their delivery requests
- Dynamic day/night cycle with lighting transitions

### Inventory System
- **Slot-based UI** with 9 inventory slots
- **Drag and drop** items between slots
- **Item stacking** and merging for stackable items
- **Item deletion** with quantity slider for multiple amounts
- **Tooltips** showing item details on hover
- Automatic save on every inventory change

### NPC Quest System
- Dialogue system with NPC portraits
- NPCs request specific items for delivery
- Multi-step quest chains with progression tracking
- Dynamic requirement checking from inventory
- Visual feedback on quest completion

### Save/Load System
- JSON-based persistence
- Slot-based inventory state preservation
- Items maintain exact positions across sessions
- Error recovery for corrupted saves

---

## Controls

| Key | Action |
|-----|--------|
| WASD / Arrow Keys | Move character |
| E | Interact (NPCs, collect items) |
| I | Open/Close inventory |
| Mouse Drag | Move items between inventory slots |

---

## How to Play

1. Explore the world and find collectible items
2. Press **E** near glowing nodes to collect items
3. Talk to NPCs by pressing **E** - they will request specific items
4. Gather the required items and return to the NPC to complete the delivery
5. Progress through multiple quests per NPC

### Download
1. Go to the [Releases](../../releases) section
2. Download `CindySolano_Task.zip`
3. Extract and run `CindySolano_Task.exe`

### Open in Unity
1. Clone this repository
2. Open the project with **Unity 6000.2.15f1**
3. Open the main scene in `Assets/_ProyectTask/Scenes/`
4. Press Play

---

## Project Structure

```
Assets/
├── _ProyectTask/           # Main project scripts and assets
│   ├── Scripts/
│   │   ├── Inventory/      # Slot-based inventory system
│   │   ├── NPC/            # Dialogue and quest system
│   │   ├── Player/         # Character controller
│   │   └── Harvest/        # Item collection
│   ├── Data/               # ScriptableObjects (items, quests)
│   └── Scenes/             # Game scenes
│
└── HappyHarvest/           # Base art assets and utilities
```

---

## Technical Highlights

- **Data-Driven Design**: ScriptableObjects for items, quests, and databases
- **Modular Architecture**: Loosely coupled systems via interfaces
- **Singleton Managers**: GameManager, NPCRequestManager, UIHandler
- **View Pool Pattern**: Efficient UI slot management
- **JSON Serialization**: Robust save system with error recovery

---

## Requirements Checklist

- [x] Character movement logic
- [x] Character animations
- [x] Character interaction with world (items, NPCs)
- [x] UI slot-based inventory
- [x] Add items to inventory
- [x] Remove items from inventory
- [x] Move items within inventory
- [x] Drag and swap items between slots
- [x] Item tooltips on hover
- [x] Save system for inventory state
- [x] Load inventory data on game start
- [x] Slot-based persistence

---

## Documentation

See [CindySolano_SystemDocument.pdf](CindySolano_SystemDocument.pdf) for detailed system explanation, thought process, and personal assessment.

---

## Author

**Cindy Solano**

Developed for NG+ Unity Programmer Interview Task
