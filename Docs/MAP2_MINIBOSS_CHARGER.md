# Map 2 Mini-Boss: Charger Boss Design Plan

**Date:** November 5, 2025  
**Boss Type:** Charge-focused knockback specialist  
**Map:** Level 2  
**Difficulty:** Mini-boss (easier than main boss, harder than elite enemies)

**Animation System:** Clip-length timing (simplified, like State_Attack_Boss)

---

## 🎯 Boss Identity

**Core Concept:** A telegraphed, high-damage charger that punishes poor positioning through massive knockback.

**Playstyle:**
- No close-range attacks (unlike existing boss)
- Always telegraphs before attacking (charge animation)
- High damage + extreme knockback on all attacks
- Player can damage during charge window (risk/reward)

**Counterplay:**
- Dodge during charge animation
- Attack during charge telegraph (risky but rewarding)
- Use knockback to reposition for safety

---

## 📋 Attack Patterns

### **1. Charge Attack (Primary)**
**When:** Player within medium range (2-5 units away)

**Phases:**
1. **Telegraph (1.0s):**
   - Play "Charge" animation (boss glows/winds up)
   - Boss is vulnerable (player can attack)
   - Freezes in place

2. **Dash (0.5s):**
   - Dashes straight toward player's **current position** (not tracked)
   - Speed: Fast (8-10 units/s)
   - Stops at player's position OR max distance (5 units)

3. **Impact:**
   - Damage: High (20-25)
   - Knockback: **Extreme** (3-4 units, double normal)
   - Stuns boss briefly (0.3s recovery)

4. **Cooldown:** 2.5s

---

### **2. Jump Attack (Special)**
**When:** Player at long range (5-8 units away) OR charge attack on cooldown

**Phases:**
1. **Telegraph (1.2s):**
   - Play "Jump Charge" animation (boss crouches)
   - Boss is vulnerable (player can attack)
   - Freezes in place

2. **Leap (0.8s):**
   - Jumps in arc toward player's **last known position**
   - Arc animation (sprite Y offset + scale)
   - Cannot change direction mid-air

3. **Landing:**
   - Lands at target position
   - AoE damage (small radius: 1.5 units)
   - Damage: Medium (15-20)
   - Knockback: **Heavy** (2-3 units)
   - Impact effect (dust cloud sprite)

4. **Cooldown:** 4.0s

---

## 🔄 State Machine

**Reuse:** `B_Controller.cs` structure (Idle → Wander → Chase → Attack)

**States:**
- `State_Idle` (reuse existing)
- `State_Wander` (reuse existing)
- `State_Chase_Charger` (new - simplified chase, no Y-alignment)
- `State_Attack_Charger` (new - handles charge + jump attacks)

**State Transitions:**
```
Idle/Wander → Chase (player in detectionRange: 8 units)
Chase → Attack (player in attackRange: 2-8 units)
Attack → Chase (after attack completes)
```

---

## 🛠️ Implementation Plan

### **Files to Create:**

1. **MB_Controller.cs** (Mini-Boss Controller)
   - Copy `B_Controller.cs` structure
   - Remove special attack complexity
   - Simplify to 2 attacks: Charge + Jump
   - States: Idle, Wander, Chase, Attack

2. **State_Chase_Charger.cs**
   - Copy `State_Chase_Boss.cs`
   - Remove Y-alignment band logic
   - Simple direct chase (no stop-short)
   - Stop at `attackRange` (2 units)

3. **State_Attack_Charger.cs**
   - New file (inspired by `State_Attack_Boss.cs`)
   - Two attack routines:
     - `ChargeRoutine()` - telegraph → dash → impact
     - `JumpRoutine()` - telegraph → leap → landing AoE
   - Attack selection logic:
     - Distance 2-5 units → Charge Attack
     - Distance 5-8 units OR charge on cooldown → Jump Attack

---

### **Code Reuse Strategy:**

**From B_Controller.cs:**
- ✅ State enum structure
- ✅ Detection/attack range logic
- ✅ `SetDesiredVelocity()` interface
- ✅ Gizmo drawing (detection/attack ranges)

