# NinjaAdventure - Coding Style Guide

**CRITICAL:** This file establishes project-wide coding conventions. **NEVER DELETE THIS FILE.**

---

## 🤖 Instructions for AI Assistants

1. **Read this ENTIRE document first** - All rules are mandatory
2. **Wait for the user to specify which file to refactor** - Don't act until asked
3. **Apply ALL rules** when refactoring
4. **After refactoring, explain changes briefly** - Don't repeat the guide

**Project architecture:** See `.github/copilot-instructions.md` for system overview.

---

## 📋 Quick Reference

### Field Order
```csharp
[Header("References")]              // 1. Components (private)
C_Health c_Health;

[Header("MUST wire MANUALLY in Inspector")]  // 2. UI (public if needed)
public TMP_Text goldText;

[Header("Movement Settings")]       // 3. Public Settings
public float moveSpeed = 5f;

// Runtime Variables                // 4. Private state (last)
bool isDead;
```

### Awake Pattern
```csharp
void Awake()
{
    // 1. GetComponent (aligned ??=)
    c_Health ??= GetComponent<C_Health>();
    sr       ??= GetComponent<SpriteRenderer>();
    
    // 2. Validate required (aligned, with context + this)
    if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
    if (!sr)       { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
}
```

### Debug Format
```csharp
Debug.LogError($"{name}: Component is missing!", this);    // Required
Debug.LogWarning($"{name}: Component is missing!", this);  // Optional
```

---

## � 1. Field Declaration Order

### 1.1 Component References (Always First)
```csharp
[Header("References")]
C_Health       c_Health;      // Custom: camelCase with prefix
C_Stats        c_Stats;
SpriteRenderer sr;            // Unity: short abbreviations
Rigidbody2D    rb;
Animator       anim;
```

**Rules:**
- ✅ Custom components: `c_Health`, `p_Controller`, `w_Sword`, `inv_Manager`
  - Enemy-only scripts can use: `e_Health`, `e_Stats`
- ✅ Unity components: `sr`, `rb`, `anim`, `boxCol`, `ps`
- ✅ ALL private (no `public`, no `[SerializeField]`)
- ✅ Align variable names

**Exception - UI/Critical References:**
```csharp
[Header("MUST wire MANUALLY in Inspector")]
public TMP_Text      goldText;
public GameObject    lootPrefab;
public Transform     statContainer;
```
- ✅ UI components (TMP_Text, Image) can be `public`
- ✅ Use header: `"MUST wire MANUALLY in Inspector"` or `"References - Assign in Inspector"`

### 1.2 Public Settings
```csharp
[Header("Movement Settings")]
                    public float moveSpeed = 5f;
                    public float jumpForce = 10f;
[Range(0, 100)]     public float accuracy  = 75f;
```

**Rules:**
- ✅ Deep indentation alignment (types/names line up)
- ✅ Attributes on same line: `[Range(0, 100)] public float`
- ✅ Descriptive headers encouraged: `[Header("Central API for Inventory")]`

### 1.3 Runtime Variables (Last)
```csharp
// State tracking
bool isDead;
bool isGrounded;
float attackTimer;
```

**Rules:**
- ✅ Last section of fields
- ✅ Private (no modifier keyword)
- ✅ Not visible in Inspector

---

## 🔧 2. Awake() Pattern (STANDARD)

```csharp
void Awake()
{
    // 1. GetComponent (aligned ??=)
    c_Health ??= GetComponent<C_Health>();
    c_Stats  ??= GetComponent<C_Stats>();
    sr       ??= GetComponent<SpriteRenderer>();
    rb       ??= GetComponent<Rigidbody2D>();
    
    // 2. Validate REQUIRED (aligned, one-liner, with context + this)
    if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
    if (!sr)       { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
    
    // Note: c_Stats is optional (no error if missing)
    
    // 3. Additional setup (if needed)
    originalColor = sr.color;
}
```

**Rules:**
- ✅ All `??=` aligned at top
- ✅ All `if (!x)` checks aligned next
- ✅ Format: `Debug.LogError($"{name}: Component is missing!", this)`
- ✅ ALWAYS `return;` after error
- ✅ One-liner format: `if (!x) { Debug.LogError(...); return; }`

**Debug.LogWarning vs LogError:**
```csharp
// Required component - LogError
if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }

// Optional component - LogWarning (no return)
if (!audioSource) Debug.LogWarning($"{name}: AudioSource is missing!", this);
```

