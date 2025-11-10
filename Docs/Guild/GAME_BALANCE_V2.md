# NinjaAdventure - Complete Game Balance Guide

**Version:** 3.0  
**Last Updated:** November 9, 2025  
**Status:** Production-Ready (3 Complete Levels)

---

## 🎮 Core Systems

### Player Progression
- **XP Formula:** Level N → N+1 = 60 + (30 × (N-1)) XP
- **Skill Points:** 2 SP per level
- **Target:** 44 SP (80% coverage) at Level 21 (before final boss)

### Player Base Stats

| Stat     | Value | Stat     | Value |
|----------|-------|----------|-------|
| maxHP    | 100   | maxMP    | 100   |
| AD       | 5     | AP       | 0     |
| MS       | 4.0   | AR       | 0     |
| KR       | 10    | MR       | 0     |
| Lifesteal| 0.0   | -        | -     |

### Difficulty Modes

| Mode   | Starting SP | Bonus Item | Coverage at Lvl 21 | Notes                    |
|--------|-------------|------------|--------------------|--------------------------| 
| Normal | 0           | None       | 44 SP (80%)        | Balanced challenge       |
| Easy   | +10         | Yes        | 54 SP (98%)        | Forgiving, more upgrades |

---

## 🌟 Skill Tree System

**Location:** `Assets/GAME/Scripts/SkillTree/ST_SO_Prefabs`  
**Total:** 11 Skills × Variable Levels = 55 Possible Upgrades

| # | Skill Name       | Stat                | Per Level | Max   | Total      |
|---|------------------|---------------------|-----------|-------|------------|
| 1 | MaxHP            | MaxHealth           | +15 HP    | 5     | +75 HP     |
| 2 | AD               | AttackDamage        | +2 AD     | 5     | +10 AD     |
| 3 | AR               | Armor               | +10 AR    | 5     | +50 AR     |
| 4 | MS               | MoveSpeed           | +0.5 MS   | 3     | +1.5 MS    |
| 5 | Lifesteal        | Lifesteal           | +5%       | 5     | +25%       |
| 6 | Mana             | MaxMana             | +10 MP    | 5     | +50 MP     |
| 7 | Wide Slash       | SlashArc            | +10°      | 5     | +50°       |
| 8 | Stunning Blow    | StunTimeBonus       | +10%      | 5     | +50%       |
| 9 | Combat Mobility  | MovePenaltyReduction| +5%       | 5     | +25%       |
| 10| Extended Reach   | ThrustDistanceBonus | +10%      | 5     | +50%       |
| 11| Others           | MR, KR, AP, AS      | Varies    | 5     | Varies     |

### SP Progression

| End of Level | Total SP | Coverage | Mode Comparison          |
|--------------|----------|----------|--------------------------|
| Level 7      | 12       | 22%      | Easy: 22 SP (40%)        |
| Level 14     | 26       | 47%      | Easy: 36 SP (65%)        |
| Level 21     | 44       | 80%      | Easy: 54 SP (98%)        |

### Recommended Build (44 SP at Level 21)

**Core Damage (15 SP):** MaxHP 5/5, AD 5/5, Lifesteal 5/5  
**Defense (15 SP):** AR 5/5, MS 3/3, Combat Mobility 5/5, Wide Slash 2/5  
**Utility (14 SP):** Stunning Blow 5/5, Extended Reach 5/5, Mana 4/5

**Player Stats at Level 21:**
- HP: 100 + 75 = **175 HP**
- AD: 5 + 10 = **15 AD** (base, before weapon)
- AR: 0 + 50 = **50 AR**
- Lifesteal: 25%
- MS: 4.0 + 1.5 = 5.5

---

## ⚔️ Weapons

### Complete Weapon Table

