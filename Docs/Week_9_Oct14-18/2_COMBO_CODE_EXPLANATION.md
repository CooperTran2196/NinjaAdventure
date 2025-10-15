# 🔍 Combo System Code Deep Dive

**Purpose:** Detailed explanation of how the combo system works internally

---

## 📊 Overview: Old vs New

### Old System (Pre-Combo)
- **Single attack pattern:** Simple thrust forward
- **No combo chaining:** Each attack independent
- **Simple movement:** Weapon thrusts along attack direction
- **No damage scaling:** Same damage every attack

### New System (Current)
- **3-hit combo chain:** Slash Down → Slash Up → Thrust
- **Combo tracking:** Attack state manages combo progression
- **Radar rotation:** Weapon sweeps in arc around player
- **Progressive scaling:** Damage/stun increases per combo stage

---

## 🎯 Part 1: ArcSlashOverTime Explained

### Location
`W_Base.cs` lines 126-164

### Purpose
Animates weapon sweeping in an arc around the player (like a radar arm rotating).

### Full Code with Line-by-Line Breakdown

```csharp
protected IEnumerator ArcSlashOverTime(Vector2 attackDir, float startAngleDeg, float endAngleDeg, float duration)
{
    float t = 0f;
    
    // Animation loop - runs every frame for 'duration' seconds
    while (t < duration)
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);  // k = 0→1 over duration
        
        // ═══════════════════════════════════════════════════════════
        // STEP 1: Calculate current angle (interpolate from start to end)
        // ═══════════════════════════════════════════════════════════
        float currentAngleDeg = Mathf.Lerp(startAngleDeg, endAngleDeg, k);
        // Example: startAngleDeg=45°, endAngleDeg=135°, k=0.5
        //          → currentAngleDeg = 90° (halfway through arc)
        
        float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;
        // Convert to radians for Sin/Cos functions
        
        // ═══════════════════════════════════════════════════════════
        // STEP 2: Calculate position on circular arc
        // ═══════════════════════════════════════════════════════════
        // COORDINATE SYSTEM:
        //   - UP is 0° (player facing up)
        //   - RIGHT is 90°
        //   - DOWN is 180°
        //   - LEFT is 270°
        //
        // POLAR TO CARTESIAN CONVERSION (with UP as 0°):
        //   x = -r * sin(angle)  ← NEGATED for correct left/right
        //   y =  r * cos(angle)
        //
        // WHY NEGATE X?
        //   - Standard math: 90° → x=+1 (right)
        //   - Unity with UP baseline: 90° → x should be -1 (right in our coord)
        //   - Negation fixes this mismatch
        
        float radius = weaponData.offsetRadius;  // Distance from player center
        
        Vector3 circularPosition = new Vector3(
            -Mathf.Sin(currentAngleRad) * radius,  // X: NEGATED
            Mathf.Cos(currentAngleRad) * radius,   // Y: Normal
            0f
        );
        
        // Example with offsetRadius=0.7, angle=90° (right):
        //   x = -sin(90°) * 0.7 = -1.0 * 0.7 = -0.7  ← Correct!
        //   y =  cos(90°) * 0.7 =  0.0 * 0.7 =  0.0
        //   → Weapon positioned to the RIGHT of player
        
        // ═══════════════════════════════════════════════════════════
        // STEP 3: Apply position (weapon handle at calculated point)
        // ═══════════════════════════════════════════════════════════
        transform.localPosition = circularPosition;
        // LOCAL position because weapon is parented to owner
        // Owner moves → weapon automatically follows
        
        // ═══════════════════════════════════════════════════════════
        // STEP 4: Apply rotation (weapon points outward from center)
        // ═══════════════════════════════════════════════════════════
        transform.localRotation = Quaternion.Euler(0, 0, currentAngleDeg);
        
        // WHY SO SIMPLE?
        //   - Weapon sprite points UP (0°)
        //   - Pivot at BOTTOM (handle)
        //   - Rotating to angle X makes weapon point in direction X
        //   - No bias/offset needed! (old system needed angleBiasDeg)
        
        yield return null;  // Wait one frame, then loop
    }
}
```

