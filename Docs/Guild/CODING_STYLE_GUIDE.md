# NinjaAdventure - Coding Style Guide (v2.0)

**CRITICAL:** This file establishes project-wide coding conventions based on actual codebase patterns. **NEVER DELETE THIS FILE.**

---

## 🤖 Instructions for AI Assistants

1. **Read this ENTIRE document first** - All rules are mandatory
2. **Wait for the user to specify which file to refactor** - Don't act until asked
3. **Apply ALL rules** when refactoring
4. **After refactoring, explain changes briefly** - Don't repeat the guide

**Project architecture:** See `.github/copilot-instructions.md` for system overview.

---

## 📋 Quick Reference

### Field Order Template
```csharp
[Header("Optional descriptive context")]  // Optional: script purpose/dependencies
[Header("References")]                     // 1. Components (private, aligned)
C_Health c_Health;
C_Stats  c_Stats;

[Header("MUST wire MANUALLY in Inspector")]  // 2. UI/Prefabs (public, aligned)
public TMP_Text goldText;

[Header("Settings Category")]              // 3. Public Settings (deep indent)
                public float moveSpeed = 5f;
[Range(0, 100)] public float accuracy  = 75f;

// Runtime state                           // 4. Private variables (last)
bool isDead;
```

### Awake/Start Split
```csharp
void Awake()
{
    // 1. GetComponent same-GameObject refs (aligned ??=)
    c_Health ??= GetComponent<C_Health>();
    sr       ??= GetComponent<SpriteRenderer>();
    
    // 2. Validate required (aligned, with this)
    if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
    
    // 3. Create P_InputActions (NO ??=, always new)
    input = new P_InputActions();
}

void Start()
{
    // Find managers/singletons (explicit, no ??=)
    inv_Manager = INV_Manager.Instance;
    if (!inv_Manager) { Debug.LogError($"{name}: INV_Manager.Instance is null!", this); return; }
}
```

---

## 📐 1. Field Declaration Order

### 1.1 Optional Context Header (First)
```csharp
[Header("Central API for the Inventory system, depend on P_StatsManager")]
[Header("References")]
```

**When to use:**
- ✅ Complex scripts with non-obvious purpose
- ✅ Scripts with important dependencies to highlight
- ✅ Examples: `"Central game manager - handles singletons, scene, audio"`, `"Depend on C_Stats and C_Health"`
- ❌ Skip for simple, self-explanatory scripts

### 1.2 Component References
```csharp
[Header("References")]
C_Health         c_Health;      // Custom components
C_Stats          c_Stats;
P_StatsManager   p_StatsManager;
SpriteRenderer   sr;            // Unity components
Rigidbody2D      rb;
Animator         anim;
CircleCollider2D trigger;
P_InputActions   input;         // Input actions (NO public, NO [SerializeField])
```

**Rules:**
- ✅ Custom: `c_Health`, `p_Controller`, `inv_Manager`, `shop_Manager`
- ✅ Unity: `sr`, `rb`, `anim`, `trigger`, `boxCol`, `ps`
- ✅ ALL private (no `public`, no `[SerializeField]`)
- ✅ Align variable names in column
- ✅ `P_InputActions` goes here (private, no serialization)

**EXCEPTION - Manager Singletons (SYS_GameManager ONLY):**
```csharp
[Header("Central game manager - handles singletons, scene, audio")]
[Header("References")]
public SYS_Fader        sys_Fader;        // Public - accessed via Instance
public SYS_SoundManager sys_SoundManager;
public D_Manager        d_Manager;
```
- ✅ SYS_GameManager refs are public (accessed via `SYS_GameManager.Instance.xxx`)
- ✅ Deep indentation alignment

### 1.3 "MUST wire MANUALLY" Section
```csharp
[Header("MUST wire MANUALLY in Inspector")]
public TMP_Text      goldText;
public Image         itemImage;
public GameObject    lootPrefab;
public Transform     statContainer;
public INV_Slots[]   inv_Slots;
```

