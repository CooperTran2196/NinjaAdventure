# Enemy & Weapon Balance Guide v2

## 🎮 Design Philosophy

**Player Base Stats:**
- HP: 100
- AD: 5 (base attack damage)
- MS: 4.0 (movement speed - BASE)
- MaxMana: 100
- AR: 0
- MR: 0
- KR: 10
- Lifesteal: 0%

**Skill Tree System (5 Levels Each):**
- **Max HP Skill**: +20 HP per level → Max +100 HP (200 total HP at level 5)
- **Lifesteal Skill**: +5% per level → Max +25% lifesteal at level 5
- **Armor Skill**: +5 AR per level → Max +25 AR at level 5
- **Magic Resist Skill**: +5 MR per level → Max +25 MR at level 5
- **Knockback Resist Skill**: +5 KR per level → Max +35 KR (10 base + 25) at level 5
- **Move Speed Skill**: +0.5 MS per level → Max +2.5 MS (6.5 total) at level 5
- **Max Mana Skill**: +20 MP per level → Max +100 MP (200 total) at level 5
- **Weapon Bonus Skills**: Slash Arc, Move Penalty Reduction, Stun Time, Thrust Distance

**Total Player Potential (All Skills Maxed):**
- HP: 200 (100 base + 100 from skills)
- MS: 6.5 (4.0 base + 2.5 from skills)
- Mana: 200 (100 base + 100 from skills)
- AR: 25, MR: 25, KR: 35
- Lifesteal: 25%

**Combat Goals:**
- Early game: 4-5 hits to kill basic enemy
- Mid game: 3-4 hits to kill basic enemy (with upgrades)
- Late game: 2-3 hits to kill basic enemy (fully upgraded)
- Player should feel progression but not overpowered
- Enemy variety creates strategic choices (kite archer, tank bruiser, dodge assassin)

---

## 📊 Enemy Types (5 Tiers)

### **1. Archer (Squishiest, Ranged)**
**Role:** Glass cannon, stays at range, high damage but dies fast

**Stats:**
```
maxHP: 30
AD: 3
AP: 0
MS: 3.5
AR: 0
MR: 0
KR: 5 (easily knocked back)

attackCooldown: 1.5
collisionDamage: 2 (if player gets close)
collisionTick: 0.5

detectionRange: 8.0 (sees player from far)
attackRange: 6.0 (keeps distance)
```

**Weapon:** Bow (W_Ranged)
```
Weapon Name: "Archer Bow"
Type: Ranged
AD: 8
AP: 0
knockbackForce: 2.0
showTime: 0.3
attackMovePenalty: 0.8 (slowed while shooting)

projectileSpeed: 12.0
projectileLifetime: 2.0
pierceCount: 0
```

**Combat Analysis:**
- Player takes: 8 AD damage per hit (8% HP per hit)
- Player needs: 4-5 hits to kill (30 HP ÷ 7 dmg/hit)
- Threat: Kiting, ranged poke
- Counter: Chase and corner, dodge projectiles

---

### **2. Scout (Light Melee, Fast)**
**Role:** Fast skirmisher, hits and runs, low HP but mobile

**Stats:**
```
maxHP: 50
AD: 4
AP: 0
MS: 6.0 (faster than player!)
AR: 2
MR: 0
KR: 8

attackCooldown: 0.9
collisionDamage: 3
collisionTick: 0.5

detectionRange: 6.0
attackRange: 1.5
```

**Weapon:** Short Sword (W_Melee)
```
Weapon Name: "Scout Blade"
Type: Melee
AD: 6
AP: 0
knockbackForce: 3.0
onlyThrustKnocksBack: true

slashArcDegrees: 40
thrustDistance: 0.2

comboShowTimes: [0.35, 0.35, 0.35]
comboDamageMultipliers: [1.0, 1.2, 1.5]
comboMovePenalties: [0.7, 0.6, 0.4]
comboStunTimes: [0.05, 0.1, 0.2]
```

**Combat Analysis:**
- Player takes: ~6-9 damage per combo (6-9% HP)
- Player needs: 5-6 hits to kill (50 HP ÷ 9 dmg/hit)
- Threat: Speed, hit and run tactics
- Counter: Predict movement, use thrust knockback

---