### Visual Example

```
Player at center, attacking RIGHT (90°):

Frame 1 (k=0.0, angle=45°):
         Blade
           ╱
          ╱
    Handle●  ← Player

Frame 2 (k=0.5, angle=90°):
    Blade
      |
      |
      Handle● ← Player

Frame 3 (k=1.0, angle=135°):
      Blade
       ╲
        ╲
         ●Handle ← Player
```

### Key Insights

1. **Radar Rotation:** Handle orbits at fixed radius, blade extends outward
2. **Negated X:** Fixes coordinate system mismatch (Unity vs math convention)
3. **Bottom Pivot:** Makes rotation simple (no complex offset calculations)
4. **Local Space:** Weapon parented to owner → automatic player following
5. **Smooth Arc:** Linear interpolation creates smooth sweep motion

---

## ⚔️ Part 2: W_Melee Hit() Comparison

### Old Hit() (Pre-Combo)

```csharp
IEnumerator Swing()
{
    hitThisSwing.Clear();  // Reset hit tracking
    
    Vector2 rawDir = GetRawAimDir();  // Get aim direction
    LockAttackFacing(rawDir);         // Lock animator facing
    
    // Calculate position and angle
    var a = AimDirection();
    Vector3 pos = owner.position + (Vector3)a.offset;
    
    // Show weapon
    BeginVisual(pos, a.angleDeg, enableHitbox: true);
    
    // SINGLE ATTACK PATTERN: Thrust forward
    yield return ThrustOverTime(a.dir, data.showTime, data.thrustDistance);
    
    // Hide weapon
    if (hitbox) hitbox.enabled = false;
    if (sprite) sprite.enabled = false;
}
```

**Characteristics:**
- ❌ No combo support
- ❌ Always thrusts forward
- ❌ Fixed damage
- ❌ Simple position calculation
- ✅ Clean and simple

---

### New Hit() (Current with Combo)

