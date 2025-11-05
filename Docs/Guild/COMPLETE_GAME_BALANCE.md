# Complete Game Balance Guide - NinjaAdventure

## 📊 Overview

**Design Philosophy:**
- 3 Map Levels with increasing difficulty
- Player can max ~80% of all skills (forces strategic choices)
- Two enemy types: Collision-only (weak, faster MS 2.5) and Weapon-wielding (strong, MS 2.0)
- Clear progression curve with meaningful weapon upgrades
- Armor/Magic system ignored for simplicity (pure HP-based balance)

---

## 🎮 Player Base Stats & Progression

### **Player Starting Stats (Level 1)**
```
maxHP: 100
currentHP: 100
AD: 5 (base attack damage, added to weapon AD)
AP: 0
MS: 4.0
maxMP: 100
currentMP: 100
AR: 0 (ignored for balance)
MR: 0 (ignored for balance)
KR: 10
lifesteal: 0.0
armorPen: 0.0 (for items only)
magicPen: 0.0 (for items only)
```

### **EXP System** (From `P_Exp.cs`)
```
Level 1 → 2: 60 XP
Level 2 → 3: 90 XP
Level 3 → 4: 120 XP
Level N → N+1: 60 + (30 × (N-1)) XP

Skill Points per Level: 2
```

### **Total Skill Points Needed**
```
Total Skills: 11 skills × 5 levels = 55 total upgrades
Target Coverage: 80% of 55 = 44 SP needed
Levels Required: 44 SP ÷ 2 SP/level = 22 levels
Total XP for 22 Levels: 8,250 XP

Distribution Across 3 Maps:
- Map 1: ~2,400 XP (reaches Level 8)
- Map 2: ~2,800 XP (reaches Level 16)
- Map 3: ~3,050 XP (reaches Level 23)
```

---

## ⚔️ Player Weapon Progression

### **Weapon Tier System**

**Player obtains weapons in this order:**

1. **Stick** (Starting Weapon)
2. **Katana** (Found in Map 1)
3. **Sword2** (Upgrade of Katana, found in Map 2)
4. **Bow** (Ranged option, found in Map 1-2)
5. **Shuriken** (Mana-based ranged, found in Map 2)
6. **Axe** (Heavy damage, found in Map 2)
7. **Lance** (Highest thrust damage, found in Map 2-3)

### **Weapon Stats Table**

| Weapon   | Type   | AD | KB | Thrust | Arc° | Combo Dmg       | Speed | Mana | Notes                          |
|----------|--------|----|----|--------|------|-----------------|-------|------|--------------------------------|
| Stick    | Melee  | 1  | 3  | 0.50   | 60   | 1.0/1.2/2.0     | Fast  | 0    | Starter, weak but fast         |
| Katana   | Melee  | 2  | 4  | 0.25   | 90   | 1.2/1.2/2.0     | Fast  | 0    | Balanced all-around            |
| Sword2   | Melee  | 3  | 4  | 0.25   | 45   | 1.0/1.2/2.0     | Fast  | 0    | Better Katana, tighter arc     |
| Sai      | Melee  | 2  | 4  | 0.30   | 90   | 1.0/1.5/2.0     | V.Fast| 0    | Fast attack speed (enemy use)  |
| Axe      | Melee  | 7  | 5  | 0.25   | 120  | 1.2/1.5/1.0     | Slow  | 0    | Highest base dmg, wide arc     |
| Lance    | Melee  | 7  | 5  | 0.70   | 130  | 1.2/1.5/2.0     | Slow  | 0    | Best thrust, highest final hit |
| Bow      | Ranged | 2  | 4  | 0.25   | 45   | 1.0/1.2/2.0     | Med   | 0    | Safe ranged, no mana           |
| Shuriken | Ranged | 3  | 5  | 0.25   | 45   | 1.0/1.2/2.0     | Fast  | 10   | High dmg ranged, costs mana    |
| CFG_Katana| Melee | 4  | 5  | 0.90   | 45   | 1.0/1.2/2.0     | Med   | 0    | Mini-boss weapon, longer reach |

### **Damage Calculation Formula**

```
Total Damage = (Player_AD + Weapon_AD) × Combo_Multiplier
```

**Examples with Player AD = 5:**

| Weapon   | Hit 1 | Hit 2 | Hit 3 (Finisher) | Total Combo |
|----------|-------|-------|------------------|-------------|
| Stick    | 6     | 7.2   | 12               | 25.2        |
| Katana   | 8.4   | 8.4   | 14               | 30.8        |
| Sword2   | 8     | 9.6   | 16               | 33.6        |
| Sai      | 7     | 10.5  | 14               | 31.5        |
| Axe      | 14.4  | 18    | 12               | 44.4        |
| Lance    | 14.4  | 18    | 24               | 56.4        |
| Bow      | 7     | 8.4   | 14               | 29.4        |
| Shuriken | 8     | 9.6   | 16               | 33.6        |

