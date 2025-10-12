# NinjaAdventure - Development Documentation

Complete technical documentation for the combat, state, and AI systems.

---

## 🎯 Quick Start

**New developers start here:**

1. **[Combo System Guide](COMBO_SYSTEM_GUIDE.md)** ⭐  
   Complete 3-hit combo system with radar rotation, input buffering, and sprite setup

2. **[Weapon System Guide](WEAPON_SYSTEM_GUIDE.md)**  
   Weapon architecture, anchoring, movement penalties, and configuration

3. **[State System Guide](STATE_SYSTEM_GUIDE.md)**  
   Player state management, concurrent states, transitions, and animation priority

4. **[Enemy AI Guide](ENEMY_AI_GUIDE.md)**  
   Enemy state machine, behaviors, detection, combat, and testing

---

## 📚 System Overviews

### Combat Systems
- **Combo System:** 3-hit melee chain (Slash Down → Slash Up → Thrust)
- **Weapons:** Modular ScriptableObject-based system with shared player/enemy support
- **Damage:** Unified hit detection with lifesteal, knockback, and stun
- **Movement Penalties:** Per-weapon and per-attack speed modifiers

### State Management
- **Concurrent States:** Attack + Movement work together
- **Clean Transitions:** Auto-restore proper states after actions
- **Animation Priority:** Attack > Dodge > Movement > Idle
- **Cleanup:** Proper OnDisable handling for animator bools

### Enemy AI
- **State Machine:** Idle → Wander → Chase → Attack
- **Detection:** Circular player tracking with configurable radius
- **Combat:** Same weapon system as player
- **Rewards:** XP/loot on death

---

## 🏗️ Architecture Highlights

**Shared Systems (Player + Enemy):**
- `C_Stats` - Health, damage, armor, speed
- `C_Health` - Damage/healing with events
- `W_Base` - Weapon attacks & hit effects

**Player Systems:**
- `P_Controller` - Input routing & state switching
- `P_State_*` - Idle, Movement, Attack, Dodge
- `P_Combat` - Weapon management & aiming

**Enemy Systems:**
- `E_Controller` - AI brain & state switching
- `E_State_*` - Idle, Wander, Chase, Attack
- `E_Detection` - Player tracking
- `E_Combat` - Attack execution

---

## 📖 Additional Docs

- **[Design History](DESIGN_HISTORY.md)** - Evolution of major systems
- **[Maintenance Notes](MAINTENANCE_NOTES.md)** - Known issues & workarounds

---

## ✅ System Status

All core systems production-ready:
- ✅ Combo system (3-hit chain with radar rotation)
- ✅ Weapon system (modular, shared player/enemy)
- ✅ State management (concurrent states, clean transitions)
- ✅ Enemy AI (full state machine with combat)
- ✅ Input buffering (1.0s window, button mashing support)
- ✅ Animation priority (attack overrides movement)

**Last Updated:** 2024 (Documentation Consolidation Complete)

### Animation System

9. **[Animation Priority](ANIMATION_PRIORITY_FIX.md)**
   - Attack animation always overrides movement
   - Prevents animation conflicts
   - Status: ✅ Implemented

10. **[Attack State Cleanup](ATTACK_STATE_CLEANUP_FIX.md)**
    - Proper cleanup when attack finishes
    - Prevents stuck animator bools
    - Status: ✅ Implemented

---

## 🗂️ Reading Order (For New Developers)

### ⚡ Fast Track (Get Started Quick):
1. **[Combo System Complete Guide](COMBO_SYSTEM_COMPLETE_GUIDE.md)** - Everything you need!
2. **[Weapon Sprite Setup Guide](WEAPON_SPRITE_SETUP_GUIDE.md)** - Configure your sprites
3. Test in Unity Play Mode!

### 📖 Full Understanding (Deep Dive):

#### Phase 1: Core Concepts
1. [Combo System Complete Guide](COMBO_SYSTEM_COMPLETE_GUIDE.md) - Modern combat system
2. [Weapon Sprite Setup](WEAPON_SPRITE_SETUP_GUIDE.md) - Bottom-pivot requirement
3. [Weapon Anchoring](WEAPON_ANCHOR_CHANGES.md) - How weapons position
4. [Per-Weapon Movement Penalty](WEAPON_MOVE_PENALTY.md) - Movement during attacks
5. [Weapon ShowTime Animation Lock](WEAPON_SHOWTIME_ANIMATION_LOCK.md) - Animation timing

#### Phase 2: State System
6. [Attack + Movement Fix](ATTACK_MOVEMENT_FIX.md) - Concurrent states
7. [State Transition Fix](STATE_TRANSITION_FIX.md) - State management
8. [Animation Priority](ANIMATION_PRIORITY_FIX.md) - Animation system
9. [Attack State Cleanup](ATTACK_STATE_CLEANUP_FIX.md) - Edge cases

### Phase 3: Enemy AI System
8. Then: [Enemy State System](ENEMY_STATE_SYSTEM.md) - Full enemy AI refactor applying all player patterns

### Phase 4: Advanced Combat
9. Read: [Attack Combo System Design](COMBO_SYSTEM_DESIGN.md) - 3-hit combo design plan
10. Then: [Combo Implementation Summary](COMBO_IMPLEMENTATION_SUMMARY.md) - How it all works together
8. Finally: [Enemy State System](ENEMY_STATE_SYSTEM.md) - Same architecture applied to enemies

---

## 🏗️ System Architecture Overview

