# Complete Balance System v2.0

## 🎯 Player Progression System

### **Player Base Stats** (Level 1, No Upgrades)
```
maxHP: 100
currentHP: 100
AD: 5
AP: 0
MS: 4.0
maxMP: 100
currentMP: 100
AR: 0
MR: 0
KR: 10
lifesteal: 0.0
armorPen: 0.0 (ignored for now)
magicPen: 0.0 (ignored for now)
```

---

## 🌟 Skill Tree Design (Each 5 Levels)

### **1. Vitality Skill** (Max HP)
```
Level 1: +20 HP (120 total)
Level 2: +20 HP (140 total)
Level 3: +20 HP (160 total)
Level 4: +20 HP (180 total)
Level 5: +20 HP (200 total)
```
**StatEffect:** `StatName.MaxHealth`, Value: 20, Duration: 0 (Permanent)

---

### **2. Vampirism Skill** (Lifesteal)
```
Level 1: +5% lifesteal (5% total)
Level 2: +5% lifesteal (10% total)
Level 3: +5% lifesteal (15% total)
Level 4: +5% lifesteal (20% total)
Level 5: +5% lifesteal (25% total)
```
**StatEffect:** `StatName.Lifesteal`, Value: 0.05, Duration: 0 (Permanent)
**Note:** Lifesteal uses 0-1 format (0.05 = 5%)

---

### **3. Iron Skin Skill** (Armor)
```
Level 1: +5 AR (5 total)
Level 2: +5 AR (10 total)
Level 3: +5 AR (15 total)
Level 4: +5 AR (20 total)
Level 5: +5 AR (25 total)
```
**StatEffect:** `StatName.Armor`, Value: 5, Duration: 0 (Permanent)

---

### **4. Magic Shell Skill** (Magic Resist)
```
Level 1: +5 MR (5 total)
Level 2: +5 MR (10 total)
Level 3: +5 MR (15 total)
Level 4: +5 MR (20 total)
Level 5: +5 MR (25 total)
```
**StatEffect:** `StatName.MagicResist`, Value: 5, Duration: 0 (Permanent)

---

### **5. Immovable Skill** (Knockback Resist)
```
Level 1: +5 KR (15 total)
Level 2: +5 KR (20 total)
Level 3: +5 KR (25 total)
Level 4: +5 KR (30 total)
Level 5: +5 KR (35 total)
```
**StatEffect:** `StatName.KnockbackResist`, Value: 5, Duration: 0 (Permanent)

---

### **6. Swift Feet Skill** (Move Speed)
```
Level 1: +0.5 MS (4.5 total)
Level 2: +0.5 MS (5.0 total)
Level 3: +0.5 MS (5.5 total)
Level 4: +0.5 MS (6.0 total)
Level 5: +0.5 MS (6.5 total)
```
**StatEffect:** `StatName.MoveSpeed`, Value: 0.5, Duration: 0 (Permanent)

---

### **7. Mana Pool Skill** (Max Mana)
```
Level 1: +20 MP (120 total)
Level 2: +20 MP (140 total)
Level 3: +20 MP (160 total)
Level 4: +20 MP (180 total)
Level 5: +20 MP (200 total)
```
**StatEffect:** `StatName.MaxMana`, Value: 20, Duration: 0 (Permanent)

---

### **8. Wide Slash Skill** (Slash Arc Bonus)
```
Level 1: +10° arc (example weapon: 45° → 55°)
Level 2: +10° arc (65°)
Level 3: +10° arc (75°)
Level 4: +10° arc (85°)
Level 5: +10° arc (95°)
```
**StatEffect:** `StatName.SlashArcBonus`, Value: 10, Duration: 0 (Permanent)

---

### **9. Combat Mobility Skill** (Move Penalty Reduction)
```
Level 1: +5% less penalty (e.g., 0.6 → 0.62)
Level 2: +5% less penalty (0.64)
Level 3: +5% less penalty (0.66)
Level 4: +5% less penalty (0.68)
Level 5: +5% less penalty (0.70)
```
**StatEffect:** `StatName.MovePenaltyReduction`, Value: 5, Duration: 0 (Permanent)
**Note:** Uses 1=1% format

---

### **10. Stunning Blow Skill** (Stun Time Bonus)
```
Level 1: +10% stun duration
Level 2: +10% stun duration
Level 3: +10% stun duration
Level 4: +10% stun duration
Level 5: +10% stun duration (50% total)
```
**StatEffect:** `StatName.StunTimeBonus`, Value: 10, Duration: 0 (Permanent)

---

### **11. Extended Reach Skill** (Thrust Distance Bonus)
```
Level 1: +10% thrust distance
Level 2: +10% thrust distance
Level 3: +10% thrust distance
Level 4: +10% thrust distance
Level 5: +10% thrust distance (50% total)
```
**StatEffect:** `StatName.ThrustDistanceBonus`, Value: 10, Duration: 0 (Permanent)

