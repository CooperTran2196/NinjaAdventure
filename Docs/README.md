# NinjaAdventure - Development Documentation

Documentation for the weapon and combat system implementation.

## 📚 Table of Contents

### Core Systems

1. **[Weapon Anchoring](WEAPON_ANCHOR_CHANGES.md)**
   - Problem: Weapons floating in place when player moves during long attacks
   - Solution: Parent-based anchoring system
   - Status: ✅ Implemented

2. **[Attack Movement System](ATTACK_MOVEMENT_CHANGES.md)**
   - Problem: Player locked in place during attacks
   - Solution: Allow movement with configurable speed penalty
   - Status: ⚠️ Superseded by per-weapon penalty system

3. **[Per-Weapon Movement Penalty](WEAPON_MOVE_PENALTY.md)**
   - Problem: All weapons had same movement penalty
   - Solution: Move penalty from C_Stats to W_SO (weapon data)
   - Status: ✅ Implemented & Active

4. **[Weapon ShowTime Animation Lock](WEAPON_SHOWTIME_ANIMATION_LOCK.md)**
   - Problem: Animation loops/ends before weapon showTime completes
   - Solution: Freeze animation at final frame until showTime completes
   - Status: ✅ Implemented

### State Management

5. **[Attack + Movement Concurrent States](ATTACK_MOVEMENT_FIX.md)**
   - Problem: Attack state disabled when player tried to move
   - Solution: Allow Attack and Movement states to coexist
   - Status: ✅ Implemented

6. **[State Transition System](STATE_TRANSITION_FIX.md)**
   - Problem: States not properly transitioning after Attack/Dodge ends
   - Solution: Auto-restore proper state based on input
   - Status: ✅ Implemented

7. **[Enemy State System](ENEMY_STATE_SYSTEM.md)**
   - Problem: Enemy AI lacked player's state architecture benefits
   - Solution: Applied same patterns - concurrent states, weapon penalty, showTime lock
   - Status: ✅ Implemented

### Animation System

7. **[Animation Priority](ANIMATION_PRIORITY_FIX.md)**
   - Problem: Both isAttacking and isMoving true → animation conflict
   - Solution: Attack animation always has priority over movement
   - Status: ✅ Implemented

8. **[Attack State Cleanup](ATTACK_STATE_CLEANUP_FIX.md)**
   - Problem: isAttacking animator bool never turning off
   - Solution: Disable state component when attack finishes
   - Status: ✅ Implemented

---

## 🗂️ Reading Order (For New Developers)

If you're new to this codebase, read in this order:

1. Start: [Weapon Anchoring](WEAPON_ANCHOR_CHANGES.md) - Understanding weapon positioning
2. Then: [Per-Weapon Movement Penalty](WEAPON_MOVE_PENALTY.md) - How weapons affect movement
3. Then: [Weapon ShowTime Animation Lock](WEAPON_SHOWTIME_ANIMATION_LOCK.md) - How animations work
4. Then: [Attack + Movement Fix](ATTACK_MOVEMENT_FIX.md) - Understanding concurrent states
5. Then: [State Transition Fix](STATE_TRANSITION_FIX.md) - State management logic
6. Then: [Animation Priority](ANIMATION_PRIORITY_FIX.md) - Animation system priority
7. Then: [Attack State Cleanup](ATTACK_STATE_CLEANUP_FIX.md) - Edge case handling
8. Finally: [Enemy State System](ENEMY_STATE_SYSTEM.md) - Same architecture applied to enemies

---

## 🏗️ System Architecture Overview

```
Player Input                          Enemy AI (ProcessAI)
    ↓                                      ↓
P_Controller (State Manager)      E_Controller (State Manager)
    ↓                                      ↓
P_State_* Components              E_State_* Components
    ├── P_State_Idle                   ├── State_Idle (shared)
    ├── P_State_Movement               ├── State_Chase
    ├── P_State_Attack                 ├── State_Attack (enemy version)
    └── P_State_Dodge                  └── State_Wander (shared with NPCs)
    ↓                                      ↓
Concurrent States Support         Concurrent States Support
(Movement + Attack)              (Chase + Attack)
    ↓                                      ↓
Animator (Priority: Attack > Movement > Idle)
    ↓
Visual Output
```

### Key Components

- **P_Controller / E_Controller**: Central state managers, handle input/AI and state transitions
- **P_State_Attack / State_Attack**: Attack timing, weapon invocation, animation freeze
- **P_State_Movement / State_Chase**: Movement velocity, penalty application, animation control
- **W_Base**: Weapon base class, anchoring system, hit effects
- **W_SO**: Weapon ScriptableObject, data-driven design (penalty, showTime, damage)

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
**Solution:** Check [STATE_TRANSITION_FIX.md](STATE_TRANSITION_FIX.md)
- SetAttacking(false) should restore state
- Check moveAxis still has input

### Issue: Both attack and walk animations playing
**Solution:** Check [ANIMATION_PRIORITY_FIX.md](ANIMATION_PRIORITY_FIX.md)
- P_State_Movement should set isMoving = false during attack
- Attack animation has priority

---

## 📝 Contributing

When adding new features:
1. Follow existing state component patterns
2. Document decisions in new .md file in this folder
3. Update this README with links
4. Add to relevant sections

---

## 📅 Document History

| Date | Document | Change |
|------|----------|--------|
| 2025-10-11 | All | Initial documentation creation |
| 2025-10-11 | WEAPON_MOVE_PENALTY.md | Moved penalty from C_Stats to W_SO |
| 2025-10-11 | README.md | Created master index |

---

**Last Updated:** October 11, 2025
**Status:** All systems operational ✅