**Rules:**
- ✅ UI components: `TMP_Text`, `Image`, `CanvasGroup`, `RectTransform`
- ✅ Prefabs: `GameObject` references
- ✅ Inspector arrays/lists
- ✅ Always public
- ✅ Deep indentation alignment
- ✅ Header text: **exactly** `"MUST wire MANUALLY in Inspector"`

### 1.4 Public Settings (Configuration)
```csharp
[Header("Movement Settings")]
                    public float moveSpeed = 5f;
                    public float jumpForce = 10f;
[Range(0, 100)]     public float dropChance = 50f;

[Header("Data")]    // Exposed state variables
                    public int         gold;
                    public INV_ItemSO  itemSO;
```

**Rules:**
- ✅ Deep indentation: types and names align in columns
- ✅ Attributes same line: `[Range(0, 100)] public float`
- ✅ Use `[Header("Data")]` for public state (not settings)
- ✅ Descriptive headers: `"Movement Settings"`, `"Loot"`, `"Visibility"`

### 1.5 Runtime Variables (Last)
```csharp
// Runtime state
bool isDead;
bool isGrounded;
float attackTimer;
```

**Rules:**
- ✅ Last field section
- ✅ Private (no keyword)
- ✅ Comment: `// Runtime state` or `// Internal state`
- ❌ No header attribute

---

## 🔧 2. Awake() vs Start() - The Critical Split

### Awake() - Same GameObject Only
```csharp
void Awake()
{
    // 1. GetComponent for same-GameObject components (aligned ??=)
    c_Health ??= GetComponent<C_Health>();
    c_Stats  ??= GetComponent<C_Stats>();
    sr       ??= GetComponent<SpriteRenderer>();
    rb       ??= GetComponent<Rigidbody2D>();
    
    // 2. Validate REQUIRED components (aligned one-liner with this)
    if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
    if (!sr)       { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
    
    // 3. Validate "MUST wire MANUALLY" Inspector refs (NO ??=!)
    if (!goldText)      { Debug.LogError($"{name}: goldText is missing!", this); return; }
    if (!lootPrefab)    { Debug.LogError($"{name}: lootPrefab is missing!", this); return; }
    if (!statContainer) { Debug.LogError($"{name}: statContainer is missing!", this); return; }
    
    // 4. Optional components - LogWarning (no return, no braces)
    if (!c_FX) Debug.LogWarning($"{name}: C_FX is missing!", this);
    
    // 5. Create P_InputActions (NO ??=, always create fresh)
    input = new P_InputActions();
    
    // 6. Additional setup
    originalColor = sr.color;
}
```

### Start() - Manager/Singleton References
```csharp
void Start()
{
    // Find manager singletons (explicit assignment, no ??=)
    inv_Manager = INV_Manager.Instance;
    
    if (!inv_Manager) { Debug.LogError($"{name}: INV_Manager.Instance is null!", this); return; }
    
    // Find scene-wide objects (explicit, no ??=)
    if (SYS_GameManager.Instance != null)
    {
        shop_Manager = SYS_GameManager.Instance.shop_Manager;
        if (shop_Manager != null)
        {
            shopCanvasGroup = shop_Manager.GetComponent<CanvasGroup>();
        }
    }
    
    if (!shop_Manager)    { Debug.LogError($"{name}: SHOP_Manager not found!", this); return; }
    if (!shopCanvasGroup) { Debug.LogError($"{name}: CanvasGroup not found!", this); return; }
    
    // Additional initialization
    foreach (INV_Slots slot in inv_Slots) slot.UpdateUI();
}
```

