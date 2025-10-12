# Weapon ShowTime Animation Lock System

## Feature Implemented
Attack animation now properly handles weapons with **showTime longer than the animation duration**. The animation plays once, freezes at the final frame, and holds until the weapon's full showTime completes.

## Problem Solved
**Before:**
- Attack animation duration: `0.45f` (fixed)
- Weapon showTime: `1.5f` (variable per weapon)
- Animation would loop or return to idle before weapon finished
- Movement penalty only applied during animation, not full showTime

**After:**
- Attack animation plays for `0.45f`
- Animation **freezes** at final frame
- Player locked in attack state for full `1.5f` (weapon showTime)
- Movement penalty applies for the **entire showTime**
- Smooth transition when showTime completes

## How It Works

### Timeline Example: Slow Weapon (showTime = 1.5f)

```
Time 0.00s: Attack triggered
  → isAttacking = true
  → Animation starts playing
  → Movement penalty active (50% speed)

Time 0.15s: Hit delay passes
  → activeWeapon.Attack(attackDir) called
  → Weapon visuals appear, thrust begins

Time 0.45s: Animation reaches final frame
  → Animation completes naturally
  → anim.speed = 0 (FREEZE at final frame)
  → Player still locked in attack state
  → Movement penalty still active

Time 0.45s - 1.5s: Lockout period (1.05s)
  → Animation frozen on last frame
  → Player can still move (at 50% speed)
  → isAttacking still true
  → Weapon thrust/effects still showing

Time 1.5s: Full showTime complete
  → anim.speed = 1 (restore animation speed)
  → controller.SetAttacking(false)
  → enabled = false → OnDisable()
  → isAttacking = false
  → Movement animation resumes (if moving)
```

### Timeline Example: Fast Weapon (showTime = 0.3f)

```
Time 0.00s: Attack triggered
  → Animation starts

Time 0.15s: Hit delay passes
  → Weapon attacks

Time 0.30s: ShowTime complete (before animation finishes)
  → No freeze needed
  → Wait until animation naturally finishes at 0.45s

Time 0.45s: Animation complete
  → Attack ends normally
  → No lockout period
```

## Code Changes

### `P_State_Attack.cs` - Complete Refactor

#### Renamed Variable for Clarity:
```csharp
// OLD
float attackDuration = 0.45f;

// NEW
float attackAnimDuration = 0.45f; // How long the attack animation actually is
```

#### New Attack Routine Logic:
```csharp
IEnumerator AttackRoutine()
{
    // Get weapon's showTime (how long the weapon is active/visible)
    float weaponShowTime = activeWeapon.weaponData.showTime;
    
    // Wait for hit delay, then trigger weapon attack
    yield return new WaitForSeconds(hitDelay);
    activeWeapon.Attack(attackDir);
    
    // Calculate how long until animation finishes
    float remainingAnimTime = attackAnimDuration - hitDelay;
    
    // If weapon showTime is longer than animation, we need to freeze and wait
    if (weaponShowTime > attackAnimDuration)
    {
        // Let animation play normally until it finishes
        yield return new WaitForSeconds(remainingAnimTime);
        
        // Freeze animation at final frame (speed = 0)
        anim.speed = 0f;
        
        // Wait for the remaining weapon showTime (lockout period)
        float lockoutTime = weaponShowTime - attackAnimDuration;
        yield return new WaitForSeconds(lockoutTime);
        
        // Restore animation speed
        anim.speed = 1f;
    }
    else
    {
        // Weapon showTime is shorter than or equal to animation
        // Just wait for the remaining animation time
        yield return new WaitForSeconds(remainingAnimTime);
    }

    controller.SetAttacking(false);
    enabled = false;
}
```

#### Enhanced OnDisable() Cleanup:
```csharp
void OnDisable()
{
    StopAllCoroutines();
    anim.SetBool("isAttacking", false);
    
    // Resume animation speed if it was frozen
    anim.speed = 1f; // ← NEW: Safety restore
    
    controller.SetAttacking(false);
    
    // Re-enable movement animation if player is moving
    var moveState = GetComponent<P_State_Movement>();
    if (moveState != null && moveState.enabled)
    {
        anim.SetBool("isMoving", true);
    }
}
```

## Behavior Matrix

| Weapon ShowTime | Animation Duration | Result |
|-----------------|-------------------|--------|
| 0.2f | 0.45f | Animation plays normally, ends when complete (0.45s) |
| 0.45f | 0.45f | Animation plays normally, ends exactly when complete |
| 1.0f | 0.45f | Animation plays (0.45s) → **Freezes** → Lockout (0.55s) → Total 1.0s |
| 1.5f | 0.45f | Animation plays (0.45s) → **Freezes** → Lockout (1.05s) → Total 1.5s |
| 2.0f | 0.45f | Animation plays (0.45s) → **Freezes** → Lockout (1.55s) → Total 2.0s |

## Animation Speed States

| Phase | anim.speed | isAttacking | Visual |
|-------|-----------|-------------|---------|
| Before attack | 1.0 | false | Idle/Walk |
| Animation playing | 1.0 | true | Attack animation progressing |
| Animation complete (if showTime > animDuration) | **0.0** | true | **Frozen on final frame** |
| Lockout period | **0.0** | true | **Frozen on final frame** |
| Attack complete | 1.0 | false | Resume Idle/Walk |

## Movement During Attack

