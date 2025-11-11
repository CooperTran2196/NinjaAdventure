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
| 1 | MaxHP            | MaxHealth           | +20 HP    | 5     | +100 HP    |
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
- HP: 100 + 100 = **200 HP**
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
| Katana   | Lvl 1     | Melee  | 3  | 4  | 0.25   | 90   | 0     | 1.5/1.5/2.0 | 5            | 40.0                 | -          | 0               | NPC gift         |
| Lance    | Lvl 2     | Melee  | 6  | 5  | 0.70   | 130  | -5    | 1.2/1.5/2.0 | 11           | 79.9                 | -          | 0               | Boss drop        |
| Bow      | Lvl 2     | Ranged | 10 | 4  | 0.25   | 45   | 0     | Single Hit  | 11           | 21.0                 | 100        | 5               | Safe ranged      |
| Shuriken | Lvl 2     | Ranged | 20 | 5  | 0.25   | 45   | 0     | Single Hit  | 11           | 31.0                 | 100        | 10              | Homing, powerful |
| Axe      | Lvl 3     | Melee  | 10 | 5  | 0.25   | 120  | -6    | 1.7/1.7/1.0 | 15           | 110.0                | -          | 0               | Highest damage   |

**Damage Formula:** `(Player_AD + Weapon_AD) × Combo_Multiplier`  
**Total Combo Damage = Sum of all 3 hits at typical player AD for that level**  
**Ranged Weapons (Bow/Shuriken) = Single projectile hit, no combo**

**Player AD Progression:**
- **Level 1:** Base 5 AD (0 skill points in AD)
- **Level 7:** 5 + 6 = 11 AD (AD skill 3/5)
- **Level 21:** 5 + 10 = 15 AD (AD skill 5/5)

**Mana System:**
- Base MP: 100 (20 bow shots / 10 shuriken throws)
- With Mana 5/5: 150 MP (30 bow / 15 shuriken)
- **Bow:** 10 AD, single hit, 5 mana per shot
- **Shuriken:** 18 AD, single hit (homing), 10 mana per throw

**Acquisition:**
- **Stick:** Starter
- **Katana:** NPC gift (Level 1)
- **Lance:** GiantRaccoon drop (Level 2)
- **Bow/Shuriken:** Hidden treasures (Level 2)
- **Axe:** GiantSlime2 drop (Level 3)

---

## 🗺️ LEVEL 1: Tutorial Village

### Enemy Composition

| Enemy Type         | Count | HP    | AD  | MS  | XP  | Total XP | Notes                        |
|--------------------|-------|-------|-----|-----|-----|----------|------------------------------|
| **No-Weapon**      | 6     | 35    | 5   | 2.5 | 30  | 180      | Collision damage             |
| **With-Weapon**    | 6     | 170   | 10  | 2.0 | 60  | 360      | Single melee hit             |
| **CamouflageGreen**| 1     | 425   | 15  | 1.8 | 250 | 250      | Mini-boss                    |
| **GRS (Encounter)**| 1     | 2,000 | 25  | 1.5 | 0   | 0        | Unbeatable, tutorial death   |

**Total XP:** 790 (180 + 360 + 250) → Level 1 → 7 (12 SP, 22% coverage)

**Player @ Lvl 1:** 100 HP, 5 AD, Stick (AD 1)

### Hits-to-Kill (Player AD=5, Katana best weapon, 3 hits = 1 combo)

| Weapon | Combo<br>Damage | No-Weapon<br>(35 HP) | With-Weapon<br>(170 HP) | Green<br>(425 HP) | GRS<br>(2,000 HP) |
|--------|-----------------|----------------------|-------------------------|-------------------|-------------------|
| Stick  | 25.2            | 9 hits (3 combos)    | 27 hits (9 combos)      | 68 hits (23 combos) | 318 hits (106 combos)|
| Katana | 40.0            | 6 hits (2 combos)    | 13 hits (5 combos)      | 32 hits (11 combos) | 150 hits (50 combos) |

**Enemy Threat:** No-Weapon (20 hits), With-Weapon (10 hits), Green (7 hits), **GRS (4 hits)**

**Why GRS is Unbeatable:**
- GRS has **SAME STATS as Level 3 final boss** (2,000 HP, 25 AD)
- Player needs 50 Katana combos (150 individual hits) - impossible at Level 1
- GRS kills player in 4 hits
- Player has no skills, low damage, weak weapon
- **Guarantees tutorial death mechanic**