**Key Insights:**
- **Stick:** Weakest (25 total damage)
- **Katana/Sword2:** Balanced (31-34 damage)
- **Axe:** Heavy but slow (44 damage, frontloaded)
- **Lance:** Highest finisher damage (56 damage total!)
- **Shuriken:** Best ranged DPS but costs 10 mana per combo

---

## � Enemy Types & Balance

### **Design Rules**
- **Collision-Only Enemies:** MS 2.5, no weapons, lower HP, easier to kill
- **Weapon Enemies:** MS 2.0, armed, higher HP, dangerous
- **No AR/MR:** Pure HP-based damage (easier balance calculations)
- **Collision Damage:** 2-5 damage per 0.5s tick

---

### **Type 1: COLLISION-ONLY ENEMIES** (No Weapons)

#### **Spider (Green/Yellow variants)**
```
[Stats - Map 1: Green Spider]
maxHP: 30
AD: 0
AP: 0
MS: 2.5
AR: 0
MR: 0
KR: 5

collisionDamage: 2
collisionTick: 0.5

detectionRange: 5.0
attackRange: 0.5 (melee collision)

[Rewards]
expReward: 5
dropChance: 5%
lootTable: [Health Potion (60%), Gold (40%)]
```

**Combat Analysis:**
- Player needs: 5 hits with Stick to kill (30 HP ÷ 6 dmg)
- Enemy needs: 50 hits to kill player (100 HP ÷ 2 dmg)
- Threat Level: Very Low (swarm enemy)

```
[Stats - Map 2: Yellow Spider]
maxHP: 50
collisionDamage: 3
MS: 2.5
expReward: 8
All other stats same as Green Spider
```

---

#### **Bamboo (Green/Yellow variants)**
```
[Stats - Map 1: Bamboo Green]
maxHP: 40
AD: 0
AP: 0
MS: 2.5
AR: 0
MR: 0
KR: 8

collisionDamage: 3
collisionTick: 0.5

detectionRange: 6.0
attackRange: 0.5

[Rewards]
expReward: 6
dropChance: 5%
lootTable: [Mana Potion (50%), Gold (50%)]
```

**Combat Analysis:**
- Player needs: 6-7 hits with Stick
- Slightly tougher than Spider
- Threat Level: Low

```
[Stats - Map 2: Bamboo Yellow]
maxHP: 60
collisionDamage: 4
MS: 2.5
expReward: 10
All other stats same as Bamboo Green
```

---

#### **Lizard (Map 2 only)**
```
[Stats]
maxHP: 70
AD: 0
AP: 0
MS: 2.5
AR: 0
MR: 0
KR: 10

collisionDamage: 4
collisionTick: 0.5

detectionRange: 7.0
attackRange: 0.5

[Rewards]
expReward: 12
dropChance: 8%
lootTable: [Health Potion (40%), Mana Potion (30%), Gold (30%)]
```

**Combat Analysis:**
- Player needs: 11-12 hits with Stick, 4-5 hits with Katana
- Fast and tanky for collision enemy
- Threat Level: Medium (can corner player)

---

#### **Bat Yellow (Map 2 only)**
```
[Stats]
maxHP: 50
AD: 0
AP: 0
MS: 3.0 (faster!)
AR: 0
MR: 0
KR: 5

collisionDamage: 3
collisionTick: 0.5

detectionRange: 8.0
attackRange: 0.5

[Rewards]
expReward: 10
dropChance: 8%
lootTable: [Mana Potion (60%), Gold (40%)]
```

**Combat Analysis:**
- Very fast, hard to avoid
- Lower HP compensates for speed
- Threat Level: Medium (harassment)

---

### **Type 2: WEAPON-WIELDING ENEMIES** (Armed & Dangerous)

#### **SamuraiBlue (Map 1, Uses Sai)**
```
[Stats]
maxHP: 80
AD: 3
AP: 0
MS: 2.0
AR: 0
MR: 0
KR: 10

attackCooldown: 1.2
collisionDamage: 3
collisionTick: 0.5

detectionRange: 6.0
attackRange: 1.5

[Weapon: Sai]
Weapon_AD: 2
Total Combo Damage: (3+2) × (1.0+1.5+2.0) = 22.5 damage
knockbackForce: 4
thrustDistance: 0.3
slashArcDegrees: 90
maxAttackSpeed: +2 (very fast attacks!)
comboStunTimes: [0.7, 0.7, 0.7]

[Rewards]
expReward: 20
dropChance: 40%
lootTable: [Sai (50%), Health Potion (30%), Gold (20%)]
```

