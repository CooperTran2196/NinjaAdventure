# Weapon Skill Bonuses System Guide

## 📋 Overview
This document explains the new weapon stat bonuses that can be applied through the Skill Tree system. These bonuses enhance weapon performance for the **player only** (enemies use default values of 0).

---

## 🎯 New Stat Types

### 1. **Slash Arc Bonus** (`StatName.SlashArcBonus`)
- **Type**: Additive (degrees)
- **What it does**: Increases the sweep angle of slash attacks
- **Base value**: Weapon's `slashArcDegrees` (default: 45°)
- **Formula**: `finalArc = baseArc + slashArcBonus`
- **Example**: 
  - Base arc: 45°
  - Skill bonus: +15°
  - Result: 60° sweep (wider slash coverage)

### 2. **Move Penalty Reduction** (`StatName.MovePenaltyReduction`)
- **Type**: Multiplicative (percentage, 0.0 to 1.0)
- **What it does**: Reduces movement speed penalty during attacks
- **Base value**: Weapon's `comboMovePenalties[comboIndex]` (e.g., 0.6 = 60% speed)
- **Formula**: `finalPenalty = basePenalty + (1 - basePenalty) * reduction`
- **Example**:
  - Base penalty: 0.6 (40% speed reduction → 60% of normal speed)
  - Skill bonus: 0.1 (10% reduction)
  - Calculation: `0.6 + (1 - 0.6) * 0.1 = 0.64`
  - Result: 0.64 (36% speed reduction → 64% of normal speed)

### 3. **Stun Time Bonus** (`StatName.StunTimeBonus`)
- **Type**: Multiplicative (percentage, 0.0 to 1.0)
- **What it does**: Increases stun duration on hit
- **Base value**: Weapon's `comboStunTimes[comboIndex]` (e.g., 0.5s)
- **Formula**: `finalStun = baseStun * (1 + stunTimeBonus)`
- **Example**:
  - Base stun: 0.5s
  - Skill bonus: 0.2 (20% increase)
  - Result: `0.5 * 1.2 = 0.6s` stun duration

### 4. **Thrust Distance Bonus** (`StatName.ThrustDistanceBonus`)
- **Type**: Multiplicative (percentage, 0.0 to 1.0)
- **What it does**: Increases forward thrust attack distance
- **Base value**: Weapon's `thrustDistance` (e.g., 0.25)
- **Formula**: `finalDistance = baseDistance * (1 + thrustDistanceBonus)`
- **Example**:
  - Base thrust: 0.25
  - Skill bonus: 0.3 (30% increase)
  - Result: `0.25 * 1.3 = 0.325` distance

---

## 🛠️ Creating Skill Assets in Unity

### Step 1: Create New Skill ScriptableObject
1. Right-click in Project window → `Create` → `SkillTree`
2. Name it descriptively (e.g., `SK_WideSlash`, `SK_SwiftStrike`)

### Step 2: Configure Skill Properties
```
Meta:
  - id: "wide_slash_01"
  - skillName: Auto-filled from filename
  - skillIcon: Assign sprite
  - maxLevel: Set how many times it can be upgraded

Effects Per Level:
  - Size: 1 (or more if multiple effects)
```

### Step 3: Configure Stat Effect

**Example 1: Wide Slash Skill (+5° per level)**
```
StatEffectList:
  Element 0:
    - statName: SlashArcBonus
    - Value: 5
    - Duration: 0 (Permanent)
    - IsOverTime: false
```

**Example 2: Swift Strike Skill (10% move penalty reduction per level)**
```
StatEffectList:
  Element 0:
    - statName: MovePenaltyReduction
    - Value: 0.1
    - Duration: 0 (Permanent)
    - IsOverTime: false
```

**Example 3: Extended Thrust Skill (15% distance per level)**
```
StatEffectList:
  Element 0:
    - statName: ThrustDistanceBonus
    - Value: 0.15
    - Duration: 0 (Permanent)
    - IsOverTime: false
```

**Example 4: Paralyzing Strike (20% stun time per level)**
```
StatEffectList:
  Element 0:
    - statName: StunTimeBonus
    - Value: 0.2
    - Duration: 0 (Permanent)
    - IsOverTime: false
```

---

## 📊 Recommended Skill Values

### Slash Arc Bonus
- **Per Level**: 5-10 degrees
- **Max Levels**: 3-5
- **Total Bonus Range**: 15-50 degrees
- **Notes**: Too high makes combat trivial (360° slash would hit everything)

### Move Penalty Reduction
- **Per Level**: 0.05 to 0.15 (5% to 15%)
- **Max Levels**: 3-5
- **Total Bonus Range**: 0.15 to 0.75 (15% to 75% reduction)
- **Notes**: 1.0 (100% reduction) = no penalty at all during attacks

### Stun Time Bonus
- **Per Level**: 0.1 to 0.25 (10% to 25%)
- **Max Levels**: 3-4
- **Total Bonus Range**: 0.3 to 1.0 (30% to 100% increase)
- **Notes**: Doubling stun time can be very powerful