---

## 🌲 LEVEL 2: Dark Forest

### Enemy Composition

| Enemy Type          | Count | HP  | AD  | MS  | XP  | Total XP | Notes                   |
|---------------------|-------|-----|-----|-----|-----|----------|-------------------------|
| **No-Weapon**       | 29    | 48  | 7   | 2.5 | 12  | 348      | Tougher collision       |
| **With-Weapon**     | 19    | 290 | 13  | 2.0 | 22  | 418      | Elite guards            |
| **CamouflageRed**   | 1     | 400 | 18  | 1.8 | 183 | 183      | Mini-boss               |
| **GiantRaccoon**    | 1     | 800 | 22  | 1.6 | 183 | 183      | Boss, drops Lance       |

**Total XP:** 1,110 (348 + 418 + 366) → Level 7 → 14 (26 SP, 47% coverage)  
**Player @ Lvl 7:** ~130 HP, 11 AD (MaxHP 2/5 + AD 3/5), Katana (AD 3)

### Hits-to-Kill (Player AD=11, Lance best weapon, 3 hits = 1 combo)

| Weapon   | Combo<br>Damage | No-Weapon<br>(48 HP) | With-Weapon<br>(290 HP) | Red<br>(400 HP) | Raccoon<br>(800 HP) |
|----------|-----------------|----------------------|-------------------------|-----------------|---------------------|
| Katana   | 70.0            | 7 hits (3 combos)    | 13 hits (5 combos)      | 18 hits (6 combos) | 35 hits (12 combos)|
| Lance    | 79.9            | 6 hits (2 combos)    | 11 hits (4 combos)      | 15 hits (5 combos) | 31 hits (11 combos)|
| Bow      | 21.0            | 48 hits (48 shots)   | 290 hits (290 shots)    | 400 hits (400 shots)| 800 hits (800 shots)|
| Shuriken | 31.0            | 48 hits (48 throws)  | 290 hits (290 throws)   | 400 hits (400 throws)| 800 hits (800 throws)|

**Enemy Threat:** No-Weapon (18 hits), With-Weapon (10 hits), Red (7 hits), Raccoon (6 hits)

**Hidden Weapons:** Bow (forest chest), Shuriken (secret cave)  
**Enemy Distribution:** 29 No-Weapon + 19 With-Weapon + 2 Mini-bosses = 50 total

---

## 🏛️ LEVEL 3: Ancient Temple

### Enemy Composition

| Enemy Type          | Count | HP    | AD  | MS  | XP   | Total XP | Notes                   |
|---------------------|-------|-------|-----|-----|------|----------|-------------------------|
| **No-Weapon**       | 8     | 66    | 10  | 2.5 | 18   | 144      | Elite collision         |
| **With-Weapon**     | 18    | 495   | 16  | 2.0 | 35   | 630      | Elite guards            |
| **CamouflageGreen** | 1     | 600   | 15  | 1.8 | 75   | 75       | Easy mini-boss          |
| **CamouflageRed**   | 1     | 600   | 20  | 1.8 | 75   | 75       | Easy mini-boss          |
| **GiantSlime2**     | 1     | 1,200 | 25  | 1.5 | 250  | 250      | Mini-boss, drops Axe    |
| **GRS (Final)**     | 1     | 2,000 | 25  | 1.5 | 1000 | 1000     | Final boss (beatable!)  |

**Total XP:** 2,174 (144 + 630 + 75 + 75 + 250 + 1000) → Level 14 → 25+ (50+ SP at Lvl 25)  
**GRS XP:** 1000 (awarded after ending UI, game complete!)  
**Player @ Lvl 14:** ~160 HP, 13 AD (MaxHP 4/5 + AD 4/5), Lance (AD 6), 26 SP

### Hits-to-Kill (Player AD=15 at Level 21, Axe best weapon, 3 hits = 1 combo)