**Critical Rules:**
- ✅ **Awake()**: GetComponent (same GameObject), validate Inspector refs, create input
- ✅ **Start()**: FindFirstObjectByType, singleton access, scene-wide setup
- ✅ All `??=` aligned
- ✅ All validations aligned
- ✅ Format: `Debug.LogError($"{name}: Component is missing!", this)`
- ✅ ALWAYS `return;` after LogError
- ❌ **NEVER `??=` for FindFirstObjectByType** - always explicit assignment
- ❌ **NEVER `??=` for P_InputActions** - always `= new P_InputActions()`
- ❌ **NEVER `??=` for "MUST wire MANUALLY" fields** - only validate

**Error vs Warning:**
```csharp
// Required - LogError with return and braces
if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }

// Optional - LogWarning, no return, no braces
if (!audioSource) Debug.LogWarning($"{name}: AudioSource is missing!", this);
```

---

## 🔄 3. Event Subscription Patterns

### Standard Pattern (Validated Components)
```csharp
void OnEnable()
{
    // No null checks - already validated in Awake
    c_Health.OnDied    += HandleDeath;
    c_Health.OnDamaged += HandleDamaged;
}

void OnDisable()
{
    c_Health.OnDied    -= HandleDeath;
    c_Health.OnDamaged -= HandleDamaged;
}
```

**Rules:**
- ✅ No null checks (validated in Awake/Start)
- ✅ Align `+=` and `-=` operators
- ✅ Mirror OnEnable and OnDisable

### Static Events Pattern
```csharp
void OnEnable()  => INV_Loot.OnItemLooted += AddItem;
void OnDisable() => INV_Loot.OnItemLooted -= AddItem;
```

**Rules:**
- ✅ Lambda syntax for single-line subscriptions
- ✅ Align `=>` operators

### Optional Components Pattern
```csharp
void Awake()
{
    c_FX ??= GetComponent<C_FX>();
    // No validation = optional
}

void HandleDamaged(int amount)
{
    c_FX?.FlashOnDamaged();  // Null-conditional operator
}
```

**Rules:**
- ✅ Optional components: use `?.` operator
- ❌ NEVER: `if (c_FX) c_FX.Method();`
- ✅ ALWAYS: `c_FX?.Method();`

---

## ⚠️ 4. P_InputActions Lifecycle (CRITICAL)

**Problem:** Scene transitions can destroy objects WITHOUT calling OnDisable → memory leaks.

**Solution:** Create in Awake(), Dispose in OnDestroy().

```csharp
[Header("References")]
P_InputActions input;  // Private, in References section

void Awake()
{
    input = new P_InputActions();  // NO ??=, always create fresh
}

void OnEnable()
{
    input.UI.Enable();    // Enable specific action maps
    input.Debug.Enable();
}

void OnDisable()
{
    input.UI.Disable();   // Disable matching maps
    input.Debug.Disable();
}

void OnDestroy()
{
    input?.Dispose();  // ONLY place to Dispose (guaranteed call)
}
```

**Rules:**
- ✅ Declare in References (private, no public/serialization)
- ✅ Create in Awake: `input = new P_InputActions()` (NO `??=`)
- ✅ Enable maps in OnEnable
- ✅ Disable maps in OnDisable (mirror enables)
- ✅ Dispose in OnDestroy with `?.`
- ❌ NEVER create in OnEnable (creates duplicates)
- ❌ NEVER Dispose in OnDisable (might not be called)
- ❌ NEVER use `??=` for P_InputActions

---

## 💬 5. Commenting Rules

### Function Comments (Selective)
```csharp
// Adds item to inventory, stacking into existing slots first
public void AddItem(INV_ItemSO itemSO, int quantity) { ... }

// Spawns loot prefab at player position
public void DropItem(INV_ItemSO itemSO, int quantity) { ... }

// Update all slots & gold text at start
void Start() { ... }

// Self-explanatory - no comment needed
public void Hide() { ... }
```

**Rules:**
- ✅ Comment public methods if behavior non-obvious
- ✅ Skip if method name is self-explanatory (`Hide`, `Show`, `UpdateUI`)
- ✅ Unity lifecycle methods if doing something special
- ✅ Focus on WHAT/WHY, not HOW