**Combat Analysis:**
- Player takes: ~7-8 damage per hit, 22.5 total combo (22% HP!)
- Player needs: 13 hits with Stick, 4-5 hits with Katana
- Threat Level: Medium-High (fast combos, good stun)

---

#### **CamouflageGreen (Map 1 Mini-Boss, Uses CFG_Katana 1)**
```
[Stats]
maxHP: 150
AD: 4
AP: 0
MS: 2.0
AR: 0
MR: 0
KR: 15

attackCooldown: 1.5
collisionDamage: 4
collisionTick: 0.5

detectionRange: 7.0
attackRange: 2.0 (longer reach!)

[Weapon: CFG_Katana 1]
Weapon_AD: 4
Total Combo Damage: (4+4) × (1.0+1.2+2.0) = 33.6 damage
knockbackForce: 5
thrustDistance: 0.9 (very long!)
slashArcDegrees: 45
comboStunTimes: [0.1, 0.2, 0.5]

[Rewards]
expReward: 50
dropChance: 100%
lootTable: [Katana (guaranteed drop), Health Potion (50%), Gold (30%)]
```

**Combat Analysis:**
- Player takes: 33.6 damage per combo (33% HP!)
- Mini-boss tier, long reach makes it hard to counter
- Player needs: 24 hits with Stick, 8-9 hits with Katana
- Threat Level: Boss (Map 1 finale)

---

#### **Samurai (Map 2, Uses Bow)**
```
[Stats]
maxHP: 60
AD: 2
AP: 0
MS: 2.0
AR: 0
MR: 0
KR: 8

attackCooldown: 1.8
collisionDamage: 2
collisionTick: 0.5

detectionRange: 10.0 (long range detection!)
attackRange: 8.0 (ranged)

[Weapon: Bow1]
Weapon_AD: 2
Total Combo Damage: (2+2) × (1.0+1.2+2.0) = 16.8 damage
projectileSpeed: 8
projectileLifetime: 5
stickOnHit: 3 (arrows stick!)

[Rewards]
expReward: 18
dropChance: 50%
lootTable: [Bow (60%), Arrows (30%), Gold (10%)]
```

**Combat Analysis:**
- Player takes: ~4 damage per arrow, 16.8 total combo
- Low HP to compensate for ranged safety
- Player needs: 3-4 hits with Katana to kill
- Threat Level: Medium (kiting, ranged harassment)

---

#### **SamuraiRed (Map 2, Uses Katana)**
```
[Stats]
maxHP: 100
AD: 4
AP: 0
MS: 2.0
AR: 0
MR: 0
KR: 12

attackCooldown: 1.2
collisionDamage: 4
collisionTick: 0.5

detectionRange: 6.0
attackRange: 1.5

[Weapon: Katana]
Weapon_AD: 2
Total Combo Damage: (4+2) × (1.2+1.2+2.0) = 26.4 damage
knockbackForce: 4
thrustDistance: 0.25
slashArcDegrees: 90
maxAttackSpeed: -2 (slightly slower)
comboStunTimes: [0.7, 0.7, 1.0]

[Rewards]
expReward: 25
dropChance: 50%
lootTable: [Sword2 (40%), Health Potion (30%), Gold (30%)]
```

**Combat Analysis:**
- Player takes: 26.4 damage per combo (26% HP)
- Stronger than SamuraiBlue, good stun
- Player needs: 6-7 hits with Katana to kill
- Threat Level: High (mid-game challenge)

---

#### **NinjaMageBlack (Map 2, Uses Shuriken - LOW HP!)**
```
[Stats]
maxHP: 50 (intentionally low!)
AD: 3
AP: 0
MS: 2.0
AR: 0
MR: 0
KR: 8

attackCooldown: 1.5
collisionDamage: 3
collisionTick: 0.5
manaCost: 0 (enemies don't use mana!)

detectionRange: 9.0
attackRange: 7.0 (ranged)

[Weapon: Shuriken]
Weapon_AD: 3
Total Combo Damage: (3+3) × (1.0+1.2+2.0) = 25.2 damage
projectileSpeed: 10
projectileLifetime: 1.5
knockbackForce: 5

[Rewards]
expReward: 22
dropChance: 60%
lootTable: [Shuriken (70%), Mana Potion (20%), Gold (10%)]
```

**Combat Analysis:**
- Player takes: 25.2 damage per combo (25% HP)
- **LOW HP = Easy to kill!** (2-3 hits with Katana)
- Great weapon (Shuriken) balanced by fragility
- Threat Level: Medium-High (high damage, low survivability)

---

#### **CamouflageRed (Map 2-3 Mini-Boss, Uses Axe or Lance)**

