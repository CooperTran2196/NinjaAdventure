# Attack Move Penalty - Per-Weapon Configuration

## Change Summary
Moved `attackMovePenalty` from **character stats** (`C_Stats`) to **weapon data** (`W_SO`). Now each weapon has its own movement penalty, allowing for more variety in weapon design.

## Rationale
Different weapon types should impose different movement penalties:
- **Light weapons** (daggers, rapiers) → Small penalty, stay mobile
- **Medium weapons** (swords, axes) → Moderate penalty
- **Heavy weapons** (greatswords, hammers) → Large penalty, very slow

This creates more tactical depth and weapon variety.

## Files Modified

### 1. `W_SO.cs` - Added attackMovePenalty
```csharp
[Header("Melee timings + Thrust Distance")]
public float showTime = 0.3f;
public float thrustDistance = 0.25f;
[Range(0f, 1f)]
public float attackMovePenalty = 0.5f; // Movement speed multiplier while attacking
```

**Location:** Under "Melee timings" header  
**Type:** Float with Range slider (0-1)  
**Default:** 0.5 (50% movement speed)

### 2. `C_Stats.cs` - Removed attackMovePenalty
```csharp
// REMOVED:
// [Range(0f, 1f)]
// public float attackMovePenalty = 0.5f;
```

**Impact:** No longer a character-level stat  
**Note:** Existing character prefabs may show "missing reference" warning in Inspector - this is safe to ignore

### 3. `P_State_Attack.cs` - Added Weapon Getter
```csharp
// Public getter for movement state to access current weapon
public W_Base GetActiveWeapon() => activeWeapon;
```

**Purpose:** Allows `P_State_Movement` to read the active weapon's penalty  
**Scope:** Public access

### 4. `P_State_Movement.cs` - Read Penalty from Weapon

#### Added Cache Reference:
```csharp
P_State_Attack attackState;

void Awake()
{
    // ... other references ...
    attackState = GetComponent<P_State_Attack>();
}
```

#### Updated Movement Speed Calculation:
```csharp
void Update()
{
    float speed = c_Stats.MS;
    
    if (controller.currentState == P_Controller.PState.Attack)
    {
        // Get the active weapon and its movement penalty
        W_Base activeWeapon = attackState.GetActiveWeapon();
        if (activeWeapon != null && activeWeapon.weaponData != null)
        {
            speed *= activeWeapon.weaponData.attackMovePenalty;
        }
        
        anim.SetBool("isMoving", false);
    }
    else
    {
        anim.SetBool("isMoving", true);
    }
    
    controller.SetDesiredVelocity(moveAxis * speed);
    // ... animation parameters ...
}
```

## How It Works Now

### Flow:
1. Player attacks with a weapon
2. `P_State_Attack` stores the `activeWeapon` reference
3. `P_State_Movement.Update()` checks if attacking
4. If yes, reads `activeWeapon.weaponData.attackMovePenalty`
5. Applies weapon-specific penalty to movement speed
6. When attack ends, full speed restored

### Example:
```csharp
// Light Dagger
showTime = 0.3f
attackMovePenalty = 0.8f  // 80% speed (very mobile)
→ Fast attacks, stay agile

// Standard Sword
showTime = 0.45f
attackMovePenalty = 0.5f  // 50% speed (balanced)
→ Moderate lockout, moderate mobility

// Heavy Greatsword
showTime = 1.5f
attackMovePenalty = 0.2f  // 20% speed (very slow)
→ Long lockout, almost immobile
```

## Weapon Archetypes

### Fast & Mobile (Assassin Style):
```
Weapon: Dual Daggers
showTime: 0.25f
attackMovePenalty: 0.8f
thrustDistance: 0.15f
AD: 2
→ Quick hits, stay mobile, hit-and-run
```

### Balanced (Knight Style):
```
Weapon: Longsword
showTime: 0.5f
attackMovePenalty: 0.5f
thrustDistance: 0.3f
AD: 5
→ Standard combat, moderate everything
```

### Slow & Powerful (Tank Style):
```
Weapon: Greatsword
showTime: 1.5f
attackMovePenalty: 0.2f
thrustDistance: 0.4f
AD: 15
→ Devastating hits, committed attacks, positioning critical
```

### Ranged & Mobile (Archer Style):
```
Weapon: Bow
showTime: 0.6f
attackMovePenalty: 0.7f
projectileSpeed: 10f
AD: 4
→ Kite enemies, stay at range, mobile
```

### Ranged & Slow (Mage Style):
```
Weapon: Staff (magic projectiles)
showTime: 1.2f
attackMovePenalty: 0.3f
projectileSpeed: 8f
AP: 10
→ Powerful spells, long cast time, need protection
```

## Movement Speed Calculation

```
Base Movement Speed: MS = 5.0
Weapon: Greatsword (attackMovePenalty = 0.2)

Not Attacking:
  speed = 5.0 × 1.0 = 5.0 units/sec

Attacking:
  speed = 5.0 × 0.2 = 1.0 units/sec (very slow)

Attack Ends:
  speed = 5.0 × 1.0 = 5.0 units/sec (full speed restored)
```