The movement penalty (`attackMovePenalty`) applies for the **entire duration** of the attack, including the frozen lockout period:

```
Weapon showTime = 1.5f
Player moving during attack:

0.0s - 1.5s: Movement speed = MS × attackMovePenalty
  → If MS = 5.0 and penalty = 0.5
  → Speed = 2.5 during entire attack

1.5s+: Movement speed = MS (full speed)
  → Speed = 5.0 after attack completes
```

This is handled automatically by `P_State_Movement.Update()`:
```csharp
if (controller.currentState == P_Controller.PState.Attack)
{
    speed *= c_Stats.attackMovePenalty; // Applied throughout
}
```

## Example Weapon Configurations

### Fast Dagger (showTime < animation):
```
showTime = 0.3f
→ Animation plays normally
→ No freeze
→ Quick attacks, high DPS
```

### Balanced Sword (showTime ≈ animation):
```
showTime = 0.45f
→ Animation plays exactly to completion
→ No freeze
→ Standard attack feel
```

### Heavy Hammer (showTime > animation):
```
showTime = 1.2f
→ Animation plays (0.45s)
→ Freezes on final frame
→ Lockout period (0.75s)
→ Slow, powerful attacks
```

### Ultra Slow Greatsword (showTime >> animation):
```
showTime = 2.0f
→ Animation plays (0.45s)
→ Freezes on final frame
→ Long lockout period (1.55s)
→ Very slow, very powerful
```

## Integration with Existing Systems

### ✅ Works With: Attack Movement Penalty
- Penalty applies for full showTime
- Player can move during freeze, just slower
- Smooth transition back to full speed

### ✅ Works With: State Restoration
- When attack ends, proper state restored (Move or Idle)
- Movement animation resumes correctly
- `anim.speed` safely restored in OnDisable()

### ✅ Works With: Death/Interruption
- `OnDisable()` restores `anim.speed = 1f`
- No stuck frozen animations
- Coroutines stopped properly

### ✅ Works With: Weapon Anchoring
- Weapon still anchored to player during freeze
- Thrust animation completes normally
- Visual polish maintained

## Key Benefits

✅ **Weapon variety**: Different weapons feel different (fast vs slow)  
✅ **Visual clarity**: Animation always completes, never loops oddly  
✅ **Gameplay depth**: Heavy weapons = high risk/reward (long lockout)  
✅ **Movement during lockout**: Player can still reposition slowly  
✅ **Clean code**: One routine handles all cases  
✅ **Safe cleanup**: `anim.speed` always restored  
✅ **Existing systems work**: No breaking changes  

## Balancing Implications

### Fast Weapons (showTime < 0.5s):
- High attack speed
- Low damage per hit (balance with weapon AD/AP)
- Mobile playstyle
- Good for hit-and-run

### Medium Weapons (showTime ~0.5-1.0s):
- Balanced
- Standard damage
- Moderate risk/reward

### Slow Weapons (showTime > 1.0s):
- Low attack speed (long lockout)
- High damage per hit (balance with weapon AD/AP)
- Risky playstyle (vulnerable during freeze)
- Requires positioning skill

## Animation Freeze Caveats

### What Gets Frozen:
- ✅ Attack animation (visual pose held)
- ✅ Animator state time

### What Doesn't Get Frozen:
- ✅ Movement still works (at penalty speed)
- ✅ Weapon thrust/effects continue (they use coroutines)
- ✅ Physics updates (knockback, etc.)
- ✅ Game time

### Animator Speed = 0 Behavior:
- Animation **pauses** at current frame
- Does NOT reset or loop
- Stays on final frame until `speed = 1` restored
- Safe to use, Unity handles this well

## Testing Checklist

- [ ] Fast weapon (showTime 0.3s) - no freeze, smooth
- [ ] Balanced weapon (showTime 0.45s) - no freeze, ends exactly
- [ ] Slow weapon (showTime 1.0s) - freeze visible, lockout works
- [ ] Ultra slow weapon (showTime 2.0s) - long freeze, movement penalty applies
- [ ] Move during freeze - can move at reduced speed
- [ ] Attack while moving → freeze → keep moving → attack ends → full speed
- [ ] Death during freeze - anim.speed restored properly
- [ ] Multiple attacks in sequence with different weapons
- [ ] Weapon anchoring still works during freeze

## Future Enhancements

### Optional: Visual Feedback During Freeze
Add a subtle effect to show lockout period:
```csharp
// In lockout period
var sr = GetComponent<SpriteRenderer>();
StartCoroutine(FlashSprite(sr, lockoutTime)); // Subtle flash/pulse
```

### Optional: Cancel Lockout with Dodge
Allow dodge to interrupt freeze:
```csharp
// In P_Controller dodge handling
if (currentState == PState.Attack && isAttacking)
{
    attack.enabled = false; // Cancel attack
}
```

### Optional: Attack Speed Stat
Modify showTime based on character stats:
```csharp
float effectiveShowTime = weaponShowTime / c_Stats.attackSpeed;
```

## Notes

- `attackAnimDuration` should match your actual animation length in the Animator
- If animations change length, update this value
- Freeze only happens if `weaponShowTime > attackAnimDuration`
- `anim.speed = 0` is safe and commonly used for animation holds
- Safety restore in `OnDisable()` prevents stuck frozen states
- This pattern is used in many action games (Monster Hunter, Dark Souls, etc.)
