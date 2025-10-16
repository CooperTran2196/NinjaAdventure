# NinjaAdventure - Coding Style Guide

**CRITICAL:** This file establishes project-wide coding conventions. **NEVER DELETE THIS FILE.**

---

## 📋 Field Declaration Order & Rules

### **1. Component References** (Always First)

```csharp
[Header("References")]
// Custom components (our scripts) - use camelCase with prefix
C_Health       c_Health;
P_Controller   p_Controller;
W_Base         w_Base;

// Unity built-in components (ALWAYS private, NO [SerializeField])
SpriteRenderer sr;
Rigidbody2D    rb;
Animator       anim;
BoxCollider2D  boxCol;
```

**Rules:**
- ✅ **Custom components** use **camelCase with class prefix**: `c_Health`, `p_Controller`, `w_Sword`
- ✅ **Unity components** use **short abbreviations**: `sr`, `rb`, `anim`, `boxCol`
- ✅ **ALL components are private** (no `public`, no `[SerializeField]`)
- ✅ **Don't write `private` keyword** (it's default in C#)
- ✅ **Align variable names** for easy reading

**Naming Convention:**
```csharp
// Custom Scripts:
C_Health    → c_Health
C_Stats     → c_Stats
C_FX        → c_FX
P_Controller → p_Controller
E_Controller → e_Controller
W_Sword     → w_Sword
INV_Manager → inv_Manager

// Unity Components:
SpriteRenderer   → sr
Rigidbody2D      → rb
Animator         → anim
BoxCollider2D    → boxCol
CircleCollider2D → circleCol
ParticleSystem   → ps (or descriptive name like breakParticleSystem)
```

---

### **2. Public Settings** (Tweakable in Inspector)

```csharp
[Header("Movement Settings")]
                    public float moveSpeed = 5f;
                    public float jumpForce = 10f;
[Range(0, 100)]     public float accuracy  = 75f;
                    public bool  canDash   = true;
```

**Rules:**
- ✅ **All settings are `public`** (visible in Inspector)
- ✅ **Use deep indentation alignment** (all types/names line up vertically)
- ✅ **[Range] attribute on same line** as declaration
- ✅ Use `[Header()]` to group related settings
- ✅ Use `[Tooltip("")]` for complex settings

**Alignment Pattern:**
```csharp
[Header("Combat Settings")]
                    public int   attackDamage  = 10;
                    public float attackRange   = 1.5f;
[Range(0, 1)]       public float critChance    = 0.25f;
                    public bool  autoAim       = false;
```

---

### **3. Runtime Variables** (Last, No Inspector)

```csharp
// State tracking
bool isDead;
bool isGrounded;
bool canMove = true;

// Cached values
Color originalColor;
Vector2 lastPosition;

// Counters
int comboCount;
float attackTimer;
```

**Rules:**
- ✅ **Last section** of field declarations
- ✅ **No access modifier** (private is default)
- ✅ **Not visible in Inspector**
- ✅ Used for runtime state tracking, caches, timers
- ✅ **No Coroutine variables** (use existing systems like C_FX instead)

---

## 🔧 Awake() Pattern (STANDARD FOR ALL SCRIPTS)

### **The Golden Template:**

```csharp
void Awake()
{
    // 1. GetComponent for all references (aligned)
    c_Health            ??= GetComponent<C_Health>();
    c_Stats             ??= GetComponent<C_Stats>();
    sr                  ??= GetComponent<SpriteRenderer>();
    rb                  ??= GetComponent<Rigidbody2D>();
    anim                ??= GetComponent<Animator>();
    breakParticleSystem ??= GetComponentInChildren<ParticleSystem>();

    // 2. Validate REQUIRED components (with context + return!)
    if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
    if (!sr)       { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
    if (!rb)       { Debug.LogError($"{name}: Rigidbody2D is missing!", this); return; }

    // Note: c_Stats, anim are optional (no error if missing)

    // 3. Additional setup (if needed)
    var main = breakParticleSystem.main;
    main.playOnAwake = false;
}
```

