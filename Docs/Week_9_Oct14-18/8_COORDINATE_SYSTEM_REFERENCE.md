# 🧭 Coordinate System Reference - NinjaAdventure

Quick reference for understanding angles in our combo system.

---

## Our Coordinate System (0° = UP)

```
          0° ↑ (UP)
              |
              |
              |
270° ←--------●--------→ 90° (RIGHT)
 (LEFT)       |
              |
              |
            180° ↓ (DOWN)
```

### Key Angles:

| Angle | Direction | Visual | Unity Vector2 |
|-------|-----------|--------|---------------|
| **0°** | **UP** | ↑ | `(0, 1)` |
| **45°** | **Up-Right** | ↗ | `(0.707, 0.707)` |
| **90°** | **RIGHT** | → | `(1, 0)` |
| **135°** | **Down-Right** | ↘ | `(0.707, -0.707)` |
| **180°** | **DOWN** | ↓ | `(0, -1)` |
| **225°** | **Down-Left** | ↙ | `(-0.707, -0.707)` |
| **270°** | **LEFT** | ← | `(-1, 0)` |
| **315°** | **Up-Left** | ↖ | `(-0.707, 0.707)` |

---

## Why 0° = UP?

**Because our weapon sprite points UP by default!**

```
Weapon sprite in Unity:
    ╱╲    ← Blade (top)
   ╱  ╲
  ╱    ╲
 ╱      ╲
╱________╲
    ██      ← Handle (bottom) ← PIVOT POINT
```

When the weapon has **0 rotation**, it points **UP** naturally.

---

## Angle Calculation Formula

```csharp
// From attackDir (Vector2) to angle (degrees)
float angle = Mathf.Atan2(-attackDir.x, attackDir.y) * Mathf.Rad2Deg;
```

### Examples:

| attackDir | Calculation | Result | Meaning |
|-----------|-------------|--------|---------|
| `(0, 1)` | `atan2(-0, 1) * 57.3` | **0°** | UP ↑ |
| `(1, 0)` | `atan2(-1, 0) * 57.3` | **90°** | RIGHT → |
| `(0, -1)` | `atan2(-0, -1) * 57.3` | **180°** | DOWN ↓ |
| `(-1, 0)` | `atan2(1, 0) * 57.3` | **270°** (or -90°) | LEFT ← |
| `(0.707, 0.707)` | `atan2(-0.707, 0.707) * 57.3` | **45°** | UP-RIGHT ↗ |

---

## Position Calculation Formula

```csharp
// From angle (degrees) to position around player
float angleRad = angle * Mathf.Deg2Rad;
Vector3 position = new Vector3(
    -Mathf.Sin(angleRad) * radius,  // X (negated!)
    Mathf.Cos(angleRad) * radius,   // Y
    0f
);
```

### Examples (with radius = 1.0):

| Angle | Sin/Cos | Position | Location |
|-------|---------|----------|----------|
| **0°** | `-sin(0)=0, cos(0)=1` | `(0, 1)` | **Above player** ↑ |
| **45°** | `-sin(45)=-0.707, cos(45)=0.707` | `(-0.707, 0.707)` | **Up-Right of player** ↗ |
| **90°** | `-sin(90)=-1, cos(90)=0` | `(-1, 0)` | **Right of player** → |
| **135°** | `-sin(135)=-0.707, cos(135)=-0.707` | `(-0.707, -0.707)` | **Down-Right** ↘ |
| **180°** | `-sin(180)=0, cos(180)=-1` | `(0, -1)` | **Below player** ↓ |
| **270°** | `-sin(270)=1, cos(270)=0` | `(1, 0)` | **Left of player** ← |

---

## Combo Attack Examples

### Attacking RIGHT (90°):

**Player facing right, attacking enemy to the right:**

```
Base angle = 90°
Arc sweep = 90° (±45°)

Slash Down (Combo 0):
  Start: 90° - 45° = 45° (up-right)
  End:   90° + 45° = 135° (down-right)
  
  Visual:
      45° ↗
           ╲
            ╲ 
     Player ●--→ 90°
            ╱
           ╱
     135° ↘

Slash Up (Combo 1):
  Start: 90° + 45° = 135° (down-right)
  End:   90° - 45° = 45° (up-right)
  
  Visual (REVERSED):
     135° ↘
           ╱
          ╱ 
     Player ●--→ 90°
          ╲
           ╲
      45° ↗
```