### Inline Comments (Complex Logic Only)
```csharp
// ✅ DO comment:
if (itemSO.isGold) { AddGold(quantity); return; }  // Gold handled specially

// Duration == 0 is PERMANENT, Duration == 1 is INSTANT
if (stat.Duration == 0) { ApplyPermanentEffect(stat); }

// Select random subset of unique items to drop
itemsToDrop = lootTable.OrderBy(x => Random.value).Take(numberOfDrops).ToList();

// ❌ DON'T comment obvious code:
isDead = true;        // NO
gold += 10;           // NO
sprite = itemSO.image; // NO
```

**Rules:**
- ✅ Comment: Special cases, non-obvious logic, magic numbers, workarounds
- ❌ Don't: Obvious assignments, simple operations
- ✅ Preserve all original user comments

---

## 🎨 6. Alignment Examples

### Complete Script Example
```csharp
using UnityEngine;
using TMPro;

public class INV_Manager : MonoBehaviour
{
    public static INV_Manager Instance;

    [Header("Central API for the Inventory system, depend on P_StatsManager")]
    [Header("References")]
    P_StatsManager p_StatsManager;

    [Header("MUST wire MANUALLY in Inspector")]
    public TMP_Text   goldText;
    public GameObject lootPrefab;
    public Transform  player;

    [Header("Data")]
                    public int         gold;
                    public INV_Slots[] inv_Slots;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Find managers in Start, not here
        if (!goldText)   { Debug.LogError($"{name}: goldText is missing!", this); return; }
        if (!lootPrefab) { Debug.LogError($"{name}: lootPrefab is missing!", this); return; }
    }

    void Start()
    {
        p_StatsManager = FindFirstObjectByType<P_StatsManager>();
        
        if (!p_StatsManager) { Debug.LogError($"{name}: P_StatsManager not found!", this); return; }

        foreach (INV_Slots slot in inv_Slots) slot.UpdateUI();
        UpdateGoldText();
    }

    void OnEnable()  => INV_Loot.OnItemLooted += AddItem;
    void OnDisable() => INV_Loot.OnItemLooted -= AddItem;

    // Adds item to inventory, stacking into existing slots first
    public void AddItem(INV_ItemSO itemSO, int quantity)
    {
        // Gold handled specially
        if (itemSO.isGold)
        {
            gold += quantity;
            UpdateGoldText();
            return;
        }

        // Implementation...
    }

    void UpdateGoldText()
    {
        goldText.text = gold.ToString();
    }
}
```

---

## ✅ Checklist

- [ ] Optional context header if complex script
- [ ] References first (private, aligned)
- [ ] "MUST wire MANUALLY" section (public, aligned)
- [ ] Public settings with deep indentation
- [ ] Runtime variables last with comment
- [ ] Awake: GetComponent same-GameObject only
- [ ] Start: FindFirstObjectByType and singletons
- [ ] P_InputActions: create in Awake (no ??=), Dispose in OnDestroy
- [ ] No ??= for Find methods or Inspector refs
- [ ] Debug format: `$"{name}: Message!", this`
- [ ] Return after LogError
- [ ] No null checks in OnEnable/OnDisable
- [ ] Optional components use `?.`
- [ ] Comments on complex logic only
- [ ] Align all operators

---

## 🎯 Why These Rules?

✅ **Consistency** - Predictable patterns across all scripts
✅ **Safety** - Proper validation prevents null errors  
✅ **Maintainability** - Clear separation of concerns  
✅ **Performance** - Awake/Start split optimizes initialization
✅ **No Memory Leaks** - Proper P_InputActions lifecycle

---

**This guide reflects actual codebase patterns from INV_Manager, SHOP_Manager, INV_Loot, C_Health, and SYS_GameManager.** 🚀