**Option A: Axe Variant**
```
[Stats]
maxHP: 200
AD: 5
AP: 0
MS: 1.5 (slower, heavy weapon)
AR: 0
MR: 0
KR: 20

attackCooldown: 2.0
collisionDamage: 5
collisionTick: 0.5

detectionRange: 6.0
attackRange: 1.8

[Weapon: Axe]
Weapon_AD: 7
Total Combo Damage: (5+7) × (1.2+1.5+1.0) = 44.4 damage
knockbackForce: 5
thrustDistance: 0.25
slashArcDegrees: 120
maxAttackSpeed: -6 (very slow)
comboStunTimes: [1.0, 1.0, 1.0]

[Rewards]
expReward: 60
dropChance: 100%
lootTable: [Axe (80%), Heavy Armor (15%), Gold (5%)]
```

**Combat Analysis:**
- Player takes: 44.4 damage per combo (44% HP!!!)
- Slow but devastating, wide arc
- Player needs: 14-16 hits with Katana
- Threat Level: Boss (punishes mistakes)

---

**Option B: Lance Variant**
```
[Stats]
maxHP: 180
AD: 5
AP: 0
MS: 1.8
AR: 0
MR: 0
KR: 18

attackCooldown: 2.0
collisionDamage: 5
collisionTick: 0.5

detectionRange: 6.0
attackRange: 2.0 (long thrust!)

[Weapon: Lance]
Weapon_AD: 7
Total Combo Damage: (5+7) × (1.2+1.5+2.0) = 56.4 damage
knockbackForce: 5
thrustDistance: 0.7 (longest reach!)
slashArcDegrees: 130
maxAttackSpeed: -5 (slow)
comboStunTimes: [1.0, 1.0, 1.5]

[Rewards]
expReward: 70
dropChance: 100%
lootTable: [Lance (guaranteed), Legendary Item (20%), Gold]
```

**Combat Analysis:**
- Player takes: 56.4 damage per combo (56% HP!!!)
- Highest damage in game, long reach
- Player needs: 13-15 hits with Katana
- Threat Level: Elite Boss (requires mastery)

---

#### **FINAL BOSS (Map 3, Custom Weapon TBD)**

**Recommendation: Create "Death Lance" or "Demon Axe" - bigger sprite version**

**Stats Recommendation:**
```
[Stats]
maxHP: 300
AD: 6
AP: 0
MS: 1.5
AR: 0
MR: 0
KR: 25

attackCooldown: 2.5
collisionDamage: 6
collisionTick: 0.5

detectionRange: 8.0
attackRange: 2.5

[Weapon: Death Lance (NEW - Create this!)]
Weapon_AD: 10
Total Combo Damage: (6+10) × (1.5+2.0+3.0) = 104 damage
knockbackForce: 8
thrustDistance: 1.2
slashArcDegrees: 140
comboStunTimes: [1.5, 2.0, 2.5]

[Rewards]
expReward: 100
dropChance: 100%
lootTable: [Legendary Weapon, Unique Armor, Gold]
```

**Combat Analysis:**
- Can ONE-SHOT player if full combo lands!
- Requires dodge mastery, i-frames
- 3-phase fight recommended (HP thresholds: 200, 100, 0)
- Threat Level: LEGENDARY

---

## 📈 Level-by-Level Enemy Distribution

### **Map 1: Starting Village** (Target: 2,400 XP)

**Available Enemies:**
- Collision: Spider Green, Bamboo Green
- Weapon: SamuraiBlue (Sai)
- Mini-Boss: CamouflageGreen (CFG_Katana 1)

**Enemy Composition:**
```
Spider Green × 120        = 120 × 5 XP   = 600 XP
Bamboo Green × 100        = 100 × 6 XP   = 600 XP
SamuraiBlue × 50          = 50 × 20 XP   = 1,000 XP
CamouflageGreen (Boss) × 1 = 1 × 200 XP  = 200 XP
--------------------------------------------------------
Total: 271 enemies        = 2,400 XP ✅
```

**Breakdown:**
- Collision-only: 220 enemies (81%)
- Weapon enemies: 50 enemies (18%)
- Mini-Boss: 1 (1%)

**Player Progression:**
- Starts: Level 1 (0 SP, Stick weapon)
- Ends: Level 8 (14 SP)
- **Finds: Katana** (from CamouflageGreen)
- Can upgrade: 14 skills (25% coverage)

**Difficulty Curve:**
- Early: Mostly Spiders/Bamboo (tutorial, learn combat)
- Mid: Mix in SamuraiBlue (learn weapon combat)
- End: CamouflageGreen boss (test mastery)

---

### **Map 2: Dark Forest** (Target: 2,800 XP)

