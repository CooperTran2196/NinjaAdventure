# Copilot Instructions for NinjaAdventure

Concise guidance for AI agents working in this Unity 2D action-RPG codebase (C#). Focus on existing patterns; do not invent new architectures unless asked.

## 1. High-Level Architecture
- Feature‐first folder layout under `Assets/GAME/Scripts/` (Character, Enemy, Weapon, Inventory, Dialog, SkillTree, UI, System, Player, Legacy).
- Modern systems (Controllers + State_* components) coexist with legacy movement/combat scripts (kept under `Legacy/` or referenced as "OLD system" fallbacks). Prefer NEW controller/state approach when extending.
- Core gameplay loop pieces:
  - Character stats + health: `C_Stats`, `C_Health` (events drive FX + death handling).
  - AI & player control use modular state components (`State_Idle`, `State_Wander`, `State_Chase`, `State_Attack`, etc.) enabled/disabled by a central controller (e.g. `E_Controller`, `P_Controller`).
  - Weapons derive from `W_Base` and implement `Attack(Vector2)`. Concrete weapon behaviors (melee, ranged, projectile) live in `Weapon/` and share common hit / effect application via static helpers in `W_Base`.
  - Dialog system: `D_Manager` orchestrates dialog ScriptableObjects (`D_SO` + related actor/location SOs) and hands out rewards via `INV_Manager`.
  - Inventory items defined as ScriptableObjects (`INV_ItemSO`) containing stat effect lists.
  - SkillTree system: `ST_Manager` manages skills (`ST_SkillSO`) unlocked via XP/leveling (`P_Exp`), applying permanent stat effects via `P_StatsManager`.
  - Central game control via `SYS_GameManager` singleton that maintains references to key subsystems (dialog, scene transitions, audio, persistent objects).

## 2. Key Patterns & Conventions
- Naming prefixes: `C_` (Character), `E_` (Enemy), `P_` (Player), `W_` (Weapon), `D_` (Dialog), `INV_` (Inventory), `SYS_` (System-wide singletons), `State_` (enable/disable state components), `P_State_*` (player-specific states), `ST_` (SkillTree), `*_SO` (ScriptableObject data).
- Controllers implement thin interfaces (e.g. `I_Controller`) for state components to send intent (e.g. `SetDesiredVelocity`). Keep states passive: they compute desired velocity / actions, controller applies them in `FixedUpdate`.
- State switching: disable all state MonoBehaviours then enable exactly one (plus possible concurrent ones like attack if design changes). Follow `E_Controller.SwitchState` and `P_Controller.SwitchState` patterns.
- Health & damage: Always route damage through `C_Health.ApplyDamage` (for stat + penetration calc) or `ChangeHealth` (for direct heals/kills). Avoid editing `currentHP` directly elsewhere.
- Weapon attack flow: State (e.g. `State_Attack`) determines `attackDir`, calls `activeWeapon.Attack(dir)` after a timing delay; weapon handles visuals + collider enabling and invokes `W_Base.ApplyHitEffects` when collisions occur.
- Effects ordering: Damage -> lifesteal heal -> knockback -> stun. Preserve this sequence.
- ScriptableObject data drives visual + numeric setup; OnValidate hooks (e.g. `INV_ItemSO`, `ST_SkillSO`) auto-sync names.
- Event usage: `C_Health` exposes `OnDamaged`, `OnHealed`, `OnDied`; `P_Exp` exposes `OnLevelUp`, `OnXPChanged`, `OnSPChanged`; `ST_Slots` exposes `OnSkillUpgraded`, `OnSkillMaxed`. Listeners cached to allow clean unsubscribe in `OnDisable`.
- Singleton pattern: `SYS_GameManager.Instance` provides centralized access to dialog, history, fader, and audio systems. Access via `SYS_GameManager.Instance.[component]`.
- Stat effects apply through `P_StatsManager.ApplyModifier()` with support for permanent, instant, and timed effects via the `P_StatEffect` class.
- Fallback strategy: New systems attempt controller-based knockback/stun first; legacy systems (`W_Knockback`, `W_Stun`, `P_Movement`, `E_Movement`) used only if modern controller absent.

## 3. Physics & Animation Integration
- Rigidbody2D linear velocity set centrally in controller (`rb.linearVelocity = desired + knockback`). States should never set `rb.linearVelocity` directly (only call controller APIs).
- Animator parameters standardization: movement (`moveX`, `moveY`), facing idle (`idleX`, `idleY`), attacking direction (`atkX`, `atkY`), activity booleans (`isWandering`, `isAttacking`). Maintain consistency when adding states/animations.

## 4. Input & Player Specifics
- Player systems mirror enemy AI but with `P_` prefixed states/components (e.g. `P_State_Dodge`, `P_State_Movement`, `P_State_Attack`, `P_State_Idle`) and generated input actions (`P_InputActions`).
- Input hierarchy: Death check → Dodge → Attack → Movement → Idle (priorities cascade downward).
- Dodge grants i-frames checked via `C_Health.IsDodging` (requires enabled `P_State_Dodge`). When adding invulnerable windows re-use this boolean gate.
- Player controller tracks two weapons (`meleeWeapon` and `rangedWeapon`) and manages their state via `P_State_Attack`.
- `P_Exp` handles player progression with linear XP curve, awarding skill points on level up (customizable via `skillPointsPerLevel`).
- `P_StatsManager` applies all stat effects, handling permanent, instant, over-time, and temporary buffs/debuffs. It acts as a bridge between the `StatEffect` system and base `C_Stats`.

## 5. Dialog & Quest-Like Flow
- Dialog progression increments `dialogIndex`; when exhausted, options are displayed (up to 3). Reward granting occurs via `EndDialog_WithRewards` before closing UI. Add new reward types by extending the auto reward loop in `D_Manager.GrantAutoRewards`.
- History tracking: `SYS_GameManager.Instance.d_HistoryTracker.RecordNPC(line.speaker)` indicates a central gameplay log—call this for new dialog-driven triggers.

## 6. Inventory & Stat Effects
- Item SO: `INV_ItemSO` holds `List<P_StatEffect>`; any runtime application pipeline should iterate that list—maintain OnValidate naming pattern. Stack size & price are authoritative here.
- `P_StatEffect` system: Unified system for all stat modifications using `StatName` enum (AttackDamage, AbilityPower, MoveSpeed, etc.).
- Effect types: Duration=0 (permanent), Duration=1 (instant), Duration>1 (timed); with optional `IsOverTime` flag for healing over time (HoT).
- Stat effects flow: Items → Skills → Buffs/Debuffs all use the same `P_StatEffect` system, applied through `P_StatsManager.ApplyModifier()`.

## 7. Extending Combat
When adding a weapon:
1. Create `W_SO` asset with sprite + numeric fields (AD, AP, knockbackForce, stunTime, offsetRadius, etc.).
2. Derive from `W_Base`; implement `Attack(dir)` using provided helpers (`GetPolarPosition`, `GetPolarAngle`, `BeginVisual`, `ThrustOverTime`).
3. Trigger `ApplyHitEffects` on collision, not manually re-implementing damage logic.

Adding a new status effect (e.g. slow):
- Prefer extending controller (e.g. temporary modifier on desired velocity) rather than embedding in weapons; follow stun pattern (time-window, coroutine, flag).

## 8. Error / Edge Case Handling
- Always null-check optional components (e.g. player-only dodge) as shown in `C_Health.Awake`.
- Use layer masks for target filtering (`W_Base.TryGetTarget`) to avoid friendly fire & self-hits.
- Health changes: ignore attempts if entity dead or dodging (consistent with i-frame semantics).

## 9. Build & Workflow
- Unity project; compile by opening in Unity Editor. A `build` shell task calls `msbuild` but typical iteration uses Unity's import pipeline. Use the task only if invoking an automated CI compile of generated `.csproj` files.
- Place new ScriptableObjects via `[CreateAssetMenu]` patterns consistent with `INV_ItemSO`.
- Keep new gameplay scripts under correct feature folder; maintain prefix conventions for quick grep-based discovery.

## 10. Practical Examples
- Switching an enemy to attack: `eController.SwitchState(E_Controller.EState.Attack);` (never enable `State_Attack` directly).
- Dealing raw damage: `targetHealth.ApplyDamage(ad, ap, weaponAD, weaponAP, armorPen, magicPen);` (returns final dealt).
- Healing: `someHealth.ChangeHealth(+amount);` Kill: `someHealth.Kill();`

## 11. When Unsure
- Prefer reusing existing helpers (`ApplyHitEffects`, lifesteal logic) over duplicating calculations.
- If both NEW and OLD (legacy) options exist, choose NEW unless integration requires legacy compatibility.

(End)  — Please review; specify any missing subsystems (e.g., player-specific scripts, SkillTree) that need inclusion or further detail.## Copilot Instructions — NinjaAdventure (concise)

Quick overview
- Unity 2D action RPG. Code lives in `Assets/GAME/Scripts/` and is organized by feature (Player, Enemy, Character, Weapon, UI, SkillTree).
- Ignore any `.txt` files found in the scripts directory; they are generated summaries for reference only, not active code.

Big-picture architecture
- Component-driven: small MonoBehaviours own single responsibilities (examples: `C_Stats`, `C_Health`, `P_Movement`, `E_Combat`).
- Centralized Stat System: All stat modifications (from skills, items, etc.) are handled by `P_StatsManager`. Effects are defined as a `List<StatEffect>` in `ScriptableObject`s (`INV_ItemSO`, `ST_SkillSO`).
- Weapons are data-driven: `W_SO` ScriptableObjects hold weapon stats; `W_Base` + `W_Melee`/`W_Ranged` implement behaviour; `W_Projectile` handles ranged.
- Input is routed via generated `P_InputActions` (Player / UI / Debug maps). Don’t edit `P_InputActions.cs` — edit `Assets/Settings/P_InputActions.inputactions`.

Developer workflows (essentials)
- Build: run the VS Code `build` task (msbuild). Play and debug in the Unity Editor.
- Quick debug keys are defined in `P_InputActions` Debug map (e.g., `M` GainExp, `N` OnDamaged, `B` OnHealed, `O` ToggleStats).

Project conventions you must know
- Naming: prefixes indicate role — `P_` (player), `E_` (enemy), `C_` (shared character), `W_` (weapon), `UI` (UI), `ST_` (SkillTree), `INV_` (Inventory).
- Awake auto-wiring: many scripts use `x ??= GetComponent<...>()` in `Awake()` and emit `Debug.LogError` when required refs are missing. Check `Awake()` when a component seems unassigned in Inspector.
- Events: `C_Health` exposes `OnDamaged/OnHealed/OnDied` and UIs subscribe in `OnEnable`/`OnDisable`.

Integration & data flows (examples)
- Stat Modification: `ST_Manager` (on skill upgrade) or `INV_Manager` (on item use) iterates through the `StatEffect` list on the `ST_SkillSO` or `INV_ItemSO` and calls `P_StatsManager.ApplyModifier()` for each. `P_StatsManager` then handles the logic for permanent, instant, or temporary effects and recalculates the final stats in `C_Stats`.
- Attack/aim flow: `P_Combat` reads mouse world position (ScreenToWorldPoint) → sets animator `atkX/atkY` → `W_Base` uses `ApplyHitEffects` to apply damage/stuns/knockback.
- Movement vs attack locking: state lives in `C_State`; `CheckIsBusy()` is consulted by `P_Movement` to decide whether to apply velocity. `P_Combat` writes attack facing via `C_State.SetAttackDirection(Vector2 dir)` but movement `lastMove` should remain driven by WASD. `C_State` acts as a state machine (Idle, Move, Attack, Dodge) and is the single source of truth for animator states.
- Rewards: `E_Reward` listens to enemy `C_Health.OnDied` and calls `P_Exp.AddXP(expReward)` (cached static reference).
- Enemy AI: `E_Combat` uses a simple `ThinkLoop` coroutine to check for the player in `attackRange` and trigger an `AttackRoutine`. It uses `Physics2D.OverlapCircle` for detection.

Common gotchas & quick fixes
- If UI shows default/blank values, check Inspector refs and `Awake()` logs (missing SpriteRenderer/Animator/Stats are frequent).
- The `StatEffect` `Duration` field has specific rules: `0` = Permanent (baked into base stats), `1` = Instant (applied once, now treated as permanent for stat boosts), `>1` = Timed effect (in seconds).
- The generated `P_InputActions.cs` uses `FindObjectOfType` patterns elsewhere — Unity 2023+ deprecates `FindObjectOfType`; prefer `FindFirstObjectByType`/`FindAnyObjectByType` if you update code.

Files to open first
- `Assets/GAME/Scripts/Player/P_StatsManager.cs` — The core of the stat system.
- `Assets/GAME/Scripts/Character/C_StatEffect.cs` — Defines the `StatEffect` class and related enums.
- `Assets/GAME/Scripts/Player/P_Combat.cs` — attack/aim, input usage
- `Assets/GAME/Scripts/Character/C_Health.cs` — health events and damage API
- `Assets/GAME/Scripts/Weapon/W_Base.cs` and `Weapon/W_SO.cs` — weapon data + hit/effect pipeline
- `Assets/GAME/Scripts/SkillTree/ST_Manager.cs` & `Inventory/INV_Manager.cs` — Triggers for stat changes.
- `Assets/GAME/Scripts/Character/C_State.cs` — The character state machine.
- `Assets/GAME/Scripts/Enemy/E_Movement.cs` — Example of wander/chase logic.