## Penalty Scale Guidelines

| Penalty Value | Speed % | Feel | Weapon Type |
|---------------|---------|------|-------------|
| 1.0 | 100% | No penalty | None (don't use) |
| 0.9 | 90% | Barely noticeable | Ultra-light (throwing knives) |
| 0.8 | 80% | Very mobile | Light (daggers, rapiers) |
| 0.7 | 70% | Mobile | Light-medium (shortswords, bows) |
| 0.6 | 60% | Moderate-mobile | Medium (swords, spears) |
| 0.5 | 50% | Balanced | Medium-heavy (axes, maces) |
| 0.4 | 40% | Sluggish | Heavy (warhammers) |
| 0.3 | 30% | Very sluggish | Very heavy (greatswords) |
| 0.2 | 20% | Almost immobile | Ultra heavy (ultra greatswords) |
| 0.1 | 10% | Nearly stuck | Siege weapons |
| 0.0 | 0% | Completely locked | Old system (not recommended) |

## Design Benefits

### ✅ Weapon Variety
Each weapon feels mechanically different, not just visually

### ✅ Tactical Depth
- Fast weapons = chase enemies, escape danger
- Slow weapons = need positioning, high risk/high reward

### ✅ Build Diversity
- Assassin builds → fast weapons + mobility
- Tank builds → slow weapons + armor
- Hybrid builds → medium weapons + versatility

### ✅ Per-Weapon Tuning
Can balance individual weapons without affecting character stats

### ✅ Player Choice
Visual AND mechanical differences between weapons

## Balancing Framework

When designing a new weapon, consider the **risk/reward** triangle:

```
      Damage
       /\
      /  \
     /    \
    /      \
Speed ---- Mobility
(showTime) (attackMovePenalty)
```

**High damage** → Should have either:
- Long showTime (slow attacks), OR
- High penalty (immobile during attack), OR
- Both

**Low damage** → Should have:
- Short showTime (fast attacks), AND/OR
- Low penalty (mobile during attack)

### Examples:
- **Dagger**: Low damage, fast attacks, high mobility ✅
- **Greatsword**: High damage, slow attacks, low mobility ✅
- **Dagger with high damage & mobility**: ❌ Overpowered
- **Greatsword with low damage & mobility**: ❌ Useless

## Migration Notes

### Existing Weapon ScriptableObjects:
Need to set `attackMovePenalty` in Inspector:
1. Open weapon SO asset
2. Find "Melee timings + Thrust Distance" section
3. Set `attackMovePenalty` slider (default 0.5)
4. Save

### Recommended Starting Values:
- Small/fast weapons: 0.7 - 0.8
- Medium weapons: 0.5 - 0.6
- Large/slow weapons: 0.2 - 0.4

### Character Prefabs:
May show warning about removed `attackMovePenalty` from `C_Stats`:
- This is safe to ignore
- The field is no longer used
- Movement penalty now comes from weapon

## Code Safety

### Null Checks:
```csharp
W_Base activeWeapon = attackState.GetActiveWeapon();
if (activeWeapon != null && activeWeapon.weaponData != null)
{
    speed *= activeWeapon.weaponData.attackMovePenalty;
}
```

**Protection against:**
- No weapon equipped ✅
- Weapon SO not assigned ✅
- Attack state not initialized ✅

### Fallback Behavior:
If weapon data is missing, penalty is NOT applied (full speed). This is safer than applying a default penalty.

## Testing Checklist

- [ ] Create fast weapon (penalty 0.8) - should move quickly while attacking
- [ ] Create slow weapon (penalty 0.2) - should barely move while attacking
- [ ] Switch weapons mid-game - penalty should change
- [ ] Attack with no weapon equipped - should not crash (null check)
- [ ] Attack with weapon missing SO data - should not crash (null check)
- [ ] Verify different weapons feel different
- [ ] Balance damage vs mobility for each weapon

## Future Enhancements

### Optional: Attack Speed Stat
Character stat that modifies weapon showTime:
```csharp
float effectiveShowTime = weaponShowTime / c_Stats.attackSpeed;
```

### Optional: Weapon Weight Stat
Base penalty on weapon weight:
```csharp
float penalty = Mathf.Lerp(0.8f, 0.2f, weaponWeight / 100f);
```

### Optional: Buffs/Debuffs
Temporary modifiers to attack penalty:
```csharp
float finalPenalty = weaponPenalty * c_Stats.attackPenaltyModifier;
```

### Optional: Movement Skills
Skills that reduce attack penalty:
```csharp
if (hasLightFootedSkill)
    penalty = Mathf.Max(penalty, 0.6f); // minimum 60% speed
```

## Notes

- Each weapon SO now has its own `attackMovePenalty` value
- Movement speed calculation happens per-frame in `P_State_Movement.Update()`
- Penalty only applies during `PState.Attack` (same as before)
- No performance impact (simple multiplication)
- Easy to tune per-weapon in Unity Inspector
- Creates meaningful weapon choices beyond just damage numbers
