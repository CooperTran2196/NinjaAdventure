# ⚔️ Weapon System Guide

Complete guide to weapon mechanics, anchoring, movement penalties, and animations.

---

## Overview

The weapon system consists of:
1. **Anchoring** - Weapons follow player during attacks
2. **Movement Penalties** - Per-weapon speed reduction during attacks
3. **ShowTime Lock** - Animation freeze until attack completes
4. **Sprite Configuration** - Bottom-pivot setup for combo system

---

## 1. Weapon Anchoring

### Problem:
Weapons stayed in place when player moved during long attacks (thrust, heavy swings).

### Solution:
**Parent-based anchoring** - weapons parent to owner during attacks.

### Implementation:

```csharp
// W_Base.cs - BeginVisual()
protected void BeginVisual(Vector3 localPos, float angle, bool enableHitbox)
{
    // Parent to owner so weapon follows player automatically
    transform.SetParent(owner, false);
    
    transform.localPosition = localPos;
    transform.localRotation = Quaternion.Euler(0, 0, angle);
    
    sprite.enabled = true;
    hitbox.enabled = enableHitbox;
}

// W_Base.cs - EndVisual()
protected void EndVisual()
{
    sprite.enabled = false;
    hitbox.enabled = false;
    
    // Restore original parent hierarchy
    transform.SetParent(originalParent, true);
}
```

### Benefits:
- ✅ Weapon follows player automatically
- ✅ Works with all attack types (thrust, arc slash)
- ✅ No complex position tracking needed
- ✅ Clean hierarchy management

---

## 2. Per-Weapon Movement Penalties

### Problem:
All weapons had same movement penalty during attacks - daggers felt as slow as greatswords.

### Solution:
Move `attackMovePenalty` from `C_Stats` to `W_SO` (weapon data).

### Implementation:

**W_SO.cs:**
```csharp
[Header("Melee timings + Thrust Distance")]
public float showTime = 0.3f;
[Range(0f, 1f)]
public float attackMovePenalty = 0.5f;  // 0.5 = 50% speed

// Combo system overrides this with per-attack penalties:
public float[] comboMovePenalties = { 0.6f, 0.5f, 0.3f };
```

**P_State_Movement.cs:**
```csharp
void Update()
{
    // Get penalty from attack state (combo-aware)
    float comboPenalty = attackState != null && attackState.enabled
        ? attackState.GetCurrentMovePenalty()
        : 1f;
    
    speed *= comboPenalty;
}
```

**P_State_Attack.cs:**
```csharp
public float GetCurrentMovePenalty()
{
    if (!enabled || activeWeapon == null || activeWeapon.weaponData == null)
        return 1f;
    
    // Return combo-specific penalty
    return activeWeapon.weaponData.comboMovePenalties[comboIndex];
}
```

### Weapon Presets:

| Weapon Type | Penalty | Feel |
|-------------|---------|------|
| Dagger | 0.8 | Very mobile |
| Sword | 0.5 | Balanced |
| Greatsword | 0.3 | Slow, committed |
| Spear | 0.6 | Mobile thrust |

### Combo Penalties:
- Slash Down: 60% speed (still agile)
- Slash Up: 50% speed (more committed)
- Thrust: 30% speed (fully committed finisher)

---

## 3. ShowTime Animation Lock

### Problem:
Animations looped or ended before weapon `showTime` completed, breaking visual sync.

### Solution:
**Freeze animation** at final frame until showTime completes.

### Implementation:

**P_State_Attack.cs:**
```csharp
IEnumerator AttackRoutine()
{
    // ... attack setup ...
    
    // Get animation length
    AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
    float animDuration = stateInfo.length;
    float showTime = activeWeapon.weaponData.comboShowTimes[comboIndex];
    
    if (showTime > animDuration)
    {
        // Wait for animation to finish
        yield return new WaitForSeconds(animDuration);
        
        // FREEZE at final frame
        anim.speed = 0f;
        
        // Wait remaining showTime
        yield return new WaitForSeconds(showTime - animDuration);
        
        // Restore normal speed
        anim.speed = 1f;
    }
    else
    {
        // ShowTime shorter than animation - just wait
        yield return new WaitForSeconds(showTime);
    }
    
    // ... cleanup ...
}
```

### Benefits:
- ✅ Visual sync: weapon visible matches animation
- ✅ No animation loops during long attacks
- ✅ Works with any showTime value
- ✅ Smooth animation resume after freeze

---

## 4. Sprite Configuration

### Bottom-Pivot Requirement:

**For combo arc slashing**, weapon sprites **MUST** have pivot at bottom.

### Unity Setup:

1. Select weapon sprite in Project
2. Open Sprite Editor
3. Set Pivot → "Bottom" or `(0.5, 0)`
4. Apply

### Visual Guide:

```
┌─────────────┐
│   ╱╲  ← Blade (top)
│   ││
│   ││  ← Shaft
│   ││
│  ╱══╲ ← Handle (bottom)
│   ●   ← PIVOT (0.5, 0)
└─────────────┘
```

### Why Bottom Pivot?

```
Bottom Pivot (Correct):
      🗡️ Blade extends outward
      |
      ● Handle ← Positioned at offsetRadius
     ⭕ Player

Center Pivot (Wrong):
      Blade ↗
      ● Pivot
      Handle ↘ (goes opposite direction!)
     ⭕ Player
```