```csharp
IEnumerator Hit()
{
    // ═══════════════════════════════════════════════════════════
    // SETUP: Clear hit tracking
    // ═══════════════════════════════════════════════════════════
    alreadyHit.Clear();
    
    // ═══════════════════════════════════════════════════════════
    // COMBO SUPPORT: Get combo-specific configuration
    // ═══════════════════════════════════════════════════════════
    float showTime = weaponData.comboShowTimes[currentComboIndex];
    // currentComboIndex set in Attack() method (0, 1, or 2)
    // showTime varies per combo attack for animation pacing
    
    // ═══════════════════════════════════════════════════════════
    // ANGLE CALCULATION: Base direction for attack
    // ═══════════════════════════════════════════════════════════
    float baseAngle = Mathf.Atan2(-attackDir.x, attackDir.y) * Mathf.Rad2Deg;
    // NEGATED X here too! Matches ArcSlashOverTime coordinate system
    // Example: attackDir = (1, 0) [RIGHT]
    //          → atan2(-1, 0) = -90° → *Rad2Deg = 90° ✓
    
    // ═══════════════════════════════════════════════════════════
    // COMBO PATTERN SELECTION: Switch based on combo index
    // ═══════════════════════════════════════════════════════════
    switch (currentComboIndex)
    {
        // ───────────────────────────────────────────────────────
        // COMBO 0: Slash Down (clockwise arc)
        // ───────────────────────────────────────────────────────
        case 0:
            {
                float halfArc = weaponData.slashArcDegrees * 0.5f;  // e.g., 90° / 2 = 45°
                float startAngle = baseAngle - halfArc;  // e.g., 90° - 45° = 45°
                float endAngle = baseAngle + halfArc;    // e.g., 90° + 45° = 135°
                // Arc sweeps: 45° → 90° → 135° (downward motion when attacking right)
                
                // Calculate START position (for instant weapon appearance)
                float startAngleRad = startAngle * Mathf.Deg2Rad;
                Vector3 startPos = new Vector3(
                    -Mathf.Sin(startAngleRad) * weaponData.offsetRadius,  // X: NEGATED
                    Mathf.Cos(startAngleRad) * weaponData.offsetRadius,   // Y: Normal
                    0f
                );
                // CRITICAL: Must negate X here to match ArcSlashOverTime calculation
                // Otherwise weapon would "jump" when animation starts
                
                BeginVisual(startPos, startAngle, enableHitbox: true);
                yield return ArcSlashOverTime(attackDir, startAngle, endAngle, showTime);
            }
            break;
            
        // ───────────────────────────────────────────────────────
        // COMBO 1: Slash Up (counter-clockwise arc - REVERSED)
        // ───────────────────────────────────────────────────────
        case 1:
            {
                float halfArc = weaponData.slashArcDegrees * 0.5f;
                float startAngle = baseAngle + halfArc;  // SWAPPED: Start high
                float endAngle = baseAngle - halfArc;    // End low
                // Arc sweeps: 135° → 90° → 45° (upward motion - opposite of case 0)
                
                float startAngleRad = startAngle * Mathf.Deg2Rad;
                Vector3 startPos = new Vector3(
                    -Mathf.Sin(startAngleRad) * weaponData.offsetRadius,
                    Mathf.Cos(startAngleRad) * weaponData.offsetRadius,
                    0f
                );
                
                BeginVisual(startPos, startAngle, enableHitbox: true);
                yield return ArcSlashOverTime(attackDir, startAngle, endAngle, showTime);
            }
            break;
            
        // ───────────────────────────────────────────────────────
        // COMBO 2: Thrust Finisher (forward thrust - like old system)
        // ───────────────────────────────────────────────────────
        case 2:
        default:
            {
                Vector3 localPosition = GetPolarPosition(attackDir);
                float thrustAngle = GetPolarAngle(attackDir);
                
                BeginVisual(localPosition, thrustAngle, enableHitbox: true);
                yield return ThrustOverTime(attackDir, showTime, weaponData.thrustDistance);
                // Same as old system - simple thrust forward
            }
            break;
    }
    
    // ═══════════════════════════════════════════════════════════
    // CLEANUP: Hide weapon (EndVisual also restores parent)
    // ═══════════════════════════════════════════════════════════
    EndVisual();
}
```

**Characteristics:**
- ✅ Combo support (3 different patterns)
- ✅ Arc rotation for slashes
- ✅ Variable showTime per attack
- ✅ Damage scaling (handled in ApplyHitEffects)
- ✅ Negated X coordinate consistency
- ⚠️ More complex (but modular)

---

## 🔄 Part 3: Hit Detection Comparison

### Old OnTriggerStay2D

```csharp
void OnTriggerStay2D(Collider2D other)
{
    var (targetHealth, root) = TryGetTarget(other);
    if (targetHealth == null) return;
    
    // One hit per swing (single tracking set)
    if (!hitThisSwing.Add(root.GetInstanceID())) return;
    
    // Apply effects (no combo support)
    ApplyHitEffects(c_Stats, data, targetHealth, AimDirection().dir, other);
}
```

---

### New OnTriggerStay2D

```csharp
void OnTriggerStay2D(Collider2D targetCollider)
{
    // ═══════════════════════════════════════════════════════════
    // STEP 1: Validate target
    // ═══════════════════════════════════════════════════════════
    var (targetHealth, root) = TryGetTarget(targetCollider);
    if (!targetHealth) return;
    
    // ═══════════════════════════════════════════════════════════
    // STEP 2: Check weapon-level hit tracking
    // ═══════════════════════════════════════════════════════════
    if (!alreadyHit.Add(root.GetInstanceID())) return;
    // Prevents hitting same enemy multiple times in ONE ATTACK
    // (e.g., if they stay in hitbox during arc sweep)
    
    // ═══════════════════════════════════════════════════════════
    // STEP 3: Check attack state hit tracking (player only)
    // ═══════════════════════════════════════════════════════════
    var playerAttackState = owner?.GetComponent<P_State_Attack>();
    if (playerAttackState != null)
    {
        // Additional tracking: Prevents hitting same enemy MULTIPLE TIMES WITHIN ONE ATTACK
        // Example: If arc sweeps through Skeleton, only registers 1 hit (not 10 hits)
        // NOTE: This HashSet is CLEARED between combo attacks, so enemy CAN be hit 3 times in full combo
        if (playerAttackState.WasTargetHitThisAttack(targetHealth)) return;
        playerAttackState.MarkTargetHit(targetHealth);
    }
    
    // ═══════════════════════════════════════════════════════════
    // STEP 4: Apply damage with combo scaling
    // ═══════════════════════════════════════════════════════════
    ApplyHitEffects(c_Stats, weaponData, targetHealth, attackDir, targetCollider, currentComboIndex);
    // currentComboIndex passed to scale damage/stun based on combo stage
}
```

