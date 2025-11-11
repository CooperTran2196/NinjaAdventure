# Copilot Instructions for NinjaAdventure

Unity 2D action-RPG using modern controller/state architecture with clean separation of concerns. Follow existing patterns—don't invent new systems unless explicitly asked.

---

## 🏗️ Architecture Overview

**Folder layout:** `Assets/GAME/Scripts/` organized by feature (Player, Enemy, Character, Weapon, Inventory, Dialog, SkillTree, UI, System, Environment).

**Core systems:**
- **Controller + State Pattern**: `P_Controller`/`E_Controller`/`NPC_Controller` manage state switching; states (`P_State_*`, `State_*`) are modular MonoBehaviours enabled/disabled as needed. States compute intent (`SetDesiredVelocity`), controllers apply physics in `FixedUpdate`.
- **Stats & Health & Mana**: `C_Stats` (base values + modifiers) + `C_Health` (damage/heal events) + `C_Mana` (mana consumption events). Always route damage through `C_Health.ApplyDamage()` which handles armor/penetration. C_FX handles death visuals (FadeOut), controllers handle death logic (HandleDeath coroutines).
- **Experience & Leveling**: `P_Exp` manages XP, levels, skill points, kills, and playtime. Fires events (OnLevelUp, OnXPChanged, OnSPChanged) for UI updates. Uses linear XP curve (xpBase + xpStep × level).
- **Loot System**: `E_Reward` drops loot on death (items/weapons via LootDrop table). `INV_Loot` handles ground pickups with auto-collect trigger. Loot prefabs use ExecuteAlways for editor preview.
- **Weapons**: Data-driven via `W_SO` ScriptableObjects. All weapons derive from `W_Base` (melee: `W_Melee`, ranged: `W_Ranged`) and use shared `ApplyHitEffects()` helper for damage/knockback/stun. Supports homing projectiles (`W_ProjectileHoming`).
- **Stat Effects**: Unified `P_StatEffect` system (Duration: 0=permanent, 1=instant, >1=timed). Applied via `P_StatsManager.ApplyModifier()` from items (`INV_ItemSO`), skills (`ST_SkillSO`), or buffs.
- **Input**: Auto-generated `P_InputActions` from `Assets/GAME/Settings/P_InputActions.inputactions`. Never edit `.cs` file; edit `.inputactions` instead. Always create in Awake (no `??=`), Dispose in OnDestroy. Hotbar uses `INV_HotbarInput` for 1-9 key item switching.
- **Game Management**: `SYS_GameManager.Instance` singleton provides centralized death orchestration (HandlePlayerDeath with tutorial/normal routing), scene transitions (SYS_Fader, SYS_SceneTeleport), dialog (D_Manager), audio (SYS_SoundManager, SYS_MusicTrigger). Camera confinement via `SYS_ConfinerFinder`. Player spawn via `SYS_SpawnPoint` (supports respawn ID system).
- **Shop System**: `SHOP_Manager` handles buying/selling via `TryBuyItem()`/`SellItem()`, validates gold and inventory space. `SHOP_Keeper` populates shop from location-specific item lists. `SHOP_CategoryButtons` for item filtering.
- **Environment**: Ladders (`ENV_Ladder`), gates (`ENV_Gate` linked to boss health), traps (`ENV_Trap`), destructibles (`ENV_Destructible`), drowning zones (`ENV_Drowning`).
- **Visual Effects**: Afterimages (`C_AfterimageSpawner`) for dodge/dash trails, FX system (`C_FX`) for damage flash, fade-out, color tints. Health bars (`E_HealthBarSlider`) for enemies. Collider shape updates (`C_UpdateColliderShape`) for dynamic sprites.

---

## 🎯 Critical Conventions

