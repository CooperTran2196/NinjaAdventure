# NinjaAdventure - Game Balance Guide

**Last Updated:** November 4, 2025  
**Maps:** 3 (Village, Forest, Temple)  
**Target:** Player reaches Level 23 after Map 3 (80% skill coverage)

---

## 🎮 Player Base Stats

### Starting Stats (Level 1)
```
maxHP: 100
AD: 5 (base attack, added to weapon AD)
MS: 4.0
maxMP: 100
```

### Progression
```
XP Formula: Level N → N+1 = 60 + (30 × (N-1)) XP
Skill Points: 2 SP per level

Total XP Needed:
L1 → L8: 1,050 XP (Map 1 target)
L8 → L15: 2,520 XP (Map 2 target)
L15 → L23: 4,680 XP (Map 3 target)
```

---

## ⚔️ Weapon Stats & Progression

### Weapon Unlock Order
```
Map 1: Stick (starter) → Katana (boss drop)
Map 2: Axe + Lance (mini-boss drops)
Map 3: Sword2 (boss drop)
```

### Complete Weapon Table

| Weapon      | AD  | Speed | Combo Multipliers | Total Combo | Player Type        | Notes                          |
|-------------|-----|-------|-------------------|-------------|--------------------|---------------------------------|
| Stick       | 1   | +3    | 1.0/1.2/2.0       | 25.2        | Starter            | Fast, weak training weapon      |
| Katana      | 2   | 0     | 1.2/1.2/2.0       | 30.8        | Balanced           | Map 1 boss drop, baseline       |
| Axe         | 10  | -6    | 1.2/1.5/1.0       | 73.8        | Tank/Brute         | Highest damage, slowest         |
| Lance       | 6   | -5    | 1.2/1.5/2.0       | 52.8        | Skilled/Spacing    | 2nd damage, long range          |
| Sword2      | 4   | +1    | 1.0/1.2/2.0       | 42.0        | Normal/All-Around  | Map 3 boss drop, balanced       |
| Bow         | 2   | 0     | 1.0/1.2/2.0       | 29.4        | Ranged             | Safe, no mana                   |
| Shuriken    | 3   | 0     | 1.0/1.2/2.0       | 33.6        | Ranged             | 10 mana cost                    |
| Sai         | 0   | +2    | 1.0/1.5/2.0       | -           | Enemy Only         | SamuraiBlue, AD from enemy stats|
| CFG_Katana  | 0   | 0     | 1.0/1.2/2.0       | -           | Mini-Boss          | AD from enemy stats             |

---

## 🗺️ MAP 1: VILLAGE

**Target XP:** 1,050 XP | **Level Progress:** 1 → 8 (+7 levels, 14 SP total)  
**Total Enemies:** 11 (6 no-weapon + 5 weapon + 1 mini-boss)

| Enemy Type        | Count | HP  | MS  | Collision DMG | Weapon          | Single Attack DMG | XP  | Drop Rate | Notes                                   |
|-------------------|-------|-----|-----|---------------|-----------------|-------------------|-----|-----------|------------------------------------------|
| Spider Green      | 3     | 60  | 2.5 | 5             | None            | -                 | 40  | 5%        | Basic melee, 5 dmg per touch             |
| Bamboo Green      | 3     | 70  | 2.5 | 6             | None            | -                 | 40  | 5%        | Basic melee, 6 dmg per touch             |
| SamuraiBlue       | 5     | 120 | 2.0 | 5             | Sai (AD 0)      | 10.5              | 100 | 40%       | Melee, 7×1.5 = 10.5 dmg per hit          |
| Samurai (Bow)     | 2     | 100 | 2.0 | 4             | Bow (AD 2)      | 7.0               | 100 | 40%       | Ranged 2s cooldown, (5+2)×1.0 = 7 dmg    |
| CamouflageGreen   | 1     | 450 | 1.8 | 8             | CFG_Katana (AD 0) | 10.8            | 310 | 100%      | Mini-Boss, 9×1.2 = 10.8 dmg per hit      |

**XP Distribution:** 3×40 + 3×40 + 5×100 + 2×100 + 1×310 = **1,050 XP** ✅