**Available Enemies:**
- Collision: Spider Yellow, Bamboo Yellow, Lizard, Bat Yellow
- Weapon: SamuraiRed (Katana), Samurai (Bow), NinjaMageBlack (Shuriken)
- Mini-Boss: CamouflageRed (Axe or Lance)

**Enemy Composition:**
```
Spider Yellow × 60        = 60 × 8 XP    = 480 XP
Bamboo Yellow × 50        = 50 × 10 XP   = 500 XP
Lizard × 30               = 30 × 12 XP   = 360 XP
Bat Yellow × 40           = 40 × 10 XP   = 400 XP
Samurai (Bow) × 20        = 20 × 18 XP   = 360 XP
NinjaMageBlack × 15       = 15 × 22 XP   = 330 XP
SamuraiRed × 20           = 20 × 25 XP   = 500 XP
CamouflageRed (Boss) × 1  = 1 × 70 XP    = 70 XP
--------------------------------------------------------
Total: 236 enemies        = 3,000 XP (slightly over, okay!)
```

**Breakdown:**
- Collision-only: 180 enemies (76%)
- Weapon enemies: 55 enemies (23%)
- Mini-Boss: 1 (1%)

**Player Progression:**
- Starts: Level 8 (14 SP, has Katana)
- Ends: Level 16 (32 SP)
- **Finds: Bow, Shuriken, Sword2, Axe/Lance** (from various enemies)
- Can upgrade: 32 skills total (58% coverage)

**Difficulty Curve:**
- Early: Yellow variants (tougher than Map 1)
- Mid: Mix of ranged enemies (Bow, Shuriken)
- Late: Dense SamuraiRed fights (melee skill test)
- End: CamouflageRed boss (heavy weapon introduction)

---

### **Map 3: Ancient Temple** (Target: 3,050 XP)

**Available Enemies:**
- Reuse Map 2 enemies + increase difficulty
- Add: Final Boss

**Enemy Composition (Adjust as needed):**
```
Lizard × 40               = 40 × 12 XP   = 480 XP
Bat Yellow × 50           = 50 × 10 XP   = 500 XP
Samurai (Bow) × 30        = 30 × 18 XP   = 540 XP
NinjaMageBlack × 25       = 25 × 22 XP   = 550 XP
SamuraiRed × 40           = 40 × 25 XP   = 1,000 XP
CamouflageRed × 3         = 3 × 70 XP    = 210 XP (elite guards)
FINAL BOSS × 1            = 1 × 100 XP   = 100 XP
--------------------------------------------------------
Total: 189 enemies        = 3,380 XP (over target)
```

**ADJUSTED to hit 3,050 XP:**
```
Lizard × 30               = 30 × 12 XP   = 360 XP
Bat Yellow × 40           = 40 × 10 XP   = 400 XP
Samurai (Bow) × 25        = 25 × 18 XP   = 450 XP
NinjaMageBlack × 20       = 20 × 22 XP   = 440 XP
SamuraiRed × 35           = 35 × 25 XP   = 875 XP
CamouflageRed × 3         = 3 × 70 XP    = 210 XP
FINAL BOSS × 1            = 1 × 150 XP   = 150 XP (buffed XP)
Spid Yellow × 20          = 20 × 8 XP    = 160 XP (filler)
--------------------------------------------------------
Total: 174 enemies        = 3,045 XP ✅
```

**Breakdown:**
- Collision-only: 90 enemies (52%)
- Weapon enemies: 83 enemies (48%)
- Bosses: 1 Final + 3 Elites (0.5%)

**Player Progression:**
- Starts: Level 16 (32 SP, multiple weapons)
- Ends: Level 23 (44 SP)
- **Final Reward: Legendary weapon from boss**
- Can upgrade: 44 skills total (80% coverage) ✅

**Difficulty Curve:**
- Heavy weapon enemy density
- Multiple elite mini-bosses
- Final boss gauntlet
- Requires build optimization & mastery

---

## 📊 Complete Summary Tables

### **XP Per Enemy Type**

| Enemy Type         | Map | XP | Drop % | Weapon         | Notes                  |
|--------------------|-----|----|--------|----------------|------------------------|
| Spider Green       | 1   | 5  | 5%     | None           | Swarm fodder           |
| Spider Yellow      | 2-3 | 8  | 5%     | None           | Tougher swarm          |
| Bamboo Green       | 1   | 6  | 5%     | None           | Basic enemy            |
| Bamboo Yellow      | 2-3 | 10 | 5%     | None           | Mid-tier collision     |
| Lizard             | 2-3 | 12 | 8%     | None           | Tanky collision        |
| Bat Yellow         | 2-3 | 10 | 8%     | None           | Fast harassment        |
| SamuraiBlue        | 1   | 20 | 40%    | Sai            | Fast melee             |
| Samurai            | 2-3 | 18 | 50%    | Bow            | Ranged threat          |
| SamuraiRed         | 2-3 | 25 | 50%    | Katana         | Elite melee            |
| NinjaMageBlack     | 2-3 | 22 | 60%    | Shuriken       | Low HP, high damage    |
| CamouflageGreen    | 1   | 200| 100%   | CFG_Katana 1   | Map 1 Boss             |
| CamouflageRed      | 2-3 | 70 | 100%   | Axe/Lance      | Mini-Boss              |
| FINAL BOSS         | 3   | 150| 100%   | Death Lance    | Ultimate Challenge     |