**Naming prefixes (strict):**
- `P_` = Player, `E_` = Enemy, `C_` = Character (shared), `W_` = Weapon
- `D_` = Dialog, `INV_` = Inventory, `ST_` = SkillTree, `SYS_` = System singletons
- `State_*` = Enemy states, `P_State_*` = Player states, `*_SO` = ScriptableObject data
- `GRS_` = Giant Red Samurai (final boss), `GR_` = Giant Raccoon (miniboss), `GS2_` = Giant Summoner (summoner boss)
- `SHOP_` = Shop system, `NPC_` = NPC controllers, `ENV_` = Environment objects
- `B_` = Boss weapon collider helper

**Component wiring (see CODING_STYLE_GUIDE.md v3.2):**
- **RequireComponent types**: Use `=` (not `??=`) in Awake - shows guaranteed existence
- **Optional components**: Use `??=` with LogWarning if needed
- **FindFirstObjectByType**: Always in Start(), never in Awake, never use `??=`
- **P_InputActions**: Always create new in Awake (no `??=`), Dispose in OnDestroy
- **Controllers declare dependencies**: Use `[RequireComponent]` at class level for all components
- **State scripts trust controllers**: Use `=` for components guaranteed by controller's RequireComponent
- Subscribe to events in `OnEnable`, unsubscribe in `OnDisable` (use cached delegates)
- Example events: `C_Health.OnDamaged/OnHealed/OnDied`, `P_Exp.OnLevelUp/OnXPChanged`, `INV_Loot.OnItemLooted/OnWeaponLooted`
- **No null checks for guaranteed objects**: Camera.main in persistent objects, RequireComponent refs

**State management rules:**
- NEVER enable states directly—always call controller's `SwitchState()` method
- States must NOT set `rb.linearVelocity` directly—only through controller APIs
- Player input priority: Death → Dodge → Attack → Movement → Idle

**Set/Get API pattern (MANDATORY):**
- **Setters**: `SetKnockback()`, `SetDesiredVelocity()`, `SetStunTime()`, `SetWeapon()`, `SetAttacking()`
- **Getters**: `GetMeleeWeapon()`, `GetRangedWeapon()`, `GetTarget()`, `GetComboIndex()`, `GetCurrentMovePenalty()`
- Apply to ALL public methods that change/retrieve state

**ScriptableObjects:**
- Use `[CreateAssetMenu]` for all data assets (weapons, items, skills, dialog)
- Implement `OnValidate()` to auto-sync names: `if (skillName != name) skillName = name;`

---

---

## 🎮 Boss Systems

**Three Boss Types (all use I_Controller interface):**

### GRS - Giant Red Samurai (Final Boss)
- **Location:** Final boss arena
- **AI States:** `GRS_State_Chase` (Y-alignment bias) + `GRS_State_Attack` (charge+dash+double-hit combo)
- **Key Mechanics:**
  - Y-alignment gate (yAlignBand = 0.35f): Must be vertically aligned to attack
  - Normal Attack: Charge animation → dash toward player → weapon hitbox
  - Special Attack: Charge → dash → **double-hit combo** (followupGap = 0.14f)
  - Physics prediction: `TimeReach = dashSpeed * ComputedMoveWindow`
  - Face spot dash: Stops short of player (0.96f offset) for counterplay
  - Afterimage effects during dash
  - **No collision damage** (weapon-only)
- **Files:** `GRS_Controller.cs`, `GRS_State_Chase.cs`, `GRS_State_Attack.cs`

### GR - Giant Raccoon (Miniboss)
- **Location:** Map 2 miniboss arena
- **AI States:** `GR_State_Chase` (simple path) + `GR_State_Attack` (unified charge OR jump)
- **Key Mechanics:**
  - Simple direct chase (no Y-alignment)
  - Normal Attack: Charge + dash toward player
  - Special Attack: Jump animation → AoE damage + **radial knockback** (8f force)
  - Collision damage system (contactTimer cooldown)
  - Both attacks use unified coroutine
  - Afterimage effects during dash
- **Files:** `GR_Controller.cs`, `GR_State_Chase.cs`, `GR_State_Attack.cs`