**From State_Attack_Boss.cs:**
- ✅ Coroutine-based attack flow (telegraph → action → cooldown)
- ✅ `IsAttacking` flag
- ✅ Animator parameter management (`isAttacking`, `isCharging`, `isJumping`)
- ✅ Weapon attack trigger (reuse `W_Base.Attack()`)
- ❌ **Remove:** Special attack complexity, afterimage, dash stopping logic

**From State_Chase_Boss.cs:**
- ✅ Basic chase movement
- ✅ `SetTarget()` and `SetRanges()` methods
- ❌ **Remove:** Y-alignment band, stop-short offset, special reach calculation

**From C_Health.cs:**
- ✅ Knockback application (`ApplyDamage()` already supports knockback parameter)
- ✅ Just pass higher knockback values (3.0f for charge, 2.5f for jump)

---

## ⚙️ Technical Details

### **Damage & Knockback Values:**

| Attack | Damage | Knockback | Stun |
|--------|--------|-----------|------|
| Charge | 20-25  | 3.5 units | 0.3s (self) |
| Jump   | 15-20  | 2.5 units | 0.2s (self) |

**Comparison to existing boss:**
- Existing boss normal attack: ~10 damage, 1.0 knockback
- Charger boss: 2x damage, 3x knockback (high risk/reward)

---

### **Animation Requirements:**

**New Animations Needed:**
1. **Charge Telegraph** (1.0s loop)
   - Boss winds up, leans back
   - Particle effect (optional: charge glow)

2. **Charge Dash** (0.5s once)
   - Boss lunges forward
   - Motion blur sprite (optional)

3. **Jump Charge** (1.2s loop)
   - Boss crouches low
   - Anticipation pose

4. **Jump Leap** (0.8s once)
   - Jump arc (use Transform.position.y offset)
   - Midair sprite

5. **Jump Landing** (0.3s once)
   - Impact pose
   - Dust cloud effect sprite

**Reuse Existing:**
- Idle, Wander, Chase animations (standard enemy anims)
- Hurt/Death animations

---

### **Animator Parameters:**

```csharp
// Movement (reuse existing)
moveX, moveY, idleX, idleY

// Attack states (new)
isCharging (bool) - triggers charge telegraph
isJumping (bool)  - triggers jump telegraph
```

---

### **Collision & Hitboxes:**

**Charge Attack:**
- Enable boss's main `Collider2D` during dash
- Use `OnTriggerEnter2D` to detect player hit
- Apply damage + knockback on first contact
- Disable further hits until next charge (prevent multi-hit)

**Jump Attack:**
- Create temporary AoE trigger at landing position
- Radius: 1.5 units (use `Physics2D.OverlapCircle`)
- Apply damage + knockback to all targets in radius
- Destroy AoE after 0.1s

---

## 🎮 Gameplay Flow Example

**Scenario: Player at 6 units away**

1. Boss detects player → `Chase` state
2. Boss moves toward player (simple direct chase)
3. Player reaches 6 units → `Attack` state triggered
4. Boss selects **Jump Attack** (long range)
5. **Telegraph Phase (1.2s):**
   - Boss plays "Jump Charge" animation
   - Player sees windup, decides to dodge or attack
6. **Leap Phase (0.8s):**
   - Boss jumps toward player's position at start of telegraph
   - Player dodges away
7. **Landing:**
   - Boss lands, AoE triggers
   - Player outside radius → no damage
8. **Recovery:**
   - Boss stunned 0.2s
   - Returns to `Chase` state
9. **Repeat** after 4s cooldown

---

## ⏱️ Animation Timing System (Frame-Based)

**Philosophy:** Simple frame counts - just set clip length to match your animation frames.

### **How It Works:**

Use the **exact frame timings** from your animation clips:

```csharp
// Charge Attack: GR_CAtk_L/R animation
chargeClipLength = 2.35f;  // Total: 0f to 2.35f
chargeHitDelay = 2.0f;     // Telegraph: 0f to 2f, then attack at 2f to 2.35f

// Jump Attack: Jump animation
jumpClipLength = 5.0f;     // Total: 0f to 5f
jumpHitDelay = 3.0f;       // Telegraph: 0f to 3f, then jump at 3f to 5f
```

### **Coroutine Flow:**

```csharp
float t = 0f;
while (t < hitDelay) { t += Time.deltaTime; yield return null; } // Telegraph phase
BeginAttack();
while (t < clipLength) { t += Time.deltaTime; yield return null; } // Attack phase
```