---

### Attacking UP (0°):

**Player facing up, attacking enemy above:**

```
Base angle = 0°
Arc sweep = 90° (±45°)

Slash Down (Combo 0):
  Start: 0° - 45° = -45° (315°, up-left)
  End:   0° + 45° = 45° (up-right)
  
  Visual:
    -45° ↖     ↑ 0°    45° ↗
          ╲    |    ╱
           ╲   |   ╱
            ╲  |  ╱
          Player ●

Slash Up (Combo 1):
  Start: 0° + 45° = 45° (up-right)
  End:   0° - 45° = -45° (up-left)
  
  Visual (REVERSED):
    45° ↗      ↑ 0°    -45° ↖
          ╱    |    ╲
         ╱     |     ╲
        ╱      |      ╲
          Player ●
```

---

## Why Negate X in Both Formulas?

**Consistency!** Both angle→position and direction→angle use the same coordinate system.

### Without Negation (WRONG):
```
attackDir = (1, 0) [want to attack RIGHT]
angle = atan2(1, 0) = 90° ← Seems right...
position = (sin(90)=1, cos(90)=0) = (1, 0) ← Weapon to the LEFT! ❌
```

### With Negation (CORRECT):
```
attackDir = (1, 0) [want to attack RIGHT]
angle = atan2(-1, 0) = 90° ← Correct!
position = (-sin(90)=-1, cos(90)=0) = (-1, 0) ← Weapon to the RIGHT! ✓
```

**Why?** Unity's coordinate system has X increasing to the RIGHT, but standard polar coordinates (with UP as 0°) expect the opposite.

---

## Standard Math vs Our System

### Standard Polar Coordinates (0° = RIGHT):
```
Used in: Math textbooks, calculators

     90° ↑
      |
      |
270° ←--●--→ 0° (RIGHT)
      |
      |
    180° ↓

Formula: x = r*cos(θ), y = r*sin(θ)
```

### Our System (0° = UP):
```
Used in: NinjaAdventure weapon system

      0° ↑ (UP)
       |
       |
270° ←--●--→ 90°
       |
       |
     180° ↓

Formula: x = -r*sin(θ), y = r*cos(θ)  ← Negated X!
```

**We rotated the standard system 90° counter-clockwise** (so UP is 0° instead of RIGHT).

---

## Quick Reference Card

```
┌─────────────────────────────────────────┐
│  ANGLE → DIRECTION                      │
├─────────────────────────────────────────┤
│  0° = UP ↑                              │
│  45° = UP-RIGHT ↗                       │
│  90° = RIGHT →                          │
│  135° = DOWN-RIGHT ↘                    │
│  180° = DOWN ↓                          │
│  225° = DOWN-LEFT ↙                     │
│  270° = LEFT ←                          │
│  315° = UP-LEFT ↖                       │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│  FORMULAS                               │
├─────────────────────────────────────────┤
│  Vector → Angle:                        │
│    atan2(-x, y) * Rad2Deg               │
│                                         │
│  Angle → Position:                      │
│    x = -sin(angle) * radius             │
│    y =  cos(angle) * radius             │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│  WHY NEGATE X?                          │
├─────────────────────────────────────────┤
│  Makes Unity's coordinate system        │
│  work with UP-as-0° polar math          │
└─────────────────────────────────────────┘
```

---

## Testing in Unity Console

Want to verify angles? Add this to any MonoBehaviour:

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.T))
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        
        float angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
        
        Debug.Log($"Direction: {dir}, Angle: {angle}°");
        
        // Test position calculation
        float angleRad = angle * Mathf.Deg2Rad;
        Vector2 pos = new Vector2(
            -Mathf.Sin(angleRad),
            Mathf.Cos(angleRad)
        );
        Debug.Log($"Position on unit circle: {pos}");
    }
}
```

---

**Remember:** In NinjaAdventure, **0° always means UP** because that's how the weapon sprite naturally points! ↑