### GS2 - Giant Summoner (Two-Phase Boss)
- **Location:** Summoner boss arena
- **AI States:** `GS2_State_Chase` (phase-dependent)
- **Key Mechanics:**
  - **Phase 1 (HP > 20%):** Aggressive chase + periodic summon
    - Summon cooldown: 12 seconds
    - Spawns 2-3 minions per summon (max 8 total)
    - Boss vulnerable during 1.5s telegraph + 0.5s cast
    - Minions spawn in circular spread (2.5 unit radius)
  - **Phase 2 (HP ≤ 20%):** Defensive retreat mode
    - Retreat when player < 4 units away
    - Retreat duration: 3s, then 2s vulnerable cooldown
    - Emergency spawn: 4-5 minions at phase transition
  - Collision damage (no weapon)
  - Spawned enemies launched outward with initial force (6f)
- **Files:** `GS2_Controller.cs`, `GS2_State_Chase.cs`
- **Spawned Enemy:** Regular enemy prefab (no weapon) with launch velocity

**Boss Weapon Collider Helper:**
- `B_WeaponCollider.cs` — Proxy component for boss weapon colliders
- Links weapon collider triggers to boss controller's OnWeaponHit handler
- Used by GRS and GR for charge/dash attack hitboxes

**Boss Design Patterns:**
- All bosses extend `I_Controller` interface (state machine compatible)
- Controllers handle state switching, states compute intent
- `HandleDeath()` coroutines for death sequences
- Collision damage uses `contactTimer` system (if applicable)
- Attack animations use clip-length timing for predictability
- Specialized states for unique mechanics (no shared attack states)

---

## 🔄 Key Data Flows

**Combat sequence:**
```
State determines attackDir → calls weapon.Attack(dir) → weapon enables collider/visual 
→ OnTrigger2D → W_Base.ApplyHitEffects() → C_Health.ApplyDamage() 
→ damage → lifesteal heal → knockback → stun (preserve this order)
```

**Death system (clean architecture):**
```
C_Health fires OnDied event → Controller HandleDeath() coroutine → C_FX.FadeOut() 
→ GameManager.HandlePlayerDeath(P_Controller) → checks isInTutorialZone 
→ routes to TutorialDeathSequence (fade → load Level2 → Revive at "Respawn") 
   OR NormalDeathSequence (wait 2s → show EndingUI)
```

**Tutorial zone tracking:**
```
TutorialDeathZone (trigger collider) → P_Controller OnTriggerEnter/Exit 
→ sets isInTutorialZone bool → GameManager reads it during HandlePlayerDeath
```

**Experience & leveling:**
```
E_Reward.OnDied → P_Exp.AddXP(expReward) → check level threshold 
→ fire OnLevelUp(newLevel) + OnSPChanged(skillPoints) → UI updates
```

**Loot system:**
```
E_Reward.OnDied → check dropChance → instantiate loot prefab (INV_Loot) 
→ player trigger → INV_Loot fires OnItemLooted/OnWeaponLooted 
→ INV_Manager adds to inventory + plays pickup sound
```

**Stat modification:**
```
Item/Skill SO contains List<P_StatEffect> → Manager iterates effects 
→ calls P_StatsManager.ApplyModifier() → recalculates C_Stats final values
```

**Enemy AI:**
```
E_Controller switches states: Idle → Wander → Chase (player in detectionRange) 
→ Attack (player in attackRange) → HandleDeath coroutine (OnDied event)
```

**Boss AI (GRS/GR/GS2):**
```
Boss controllers extend I_Controller → specialized states for boss mechanics
GRS: Chase (Y-alignment) → Attack (charge + dash + double-hit combo)
GR: Chase (simple path) → Attack (charge + dash OR jump AoE with radial knockback)
GS2: Chase (Phase 1: aggressive) → Special Attack (summon minions) 
     → Phase 2 (HP ≤ 20%: retreat behavior + emergency spawn)
```