### **Benefits:**

✅ **Direct frame mapping:** Animation shows 2.35f? Set `chargeClipLength = 2.35f`  
✅ **No fps math:** Don't worry about 30fps/60fps conversions  
✅ **Self-syncing:** Code automatically matches animation length  
✅ **Easy tweaking:** Adjust animation? Just update the clip length value

### **C_AfterimageSpawner Integration:**

**Jump attack only** uses afterimage (charge doesn't need it):

```csharp
float jumpPhaseTime = jumpClipLength - jumpHitDelay; // 5f - 3f = 2f
if (afterimage && sr)
    afterimage.StartBurst(jumpPhaseTime, sr.sprite, sr.flipX, sr.flipY);
```

Afterimage duration auto-calculated from clip timings.

---

## 🔧 Configuration (Inspector Settings)

**MB_Controller.cs:**
```
detectionRange = 8.0
attackRange = 2.0 (minimum charge distance)
maxAttackRange = 8.0 (jump attack trigger)
attackStartBuffer = 0.2
```

**State_Attack_MBlv2.cs:**
```
// Charge Attack (no cooldown - normal attack)
// Animation: GR_CAtk_L/R (0f to 2.35f)
chargeClipLength = 2.35     // Total animation length
chargeHitDelay = 2.0        // Telegraph (0f to 2f), attack starts at 2f
chargeDashSpeed = 9.0
chargeDashMaxDist = 5.0
chargeDamage = 22
chargeKnockback = 3.5

// Jump Attack (5s cooldown - special attack)
// Animation: Jump (0f to 5f)
jumpClipLength = 5.0        // Total animation length
jumpHitDelay = 3.0          // Telegraph (0f to 3f), jump starts at 3f
jumpAoERadius = 1.5
jumpDamage = 18
jumpKnockback = 2.5
jumpCooldown = 5.0
jumpIdleTime = 2.0          // Vulnerable window after landing
```

**Required Components:**
- `C_AfterimageSpawner` on sprite child (for jump attack visual)

---

## ✅ Success Criteria

**Boss feels fair when:**
- ✅ Player has 1+ second to react to all attacks (telegraph windows)
- ✅ High damage is balanced by predictability
- ✅ Knockback creates dynamic positioning (not just annoying)

**Boss is challenging when:**
- ✅ Player must choose: dodge safely or attack during telegraph (risk)
- ✅ Knockback can push player into hazards (environmental interaction)
- ✅ Two attack patterns require different counterplay

**Code is maintainable when:**
- ✅ Reuses existing boss patterns (B_Controller structure)
- ✅ Simple attack selection logic (distance-based)
- ✅ No complex timing calculations (unlike existing boss's dash stopping)

---

## 📝 Next Steps (When Coding)

1. **Create MB_Controller.cs:**
   - Copy `B_Controller.cs`
   - Rename states to Charger variants
   - Simplify attack decision logic (distance only)

2. **Create State_Chase_Charger.cs:**
   - Copy `State_Chase_Boss.cs`
   - Remove Y-alignment, stop-short
   - Keep simple chase toward player

3. **Create State_Attack_Charger.cs:**
   - New file inspired by `State_Attack_Boss.cs`
   - Implement `ChargeRoutine()` coroutine
   - Implement `JumpRoutine()` coroutine
   - Distance-based attack selection in `Update()`

4. **Test & Balance:**
   - Adjust telegraph times for fairness
   - Tune knockback values (not too frustrating)
   - Verify charge dash stops at max distance

---

## 🎯 Key Design Decisions

**Why no close-range attack?**
- Differentiates from existing boss (variety)
- Forces player to engage at mid-range (more dynamic)
- Simplifies AI (fewer attack types = easier to balance)

**Why always telegraph?**
- Fair counterplay (player can always react)
- Allows "punish window" (attack during windup)
- Teaches timing/pattern recognition

**Why extreme knockback?**
- Boss identity (knockback specialist)
- Creates environmental hazards synergy (knock into traps)
- Makes dodging feel impactful (high stakes)

**Why reuse B_Controller structure?**
- Proven architecture (already works)
- Less debugging needed
- Consistent with existing boss patterns

---

**End of Design Plan**

Ready to implement when you say the word! 🚀