| Weapon   | Combo<br>Damage | No-Weapon<br>(66 HP) | With-Weapon<br>(495 HP) | Green<br>(600 HP) | Red<br>(600 HP) | Slime2<br>(1,200 HP) | GRS<br>(2,000 HP) |
|----------|-----------------|----------------------|-------------------------|-------------------|-----------------|----------------------|-------------------|
| Katana   | 90.0            | 8 hits (3 combos)    | 17 hits (6 combos)      | 20 hits (7 combos)| 20 hits (7 combos)| 40 hits (14 combos)| 67 hits (23 combos)|
| Lance    | 98.7            | 7 hits (3 combos)    | 15 hits (5 combos)      | 19 hits (7 combos)| 19 hits (7 combos)| 37 hits (13 combos)| 61 hits (21 combos)|
| Bow      | 25.0            | 66 hits (66 shots)   | 495 hits (495 shots)    | 600 hits (600 shots)| 600 hits (600 shots)| 1,200 hits (1,200 shots)| 2,000 hits (2,000 shots)|
| Shuriken | 35.0            | 66 hits (66 throws)  | 495 hits (495 throws)   | 600 hits (600 throws)| 600 hits (600 throws)| 1,200 hits (1,200 throws)| 2,000 hits (2,000 throws)|
| Axe      | 110.0           | 6 hits (2 combos)    | 14 hits (5 combos)      | 17 hits (6 combos)| 17 hits (6 combos)| 33 hits (11 combos)| 55 hits (19 combos)|

**Enemy Threat:** No-Weapon (18 hits), With-Weapon (11 hits), Green (12 hits), Red (9 hits), Slime2 (7 hits), **GRS (7 hits)**

**Boss Gauntlet:** Green (6 combos) → Red (6 combos) → Slime2 (11 combos, get Axe) → GRS (19 combos, final)

**Enemy Distribution:** 8 No-Weapon + 18 With-Weapon + 2 Easy Mini + 1 Slime2 + 1 GRS = 30 total

**Why GRS is Now Beatable:**
- Player AD: 5 → 15 (+200%)
- Player HP: 100 → 200 (+100%)
- Player has 50 AR, 25% lifesteal
- Axe combo: 110.0 damage vs Katana: 40.0 damage (Level 1)
- Only needs 19 Axe combos (55 hits) vs 50 Katana combos (150 hits) at Level 1
- **GRS has SAME STATS both encounters (2,000 HP, 25 AD) - impossible at L1, beatable at L21**

---

## 📊 Game Summary

| Metric           | Level 1 | Level 2 | Level 3 | Total  |
|------------------|---------|---------|---------|--------|
| No-Weapon        | 6       | 29      | 8       | 43     |
| With-Weapon      | 6       | 19      | 18      | 43     |
| Mini-Bosses      | 1       | 2       | 3       | 6      |
| Final Boss       | 1 (fake)| 0       | 1 (real)| 1      |
| **Total Enemies**| **14**  | **50**  | **30**  | **94** |
| **XP Available** | **790** |**1,110**|**2,174**|**4,074**|
| **Player Level** | 1 → 7   | 7 → 14  | 14 → 25+| 1 → 25+|
| **SP Gained**    | 12      | 14      | 22+     | 48+    |

### Player Power Curve

| Level | HP  | AD | Weapon | SP | Key Stats                             | Combo Damage |
|-------|-----|----|--------|----|---------------------------------------|--------------|
| 1     | 100 | 5  | Katana | 0  | Base stats                            | 40.0         |
| 7     | 140 | 11 | Lance  | 12 | +40 HP, +6 AD, +10% lifesteal         | 79.9         |
| 14    | 180 | 13 | Axe    | 26 | +80 HP, +8 AD, +20% lifesteal, +30 AR | 101.2        |
| 21    | 200 | 15 | Axe    | 44 | +100 HP, +10 AD, +25% lifesteal, +50 AR| 110.0       |
| 25+   | 200 | 15 | Axe    | 50+| Max skills                            | 110.0        |

### GRS Boss Evolution

| Stat      | Level 1 (Tutorial)   | Level 3 (Final)      | Change               |
|-----------|----------------------|----------------------|----------------------|
| HP        | 2,000                | 2,000                | **IDENTICAL STATS**  |
| AD        | 25                   | 25                   | **IDENTICAL STATS**  |
| Threat    | 4-shots player       | 8-shots player       | Player much tankier  |
| Player    | 5 AD, Katana, 0 SP   | 15 AD, Axe, 44 SP    | 3x AD, 2.75x combo dmg|
| Hits      | 150 hits (50 combos) | 55 hits (19 combos)  | 62% fewer combos     |
| Result    | **GUARANTEED LOSS**  | Skill-based victory  | Same boss, stronger player|
| Reward    | 0 XP (tutorial death)| 1000 XP (game complete!)| Massive XP reward |

---

**END OF DOCUMENT**
