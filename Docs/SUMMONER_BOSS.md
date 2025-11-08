# Summoner Boss Design Documentation

**Boss Prefix:** `SUM_`  
**Boss Type:** Two-Phase Summoner  
**Created:** November 7, 2025

---

## 🎯 BOSS IDENTITY

**Core Concept:**
- Phase 1: Aggressive chaser that periodically summons minions
- Phase 2: Defensive retreater triggered at low HP (20%)
- No weapon attacks - relies on collision damage + minion pressure

**Playstyle:**
- Player must manage minion waves while damaging boss
- Phase 2 creates retreat windows for strategic burst damage
- Summon animations provide vulnerable windows

---

## 📋 BEHAVIOR PHASES

### **PHASE 1: Aggressive (HP > 20%)**

**Chase Behavior:**
- Normal chase toward player (uses `SUM_State_Chase`)
- No retreat - always pursuing
- Uses collision damage when touching player

**Summon Mechanic:**
- **Cooldown:** 12 seconds
- **Spawn Count:** 2-3 minions per summon (random)
- **Max Minions:** 8 (hard cap to prevent spam)
- **During Summon:**
  - Boss stops moving for 1.5s telegraph
  - Boss is vulnerable (perfect time to attack)
  - Spawns minions in circular spread (2.5 unit radius)
  - Returns to chase after 0.5s cast

**Summon Animation Timing:**
- Telegraph: 1.5s (vulnerable)
- Cast: 0.5s (spawn happens here)
- Total: 2.0s immobile

---

### **PHASE 2: Defensive (HP ≤ 20%)**

**Phase Transition (Triggered Once):**
- **Emergency Summon:** 4-5 minions spawn immediately
- Switches to retreat behavior permanently
- Cannot return to Phase 1

**Retreat Cycle:**
1. **Retreat (3 seconds):**
   - Moves AWAY from player when player < 4 units
   - Uses full movement speed
   - Can still summon during retreat
   
2. **Stop Window (2 seconds):**
   - Boss stops moving (vulnerable)
   - Perfect time for player to deal damage
   - Can still summon if cooldown ready

3. **Repeat:** Retreat 3s → Stop 2s → Retreat 3s → Stop 2s...

**Retreat Trigger:**
- Only retreats when player < 4 units
- If player > 4 units during stop window, boss doesn't retreat next cycle

**Summon in Phase 2:**
- Same cooldown (12s)
- Same spawn count (2-3 minions)
- Can happen during retreat OR stop window

---

## 👾 MINION DESIGN

**Spawned Enemies:**
- Boss spawns **regular enemy prefabs** (collision-damage only, no weapons)
- Enemies are treated as normal enemies in the game world
- They are **NOT linked to the boss** (independent lifecycle)
- Enemies persist after boss dies (normal gameplay)
- Enemies drop normal loot/exp (not minion-specific)

**Enemy Characteristics:**
- Use existing enemy controller (E_Controller) without weapon
- Simple AI - chase player with collision damage
- Stats configured per enemy prefab (not boss-specific)
- Spawned in circular pattern around boss

**No Special Minion Controller Needed:**
- Boss just instantiates enemy prefabs
- Enemies behave like normal enemies
- No parent-child relationship with boss
- No cleanup on boss death

---

## 🎮 GAMEPLAY FLOW

### **Example Combat Scenario:**

**Phase 1 (100%-20% HP):**
1. Boss detects player → Chase state (aggressive pursuit)
2. Boss contacts player → 15 collision damage
3. After 12s → Boss stops, summon animation (1.5s telegraph)
4. Player attacks during summon (vulnerable window)
5. 2-3 minions spawn in spread pattern
6. Minions chase player with collision damage
7. Boss returns to chase
8. Player must kite minions OR kill them quickly
9. Repeat until boss reaches 20% HP

**Phase 2 (≤20% HP):**
1. Boss hits 20% HP → Emergency summon (4-5 minions)
2. Boss enters retreat mode permanently
3. Player approaches (< 4 units) → Boss retreats for 3s
4. Boss stops for 2s (vulnerable window)
5. Player deals burst damage during stop
6. Boss retreats again for 3s
7. During retreat, boss can still summon if cooldown ready
8. Player must manage minions while exploiting stop windows
9. Repeat until boss defeated

---

## ⚙️ TECHNICAL IMPLEMENTATION

### **Files Created:**

