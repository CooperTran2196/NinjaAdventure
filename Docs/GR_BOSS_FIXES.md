# Giant Raccoon Boss Fixes - November 8, 2024

## Issues Fixed

### 1. Multiple Damage Per Attack
**Problem:** Boss weapon was dealing damage multiple times during a single attack.

**Root Cause:** `OnCollisionStay2D` fires every physics frame while colliding. Since the boss collides with the player throughout the entire dash, damage was applied repeatedly.

**Solution:** Added `hasDealtAttackDamage` flag to `B_WeaponCollider`:
- Flag is `false` when weapon mode is enabled (attack starts)
- First collision sets flag to `true` and deals damage
- Subsequent collisions during same attack are ignored
- Flag resets when weapon mode is disabled (attack ends)

**Code Changes:**
```csharp
// B_WeaponCollider.cs
bool hasDealtAttackDamage; // Added to runtime state

public void EnableWeaponMode()
{
    isInAttackMode = true;
    hasDealtAttackDamage = false; // Reset for new attack
}

public void DisableWeaponMode()
{
    isInAttackMode = false;
    hasDealtAttackDamage = false; // Reset for next attack
}

void ApplyAttackDamage(C_Health playerHealth, Collider2D playerCollider)
{
    // Only deal damage once per attack cycle
    if (hasDealtAttackDamage) return;
    hasDealtAttackDamage = true; // Mark damage as dealt
    
    // ... rest of damage logic
}
```

---

### 2. Player Getting Stuck Inside Boss
**Problem:** During special attack (jump), player sprite would get swallowed inside the boss sprite.

**Root Causes:**
1. **Duplicate Collision Systems:** `GR_Controller` had its own `OnCollisionStay2D` for contact damage, which was redundant with `B_WeaponCollider`
2. **Large Collider Overlap:** Boss's `PolygonCollider2D` is large and can trap player during jump landing

**Solution:** Removed duplicate collision damage system from `GR_Controller`:
- Deleted `contactTimer` field (unused now)
- Removed entire `OnCollisionStay2D` method
- Added note that collision damage is now handled by `B_WeaponCollider` on Sprite child

**Code Changes:**
```csharp
// GR_Controller.cs - REMOVED
float contactTimer; // Collision damage cooldown

void OnCollisionStay2D(Collision2D collision)
{
    // ... entire method removed
}

// ADDED NOTE
// NOTE: Collision damage now handled by B_WeaponCollider on Sprite child
```

**Why This Helps:**
- Eliminates conflicting collision handlers
- `B_WeaponCollider` is universal solution for all bosses
- Cleaner separation of concerns: Controller manages AI, WeaponCollider manages damage

---

### 3. Prefab Structure Verification
**Confirmed Structure:**
```
GiantRacoon (Parent GameObject)
├── Rigidbody2D
├── Animator ⬅️ Lives on PARENT, not Sprite child
├── C_Stats (AD: 10, collisionDamage: 5)
├── C_Health
├── C_FX
├── GR_Controller
├── GR_State_Attack (enabled: false)
├── GR_State_Chase (enabled: false)
├── State_Idle (enabled: false)
├── State_Wander (enabled: false)
└── C_AfterimageSpawner

Sprite (Child GameObject)
├── SpriteRenderer
├── PolygonCollider2D
├── C_UpdateColliderShape
└── B_WeaponCollider
    ├── knockbackForce: 5
    ├── stunDuration: 0.3
    └── playerLayer: Player (layer 6)
```

**Code Correctly Handles This:**
- `GR_State_Attack.cs` uses `GetComponent<Animator>()` on parent (self)
- `B_WeaponCollider.cs` uses `GetComponentInParent<>()` to find parent components
- `GR_State_Attack.cs` uses `GetComponentInChildren<B_WeaponCollider>()` to find Sprite child component

---

## Files Modified

### 1. B_WeaponCollider.cs
**Location:** `Assets/GAME/Scripts/Enemy/B_WeaponCollider.cs`

**Changes:**
- Added `bool hasDealtAttackDamage` to runtime state
- Modified `EnableWeaponMode()` to reset damage flag
- Modified `DisableWeaponMode()` to reset damage flag
- Modified `ApplyAttackDamage()` to check and set damage flag

**Impact:** Universal fix for all bosses using baked weapon system (GR, GRS, GS2)

---

### 2. GR_Controller.cs
**Location:** `Assets/GAME/Scripts/Enemy/GR_Controller.cs`

**Changes:**
- Removed `float contactTimer` field
- Removed entire `OnCollisionStay2D()` method (26 lines)
- Added note explaining collision damage is handled by `B_WeaponCollider`

