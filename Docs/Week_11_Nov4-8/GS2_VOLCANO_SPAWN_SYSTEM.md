# GS2 Volcano Spawn System - Complete Setup Guide
**Created:** November 8, 2025  
**Status:** ✅ Production Ready  
**Boss:** GS2 (Giant Slime Phase 2)

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Setup Instructions](#setup-instructions)
4. [How It Works](#how-it-works)
5. [Tuning Guide](#tuning-guide)
6. [Troubleshooting](#troubleshooting)

---

## Overview

### **What is Volcano Spawn?**

GS2 boss performs **jump/slam attack** that:
1. Plays special animation (2.5s jump + land)
2. **Spawns enemies AFTER animation completes**
3. Enemies spawn on **circle perimeter** (not center)
4. Enemies **fly outward** using knockback physics
5. Creates "volcanic eruption" visual effect

### **The Problem We Solved**

**Original Issues:**
- ❌ Enemies spawned at exact same position (GS2 center)
- ❌ All enemies overlapped/collided with each other
- ❌ Setting `rb.linearVelocity` directly didn't work
- ❌ Random spawn count was unpredictable

**Solutions:**
- ✅ Circle perimeter spawn (minSpawnRadius)
- ✅ Knockback system integration (E_Controller.SetKnockback)
- ✅ Exact spawn counts (removed randomness)
- ✅ Proper animation timing (spawn AFTER slam)

---

## System Architecture

### **Component Stack**

```
GS2_Boss (parent)
├── Rigidbody2D (kinematic)
├── GS2_Controller
├── C_Stats, C_Health, C_FX
│
└── Sprite (child - animated sprite)
    ├── SpriteRenderer
    ├── Animator
    ├── PolygonCollider2D
    └── C_UpdateColliderShape (collision accuracy)
```

### **State Machine Flow**

```
Phase 1 (100% → 20% HP):
├── Idle
├── Wander
├── Chase
└── Attack (periodic spawns: normalSpawnCount)

Phase 2 (<20% HP):
├── No more chase
├── Retreat cycle (run away, stop, special)
└── Special attack spawns: emergencySpawnCount
```

### **Special Attack Timeline**

```
Frame 0.0s: Special attack starts
    - Trigger "Special" animation
    - Set speed = 0
    - Wait specialIdleDelay (1.0s)

Frame 1.0s: Animation plays
    - Jump/slam animation begins
    - Wait specialAnimLength (2.5s)

Frame 3.5s: Spawn enemies
    - Animation completed
    - Call SpawnEnemies()
    - Create enemies on circle
    - Apply knockback to each

Frame 3.5s+: Resume behavior
    - Back to chase (Phase 1)
    - OR back to retreat cycle (Phase 2)
```

---

## Setup Instructions

### **Step 1: GS2 Inspector Configuration**

**Required Components (Already Exist):**
- GS2_Controller
- C_Stats
- C_Health
- C_FX
- SpriteRenderer (on child)
- Animator (on child)

**GS2_Controller Settings:**

**[Spawn Settings]**
```
Normal Spawn Count: 2 (Phase 1)
Emergency Spawn Count: 5 (Phase 2)
Min Spawn Radius: 0.8 (circle edge distance)
Launch Speed: 4.0 (knockback force)
Spawn Delay: 8.0 (seconds between spawns)
```

**[Special Attack Settings]**
```
Special Idle Delay: 1.0 (pre-animation wait)
Special Anim Length: 2.5 (jump/slam duration)
```

**[Phase 2 Settings]**
```
Phase 2 HP Threshold: 0.2 (20% HP)
Retreat Duration: 3.0 (run away time)
Stop Duration: 1.5 (pause before special)
```

---

### **Step 2: Enemy Prefab Setup**

**Enemy Requirements:**
- Must have `E_Controller` component
- Must have `SetKnockback(Vector2)` method
- Must have `c_Stats` with `KR` (knockback recovery) stat

**Example Enemy Stats:**
```
Knockback Recovery (KR): 0.8 (higher = slows faster)
Base Speed: 2.0
Collision Damage: 5
```

**Enemy Prefab References:**
- Assign enemy prefabs to GS2_Controller's spawn list
- Supports multiple enemy types (randomly selected)
- Make sure enemies have proper collision layers

---

### **Step 3: Animation Setup**

**Required Animations:**
- `Idle` - Default state
- `Wander` - Walking animation
- `Chase` - Running animation (faster)
- `Special` - Jump/slam animation (2.5s duration)

**Animator Parameters:**
```
isWandering (bool) - Triggers wander animation
isAttacking (bool) - Currently unused (spawns via code)
Special (Trigger) - Plays special animation
```

**Special Animation Requirements:**
- **Must be exactly 2.5 seconds long** (or update `specialAnimLength`)
- Should show jump → peak → slam landing
- Land impact frame should be at end

---

## How It Works

### **Phase System**

#### **Phase 1 (Normal - 100% → 20% HP)**

```csharp
Behavior:
- Chase player normally
- Periodic spawns every spawnDelay seconds
- Spawns normalSpawnCount enemies (default: 2)
- Can enter special attack randomly

Phase Transition:
- Monitors HP in Update()
- Calls CheckPhaseTransition() constantly
- When HP ≤ 20%: Switch to Phase 2
```

#### **Phase 2 (Emergency - Below 20% HP)**

```csharp
Behavior:
- NO MORE CHASE (stops pursuing player)
- Retreat cycle loop:
  1. Run away from player (retreatDuration = 3.0s)
  2. Stop movement (stopDuration = 1.5s)
  3. Perform special attack (spawns emergencySpawnCount)
  4. Repeat forever

Phase Details:
- Spawns emergencySpawnCount enemies (default: 5)
- More aggressive spawn frequency
- Cannot exit Phase 2 (no HP threshold to revert)
```

**Key Code:**
```csharp
void CheckPhaseTransition()
{
    float hpPercent = (float)c_Health.CurrentHealth / c_Health.MaxHealth;
    
    if (!isPhase2 && hpPercent <= phase2HPThreshold)
    {
        isPhase2 = true;
        SwitchState(E_Controller.EState.Idle);
        // Will enter retreat cycle on next Update
    }
}
```

---

### **Spawn System Deep Dive**

#### **Circle Perimeter Spawn**

```csharp
void SpawnEnemies()
{
    Vector2 center = transform.position;
    int spawnCount = isPhase2 ? emergencySpawnCount : normalSpawnCount;
    
    for (int i = 0; i < spawnCount; i++)
    {
        // 1. Calculate random angle
        float angle = Random.Range(0f, 360f);
        Vector2 direction = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;
        
        // 2. Spawn on circle edge (not center)
        Vector2 spawnOffset = direction * minSpawnRadius;
        Vector2 spawnPos = center + spawnOffset;
        
        // 3. Instantiate enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        // 4. Apply knockback (launch outward)
        E_Controller enemyController = enemy.GetComponent<E_Controller>();
        if (enemyController)
        {
            enemyController.SetKnockback(direction * launchSpeed);
        }
    }
}
```

**Key Points:**
- `angle = Random.Range(0f, 360f)` - Full circle coverage
- `spawnOffset = direction * minSpawnRadius` - Edge spawn, not center
- `SetKnockback(direction * launchSpeed)` - Launch outward using knockback system
- Enemies spawn in circle formation, fly outward radially

---

#### **Why Knockback System?**

**Problem with Direct Velocity:**
```csharp
// ❌ DOESN'T WORK:
rb.linearVelocity = direction * launchSpeed;

// Why? E_Controller.FixedUpdate() does this EVERY frame:
void FixedUpdate()
{
    rb.linearVelocity = desiredVelocity + knockback;
    // ↑ Overwrites any external velocity changes!
}
```

**Solution using Knockback:**
```csharp
// ✅ WORKS:
enemyController.SetKnockback(direction * launchSpeed);

// Why? Knockback is ADDED to desiredVelocity:
void FixedUpdate()
{
    rb.linearVelocity = desiredVelocity + knockback;
    // ↑ Knockback preserved, naturally decays via KR stat
}

public void SetKnockback(Vector2 force)
{
    knockback = force; // Stored and applied each frame
}
```

**Knockback Decay:**
```csharp
void FixedUpdate()
{
    // Gradually reduce knockback each frame
    knockback = Vector2.Lerp(knockback, Vector2.zero, c_Stats.KR * Time.fixedDeltaTime);
    
    // Apply movement
    rb.linearVelocity = desiredVelocity + knockback;
}
```

**Result:**
- Enemy launches at `launchSpeed` (e.g., 4.0)
- Smoothly decelerates over time
- Eventually reaches zero velocity
- Natural physics behavior

---

### **Special Attack Execution**

```csharp
IEnumerator SpecialRoutine()
{
    // 1. Pre-animation wait
    yield return new WaitForSeconds(specialIdleDelay); // 1.0s
    
    // 2. Play animation
    gfxAnimator.SetTrigger("Special");
    
    // 3. Wait for animation to complete
    yield return new WaitForSeconds(specialAnimLength); // 2.5s
    
    // 4. Spawn enemies AFTER animation
    SpawnEnemies();
    
    // 5. Reset state
    isSpecialAttacking = false;
    SwitchState(E_Controller.EState.Idle);
}
```

**Timing Breakdown:**
- `specialIdleDelay = 1.0s` - Boss pauses before jump
- `specialAnimLength = 2.5s` - Jump/slam animation plays
- `SpawnEnemies()` - Called at end (frame 3.5s)
- Total duration: 3.5 seconds

**Why Wait for Animation?**

Original behavior spawned enemies at START:
```csharp
// ❌ OLD:
gfxAnimator.SetTrigger("Special");
SpawnEnemies(); // Spawn immediately
yield return new WaitForSeconds(specialAnimLength);
```

**Problem:** Enemies appear before slam lands (looks unnatural)

New behavior spawns enemies at END:
```csharp
// ✅ NEW:
gfxAnimator.SetTrigger("Special");
yield return new WaitForSeconds(specialAnimLength);
SpawnEnemies(); // Spawn after slam
```

**Result:** Enemies erupt when slam hits ground (volcanic effect)

---

## Tuning Guide

### **Spawn Parameters**

| Parameter | Default | Purpose | Recommendations |
|-----------|---------|---------|-----------------|
| **normalSpawnCount** | 2 | Phase 1 spawn count | 1-3 for manageable challenge |
| **emergencySpawnCount** | 5 | Phase 2 spawn count | 3-7 for final push difficulty |
| **minSpawnRadius** | 0.8 | Circle edge distance | 0.5-1.5 based on enemy size |
| **launchSpeed** | 4.0 | Knockback force | 3-6 for natural spread |
| **spawnDelay** | 8.0 | Seconds between spawns | 5-15 based on difficulty |

### **Special Attack Timing**

| Parameter | Default | Purpose | Recommendations |
|-----------|---------|---------|-----------------|
| **specialIdleDelay** | 1.0 | Pre-animation pause | 0.5-1.5 for telegraph |
| **specialAnimLength** | 2.5 | Jump/slam duration | Match animation length exactly |

### **Phase 2 Behavior**

| Parameter | Default | Purpose | Recommendations |
|-----------|---------|---------|-----------------|
| **phase2HPThreshold** | 0.2 | Trigger HP (20%) | 0.1-0.3 based on difficulty curve |
| **retreatDuration** | 3.0 | Run away time | 2-5 seconds |
| **stopDuration** | 1.5 | Pause before special | 1-3 seconds |

---

### **Balance Tips**

#### **Easy Boss (Beginner Friendly):**
```
normalSpawnCount: 1
emergencySpawnCount: 3
launchSpeed: 3.0
spawnDelay: 10.0
phase2HPThreshold: 0.25 (25%)
```

#### **Medium Boss (Default):**
```
normalSpawnCount: 2
emergencySpawnCount: 5
launchSpeed: 4.0
spawnDelay: 8.0
phase2HPThreshold: 0.2 (20%)
```

#### **Hard Boss (Challenge):**
```
normalSpawnCount: 3
emergencySpawnCount: 7
launchSpeed: 5.0
spawnDelay: 6.0
phase2HPThreshold: 0.3 (30%)
```

#### **Nightmare Boss (Extreme):**
```
normalSpawnCount: 4
emergencySpawnCount: 10
launchSpeed: 6.0
spawnDelay: 5.0
phase2HPThreshold: 0.4 (40%)
```

---

### **Visual Tuning**

#### **Spawn Spread:**
```
minSpawnRadius controls circle size:
- 0.3 = Tight cluster (enemies overlap)
- 0.8 = Medium spread (clean separation)
- 1.5 = Wide spread (dramatic volcano)
```

#### **Launch Distance:**
```
launchSpeed + enemy KR stat:
- Speed 3.0, KR 0.5 = Short flight
- Speed 4.0, KR 0.8 = Medium flight
- Speed 6.0, KR 1.0 = Long flight
```

**Formula:** `Flight Distance ≈ launchSpeed / KR`

#### **Spawn Density:**
```
More enemies = More visual impact
- 2 enemies = Minimal threat
- 5 enemies = Moderate chaos
- 10 enemies = Screen full of danger
```

---

## Troubleshooting

### **Issue: Enemies spawn at center (overlap)**

**Symptoms:**
- All enemies appear at exact same position
- Enemies collide with each other immediately
- No circle formation

**Solutions:**
1. Check `minSpawnRadius > 0` (not zero)
2. Verify `spawnOffset = direction * minSpawnRadius` in code
3. Confirm spawn position calculation: `center + spawnOffset`
4. Increase `minSpawnRadius` value (try 1.0)

---

### **Issue: Enemies don't fly outward**

**Symptoms:**
- Enemies spawn but don't move
- Enemies just stand still in circle
- No knockback effect

**Solutions:**
1. Check enemy prefab has `E_Controller` component
2. Verify `E_Controller.SetKnockback()` method exists
3. Confirm `launchSpeed > 0` (not zero)
4. Check enemy's `c_Stats.KR` stat (not too high)
5. Increase `launchSpeed` value (try 5.0)

**Debug Code:**
```csharp
// Add to SpawnEnemies() after SetKnockback call:
Debug.Log($"Enemy launched: direction={direction}, speed={launchSpeed}");
```

---

### **Issue: Enemies spawn before animation completes**

**Symptoms:**
- Enemies appear during jump
- Spawn happens before slam lands
- Timing feels off

**Solutions:**
1. Check `yield return new WaitForSeconds(specialAnimLength)` BEFORE `SpawnEnemies()`
2. Verify `specialAnimLength` matches animation duration
3. Open Animator window, check "Special" animation length
4. Update `specialAnimLength` to match (e.g., 2.5 seconds)

---

### **Issue: Random spawn count (inconsistent)**

**Symptoms:**
- Sometimes spawns 1 enemy, sometimes 2
- Count varies each time
- Not predictable

**Cause:** Unity's `Random.Range(min, max)` with integers is EXCLUSIVE of max

**Wrong Code:**
```csharp
// ❌ Random.Range(1, 3) returns 1 OR 2 (not 3!)
int count = Random.Range(normalSpawnCount - 1, normalSpawnCount + 2);
```

**Fixed Code:**
```csharp
// ✅ Use exact count
int spawnCount = isPhase2 ? emergencySpawnCount : normalSpawnCount;
```

---

### **Issue: Phase 2 doesn't trigger**

**Symptoms:**
- Boss reaches low HP but Phase 2 never starts
- No retreat behavior
- Spawn count doesn't increase

**Solutions:**
1. Check `phase2HPThreshold` value (default 0.2 = 20%)
2. Verify `CheckPhaseTransition()` called in `Update()`
3. Add debug log:
```csharp
void CheckPhaseTransition()
{
    float hpPercent = (float)c_Health.CurrentHealth / c_Health.MaxHealth;
    Debug.Log($"HP: {hpPercent * 100}%, Threshold: {phase2HPThreshold * 100}%");
    
    if (!isPhase2 && hpPercent <= phase2HPThreshold)
    {
        Debug.Log("Phase 2 TRIGGERED!");
        isPhase2 = true;
    }
}
```
4. Watch Console during fight to see HP percentage
5. Lower `phase2HPThreshold` if needed (try 0.3 = 30%)

---

### **Issue: Phase 2 retreat loop broken**

**Symptoms:**
- Boss enters Phase 2 but doesn't retreat
- Boss stands still forever
- No special attack happens

**Solutions:**
1. Check Phase 2 coroutine starts in `Update()`:
```csharp
if (isPhase2 && !isInRetreatCycle && !isSpecialAttacking)
{
    StartCoroutine(RetreatCycle());
}
```
2. Verify `isInRetreatCycle` flag set correctly
3. Ensure `SwitchState()` calls succeed
4. Check no other state overriding retreat behavior

---

### **Issue: Enemies spawn with wrong velocity direction**

**Symptoms:**
- Enemies fly toward center instead of outward
- Movement direction is reversed
- Chaotic/random directions

**Solutions:**
1. Verify angle calculation correct:
```csharp
Vector2 direction = new Vector2(
    Mathf.Cos(angle * Mathf.Deg2Rad),
    Mathf.Sin(angle * Mathf.Deg2Rad)
).normalized;
```
2. Check knockback uses positive force:
```csharp
SetKnockback(direction * launchSpeed); // NOT negative
```
3. Confirm `direction.magnitude ≈ 1.0` (normalized)
4. Add gizmo to visualize directions:
```csharp
Debug.DrawRay(spawnPos, direction * launchSpeed, Color.red, 2f);
```

---

### **Issue: Collision shape doesn't follow sprite**

**Symptoms:**
- Collider stays in one place
- Sprite jumps but collider doesn't
- Player can walk through sprite during jump

**Solutions:**
1. Add `C_UpdateColliderShape` script to sprite child
2. Verify Custom Physics Shape defined in Sprite Editor
3. Check sprite child has PolygonCollider2D
4. See [GRS_BAKED_WEAPON_SYSTEM.md](./GRS_BAKED_WEAPON_SYSTEM.md) for details

---

## Testing Checklist

### **Setup Verification:**
- [ ] GS2_Controller component configured
- [ ] Enemy prefabs assigned to spawn list
- [ ] Animation "Special" trigger exists
- [ ] Special animation duration = specialAnimLength
- [ ] C_UpdateColliderShape on sprite child (optional, for collision accuracy)

### **Phase 1 Tests:**
- [ ] Boss chases player normally
- [ ] Periodic spawns work (every spawnDelay seconds)
- [ ] Spawns normalSpawnCount enemies (default: 2)
- [ ] Special attack can trigger randomly
- [ ] Enemies spawn on circle perimeter (not center)
- [ ] Enemies fly outward after spawn
- [ ] Enemies slow down naturally (knockback decay)

### **Phase 2 Transition:**
- [ ] Phase 2 triggers at 20% HP
- [ ] Boss stops chasing player
- [ ] Retreat cycle starts immediately
- [ ] No errors in console during transition

### **Phase 2 Retreat Cycle:**
- [ ] Boss runs away from player (retreatDuration)
- [ ] Boss stops moving (stopDuration)
- [ ] Boss performs special attack
- [ ] Special attack spawns emergencySpawnCount enemies (default: 5)
- [ ] Cycle repeats continuously
- [ ] No stuck states or infinite loops

### **Special Attack Tests:**
- [ ] Boss pauses before jump (specialIdleDelay)
- [ ] Jump animation plays (specialAnimLength)
- [ ] Enemies spawn AFTER animation completes (at slam impact)
- [ ] Enemies spawn on circle edge
- [ ] Enemies fly outward in all directions
- [ ] Spawn count correct (2 Phase 1, 5 Phase 2)

### **Visual/Polish:**
- [ ] Spawn timing feels natural (eruption on slam)
- [ ] Enemy spread looks good (no overlap)
- [ ] Launch distance appropriate (not too far/close)
- [ ] Phase 2 feels more threatening (more enemies)
- [ ] Collider follows sprite during jump (if C_UpdateColliderShape used)

---

## Quick Reference

### **Inspector Settings Summary**

**GS2_Controller (Default Values):**
```
[Spawn Settings]
Normal Spawn Count: 2
Emergency Spawn Count: 5
Min Spawn Radius: 0.8
Launch Speed: 4.0
Spawn Delay: 8.0

[Special Attack]
Special Idle Delay: 1.0
Special Anim Length: 2.5

[Phase 2]
Phase 2 HP Threshold: 0.2
Retreat Duration: 3.0
Stop Duration: 1.5
```

### **Key Methods**

**GS2_Controller:**
- `SpawnEnemies()` - Create enemies on circle, apply knockback
- `CheckPhaseTransition()` - Monitor HP, trigger Phase 2
- `RetreatCycle()` - Phase 2 retreat → stop → special loop
- `SpecialRoutine()` - Wait → animate → spawn

**E_Controller:**
- `SetKnockback(Vector2 force)` - Apply knockback to enemy
- `FixedUpdate()` - Applies `rb.linearVelocity = desiredVelocity + knockback`

### **File Locations**

```
Assets/GAME/Scripts/
├── Character/
│   └── C_UpdateColliderShape.cs (optional, for collision)
└── Enemy/
    ├── GS2_Controller.cs (main boss script)
    └── E_Controller.cs (used by spawned enemies)
```

---

## Advanced Notes

### **Why Circle Spawn?**

**Alternative approaches considered:**

**1. Grid Spawn:**
```csharp
// ❌ Too rigid, doesn't look natural
for (int x = -1; x <= 1; x++)
    for (int y = -1; y <= 1; y++)
        Spawn at (x, y)
```

**2. Random Area:**
```csharp
// ❌ Uneven distribution, some enemies too close
Vector2 randomPos = center + Random.insideUnitCircle * radius;
```

**3. Circle Perimeter:**
```csharp
// ✅ Even distribution, clean separation, dramatic effect
float angle = Random.Range(0f, 360f);
Vector2 direction = AngleToVector(angle);
Vector2 spawnPos = center + direction * minSpawnRadius;
```

**Benefits:**
- Even radial distribution
- No overlapping enemies
- Dramatic "explosion" effect
- Consistent spacing
- Scalable (more enemies = denser circle, no overlap)

---

### **Knockback vs Velocity**

**Why not just set velocity?**

```csharp
// Common mistake:
rb.linearVelocity = direction * speed;

// Problem: E_Controller overwrites it every frame!
void FixedUpdate()
{
    rb.linearVelocity = desiredVelocity + knockback;
    // ↑ Your velocity is lost here
}
```

**Solution: Use the knockback system**

```csharp
// Correct approach:
enemyController.SetKnockback(direction * launchSpeed);

// Why it works:
void FixedUpdate()
{
    rb.linearVelocity = desiredVelocity + knockback;
    // ↑ Knockback is PART of the velocity calculation
}
```

**Knockback advantages:**
- Integrates with existing physics system
- Natural deceleration via KR stat
- No velocity fighting between systems
- Smooth, predictable behavior

---

### **Exact vs Random Spawn Count**

**Original implementation:**
```csharp
int count = Random.Range(normalSpawnCount - 1, normalSpawnCount + 2);
// Intention: Sometimes spawn 1, 2, or 3 enemies
// Reality: Random.Range(1, 3) returns 1 or 2 (exclusive of max!)
```

**Problem:** Inconsistent difficulty, unpredictable gameplay

**Solution:** Use exact counts
```csharp
int spawnCount = isPhase2 ? emergencySpawnCount : normalSpawnCount;
// Phase 1: Always 2 enemies
// Phase 2: Always 5 enemies
```

**Benefits:**
- Predictable difficulty curve
- Easier to balance
- Consistent player experience
- Clear Phase 2 escalation

**If you want variance:**
```csharp
// Inclusive range (use floats, then cast):
int count = Mathf.FloorToInt(Random.Range(1f, 4f)); // 1, 2, or 3

// Or explicit inclusive:
int count = Random.Range(1, 4); // 1, 2, or 3 (4 is exclusive)
```

---

### **Animation Sync Importance**

**Why timing matters:**

```
Bad timing (spawn at start):
Frame 0.0s: Animation starts
Frame 0.0s: Enemies spawn ← TOO EARLY
Frame 1.0s: Boss jumps
Frame 2.5s: Boss slams down

Player sees: Enemies appear, THEN boss jumps (disconnected)
```

```
Good timing (spawn at end):
Frame 0.0s: Animation starts
Frame 1.0s: Boss jumps
Frame 2.5s: Boss slams down
Frame 2.5s: Enemies spawn ← PERFECT
Player sees: Boss slams, enemies erupt (cause and effect!)
```

**Critical for game feel:**
- Visual feedback matches gameplay
- Clear cause → effect relationship
- Satisfying "impact" moment
- Professional polish

---

**Status:** ✅ System ready for production use  
**Last Updated:** November 8, 2025  
**Related Docs:** [GRS_BAKED_WEAPON_SYSTEM.md](./GRS_BAKED_WEAPON_SYSTEM.md)  
**Next Steps:** Balance spawn counts, tune Phase 2 difficulty, polish special animation