---

### **Level Progression Summary**

| Map   | Enemies | Total XP | Player Level | Skill Points | Coverage | Weapons Found             |
|-------|---------|----------|--------------|--------------|----------|---------------------------|
| Map 1 | 271     | 2,400    | 1 → 8        | 14 SP        | 25%      | Katana                    |
| Map 2 | 236     | 3,000    | 8 → 16       | 32 SP        | 58%      | Bow, Shuriken, Sword2, Axe/Lance |
| Map 3 | 174     | 3,045    | 16 → 23      | 44 SP        | 80%      | Legendary (Boss drop)     |
| **Total** | **681** | **8,445** | **23**   | **44 SP**    | **80%**  | **Full Arsenal**          |

---

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
1. **Vitality (Max HP):** 5/5 = 5 SP → +100 HP (200 total)
2. **Iron Skin (Armor):** 5/5 = 5 SP → +25 AR
3. **Swift Feet (Move Speed):** 5/5 = 5 SP → +2.5 MS (6.5 total)
4. **Vampirism (Lifesteal):** 5/5 = 5 SP → +25% lifesteal

**Combat Power (15 SP):**
5. **Wide Slash (Slash Arc):** 5/5 = 5 SP → +50° arc
6. **Stunning Blow (Stun Time):** 3/5 = 3 SP → +30% stun
7. **Combat Mobility (Move Penalty):** 4/5 = 4 SP → +20% less penalty
8. **Extended Reach (Thrust Distance):** 3/5 = 3 SP → +30% thrust

**Utility (9 SP):**
9. **Magic Shell (Magic Resist):** 3/5 = 3 SP → +15 MR
10. **Immovable (Knockback Resist):** 3/5 = 3 SP → +15 KR
11. **Mana Pool (Max Mana):** 3/5 = 3 SP → +60 MP (160 total)

**Total: 44 SP** ✅

**What Player Sacrifices (11 SP unused):**
- Stunning Blow levels 4-5 (2 SP)
- Extended Reach levels 4-5 (2 SP)
- Combat Mobility level 5 (1 SP)
- Magic Shell levels 4-5 (2 SP)
- Immovable levels 4-5 (2 SP)
- Mana Pool levels 4-5 (2 SP)

