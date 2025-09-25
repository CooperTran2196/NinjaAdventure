## Copilot Instructions — NinjaAdventure (concise)

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
- Movement vs attack locking: state lives in `C_State`; `CheckIsBusy()` is consulted by `P_Movement` to decide whether to apply velocity. `P_Combat` writes attack facing via `C_State.SetAttackDirection(animator, aimDir)` but movement `lastMove` should remain driven by WASD. `C_State` acts as a state machine (Idle, Move, Attack, Dodge) and is the single source of truth for animator states.
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

