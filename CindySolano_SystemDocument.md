# System Documentation - Unity Programmer Task
**Cindy Solano**

## System Overview

This prototype is a **2D adventure game** built on Unity 6000.2.15f1 where the player explores the world, collects items, and completes delivery requests from NPCs. The project implements a slot-based inventory system, NPC dialogue with quest chains, collectible resources, and a persistent save/load system.

**Core Systems Implemented:**
- **Character Controller**: Top-down 2D movement with animator-driven animations and flexible input handling
- **Slot-Based Inventory**: 9-slot inventory with drag-and-drop functionality, item stacking, merging, and deletion with quantity slider
- **NPC Quest System**: Dialogue system with multi-step quest chains where NPCs request specific items and track delivery progress
- **Collectible Nodes**: Resource gathering points scattered throughout the world with cooldown timers
- **Save/Load System**: JSON-based persistence maintaining exact slot positions and inventory state

## Thought Process

My approach focused on building modular, data-driven systems. I utilized ScriptableObjects for items, quests, and databases to enable easy content expansion without code changes. The inventory system prioritizes slot persistence—items remain in their exact positions across sessions, which is essential for player experience.

I chose to extend a base asset rather than build from scratch, allowing me to focus on implementing the core requirements while maintaining production-quality visuals. The NPC request system was designed with chainable quests, enabling progressive storylines where players must gather and deliver items.

**Initial Vision**: I planned to implement a crafting system where players would collect materials, craft items at workstations, and deliver orders to NPCs. A stamina/mana system would add resource management depth. Time constraints prevented full implementation of these features.

## Personal Assessment

**Strengths**: Clean code architecture, functional drag-and-drop inventory, working save/load persistence, and polished NPC dialogue integration with item delivery mechanics.

**Areas for Improvement**: The crafting system remains unimplemented. Given more time, I would add crafting recipes, workstation interactions, and the stamina mechanic for crafting actions.

**Final Reflection**: The prototype successfully demonstrates core inventory mechanics, world interaction, and data persistence—the essential building blocks for an engaging gameplay loop.