**Balance Notes:**
- Player: 100 HP, Stick single hit: (5+1)×1.0 = 6 dmg → kills Spider in 10 hits, Bamboo in 12 hits
- Spider/Bamboo: 60-70 HP, 5-6 dmg per touch → need 17-20 touches to kill player (manageable with dodge)
- SamuraiBlue: 120 HP, 10.5 dmg per swing → kills player in ~10 hits, player needs 20 Stick hits
- Samurai (Bow): 100 HP, 7 dmg per shot (every 2s) → kills player in 15 shots = 30 seconds, slow threat
- CamouflageGreen: 450 HP, 10.8 dmg per swing → kills player in ~10 hits, boss fight balanced with Katana drop

---

## 🌲 MAP 2: DARK FOREST

**Target XP:** 2,520 XP | **Level Progress:** 8 → 15 (+7 levels, 28 SP total)  
**Total Enemies:** 38 (21 no-weapon + 15 weapon + 2 mini-bosses)

| Enemy Type          | Count | HP  | MS  | Collision DMG | Weapon          | Single Attack DMG | XP  | Drop Rate | Notes                                      |
|---------------------|-------|-----|-----|---------------|-----------------|-------------------|-----|-----------|---------------------------------------------|
| Spider Yellow       | 7     | 90  | 2.5 | 7             | None            | -                 | 40  | 6%        | Basic melee, 7 dmg per touch                |
| Bamboo Yellow       | 7     | 100 | 2.5 | 8             | None            | -                 | 40  | 6%        | Basic melee, 8 dmg per touch                |
| Lizard              | 4     | 95  | 2.5 | 7             | None            | -                 | 40  | 7%        | Ground enemy, 7 dmg per touch               |
| Bat Yellow          | 3     | 80  | 2.5 | 6             | None            | -                 | 40  | 6%        | Flying, 6 dmg per touch                     |
| Samurai (Bow)       | 6     | 140 | 2.0 | 6             | Bow (AD 2)      | 7.0               | 80  | 55%       | Ranged 2s cooldown, (5+2)×1.0 = 7 dmg       |
| SamuraiRed          | 6     | 150 | 2.0 | 7             | Katana (AD 2)   | 8.4               | 80  | 60%       | Melee weapon, (5+2)×1.2 = 8.4 dmg per hit   |
| NinjaMageBlack      | 3     | 120 | 2.0 | 5             | Shuriken (AD 3) | 8.0               | 96  | 50%       | Ranged 10 mana, (5+3)×1.0 = 8 dmg, +20% XP  |
| Mini-Boss 1         | 1     | 600 | 1.6 | 10            | TBD             | TBD               | 522 | 0%        | No drop for now                             |
| Mini-Boss 2         | 1     | 600 | 1.6 | 10            | TBD             | TBD               | 522 | 0%        | No drop for now                             |

**XP Distribution:** 21×40 + 12×80 + 3×96 + 2×522 = 840 + 960 + 288 + 1,044 = **2,520 XP** ✅

**Balance Notes:**
- Player at Level 8: ~100 HP (base) + skill upgrades, has Katana, single hit: (5+2)×1.2 = 8.4 dmg
- No-weapon enemies: 80-100 HP, 6-8 dmg per touch → need 13-17 touches to kill player (dodge is key)
- Samurai (Bow): 140 HP, 7 dmg per shot (every 2s) → kills player in 15 shots = 30 seconds, kiting possible
- SamuraiRed: 150 HP, 8.4 dmg per swing → EVEN match with player's Katana damage! Takes ~18 Katana hits
- NinjaMageBlack: 120 HP, 8 dmg per shuriken → kills player in 13 shots, needs ~15 Katana hits
- Mini-Bosses: 600 HP, 10 dmg per touch → need 10 touches to kill player, takes ~72 Katana hits to kill

**XP Distribution:** 32×25 + 16×50 + 2×460 = **2,520 XP** ✅

---

## 🏛️ MAP 3: ANCIENT TEMPLE

**Target XP:** 4,680 XP | **Level Progress:** 15 → 23 (+8 levels, 44 SP total = 80% skills)  
**Total Enemies:** 30 (20 no-weapon + 9 weapon + 1 final boss)