### **3. Warrior (Medium, Balanced)**
**Role:** Standard melee fighter, balanced stats, bread-and-butter enemy

**Stats:**
```
maxHP: 80
AD: 5
AP: 0
MS: 4.0
AR: 5
MR: 2
KR: 12

attackCooldown: 1.2
collisionDamage: 4
collisionTick: 0.5

detectionRange: 5.0
attackRange: 1.5
```

**Weapon:** Longsword (W_Melee)
```
Weapon Name: "Warrior Sword"
Type: Melee
AD: 10
AP: 0
knockbackForce: 5.0
onlyThrustKnocksBack: true

slashArcDegrees: 45
thrustDistance: 0.25

comboShowTimes: [0.45, 0.45, 0.45]
comboDamageMultipliers: [1.0, 1.2, 2.0]
comboMovePenalties: [0.6, 0.5, 0.3]
comboStunTimes: [0.1, 0.2, 0.5]
```

**Combat Analysis:**
- Player takes: ~10-20 damage per combo (10-20% HP)
- Player needs: 6-8 hits to kill (80 HP ÷ 11 dmg/hit, armor considered)
- Threat: Well-rounded, thrust hits hard
- Counter: Standard combat, dodge thrust

---

### **4. Bruiser (Tanky, Slow)**
**Role:** High HP, high damage, slow, punishes mistakes

**Stats:**
```
maxHP: 120
AD: 7
AP: 0
MS: 3.0 (slower than player)
AR: 8
MR: 4
KR: 20 (hard to knockback)

attackCooldown: 1.5
collisionDamage: 5
collisionTick: 0.5

detectionRange: 5.0
attackRange: 1.8 (longer reach)
```

**Weapon:** Battle Axe (W_Melee)
```
Weapon Name: "Bruiser Axe"
Type: Melee
AD: 15
AP: 0
knockbackForce: 8.0
onlyThrustKnocksBack: false (all hits knockback!)

slashArcDegrees: 60 (wide swings)
thrustDistance: 0.3

comboShowTimes: [0.6, 0.6, 0.6] (slow attacks)
comboDamageMultipliers: [1.0, 1.5, 2.5]
comboMovePenalties: [0.5, 0.4, 0.2] (very slowed)
comboStunTimes: [0.15, 0.3, 0.7]
```

**Combat Analysis:**
- Player takes: ~15-37 damage per combo (15-37% HP!!!)
- Player needs: 10-12 hits to kill (120 HP ÷ 10 dmg/hit, high armor)
- Threat: Massive damage, wide attacks, knockback
- Counter: Kite, hit and run, don't get hit

---

### **5. Tank (Tankiest, Boss-like)**
**Role:** Mini-boss, extremely tanky, area denial, high threat

**Stats:**
```
maxHP: 200
AD: 8
AP: 2
MS: 2.5 (very slow)
AR: 12
MR: 8
KR: 30 (almost immune to knockback)

attackCooldown: 2.0
collisionDamage: 6
collisionTick: 0.5

detectionRange: 6.0
attackRange: 2.0 (reach advantage)
```

**Weapon:** Great Hammer (W_Melee)
```
Weapon Name: "Tank Hammer"
Type: Melee
AD: 20
AP: 0
knockbackForce: 12.0
onlyThrustKnocksBack: false

slashArcDegrees: 90 (massive arc!)
thrustDistance: 0.4

comboShowTimes: [0.8, 0.8, 1.0] (very slow)
comboDamageMultipliers: [1.0, 1.5, 3.0] (devastating thrust)
comboMovePenalties: [0.4, 0.3, 0.1] (nearly immobile)
comboStunTimes: [0.2, 0.4, 1.0]
```

**Combat Analysis:**
- Player takes: ~20-60 damage per combo (20-60% HP!!!)
- Player needs: 18-20 hits to kill (200 HP ÷ 11 dmg/hit, high armor)
- Threat: Extreme tankiness, one-shot potential, area control
- Counter: Patience, perfect dodges, sustained damage

---

## 📈 Progression Table

### **Player Damage vs Enemies (Assuming Player AD + Weapon AD)**

