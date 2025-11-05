# Complete Game Balance Guide - NinjaAdventure

## 📊 Overview

**Design Philosophy:**
- 3 Levels total with increasing difficulty
- Player can max ~80% of all skills (forces strategic choices)
- Two enemy types: Collision-only (weak) and Weapon-wielding (strong)
- Clear progression curve from weak to overpowered

---

## 🎮 Player Base Stats & Progression

### **Player Starting Stats (Level 1)**
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
armorPen: 0.0
magicPen: 0.0
```

### **EXP System** (From `P_Exp.cs`)
```
Level 1 → 2: 60 XP
Level 2 → 3: 90 XP
Level 3 → 4: 120 XP
Level N → N+1: 60 + (30 × (N-1)) XP

Skill Points per Level: 2
```

### **Total XP Calculation for 3 Levels**
```
Level 1 → 2: 60 XP
Level 2 → 3: 90 XP
Level 3 → 4: 120 XP
-----------------------
Total XP Needed: 270 XP
Total Skill Points: 6 SP (2 per level × 3 levels)
```

**WAIT - This is only 3 level-ups! Let me recalculate for more levels...**

To max 80% of all skills (11 skills × 5 levels = 55 total upgrades):
- 80% of 55 = 44 upgrades needed
- Each upgrade costs 1 SP
- Need 44 SP total
- At 2 SP per level, need 22 levels
- Starting at level 1, need to reach level 23

**Revised Total XP for 22 Levels:**
```
L1→L2: 60
L2→L3: 90
L3→L4: 120
...
L22→L23: 60 + 30×21 = 690

Total XP = Σ(60 + 30×(n-1)) for n=1 to 22
Total XP = 60×22 + 30×(0+1+2+...+21)
Total XP = 1320 + 30×231
Total XP = 1320 + 6930 = 8,250 XP
```

**BUT** - You said "3 levels" so I'll design for **3 complete game levels (maps)**, not player levels!

Let me redesign:

---

## 🗺️ Game Structure (3 Map Levels)

### **Level 1: Village Outskirts** (Early Game)
- Enemy Count: ~30-40 enemies
- Mix: 70% Collision-only, 30% Weapon enemies
- Boss: 1 Mini-Boss (Warrior type)
- Total XP: ~600 XP (enough for 5-6 player levels)
- Player reaches Level 6-7 (10-12 SP)

### **Level 2: Dark Forest** (Mid Game)
- Enemy Count: ~50-60 enemies
- Mix: 50% Collision-only, 50% Weapon enemies
- Boss: 1 Strong Boss (Bruiser type)
- Total XP: ~1,200 XP (enough for 8-10 player levels)
- Player reaches Level 14-17 (24-30 SP cumulative)

### **Level 3: Ancient Temple** (End Game)
- Enemy Count: ~60-80 enemies
- Mix: 30% Collision-only, 70% Weapon enemies
- Boss: 1 Final Boss (Tank type)
- Total XP: ~2,000 XP (enough for 12-15 player levels)
- Player reaches Level 27-30 (50-56 SP total)

**Total Skill Points Available: ~50-56 SP**
**Total Upgrades Needed: 55 (11 skills × 5 levels)**
**Coverage: ~90-100% of skills** ✅ (Perfect!)

Actually, let me adjust to hit exactly 80%:
- 80% of 55 = 44 SP needed
- Across 3 levels, player should earn 44 SP
- At 2 SP/level, need 22 player levels
- Distribute: Level 1 (6 lvls), Level 2 (8 lvls), Level 3 (8 lvls) = 22 levels total

---

## 👾 Enemy Types & Balance

### **Type 1: Collision-Only Enemies** (Weak, Common)

#### **1a. Weak Slime** (Level 1)
```
[Stats]
maxHP: 25
AD: 2 (collision damage)
AP: 0
MS: 2.5
AR: 0
MR: 0
KR: 3

