# Animation Priority System - Attack Over Movement

## Problem Fixed
When player was attacking while moving, BOTH `isMoving` and `isAttacking` bools were set to `true`, causing animation conflicts. The animator didn't know which animation to prioritize.

## Root Cause
- `P_State_Movement` always set `isMoving = true` in `OnEnable()`
- `P_State_Attack` set `isAttacking = true` in `OnEnable()`
- When both states were enabled simultaneously → both bools = true → animation conflict

## Solution Overview
Implement **animation priority**: Attack animation always takes precedence over movement animation.

### Priority Hierarchy:
1. **Attack** (highest) - `isAttacking = true`, `isMoving = false`
2. **Movement** - `isMoving = true`, `isAttacking = false`
3. **Idle** (lowest) - both false

## Files Modified

### 1. `P_State_Movement.cs` - Animation Priority Logic

#### Modified `OnEnable()`:
```csharp
void OnEnable()
{
    // Only set isMoving if not attacking (attack animation takes priority)
    if (controller.currentState != P_Controller.PState.Attack)
    {
        anim.SetBool("isMoving", true);
    }
}
```
- Checks if player is attacking before enabling movement animation
- Prevents movement animation from overriding attack animation on enable

#### Modified `Update()`:
```csharp
void Update()
{
    // Calculate base movement velocity
    float speed = c_Stats.MS;
    
    // Apply attack movement penalty if attacking
    if (controller.currentState == P_Controller.PState.Attack)
    {
        speed *= c_Stats.attackMovePenalty;
        
        // Turn off isMoving animation while attacking (attack animation has priority)
        anim.SetBool("isMoving", false);
    }
    else
    {
        // Normal movement - show movement animation
        anim.SetBool("isMoving", true);
    }
    
    controller.SetDesiredVelocity(moveAxis * speed);

    // Set movement animation parameters (always update for directional info)
    anim.SetFloat("moveX", moveAxis.x);
    anim.SetFloat("moveY", moveAxis.y);
}
```

**Key Points:**
- Every frame checks if attacking
- If attacking: `isMoving = false` (even though player is moving)
- If not attacking: `isMoving = true`
- Movement parameters (`moveX`, `moveY`) still update (for future blend trees)

### 2. `P_State_Attack.cs` - Restore Movement Animation After Attack

#### Modified `OnDisable()`:
```csharp
void OnDisable()
{
    StopAllCoroutines();
    anim.SetBool("isAttacking", false);
    controller.SetAttacking(false);
    
    // Re-enable movement animation if player is moving
    var moveState = GetComponent<P_State_Movement>();
    if (moveState != null && moveState.enabled)
    {
        anim.SetBool("isMoving", true);
    }
}
```

**Key Points:**
- When attack finishes, check if movement state is still enabled
- If player is still holding WASD → re-enable `isMoving = true`
- Smooth transition back to movement animation

## Animation State Flow

### Scenario 1: Attack While Standing
```
State: Attack only
- isAttacking = true
- isMoving = false
Result: Attack animation plays ✅
```

### Scenario 2: Attack While Moving
```
State: Attack + Move (both enabled)
- isAttacking = true
- isMoving = false (forced off by P_State_Movement)
Result: Attack animation plays while player moves at reduced speed ✅
```

### Scenario 3: Attack Finishes, Still Moving
```
State: Move only (attack disabled)
- P_State_Attack.OnDisable() called
- Checks if P_State_Movement is enabled
- If yes: Set isMoving = true
Result: Smooth transition to movement animation ✅
```

### Scenario 4: Attack Finishes, Stopped Moving
```
State: Idle
- P_State_Attack.OnDisable() called
- P_State_Movement is disabled
- isMoving stays false
Result: Returns to idle animation ✅
```

## Timeline Example: Attack → Move → Attack Ends

```
Frame 1: Player clicks attack
  → P_State_Attack enabled
  → isAttacking = true
  → Attack animation starts

Frame 5: Player presses WASD (during attack)
  → P_State_Movement enabled
  → P_State_Movement.OnEnable() checks: currentState == Attack? Yes!
  → isMoving stays false (not set to true)
  → Player starts moving at reduced speed
  → Attack animation continues playing ✅

Frame 10-50: Player holds WASD (during attack)
  → P_State_Movement.Update() every frame:
    → Checks: currentState == Attack? Yes!
    → Sets isMoving = false
    → Attack animation keeps playing ✅
  → Player moves at reduced speed

Frame 60: Attack finishes
  → P_State_Attack.OnDisable() called
  → isAttacking = false
  → Checks: P_State_Movement enabled? Yes!
  → Sets isMoving = true
  → Movement animation starts ✅

Frame 61+: Player still holding WASD
  → P_State_Movement.Update():
    → Checks: currentState == Attack? No!
    → Sets isMoving = true (redundant but safe)
    → Movement animation playing ✅
  → Player moves at full speed
```

## Animator Bool States Matrix

| Scenario | isAttacking | isMoving | Animation Playing | Movement Speed |
|----------|-------------|----------|-------------------|----------------|
| Idle | false | false | Idle | 0 |
| Moving | false | true | Walk/Run | 100% (MS) |
| Attack (standing) | true | false | Attack | 0 |
| **Attack (moving)** | true | **false** | **Attack** | **50%** (MS × penalty) |
| After attack (still moving) | false | true | Walk/Run | 100% (MS) |

## Key Benefits
✅ Attack animation always has priority  
✅ No animation conflicts when both states active  
✅ Smooth transition back to movement animation after attack  
✅ Player can still move during attack (gameplay)  
✅ Attack animation plays during movement (visual)  
✅ Movement parameters still update (ready for blend trees)  

## Movement Parameters Still Updated
Even though `isMoving = false` during attacks, the movement parameters are still updated:
```csharp
anim.SetFloat("moveX", moveAxis.x);
anim.SetFloat("moveY", moveAxis.y);
```

This is useful for:
- **Blend trees**: Can blend attack direction with movement direction in the future
- **Directional attacks**: Attack animation can use movement direction
- **State persistence**: When attack ends, animator already has correct movement direction

## Future Enhancement: Attack-Walk Blend Tree
If you want to show attack animation blended with walking:

1. **In Animator:**
   - Create blend tree
   - Use `isAttacking && moveX/moveY != 0` condition
   - Blend between attack-idle and attack-walk animations

2. **In Code:**
   - Remove `isMoving = false` during attack
   - Let both bools be true
   - Animator handles blending via blend tree

But for now, **clean attack animation priority works perfectly!**

## Testing Results
- [x] Attack while standing → Attack animation plays ✅
- [x] Attack while moving → Attack animation plays, player moves slowly ✅
- [x] Attack finishes while moving → Returns to walk animation ✅
- [x] Attack finishes while stopped → Returns to idle animation ✅
- [x] No animation flickering or conflicts ✅

## Notes
- This is a **priority-based** animation system, not a blend system
- Attack always wins when both states are active
- Gameplay (movement) and visuals (animation) are now properly separated
- Clean, deterministic animation behavior
