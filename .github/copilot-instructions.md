## Copilot Instructions — NinjaAdventure (concise)

Quick overview
- Unity 2D action RPG. Code lives in `Assets/GAME/Scripts/` and is organized by feature (Player, Enemy, Character, Weapon, UI, SkillTree).

Big-picture architecture
- Component-driven: small MonoBehaviours own single responsibilities (examples: `C_Stats`, `C_Health`, `P_Movement`, `E_Combat`).
- Weapons are data-driven: `W_SO` ScriptableObjects hold weapon stats; `W_Base` + `W_Melee`/`W_Ranged` implement behaviour; `W_Projectile` handles ranged.
- Input is routed via generated `P_InputActions` (Player / UI / Debug maps). Don’t edit `P_InputActions.cs` — edit `Assets/Settings/P_InputActions.inputactions`.

Developer workflows (essentials)
- Build: run the VS Code `build` task (msbuild). Play and debug in the Unity Editor.
- Quick debug keys are defined in `P_InputActions` Debug map (e.g., `M` GainExp, `N` OnDamaged, `B` OnHealed, `O` ToggleStats).

Project conventions you must know
- Naming: prefixes indicate role — `P_` (player), `E_` (enemy), `C_` (shared character), `W_` (weapon), `UI` (UI).
- Awake auto-wiring: many scripts use `x ??= GetComponent<...>()` in `Awake()` and emit `Debug.LogError` when required refs are missing. Check `Awake()` when a component seems unassigned in Inspector.
- Events: `C_Health` exposes `OnDamaged/OnHealed/OnDied` and UIs subscribe in `OnEnable`/`OnDisable`.

Integration & data flows (examples)
- Attack/aim flow: `P_Combat` reads mouse world position (ScreenToWorldPoint) → sets animator `atkX/atkY` → `W_Base` uses `ApplyHitEffects` to apply damage/stuns/knockback.
- Movement vs attack locking: state lives in `C_State`; `CheckIsBusy()` is consulted by `P_Movement` to decide whether to apply velocity. `P_Combat` writes attack facing via `C_Anim.SetAttackDirection(animator, aimDir)` but movement `lastMove` should remain driven by WASD. `C_State` acts as a state machine (Idle, Move, Attack, Dodge) and is the single source of truth for animator states.
- Skills: Skill data is `SkillSO` (one asset per skill node). `SkillManager` listens for `SkillSlot.OnSkillUpgraded` events and calls `P_Skills` to apply the stat changes or unlock new abilities like lifesteal. `P_Skills` then recalculates the player's final stats.
- Rewards: `E_Reward` listens to enemy `C_Health.OnDied` and calls `P_Exp.AddXP(expReward)` (cached static reference).
- Enemy AI: `E_Combat` uses a simple `ThinkLoop` coroutine to check for the player in `attackRange` and trigger an `AttackRoutine`. It uses `Physics2D.OverlapCircle` for detection.

Common gotchas & quick fixes
- If UI shows default/blank values, check Inspector refs and `Awake()` logs (missing SpriteRenderer/Animator/Stats are frequent).
- The generated `P_InputActions.cs` uses `FindObjectOfType` patterns elsewhere — Unity 2023+ deprecates `FindObjectOfType`; prefer `FindFirstObjectByType`/`FindAnyObjectByType` if you update code.
- When adding new skill effects (e.g., `MS` or `KR`) update `P_Skills.Recalculate()` (we added MS/KR support here).

Files to open first
- `Assets/GAME/Scripts/Player/P_Combat.cs` — attack/aim, input usage
- `Assets/GAME/Scripts/Player/P_Movement.cs` — movement/facing and valve logic
- `Assets/GAME/Scripts/Character/C_Health.cs` — health events and damage API
- `Assets/GAME/Scripts/Weapon/W_Base.cs` and `Weapon/W_SO.cs` — weapon data + hit/effect pipeline
- `Assets/GAME/Scripts/Character/C_State.cs` — character state machine
- `Assets/GAME/Scripts/SkillTree/SkillSO.cs`, `SkillManager.cs`, and `Player/P_Skills.cs` — skill system data and logic