**New Features:**
- ✅ **Two-level tracking:** Weapon-level + State-level
- ✅ **Per-attack isolation:** Each combo attack has fresh hit tracking
- ✅ **Multi-hit prevention:** Can't hit same target multiple times in ONE attack
- ✅ **Combo scaling:** Damage/stun increases per stage (enemy gets hit 3 times total in full combo)

---

## 📈 Part 4: Damage Scaling in ApplyHitEffects

### Inside W_Base.ApplyHitEffects()

```csharp
public static void ApplyHitEffects(..., int comboIndex = 0)
{
    int attackerAD = attackerStats.AD;
    int attackerAP = attackerStats.AP;
    
    // ═══════════════════════════════════════════════════════════
    // COMBO DAMAGE SCALING
    // ═══════════════════════════════════════════════════════════
    int baseWeaponAD = weaponData.AD;  // e.g., 10
    int baseWeaponAP = weaponData.AP;  // e.g., 5
    
    float damageMultiplier = weaponData.comboDamageMultipliers[comboIndex];
    // comboIndex 0: multiplier = 1.0  (100% damage)
    // comboIndex 1: multiplier = 1.2  (120% damage)
    // comboIndex 2: multiplier = 1.5  (150% damage)
    
    int weaponAD = Mathf.RoundToInt(baseWeaponAD * damageMultiplier);
    int weaponAP = Mathf.RoundToInt(baseWeaponAP * damageMultiplier);
    // Example: comboIndex=2 → weaponAD = 10 * 1.5 = 15
    
    // Apply damage with scaled values
    int dealtDamage = targetHealth.ApplyDamage(attackerAD, attackerAP, weaponAD, weaponAP, ...);
    
    // ... lifesteal logic ...
    
    // ═══════════════════════════════════════════════════════════
    // COMBO KNOCKBACK (only on thrust finisher)
    // ═══════════════════════════════════════════════════════════
    bool shouldKnockback = !weaponData.onlyThrustKnocksBack || comboIndex == 2;
    // If onlyThrustKnocksBack = true:
    //   Combo 0 (slash down): NO knockback
    //   Combo 1 (slash up): NO knockback
    //   Combo 2 (thrust): KNOCKBACK! 💥
    
    if (shouldKnockback && weaponData.knockbackForce > 0f)
    {
        // Apply knockback via controller
    }
    
    // ═══════════════════════════════════════════════════════════
    // COMBO STUN SCALING
    // ═══════════════════════════════════════════════════════════
    float stunTime = weaponData.comboStunTimes[comboIndex];
    // comboIndex 0: stunTime = 0.1s  (minor stagger)
    // comboIndex 1: stunTime = 0.2s  (longer stagger)
    // comboIndex 2: stunTime = 0.5s  (significant stun)
    
    if (stunTime > 0f)
    {
        // Apply stun via controller
    }
}
```

---

## 🎨 Visual Flow Comparison

### Old System Flow:
```
Attack() called
    ↓
Swing() coroutine
    ↓
Calculate position (simple offset)
    ↓
BeginVisual()
    ↓
ThrustOverTime() ← SINGLE PATTERN
    ↓
EndVisual()
```

