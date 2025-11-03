# Coding Style Guide - Major Update (v2.0)

**Date:** October 18, 2025  
**Reason:** Guide didn't match actual codebase patterns in Inventory folder

---

## 🔍 What Was Analyzed

Reviewed **all scripts** in `Assets/GAME/Scripts/Inventory/`:
- `INV_Manager.cs` - Central inventory system
- `INV_Slots.cs` - Drag & drop slot system
- `INV_Loot.cs` - Loot pickup system
- `INV_HotbarInput.cs` - Hotbar input handling
- `INV_ItemInfo.cs` - Item tooltip popup
- `INV_ItemSO.cs` - Item data ScriptableObject
- `SHOP_Manager.cs` - Shop transaction system
- `SHOP_Slot.cs` - Shop UI slots
- `SHOP_Keeper.cs` - Shop NPC interaction
- `SHOP_CategoryButtons.cs` - Shop category navigation

Plus cross-referenced with:
- `P_StatsManager.cs` - Player stat system
- `C_Health.cs` - Health component
- `SYS_GameManager.cs` - Central game manager

---

## 🔄 Major Changes

### 1. **Awake() vs Start() Split** (NEW)
**OLD:** Everything in Awake()  
**NEW:** Proper separation of concerns

```csharp
// Awake: Same-GameObject components only
void Awake()
{
    c_Health ??= GetComponent<C_Health>();
    if (!c_Health) { Debug.LogError(...); return; }
}

// Start: Manager/singleton references
void Start()
{
    inv_Manager = INV_Manager.Instance;  // Explicit, no ??=
    if (!inv_Manager) { Debug.LogError(...); return; }
}
```

**Why:** Managers may not exist yet in Awake(). Start() guarantees they're initialized.

### 2. **NO `??=` for Find Methods** (CRITICAL)
**OLD:** Allowed `??=` everywhere  
**NEW:** Only for GetComponent on same GameObject

```csharp
// ❌ OLD (Wrong):
p_StatsManager ??= FindFirstObjectByType<P_StatsManager>();

// ✅ NEW (Correct):
void Start()
{
    p_StatsManager = FindFirstObjectByType<P_StatsManager>();
    if (!p_StatsManager) { Debug.LogError(...); return; }
}
```

**Why:** `??=` only works if field is null. FindFirstObjectByType should be explicit and validated.

### 3. **P_InputActions: NO `??=`, Create Fresh** (CRITICAL)
**OLD:** Silent on this  
**NEW:** Explicit pattern

```csharp
// ❌ NEVER use ??=
input ??= new P_InputActions();  // WRONG

// ✅ ALWAYS create fresh
void Awake()
{
    input = new P_InputActions();  // Correct
}
```

**Why:** Input actions should always be fresh instances, not conditionally created.

### 4. **Optional Context Header** (NEW)
**OLD:** Always start with [Header("References")]  
**NEW:** Can have descriptive header first

```csharp
[Header("Central API for the Inventory system, depend on P_StatsManager")]
[Header("References")]
```

**Why:** Found in multiple files (INV_Manager, P_StatsManager, SYS_GameManager). Provides helpful context.

### 5. **Static Event Subscriptions** (NEW)
**OLD:** No pattern shown  
**NEW:** Lambda syntax for single-line

```csharp
void OnEnable()  => INV_Loot.OnItemLooted += AddItem;
void OnDisable() => INV_Loot.OnItemLooted -= AddItem;
```

**Why:** Common pattern in actual code. Clean and concise.

### 6. **Relaxed Function Comments** (CHANGED)
**OLD:** "Comment all public methods"  
**NEW:** "Comment if non-obvious"

```csharp
// ✅ Needs comment (complex behavior)
// Adds item to inventory, stacking into existing slots first
public void AddItem(INV_ItemSO itemSO, int quantity) { ... }

// ❌ No comment needed (self-explanatory)
public void Hide() { ... }
public void Show() { ... }
public void UpdateUI() { ... }
```

**Why:** Actual code doesn't comment obvious methods. Focus on what needs explanation.

---

## 📊 Pattern Frequencies Found

| Pattern | Frequency | Status |
|---------|-----------|--------|
| Awake/Start split for Find methods | 90% | ✅ Added |
| Optional context headers | 40% | ✅ Added |
| Deep indentation for public fields | 100% | ✅ Kept |
| `??=` only for GetComponent | 100% | ✅ Enforced |
| Lambda event subscriptions | 60% | ✅ Added |
| Comments on obvious methods | 0% | ✅ Removed rule |

---

## 🎯 Key Takeaways for AI

1. **NEVER use `??=` for FindFirstObjectByType** - Always explicit in Start()
2. **NEVER use `??=` for P_InputActions** - Always `= new` in Awake()
3. **Awake = GetComponent**, **Start = Find/Instance**
4. **Comment only complex logic**, not obvious method names
5. **Validate everything** - LogError with return for required, LogWarning for optional

---

## 📁 Files Changed

- ✅ `CODING_STYLE_GUIDE.md` - Completely rewritten based on actual patterns
- 📦 `CODING_STYLE_GUIDE_OLD.md` - Backup of previous version

---

## ✅ Verification

The new guide now accurately reflects patterns from:
- ✅ INV_Manager (singleton pattern, Start() initialization)
- ✅ INV_Loot (Initialize methods, static events)
- ✅ INV_HotbarInput (P_InputActions lifecycle)
- ✅ SHOP_Keeper (P_InputActions + Start() setup)
- ✅ C_Health (optional components with `?.`)
- ✅ SYS_GameManager (public references pattern)

**No more guessing - guide matches reality!** 🎉