**Rules:**
- ✅ **All `??=` assignments at the top** (aligned)
- ✅ **All `if (!component)` checks next** (aligned)
- ✅ **Use `Debug.LogError(..., this)`** for clickable errors
- ✅ **ALWAYS `return;` after error** (prevent cascading failures)
- ✅ **One-liner format:** `if (!x) { Debug.LogError(...); return; }`
- ✅ **Additional setup at the end** (after validation)

---

## 🎯 Event Subscription Pattern

### **No Null Checks After Awake Validation:**

```csharp
void OnEnable()
{
    // Don't recheck - already validated in Awake
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
- ✅ **No `if (c_Health)` checks** (already validated in Awake)
- ✅ **Direct subscription/unsubscription**
- ✅ **Align `+=` and `-=` operators** for readability

---

## 🔄 Optional Component Pattern

### **For Components That May Not Exist:**

If a component is **optional** (not required, no error in Awake), use **null-conditional operator `?.`**:

```csharp
// In Awake - no error if missing
void Awake()
{
    c_Health ??= GetComponent<C_Health>();
    c_FX     ??= GetComponent<C_FX>();
    
    if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
    // Note: c_FX is optional (no error check)
}

// In other methods - use ?. operator
void HandleDamaged(int amount)
{
    c_FX?.FlashOnDamaged(); // Only calls if c_FX exists
}