1. **SUM_Controller.cs** - Main boss controller
   - State machine (Idle, Wander, Chase, Summon)
   - Phase transition logic
   - Retreat cycle timing
   - Enemy spawn system (simple instantiation)
   - Collision damage

2. **SUM_State_Chase.cs** - Custom chase with retreat
   - Normal chase in Phase 1
   - Retreat behavior in Phase 2
   - Stop window handling

3. **SUM_State_Summon.cs** - Summon animation state
   - Telegraph phase (1.5s)
   - Cast phase (0.5s)
   - Calls `controller.CompleteSummon()`

**No Minion Controller Needed:**
- Boss simply spawns enemy prefabs
- Enemies use existing `E_Controller` (without weapon)
- No special minion tracking or cleanup

### **Key Methods:**

**SUM_Controller:**
```csharp
void CheckPhaseTransition()      // Check HP and trigger Phase 2
void TriggerPhase2()             // Emergency summon + enable retreat
void UpdateRetreatBehavior()     // Handle retreat timing cycle
void StartRetreat()              // Begin 3s retreat
bool IsRetreating()              // Check if currently retreating
bool IsInRetreatCooldown()       // Check if in 2s stop window
bool CanSummonNow()              // Check cooldown only (no cap)
void StartSummon()               // Begin summon animation
void CompleteSummon()            // Spawn enemies (called by state)
void SpawnEnemies(int count)     // Instantiate enemy prefabs in spread
```

**SUM_State_Chase:**
```csharp
void Update()
{
    if (controller.IsRetreating())
        // Move away from player
    else if (controller.IsInRetreatCooldown())
        // Stop moving (vulnerable)
    else
        // Normal chase toward player
}
```

---

## 🔢 CONFIGURATION VALUES

### **Boss Stats (Recommended):**

| Stat | Value | Notes |
|------|-------|-------|
| HP | 600 | Higher than GR/GRS (no weapon risk) |
| Speed | 2.5 | Slower than normal enemy |
| Collision Damage | 15 | Moderate damage |
| Collision Tick | 1.0s | Standard cooldown |
| Detection Range | 12f | Wider detection |

### **Summon Settings:**

```csharp
normalSummonCount = 3;       // 2-3 enemies (random -1 to +0)
emergencySummonCount = 5;    // 4-5 enemies (random -1 to +0)
summonCooldown = 12f;        // 12 seconds between summons
spawnSpreadRadius = 2.5f;    // Circular spawn radius
// No max cap - boss can spawn infinitely
```

### **Phase 2 Settings:**

```csharp
phase2Threshold = 0.20f;     // 20% HP
retreatDistance = 4f;        // Start retreating at < 4 units
retreatDuration = 3f;        // Retreat for 3 seconds
retreatCooldown = 2f;        // Stop for 2 seconds (vulnerable)
```

### **Animation Timing:**

```csharp
telegraphDuration = 1.5f;    // Summon telegraph
castDuration = 0.5f;         // Summon cast
```

---

## 🎨 ANIMATION REQUIREMENTS

**Boss Animations:**
1. **Idle** (existing)
2. **Walk** (existing)
3. **Summon Telegraph** (1.5s loop) - Raise arms, glow effect
4. **Summon Cast** (0.5s once) - Release energy burst
5. **Hurt** (existing)
6. **Death** (existing)

**Animator Parameters:**
```csharp
// Movement (existing)
moveX, moveY, idleX, idleY, isMoving

// Summon (new)
isSummoning (bool) - triggers summon animation
```

**Minion Animations:**
- Reuse existing enemy animations (4-direction walk)
- Death animation (quick fade)

**Visual Effects:**
- Purple/green glow during summon telegraph
- Particle burst at minion spawn positions
- Ground circles at spawn locations

---

## 🎯 BALANCING TIPS

### **Making Boss Easier:**
- Increase summon cooldown to 15s (less minion pressure)
- Reduce max minions to 6
- Increase retreat cooldown to 3s (longer vulnerable window)
- Reduce Phase 2 threshold to 15% HP (shorter defensive phase)

### **Making Boss Harder:**
- Decrease summon cooldown to 8s (constant minion waves)
- Increase max minions to 10
- Reduce retreat cooldown to 1s (shorter vulnerable window)
- Increase Phase 2 threshold to 30% HP (longer defensive phase)
- Add more minions per summon (4-5 normal, 6-7 emergency)