### Player System
```
Player Input
    ↓
P_Controller (State Manager)
    ↓
P_State_* Components (Concurrent States)
    ├── P_State_Idle
    ├── P_State_Movement ← Reads weapon penalty
    ├── P_State_Attack ← Manages weapon showTime
    └── P_State_Dodge
    ↓
Animator (Priority: Attack > Movement > Idle)
    ↓
Visual Output
```

### Enemy System
```
AI Logic
    ↓
E_Controller (State Manager)
    ↓
State_* Components (Concurrent States)
    ├── State_Idle
    ├── State_Wander
    ├── State_Chase ← Reads weapon penalty during attack
    └── State_Attack ← Manages weapon showTime
    ↓
Animator (Priority: Attack > Movement > Idle)
    ↓
Visual Output
```

### Key Components

**Player:**
- **P_Controller**: Central state manager, handles input and state transitions
- **P_State_Attack**: Attack timing, weapon invocation, animation freeze
- **P_State_Movement**: Movement velocity, penalty application, animation control

**Enemy:**
- **E_Controller**: AI state manager, handles target detection and attack logic
- **State_Attack**: Attack timing, weapon invocation, animation freeze, state restoration
- **State_Chase**: Chase movement, penalty application during attack, animation control

**Shared:**
- **W_Base**: Weapon base class, anchoring system (used by both player and enemy)
- **W_SO**: Weapon ScriptableObject, data-driven design (attackMovePenalty, showTime)

---

## 🔧 Quick Reference

### Attack Movement Penalty (Per Weapon)
```csharp
// In W_SO.cs
[Range(0f, 1f)]
public float attackMovePenalty = 0.5f;

// Fast weapons: 0.7-0.8 (mobile)
// Medium weapons: 0.5-0.6 (balanced)
// Slow weapons: 0.2-0.4 (sluggish)
```

### Weapon ShowTime
```csharp
// In W_SO.cs
public float showTime = 0.3f;

// If showTime > 0.45f (animation duration):
// → Animation freezes at final frame
// → Player locked until showTime completes
```

### State Component Lifecycle
```csharp
// Enable state
component.enabled = true;
→ OnEnable() called
→ Animator bools set

// Disable state
component.enabled = false;
→ OnDisable() called
→ Cleanup animator bools
```

---

## 🐛 Common Issues & Solutions

### Issue: Weapon floats away from player
**Solution:** Check [WEAPON_ANCHOR_CHANGES.md](WEAPON_ANCHOR_CHANGES.md)
- Weapon should parent to owner during attack
- EndVisual() should restore original parent

### Issue: Player stuck in attack animation
**Solution:** Check [ATTACK_STATE_CLEANUP_FIX.md](ATTACK_STATE_CLEANUP_FIX.md)
- Ensure `enabled = false` in AttackRoutine()
- Check OnDisable() is called

### Issue: Movement animation doesn't resume after attack
**Solution:** Check [STATE_TRANSITION_FIX.md](STATE_TRANSITION_FIX.md) (Player) or [ENEMY_STATE_SYSTEM.md](ENEMY_STATE_SYSTEM.md) (Enemy)
- SetAttacking(false) should restore state
- Check moveAxis/target still has input

### Issue: Both attack and walk animations playing
**Solution:** Check [ANIMATION_PRIORITY_FIX.md](ANIMATION_PRIORITY_FIX.md) (Player) or [ENEMY_STATE_SYSTEM.md](ENEMY_STATE_SYSTEM.md) (Enemy)
- P_State_Movement / State_Chase should set isMoving = false during attack
- Attack animation has priority

### Issue: Enemy stuck in attack state
**Solution:** Check [ENEMY_STATE_SYSTEM.md](ENEMY_STATE_SYSTEM.md)
- Ensure `enabled = false` in State_Attack.AttackRoutine()
- Check State_Attack.OnDisable() restores chase state
- Verify E_Controller.SetAttacking(false) is called

---

## 📝 Contributing

When adding new features:
1. Follow existing state component patterns (see Player & Enemy systems)
2. Apply changes to both Player AND Enemy if it affects combat
3. Document decisions in new .md file in this folder
4. Update this README with links
5. Add to relevant sections
6. Update the "Document History" table below

---

## 📅 Document History

| Date | Document | Change |
|------|----------|--------|
| 2025-10-11 | All (Player docs) | Initial documentation creation |
| 2025-10-11 | WEAPON_MOVE_PENALTY.md | Moved penalty from C_Stats to W_SO |
| 2025-10-11 | README.md | Created master index |
| 2025-10-12 | ENEMY_STATE_SYSTEM.md | Applied all player state patterns to enemy AI |
| 2025-10-12 | README.md | Updated with enemy system documentation |
| 2025-10-12 | COMBO_SYSTEM_DESIGN.md | Complete design doc for 3-hit combo system |
| 2025-10-12 | COMBO_IMPLEMENTATION_SUMMARY.md | Full implementation + testing guide |
| 2025-10-12 | COMBO_IMPROVEMENTS.md | Arc rotation fix + button mashing improvements |
| 2025-10-12 | W_SO.cs, W_Base.cs, W_Melee.cs | Combo arrays + arc slash movement |
| 2025-10-12 | P_State_Attack.cs | Combo tracking, 1.0s input window, immediate buffering |
| 2025-10-12 | P_State_Movement.cs, P_Controller.cs | Combo penalties + input handling |
| 2025-10-12 | C_Health.cs | Damage cancels combo |

---

**Last Updated:** October 12, 2025
**Status:** All systems operational ✅ | Combo system implemented and ready to test! 🎮
