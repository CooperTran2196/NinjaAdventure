# Boss Systems - Complete Technical Documentation
**Created:** November 8, 2025  
**Status:** ✅ Production Ready  
**Bosses:** GR (Giant Raccoon), GRS (Giant Raccoon Shaman), GS2 (Giant Slime Phase 2)

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [GR - Giant Raccoon (Charger Boss)](#gr---giant-raccoon-charger-boss)
3. [GRS - Giant Raccoon Shaman (Melee/Dash Boss)](#grs---giant-raccoon-shaman-meleadash-boss)
4. [GS2 - Giant Slime Phase 2 (Summoner Boss)](#gs2---giant-slime-phase-2-summoner-boss)
5. [Shared Systems](#shared-systems)
6. [Testing Checklist](#testing-checklist)

---

## Overview

### Architecture Pattern
All three bosses follow the **Controller + State Pattern**:
- **Controller**: Manages state machine, health events, collision damage, death sequence
- **States**: Modular behaviors (Idle, Wander, Chase, Attack) enabled/disabled by controller
- **Clean separation**: Controllers use `[RequireComponent]`, states use `=` (not `??=`)

### Common Features
✅ **Collision damage** - Contact damage with cooldown timer  
✅ **Death handling** - Stops coroutines before disabling states (prevents dash/attack continuation)  
✅ **Animator integration** - All use same animator parameters (`isIdle`, `isMoving`, `isWandering`, `isAttacking`, `isSpecialAttack`)  
✅ **Event-driven** - Subscribe to `C_Health.OnDied` for death sequence  
✅ **Gizmos** - Visual debug ranges in Scene view

---

## GR - Giant Raccoon (Charger Boss)

### 🎯 Combat Style
**High-mobility charger with two attack modes:**
- **Normal Attack**: Charge at player with weapon hitbox
- **Special Attack**: Jump attack with AoE damage on landing

### 📁 Scripts
- `GR_Controller.cs` - State machine and collision damage
- `GR_State_Chase.cs` - Direct chase toward player
- `GR_State_Attack.cs` - Charge/jump attack with dash mechanics

---

### GR_Controller.cs

**States:**
```csharp
public enum GRState { Idle, Wander, Chase, Attack }
```

**Inspector Fields:**
```csharp
[Header("Detection")]
public GRState   defaultState       = GRState.Idle;
public float     detectionRange     = 10f;   // Chase + special attack range
public float     attackRange        = 3f;    // Normal attack trigger range
public LayerMask playerLayer;
public float     attackStartBuffer  = 0.20f; // Delay before attack triggers
```

**State Transition Logic (Update):**
```
1. Check if player in attackRange → set target, start inRangeTimer
2. Check if attack state has IsAttacking flag → stay in Attack
3. Check if special attack ready (CanSpecialNow) → switch to Attack
4. Check if melee ready (inRangeTimer >= buffer) → switch to Attack
5. Check if player in detectionRange → switch to Chase
6. Otherwise → use defaultState (Idle/Wander)
```

**Collision Damage:**
- Uses `OnCollisionStay2D` to detect player contact
- Applies `c_Stats.collisionDamage` with `c_Stats.collisionTick` cooldown
- Filters by `playerLayer` mask
- Disabled on death (colliders disabled in HandleDeath)

**Death Sequence:**
```csharp
OnDiedHandler() called by C_Health.OnDied event:
1. StopAllCoroutines() on all states (CRITICAL - prevents dash continuation)
2. Disable all states immediately
3. Start HandleDeath() coroutine:
   - Stop movement (desiredVelocity = zero, rb.linearVelocity = zero)
   - Disable all colliders (stops collision damage)
   - Play death animation (anim.SetTrigger("Die"))
   - Wait 1.5 seconds
   - Fade out (C_FX.FadeOut)
   - Destroy GameObject
```

---

### GR_State_Chase.cs

**Purpose:** Chase player until within attack range

**Inspector Fields:**
```csharp
public float stopBuffer = 0.10f; // Extra distance before stopping
```

**Runtime Behavior:**
```csharp
void Update()
{
    if (!target) { stop movement, return }
    
    Vector2 toTarget = target.position - transform.position
    float distance = toTarget.magnitude
    
    // Move if outside (chargeRange + stopBuffer)
    if (distance > chargeRange + stopBuffer)
        velocity = toTarget.normalized * stats.MS
    else
        velocity = zero
    
    controller.SetDesiredVelocity(velocity)
    UpdateFloats(velocity) // Sets animator moveX/Y, idleX/Y
}
```

**Animator Parameters Set:**
- `isMoving` (true when enabled, false when velocity = zero)
- `isIdle` (false when enabled, true when disabled)
- `moveX`, `moveY` (current movement direction)
- `idleX`, `idleY` (last movement direction for idle facing)

---

### GR_State_Attack.cs

**Purpose:** Execute normal charge or special jump attack with dash mechanics

**Inspector Fields:**
```csharp
[Header("Normal Attack (Charge)")]
public float attackCooldown   = 1.10f;  // Cooldown between attacks
public float attackClipLength = 2.35f;  // Total animation length
public float attackHitDelay   = 2.00f;  // Charging duration before dash
public float attackDashSpeed  = 9.0f;   // Constant dash velocity

[Header("Special Attack (Jump)")]
public float specialCooldown       = 8.0f;   // Cooldown between specials
public float specialClipLength     = 5.0f;   // Total animation length
public float specialHitDelay       = 3.0f;   // Charging duration before dash
public float specialDashSpeed      = 12.0f;  // Constant dash velocity
public float specialAoERadius      = 1.8f;   // AoE damage radius
public float specialAoEOffsetY     = -1f;    // Y offset for AoE center
public int   specialDamage         = 25;     // Direct damage (bypasses weapon)
public float specialKnockbackForce = 8f;     // Knockback force

[Header("Dash Settings")]
public float stopShortOffset = 0.96f; // Distance kept in front of player
```

**Attack Decision (Update):**
```csharp
Priority order:
1. If IsAttacking → stay locked (don't start new attack)
2. If specialReady && inSpecialRange → StartCoroutine(AttackRoutine(dir, isSpecial: true))
3. If canAttackNow && inAttackRange → StartCoroutine(AttackRoutine(dir, isSpecial: false))
```

**Attack Flow (AttackRoutine):**
```
┌─────────────────────────────────────────────────────────────┐
│ 1. IsAttacking = true                                       │
│ 2. Set animator bools (isSpecialAttack or isAttacking)     │
│ 3. Set facing direction (atkX, atkY animator floats)       │
│ 4. anim.speed = 1.0 (normal speed)                          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ CHARGING PHASE                                              │
│ - Wait hitDelay seconds (player sees charge animation)     │
│ - Boss is stationary during this time                       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ DASH ANIMATION SPEED CALCULATION                            │
│ dashPhaseTime = clipLength - hitDelay                       │
│ actualDashDist = CalculateDashDistance(maxRange)           │
│ timeNeeded = actualDashDist / dashSpeed                     │
│ animSpeed = dashPhaseTime / timeNeeded                      │
│ anim.speed = animSpeed (sync animation with movement)      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ DASH PHASE                                                  │
│ - BeginDash(dashSpeed, actualDashDist)                     │
│   • isDashing = true                                        │
│   • Calculate dashDir, dashDest                             │
│   • Start afterimage burst                                  │
│ - Normal: Enable weapon hitbox (activeWeapon.Attack)       │
│ - Loop: Check ReachedDashDest() or clipLength reached      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ LANDING/IMPACT                                              │
│ - StopDash()                                                │
│   • Stop afterimage burst                                   │
│   • isDashing = false                                       │
│   • SetDesiredVelocity(zero)                                │
│ - Special only: ApplyAoEDamageKnockback()                  │
│   • OverlapCircle at landing position                       │
│   • Apply specialDamage directly to C_Health                │
│   • Apply knockback to P_Controller                         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ COOLDOWN & RESET                                            │
│ - Set nextAttackReadyAt / nextSpecialReadyAt               │
│ - IsAttacking = false                                       │
│ - anim.speed = 1.0                                          │
│ - Reset animator bools                                      │
└─────────────────────────────────────────────────────────────┘
```

**Dash Distance Calculation:**
```csharp
CalculateDashDistance(maxRange):
    target position (player location when dash starts)
    faceSpot = targetPos - (stopShortOffset in direction)
    return distance from boss to faceSpot
    
    // Always dashes to player position (no maxRange limit)
    // Player can dodge by moving away
```

**Key Features:**
- **Dynamic animation speed**: Animation syncs with actual dash movement
- **Afterimage trail**: Visual effect during dash using C_AfterimageSpawner
- **Two damage sources**:
  - Normal: Weapon hitbox collision during dash
  - Special: AoE damage on landing (bypasses weapon stats)
- **Stops on death**: HandleDeath in controller calls StopAllCoroutines to prevent dash continuation

---

## GRS - Giant Raccoon Shaman (Melee/Dash Boss)

### 🎯 Combat Style
**Close-range melee with double-hit dash special:**
- **Normal Attack**: Basic melee strike with weapon
- **Special Attack**: Dash forward with two hits (first on dash, second followup)

### 📁 Scripts
- `GRS_Controller.cs` - State machine and collision damage
- `GRS_State_Chase.cs` - Horizontal-first chase with Y-axis alignment
- `GRS_State_Attack.cs` - Normal melee + double-hit dash special

---

### GRS_Controller.cs

**States:**
```csharp
public enum GRSState { Idle, Wander, Chase, Attack }
```

**Inspector Fields:**
```csharp
[Header("Detection")]
public GRSState  defaultState       = GRSState.Idle;
public float     detectionRange     = 10f;
public float     attackRange        = 1.6f;
public LayerMask playerLayer;
public float     attackStartBuffer  = 0.20f;
```

**State Transition Logic:** Same as GR (see GR_Controller above)

**Unique Features:**
- Smaller attack range (1.6f vs GR's 3f) - requires closer proximity
- Uses `GetComponentsInChildren<Collider2D>()` in death (GR uses `GetComponents`)

---

### GRS_State_Chase.cs

**Purpose:** Chase player with horizontal-first movement and Y-axis alignment bias

**Inspector Fields:**
```csharp
public float stopBuffer = 0.10f;
public float yAlignBand = 0.35f; // Shrink |dy| toward this during chase
```

**Movement Logic:**
```csharp
if (Abs(dy) > yAlignBand):
    // Prioritize vertical alignment
    desired.y = Sign(dy)
    desired.x = Sign(dx) * 0.6f
else:
    // Horizontal chase (already aligned)
    desired.x = Sign(dx)
    desired.y = Sign(dy) * 0.35f

// Move if outside attack range + buffer
if (distance > attackRange + stopBuffer)
    velocity = desired.normalized * c_Stats.MS
else
    velocity = zero
```

**Why this pattern?**
- Top-down movement: Y-axis alignment important for melee attacks
- Horizontal priority: Natural movement feels better
- Smooth transitions: Gradual shift between modes

---

### GRS_State_Attack.cs

**Purpose:** Execute normal melee or special double-hit dash

**Inspector Fields:**
```csharp
[Header("Normal Attack")]
public float attackCooldown = 1.10f;
public float attackDuration = 0.45f;
public float hitDelay       = 0.25f;

[Header("Special (single clip flow)")]
public float specialCooldown  = 8.0f;
public float specialClipLength = 1.50f;
public float specialHitDelay  = 0.50f;   // Charging duration
public float specialDashSpeed = 9.0f;    // Constant dash velocity
public float followupGap      = 0.14f;   // Gap between first and second hit

[Header("Alignment Gate")]
public float yHardCap = 0.55f; // Must be within this to attack
```

**Attack Decision:**
```csharp
// Alignment gate - MUST be vertically aligned to attack
if (!alignedY) return // abs(dy) > yHardCap

Priority:
1. If specialReady && inSpecialRange → SpecialRoutine (dash with 2 hits)
2. If inNormalRange → NormalRoutine (basic melee)
```

**Normal Attack Flow:**
```
1. IsAttacking = true
2. Set animator (isAttacking = true)
3. Wait hitDelay (0.25s)
4. Enable weapon hitbox
5. Wait rest of duration
6. Set cooldown, IsAttacking = false
```

**Special Attack Flow (Double-Hit Dash):**
```
┌─────────────────────────────────────────────────────────────┐
│ SETUP                                                       │
│ - IsAttacking = true                                        │
│ - Set animator (isSpecialAttack = true)                    │
│ - Calculate dash distance                                   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ CHARGING PHASE                                              │
│ - Wait specialHitDelay (0.5s)                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ DASH SPEED CALCULATION                                      │
│ dashPhaseTime = specialClipLength - specialHitDelay         │
│ animSpeed = dashPhaseTime / (dashDist / dashSpeed)         │
│ anim.speed = animSpeed                                      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ FIRST HIT (Dash Start)                                      │
│ - BeginDash(specialDashSpeed, dashDist)                    │
│ - Enable weapon hitbox (activeWeapon.Attack)               │
│ - Start afterimage burst                                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ DASH LOOP                                                   │
│ while (t < specialClipLength && !ReachedDashDest):         │
│   - controller.SetDesiredVelocity(dashDir * dashSpeed)     │
│   - yield return null                                       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ STOP DASH                                                   │
│ - Stop afterimage burst                                     │
│ - isDashing = false                                         │
│ - SetDesiredVelocity(zero)                                  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ FOLLOWUP GAP                                                │
│ - Wait followupGap (0.14s)                                  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ SECOND HIT                                                  │
│ - Enable weapon hitbox again (activeWeapon.Attack)         │
│ - Wait hitDelay (0.25s for second hit to register)         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ COOLDOWN & RESET                                            │
│ - Set nextSpecialReadyAt                                    │
│ - IsAttacking = false                                       │
│ - anim.speed = 1.0                                          │
│ - Reset animator bools                                      │
└─────────────────────────────────────────────────────────────┘
```

**Key Features:**
- **Y-axis alignment gate**: Prevents attacks when not aligned (yHardCap = 0.55f)
- **Double-hit mechanic**: Two weapon hitbox activations (dash + followup)
- **Gap timing**: `followupGap` controls delay between hits (0.14s)
- **Shorter dash**: Smaller detection range vs GR (dash range = detectionRange)

---

## GS2 - Giant Slime Phase 2 (Summoner Boss)

### 🎯 Combat Style
**Summoner boss with phase-based retreat mechanics:**
- **Phase 1 (100% → 20% HP)**: Chase player, periodic enemy spawns
- **Phase 2 (<20% HP)**: Emergency spawn, then retreat/stop cycle

### 📁 Scripts
- `GS2_Controller.cs` - State machine, phase system, special attack (spawn enemies)
- `GS2_State_Chase.cs` - Chase with Phase 2 retreat behavior

---

### GS2_Controller.cs

**States:**
```csharp
public enum GS2State { Idle, Wander, Chase }
// No Attack state - uses special attack from Chase state
```

**Inspector Fields:**
```csharp
[Header("Detection")]
public GS2State  defaultState   = GS2State.Idle;
public float     detectionRange = 12f;
public LayerMask playerLayer;

[Header("Special Attack (Spawn Enemies)")]
public GameObject enemyPrefab;              // Regular enemy prefab (no weapon)
public int        normalSpawnCount = 3;     // 2-3 enemies per special
public int        emergencySpawnCount = 5;  // 4-5 enemies at phase 2 start
public float      specialCooldown   = 12f;  // Cooldown between spawns
public float      specialIdleDelay  = 1.0f; // Idle before animation starts
public float      specialAnimLength = 2.5f; // Length of special attack animation
public float      spawnSpreadRadius = 2.5f; // Max spawn distance from boss

[Header("Phase 2 Settings")]
[Range(0f, 1f)]
public float phase2Threshold = 0.20f; // 20% HP
public float retreatDistance = 4f;    // Start retreating when player < 4 units
public float retreatDuration = 3f;    // Retreat for 3 seconds
public float retreatCooldown = 2f;    // Stop for 2 seconds (vulnerable window)
```

**State Machine (Update):**
```csharp
1. CheckPhaseTransition() // Check for phase 2 trigger
2. if (isPhase2) UpdateRetreatBehavior() // Handle retreat cycle
3. if (target && CanSpecialNow()) TriggerSpecial() // Check special attack
4. Detect player in detectionRange → switch to Chase or defaultState
```

---

### Special Attack System - Complete Explanation

#### How SpawnEnemies Works

**Method Signature:**
```csharp
public void SpawnEnemies(int count)
```

**Purpose:** Instantiate multiple enemies in a circular spread pattern around the boss

**Algorithm:**
```csharp
for each enemy (i = 0 to count):
    1. Calculate base angle for even distribution:
       angle = (360° / count) * i
       
    2. Add randomness:
       angle += Random.Range(-20°, +20°)
       
    3. Calculate random radius:
       radius = Random.Range(1.5f, spawnSpreadRadius)
       
    4. Convert polar coordinates to cartesian:
       offset.x = cos(angle * Deg2Rad) * radius
       offset.y = sin(angle * Deg2Rad) * radius
       
    5. Calculate spawn position:
       spawnPos = boss.position + offset
       
    6. Instantiate enemy prefab at spawnPos
```

**Visual Example (3 enemies, spawnSpreadRadius = 2.5f):**
```
         Enemy 2
           /|\
          / | \
         /  |  \
   radius  Boss  radius
        \   |   /
         \  |  /
          \ | /
    Enemy 1  |  Enemy 3
           
Base angles: 0°, 120°, 240°
Randomness: ±20° per enemy
Radius: 1.5f to 2.5f per enemy
```

**spawnSpreadRadius Parameter:**
- **Meaning**: Maximum distance from boss center to spawn point
- **Range**: Enemies spawn between `1.5f` and `spawnSpreadRadius`
- **Recommended**: 2.5f (default) - keeps enemies visible but not too far
- **Too small (<2.0f)**: Enemies overlap, hard to distinguish
- **Too large (>4.0f)**: Enemies spawn off-screen, player confused

---

#### Special Attack Workflow

**Trigger Conditions:**
```csharp
CanSpecialNow():
    - NOT already doing special attack (isDoingSpecialAtk = false)
    - Cooldown expired (Time.time >= nextSpecialTime)
    return true if both conditions met
```

**Complete Flow (SpecialAtkRoutine coroutine):**

```
┌─────────────────────────────────────────────────────────────┐
│ TRIGGER (TriggerSpecial called from Update)                │
│ - StartCoroutine(SpecialAtkRoutine())                      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ PHASE 1: IDLE PREPARATION (specialIdleDelay)               │
│ Duration: 1.0 second (default)                              │
│                                                             │
│ isDoingSpecialAtk = true                                    │
│ desiredVelocity = zero                                      │
│ rb.linearVelocity = zero                                    │
│ yield WaitForSeconds(specialIdleDelay)                      │
│                                                             │
│ Visual: Boss stops moving, stands still                     │
│ Purpose: Telegraph to player "special attack coming!"       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ PHASE 2: ANIMATION (specialAnimLength)                     │
│ Duration: 2.5 seconds (default)                             │
│                                                             │
│ anim.SetBool("isSpecialAttack", true)                       │
│ yield WaitForSeconds(specialAnimLength)                     │
│                                                             │
│ Visual: Boss plays summon/cast animation                    │
│ Purpose: Visual feedback, give player time to react         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ PHASE 3: SPAWN ENEMIES                                     │
│ Duration: Instant                                           │
│                                                             │
│ spawnCount = Random.Range(normalSpawnCount - 1,             │
│                           normalSpawnCount + 1)             │
│ // Default: 2-4 enemies (normalSpawnCount = 3)             │
│                                                             │
│ SpawnEnemies(spawnCount)                                    │
│ // Circular spread pattern around boss                      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ PHASE 4: RESET                                              │
│ Duration: Instant                                           │
│                                                             │
│ anim.SetBool("isSpecialAttack", false)                      │
│ nextSpecialTime = Time.time + specialCooldown (12s)        │
│ isDoingSpecialAtk = false                                   │
│                                                             │
│ Visual: Boss returns to normal behavior                     │
│ Purpose: Re-enable movement, set cooldown                   │
└─────────────────────────────────────────────────────────────┘
                            ↓
                    Return to Chase
```

**Total Duration:**
```
specialIdleDelay + specialAnimLength = 1.0s + 2.5s = 3.5 seconds
Boss is frozen during this entire time
```

**Parameter Tuning Guide:**

| Parameter | Default | Purpose | Tuning Tips |
|-----------|---------|---------|-------------|
| `specialIdleDelay` | 1.0s | Telegraph time | **Increase** for easier gameplay (more warning)<br>**Decrease** for harder gameplay (less reaction time)<br>**Min**: 0.5s (fair warning)<br>**Max**: 2.0s (too obvious) |
| `specialAnimLength` | 2.5s | Visual feedback | **Match animation clip length**<br>**Sync with spawn timing** in animation events (if used)<br>Should feel smooth, not abrupt |
| `spawnSpreadRadius` | 2.5f | Spawn distance | **Increase** (3.0f-4.0f): Enemies spawn farther, easier for player<br>**Decrease** (2.0f): Tighter spawn, harder for player<br>**Min**: 1.5f (enemies too clustered)<br>**Max**: 5.0f (enemies off-screen) |
| `normalSpawnCount` | 3 | Enemies per cast | **Actual spawns**: count ± 1 (2-4 enemies)<br>**Balance**: Higher HP boss = more spawns<br>**Recommended**: 2-4 for normal difficulty |
| `specialCooldown` | 12f | Time between spawns | **Lower** (8-10s): More pressure, harder<br>**Higher** (15-20s): More breathing room, easier<br>**Balance with**: Enemy HP, player damage |

**Example Scenarios:**

**Easy Mode (Give player more time):**
```csharp
specialIdleDelay  = 1.5f  // More warning
specialAnimLength = 3.0f  // Longer animation (more time to prepare)
spawnSpreadRadius = 3.5f  // Enemies farther away
normalSpawnCount  = 2     // Fewer enemies (1-3)
specialCooldown   = 15f   // Less frequent
```

**Hard Mode (Aggressive boss):**
```csharp
specialIdleDelay  = 0.5f  // Minimal warning
specialAnimLength = 2.0f  // Quick cast
spawnSpreadRadius = 2.0f  // Enemies close
normalSpawnCount  = 4     // More enemies (3-5)
specialCooldown   = 8f    // More frequent
```

---

### Phase 2 System

**Phase Transition:**
```csharp
CheckPhaseTransition() // Called every frame in Update

if (currentHP / maxHP <= 0.20 && !hasTriggeredPhase2):
    TriggerPhase2():
        1. hasTriggeredPhase2 = true (prevent re-trigger)
        2. isPhase2 = true
        3. Emergency spawn: 4-6 enemies (emergencySpawnCount ± 1)
        4. StartRetreat() // Begin retreat cycle immediately
```

**Retreat Cycle (Phase 2 Only):**
```
┌───────────────────────────────────────────────────────────┐
│ RETREAT MODE (isRetreating = true)                       │
│ Duration: retreatDuration (3 seconds)                     │
│                                                           │
│ In GS2_State_Chase:                                       │
│   moveVector = away from player                           │
│   speed = c_Stats.MS * retreatSpeedMultiplier (0.7)      │
│                                                           │
│ When retreatEndTime reached:                              │
│   isRetreating = false                                    │
│   retreatCooldownEndTime = now + retreatCooldown (2s)    │
└───────────────────────────────────────────────────────────┘
                            ↓
┌───────────────────────────────────────────────────────────┐
│ COOLDOWN WINDOW (IsInRetreatCooldown = true)             │
│ Duration: retreatCooldown (2 seconds)                     │
│                                                           │
│ In GS2_State_Chase:                                       │
│   controller.SetDesiredVelocity(zero)                     │
│   Boss stops moving completely                            │
│   VULNERABLE - Player can attack freely                   │
│                                                           │
│ When retreatCooldownEndTime reached:                      │
│   Check if player < retreatDistance                       │
│   If yes: StartRetreat() again (loop)                     │
│   If no: Return to normal chase                           │
└───────────────────────────────────────────────────────────┘
                            ↓
                    (Repeat cycle)
```

**Phase 2 Strategy:**
- Boss becomes defensive, runs away when player gets close
- Creates vulnerability windows (2s cooldown) for player to attack
- Continues spawning enemies on special attack cooldown
- Player must manage both boss and adds

---

### GS2_State_Chase.cs

**Purpose:** Chase player with Phase 2 retreat/stop behavior

**Inspector Fields:**
```csharp
public float retreatSpeedMultiplier = 0.7f; // 70% speed when retreating
```

**Update Logic:**
```csharp
1. if (!hasTarget) { stop movement, return }

2. if (controller.IsDoingSpecialAtk()) { stop movement, return }

3. PHASE 2 checks:
   if (controller.IsRetreating()):
       moveVector = away from player
       velocity = moveVector * MS * retreatSpeedMultiplier
   
   else if (controller.IsInRetreatCooldown()):
       velocity = zero (VULNERABLE)
   
   else:
       // Normal chase
       moveVector = toward player
       velocity = moveVector * MS

4. Set animator (isMoving) and UpdateFloats(moveVector)
```

**Animator Parameters:**
- Sets `isIdle`, `isMoving` based on state
- Updates `moveX/Y` and `idleX/Y` for directional facing

---

## Shared Systems

### Collision Damage (All 3 Bosses)

**Implementation:** `OnCollisionStay2D` in controller

```csharp
void OnCollisionStay2D(Collision2D collision)
{
    // Early exits
    if (!c_Health.IsAlive) return
    if (playerLayer check fails) return
    if (contactTimer > 0) { contactTimer -= fixedDeltaTime; return }
    
    // Find player C_Health
    playerHealth = collision.collider.GetComponent<C_Health>()
    if (!playerHealth || !playerHealth.IsAlive) return
    
    // Apply damage
    playerHealth.ChangeHealth(-c_Stats.collisionDamage)
    contactTimer = c_Stats.collisionTick
}
```

**How It Works:**
- Checks every **physics frame** (FixedUpdate rate)
- Uses `contactTimer` to prevent damage spam
- Damage values from `C_Stats` component
- Automatically disabled on death (colliders disabled)

---

### Death Handling (All 3 Bosses)

**Critical Pattern:**
```csharp
void OnDiedHandler() // Subscribed to C_Health.OnDied
{
    // 1. STOP COROUTINES FIRST (prevents dash/attack continuation)
    idle.StopAllCoroutines()
    wander.StopAllCoroutines()
    chase.StopAllCoroutines()
    attack?.StopAllCoroutines() // GS2 has no attack state
    
    // 2. DISABLE STATES IMMEDIATELY
    idle.enabled = false
    wander.enabled = false
    chase.enabled = false
    attack?.enabled = false
    
    // 3. START DEATH SEQUENCE
    StartCoroutine(HandleDeath())
}

IEnumerator HandleDeath()
{
    // Stop movement
    desiredVelocity = zero
    rb.linearVelocity = zero
    
    // Disable colliders (stop collision damage)
    colliders = GetComponentsInChildren<Collider2D>()
    foreach (col) col.enabled = false
    
    // Play death animation
    anim.SetTrigger("Die")
    yield WaitForSeconds(1.5f)
    
    // Fade out
    yield StartCoroutine(c_FX.FadeOut())
    
    // Clean up
    Destroy(gameObject)
}
```

**Why This Order Matters:**
1. **StopAllCoroutines first**: Prevents dash/attack coroutines from continuing after state disabled
2. **Disable states second**: Stops Update loops from starting new coroutines
3. **HandleDeath last**: Visual death sequence (animation + fade)

**Common Bug (Fixed):**
```
❌ WRONG (old code):
- Disable states first
- Start HandleDeath
- Coroutines keep running (dash continues)

✅ CORRECT (current code):
- StopAllCoroutines on states
- Disable states
- Start HandleDeath
- No dash/attack continuation
```

---

### Animator Integration

**All bosses use same animator parameters:**

| Parameter | Type | Purpose | Set By |
|-----------|------|---------|--------|
| `isIdle` | Bool | Idle animation | State_Idle, State transitions |
| `isMoving` | Bool | Movement animation | Chase states |
| `isWandering` | Bool | Wander animation | State_Wander |
| `isAttacking` | Bool | Normal attack | Attack states (GR/GRS) |
| `isSpecialAttack` | Bool | Special attack | Attack states (GR/GRS), GS2_Controller |
| `moveX`, `moveY` | Float | Movement direction | Chase states (UpdateFloats) |
| `idleX`, `idleY` | Float | Idle facing direction | Chase states (UpdateFloats) |
| `atkX`, `atkY` | Float | Attack direction | Attack states (GR/GRS) |
| `Die` | Trigger | Death animation | HandleDeath coroutine |

**Animator Override Controllers:**
- GR uses base animator controller
- GRS uses GR's animator (same parameters)
- GS2 uses GR's animator with overrides (no attack animations needed)

---

## Testing Checklist

### GR - Giant Raccoon

**Normal Attack (Charge):**
- [ ] Boss charges at player when in range
- [ ] Weapon hitbox damages player during dash
- [ ] Afterimage trail appears during dash
- [ ] Boss stops at correct distance (stopShortOffset)
- [ ] Attack cooldown works (attackCooldown)
- [ ] Animation syncs with dash movement

**Special Attack (Jump):**
- [ ] Boss jumps when in special range
- [ ] Afterimage trail appears during dash
- [ ] AoE damage applies on landing
- [ ] Knockback pushes player away from landing position
- [ ] Special cooldown works (specialCooldown)
- [ ] Special range larger than normal attack range

**Death:**
- [ ] Boss stops all actions immediately on death
- [ ] No dash continuation during death animation
- [ ] Collision damage stops (colliders disabled)
- [ ] Fade out works
- [ ] GameObject destroyed after fade

---

### GRS - Giant Raccoon Shaman

**Normal Attack (Melee):**
- [ ] Boss performs melee when aligned (yHardCap)
- [ ] Weapon hitbox damages player
- [ ] Attack cooldown works

**Special Attack (Double-Hit Dash):**
- [ ] Boss dashes forward when in special range
- [ ] First hit damages player (dash start)
- [ ] Second hit damages player after gap (followupGap)
- [ ] Afterimage trail during dash
- [ ] Special cooldown works

**Chase Behavior:**
- [ ] Horizontal-first movement feels natural
- [ ] Y-axis alignment bias works (yAlignBand)
- [ ] Stops at correct distance (stopBuffer)

**Death:**
- [ ] Boss stops all actions immediately on death
- [ ] No dash continuation during death animation
- [ ] Collision damage stops
- [ ] Fade out works

---

### GS2 - Giant Slime Phase 2

**Phase 1 (100% → 20% HP):**
- [ ] Boss chases player normally
- [ ] Special attack triggers on cooldown (specialCooldown)
- [ ] Boss stops and idles before special (specialIdleDelay)
- [ ] Special attack animation plays (specialAnimLength)
- [ ] Enemies spawn in circular pattern (spawnSpreadRadius)
- [ ] Spawn count correct (normalSpawnCount ± 1)

**Phase 2 Transition (20% HP):**
- [ ] Emergency spawn triggers (emergencySpawnCount ± 1)
- [ ] Boss immediately starts retreat cycle
- [ ] Phase 2 only triggers once (hasTriggeredPhase2)

**Phase 2 Retreat Cycle:**
- [ ] Boss retreats when player < retreatDistance
- [ ] Retreat lasts correct duration (retreatDuration)
- [ ] Boss moves slower during retreat (retreatSpeedMultiplier)
- [ ] Boss stops after retreat (cooldown window)
- [ ] Cooldown lasts correct duration (retreatCooldown)
- [ ] Cycle repeats if player still close
- [ ] Boss returns to normal chase if player far away

**Special Attack (Phase 2):**
- [ ] Boss can still spawn enemies during Phase 2
- [ ] Special attack stops retreat temporarily
- [ ] Boss frozen during special attack (idle + animation)

**Death:**
- [ ] Boss stops all actions immediately on death
- [ ] Special attack coroutine stops if active
- [ ] Collision damage stops
- [ ] Fade out works

---

## Common Issues & Solutions

### Issue: Boss dashes during death animation
**Cause:** Coroutines not stopped before disabling states  
**Solution:** Call `StopAllCoroutines()` on all states in `OnDiedHandler` BEFORE disabling them

### Issue: Special attack doesn't spawn enemies
**Cause:** `enemyPrefab` not assigned in Inspector  
**Solution:** Drag enemy prefab to `enemyPrefab` field in GS2_Controller

### Issue: Boss animations not playing
**Cause:** Animator parameters not set correctly  
**Solution:** Check state scripts set `isMoving`, `isIdle`, `moveX/Y`, `idleX/Y` parameters

### Issue: GRS can't hit player
**Cause:** Y-axis misalignment (yHardCap gate)  
**Solution:** Player and boss must be within 0.55 units on Y-axis. Adjust `yHardCap` if needed

### Issue: GS2 spawns enemies off-screen
**Cause:** `spawnSpreadRadius` too large  
**Solution:** Reduce to 2.0-3.0f range

### Issue: Collision damage too frequent
**Cause:** `collisionTick` too low in C_Stats  
**Solution:** Increase `c_Stats.collisionTick` (recommend 0.5-1.0s)

---

## Quick Reference

### File Locations
```
Assets/GAME/Scripts/Enemy/
├── GR_Controller.cs
├── GR_State_Chase.cs
├── GR_State_Attack.cs
├── GRS_Controller.cs
├── GRS_State_Chase.cs
├── GRS_State_Attack.cs
├── GS2_Controller.cs
└── GS2_State_Chase.cs
```

### Shared Components Required
- `Rigidbody2D` (kinematic)
- `Animator` (uses GR animator controller)
- `C_Stats` (HP, damage, speed, collision values)
- `C_Health` (health management, OnDied event)
- `C_FX` (visual effects, fade)
- `State_Idle` (shared idle behavior)
- `State_Wander` (shared wander behavior)
- `Collider2D` (for collision damage)

### Inspector Setup Checklist
- [ ] `playerLayer` set to Player layer
- [ ] `enemyPrefab` assigned (GS2 only)
- [ ] `defaultState` set (Idle or Wander)
- [ ] Detection/attack ranges configured
- [ ] Special attack parameters tuned
- [ ] Phase 2 settings configured (GS2 only)

---

**Status:** ✅ All systems tested and production ready  
**Last Updated:** November 8, 2025  
**Next Steps:** Balance tuning based on playtesting feedback