**Mana system:**
```
C_Mana.ConsumeMana(amount) → returns bool (success/fail) 
→ fires OnManaChanged event → ManaUI updates display
```

**Shop flow:**
```
SHOP_Keeper populates shop slots with location items 
→ SHOP_Manager.TryBuyItem() validates gold + space 
→ INV_Manager.AddItem() + gold deduction + sound effect
```

**Dialog system:**
```
D_Manager.StartDialog(D_SO) → shows lines sequentially 
→ D_HistoryTracker records NPCs spoken to + locations visited
→ supports branching choices (3 button options max)
```

**NPC behavior:**
```
NPC_Controller switches: Idle → Wander → Talk (triggered by player interaction)
→ uses State_Talk to display dialog via D_Manager
```

**Player input → movement:**
```
P_InputActions.Player.Move → P_State_Movement calculates velocity 
→ SetDesiredVelocity() → P_Controller applies in FixedUpdate (rb.linearVelocity)
```

---

## ⚙️ Workflows & Debug

**Build:** Use VS Code `build` task (msbuild) or play directly in Unity Editor for normal iteration.

**Debug hotkeys** (defined in `P_InputActions` Debug map):
- `M` = Gain XP, `N` = Take damage, `B` = Heal, `O` = Toggle stats UI

**Balance references:** See `Docs/BALANCE_QUICK_REF.md` for player/enemy stat templates.

**Coding style:** See `Docs/Guild/CODING_STYLE_GUIDE.md` v3.2 for complete field order, Awake/Start split, Set/Get naming, indentation rules (deep indent ONLY when attribute precedes field).

**Documentation:** Week 9 systems are production-ready (combo, states, weapons, AI, inventory). See `Docs/Week_9_Oct14-18/` for detailed guides. Week 10 focuses on tutorial death system with clean architecture. Week 11 focuses on boss implementations (GRS, GR, GS2) and collision systems.

**Boss Documentation:**
- `Docs/Boss_Renaming_Summary.md` — GRS/GR renaming conventions + coding style changes
- `Docs/BOSS_ENHANCEMENTS.md` — Boss enhancement ideas (design phase)
- `Docs/MAP2_MINIBOSS_CHARGER.md` — GR (Raccoon) design plan
- `Docs/SUMMONER_BOSS.md` — GS2 (Summoner) complete design + implementation guide
- `Docs/GR_BOSS_FIXES.md` — GR bug fixes and improvements
- `Docs/Week_11_Nov4-8/BOSS_SYSTEMS_COMPLETE.md` — Complete boss technical documentation (1024 lines)
- `Docs/Week_11_Nov4-8/CUSTOM_PHYSICS_SHAPE_SYSTEM.md` — PolygonCollider2D auto-update system
- `Docs/Week_11_Nov4-8/GRS_BAKED_WEAPON_SYSTEM.md` — GRS weapon/contact damage switching
- `Docs/Week_11_Nov4-8/GS2_VOLCANO_SPAWN_SYSTEM.md` — Circle perimeter spawn mechanics
- `Docs/Week_11_Nov4-8/WEEK_11_SUMMARY.md` — Week 11 boss system improvements

**File summaries:** Ignore `*.txt` files in Scripts/—they're generated references, not active code.

---

## 🚨 Common Pitfalls

