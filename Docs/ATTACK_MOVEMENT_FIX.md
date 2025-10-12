# Attack + Movement Fix - Concurrent States

## Problem Fixed
When trying to move during attack, the state would switch from `Attack` to `Move`, which **disabled the attack state component**, stopping the attack animation/logic.

## Root Cause
The original `SwitchState()` method always disabled ALL states before enabling the new one:
```csharp
// OLD - BAD
idle.enabled = move.enabled = attack.enabled = dodge.enabled = false;
```

This meant:
1. Player attacks → `Attack` state enabled
2. Player moves → Switch to `Move` state
3. `SwitchState()` disables all states → **Attack disabled** ❌
4. Attack animation stops, attack routine cancelled

## Solution Overview
Changed the system to allow **Movement and Attack to coexist**:
- Movement is now **independently toggled** (enabled/disabled) without affecting attack
- Attack state is **preserved** during movement
- States can now run **concurrently** instead of being mutually exclusive

## Files Modified

### `P_Controller.cs` - Major Refactor

#### 1. New Method: `TriggerAttack()`
```csharp
void TriggerAttack()
{
    if (currentWeapon == null) return;
    
    currentState = PState.Attack;
    attack.enabled = true;
    
    attackCooldown = c_Stats.attackCooldown;
    isAttacking = true;
    attack.Attack(currentWeapon, attackDir);
    currentWeapon = null;
}
```
- Triggers attack **without disabling other states**
- Enables attack component directly
- Sets attack tracking variables

#### 2. Modified: `ProcessInputs()`

**Attack Input Handling:**
```csharp
// OLD - Would switch state and return
if (input.Player.MeleeAttack.triggered)
{
    currentWeapon = meleeWeapon;
    SwitchState(PState.Attack);
    return; // ❌ Blocked movement processing
}

// NEW - Trigger attack and continue to movement
if (input.Player.MeleeAttack.triggered)
{
    currentWeapon = meleeWeapon;
    TriggerAttack();
    // ✅ Falls through to movement processing
}
```

**Movement Input Handling:**
```csharp
// Always enable movement when moving (even during attack)
if (!move.enabled) move.enabled = true;
move.SetMoveAxis(moveAxis);

// Update state tracking (but don't disable attack if attacking)
if (currentState != PState.Attack)
{
    currentState = PState.Move;
}
```

**Idle Handling:**
```csharp
// No movement input - disable movement state (but keep attack if attacking)
if (move.enabled) move.enabled = false;

// Update state to Idle only if not attacking
if (currentState != PState.Attack)
{
    SwitchState(PState.Idle);
}
```

#### 3. Modified: `SwitchState()`

**Conditional Attack Disabling:**
```csharp
// Disable all states first (except attack if it's active)
idle.enabled = false;
move.enabled = false;
dodge.enabled = false;

// Only disable attack if we're not currently attacking
if (!isAttacking) attack.enabled = false;
```

**Death Override:**
```csharp
case PState.Dead:
    // ... other death logic ...
    
    // Force disable attack on death
    attack.enabled = false;
    break;
```

**Empty Attack Case:**
```csharp
case PState.Attack:
    // Attack is handled separately via TriggerAttack()
    // This case shouldn't be called anymore
    break;
```

## State Behavior - Before vs After

### BEFORE (Broken):
```
Player clicks attack:
→ SwitchState(PState.Attack)
→ Disable all states (idle, move, attack, dodge)
→ Enable attack only
→ currentState = Attack

Player presses WASD:
→ SwitchState(PState.Move)
→ Disable all states (idle, move, attack, dodge) ❌ ATTACK DISABLED
→ Enable move only
→ Attack animation stops ❌
```

### AFTER (Fixed):
```
Player clicks attack:
→ TriggerAttack()
→ Enable attack (don't touch other states)
→ currentState = Attack
→ isAttacking = true

Player presses WASD:
→ Enable move (don't touch attack because isAttacking = true)
→ move.SetMoveAxis(moveAxis)
→ Both attack AND move are enabled ✅
→ Player moves at reduced speed while attacking ✅
```

## State Component Status Matrix

| Action | Idle | Move | Attack | Dodge | currentState |
|--------|------|------|--------|-------|--------------|
| Standing still | ✅ | ❌ | ❌ | ❌ | Idle |
| Moving | ❌ | ✅ | ❌ | ❌ | Move |
| Attack (standing) | ❌ | ❌ | ✅ | ❌ | Attack |
| **Attack + Move** | ❌ | ✅ | ✅ | ❌ | Attack |
| Dodging | ❌ | ❌ | ❌ | ✅ | Dodge |
| Dead | ❌ | ❌ | ❌ | ❌ | Dead |

## Animation Flow

### Attack While Standing:
1. Attack triggered → `isAttacking = true`
2. Animator: `isAttacking = true` (from `P_State_Attack.OnEnable`)
3. Attack animation plays
4. No movement input → `isMoving = false`
5. Result: **Pure attack animation**

### Attack While Moving:
1. Attack triggered → `isAttacking = true`, `attack.enabled = true`
2. Movement input → `move.enabled = true`, `isMoving = true`
3. Animator receives:
   - `isAttacking = true` (from attack state)
   - `isMoving = true` (from move state)
   - `moveX`, `moveY` values
4. Result: **Attack animation + movement values set**
   - Current setup: Movement animation plays (because `isMoving` overrides)
   - Future: Can use blend tree to show attack-walk animation

## Key Benefits
✅ Attack doesn't interrupt movement processing  
✅ Movement doesn't disable attack  
✅ Both states can run simultaneously  
✅ `isAttacking` flag prevents premature attack disable  
✅ Attack animation completes even while moving  
✅ Movement speed penalty still applies correctly  

## State Component Lifecycle

### Attack State:
```
Enable: When TriggerAttack() called
Disable: When attack completes (isAttacking = false) OR player dies
```

### Move State:
```
Enable: When movement input detected (WASD pressed)
Disable: When no movement input (WASD released)
Independent: Not affected by attack state
```

### Idle State:
```
Enable: When no movement AND not attacking
Disable: When movement starts OR attack starts
```

## Testing Checklist
- [x] Attack while standing still (should work as before)
- [x] Attack then start moving (should move at reduced speed, attack continues)
- [x] Move then attack (should slow down, keep moving, attack triggers)
- [x] Release movement during attack (should stop moving, attack continues)
- [x] Attack completes while moving (should return to full speed)
- [ ] Test animation (may show movement anim instead of attack - expected for now)

## Future Animation Enhancement
To show proper attack-while-walking animation:
1. Create blend tree in Animator
2. Blend between Attack and Walk based on `moveX/moveY` magnitude
3. Or create 4-directional attack-walk animations
4. Use `isAttacking && isMoving` condition in animator

## Notes
- `currentState` enum still tracks "primary" state (for logic)
- State components can now be enabled **concurrently**
- `isAttacking` flag is critical - prevents attack from being disabled during movement
- Death still force-disables everything (highest priority override)
- This is a more flexible state system than strict state machines
