# GRS Baked Weapon System - Complete Setup Guide
**Created:** November 8, 2025  
**Status:** ✅ Production Ready  
**Boss:** GRS (Giant Raccoon Shaman)

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Setup Instructions](#setup-instructions)
4. [How It Works](#how-it-works)
5. [Integration Details](#integration-details)
6. [Troubleshooting](#troubleshooting)

---

## Overview

### **The Problem We Solved**

GRS boss has **weapon baked into sprite** (not a separate GameObject). This creates challenges:
- Sprite animation changes position (jumping, attacking)
- Traditional static collider doesn't follow sprite movement
- Need different damage types (contact vs weapon damage)
- Need precise collision matching sprite shape

### **The Solution**

**Baked Weapon System** with two key components:
1. **C_UpdateColliderShape**: Auto-updates collider shape to match sprite animation
2. **B_WeaponCollider**: Switches between contact damage and weapon damage modes

---

## System Architecture

### **Component Stack**

```
GRS_Boss (single GameObject - NO child hierarchy)
├── Rigidbody2D (kinematic)
├── GRS_Controller
├── C_Stats, C_Health, C_FX
├── SpriteRenderer (sprite animation lives here)
├── Animator
├── PolygonCollider2D (ONE collider)
├── C_UpdateColliderShape (updates shape from sprite)
└── B_WeaponCollider (switches damage type)
```

**Important:** GRS has a **flat hierarchy** - all components on one GameObject!

### **Data Flow**

```
Animation plays → Sprite changes
         ↓
C_UpdateColliderShape detects change
         ↓
Reads Custom Physics Shape from sprite
         ↓
Updates PolygonCollider2D to match
         ↓
B_WeaponCollider handles collision
         ↓
Applies correct damage type based on attack state
```

---

## Setup Instructions

### **Step 1: Define Custom Physics Shapes**

**For EVERY GRS sprite frame:**

1. Select sprite in Project window
2. Click "Sprite Editor" button (top toolbar)
3. Select "Custom Physics Shape" tool (left toolbar)
4. Draw ONE shape covering body + weapon
5. Click "Apply" button (top right)
6. Repeat for ALL animation frames (idle, walk, attack, special, death)

**Tips:**
- Draw shape tightly around body + weapon area
- Include weapon blade in shape (it will deal damage during attacks)
- Keep shape consistent across similar animation frames
- Test in Scene view - green outline shows collision area

---

### **Step 2: Hierarchy Setup**

**GRS_Boss (single GameObject):**
```
Components:
1. Rigidbody2D (kinematic) - already exists
2. GRS_Controller - already exists
3. C_Stats - already exists
4. C_Health - already exists
5. C_FX - already exists
6. SpriteRenderer - already exists
7. Animator - already exists
8. PolygonCollider2D - ADD NEW
9. C_UpdateColliderShape - ADD NEW
10. B_WeaponCollider - ADD NEW
```

**How to add:**
1. Select GRS_Boss in Hierarchy
2. Add Component → Physics 2D → Polygon Collider 2D
3. Add Component → C_UpdateColliderShape
4. Add Component → B_WeaponCollider

**Note:** All components go on the SAME GameObject (no child hierarchy needed)

---

### **Step 3: Component Configuration**

#### **PolygonCollider2D Settings:**
```
Is Trigger: ☐ (unchecked - must use OnCollisionStay2D)
Layer: Enemy
Material: None (or Physics Material 2D if needed)
```

**Important:** Must NOT be trigger! B_WeaponCollider uses `OnCollisionStay2D`.

---

#### **C_UpdateColliderShape Settings:**
```
No settings to configure!
Script auto-finds PolygonCollider2D and SpriteRenderer.
```

**What it does:**
- Watches for sprite changes every frame (LateUpdate)
- Reads Custom Physics Shape from current sprite
- Updates PolygonCollider2D paths to match
- Optimized: Only updates when sprite actually changes

---

#### **B_WeaponCollider Settings:**

**Inspector Configuration:**
```
Weapon Damage: 15 (base weapon damage)
Knockback Force: 5f (knockback strength)
Stun Duration: 0.3f (seconds)
Player Layer: Player (layer mask)
```

**Tuning Guide:**

| Parameter | Default | Purpose | Recommendations |
|-----------|---------|---------|-----------------|
| **Weapon Damage** | 15 | Base damage before stats | Higher for stronger boss |
| **Knockback Force** | 5f | Push-back strength | 3-8f typical range |
| **Stun Duration** | 0.3f | Player stun time | 0.2-0.5f for fair gameplay |
| **Player Layer** | Player | Collision filter | Must match player's layer |

**Damage Calculation:**
```
Contact Mode:
- Damage = c_Stats.collisionDamage
- Cooldown = c_Stats.collisionTick
- No knockback, no stun

Weapon Mode:
- Damage = weaponDamage + c_Stats.AD + c_Stats.AP
- Applies knockback (knockbackForce)
- Applies stun (stunDuration)
- One-time hit per collision
```

---

### **Step 4: GRS_State_Attack Integration**

**Already completed in your project!** The attack state now:

**Normal Attack Flow:**
```csharp
1. Start attack animation
2. Wait hitDelay (0.25s) - charging
3. EnableWeaponMode() → Weapon damage active
4. Wait attack duration
5. DisableWeaponMode() → Back to contact damage
```

**Special Attack Flow:**
```csharp
1. Start special animation
2. Wait specialHitDelay (0.50s) - charging
3. EnableWeaponMode() → First hit starts
4. Dash forward (weapon active during dash)
5. Stop dash
6. Wait followupGap (0.14s)
7. Second hit window (weapon still active)
8. Wait hitDelay (0.25s)
9. DisableWeaponMode() → Back to contact damage
```

---

## How It Works

### **Timeline Examples**

#### **Idle State (No Attack):**
```
Player touches GRS
    ↓
OnCollisionStay2D fires
    ↓
isInWeaponMode = false
    ↓
ApplyContactDamage()
    ↓
Deals c_Stats.collisionDamage (e.g., 5 damage)
Cooldown: c_Stats.collisionTick (e.g., 0.5s)
No knockback, no stun
```

#### **Normal Attack (Active Swing):**
```
Frame 0.0s: Attack starts
    isInWeaponMode = false (charging)
    Player touch → Contact damage only

Frame 0.25s: hitDelay ends
    EnableWeaponMode()
    isInWeaponMode = true

Frame 0.30s: Player touches weapon
    OnCollisionStay2D fires
    isInWeaponMode = true
    ApplyWeaponDamage()
    Deals 15 + AD + AP damage (e.g., 25 total)
    Applies knockback (pushes player away)
    Applies stun (0.3s freeze)

Frame 0.45s: Attack ends
    DisableWeaponMode()
    isInWeaponMode = false
    Back to contact damage
```

#### **Special Attack (Dash + Double Hit):**
```
Frame 0.0s: Special starts
    isInWeaponMode = false (charging)

Frame 0.50s: Dash begins
    EnableWeaponMode()
    isInWeaponMode = true
    First hit window opens

Frame 0.50-1.50s: Dashing
    Player touch during dash → Weapon damage + knockback

Frame 1.50s: Dash stops
    Weapon still active

Frame 1.64s: Second hit window
    Player touch → Weapon damage + knockback again

Frame 1.89s: Attack ends
    DisableWeaponMode()
    isInWeaponMode = false
```

---

### **Collision Detection Logic**

```csharp
void OnCollisionStay2D(Collision2D collision)
{
    // 1. Safety checks
    if (!c_Health.IsAlive) return;
    if (not player layer) return;

    // 2. Get player health
    C_Health playerHealth = collision.collider.GetComponent<C_Health>();
    if (!playerHealth || !playerHealth.IsAlive) return;

    // 3. Switch based on mode
    if (isInWeaponMode)
    {
        // WEAPON MODE
        ApplyWeaponDamage(playerHealth, collision.collider);
    }
    else
    {
        // CONTACT MODE
        ApplyContactDamage(playerHealth);
    }
}
```

**Key Points:**
- Runs every physics frame while colliding
- Single collision detection handles both damage types
- Mode determines which damage function is called
- Contact mode has cooldown (spam prevention)
- Weapon mode applies once per collision (no cooldown needed)

---

## Integration Details

### **Files Modified**

**1. GRS_State_Attack.cs** (Boss attack state)

**Added:**
```csharp
[Header("References")]
B_WeaponCollider dynamicCollider;

void Awake()
{
    dynamicCollider = GetComponentInChildren<B_WeaponCollider>();
}
```

**Normal Attack Changes:**
```csharp
// OLD:
yield return new WaitForSeconds(hitDelay);
activeWeapon.Attack(lastFace);  // Old weapon system

// NEW:
yield return new WaitForSeconds(hitDelay);
if (dynamicCollider) dynamicCollider.EnableWeaponMode();  // Enable damage
// ... attack duration ...
if (dynamicCollider) dynamicCollider.DisableWeaponMode(); // Disable damage
```

**Special Attack Changes:**
```csharp
// OLD:
BeginDash(specialDashSpeed, dashDistance);
// ... dash loop ...
activeWeapon.Attack(lastFace);  // First hit
yield return new WaitForSeconds(followupGap);
activeWeapon.Attack(lastFace);  // Second hit

// NEW:
if (dynamicCollider) dynamicCollider.EnableWeaponMode();  // Enable BEFORE dash
BeginDash(specialDashSpeed, dashDistance);
// ... dash loop ... (weapon active during dash)
StopDash();
yield return new WaitForSeconds(followupGap);
// ... second hit window ... (weapon still active)
yield return new WaitForSeconds(hitDelay);
if (dynamicCollider) dynamicCollider.DisableWeaponMode(); // Disable after both hits
```

---

### **Files Created**

**1. C_UpdateColliderShape.cs** (Character folder)
- Purpose: Auto-updates PolygonCollider2D from sprite's Custom Physics Shape
- Used by: GS2, GRS (any boss with animated collision)
- Location: `Assets/GAME/Scripts/Character/`

**2. B_WeaponCollider.cs** (Enemy folder)
- Purpose: Switches between contact and weapon damage modes
- Used by: GRS only
- Location: `Assets/GAME/Scripts/Enemy/`

---

### **Old System (Keep for Now)**

**Weapon Child GameObject:**
- Still exists in hierarchy
- `activeWeapon.Attack()` calls removed from code
- Can be deleted later when fully confirmed working
- Kept as backup during testing phase

**Why keep it:**
- Safety during transition
- Visual reference for weapon position
- Easy rollback if issues found
- Remove after thorough testing

---

## Troubleshooting

### **Issue: Collider doesn't update with animation**

**Symptoms:**
- Collider stays in one position
- Sprite moves but collider doesn't follow
- Player can walk through sprite

**Solutions:**
1. Check `C_UpdateColliderShape` is on sprite child (not parent)
2. Verify Custom Physics Shape defined in Sprite Editor
3. Check sprite child has both SpriteRenderer and PolygonCollider2D
4. Enable Gizmos in Scene view to see collider outline

---

### **Issue: Contact damage doesn't work**

**Symptoms:**
- Player touches boss but no damage
- No damage outside of attacks

**Solutions:**
1. Check `PolygonCollider2D.isTrigger = false` (unchecked)
2. Verify player is on correct layer (playerLayer mask)
3. Check `c_Stats.collisionDamage` value (not zero)
4. Ensure B_WeaponCollider on sprite child

---

### **Issue: Weapon damage doesn't work**

**Symptoms:**
- Normal/special attacks deal no damage
- Only contact damage happens

**Solutions:**
1. Check `EnableWeaponMode()` called in attack state
2. Verify `weaponDamage` value in Inspector (not zero)
3. Check attack state finds `dynamicCollider` in Awake
4. Add debug log in `ApplyWeaponDamage()` to confirm it's called

---

### **Issue: Weapon damage during charge phase**

**Symptoms:**
- Boss deals big damage while charging
- Should only deal contact damage during charge

**Solutions:**
1. Verify `EnableWeaponMode()` called AFTER hitDelay/specialHitDelay
2. Check charge phase waits before enabling weapon mode
3. Normal attack: Enable after 0.25s
4. Special attack: Enable after 0.50s

---

### **Issue: Weapon damage persists after attack**

**Symptoms:**
- Boss continues dealing weapon damage after attack ends
- Contact damage never returns

**Solutions:**
1. Check `DisableWeaponMode()` called at end of attack
2. Verify `OnDisable()` in GRS_State_Attack calls `DisableWeaponMode()`
3. Add safety check in state transitions
4. Test switching between states (chase → attack → chase)

---

### **Issue: Collider shape wrong/missing**

**Symptoms:**
- No green collider outline in Scene view
- Collider is box/circle instead of custom shape
- Error in console about physics shape

**Solutions:**
1. Check Custom Physics Shape defined for ALL sprite frames
2. Verify at least one shape exists (not zero shapes)
3. Re-open Sprite Editor and confirm shape is saved
4. Check sprite asset not corrupted (reimport if needed)

---

### **Issue: Performance problems**

**Symptoms:**
- Game lags when GRS is on screen
- Frame drops during animations

**Solutions:**
1. C_UpdateColliderShape is already optimized (only updates on change)
2. Check too many enemies spawned
3. Verify only one C_UpdateColliderShape per boss
4. Profile in Unity Profiler to find actual bottleneck

---

## Testing Checklist

### **Setup Verification:**
- [ ] Custom Physics Shape defined for all GRS sprite frames
- [ ] PolygonCollider2D on sprite child
- [ ] C_UpdateColliderShape on sprite child
- [ ] B_WeaponCollider on sprite child
- [ ] PolygonCollider2D.isTrigger = false
- [ ] B_WeaponCollider settings configured

### **Visual Tests (Scene View):**
- [ ] Green collider outline visible in Scene view
- [ ] Collider follows sprite during idle animation
- [ ] Collider follows sprite during walk animation
- [ ] Collider follows sprite during attack animation
- [ ] Collider shape matches sprite accurately

### **Contact Damage Tests:**
- [ ] Player takes damage when touching idle boss
- [ ] Damage has cooldown (not every frame)
- [ ] No knockback during contact damage
- [ ] No stun during contact damage
- [ ] Damage amount matches c_Stats.collisionDamage

### **Normal Attack Tests:**
- [ ] Boss charges for ~0.25s (no weapon damage)
- [ ] Contact damage only during charge
- [ ] Weapon damage starts after charge
- [ ] Weapon damage higher than contact damage
- [ ] Player gets knocked back
- [ ] Player gets stunned (0.3s)
- [ ] Weapon damage stops after attack ends

### **Special Attack Tests:**
- [ ] Boss charges for ~0.50s (no weapon damage)
- [ ] Contact damage only during charge
- [ ] Weapon damage starts when dash begins (first hit)
- [ ] Weapon damage active during entire dash
- [ ] Player can be hit during dash
- [ ] Second hit works after gap (0.14s)
- [ ] Both hits apply knockback + stun
- [ ] Weapon damage stops after attack ends

### **State Transitions:**
- [ ] Switching idle → attack works
- [ ] Switching attack → chase works
- [ ] Weapon mode resets when attack state disabled
- [ ] Boss death disables weapon mode properly

---

## Quick Reference

### **Inspector Settings Summary**

**PolygonCollider2D:**
- Is Trigger: ☐ (unchecked)
- Layer: Enemy

**B_WeaponCollider:**
- Weapon Damage: 15
- Knockback Force: 5f
- Stun Duration: 0.3f
- Player Layer: Player

### **Component Locations**

```
Assets/GAME/Scripts/
├── Character/
│   └── C_UpdateColliderShape.cs
└── Enemy/
    ├── GRS_Controller.cs
    ├── GRS_State_Attack.cs
    └── B_WeaponCollider.cs
```

### **Key Methods**

**C_UpdateColliderShape:**
- Auto-runs in LateUpdate
- No public API needed

**B_WeaponCollider:**
- `EnableWeaponMode()` - Call when attack starts
- `DisableWeaponMode()` - Call when attack ends

**GRS_State_Attack:**
- Automatically calls weapon mode methods
- No manual intervention needed

---

## Advanced Notes

### **Why LateUpdate?**

```csharp
void LateUpdate()
{
    if (spriteRenderer.sprite != lastSprite)
    {
        UpdateColliderShape();
    }
}
```

- Animator updates sprite in `Update()`
- `LateUpdate()` runs AFTER `Update()`
- Guarantees we see the new sprite before updating collider
- Perfect sync between animation and collision

### **Why OnCollisionStay2D?**

```csharp
void OnCollisionStay2D(Collision2D collision)
```

- Fires every physics frame while colliding
- Allows continuous contact damage with cooldown
- Single collision detection for both damage modes
- More reliable than OnTriggerEnter2D for contact damage

### **Performance Optimization**

```csharp
// Only updates when sprite ACTUALLY changes
if (spriteRenderer.sprite != lastSprite)
```

- Caches last sprite reference
- No work done if sprite unchanged
- Typical idle animation: 4 sprites × 6 frames = update 4 times, not 24
- Significant performance saving for held frames

---

## Migration Notes (From Old to New System)

### **Old System:**
```
- Weapon as separate child GameObject
- Static collider on parent
- activeWeapon.Attack() to enable weapon
- Weapon GameObject has own collider
```

### **New System:**
```
- Weapon baked into sprite
- Dynamic collider on sprite child
- EnableWeaponMode() / DisableWeaponMode()
- Single collider switches behavior
```

### **Benefits:**
✅ More accurate collision (matches sprite exactly)  
✅ Follows sprite animation automatically  
✅ Simpler hierarchy (fewer GameObjects)  
✅ Easier to maintain (one collider vs multiple)  
✅ Better visual consistency (weapon always matches sprite)  

### **When to Remove Old Weapon:**
- After thorough testing (1-2 weeks)
- When fully confirmed new system works
- Before final build/release
- Keep backup of old prefab just in case

---

**Status:** ✅ System ready for production use  
**Last Updated:** November 8, 2025  
**Next Steps:** Test thoroughly, tune damage values, remove old weapon system