---

## ⚔️ Player Weapons (5 Types)

### **1. Katana** (Fast, Balanced)
**Role:** Starting weapon, balanced stats, good for learning combos

```
[Common]
id: "player_katana"
type: Melee
offsetRadius: 0.7

[Damage]
AD: 8
AP: 0

[Knockback]
knockbackForce: 5.0
onlyThrustKnocksBack: true

[Melee Timing]
thrustDistance: 0.25

[Combo System]
slashArcDegrees: 45
comboShowTimes: [0.4, 0.4, 0.4]
comboDamageMultipliers: [1.0, 1.2, 2.0]
comboMovePenalties: [0.6, 0.5, 0.3]
comboStunTimes: [0.1, 0.2, 0.4]
```
**Total Damage Per Combo:** 8 + 9.6 + 16 = 33.6 (with base AD: 5)
**DPS:** ~33.6 / 1.2s = 28 DPS

---

### **2. Greatsword** (Slow, Heavy)
**Role:** High damage, slow attacks, great for tanks

```
[Common]
id: "player_greatsword"
type: Melee
offsetRadius: 0.9

[Damage]
AD: 15
AP: 0

[Knockback]
knockbackForce: 8.0
onlyThrustKnocksBack: false (all attacks knock back!)

[Melee Timing]
thrustDistance: 0.35

[Combo System]
slashArcDegrees: 60
comboShowTimes: [0.6, 0.6, 0.7]
comboDamageMultipliers: [1.0, 1.3, 2.5]
comboMovePenalties: [0.5, 0.4, 0.2]
comboStunTimes: [0.15, 0.25, 0.6]
```
**Total Damage Per Combo:** 15 + 19.5 + 37.5 = 72 (with base AD: 5)
**DPS:** ~72 / 1.9s = 37.9 DPS (Higher DPS but slower, more risky)

---

### **3. Dual Daggers** (Fastest, Low Damage)
**Role:** Speed demon, hit and run, low damage per hit

```
[Common]
id: "player_daggers"
type: Melee
offsetRadius: 0.5

[Damage]
AD: 5
AP: 0

[Knockback]
knockbackForce: 3.0
onlyThrustKnocksBack: true

[Melee Timing]
thrustDistance: 0.2

[Combo System]
slashArcDegrees: 35
comboShowTimes: [0.25, 0.25, 0.3]
comboDamageMultipliers: [1.0, 1.0, 1.5]
comboMovePenalties: [0.75, 0.7, 0.5]
comboStunTimes: [0.05, 0.1, 0.2]
```
**Total Damage Per Combo:** 5 + 5 + 7.5 = 17.5 (with base AD: 5)
**DPS:** ~17.5 / 0.8s = 21.9 DPS (Lower DPS but safer)

---

### **4. Spear** (Long Range, Thrust Focus)
**Role:** Poke specialist, long reach, thrust-heavy

```
[Common]
id: "player_spear"
type: Melee
offsetRadius: 1.0

[Damage]
AD: 10
AP: 0

[Knockback]
knockbackForce: 6.0
onlyThrustKnocksBack: true

[Melee Timing]
thrustDistance: 0.4 (longest thrust!)

[Combo System]
slashArcDegrees: 35 (narrow slashes)
comboShowTimes: [0.4, 0.4, 0.5]
comboDamageMultipliers: [0.8, 1.0, 2.5] (weak slashes, strong thrust)
comboMovePenalties: [0.65, 0.55, 0.3]
comboStunTimes: [0.05, 0.15, 0.5]
```
**Total Damage Per Combo:** 8 + 10 + 25 = 43 (with base AD: 5)
**DPS:** ~43 / 1.3s = 33.1 DPS

---

### **5. Magic Staff** (Ranged, AP Scaling)
**Role:** Ranged caster, uses mana, scales with AP

```
[Common]
id: "player_staff"
type: Magic
offsetRadius: 0.7

[Damage]
AD: 0
AP: 12

[Knockback]
knockbackForce: 4.0

[Ranged + Magic]
showTime: 0.4
attackMovePenalty: 0.7
manaCost: 10
projectileSpeed: 10.0
projectileLifetime: 2.5
pierceCount: 1 (hits 2 enemies)
```
**Damage:** 0 AD + 12 AP = 12 damage per cast
**Mana:** 100 base MP = 10 casts, 200 MP = 20 casts
**DPS:** ~12 / 0.4s = 30 DPS (but limited by mana)

---

## 👾 Enemy Rebalance (Based on New Player Stats)

### **Player Damage Calculation**
```
Early Game: 5 AD + 8 weapon AD (Katana) = 13 damage/hit
Mid Game (maxed AR): 5 AD + 8 weapon AD, enemy has ~5 AR = ~12 damage/hit
Late Game (with lifesteal): 5 AD + 8 weapon AD + 25% lifesteal = ~3.25 HP per hit
```