### New System Flow:
```
Attack() called
    ↓
Get combo index from P_State_Attack
    ↓
Hit() coroutine
    ↓
Calculate base angle (negated X)
    ↓
Switch (combo index):
    ├─ Case 0: Slash Down
    │   ├─ Calculate arc (start-45° to start+45°)
    │   ├─ Calculate start position (negated X)
    │   ├─ BeginVisual()
    │   └─ ArcSlashOverTime() ← RADAR ROTATION
    │
    ├─ Case 1: Slash Up
    │   ├─ Calculate arc (start+45° to start-45°) [REVERSED]
    │   ├─ Calculate start position (negated X)
    │   ├─ BeginVisual()
    │   └─ ArcSlashOverTime() ← RADAR ROTATION (opposite)
    │
    └─ Case 2: Thrust
        ├─ Calculate position/angle
        ├─ BeginVisual()
        └─ ThrustOverTime() ← SAME AS OLD
    ↓
EndVisual()
```

---

## 🔑 Key Differences Summary

| Aspect | Old System | New System |
|--------|-----------|------------|
| **Attack Patterns** | 1 (thrust) | 3 (slash down, slash up, thrust) |
| **Movement Type** | Linear thrust | Arc rotation + thrust |
| **Combo Support** | None | Full combo chain |
| **Damage** | Fixed | Scaled by combo stage |
| **Stun** | Fixed | Scaled by combo stage |
| **Knockback** | Always | Only on finisher |
| **Hit Tracking** | Single set | Two-level (weapon + state) |
| **Coordinate System** | Standard | Negated X (UP baseline) |
| **Sprite Requirement** | Any pivot | Bottom pivot (handle) |
| **Code Complexity** | Simple | Modular & extensible |

---

## 💡 Why Negated X Everywhere?

**Problem:** Unity's coordinate system vs trigonometry conventions

**Standard Math (0° = RIGHT):**
```
     90° (up)
      |
270°--+--0° (RIGHT)
      |
    180° (down)
```

**Our System (0° = UP for sprite pointing up):**
```
      0° (UP) ↑
          |
          |
270° ←----●----→ 90° (RIGHT)
 (LEFT)   |
          |
        180° (DOWN) ↓
```

**Conversion:**
- Standard Math: `x = r*cos(θ), y = r*sin(θ)` (0° points RIGHT)
- Our System: `x = -r*sin(θ), y = r*cos(θ)` (0° points UP) ← Negated X!

**Key Examples:**
- **0° = UP** → Position: `(0, 1)` → Weapon above player ↑
- **90° = RIGHT** → Position: `(-1, 0)` → Weapon to right of player → (negated X!)
- **180° = DOWN** → Position: `(0, -1)` → Weapon below player ↓
- **270° = LEFT** → Position: `(1, 0)` → Weapon to left of player ←

**Why it matters:**
- Angle calculation: `atan2(-x, y)` → correct angle from direction
- Position calculation: `(-sin(θ), cos(θ))` → correct position on circle
- **Consistency is critical** - both must use negated X to match coordinate systems

**See:** `COORDINATE_SYSTEM_REFERENCE.md` for detailed visual diagrams

---

## 🧪 Testing the System

### Test 1: Single Attack (No Combo)
```
1. Attack once
2. Wait for attack to finish
3. Expected: Slash Down (combo 0)
4. Verify: Arc sweeps downward
```

### Test 2: Full Combo Chain
```
1. Attack → Slash Down (0)
2. Attack quickly → Slash Up (1)
3. Attack quickly → Thrust (2)
4. Expected: Smooth combo flow
5. Verify: Damage increases per hit
```

### Test 3: Hit Tracking
```
1. Start combo on enemy
2. Enemy stays in hitbox during arc
3. Expected: Only 1 hit registered
4. Verify: alreadyHit prevents double-hit
```

### Test 4: Cross-Combo Tracking
```
1. Hit enemy with Slash Down (0)
2. Continue combo → Slash Up (1)
3. Expected: Enemy NOT hit again
4. Verify: P_State_Attack tracking works
```

---

**Status:** ✅ Complete Implementation  
**Complexity:** Moderate (well-documented)  
**Maintainability:** High (modular design)
