# Project Abstract

**Project:** NinjaAdventure - A Unity 2D Action-RPG  
**Developer:** Cooper Tran  
**Development Period:** October - November 2025  
**Engine:** Unity (C#)  
**Status:** Production Complete

---

## Project Abstract

### Title
**NinjaAdventure: A Modular 2D Action RPG Built with Controller State Architecture in Unity**

### Background/Problem Statement
Traditional Unity game development often results in monolithic scripts with tightly coupled logic, making debugging difficult and system expansion cumbersome. This project addressed the challenge of building a scalable, maintainable 2D action RPG by implementing a clean separation of concerns through a controller state architecture pattern.

### Objectives
The primary objectives were to:
1. Design and implement a complete 2D action RPG with combat, inventory, progression, and exploration systems
2. Develop a reusable controller state architecture that separates game logic from execution flow
3. Create three distinct boss encounters with unique AI behaviors using the same architectural foundation
4. Build a comprehensive dialog and NPC interaction system supporting branching narratives
5. Implement a data driven weapon and skill system using ScriptableObjects for designer friendly content creation

### Methodology
The project employed a modular component-based architecture where:

**Core Architecture:**
- Controllers (Player, Enemy, NPC, Boss) managed state machines and physics execution in FixedUpdate loops
- States (Idle, Movement, Attack, Dodge, Chase) computed intent through SetDesiredVelocity() API calls
- All components declared dependencies via RequireComponent attributes, ensuring guaranteed references

**Data Systems:**
- ScriptableObjects provided data driven configuration for weapons, items, skills, and dialog content
- Event driven communication through health events, experience events, and loot events decoupled systems
- Unified stat effect system with duration based modifiers supporting permanent, instant, and timed effects

**Technical Implementation:**
- Unity Input System generated action maps from visual editor configurations
- Custom physics prediction for boss dash attacks using animation clip lengths
- Circular perimeter spawning algorithm for summoner boss minion placement
- PolygonCollider2D auto update system synchronized with sprite animation frames
- Layered architecture with clear separation between UI, managers, controllers, states, and components

**Development Tools:**
- Git version control with feature branching for organized development workflow
- Unity as the primary game engine for all game systems and runtime logic
- LDtk for level design and map editing with tileset integration
- Libresprite for sprite creation and pixel art animation editing
- VS Code with C# extensions and Unity debugger integration for efficient development

### Results/Findings
The project successfully delivered a complete 2D action-RPG with the following systems:

**Player Systems:**
- Multi-state controller supporting Idle, Movement, Attack (3-hit combo), and Dodge with invulnerability frames
- Experience and leveling system with skill point allocation
- Dual weapon slots (melee/ranged) with hotkey switching
- Inventory system supporting stackable items, consumables, and equipment
- Statistics system with 15 modifiable attributes

**Enemy AI Systems:**
- Standard enemies with four state AI that transitions from idle to wandering to chasing to attacking
- Three unique boss encounters including Giant Red Samurai with charge dash and double hit combo attacks, Giant Raccoon with dual attack modes featuring normal charge and jump area of effect with radial knockback, and Giant Summoner with two phase behavior featuring aggressive summoning in phase one transitioning to defensive retreat mode when health drops below twenty percent
- All bosses implemented collision damage with cooldown timers and proper death handling

**Content Systems:**
- Dialog system with branching choices (max 3 options), NPC tracking, and location history
- Shop system with category filtering, gold validation, and inventory space checking
- Loot system with configurable drop tables, drop chance percentages, and ground pickup mechanics
- Skill tree with passive stat bonuses and modifier stacking

**Environment Systems:**
- Climbable ladders with adjustable speed multipliers
- Boss-linked destructible gates that fade on target death
- Damage zones, destructible objects, and drowning areas
- Scene teleportation with spawn point ID system

**Audio/Visual Polish:**
- 41 sound effects integrated (attack impacts, pickups, NPC dialog, UI interactions)
- Afterimage trail system for dodge rolls and boss dashes
- Health bar UI for enemies
- Damage flash and fade-out effects on death

**Code Quality Metrics:**
- Complete game architecture with modular component based design pattern
- Zero compilation errors and warnings in production build
- Consistent naming conventions using prefix based system for different component types
- Well documented codebase with inline comments

### Conclusion/Significance
This project demonstrated that a controller state architecture with event driven communication significantly improves code maintainability and system extensibility in Unity game development. The clean separation between state logic and physics execution allowed for rapid iteration across all character types, including player controls, standard enemy AI, and specialized boss encounters. Each entity type reused the same core state infrastructure while implementing unique behaviors through specialized state scripts, proving the architecture's flexibility and scalability.

Key architectural achievements included:

**Maintainability:** The RequireComponent pattern combined with event subscription eliminated null reference exceptions and race conditions throughout the entire codebase. Controllers guaranteed component existence across player systems, enemy AI, NPC interactions, and environmental triggers, allowing all state scripts to safely use direct assignment instead of null coalescing operators. This made architectural contracts explicit and reduced debugging time significantly.

**Reusability:** The controller interface enabled complete orthogonality between all entity types in the game. Players, standard enemies, minibosses, final bosses, and NPCs all implemented the same core APIs for movement, state management, and physics interactions. This allowed the development of generic systems like the loot drop mechanism, collision detection, and animation controllers that worked seamlessly across all character types without modification.

**Scalability:** The data driven ScriptableObject approach extended beyond combat mechanics to encompass all game content. Designers could create new weapons, items, consumables, dialog trees, NPC interactions, and skill configurations without programmer intervention. The unified stat effect system supported arbitrary modifier combinations across player attributes, enemy statistics, and item bonuses, enabling rapid gameplay balancing and content expansion through simple asset creation.

**Extensibility:** The modular component architecture allowed independent development of major game systems. The inventory system, shop mechanics, dialog manager, experience tracker, and skill tree all operated through well defined interfaces without direct dependencies. Adding new features like the ladder climbing system, destructible gates, or boss specific attack patterns required minimal changes to existing code, demonstrating proper separation of concerns throughout the project.

This architecture pattern is directly applicable to other Unity projects requiring character controllers, AI state machines, or data driven content pipelines. The principles of state separation, event driven communication, and interface based design provide a foundation for building complex interactive systems that remain maintainable as projects scale in scope and complexity.