collisionDamage: 2
collisionTick: 0.5

detectionRange: 5.0
attackRange: 0.5 (melee collision)

[Rewards]
expReward: 5
dropChance: 10%
lootTable: [Health Potion (50%), Mana Potion (30%), Gold (20%)]
```

**Combat Analysis:**
- Player needs: 3-4 hits to kill (25 HP ÷ 8 dmg)
- Enemy needs: 50 hits to kill player (100 HP ÷ 2 dmg)
- Threat Level: Very Low (tutorial enemy)

---

#### **1b. Goblin Runner** (Level 1-2)
```
[Stats]
maxHP: 35
AD: 3
AP: 0
MS: 4.5
AR: 1
MR: 0
KR: 5

collisionDamage: 3
collisionTick: 0.5

detectionRange: 6.0
attackRange: 0.5

[Rewards]
expReward: 8
dropChance: 10%
lootTable: [Health Potion (40%), Gold (40%), Dagger (20%)]
```

**Combat Analysis:**
- Player needs: 4-5 hits to kill
- Faster than Slime, harder to avoid
- Threat Level: Low

---

#### **1c. Shadow Lurker** (Level 2-3)
```
[Stats]
maxHP: 50
AD: 4
AP: 0
MS: 5.5 (faster than player!)
AR: 2
MR: 1
KR: 8

collisionDamage: 4
collisionTick: 0.5

detectionRange: 7.0
attackRange: 0.5

[Rewards]
expReward: 12
dropChance: 10%
lootTable: [Mana Potion (50%), Shadow Cloak (30%), Gold (20%)]
```

**Combat Analysis:**
- Player needs: 6-7 hits to kill
- Very fast, aggressive
- Threat Level: Medium (can corner player)

---

### **Type 2: Weapon-Wielding Enemies** (Strong, Rare)

#### **2a. Archer Scout** (Level 1-2, Ranged)
```
[Stats]
maxHP: 40
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

[Weapon: Scout Bow]
AD: 10
knockbackForce: 2.0
showTime: 0.3
attackMovePenalty: 0.8
projectileSpeed: 12.0
projectileLifetime: 2.0
pierceCount: 0

[Rewards]
expReward: 15
dropChance: 50%
lootTable: [Bow (60%), Arrow Bundle (30%), Gold (10%)]
```

**Combat Analysis:**
- Player takes: 10 damage/hit (10% HP)
- Player needs: 4-5 hits to kill
- Threat Level: Medium (kiting, ranged)

---

#### **2b. Sword Soldier** (Level 1-2, Fast Melee)
```
[Stats]
maxHP: 60
AD: 3
AP: 0
MS: 5.0
AR: 3
MR: 0
KR: 8

attackCooldown: 0.9
collisionDamage: 3
collisionTick: 0.5

detectionRange: 6.0
attackRange: 1.5

[Weapon: Soldier Blade]
AD: 8
knockbackForce: 3.0
onlyThrustKnocksBack: true
slashArcDegrees: 40
thrustDistance: 0.2
comboShowTimes: [0.35, 0.35, 0.35]
comboDamageMultipliers: [1.0, 1.2, 1.5]
comboMovePenalties: [0.7, 0.6, 0.4]
comboStunTimes: [0.05, 0.1, 0.2]

[Rewards]
expReward: 20
dropChance: 50%
lootTable: [Short Sword (50%), Health Potion (30%), Gold (20%)]
```

**Combat Analysis:**
- Player takes: 8-12 damage/combo
- Player needs: 6-7 hits to kill
- Threat Level: Medium-High (fast combos)

---

#### **2c. Knight Warrior** (Level 2, Balanced)
```
[Stats]
maxHP: 100
AD: 4
AP: 0
MS: 4.0
AR: 6
MR: 3
KR: 12

attackCooldown: 1.2
collisionDamage: 4
collisionTick: 0.5

detectionRange: 5.0
attackRange: 1.5