- **Missing UI values?** Check Inspector refs + `Awake()` logs for null components.
- **Animation not playing?** Verify animator params: `moveX/Y`, `idleX/Y`, `atkX/Y`, `isWandering`, `isAttacking`.
- **Weapon sprites wrong?** Ensure bottom pivot (0.5, 0) for combo system; configure `W_SO.pointsUp=true`.
- **Dodge i-frames?** `C_Health.IsDodging` requires `P_State_Dodge` enabled + `useDodgeIFrames=true`.
- **FindObjectOfType warnings?** Unity 2023+ deprecated it—use `FindFirstObjectByType`/`FindAnyObjectByType`.
- **Animator errors on revival?** Use `Rebind()` + `Update(0f)` to reset state machine.
- **Race conditions in death?** Controllers handle death logic in HandleDeath coroutines; C_FX only does visuals; GameManager orchestrates routing.
- **Boss not attacking?** Check Y-alignment for GRS (yAlignBand), attack range, and cooldown timers.
- **Shop not working?** Verify gold amount, inventory space (HasSpace check), and SHOP_Keeper population.
- **Dialog not showing?** Ensure D_Manager CanvasGroup refs, D_SO lines array, and D_ActorSO data are set.
- **Mana not consuming?** Use `C_Mana.ConsumeMana()` which returns bool—check return value before executing ability.
- **NPC not talking?** Verify State_Talk is enabled, dialog trigger zone, and D_Manager reference.
- **Ladder not working?** Check ENV_Ladder trigger collider, climb multipliers, and P_Controller EnterLadder/ExitLadder.
- **Boss spawning enemies incorrectly?** For GS2, verify enemyPrefab has no weapon, spawn count limits, and circular spawn logic.

---

## 📂 Essential Files (Read These First)

**Documentation Rules (CRITICAL):**
- **Read `Docs/README.md` FIRST** - Contains documentation structure rules
- **Read `Docs/Guild/CODING_STYLE_GUIDE.md` BEFORE refactoring** - All field order, naming, indentation rules (v3.1)
- **ALL new docs go in `Docs/` folder** - NEVER create in workspace root
- **DON'T create new week folders** unless user explicitly asks
- **Current week continues** until user says "start Week X"
- **WIP docs stay in main `Docs/`** until user approves moving to week folder

**Controllers & States:**
- `Assets/GAME/Scripts/Player/P_Controller.cs` — Player state machine + tutorial zone tracking + Revive()
- `Assets/GAME/Scripts/Player/P_State_Attack.cs` — Attack state (refactored to style guide v3.1, serves as template)
- `Assets/GAME/Scripts/Enemy/E_Controller.cs` — Enemy AI state machine + HandleDeath()
- `Assets/GAME/Scripts/Enemy/GRS_Controller.cs` — Giant Red Samurai final boss (Y-alignment, charge+dash+double-hit)
- `Assets/GAME/Scripts/Enemy/GR_Controller.cs` — Giant Raccoon miniboss (charge+dash OR jump AoE with radial knockback)
- `Assets/GAME/Scripts/Enemy/GS2_Controller.cs` — Giant Summoner boss (2-phase: aggressive chase + summon, then retreat)
- `Assets/GAME/Scripts/Character/NPC_Controller.cs` — NPC state machine (Idle/Wander/Talk)
- `Assets/GAME/Scripts/Character/I_Controller.cs` — Minimal interface for states

**Core Systems:**
- `Assets/GAME/Scripts/Character/C_Health.cs` — Damage/heal events & i-frames (event-only, no game logic)
- `Assets/GAME/Scripts/Character/C_Mana.cs` — Mana consumption with events (ConsumeMana returns bool)
- `Assets/GAME/Scripts/Character/C_FX.cs` — Visual effects only (FadeOut, ResetAlpha, Flash)
- `Assets/GAME/Scripts/Character/C_AfterimageSpawner.cs` — Afterimage trails for dodge/dash
- `Assets/GAME/Scripts/Player/P_StatsManager.cs` — Stat effect application hub
- `Assets/GAME/Scripts/Weapon/W_Base.cs` + `W_SO.cs` — Weapon behavior & data (uses SetKnockback, SetStunTime)
- `Assets/GAME/Scripts/System/SYS_GameManager.cs` — Death orchestration (HandlePlayerDeath, TutorialDeathSequence, NormalDeathSequence)