| Weapon   | Available | Type   | AD | KB | Thrust | Arc° | Speed | Combos      | Player<br>AD | Total Combo<br>Damage| Base<br>MP | MP per<br>Combo | Notes            |
|----------|-----------|--------|----|----|--------|------|-------|-------------|--------------|----------------------|------------|-----------------|------------------|
| Stick    | Lvl 1     | Melee  | 1  | 3  | 0.50   | 60   | +3    | 1.0/1.2/2.0 | 5            | 25.2                 | -          | 0               | Starter          |
| Katana   | Lvl 1     | Melee  | 2  | 4  | 0.25   | 90   | 0     | 1.2/1.2/2.0 | 5            | 29.4                 | -          | 0               | NPC gift         |
| Lance    | Lvl 2     | Melee  | 6  | 5  | 0.70   | 130  | -5    | 1.2/1.5/2.0 | 11           | 79.9                 | -          | 0               | Boss drop        |
| Bow      | Lvl 2     | Ranged | 1  | 4  | 0.25   | 45   | 0     | 1.0/1.2/2.0 | 11           | 50.4                 | 100        | 5               | Safe ranged      |
| Shuriken | Lvl 2     | Ranged | 1  | 5  | 0.25   | 45   | 0     | 1.0/1.2/2.0 | 11           | 50.4                 | 100        | 10              | Homing, powerful |
| Axe      | Lvl 3     | Melee  | 10 | 5  | 0.25   | 120  | -6    | 1.2/1.5/1.0 | 15           | 92.5                 | -          | 0               | Highest damage   |

**Damage Formula:** `(Player_AD + Weapon_AD) × Combo_Multiplier`  
**Total Combo Damage = Sum of all 3 hits at typical player AD for that level**

**Player AD Progression:**
- **Level 1:** Base 5 AD (0 skill points in AD)
- **Level 7:** 5 + 6 = 11 AD (AD skill 3/5)
- **Level 21:** 5 + 10 = 15 AD (AD skill 5/5)

**Mana System:**
- Base MP: 100 (20 bow combos / 10 shuriken combos)
- With Mana 5/5: 150 MP (30 bow / 15 shuriken)
- **Note:** Bow and Shuriken have same AD (1), shuriken costs double mana (10 vs 5)

**Acquisition:**
- **Stick:** Starter
- **Katana:** NPC gift (Level 1)
- **Lance:** GiantRaccoon drop (Level 2)
- **Bow/Shuriken:** Hidden treasures (Level 2)
- **Axe:** GiantSlime2 drop (Level 3)

---

## 🗺️ LEVEL 1: Tutorial Village

### Enemy Composition

| Enemy Type         | Count | HP    | AD  | MS  | XP  | Notes                        |
|--------------------|-------|-------|-----|-----|-----|------------------------------|
| **No-Weapon**      | 6     | 34    | 5   | 2.5 | 30  | Collision damage             |
| **With-Weapon**    | 6     | 170   | 10  | 2.0 | 60  | Single melee hit             |
| **CamouflageGreen**| 1     | 425   | 15  | 1.8 | 250 | Mini-boss                    |
| **GRS (Encounter)**| 1     | 2,475 | 25  | 1.5 | 0   | Unbeatable, tutorial death   |

**Total XP:** 790 (180 + 360 + 250) → Level 1 → 7 (12 SP, 22% coverage)

**Player @ Lvl 1:** 100 HP, 5 AD, Stick (AD 1)

### Hits-to-Kill (Player AD=5, Katana best weapon, 3 hits = 1 combo)

**Stick Combo Damage:** (5+1) × (1.0 + 1.2 + 2.0) = 6 × 4.2 = **25.2 per combo**  
**Katana Combo Damage:** (5+2) × (1.2 + 1.2 + 2.0) = 7 × 4.4 = **30.8 per combo**

| Weapon | No-Weapon (34 HP) | With-Weapon (170 HP) | Green (425 HP)  | GRS (2,475 HP)           |
|--------|-------------------|----------------------|-----------------|--------------------------|
| Stick  | 9 hits (3 combos) | 27 hits (9 combos)   | 68 hits (23 combos) | 393 hits (131 combos)|
| Katana | 8 hits (3 combos) | 17 hits (6 combos)   | 42 hits (14 combos) | 241 hits (81 combos) |

**Enemy Threat:** No-Weapon (20 hits), With-Weapon (10 hits), Green (7 hits), **GRS (4 hits)**

**Why GRS is Unbeatable:**
- GRS has **SAME STATS as Level 3 final boss** (2,475 HP, 25 AD)
- Player needs 81 Katana combos (241 individual hits) - impossible at Level 1
- GRS kills player in 4 hits
- Player has no skills, low damage, weak weapon
- **Guarantees tutorial death mechanic**

---

## 🌲 LEVEL 2: Dark Forest

### Enemy Composition