---

### **1. Archer** (Ranged, Glass Cannon)
```
maxHP: 40 (increased from 30)
AD: 2
AP: 0
MS: 3.5
AR: 0
MR: 0
KR: 5

attackCooldown: 1.5
collisionDamage: 2
collisionTick: 0.5

detectionRange: 8.0
attackRange: 6.0
```

**Weapon: Archer Bow**
```
AD: 10
knockbackForce: 2.0
showTime: 0.3
attackMovePenalty: 0.8
projectileSpeed: 12.0
projectileLifetime: 2.0
```

**Combat Math:**
- Player takes: 10 damage/hit (10% HP early, 5% HP late)
- Player needs: ~4 hits to kill early (40 HP ÷ 13 dmg)
- Player needs: ~3 hits to kill mid/late (with weapon upgrades)
- Threat: 10 hits to kill player (100 HP), 20 hits with max HP

---

### **2. Scout** (Fast Melee)
```
maxHP: 60 (increased from 50)
AD: 3
AP: 0
MS: 6.0 (still faster than player base 4.0)
AR: 3
MR: 0
KR: 8

attackCooldown: 0.9
collisionDamage: 3
collisionTick: 0.5

detectionRange: 6.0
attackRange: 1.5
```

**Weapon: Scout Blade**
```
AD: 8
knockbackForce: 3.0
onlyThrustKnocksBack: true
slashArcDegrees: 40
thrustDistance: 0.2
comboShowTimes: [0.35, 0.35, 0.35]
comboDamageMultipliers: [1.0, 1.2, 1.5]
comboMovePenalties: [0.7, 0.6, 0.4]
comboStunTimes: [0.05, 0.1, 0.2]
```

**Combat Math:**
- Player takes: 8-12 damage/combo (8-12% HP early)
- Reduced to 4-8 damage late (with 25 AR)
- Player needs: ~5 hits to kill (60 HP ÷ 12 dmg after AR)
- Threat: 9-13 combos to kill player

---

### **3. Warrior** (Balanced)
```
maxHP: 100 (increased from 80)
AD: 4
AP: 0
MS: 4.0 (same as player base)
AR: 6
MR: 3
KR: 12

attackCooldown: 1.2
collisionDamage: 4
collisionTick: 0.5

detectionRange: 5.0
attackRange: 1.5
```

**Weapon: Warrior Sword**
```
AD: 12
knockbackForce: 5.0
onlyThrustKnocksBack: true
slashArcDegrees: 45
thrustDistance: 0.25
comboShowTimes: [0.45, 0.45, 0.45]
comboDamageMultipliers: [1.0, 1.2, 2.0]
comboMovePenalties: [0.6, 0.5, 0.3]
comboStunTimes: [0.1, 0.2, 0.5]
```

**Combat Math:**
- Player takes: 12-24 damage/combo (12-24% HP)
- Reduced to 7-14 damage late (with 25 AR)
- Player needs: ~8 hits to kill (100 HP ÷ 12 dmg, factoring AR)
- Threat: 5-8 combos to kill player early, 12-15 late

---

### **4. Bruiser** (Slow Tank)
```
maxHP: 150 (increased from 120)
AD: 6
AP: 0
MS: 3.0
AR: 10
MR: 5
KR: 20

attackCooldown: 1.5
collisionDamage: 5
collisionTick: 0.5

detectionRange: 5.0
attackRange: 1.8
```

**Weapon: Bruiser Axe**
```
AD: 18
knockbackForce: 8.0
onlyThrustKnocksBack: false
slashArcDegrees: 60
thrustDistance: 0.3
comboShowTimes: [0.6, 0.6, 0.6]
comboDamageMultipliers: [1.0, 1.5, 2.5]
comboMovePenalties: [0.5, 0.4, 0.2]
comboStunTimes: [0.15, 0.3, 0.7]
```

**Combat Math:**
- Player takes: 18-45 damage/combo (18-45% HP!!!)
- Reduced to 10-27 damage late (with 25 AR)
- Player needs: ~13 hits to kill (150 HP ÷ 11 dmg, high AR)
- Threat: 3-6 combos to kill player early, 5-10 late

---

### **5. Tank** (Boss-tier)
```
maxHP: 250 (increased from 200)
AD: 7
AP: 3
MS: 2.5
AR: 15
MR: 10
KR: 30

attackCooldown: 2.0
collisionDamage: 6
collisionTick: 0.5

detectionRange: 6.0
attackRange: 2.0
```

