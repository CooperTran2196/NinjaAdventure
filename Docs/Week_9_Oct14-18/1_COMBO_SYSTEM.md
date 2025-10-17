# 🗡️ Combo System Guide

## Overview

3-hit melee combo system with radar-style arc slashing.

**Status:** ✅ Production Ready (v1.6)  
**Chain:** Slash Down → Slash Up → Thrust

---

## Quick Start

### For Players:
1. Spam attack button
2. Watch the combo chain
3. Enjoy!

### For Developers:
1. Set weapon sprite **pivot to BOTTOM** (0.5, 0) in Unity Sprite Editor
2. Configure W_SO: `pointsUp = true`, `offsetRadius = 0.7`
3. Test in Play Mode
4. Tune values as needed

---

## Critical Setup: Bottom-Pivot Sprites

### Why Bottom Pivot?

The weapon rotates like a **radar arm** - handle stays near player, blade extends outward.

```
✅ CORRECT (Bottom Pivot):
      🗡️ Blade extends outward
      |
      | Weapon
      |
      ● Pivot/Handle ← At offsetRadius from player
     ⭕ Player

❌ WRONG (Center Pivot):
      Blade
      |
      ● Pivot ← Handle and blade rotate opposite!
      |
      Handle
     ⭕ Player
```

### Unity Setup Steps:

1. **Select weapon sprite** in Project window
2. **Open Sprite Editor** (button in Inspector)
3. **Set Pivot to "Bottom"** or custom `(0.5, 0)`
4. **Click Apply**
5. **Verify** pivot gizmo is at bottom-center

**Requirements:**
- ✅ Sprite points UP (blade at top, handle at bottom)
- ✅ Pivot at BOTTOM (0.5, 0)
- ✅ W_SO: `pointsUp = true`

---

## How It Works

### Radar Arm Rotation

```
Attack RIGHT with 45° arc:

       🗡️ Start (45°)
        \
         \  Arc sweep
    ⭕----🗡️ Middle (90°)
   Player  \
            \
            🗡️ End (135°)
```

### Key Math:
```csharp
// Angle: negated X for correct direction
float angle = Mathf.Atan2(-attackDir.x, attackDir.y) * Mathf.Rad2Deg;

// Position: negated X to match angle
Vector3 pos = new Vector3(
    -Mathf.Sin(angleRad) * offsetRadius,
    Mathf.Cos(angleRad) * offsetRadius,
    0f
);

// Rotation: simple with bottom pivot!
rotation = angle;
```

---

## Configuration (W_SO)

### Essential Settings:
| Field | Purpose | Default |
|-------|---------|---------|
| `pointsUp` | Sprite orientation | `true` |
| `offsetRadius` | Handle distance from player | `0.7` |
| `slashArcDegrees` | Arc coverage | `45°` |

### Combo Arrays (3 elements):
| Array | Slash Down | Slash Up | Thrust |
|-------|-----------|----------|--------|
| `comboShowTimes` | 0.3s | 0.3s | 0.5s |
| `comboDamageMultipliers` | 1.0x | 1.2x | 2.0x |
| `comboMovePenalties` | 60% | 50% | 30% |
| `comboStunTimes` | 0.1s | 0.2s | 0.5s |

### Special:
- `onlyThrustKnocksBack = true` - Only thrust (finisher) knocks back

---

## Combo Mechanics

### Input System:
- **Window:** 1.0 second (very forgiving)
- **Opens:** Immediately when attack starts
- **Button Mashing:** ✅ Fully supported!
- **Buffering:** Queues next attack if pressed during window

### Combo Chain:

**1. Slash Down (Case 0)**
- Arc sweeps downward
- 1.0x damage, 60% movement speed
- 0.1s stun

**2. Slash Up (Case 1)**
- Arc sweeps upward
- 1.2x damage, 50% movement speed
- 0.2s stun

**3. Thrust (Case 2)**
- Forward stab with knockback
- 2.0x damage, 30% movement speed
- 0.5s stun

### Combo Canceling:
- Dodge → Resets to stage 0
- Taking Damage → Resets to stage 0

---

## Tuning Presets

### Fast Dagger:
```
offsetRadius = 0.5
slashArcDegrees = 30
comboShowTimes = [0.2, 0.2, 0.3]
```

### Standard Sword:
```
offsetRadius = 0.7  ← Recommended
slashArcDegrees = 45
comboShowTimes = [0.3, 0.3, 0.5]
```

### Heavy Greatsword:
```
offsetRadius = 1.0
slashArcDegrees = 90
comboShowTimes = [0.5, 0.5, 0.8]
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Weapon rotates around center | Set sprite pivot to bottom |
| Left/Right reversed | Already fixed (negated X) |
| Handle covered by player | Increase `offsetRadius` |
| Weapon goes backward | Already fixed (`-Sin(angle)`) |
| Combo doesn't chain | Already fixed (1.0s window) |
| Can't button mash | Already fixed (immediate opening) |

---

## Files Modified

**Core Files:**
1. `W_SO.cs` - Combo configuration
2. `W_Base.cs` - Arc rotation logic
3. `W_Melee.cs` - Combo pattern switching
4. `P_State_Attack.cs` - State machine & input buffering
5. `P_State_Movement.cs` - Movement penalty integration
6. `P_Controller.cs` - Input queuing
7. `C_Health.cs` - Damage cancels combo

---

## Design History

### Original Problem:
Single attack felt repetitive, lacked depth and visual appeal.

### Solution:
3-hit combo chain with:
- Progressive damage scaling (rewards commitment)
- Arc slashing (visual variety)
- Movement penalties (strategic positioning)
- Input buffering (accessible execution)

### Iterations:
1. **v1.0** - Basic 3-hit combo
2. **v1.1** - Arc rotation fix
3. **v1.2** - Left/right direction fix
4. **v1.3** - Handle offset fix
5. **v1.4** - Bottom-pivot implementation
6. **v1.5** - Direction + offset perfected
7. **v1.6** - Final polish ✅

---

## Testing Checklist

- [ ] Set sprite pivot to bottom
- [ ] Test all 8 directions (up, down, left, right, diagonals)
- [ ] Button mash full combo
- [ ] Verify handle visible
- [ ] Check weapon sweeps correctly
- [ ] Test dodge canceling
- [ ] Confirm damage scaling

---

**Version:** 1.6 Final  
**Status:** ✅ Production Ready