### Thrust Distance Bonus
- **Per Level**: 0.1 to 0.2 (10% to 20%)
- **Max Levels**: 3-5
- **Total Bonus Range**: 0.3 to 1.0 (30% to 100% increase)
- **Notes**: Too high makes thrust attacks feel floaty

---

## 🎮 Example Skill Tree Progression

### Early Game Skills
1. **Basic Slash Extension**
   - Slash Arc Bonus: +5° per level
   - Max Level: 3
   - Total: +15°

2. **Agile Attacker**
   - Move Penalty Reduction: 0.1 per level
   - Max Level: 2
   - Total: 20% reduction

### Mid Game Skills (Prerequisites: Early skills maxed)
3. **Wide Sweep**
   - Slash Arc Bonus: +10° per level
   - Max Level: 3
   - Total: +30°

4. **Combat Mobility**
   - Move Penalty Reduction: 0.15 per level
   - Max Level: 3
   - Total: 45% reduction

### Late Game Skills (Prerequisites: Mid skills maxed)
5. **Master Swordsman**
   - StatEffectList (Multiple effects):
     - Slash Arc Bonus: +15
     - Stun Time Bonus: 0.3
     - Move Penalty Reduction: 0.2
   - Max Level: 1
   - Total: Massive power spike

---

## 💻 Technical Implementation Summary

### C_Stats.cs (Data Layer)
```csharp
[Header("Weapon Bonuses (Player Only)")]
public float slashArcBonus = 0f;
public float movePenaltyReduction = 0f;
public float stunTimeBonus = 0f;
public float thrustDistanceBonus = 0f;
```

### P_StatsManager.cs (Logic Layer)
- Stores base values in `Awake()`
- Applies permanent effects via `ApplyPermanentEffect()`
- Recalculates final stats in `RecalculateAllStats()`
- Supports temporary effects via `CommitStatChange()`

### W_Melee.cs (Weapon Layer)
- **GetComboPattern()**: Uses `slashArcBonus` for wider arcs
- **Hit()**: Uses `thrustDistanceBonus` for longer thrusts

### W_Base.cs (Hit Effects)
- **ApplyHitEffects()**: Uses `stunTimeBonus` for longer stuns

### P_State_Attack.cs (Movement Penalty)
- **GetCurrentMovePenalty()**: Uses `movePenaltyReduction` to reduce attack slowdown

---

## 🚫 Important Notes

### Enemy Compatibility
- Enemies have the same `C_Stats` component
- All weapon bonus fields default to `0f`
- Enemies **cannot** benefit from these bonuses (by design)
- This is intentional - only player unlocks skills

### Items vs Skills
- **Items**: Do NOT use weapon bonus stats (per your request)
- **Skills**: Only source of weapon bonus stats
- This keeps progression clean and predictable

### Duration Rules
- **Duration = 0**: Permanent effect (recommended for all weapon bonuses)
- **Duration = 1**: Instant effect (not useful for weapon bonuses)
- **Duration > 1**: Timed effect (not useful for weapon bonuses)

### Multiple Effects Per Skill
- One skill can have multiple `StatEffect` entries
- Example: "Ultimate Strike" could grant:
  - Slash Arc Bonus: +20
  - Stun Time Bonus: 0.5
  - Move Penalty Reduction: 0.3

---

## ✅ Testing Checklist

After creating skill assets, test:
1. ✅ Skill appears in Skill Tree UI
2. ✅ Can unlock skill (prerequisite check)
3. ✅ Upgrading spends skill points correctly
4. ✅ Visual/mechanical effect is noticeable:
   - Slash Arc: Watch attack sweep angle
   - Move Penalty: Check speed during attacks
   - Stun Time: Enemy stays stunned longer
   - Thrust Distance: Weapon travels further forward
5. ✅ Multiple upgrades stack correctly
6. ✅ Enemy behavior unchanged (no bonuses applied)

---

## 🎨 UI Integration

You mentioned you'll handle this, but here's what to consider:
- Update skill descriptions to show:
  - Current level
  - Next level bonus
  - Total accumulated bonus
- Visual feedback when bonus is active (optional):
  - Particle effects on wider slashes
  - Speed lines during mobile attacks
  - Impact effects on longer stuns

---

## 📝 Summary

**What Changed:**
- Added 4 new `StatName` enum values
- Extended `C_Stats` with weapon bonus fields
- Updated `P_StatsManager` to handle new stats
- Modified weapon code to apply bonuses
- **Updated `E_Controller` to make `State_Attack` optional** (for collision-only enemies)
- System is fully compatible with existing SkillTree infrastructure

**What You Need to Do:**
1. Create Skill ScriptableObjects in Unity
2. Configure stat effects using this guide
3. Update Skill Tree UI (descriptions, tooltips, etc.)
4. Playtest and balance values

**What's Automatic:**
- Skill application through `ST_Manager`
- Stat recalculation through `P_StatsManager`
- Bonus application in weapon code
- Enemy exclusion (default 0 values)

Good luck with the skill design! 🎮