**Inventory & Progression:**
- `Assets/GAME/Scripts/Inventory/INV_Manager.cs` — Item/weapon management (uses SetWeapon)
- `Assets/GAME/Scripts/Inventory/INV_Slots.cs` — Drag-drop UI (Testing branch)
- `Assets/GAME/Scripts/Inventory/SHOP_Manager.cs` — Shop buy/sell logic with gold validation
- `Assets/GAME/Scripts/Inventory/SHOP_Keeper.cs` — Location-based shop population
- `Assets/GAME/Scripts/SkillTree/ST_Manager.cs` — Skill unlocks & upgrades
- `Assets/GAME/Scripts/SkillTree/ST_SkillSO.cs` — Skill data (includes ultimate ability support)

**Dialog System:**
- `Assets/GAME/Scripts/Dialog/D_Manager.cs` — Dialog display with choice buttons
- `Assets/GAME/Scripts/Dialog/D_HistoryTracker.cs` — NPC & location tracking (merged system)
- `Assets/GAME/Scripts/Dialog/D_SO.cs` — Dialog line data with branching
- `Assets/GAME/Scripts/Dialog/D_ActorSO.cs` — NPC character data
- `Assets/GAME/Scripts/Dialog/D_LocationSO.cs` — Location reference data

**Environment:**
- `Assets/GAME/Scripts/Environment/ENV_Ladder.cs` — Climbable surfaces with speed modifiers
- `Assets/GAME/Scripts/Environment/ENV_Gate.cs` — Boss-linked destructible gates
- `Assets/GAME/Scripts/Environment/ENV_Trap.cs` — Damage zones
- `Assets/GAME/Scripts/Environment/ENV_Destructible.cs` — Breakable objects
- `Assets/GAME/Scripts/Environment/ENV_Drowning.cs` — Water death zones

**Tutorial System:**
- `Assets/GAME/Scripts/System/TutorialDeathZone.cs` — Simple marker component for zone detection

---

## ✅ Quick Examples

**Add new weapon:**
```csharp
// 1. Create W_SO asset via "Weapon SO" menu
// 2. Derive from W_Base:
public class W_MyWeapon : W_Base {
    public override void Attack(Vector2 dir) {
        // Use helpers: GetPolarPosition, BeginVisual, etc.
        // Trigger ApplyHitEffects on collision—don't reimplement damage
    }
}
```

**Switch enemy state:**
```csharp
// CORRECT:
eController.SwitchState(E_Controller.EState.Attack);

// WRONG (never do this):
attack.enabled = true; // States are managed by controller!
```

**Apply damage:**
```csharp
int dealt = targetHealth.ApplyDamage(ad, ap, weaponAD, weaponAP, armorPen, magicPen);
// Returns actual damage dealt after armor/penetration/i-frames
```

**Heal/kill:**
```csharp
health.ChangeHealth(+50); // Heal
health.Kill();            // Instant death (triggers OnDied event)
```

**Apply knockback/stun (from weapon or ability):**
```csharp
targetController.SetKnockback(direction * force);
StartCoroutine(targetController.SetStunTime(0.5f));
```

**Equip weapon:**
```csharp
W_SO newWeapon = inv_Manager.SetWeapon(weaponSO, slot);
```

---

## 🧭 When Unsure

- Reuse existing helpers (`W_Base.ApplyHitEffects`, lifesteal logic) instead of duplicating
- If NEW (controller-based) and OLD (legacy) systems coexist, prefer NEW unless compatibility requires otherwise
- Check `Docs/Week_9_Oct14-18/` for implementation guides on recent systems
- Check `Docs/Week_10_Oct21-25/` for tutorial death system architecture
- Balance values in `Docs/BALANCE_QUICK_REF.md` are authoritative for stats
- **ALWAYS read `Docs/Guild/CODING_STYLE_GUIDE.md` before refactoring scripts** (v3.1 has indentation rules, Set/Get pattern, field order)
- When in doubt about indentation: deep indent ONLY when attribute like [Range] precedes field, otherwise normal indent