[Weapon: Knight Sword]
AD: 12
knockbackForce: 5.0
onlyThrustKnocksBack: true
slashArcDegrees: 45
thrustDistance: 0.25
comboShowTimes: [0.45, 0.45, 0.45]
comboDamageMultipliers: [1.0, 1.2, 2.0]
comboMovePenalties: [0.6, 0.5, 0.3]
comboStunTimes: [0.1, 0.2, 0.5]

[Rewards]
expReward: 30
dropChance: 50%
lootTable: [Longsword (40%), Steel Armor (30%), Health Potion (20%), Gold (10%)]
```

**Combat Analysis:**
- Player takes: 12-24 damage/combo (12-24% HP)
- Player needs: 9-11 hits to kill
- Threat Level: High (mini-boss tier)

---

#### **2d. Berserker** (Level 2-3, Heavy)
```
[Stats]
maxHP: 150
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

[Weapon: Battle Axe]
AD: 18
knockbackForce: 8.0
onlyThrustKnocksBack: false
slashArcDegrees: 60
thrustDistance: 0.3
comboShowTimes: [0.6, 0.6, 0.6]
comboDamageMultipliers: [1.0, 1.5, 2.5]
comboMovePenalties: [0.5, 0.4, 0.2]
comboStunTimes: [0.15, 0.3, 0.7]

[Rewards]
expReward: 50
dropChance: 75%
lootTable: [Battle Axe (50%), Heavy Armor (30%), Gold (20%)]
```

**Combat Analysis:**
- Player takes: 18-45 damage/combo (18-45% HP!)
- Player needs: 14-17 hits to kill
- Threat Level: Very High (elite enemy)

---

#### **2e. Death Knight** (Level 3, Tank Boss)
```
[Stats]
maxHP: 250
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

[Weapon: Death Hammer]
AD: 25
knockbackForce: 12.0
onlyThrustKnocksBack: false
slashArcDegrees: 90
thrustDistance: 0.4
comboShowTimes: [0.8, 0.8, 1.0]
comboDamageMultipliers: [1.0, 1.5, 3.0]
comboMovePenalties: [0.4, 0.3, 0.1]
comboStunTimes: [0.2, 0.4, 1.0]