**Weapon: Tank Hammer**
```
AD: 25
knockbackForce: 12.0
onlyThrustKnocksBack: false
slashArcDegrees: 90
thrustDistance: 0.4
comboShowTimes: [0.8, 0.8, 1.0]
comboDamageMultipliers: [1.0, 1.5, 3.0]
comboMovePenalties: [0.4, 0.3, 0.1]
comboStunTimes: [0.2, 0.4, 1.0]
```

**Combat Math:**
- Player takes: 25-75 damage/combo (25-75% HP!!!)
- Reduced to 15-50 damage late (with 25 AR)
- Player needs: ~21 hits to kill (250 HP ÷ 12 dmg, very high AR)
- Threat: 2-4 combos to kill player early, 4-8 late

---

## 📊 Progression Table (Updated)

### Player Damage Output vs Enemies

| Enemy   | Base HP | Early (13 dmg) | Mid (18 dmg) | Late (25 dmg) | Hits to Kill (E/M/L) |
|---------|---------|----------------|--------------|---------------|----------------------|
| Archer  | 40      | 13             | 18           | 25            | 4 / 3 / 2            |
| Scout   | 60      | 12 (AR)        | 17           | 24            | 5 / 4 / 3            |
| Warrior | 100     | 11 (AR)        | 16           | 23            | 10 / 7 / 5           |
| Bruiser | 150     | 9 (high AR)    | 14           | 21            | 17 / 11 / 8          |
| Tank    | 250     | 8 (very high AR)| 13          | 20            | 32 / 20 / 13         |

### Enemy Damage vs Player

| Enemy   | Combo Dmg (Early) | Combo Dmg (Late, 25 AR) | Combos to Kill (Early) | Combos to Kill (Late) |
|---------|-------------------|-------------------------|------------------------|-----------------------|
| Archer  | 10/hit            | 10/hit                  | 10 hits                | 20 hits               |
| Scout   | 8-12              | 4-8                     | 9-13                   | 17-33                 |
| Warrior | 12-24             | 7-14                    | 5-9                    | 15-29                 |
| Bruiser | 18-45             | 10-27                   | 3-6                    | 8-20                  |
| Tank    | 25-75             | 15-50                   | 2-4                    | 4-13                  |

---

## 🎯 Skill Point Distribution Strategy

**Total Skill Points Needed:** 11 skills × 5 levels = **55 skill points**

**Recommended Early Game (First 10 Points):**
1. Vitality Level 1-2 (120-140 HP)
2. Swift Feet Level 1-2 (4.5-5.0 MS)
3. Iron Skin Level 1-2 (5-10 AR)
4. Vampirism Level 1 (5% lifesteal)
5. Combat Mobility Level 1-2 (better attack movement)

**Mid Game (Next 15 Points):**
6. Vitality Level 3-5 (200 HP)
7. Iron Skin Level 3-4 (15-20 AR)
8. Magic Shell Level 1-3 (5-15 MR)
9. Swift Feet Level 3-4 (5.5-6.0 MS)
10. Stunning Blow Level 1-3

**Late Game (Final 30 Points):**
11. Max all remaining skills for specialization

---

## ✅ Implementation Checklist

### Update Player Base Stats
```
Player → C_Stats:
  maxHP: 100
  AD: 5
  MS: 4.0 (changed from 5.0!)
  maxMP: 100
  AR: 0
  MR: 0
  KR: 10
  lifesteal: 0
```

### Create 11 Skill ScriptableObjects
Use the StatEffect values listed above for each skill.

### Create 5 Player Weapon ScriptableObjects
Use the weapon stats for Katana, Greatsword, Daggers, Spear, Staff.

### Update 5 Enemy Prefabs
Use the rebalanced enemy stats (higher HP, adjusted damage).

### Create/Update 5 Enemy Weapon ScriptableObjects
Use the weapon stats for each enemy type.

---

## 🎮 Balance Philosophy Summary

**Early Game (0-10 skill points):**
- Player: 100-140 HP, 4.0-5.0 MS, 0-10 AR
- Can kill Archer/Scout comfortably
- Struggles with Warrior (mini-boss feel)
- Bruiser/Tank are very dangerous

**Mid Game (10-30 skill points):**
- Player: 140-180 HP, 5.0-6.0 MS, 10-20 AR, 10-15% lifesteal
- Can kill Warrior comfortably
- Bruiser is challenging but fair
- Tank is boss-tier

**Late Game (30-55 skill points):**
- Player: 180-200 HP, 6.0-6.5 MS, 20-25 AR, 20-25% lifesteal
- Can handle mixed encounters
- Tank becomes fair 1v1
- Multiple Bruisers/Warriors = challenge

**Weapon Choice Matters:**
- Katana: Balanced, good for learning
- Greatsword: High risk/reward, burst damage
- Daggers: Safe, low commitment, hit and run
- Spear: Poke/kite, range advantage
- Staff: Ranged, mana management, AoE potential

This creates a smooth power curve where skill points feel impactful and weapon choice creates strategic depth! 🎯