| Enemy Type          | Count | HP  | AD  | MS  | XP  | Notes                   |
|---------------------|-------|-----|-----|-----|-----|-------------------------|
| **No-Weapon**       | 29    | 48  | 7   | 2.5 | 12  | Tougher collision       |
| **With-Weapon**     | 19    | 290 | 13  | 2.0 | 22  | Elite guards            |
| **CamouflageRed**   | 1     | 483 | 18  | 1.8 | 183 | Mini-boss               |
| **GiantRaccoon**    | 1     | 966 | 22  | 1.6 | 183 | Boss, drops Lance       |

**Total XP:** 1,110 (348 + 418 + 366) → Level 7 → 14 (26 SP, 47% coverage)  
**Player @ Lvl 7:** ~125 HP, 9 AD (MaxHP 2/5 + AD 2/5), Katana (AD 2)

### Hits-to-Kill (Player AD=11, Lance best weapon, 3 hits = 1 combo)

**Katana Combo:** (11+2) × 4.4 = **57.2 per combo**  
**Lance Combo:** (11+6) × (1.2 + 1.5 + 2.0) = 17 × 4.7 = **79.9 per combo**  
**Bow Combo:** (11+1) × 4.2 = **50.4 per combo**  
**Shuriken Combo:** (11+1) × 4.2 = **50.4 per combo**

| Weapon   | No-Weapon (48 HP) | With-Weapon (290 HP) | Red (483 HP)     | Raccoon (966 HP)      |
|----------|-------------------|----------------------|------------------|-----------------------|
| Katana   | 9 hits (3 combos) | 15 hits (5 combos)   | 26 hits (9 combos)  | 51 hits (17 combos)|
| Lance    | 6 hits (2 combos) | 11 hits (4 combos)   | 19 hits (7 combos)  | 37 hits (13 combos)|
| Bow      | 9 hits (3 combos) | 18 hits (6 combos)   | 29 hits (10 combos) | 58 hits (20 combos)|
| Shuriken | 9 hits (3 combos) | 18 hits (6 combos)   | 29 hits (10 combos) | 58 hits (20 combos)|

**Enemy Threat:** No-Weapon (18 hits), With-Weapon (10 hits), Red (7 hits), Raccoon (6 hits)

**Hidden Weapons:** Bow (forest chest), Shuriken (secret cave)  
**Enemy Distribution:** 29 No-Weapon + 19 With-Weapon + 2 Mini-bosses = 50 total

---

## 🏛️ LEVEL 3: Ancient Temple

### Enemy Composition

| Enemy Type          | Count | HP    | AD  | MS  | XP  | Notes                   |
|---------------------|-------|-------|-----|-----|-----|-------------------------|
| **No-Weapon**       | 8     | 66    | 10  | 2.5 | 8   | Elite collision         |
| **With-Weapon**     | 39    | 495   | 16  | 2.0 | 18  | Elite guards            |
| **CamouflageGreen** | 1     | 825   | 15  | 1.8 | 75  | Easy mini-boss          |
| **CamouflageRed**   | 1     | 825   | 20  | 1.8 | 75  | Easy mini-boss          |
| **GiantSlime2**     | 1     | 1,650 | 25  | 1.5 | 250 | Mini-boss, drops Axe    |
| **GRS (Final)**     | 1     | 2,475 | 25  | 1.5 | 240 | Final boss (beatable!)  |

**Total XP:** 1,350 (64 + 702 + 75 + 75 + 250 + 240) → Level 14 → 23 (44 SP at Lvl 21)  
**Player @ Lvl 14:** ~150 HP, 13 AD (MaxHP 4/5 + AD 4/5), Lance (AD 6), 26 SP

### Hits-to-Kill (Player AD=15 at Level 21, Axe best weapon, 3 hits = 1 combo)

**Katana Combo:** (15+2) × 4.4 = **74.8 per combo**  
**Lance Combo:** (15+6) × 4.7 = **98.7 per combo**  
**Bow Combo:** (15+1) × 4.2 = **67.2 per combo**  
**Shuriken Combo:** (15+1) × 4.2 = **67.2 per combo**  
**Axe Combo:** (15+10) × (1.2 + 1.5 + 1.0) = 25 × 3.7 = **92.5 per combo**