| Enemy Type          | Count | HP  | MS  | Collision DMG | Weapon          | Weapon Combo | XP    | Drop Rate | Notes                    |
|---------------------|-------|-----|-----|---------------|-----------------|--------------|-------|-----------|--------------------------|
| Spider Red          | 5     | 90  | 2.5 | 4             | None            | -            | 50    | 7%        | Elite melee              |
| Bamboo Red          | 5     | 100 | 2.5 | 5             | None            | -            | 50    | 7%        | Elite melee              |
| Lizard Elite        | 5     | 95  | 2.5 | 5             | None            | -            | 50    | 8%        | Elite ground             |
| Bat Red             | 5     | 80  | 2.5 | 4             | None            | -            | 50    | 7%        | Elite flying             |
| Samurai Elite       | 3     | 130 | 2.0 | 4             | Bow (AD 2)      | 29.4         | 80    | 60%       | Elite ranged             |
| NinjaMageRed        | 3     | 120 | 2.0 | 3             | Shuriken (AD 3) | 33.6         | 80    | 55%       | Elite ranged, 10 mana    |
| SamuraiGold         | 3     | 150 | 2.0 | 5             | Katana (AD 2)   | 30.8         | 80    | 60%       | Elite melee weapon       |
| Final Boss          | 1     | 800 | 1.5 | 8             | Sword2 (AD 4)   | 42.0         | 2,960 | 100%      | Drops Sword2             |

**XP Distribution:** 20×50 + 9×80 + 1×2,960 = **4,680 XP** ✅

---

## 📊 Complete Summary

### Total Stats (All 3 Maps)
```
Total Enemies: 99 (11 + 38 + 30 planned)
Total XP: 8,250 XP (1,050 + 2,520 + 4,680)
Player Levels: 1 → 23 (22 level-ups)
Skill Points: 44 SP earned
Skill Coverage: 44/55 = 80% ✅
```

### XP Per Enemy Type (Quick Reference)

**Map 1:**
- Spider/Bamboo (no-weapon): 40 XP each
- SamuraiBlue/Samurai (weapon): 100 XP each
- Mini-Boss: 310 XP (HP: 450)

**Map 2:**
- No-Weapon: 40 XP each
- Normal Weapon (Bow/Katana): 80 XP each
- Shuriken Weapon: 96 XP each (+20%)
- Mini-Bosses: 522 XP each (×2, HP: 600 each)

**Map 3:**
- No-Weapon: 50 XP each
- Weapon: 80 XP each
- Final Boss: 2,960 XP (HP: 800)

### Map Progression Summary

| Map | Total XP | Level Range | SP Earned | Total Enemies | Boss HP       |
|-----|----------|-------------|-----------|---------------|---------------|
| 1   | 1,050    | 1 → 8       | 14        | 11            | 450           |
| 2   | 2,520    | 8 → 15      | 28 total  | 38            | 600 (×2)      |
| 3   | 4,680    | 15 → 23     | 44 total  | 30            | 800           |

---

## 🔧 Unity Implementation Notes

### Weapon Asset Changes Required

Update these weapon `.asset` files in Unity:

| Weapon     | AD Change      | Speed Change           | Other Changes                                    |
|------------|----------------|------------------------|--------------------------------------------------|
| Stick      | Keep AD: 1     | ADD maxAttackSpeed: 3  | -                                                |
| Katana     | Keep AD: 2     | Change to speed: 0     | (was -2)                                         |
| Axe        | Change to AD: 10 | Keep speed: -6       | Much higher damage than Lance!                   |
| Lance      | Change to AD: 6 | Keep speed: -5        | -                                                |
| Sword2     | Change to AD: 4 | ADD speed: 1          | Change comboMovePenalties: [0.7, 0.6, 0.4]       |
| Bow1       | Change to AD: 2 | -                     | -                                                |
| Shuriken   | Change to AD: 3 | -                     | -                                                |
| CFG_Katana | Change to AD: 4 | -                     | -                                                |

### Enemy Setup in Unity

**Required Components:**
- `E_Stats` (HP, MS, collision damage)
- `E_Collision` (collision tick rate)
- `E_Controller` (AI behavior)
- `E_Reward` (XP amount, drop rate, weapon reference)

**Map 1 Special Note:**
- Player receives Katana **before** fighting CamouflageGreen mini-boss
- Implement as story/cutscene reward or automatic pickup

**Map 2 Special Note:**
- Mini-bosses don't drop weapons yet (TBD)
- Set drop rate to 0% for both

**Map 3 Special Note:**
- Final Boss drops Sword2 (100% drop rate)

---

**END OF GUIDE**