| Enemy Type | Early (5 AD) | Mid (10 AD) | Late (15 AD) | Hits to Kill (Early/Mid/Late) |
|------------|--------------|-------------|--------------|-------------------------------|
| Archer     | ~7 dmg       | ~12 dmg     | ~17 dmg      | 5 / 3 / 2                     |
| Scout      | ~8 dmg       | ~13 dmg     | ~18 dmg      | 7 / 4 / 3                     |
| Warrior    | ~9 dmg       | ~14 dmg     | ~19 dmg      | 9 / 6 / 5                     |
| Bruiser    | ~10 dmg      | ~15 dmg     | ~20 dmg      | 12 / 8 / 6                    |
| Tank       | ~11 dmg      | ~16 dmg     | ~21 dmg      | 19 / 13 / 10                  |

### **Enemy Damage vs Player (Per Full Combo)**

| Enemy Type | Combo Damage | Hits to Kill Player (100 HP) | With Upgrades (150 HP) |
|------------|--------------|------------------------------|------------------------|
| Archer     | 8 dmg/hit    | 13 hits                      | 19 hits                |
| Scout      | 6-9 dmg      | 11-17 hits                   | 17-25 hits             |
| Warrior    | 10-20 dmg    | 5-10 hits                    | 8-15 hits              |
| Bruiser    | 15-37 dmg    | 3-7 hits                     | 4-10 hits              |
| Tank       | 20-60 dmg    | 2-5 hits                     | 3-8 hits               |

---

## 🎯 Encounter Design Guidelines

### **Early Game (Player HP: 100, AD: 5)**
- 2-3 Archers OR 1-2 Scouts
- Teaches: Dodging, positioning, ranged vs melee

### **Mid Game (Player HP: ~125, AD: ~10)**
- 1 Warrior + 2 Scouts OR 2 Warriors + 1 Archer
- Teaches: Target prioritization, combo management

### **Late Game (Player HP: ~150, AD: ~15)**
- 1 Bruiser + 2 Warriors OR 1 Tank + 2 Archers
- Teaches: Patience, kiting, burst damage windows

### **Boss Fights**
- 1 Tank + 4 Scouts (mobility challenge)
- 2 Bruisers + 2 Archers (chaos)
- Teaches: Multi-threat management, positioning mastery

---

## 🛠️ Implementation Checklist

### **Create Enemy Prefabs:**

1. **Archer Prefab**
   - C_Stats: Use values above
   - E_Controller: detectionRange=8, attackRange=6
   - State_Attack: Assign bow weapon
   - Create W_SO_ArcherBow with stats above

2. **Scout Prefab**
   - C_Stats: Use values above
   - E_Controller: detectionRange=6, attackRange=1.5
   - State_Attack: Assign short sword
   - Create W_SO_ScoutBlade with stats above

3. **Warrior Prefab**
   - C_Stats: Use values above
   - E_Controller: detectionRange=5, attackRange=1.5
   - State_Attack: Assign longsword
   - Create W_SO_WarriorSword with stats above

4. **Bruiser Prefab**
   - C_Stats: Use values above
   - E_Controller: detectionRange=5, attackRange=1.8
   - State_Attack: Assign battle axe
   - Create W_SO_BruiserAxe with stats above

5. **Tank Prefab**
   - C_Stats: Use values above
   - E_Controller: detectionRange=6, attackRange=2.0
   - State_Attack: Assign great hammer
   - Create W_SO_TankHammer with stats above

---

## 🎮 Weapon ScriptableObject Templates

Copy these values into Unity:

### **Archer Bow (W_SO_ArcherBow.asset)**
```
[Common]
id: "archer_bow"
type: Ranged
sprite: (assign bow sprite)
offsetRadius: 0.7

[Damage]
AD: 8
AP: 0

[Knockback]
knockbackForce: 2.0

[Ranged + Magic]
showTime: 0.3
attackMovePenalty: 0.8
projectilePrefab: (assign arrow prefab)
projectileSpeed: 12.0
projectileLifetime: 2.0
pierceCount: 0
```

### **Scout Blade (W_SO_ScoutBlade.asset)**
```
[Common]
id: "scout_blade"
type: Melee
sprite: (assign short sword sprite)
offsetRadius: 0.6

[Damage]
AD: 6
AP: 0

[Knockback]
knockbackForce: 3.0
onlyThrustKnocksBack: true

[Melee Timing]
thrustDistance: 0.2

[Combo System]
slashArcDegrees: 40
comboShowTimes: [0.35, 0.35, 0.35]
comboDamageMultipliers: [1.0, 1.2, 1.5]
comboMovePenalties: [0.7, 0.6, 0.4]
comboStunTimes: [0.05, 0.1, 0.2]
```