void SomeMethod()
{
    c_FX?.FlashOnHealed();
    // Never: if (c_FX) c_FX.FlashOnHealed(); ❌
}
```

**Rules:**
- ✅ **Optional components get `?.` operator** (null-conditional)
- ✅ **Never use `if (component) component.Method()`** for optional components
- ✅ **Use `?.` for cleaner, more concise code**
- ✅ **Mark optional in comments** if not obvious

**Example - Required vs Optional:**
```csharp
void Awake()
{
    // REQUIRED components
    c_Health ??= GetComponent<C_Health>();
    sr       ??= GetComponent<SpriteRenderer>();
    
    // OPTIONAL components
    c_FX    ??= GetComponent<C_FX>();
    c_Stats ??= GetComponent<C_Stats>();
    
    // Validate ONLY required
    if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
    if (!sr)       { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
    
    // Optional: c_FX, c_Stats (no error if missing)
}

void Update()
{
    // Required components - direct access
    sr.color = Color.white;
    
    // Optional components - use ?.
    c_FX?.FlashOnDamaged();
    int currentHP = c_Stats?.currentHP ?? 0;
}
```

---

## 🎨 Component Reusability

### **Prefer Existing Systems Over Duplication:**

❌ **BAD:**
```csharp
Coroutine flashCoroutine;
Color originalColor;

IEnumerator FlashDamage()
{
    sr.color = damageFlashColor;
    yield return new WaitForSeconds(flashDuration);
    sr.color = originalColor;
}
```

✅ **GOOD:**
```csharp
C_FX c_FX; // Reuse existing flash system

void Awake()
{
    c_FX = GetComponent<C_FX>();
    // Note: c_FX is optional (no error if missing)
}

void HandleDamaged(int amount)
{
    if (c_FX) c_FX.FlashOnDamaged(); // Use existing system
}
```

**Rule:** If a component already exists (`C_FX`, `C_Health`, `C_Stats`), **reuse it** instead of duplicating logic.

---

## 📐 Alignment Examples

### **References Section:**

```csharp
[Header("References")]
C_Health       c_Health;
C_Stats        c_Stats;
C_FX           c_FX;
SpriteRenderer sr;
Rigidbody2D    rb;
Animator       anim;
ParticleSystem breakParticleSystem;
```

### **Settings Section:**

```csharp
[Header("Loot Settings")]
                    public GameObject     lootPrefab;
                    public float          dropSpread    = 0.75f;
[Range(0, 100)]     public float          dropChance    = 50f;
                    public int            numberOfDrops = 1;
                    public List<LootDrop> lootTable;
```

### **Awake GetComponent:**

```csharp
void Awake()
{
    c_Health            ??= GetComponent<C_Health>();
    c_Stats             ??= GetComponent<C_Stats>();
    sr                  ??= GetComponent<SpriteRenderer>();
    rb                  ??= GetComponent<Rigidbody2D>();
    breakParticleSystem ??= GetComponentInChildren<ParticleSystem>();
}
```

### **Awake Validation:**

```csharp
if (!c_Health)            { Debug.LogError($"{name}: C_Health is missing!", this); return; }
if (!sr)                  { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
if (!breakParticleSystem) { Debug.LogError($"{name}: ParticleSystem is missing (add as child)!", this); return; }
```

---

## 📚 Complete Example

```csharp
using UnityEngine;

public class ExampleScript : MonoBehaviour
{
    // ========== 1. REFERENCES ==========
    [Header("References")]
    C_Health       c_Health;
    C_Stats        c_Stats;
    C_FX           c_FX;
    SpriteRenderer sr;
    Rigidbody2D    rb;
    Animator       anim;
    
    // ========== 2. PUBLIC SETTINGS ==========
    [Header("Movement")]
                    public float moveSpeed = 5f;
                    public float jumpForce = 10f;
    
    [Header("Combat")]
                    public int   attackDamage = 10;
                    public float attackRange  = 1.5f;
[Range(0, 1)]       public float critChance   = 0.25f;
    
    // ========== 3. RUNTIME VARIABLES (Last) ==========
    bool isDead;
    bool isGrounded;
    Vector2 lastPosition;
    
    void Awake()
    {
        // GetComponent (aligned)
        c_Health ??= GetComponent<C_Health>();
        c_Stats  ??= GetComponent<C_Stats>();
        c_FX     ??= GetComponent<C_FX>();
        sr       ??= GetComponent<SpriteRenderer>();
        rb       ??= GetComponent<Rigidbody2D>();
        anim     ??= GetComponent<Animator>();
        
        // Validate required (one-liners with context + return)
        if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
        if (!sr)       { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
        if (!rb)       { Debug.LogError($"{name}: Rigidbody2D is missing!", this); return; }
        
        // Optional: c_Stats, c_FX, anim (no error if missing)
    }
    
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
}
```

---

## ✅ Quick Checklist

When writing/reviewing a script:

- [ ] **References section first** (`[Header("References")]`)
- [ ] **Custom components use camelCase prefix** (`c_Health`, `p_Controller`)
- [ ] **Unity components use abbreviations** (`sr`, `rb`, `anim`)
- [ ] **All components are private** (no `public`, no `[SerializeField]`)
- [ ] **Public settings aligned with deep indentation**
- [ ] **Runtime variables last** (no Inspector visibility)
- [ ] **Awake() follows golden template:**
  - All `??=` assignments first (aligned)
  - All `if (!x)` checks next (aligned, one-liners)
  - `Debug.LogError(..., this)` with `return;`
- [ ] **No null checks in OnEnable/OnDisable** (already validated)
- [ ] **Reuse existing components** (C_FX, C_Health) instead of duplicating
- [ ] **Align operators** (`??=`, `+=`, `-=`, `=`) for readability

---

## 🎯 Why These Rules?

✅ **Consistency:** All scripts follow same pattern  
✅ **Readability:** Easy to find what you need (aligned columns)  
✅ **Maintainability:** Clear separation of concerns  
✅ **Encapsulation:** All components stay private  
✅ **Error handling:** Clickable, single errors with context  
✅ **Reusability:** Leverage existing systems (C_FX, C_Health)  
✅ **Safety:** Awake validation prevents null reference errors  
✅ **Performance:** No redundant null checks after validation

---

**Remember:** These rules make the codebase easier to navigate, debug, and extend. Follow them consistently! 🚀
