# Attack State Cleanup Fix

## Problem
`isAttacking` animator bool never turned off after attack finished because the `P_State_Attack` component was never disabled.

## Root Cause
In `P_State_Attack.AttackRoutine()`:
```csharp
// OLD - BAD
yield return new WaitForSeconds(attackDuration - hitDelay);
controller.SetAttacking(false); // Only sets flag, doesn't disable component
// Component stays enabled → OnDisable() never called → isAttacking stays true ❌
```

## What Was Happening
1. Attack triggered → `P_State_Attack.enabled = true`
2. `OnEnable()` called → `isAttacking = true` ✅
3. Attack routine runs and finishes
4. `controller.SetAttacking(false)` → internal flag set ✅
5. **But component stays enabled** ❌
6. `OnDisable()` never called ❌
7. `isAttacking` animator bool never set to false ❌
8. Attack animation stuck forever ❌

## The Fix
Added `enabled = false;` at the end of `AttackRoutine()`:

```csharp
// NEW - FIXED
IEnumerator AttackRoutine()
{
    yield return new WaitForSeconds(hitDelay);
    activeWeapon.Attack(attackDir);
    yield return new WaitForSeconds(attackDuration - hitDelay);

    controller.SetAttacking(false);
    
    // Disable this state component to trigger OnDisable() and clean up animation
    enabled = false;
}
```

## Complete Flow Now

### Attack Start:
```
1. TriggerAttack() called
2. attack.enabled = true
3. OnEnable() → isAttacking = true (animator)
4. AttackRoutine() starts
```

### Attack Running:
```
5. Wait hitDelay
6. Weapon.Attack() called
7. Wait remaining duration
```

### Attack End:
```
8. controller.SetAttacking(false) → isAttacking flag = false
9. enabled = false → Component disabled
10. OnDisable() triggered
11. isAttacking = false (animator) ✅
12. Check if moving → restore isMoving if needed ✅
```

## State Component Lifecycle

**Before Fix:**
```
Attack component: enabled → enabled → enabled (stuck) ❌
Animator bool: true → true → true (stuck) ❌
```

**After Fix:**
```
Attack component: enabled → enabled → disabled ✅
Animator bool: true → true → false ✅
```

## Why This Works
Unity's MonoBehaviour lifecycle:
- `enabled = true` → `OnEnable()` is called
- `enabled = false` → `OnDisable()` is called
- We leverage this to automatically clean up animation state

## Files Modified
- `P_State_Attack.cs` - Added `enabled = false;` at end of `AttackRoutine()`

## Benefits
✅ Attack animation properly ends  
✅ `isAttacking` bool correctly resets to false  
✅ Movement animation resumes after attack  
✅ Clean, automatic state cleanup  
✅ Consistent with Unity lifecycle patterns  

## Testing
- [x] Attack once → animation should complete and stop ✅
- [x] Attack while moving → animation should complete, return to walk ✅
- [x] Multiple attacks in sequence → each should complete properly ✅
- [x] Attack interrupted by death → OnDisable() handles cleanup ✅