[Rewards]
expReward: 100
dropChance: 100%
lootTable: [Legendary Weapon (60%), Rare Armor (30%), Gold (10%)]
```

**Combat Analysis:**
- Player takes: 25-75 damage/combo (25-75% HP!!!)
- Player needs: 22-28 hits to kill
- Threat Level: Boss (requires mastery)

---

## 📈 Level-by-Level Enemy Distribution

### **Level 1: Village Outskirts** (Target: 600 XP)

**Enemy Composition:**
```
Weak Slime × 40        = 40 × 5 XP  = 200 XP
Goblin Runner × 15     = 15 × 8 XP  = 120 XP
Archer Scout × 8       = 8 × 15 XP  = 120 XP
Sword Soldier × 6      = 6 × 20 XP  = 120 XP
Knight Warrior (Boss)  = 1 × 100 XP = 100 XP (treat as mini-boss)
------------------------------------------------------
Total: 70 enemies      = 660 XP
```

**Breakdown:**
- Collision-only: 55 enemies (79%)
- Weapon enemies: 14 enemies (20%)
- Boss: 1 (1%)

**Player Progression:**
- Starts: Level 1 (0 SP)
- Ends: Level 7 (12 SP)
- Can upgrade: 12 skills (21% of total)

---

### **Level 2: Dark Forest** (Target: 1,200 XP)

**Enemy Composition:**
```
Goblin Runner × 25     = 25 × 8 XP   = 200 XP
Shadow Lurker × 15     = 15 × 12 XP  = 180 XP
Archer Scout × 10      = 10 × 15 XP  = 150 XP
Sword Soldier × 12     = 12 × 20 XP  = 240 XP
Knight Warrior × 8     = 8 × 30 XP   = 240 XP
Berserker × 4          = 4 × 50 XP   = 200 XP
Death Knight (Boss)    = 1 × 150 XP  = 150 XP (boosted boss)
------------------------------------------------------
Total: 75 enemies      = 1,360 XP
```

**Breakdown:**
- Collision-only: 40 enemies (53%)
- Weapon enemies: 34 enemies (45%)
- Boss: 1 (2%)

**Player Progression:**
- Starts: Level 7 (12 SP)
- Ends: Level 17 (32 SP)
- Can upgrade: 20 more skills (total 32 = 58%)

---

### **Level 3: Ancient Temple** (Target: 2,000 XP)

**Enemy Composition:**
```
Shadow Lurker × 20     = 20 × 12 XP  = 240 XP
Archer Scout × 12      = 12 × 15 XP  = 180 XP
Sword Soldier × 15     = 15 × 20 XP  = 300 XP
Knight Warrior × 15    = 15 × 30 XP  = 450 XP
Berserker × 12         = 12 × 50 XP  = 600 XP
Death Knight × 2       = 2 × 100 XP  = 200 XP (elite guards)
Final Boss (Custom)    = 1 × 250 XP  = 250 XP
------------------------------------------------------
Total: 77 enemies      = 2,220 XP
```

**Breakdown:**
- Collision-only: 20 enemies (26%)
- Weapon enemies: 56 enemies (73%)
- Boss: 1 (1%)

**Player Progression:**
- Starts: Level 17 (32 SP)
- Ends: Level 28 (54 SP)
- Can upgrade: 22 more skills (total 54 = 98%)

**Wait, 98% is too high! Let me adjust...**

---

## 🎯 FINAL BALANCED XP DISTRIBUTION

### **Adjusted XP to Hit 80% Coverage**

**Total Skills Needed:**
- 11 skills × 5 levels = 55 total upgrades
- 80% of 55 = 44 SP needed
- At 2 SP/level = 22 player levels
- Total XP for 22 levels:
  - Σ(60 + 30×(n-1)) for n=1 to 22
  - = 1,320 + 6,930 = **8,250 XP total**

**Distribution:**
- Level 1: 2,400 XP (29%)
- Level 2: 2,800 XP (34%)
- Level 3: 3,050 XP (37%)
- **Total: 8,250 XP**

### **Level 1: Village Outskirts** (2,400 XP Target)

```
Weak Slime × 100       = 100 × 5 XP   = 500 XP
Goblin Runner × 60     = 60 × 8 XP    = 480 XP
Archer Scout × 30      = 30 × 15 XP   = 450 XP
Sword Soldier × 40     = 40 × 20 XP   = 800 XP
Knight Warrior (Boss)  = 1 × 200 XP   = 200 XP (buffed boss)
------------------------------------------------------
Total: 231 enemies     = 2,430 XP ✅
```

**Player Progression:** Level 1 → Level 8 (14 SP, 25% coverage)

---

### **Level 2: Dark Forest** (2,800 XP Target)

```
Goblin Runner × 50     = 50 × 8 XP    = 400 XP
Shadow Lurker × 40     = 40 × 12 XP   = 480 XP
Archer Scout × 25      = 25 × 15 XP   = 375 XP
Sword Soldier × 30     = 30 × 20 XP   = 600 XP
Knight Warrior × 20    = 20 × 30 XP   = 600 XP
Berserker × 6          = 6 × 50 XP    = 300 XP
Death Knight (Boss)    = 1 × 250 XP   = 250 XP (buffed boss)
------------------------------------------------------
Total: 172 enemies     = 3,005 XP ✅
```

**Player Progression:** Level 8 → Level 16 (32 SP, 58% coverage)

---

### **Level 3: Ancient Temple** (3,050 XP Target)

```
Shadow Lurker × 30     = 30 × 12 XP   = 360 XP
Archer Scout × 20      = 20 × 15 XP   = 300 XP
Sword Soldier × 25     = 25 × 20 XP   = 500 XP
Knight Warrior × 30    = 30 × 30 XP   = 900 XP
Berserker × 20         = 20 × 50 XP   = 1,000 XP
Death Knight × 2       = 2 × 100 XP   = 200 XP (elite guards)
Final Boss (Custom)    = 1 × 300 XP   = 300 XP
------------------------------------------------------
Total: 128 enemies     = 3,560 XP
```

**ADJUSTED to hit 3,050 XP:**

```
Shadow Lurker × 25     = 25 × 12 XP   = 300 XP
Archer Scout × 15      = 15 × 15 XP   = 225 XP
Sword Soldier × 20     = 20 × 20 XP   = 400 XP
Knight Warrior × 25    = 25 × 30 XP   = 750 XP
Berserker × 18         = 18 × 50 XP   = 900 XP
Death Knight × 2       = 2 × 100 XP   = 200 XP
Final Boss (Custom)    = 1 × 300 XP   = 300 XP
------------------------------------------------------
Total: 106 enemies     = 3,075 XP ✅
```

**Player Progression:** Level 16 → Level 23 (44 SP, 80% coverage) ✅

---

## 🌟 Skill Tree (11 Skills, 5 Levels Each)

### **Recommended Skill Path (44 SP Budget)**

**Core Survival (20 SP):**
1. Vitality (Max HP): 5/5 = 5 SP → +100 HP (200 total)
2. Iron Skin (Armor): 5/5 = 5 SP → +25 AR
3. Swift Feet (Move Speed): 5/5 = 5 SP → +2.5 MS (6.5 total)
4. Vampirism (Lifesteal): 5/5 = 5 SP → +25% lifesteal

**Combat Power (15 SP):**
5. Wide Slash (Slash Arc): 5/5 = 5 SP → +50° arc
6. Stunning Blow (Stun Time): 3/5 = 3 SP → +30% stun
7. Combat Mobility (Move Penalty): 4/5 = 4 SP → +20% less penalty
8. Extended Reach (Thrust Distance): 3/5 = 3 SP → +30% thrust

**Utility (9 SP):**
9. Magic Shell (Magic Resist): 3/5 = 3 SP → +15 MR
10. Immovable (Knockback Resist): 3/5 = 3 SP → +15 KR
11. Mana Pool (Max Mana): 3/5 = 3 SP → +60 MP (160 total)

**Total: 44 SP** ✅

**What Player Gives Up (11 SP):**
- Stunning Blow levels 4-5 (2 SP)
- Extended Reach levels 4-5 (2 SP)
- Combat Mobility level 5 (1 SP)
- Magic Shell levels 4-5 (2 SP)
- Immovable levels 4-5 (2 SP)
- Mana Pool levels 4-5 (2 SP)

This forces strategic choices! ✅

---

## 📊 Summary Tables

### **XP Per Enemy Type**

| Enemy Type       | XP | Drop % | Typical Loot              |
|------------------|----|--------|---------------------------|
| Weak Slime       | 5  | 10%    | Potions, Gold             |
| Goblin Runner    | 8  | 10%    | Potions, Gold, Dagger     |
| Shadow Lurker    | 12 | 10%    | Mana, Cloak, Gold         |
| Archer Scout     | 15 | 50%    | Bow, Arrows, Gold         |
| Sword Soldier    | 20 | 50%    | Sword, Health, Gold       |
| Knight Warrior   | 30 | 50%    | Longsword, Armor, Gold    |
| Berserker        | 50 | 75%    | Axe, Heavy Armor, Gold    |
| Death Knight     | 100| 75%    | Legendary Gear            |
| Boss (Custom)    | 200-300 | 100% | Unique Rewards      |

---

### **Level Progression**

| Map Level | Enemies | Total XP | Player Level | Skill Points | Coverage |
|-----------|---------|----------|--------------|--------------|----------|
| Level 1   | 231     | 2,430    | 1 → 8        | 14 SP        | 25%      |
| Level 2   | 172     | 3,005    | 8 → 16       | 32 SP        | 58%      |
| Level 3   | 106     | 3,075    | 16 → 23      | 44 SP        | 80%      |
| **Total** | **509** | **8,510**| **23**       | **44 SP**    | **80%**  |

---

## ✅ Implementation Checklist

### **1. Update E_Reward Component**

Each enemy needs these XP values:

**Collision-Only:**
```
Weak Slime → expReward: 5, dropChance: 10%
Goblin Runner → expReward: 8, dropChance: 10%
Shadow Lurker → expReward: 12, dropChance: 10%
```

**Weapon Enemies:**
```
Archer Scout → expReward: 15, dropChance: 50%
Sword Soldier → expReward: 20, dropChance: 50%
Knight Warrior → expReward: 30, dropChance: 50%
Berserker → expReward: 50, dropChance: 75%
Death Knight → expReward: 100, dropChance: 75%
```

**Bosses:**
```
Knight Warrior Boss (L1) → expReward: 200, dropChance: 100%
Death Knight Boss (L2) → expReward: 250, dropChance: 100%
Final Boss (L3) → expReward: 300, dropChance: 100%
```

---

### **2. Create Enemy Prefabs**

Use stats from sections above for each enemy type.

---

### **3. Create Weapon ScriptableObjects**

Use weapon stats from sections above.

---

### **4. Place Enemies in Levels**

Use enemy counts from distribution tables above.

---

### **5. Test Balance**

- Player should feel challenged but not overwhelmed
- Level 1: Tutorial difficulty
- Level 2: Intermediate challenge
- Level 3: Expert gameplay required

---

## 🎯 Design Notes

**Why 80% Skill Coverage?**
- Forces meaningful choices
- Encourages replayability (different builds)
- Prevents "I have everything" syndrome
- Creates build identity (tank vs DPS vs speed)

**Why More Enemies in Level 1?**
- Teaches combat mechanics
- Builds muscle memory
- Feels like clearing out
- Early game power fantasy

**Why Fewer but Stronger Enemies in Level 3?**
- Quality over quantity
- Elite encounters
- Boss rush feeling
- Tests mastery

**Collision-Only Enemy Philosophy:**
- Easy to kill (tutorial/fodder)
- Low XP (5-12)
- Low drop rate (10%)
- Fills space, creates atmosphere
- Makes weapon enemies feel special

**Weapon Enemy Philosophy:**
- Challenge encounters
- High XP (15-100)
- Better loot (50-75%)
- Strategic combat
- Rewarding to defeat

---

## 🔄 Tuning Tips

**If player levels too fast:**
- Reduce XP by 10-20%
- Reduce enemy count by 10%

**If player levels too slow:**
- Increase XP by 10-20%
- Add more collision enemies

**If enemies too easy:**
- Increase enemy HP by 20%
- Add +2 AD to weapons

**If enemies too hard:**
- Decrease enemy damage by 15%
- Increase player base HP to 120

**If skill points feel scarce:**
- Increase skillPointsPerLevel to 3 (gives 66 SP total = 120% coverage)
- Or add skill point rewards from bosses

**If skill points feel abundant:**
- Decrease skillPointsPerLevel to 1 (gives 22 SP total = 40% coverage, VERY tight)

---

## 📝 Final Summary

- **3 Map Levels** with increasing difficulty
- **509 Total Enemies** across all levels
- **8,510 Total XP** available
- **Player reaches Level 23** with 44 SP
- **80% Skill Coverage** (44/55 upgrades)
- **Two Enemy Types:**
  - Collision-only: Weak, common, 10% drops
  - Weapon enemies: Strong, rare, 50-75% drops
- **Clear progression** from weak slimes to death knights

This creates a balanced, rewarding progression where player choice matters! 🎮✨