| Weapon   | No-Weapon (66 HP) | With-Weapon (495 HP) | Green (825 HP)     | Red (825 HP)       | Slime2 (1,650 HP)   | GRS (2,475 HP)      |
|----------|-------------------|----------------------|--------------------|--------------------|---------------------|---------------------|
| Katana   | 9 hits (3 combos) | 20 hits (7 combos)   | 34 hits (12 combos)| 34 hits (12 combos)| 67 hits (23 combos) | 100 hits (34 combos)|
| Lance    | 7 hits (3 combos) | 15 hits (5 combos)   | 26 hits (9 combos) | 26 hits (9 combos) | 51 hits (17 combos) | 76 hits (26 combos) |
| Bow      | 10 hits (4 combos)| 22 hits (8 combos)   | 37 hits (13 combos)| 37 hits (13 combos)| 74 hits (25 combos) | 111 hits (37 combos)|
| Shuriken | 10 hits (4 combos)| 22 hits (8 combos)   | 37 hits (13 combos)| 37 hits (13 combos)| 74 hits (25 combos) | 111 hits (37 combos)|
| Axe      | 8 hits (3 combos) | 16 hits (6 combos)   | 27 hits (9 combos) | 27 hits (9 combos) | 54 hits (18 combos) | 81 hits (27 combos) |

**Enemy Threat:** No-Weapon (18 hits), With-Weapon (11 hits), Green (12 hits), Red (9 hits), Slime2 (7 hits), **GRS (7 hits)**

**Boss Gauntlet:** Green (9 combos) → Red (9 combos) → Slime2 (18 combos, get Axe) → GRS (27 combos, final)

**Enemy Distribution:** 8 No-Weapon + 39 With-Weapon + 2 Easy Mini + 1 Slime2 + 1 GRS = 51 total

**Why GRS is Now Beatable:**
- Player AD: 5 → 15 (+200%)
- Player HP: 100 → 175 (+75%)
- Player has 50 AR, 25% lifesteal
- Axe combo: 92.5 damage vs Katana: 30.8 damage (Level 1)
- Only needs 27 Axe combos (81 hits) vs 81 Katana combos (241 hits) at Level 1
- **GRS has SAME STATS both encounters (2,475 HP, 25 AD) - impossible at L1, beatable at L21**

---

## 📊 Game Summary

| Metric           | Level 1 | Level 2 | Level 3 | Total  |
|------------------|---------|---------|---------|--------|
| No-Weapon        | 6       | 29      | 8       | 43     |
| With-Weapon      | 6       | 19      | 39      | 64     |
| Mini-Bosses      | 1       | 2       | 3       | 6      |
| Final Boss       | 1 (fake)| 0       | 1 (real)| 1      |
| **Total Enemies**| **14**  | **50**  | **51**  | **115**|
| **XP Available** | **790** |**1,110**|**1,350**|**3,250**|
| **Player Level** | 1 → 7   | 7 → 14  | 14 → 23 | 1 → 23 |
| **SP Gained**    | 12      | 14      | 18      | 44     |

### Player Power Curve

| Level | HP  | AD | Weapon | SP | Key Stats                             | Combo Damage |
|-------|-----|----|--------|----|---------------------------------------|--------------|
| 1     | 100 | 5  | Katana | 0  | Base stats                            | 30.8         |
| 7     | 125 | 11 | Lance  | 12 | +25 HP, +6 AD, +10% lifesteal         | 79.9         |
| 14    | 150 | 13 | Axe    | 26 | +50 HP, +8 AD, +20% lifesteal, +30 AR | 85.1         |
| 21    | 175 | 15 | Axe    | 44 | +75 HP, +10 AD, +25% lifesteal, +50 AR| 92.5         |

### GRS Boss Evolution

| Stat      | Level 1 (Tutorial)   | Level 3 (Final)      | Change               |
|-----------|----------------------|----------------------|----------------------|
| HP        | 2,475                | 2,475                | **IDENTICAL STATS**  |
| AD        | 25                   | 25                   | **IDENTICAL STATS**  |
| Threat    | 4-shots player       | 7-shots player       | Player much tankier  |
| Player    | 5 AD, Katana, 0 SP   | 15 AD, Axe, 44 SP    | 3x AD, 3x combo dmg  |
| Hits      | 241 hits (81 combos) | 81 hits (27 combos)  | 67% fewer combos     |
| Result    | **GUARANTEED LOSS**  | Skill-based victory  | Same boss, stronger player|

---

**END OF DOCUMENT**
