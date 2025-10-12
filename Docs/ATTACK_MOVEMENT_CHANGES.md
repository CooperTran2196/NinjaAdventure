# Attack Movement System - Implementation Summary

## Feature Added
Players can now **move while attacking**, but with a **configurable speed penalty**. This creates a more fluid combat system similar to action RPGs like Hades or Diablo.

## Problem Solved
Previously:
- Attacking completely **locked** player movement (`desiredVelocity = Vector2.zero`)
- Input was **blocked** during attacks (`if (isAttacking) return`)
- Player felt "sticky" and unresponsive during combat

Now:
- Player can move freely during attacks
- Movement speed is **reduced by a percentage** while attacking
- More responsive, fluid combat feel

## Files Modified

### 1. `C_Stats.cs` - Added Attack Movement Penalty Setting
**Added:**
```csharp
[Range(0f, 1f)]
public float attackMovePenalty = 0.5f; // 0.5 = 50% speed while attacking
```
- Placed under `[Header("Combat")]`
- Range slider in Inspector (0% to 100%)
- Default value: `0.5` (50% movement speed during attacks)

**Examples:**
- `0.5` = Move at 50% speed while attacking (recommended)
- `0.7` = Move at 70% speed (less penalty)
- `0.3` = Move at 30% speed (heavy penalty)
- `1.0` = Full speed (no penalty)
- `0.0` = Can't move (old behavior)

### 2. `P_Controller.cs` - Removed Movement Locks
**Modified `ProcessInputs()`:**
- **Removed:** `if (currentState == PState.Attack && isAttacking) return;`
- **Changed:** Movement input now works during `PState.Attack`
- **Changed:** Added logic to switch to `PState.Move` while attacking if player moves
- **Changed:** Idle state won't override attack state

**Modified `SwitchState(PState.Attack)`:**
- **Removed:** `desiredVelocity = Vector2.zero;` (no longer zero out velocity)
- **Result:** Player can maintain movement momentum during attack

**Key Logic Changes:**
```csharp
// OLD: Blocked all input during attack
if (currentState == PState.Attack && isAttacking) return;

// NEW: Allow movement during attack
if (currentState == PState.Move || currentState == PState.Attack)
{
    move.SetMoveAxis(moveAxis);
    if (currentState != PState.Move)
    {
        SwitchState(PState.Move);
    }
}
```

### 3. `P_State_Movement.cs` - Applied Speed Penalty
**Modified `Update()`:**
```csharp
// Calculate base movement velocity
float speed = c_Stats.MS;

// Apply attack movement penalty if attacking
if (controller.currentState == P_Controller.PState.Attack)
{
    speed *= c_Stats.attackMovePenalty;
}

controller.SetDesiredVelocity(moveAxis * speed);
```

**How it works:**
1. Check if player is in `PState.Attack`
2. If yes, multiply movement speed by penalty (e.g., `5.0 * 0.5 = 2.5`)
3. Apply the reduced velocity

## State Behavior Changes

### Before:
```
Attack State:
- Movement input: BLOCKED ❌
- Velocity: ZERO ❌
- Can transition to: Nothing (locked until attack ends)
```

### After:
```
Attack State:
- Movement input: ALLOWED ✅
- Velocity: Reduced by penalty % ✅
- Can transition to: Move (while still attacking) ✅
- Attack animation: Still plays normally ✅
```

## State Transitions

### Attack + Movement Flow:
1. Player presses attack → `SwitchState(PState.Attack)`
2. Attack animation starts, `isAttacking = true`
3. Player presses WASD → Movement input processed
4. Controller switches to `PState.Move` (attack still active via `isAttacking` flag)
5. `P_State_Movement` detects `currentState == Attack` → applies penalty
6. Player moves at reduced speed while weapon thrust animation plays
7. Attack finishes → `isAttacking = false` → full speed restored

### Priority System (unchanged):
1. Death (highest)
2. Dodge
3. Attack trigger
4. Movement
5. Idle (lowest)

## Animation Considerations

The current system **doesn't blend** walk + attack animations. The player will:
- Show **movement animation** while moving during attack
- Weapon still thrusts/fires correctly (anchored to player)

If you want to show attack animation while moving, you'd need:
- Animation blend trees (Walk-Attack animations)
- Or keep `isAttacking` animation active while moving (requires animator changes)

## Benefits
✅ More fluid, responsive combat  
✅ Kiting enemies while attacking (like action RPGs)  
✅ No "stuck in place" feeling  
✅ Configurable penalty per character (different values for different classes)  
✅ Easy to tune gameplay feel via Inspector slider  

## Configuration Tips

### Melee Characters (close combat):
- `attackMovePenalty = 0.3 - 0.5` (30-50% speed)
- Heavier penalty encourages positioning

### Ranged Characters (kiting):
- `attackMovePenalty = 0.6 - 0.8` (60-80% speed)
- Lighter penalty allows mobile ranged combat

### Fast Assassin Characters:
- `attackMovePenalty = 0.7 - 0.9` (70-90% speed)
- Minimal penalty for hit-and-run playstyle

### Tank/Heavy Characters:
- `attackMovePenalty = 0.2 - 0.4` (20-40% speed)
- Heavy penalty fits slow, powerful attacks

## Testing Checklist
- [ ] Attack while standing still (should work as before)
- [ ] Attack while moving (should move at reduced speed)
- [ ] Release movement during attack (should stop moving)
- [ ] Start moving during attack (should start moving at penalty speed)
- [ ] Attack finishes while moving (should return to full speed)
- [ ] Verify weapon anchoring still works correctly
- [ ] Test with different penalty values (0.0, 0.5, 1.0)
- [ ] Check animation behavior (may need blend trees for polish)

## Future Enhancements
- [ ] Different penalties for melee vs ranged weapons
- [ ] Penalty affected by weapon weight/type
- [ ] Attack-walk blend animations
- [ ] Skills that modify attack movement penalty
- [ ] Status effects that increase/decrease penalty

## Notes
- The `P_State_Attack` component itself doesn't change - it just handles animation timing
- The penalty is applied in `P_State_Movement` which is the single source of truth for velocity
- Attack and Move states can now **coexist** (conceptually both active)
- `isAttacking` flag still tracks attack duration for animation/cooldown purposes
