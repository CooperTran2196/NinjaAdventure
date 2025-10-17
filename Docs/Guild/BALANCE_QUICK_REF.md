# Quick Setup Reference Card

## 🎮 Player Base Stats (Update in Inspector)

```
Player GameObject → C_Stats:
  AD: 5
  AP: 0
  MS: 5.0
  maxHP: 100
  currentHP: 100
  AR: 0
  MR: 0
  maxMP: 50
  currentMP: 0
  KR: 10
  lifesteal: 0
  armorPen: 0
  magicPen: 0
```

---

## 👾 Enemy Quick Stats (Copy-Paste Ready)

### Archer (Ranged, Glass Cannon)
```
maxHP: 30, AD: 3, MS: 3.5, AR: 0, MR: 0, KR: 5
attackCooldown: 1.5, collisionDamage: 2
detectionRange: 8.0, attackRange: 6.0
Weapon: Archer Bow (AD: 8, Ranged)
```

### Scout (Fast Melee)
```
maxHP: 50, AD: 4, MS: 6.0, AR: 2, MR: 0, KR: 8
attackCooldown: 0.9, collisionDamage: 3
detectionRange: 6.0, attackRange: 1.5
Weapon: Scout Blade (AD: 6, Melee, Fast combos)
```

### Warrior (Balanced)
```
maxHP: 80, AD: 5, MS: 4.0, AR: 5, MR: 2, KR: 12
attackCooldown: 1.2, collisionDamage: 4
detectionRange: 5.0, attackRange: 1.5
Weapon: Warrior Sword (AD: 10, Melee, Standard combos)
```

### Bruiser (Slow Tank)
```
maxHP: 120, AD: 7, MS: 3.0, AR: 8, MR: 4, KR: 20
attackCooldown: 1.5, collisionDamage: 5
detectionRange: 5.0, attackRange: 1.8
Weapon: Bruiser Axe (AD: 15, Melee, Heavy combos)
```

### Tank (Boss-tier)
```
maxHP: 200, AD: 8, MS: 2.5, AR: 12, MR: 8, KR: 30
attackCooldown: 2.0, collisionDamage: 6
detectionRange: 6.0, attackRange: 2.0
Weapon: Tank Hammer (AD: 20, Melee, Devastating combos)
```

---

## ⚔️ Weapon Stats Quick Copy

### Archer Bow (Ranged)
```
AD: 8, knockbackForce: 2.0
showTime: 0.3, attackMovePenalty: 0.8
projectileSpeed: 12.0, projectileLifetime: 2.0
```

### Scout Blade (Fast Melee)
```
AD: 6, knockbackForce: 3.0, onlyThrustKnocksBack: true
slashArcDegrees: 40, thrustDistance: 0.2
comboShowTimes: [0.35, 0.35, 0.35]
comboDamageMultipliers: [1.0, 1.2, 1.5]
comboMovePenalties: [0.7, 0.6, 0.4]
comboStunTimes: [0.05, 0.1, 0.2]
```

### Warrior Sword (Standard Melee)
```
AD: 10, knockbackForce: 5.0, onlyThrustKnocksBack: true
slashArcDegrees: 45, thrustDistance: 0.25
comboShowTimes: [0.45, 0.45, 0.45]
comboDamageMultipliers: [1.0, 1.2, 2.0]
comboMovePenalties: [0.6, 0.5, 0.3]
comboStunTimes: [0.1, 0.2, 0.5]
```

### Bruiser Axe (Heavy Melee)
```
AD: 15, knockbackForce: 8.0, onlyThrustKnocksBack: false
slashArcDegrees: 60, thrustDistance: 0.3
comboShowTimes: [0.6, 0.6, 0.6]
comboDamageMultipliers: [1.0, 1.5, 2.5]
comboMovePenalties: [0.5, 0.4, 0.2]
comboStunTimes: [0.15, 0.3, 0.7]
```

### Tank Hammer (Boss Weapon)
```
AD: 20, knockbackForce: 12.0, onlyThrustKnocksBack: false
slashArcDegrees: 90, thrustDistance: 0.4
comboShowTimes: [0.8, 0.8, 1.0]
comboDamageMultipliers: [1.0, 1.5, 3.0]
comboMovePenalties: [0.4, 0.3, 0.1]
comboStunTimes: [0.2, 0.4, 1.0]
```

---

## 📊 Balance at a Glance

| Enemy   | HP  | Dmg | Speed | Player Hits to Kill | Enemy Hits to Kill Player |
|---------|-----|-----|-------|---------------------|---------------------------|
| Archer  | 30  | 8   | 3.5   | 5 / 3 / 2           | ~13                       |
| Scout   | 50  | 6-9 | 6.0   | 7 / 4 / 3           | ~12                       |
| Warrior | 80  | 10-20| 4.0  | 9 / 6 / 5           | ~6                        |
| Bruiser | 120 | 15-37| 3.0  | 12 / 8 / 6          | ~4                        |
| Tank    | 200 | 20-60| 2.5  | 19 / 13 / 10        | ~3                        |

(Early / Mid / Late game with skill upgrades)

---

## 🎯 Encounter Recipes

**Early Game:**
- 2-3 Archers
- 1-2 Scouts
- 1 Scout + 1 Archer

**Mid Game:**
- 1 Warrior + 2 Scouts
- 2 Warriors + 1 Archer
- 1 Warrior + 2 Archers

**Late Game:**
- 1 Bruiser + 2 Warriors
- 1 Tank + 2 Archers
- 2 Bruisers + 1 Warrior

**Boss Fights:**
- 1 Tank + 4 Scouts
- 2 Bruisers + 2 Archers
- 1 Tank + 2 Bruisers

---

## ⚡ Quick Implementation

1. **Update Player Stats:** Set player HP to 100, AD to 5
2. **Create 5 Enemy Prefabs:** Use stats above
3. **Create 5 Weapon SOs:** Use weapon stats above
4. **Assign Weapons:** Link weapons to enemies
5. **Test:** Fight each enemy type solo
6. **Tune:** Adjust if too easy/hard
7. **Mix:** Create mixed encounters

Full details in: `/Docs/ENEMY_BALANCE_GUIDE.md`