### **Warrior Sword (W_SO_WarriorSword.asset)**
```
[Common]
id: "warrior_sword"
type: Melee
sprite: (assign longsword sprite)
offsetRadius: 0.7

[Damage]
AD: 10
AP: 0

[Knockback]
knockbackForce: 5.0
onlyThrustKnocksBack: true

[Melee Timing]
thrustDistance: 0.25

[Combo System]
slashArcDegrees: 45
comboShowTimes: [0.45, 0.45, 0.45]
comboDamageMultipliers: [1.0, 1.2, 2.0]
comboMovePenalties: [0.6, 0.5, 0.3]
comboStunTimes: [0.1, 0.2, 0.5]
```

### **Bruiser Axe (W_SO_BruiserAxe.asset)**
```
[Common]
id: "bruiser_axe"
type: Melee
sprite: (assign battle axe sprite)
offsetRadius: 0.8

[Damage]
AD: 15
AP: 0

[Knockback]
knockbackForce: 8.0
onlyThrustKnocksBack: false (all hits knock back!)

[Melee Timing]
thrustDistance: 0.3

[Combo System]
slashArcDegrees: 60
comboShowTimes: [0.6, 0.6, 0.6]
comboDamageMultipliers: [1.0, 1.5, 2.5]
comboMovePenalties: [0.5, 0.4, 0.2]
comboStunTimes: [0.15, 0.3, 0.7]
```

### **Tank Hammer (W_SO_TankHammer.asset)**
```
[Common]
id: "tank_hammer"
type: Melee
sprite: (assign great hammer sprite)
offsetRadius: 0.9

[Damage]
AD: 20
AP: 0

[Knockback]
knockbackForce: 12.0
onlyThrustKnocksBack: false

[Melee Timing]
thrustDistance: 0.4

[Combo System]
slashArcDegrees: 90
comboShowTimes: [0.8, 0.8, 1.0]
comboDamageMultipliers: [1.0, 1.5, 3.0]
comboMovePenalties: [0.4, 0.3, 0.1]
comboStunTimes: [0.2, 0.4, 1.0]
```

---

## 🎨 Design Notes

### **Movement Speed Hierarchy:**
Scout (6.0) > Player (5.0) > Warrior (4.0) > Archer (3.5) > Bruiser (3.0) > Tank (2.5)

### **Damage Hierarchy (Weapon AD):**
Tank (20) > Bruiser (15) > Warrior (10) > Archer (8) > Scout (6)

### **HP Hierarchy:**
Tank (200) > Bruiser (120) > Warrior (80) > Scout (50) > Archer (30)

### **Risk/Reward:**
- Archer: Low risk, low reward (ranged, easy to kill)
- Scout: Medium risk, low reward (fast, can escape)
- Warrior: Medium risk, medium reward (balanced)
- Bruiser: High risk, high reward (slow, hits hard)
- Tank: Very high risk, very high reward (boss-tier)

---

## 🔄 Tuning Tips

If enemies feel:
- **Too easy:** Increase enemy HP by 20%, or add +2 AD
- **Too hard:** Decrease enemy damage by 10%, or increase player base HP to 120
- **Too slow:** Increase all enemy MS by 0.5
- **Too fast:** Decrease enemy detection range by 1.0

If player feels:
- **Too weak:** Increase base AD to 7, or reduce enemy AR by 2
- **Too strong:** Decrease weapon AD by 2, or increase enemy HP by 15%
- **Too slow:** Increase base MS to 5.5
- **Too fast:** Reduce dodge distance/cooldown

---

## ✅ Summary

**Player:** 100 HP, 5 AD, 5.0 MS (base)
**Enemies:** 5 types from 30 HP (Archer) to 200 HP (Tank)
**Weapons:** 5 weapons from 6 AD (Scout Blade) to 20 AD (Tank Hammer)
**Balance:** Player takes 3-7 combo hits to die, enemies take 2-20 hits to die (scales with player upgrades)

This creates a clear progression where player growth is rewarded, but threat remains challenging! 🎮