**Why This Works:**
- Forces meaningful choices (can't max everything)
- Encourages build variety (tank vs DPS vs speed)
- Replayability (try different 80% combinations)

---

## ✅ Implementation Checklist

### **1. Verify Weapon Stats in Unity**

Open each weapon `.asset` file and verify damage values:

**Melee Weapons:**
- `Stick.asset` → AD: 1 ✅ (already correct)
- `Sai.asset` → AD: 2 ✅ (already correct)
- `Katana.asset` → AD: 2 ✅ (already correct)
- `Sword2.asset` → **AD: 3** (currently 1, needs increase!)
- `Axe.asset` → **AD: 7** (currently 5, needs increase!)
- `Lance.asset` → AD: 7 ✅ (currently 5, needs increase!)
- `CFG_Katana 1.asset` → **AD: 4** (currently 1, mini-boss weapon!)

**Ranged Weapons:**
- `Bow1.asset` → **AD: 2** (currently 1, needs increase!)
- `Shuriken.asset` → **AD: 3** (currently 1, needs increase!)

### **2. Create Enemy Prefabs with E_Stats & E_Reward**

For each enemy, add these components:

**Example: Spider Green**
```csharp
// E_Stats component
maxHP: 30
AD: 0
MS: 2.5
KR: 5
(AR, MR, AP all 0)

// E_Collision component
collisionDamage: 2
collisionTick: 0.5

// E_Reward component
expReward: 5
dropChance: 5
// Assign loot table
```

**Example: SamuraiBlue**
```csharp
// E_Stats
maxHP: 80
AD: 3
MS: 2.0
KR: 10

// E_Collision
collisionDamage: 3
collisionTick: 0.5

// E_Reward
expReward: 20
dropChance: 40

// E_Controller
Assign Sai weapon to weapon slot
attackCooldown: 1.2
detectionRange: 6.0
attackRange: 1.5
```

### **3. Update Weapon ScriptableObjects**

**CRITICAL CHANGES NEEDED:**

Open Unity Editor → `Assets/GAME/Scripts/Weapon/W_Prefabs/`

**Sword2.asset:**
```
Change: AD: 1 → AD: 3
```

**Axe.asset:**
```
Change: AD: 5 → AD: 7
```

**Lance.asset:**
```
Change: AD: 5 → AD: 7
```

**Bow1.asset:**
```
Change: AD: 1 → AD: 2
```

**Shuriken.asset:**
```
Change: AD: 1 → AD: 3
```

**CFG_Katana 1.asset:**
```
Change: AD: 1 → AD: 4
```

### **4. Create Final Boss Weapon**

**Option 1: Death Lance (Recommended)**

Duplicate `Lance.asset` → Rename to `Death_Lance.asset`

```yaml
id: death_lance
sprite: [Use larger sprite, 2x scale]
offsetRadius: 1.2
AD: 10
knockbackForce: 8
thrustDistance: 1.2
slashArcDegrees: 140
comboShowTimes: [0.8, 1.0, 1.2]
comboDamageMultipliers: [1.5, 2.0, 3.0]
comboStunTimes: [1.5, 2.0, 2.5]
```

**Option 2: Demon Axe**

Duplicate `Axe.asset` → Rename to `Demon_Axe.asset`

```yaml
id: demon_axe
sprite: [Use larger sprite, 2x scale]
offsetRadius: 1.5
AD: 12
knockbackForce: 10
slashArcDegrees: 150
comboDamageMultipliers: [2.0, 2.5, 3.0]
comboStunTimes: [2.0, 2.0, 3.0]
```

### **5. Place Enemies in Maps**

**Map 1 Layout:**
```
Early Section: Spider Green (40) + Bamboo Green (30)
Mid Section: Mix Spiders/Bamboo (50) + SamuraiBlue (20)
Late Section: Dense SamuraiBlue (30)
Boss Arena: CamouflageGreen (1)
```

**Map 2 Layout:**
```
Early Section: Yellow variants (80)
Mid Section: Mix ranged (Samurai Bow 10, NinjaMageBlack 10)
Dense Combat: SamuraiRed (20)
Boss Arena: CamouflageRed (1)
```

**Map 3 Layout:**
```
Gauntlet 1: Mixed combat (50 enemies)
Gauntlet 2: Elite section (60 enemies)
Elite Guards: CamouflageRed × 3
Final Boss Arena: FINAL BOSS (1)
```

### **6. Configure Loot Tables**

**Create INV_LootTable ScriptableObjects:**

**Collision_Common:**
- Health Potion: 60%
- Mana Potion: 30%
- Gold: 10%

**Weapon_Rare:**
- Weapon (specific): 50-70%
- Health Potion: 20%
- Gold: 10-30%

**Boss_Legendary:**
- Guaranteed Weapon
- Rare Armor/Item: 50%
- Large Gold: 50%

---

## 🎯 Balance Verification Tests

### **Test 1: Early Game (Map 1, Level 1-3)**
**Player:** Stick weapon, no skills
**Target Enemy:** Spider Green (30 HP)
- Expected kills with full combo: 1-2 enemies
- Expected hits taken: 2-3 (4-6 damage)
- **Pass Condition:** Player can kill 5-10 Spiders without dying

### **Test 2: Mid Game (Map 1 End, Level 7)**
**Player:** Katana, 12 SP invested
**Target Enemy:** SamuraiBlue (80 HP)
- Expected combos to kill: 3 combos (31 × 3 = 93 damage)
- Expected damage taken per fight: 15-25 damage
- **Pass Condition:** Can fight 3 SamuraiBlue back-to-back

### **Test 3: Map 1 Boss**
**Player:** Katana, Level 8, 14 SP
**Target Enemy:** CamouflageGreen (150 HP, 34 dmg/combo)
- Expected combos to kill: 5-6 combos
- Expected hits taken: 2-3 combos (66-100 damage)
- **Pass Condition:** Beatable with 1-2 health potions

### **Test 4: Late Game (Map 2-3)**
**Player:** Lance/Axe, Level 16+, 32+ SP
**Target Enemy:** SamuraiRed (100 HP)
- Expected combos: 2 combos (56 × 2 = 112 damage)
- **Pass Condition:** Consistent 2-combo kills

### **Test 5: Final Boss**
**Player:** Best weapon, Level 23, 44 SP, lifesteal
**Target Enemy:** Final Boss (300 HP)
- Expected combos: 6-8 combos
- Expected pattern: 3 phases, dodge i-frames critical
- **Pass Condition:** Epic 2-3 minute fight, uses all skills

---

## 📊 Quick Reference Cards

### **Player Power Curve**

| Level | Map | Weapon   | Total Combo Dmg | HP  | Key Milestone           |
|-------|-----|----------|-----------------|-----|-------------------------|
| 1     | 1   | Stick    | 25              | 100 | Tutorial phase          |
| 5     | 1   | Stick    | 25              | 140 | First skill upgrades    |
| 8     | 1→2 | Katana   | 31              | 160 | Map 1 complete          |
| 12    | 2   | Sword2   | 34              | 180 | Core build forming      |
| 16    | 2→3 | Lance    | 56              | 200 | Power spike!            |
| 20    | 3   | Lance    | 56              | 200 | Elite encounters        |
| 23    | 3   | Lance    | 56              | 200 | Final boss ready        |

### **Enemy Threat Levels**

**Tier 1 (Fodder):** Spiders, Bamboo
- HP: 30-60
- Damage: 2-4/tick
- Threat: Very Low

**Tier 2 (Standard):** Lizard, Bat, Samurai (Bow)
- HP: 50-70
- Damage: 10-20/combo
- Threat: Medium

**Tier 3 (Elite):** SamuraiBlue, SamuraiRed, NinjaMageBlack
- HP: 50-100
- Damage: 22-26/combo
- Threat: High

**Tier 4 (Mini-Boss):** CamouflageGreen, CamouflageRed
- HP: 150-200
- Damage: 34-56/combo
- Threat: Boss

**Tier 5 (Final Boss):**
- HP: 300
- Damage: 100+/combo
- Threat: LEGENDARY

---

## 🔄 Tuning Tips

**If Map 1 too hard:**
- Reduce SamuraiBlue count to 30
- Increase early Spider count to 150
- Reduce CamouflageGreen HP to 120

**If Map 1 too easy:**
- Increase SamuraiBlue damage to AD: 4
- Add more weapon enemies (10-15 more)

**If Map 2 feels grindy:**
- Increase all XP rewards by 10%
- Reduce collision enemy counts by 20%
- Add more weapon enemy variety

**If Final Boss too hard:**
- Reduce combo damage multipliers (2.0/2.5/3.0 → 1.5/2.0/2.5)
- Increase HP to 400 but lower damage
- Add health recovery between phases

**If Shuriken/NinjaMageBlack unfair:**
- Increase NinjaMageBlack HP to 70
- Reduce Shuriken AD to 2
- Increase projectile lifetime (easier to dodge)

**If players run out of mana:**
- Increase Bat/Bamboo mana potion drop rate to 80%
- Reduce Shuriken mana cost to 5
- Add mana regeneration skill upgrade

---

## 📝 Final Summary

**Game Structure:**
- **3 Maps** with clear progression
- **681 Total Enemies** across all maps
- **8,445 Total XP** available
- **Player reaches Level 23** with 44 SP (80% skill coverage)

**Weapon Progression:**
1. Stick (starter, 25 total dmg)
2. Katana (Map 1 boss, 31 dmg)
3. Sword2 (Map 2 upgrade, 34 dmg)
4. Axe/Lance (Map 2 heavy, 44-56 dmg)
5. Bow/Shuriken (ranged options, 29-34 dmg)

**Enemy Types:**
- **Collision-Only:** 490 total (72%), MS 2.5, tutorial/swarm
- **Weapon Enemies:** 188 total (28%), MS 2.0, challenges
- **Bosses:** 3 total (1 per map)

**Design Achievements:**
- ✅ 80% skill coverage (forces choices)
- ✅ Clear weapon progression
- ✅ Balanced collision vs weapon ratio
- ✅ No AR/MR complexity (pure HP balance)
- ✅ Mana-cost weapons balanced by enemy HP
- ✅ Boss difficulty scales with player power

**Critical Balance Points:**
- **Map 1:** Learn fundamentals, unlock Katana
- **Map 2:** Arsenal expansion, build optimization
- **Map 3:** Mastery test, legendary rewards

This creates a tight, skill-focused progression where player choice matters! 🎮✨

---

## 🛠️ Next Steps for Implementation

1. **Update weapon AD values** in Unity (6 weapons need changes)
2. **Create enemy prefabs** with correct stats (12 unique enemies)
3. **Create Final Boss weapon** (Death Lance or Demon Axe)
4. **Place enemies in maps** following distribution tables
5. **Test Map 1** balance (should take ~15-20 minutes)
6. **Test Map 2** balance (should take ~20-25 minutes)
7. **Test Map 3** balance (should take ~25-30 minutes)
8. **Fine-tune based on playtesting** using tuning tips above

**Total expected gameplay time: 60-75 minutes for full game completion** ✅