**Impact:** Eliminates duplicate collision system, prevents conflicts

---

## Testing Checklist

### Normal Attack (Charge Dash)
- [ ] Boss charges toward player
- [ ] Damage dealt exactly **once** during dash
- [ ] Damage amount matches `c_Stats.AD` (10 in prefab)
- [ ] Knockback applied (force: 5)
- [ ] Stun applied (duration: 0.3s)
- [ ] Player not stuck inside boss after hit

### Special Attack (Jump AoE)
- [ ] Boss jumps toward player
- [ ] Landing AoE damage separate from collision damage
- [ ] Player not trapped inside boss sprite during/after jump
- [ ] Boss returns to chase after attack completes

### Contact Damage (Not Attacking)
- [ ] Boss deals small collision damage (5) when touching player
- [ ] Cooldown timer works (1s between hits per `c_Stats.collisionTick`)
- [ ] No knockback or stun (contact mode)

### Death Sequence
- [ ] Boss stops attacking immediately
- [ ] All states disabled
- [ ] No errors in console during death animation
- [ ] `B_WeaponCollider.DisableWeaponMode()` called in cleanup

---

## Technical Notes

### Damage Flow (Normal Attack)
```
1. GR_State_Attack.AttackRoutine() starts
2. Charge phase (2s)
3. BeginDash() called
4. EnableWeaponMode() called ⬅️ Resets hasDealtAttackDamage = false
5. Boss dashes toward player
6. OnCollisionStay2D detects player
7. ApplyAttackDamage() called:
   - Checks hasDealtAttackDamage (false)
   - Sets hasDealtAttackDamage = true
   - Deals damage once
8. Subsequent collision frames ignored (flag is true)
9. StopDash() called
10. DisableWeaponMode() called ⬅️ Resets flag for next attack
```

### Why OnCollisionStay2D Fires Multiple Times
- Physics engine updates at fixed timestep (default 50 FPS)
- Each frame boss collides with player, `OnCollisionStay2D` fires
- During 1-second dash at 9 m/s, approximately 50 collision events occur
- Without flag, 50 damage applications would happen!

### Universal Boss System
This fix applies to **all bosses** using `B_WeaponCollider`:
- **Giant Raccoon (GR):** Charge dash attack
- **Giant Red Samurai (GRS):** Melee slash attack
- **Giant Slime v2 (GS2):** Charge attack

All share same damage-once-per-attack logic now.

---

## Future Improvements (Optional)

### If Player Still Gets Stuck
If player getting swallowed during jump attack persists:

**Option 1: Reduce Collider During Jump**
```csharp
// In GR_State_Attack.cs, during special attack jump
spriteCollider.enabled = false; // Disable during jump
// ... jump animation
spriteCollider.enabled = true;  // Re-enable at landing
```

**Option 2: Push Player Out**
```csharp
// After landing, check if player is inside boss bounds
Vector2 bossToPlayer = playerPos - bossPos;
if (bossToPlayer.magnitude < 1f) // Too close
{
    // Push player out
    Vector2 pushDir = bossToPlayer.normalized;
    playerController.SetKnockback(pushDir * 3f);
}
```

**Option 3: Use Physics2D.IgnoreCollision**
```csharp
// Temporarily ignore collision during jump
Physics2D.IgnoreCollision(bossCollider, playerCollider, true);
yield return new WaitForSeconds(jumpDuration);
Physics2D.IgnoreCollision(bossCollider, playerCollider, false);
```

---

## Related Documentation

- **B_WeaponCollider System:** `Docs/Week_11_Nov4-8/GRS_BAKED_WEAPON_SYSTEM.md`
- **Custom Physics Shapes:** `Docs/Week_11_Nov4-8/CUSTOM_PHYSICS_SHAPE_SYSTEM.md`
- **Coding Style Guide:** `Docs/Guild/CODING_STYLE_GUIDE.md` v3.2
- **Boss Architecture:** `Docs/copilot-instructions.md` (Combat sequence section)

---

## Summary

✅ **Fixed:** Attack damage only happens once per attack cycle  
✅ **Fixed:** Removed duplicate collision system from GR_Controller  
✅ **Verified:** Prefab structure matches expected hierarchy (Animator on parent)  
⏳ **Testing:** Need to verify player no longer gets stuck during jump attack

Both issues stemmed from collision handling:
1. Multiple damage = `OnCollisionStay2D` firing every frame without protection
2. Player swallowing = Conflicting collision handlers + large overlapping colliders

Universal `B_WeaponCollider` system now cleanly handles both contact and attack damage for all bosses.