### **Reward Scaling:**
- Boss should drop high-value loot (no farming risk)
- Minions drop minimal/no loot (prevent farming)
- Boss EXP: 500+ (major encounter)
- Minion EXP: 2-5 (minimal)

---

## 🐛 DEBUGGING

### **Gizmos (Scene View):**
- **Orange sphere:** Detection range (12 units)
- **Cyan sphere:** Retreat distance (4 units, Phase 2 only)
- **Yellow sphere:** Minion spawn radius (2.5 units)

### **Console Logs:**
- Phase 2 trigger: "Entered Phase 2! Emergency summon triggered."
- Minion spawn: "Spawned X minions. Total alive: Y"

### **Common Issues:**

**Minions not spawning?**
- Check `minionPrefab` is assigned in Inspector
- Check `maxMinions` cap not reached
- Check `summonCooldown` elapsed

**Boss not retreating?**
- Check HP is ≤ 20%
- Check player is < 4 units away
- Check retreat cooldown elapsed

**Minions not chasing?**
- Check player has "Player" tag
- Check `detectionRange` is sufficient
- Check `MINION_Controller` component attached

---

## 📊 COMPARISON TO OTHER BOSSES

| Feature | **GR** (Raccoon) | **GRS** (Samurai) | **SUM** (Summoner) |
|---------|------------------|-------------------|-------------------|
| **Weapon** | Yes (dash + jump) | Yes (double-hit) | No (minions only) |
| **Collision Damage** | Yes | Yes | Yes |
| **Special Ability** | Jump AoE | Dash combo | Summon minions |
| **Mobility** | High (charge) | Medium (dash) | Low (retreat only) |
| **Difficulty** | Direct combat | Precision required | Wave management |
| **Phase System** | None | None | 2-phase (aggressive → defensive) |
| **HP** | 500-600 | 500-600 | 600 (higher, no weapon) |

**Summoner Unique Traits:**
- Only boss with minion spawning
- Only boss with phase transition
- Only boss with retreat behavior
- Only boss without weapon attacks

---

## ✅ TESTING CHECKLIST

**Phase 1:**
- [ ] Boss chases player aggressively
- [ ] Collision damage triggers on contact
- [ ] Summon triggers every 12 seconds
- [ ] 2-3 minions spawn in spread pattern
- [ ] Boss vulnerable during summon animation
- [ ] Minions chase player after spawn
- [ ] Minion cap (8) prevents spam

**Phase 2:**
- [ ] Triggers at exactly 20% HP
- [ ] Emergency summon spawns 4-5 minions
- [ ] Retreat starts when player < 4 units
- [ ] Retreat lasts 3 seconds
- [ ] Stop window lasts 2 seconds
- [ ] Boss vulnerable during stop window
- [ ] Cycle repeats correctly

**Minions:**
- [ ] Chase player continuously
- [ ] Deal collision damage
- [ ] Die from player attacks
- [ ] Die when boss dies
- [ ] Despawn after 30s if boss dead
- [ ] No loot drops

**Edge Cases:**
- [ ] Boss death kills all minions
- [ ] Minion spawn fails gracefully if prefab missing
- [ ] Retreat doesn't trigger if player > 4 units
- [ ] Summon doesn't trigger if at max minions

---

## 🎓 USAGE NOTES

**For Level Designers:**
- Place summoner in open arena (needs space for retreat)
- Avoid tight corridors (retreat will fail)
- Consider minion pathfinding (needs clear paths)

**For Balance Designers:**
- Adjust `summonCooldown` to control pressure
- Adjust `maxMinions` to control difficulty
- Adjust `phase2Threshold` to control phase timing
- Adjust `retreatDuration` / `retreatCooldown` for skill ceiling

**For Artists:**
- Boss needs summon animation (hands raised)
- Particle effects for spawn locations
- Minions can reuse existing enemy sprites
- Consider unique color for summoner (purple/green)

---

## 🔮 FUTURE ENHANCEMENTS

**Potential Additions:**
1. **Minion Types:** Different minion variants (fast/tanky/ranged)
2. **Enrage Mode:** Boss summons faster at very low HP
3. **Sacrifice Mechanic:** Boss can consume minions to heal
4. **Boss Buff:** Minions get stronger when near boss
5. **Teleport:** Boss teleports instead of retreating (more interesting)

**Not Recommended:**
- Adding weapon attacks (conflicts with summoner identity)
- Removing collision damage (boss becomes too passive)
- More than 2 phases (too complex)