---

## Weapon Data Structure (W_SO)

### Core Fields:

```csharp
[Header("Common")]
public string id = "weaponId";
public WeaponType type = WeaponType.Melee;
public Sprite sprite;
public bool pointsUp = true;             // Sprite orientation
public float offsetRadius = 0.7f;        // Handle offset from player

[Header("Damage")]
public int AD = 1;  // Attack Damage
public int AP = 0;  // Ability Power

[Header("Impact")]
public float knockbackForce = 5f;
public float stunTime = 0.5f;

[Header("Melee Timings")]
public float showTime = 0.3f;
public float thrustDistance = 0.25f;
[Range(0f, 1f)]
public float attackMovePenalty = 0.5f;   // Base penalty (if not using combo)

[Header("Combo System")]
[Range(15f, 90f)]
public float slashArcDegrees = 45f;      // Arc coverage
public float[] comboShowTimes = { 0.3f, 0.3f, 0.5f };
public float[] comboDamageMultipliers = { 1.0f, 1.2f, 2.0f };
public float[] comboMovePenalties = { 0.6f, 0.5f, 0.3f };
public float[] comboStunTimes = { 0.1f, 0.2f, 0.5f };
public bool onlyThrustKnocksBack = true; // Only finisher knocks back
```

---

## W_Base Helper Methods

### Positioning:

```csharp
// Get position at offsetRadius along direction (LOCAL space)
protected Vector3 GetPolarPosition(Vector2 attackDir) =>
    (Vector3)(attackDir * weaponData.offsetRadius);

// Get rotation angle for weapon
protected float GetPolarAngle(Vector2 attackDir)
{
    return Vector2.SignedAngle(Vector2.up, attackDir);
}
```

### Movement Coroutines:

```csharp
// Thrust: forward/back along direction
protected IEnumerator ThrustOverTime(Vector2 dir, float showTime, float thrustDist)
{
    float t = 0f;
    Vector3 start = transform.localPosition - (Vector3)(dir * (thrustDist * 0.5f));
    Vector3 end = transform.localPosition + (Vector3)(dir * (thrustDist * 0.5f));

    while (t < showTime)
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / showTime);
        transform.localPosition = Vector3.Lerp(start, end, k);
        yield return null;
    }
}

// Arc Slash: sweep in arc like radar arm (requires bottom pivot!)
protected IEnumerator ArcSlashOverTime(Vector2 attackDir, float startAngleDeg, 
                                       float endAngleDeg, float duration)
{
    float t = 0f;
    
    while (t < duration)
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);
        
        float currentAngleDeg = Mathf.Lerp(startAngleDeg, endAngleDeg, k);
        float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;
        
        // Position: radar arm at angle (negated X for correct direction)
        float radius = weaponData.offsetRadius;
        Vector3 circularPosition = new Vector3(
            -Mathf.Sin(currentAngleRad) * radius,
            Mathf.Cos(currentAngleRad) * radius,
            0f
        );
        
        transform.localPosition = circularPosition;
        transform.localRotation = Quaternion.Euler(0, 0, currentAngleDeg);
        
        yield return null;
    }
}
```

---

## Troubleshooting

### Weapon floats in place when moving:
**Fix:** Already implemented - weapons parent to owner during attacks

### All weapons feel same speed:
**Fix:** Adjust `attackMovePenalty` per weapon (0.3-0.8 range)

### Animation loops during long attacks:
**Fix:** Already implemented - animation freezes at final frame

### Weapon rotates around center:
**Fix:** Set sprite pivot to bottom (0.5, 0)

### Handle covered by player sprite:
**Fix:** Increase `offsetRadius` (try 0.7-1.0)

---

## Integration Points

### State System:
- `P_State_Attack` queries weapon for movement penalty
- `P_State_Movement` applies penalty to speed
- `P_Controller` manages state transitions

### Animation System:
- Freeze animation when showTime > animation length
- Restore speed after attack completes

### Combat System:
- Combo system uses weapon data arrays
- Hit detection uses weapon hitbox
- Damage/stun/knockback from weapon stats

---

## Best Practices

### Creating New Weapons:

1. **Create W_SO asset** in Unity (right-click → Create → Weapon SO)
2. **Configure stats** (damage, showTime, penalties)
3. **Set sprite** with proper pivot (bottom for melee)
4. **Tune offsetRadius** for visual feel
5. **Adjust arc degrees** (daggers 30°, swords 45°, greatswords 90°)
6. **Test in Play Mode**
7. **Iterate on penalties** until feel is right

### Tuning Guidelines:

**Fast Weapons (Daggers):**
- Low showTime (0.2-0.3s)
- High movement penalty (0.7-0.8)
- Small arc (30°)
- Low damage multipliers

**Standard Weapons (Swords):**
- Medium showTime (0.3-0.5s)
- Medium penalty (0.5-0.6)
- Medium arc (45°)
- Balanced multipliers

**Heavy Weapons (Greatswords):**
- High showTime (0.5-0.8s)
- Low penalty (0.3-0.4)
- Large arc (60-90°)
- High damage multipliers

---

**Status:** ✅ All Systems Implemented  
**Version:** Final
