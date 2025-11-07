# Copilot Instructions for NinjaAdventure

Unity 2D action-RPG using modern controller/state architecture with clean separation of concerns. Follow existing patterns—don't invent new systems unless explicitly asked.

---

## 🏗️ Architecture Overview

**Folder layout:** `Assets/GAME/Scripts/` organized by feature (Player, Enemy, Character, Weapon, Inventory, Dialog, SkillTree, UI, System).

**Core systems:**
- **Controller + State Pattern**: `P_Controller`/`E_Controller` manage state switching; states (`P_State_*`, `State_*`) are modular MonoBehaviours enabled/disabled as needed. States compute intent (`SetDesiredVelocity`), controllers apply physics in `FixedUpdate`.
- **Stats & Health**: `C_Stats` (base values + modifiers) + `C_Health` (damage/heal events). Always route damage through `C_Health.ApplyDamage()` which handles armor/penetration. C_FX handles death visuals (FadeOut), controllers handle death logic (HandleDeath coroutines).
- **Weapons**: Data-driven via `W_SO` ScriptableObjects. All weapons derive from `W_Base` and use shared `ApplyHitEffects()` helper for damage/knockback/stun.
- **Stat Effects**: Unified `P_StatEffect` system (Duration: 0=permanent, 1=instant, >1=timed). Applied via `P_StatsManager.ApplyModifier()` from items (`INV_ItemSO`), skills (`ST_SkillSO`), or buffs.
- **Input**: Auto-generated `P_InputActions` from `Assets/GAME/Settings/P_InputActions.inputactions`. Never edit `.cs` file; edit `.inputactions` instead. Always create in Awake (no `??=`), Dispose in OnDestroy.
- **Game Management**: `SYS_GameManager.Instance` singleton provides centralized death orchestration (HandlePlayerDeath with tutorial/normal routing), scene transitions, dialog, audio.

---

## 🎯 Critical Conventions

**Naming prefixes (strict):**
- `P_` = Player, `E_` = Enemy, `C_` = Character (shared), `W_` = Weapon
- `D_` = Dialog, `INV_` = Inventory, `ST_` = SkillTree, `SYS_` = System singletons
- `State_*` = Enemy states, `P_State_*` = Player states, `*_SO` = ScriptableObject data

**Component wiring (see CODING_STYLE_GUIDE.md v3.1):**
- **RequireComponent types**: Use `=` (not `??=`) in Awake - shows guaranteed existence
- **Optional components**: Use `??=` with LogWarning if needed
- **FindFirstObjectByType**: Always in Start(), never in Awake, never use `??=`
- Subscribe to events in `OnEnable`, unsubscribe in `OnDisable` (use cached delegates)
- Example events: `C_Health.OnDamaged/OnHealed/OnDied`, `P_Exp.OnLevelUp/OnXPChanged`
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

**Coding style:** See `Docs/Guild/CODING_STYLE_GUIDE.md` v3.1 for complete field order, Awake/Start split, Set/Get naming, indentation rules (deep indent ONLY when attribute precedes field).

**Documentation:** Week 9 systems are production-ready (combo, states, weapons, AI, inventory). See `Docs/Week_9_Oct14-18/` for detailed guides. Week 10 focuses on tutorial death system with clean architecture.

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
- `Assets/GAME/Scripts/Character/I_Controller.cs` — Minimal interface for states

**Core Systems:**
- `Assets/GAME/Scripts/Character/C_Health.cs` — Damage/heal events & i-frames (event-only, no game logic)
- `Assets/GAME/Scripts/Character/C_FX.cs` — Visual effects only (FadeOut, ResetAlpha, Flash)
- `Assets/GAME/Scripts/Player/P_StatsManager.cs` — Stat effect application hub
- `Assets/GAME/Scripts/Weapon/W_Base.cs` + `W_SO.cs` — Weapon behavior & data (uses SetKnockback, SetStunTime)
- `Assets/GAME/Scripts/System/SYS_GameManager.cs` — Death orchestration (HandlePlayerDeath, TutorialDeathSequence, NormalDeathSequence)

**Inventory & Progression:**
- `Assets/GAME/Scripts/Inventory/INV_Manager.cs` — Item/weapon management (uses SetWeapon)
- `Assets/GAME/Scripts/Inventory/INV_Slots.cs` — Drag-drop UI (Testing branch)
- `Assets/GAME/Scripts/SkillTree/ST_Manager.cs` — Skill unlocks & upgrades

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