---

## 🔄 3. Event Subscription Pattern

### No Null Checks After Validation
```csharp
void OnEnable()
{
    // No if (c_Health) check - already validated in Awake
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
- ✅ No null checks (already validated in Awake)
- ✅ Align `+=` and `-=` operators
- ✅ No null checks in OnDisable if subscribed in OnEnable

### Optional Components - Use `?.`
```csharp
void Awake()
{
    c_FX ??= GetComponent<C_FX>();
    // No error check = optional
}

void HandleDamaged()
{
    c_FX?.FlashOnDamaged();  // Only calls if exists
}
```

**Rules:**
- ✅ Optional components: use `?.` operator (null-conditional)
- ✅ Never: `if (c_FX) c_FX.Method();` ❌
- ✅ Always: `c_FX?.Method();` ✅

---

## 💬 4. Commenting Rules

### Function Comments (Required)
```csharp
// Adds item to inventory, stacking into existing slots first
public void AddItem(INV_ItemSO itemSO, int quantity) { ... }

// Spawns loot prefab at player position
public void DropItem(INV_ItemSO itemSO, int quantity) { ... }

// Update all slots & gold text at start
void Start() { ... }
```

**Rules:**
- ✅ One-line comment above each public method
- ✅ Unity lifecycle methods (Start, Update) can have comments if doing something non-obvious
- ✅ Private helpers can skip if name is self-explanatory

### Inline Comments (Complex Logic Only)
```csharp
// ✅ DO comment complex logic:
if (itemSO.isGold) { AddGold(quantity); return; }  // Gold is handled specially

// Smoothstep interpolation for fade curve
float t = time * time * (3f - 2f * time);

// ❌ DON'T comment obvious code:
isDead = true;  // NO COMMENT NEEDED
```

**Rules:**
- ✅ Comment: Complex math, non-obvious logic, workarounds, performance notes
- ❌ Don't comment: Self-explanatory code, simple assignments, obvious operations
- ✅ Preserve ALL original user comments
- ✅ Descriptive headers encouraged: `[Header("Central API for Inventory")]`

---

## � 5. Alignment Examples

### Complete Script Structure
```csharp
using UnityEngine;

public class ExampleScript : MonoBehaviour
{
    [Header("References")]
    C_Health       c_Health;
    C_Stats        c_Stats;
    SpriteRenderer sr;
    Rigidbody2D    rb;
    
    [Header("Movement Settings")]
                    public float moveSpeed = 5f;
                    public float jumpForce = 10f;
[Range(0, 1)]       public float critChance = 0.25f;
    
    bool isDead;
    Vector2 lastPosition;
    
    void Awake()
    {
        c_Health ??= GetComponent<C_Health>();
        c_Stats  ??= GetComponent<C_Stats>();
        sr       ??= GetComponent<SpriteRenderer>();
        rb       ??= GetComponent<Rigidbody2D>();
        
        if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
        if (!sr)       { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
        if (!rb)       { Debug.LogError($"{name}: Rigidbody2D is missing!", this); return; }
    }
    
    void OnEnable()
    {
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

- [ ] References first (`[Header("References")]`)
- [ ] Custom components: camelCase prefix (`c_Health`)
- [ ] Unity components: abbreviations (`sr`, `rb`)
- [ ] All components private
- [ ] UI refs can be public with header: `"MUST wire MANUALLY in Inspector"`
- [ ] Public settings aligned (deep indentation)
- [ ] Runtime variables last
- [ ] Awake: `??=` aligned → `if (!x)` aligned → setup
- [ ] Debug format: `Debug.LogError($"{name}: Message!", this)`
- [ ] No null checks in OnEnable/OnDisable
- [ ] Optional components use `?.` operator
- [ ] Function comments on public methods
- [ ] Inline comments for complex logic only
- [ ] Align all operators (`??=`, `+=`, `-=`, `=`)

---

## 🎯 Why These Rules?

✅ **Consistency** - All scripts follow same pattern  
✅ **Readability** - Aligned columns, easy to scan  
✅ **Maintainability** - Clear separation of concerns  
✅ **Safety** - Validation prevents null errors  
✅ **Efficiency** - No redundant checks after validation

---

**Remember:** These rules make the codebase easier to navigate, debug, and extend! 🚀

