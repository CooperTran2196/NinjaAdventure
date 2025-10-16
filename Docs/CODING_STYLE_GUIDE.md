# NinjaAdventure - Coding Style Guide

**CRITICAL:** This file establishes project-wide coding conventions. **NEVER DELETE THIS FILE.**

---

## 🤖 Instructions for AI Assistants

**When a user shares this file with you:**

1. **Read this ENTIRE document first** - All rules are mandatory for this project
2. **Wait for the user to specify which file to refactor** - Don't act until asked
3. **Apply ALL rules from this guide** when refactoring:
   - Component naming (c_Health, c_Stats, sr, rb)
   - Field order (References → Settings → Runtime)
   - Awake golden template (assignments → validation → setup)
   - Alignment (all operators aligned)
   - Comments (preserve originals + explain complex logic)
4. **After refactoring, explain changes briefly** - Don't repeat the entire guide
5. **If user asks about specific rules**, refer to relevant sections below

**Recently refactored files (examples):**
- `ENV_Destructible.cs` - Environmental objects with particle systems
- `C_FX.cs` - Flash and fade effects
- `C_AfterimageSpawner.cs` - Dodge trail effect
- `C_Mana.cs` - Mana management system
- `C_Stats.cs` - Character stats (data-only class)

**Project architecture:** See `.github/copilot-instructions.md` for system overview.

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

// Enemy-specific files can use e_ prefix for Character components:
C_Health → e_Health  (in enemy-only scripts)
C_Stats  → e_Stats   (in enemy-only scripts)

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
- [ ] **Keep all original comments** (don't remove existing documentation)
- [ ] **Add comments for complex code** (per-line explanation when needed)

---

## 💬 Commenting Rules

### **1. ALWAYS Preserve Original Comments:**

```csharp
// BEFORE (user's original code):
void Update()
{
    // Jump logic - checks if grounded first
    if (isGrounded && Input.GetKeyDown(KeyCode.Space))
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }
}

// AFTER (KEEP the comment!):
void Update()
{
    // Jump logic - checks if grounded first
    if (isGrounded && Input.GetKeyDown(KeyCode.Space))
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }
}
```

**Rules:**
- ✅ **NEVER remove user's original comments**
- ✅ **Preserve comment style** (line comments, block comments)
- ✅ **Preserve comment location** (above line, inline, etc.)

---

### **2. Add Comments for Complex Code:**

If code is **not immediately obvious**, add per-line comments:

```csharp
// COMPLEX CODE - Add comments:
IEnumerator Flash(Color tint)
{
    // Preserve original alpha (don't override transparency)
    float a = sr.color.a;
    
    // Flash with tint color, keep original alpha
    sr.color = new Color(tint.r, tint.g, tint.b, a);
    
    yield return new WaitForSeconds(flashDuration);
    
    // Restore original color, keep current alpha
    sr.color = new Color(baseRGB.r, baseRGB.g, baseRGB.b, a);
}

// ANOTHER EXAMPLE:
void CalculateDamage(int attackDamage, int armorPen)
{
    // Calculate effective armor after penetration
    float effectiveArmor = Mathf.Max(0, armor - armorPen);
    
    // Armor reduction formula: damage * (100 / (100 + armor))
    float damageMultiplier = 100f / (100f + effectiveArmor);
    
    // Apply damage multiplier, clamp to minimum 1
    int finalDamage = Mathf.Max(1, Mathf.RoundToInt(attackDamage * damageMultiplier));
    
    return finalDamage;
}
```

---

### **3. What Needs Comments:**

✅ **Complex math/formulas:**
```csharp
// Smoothstep interpolation for fade curve
float t = time * time * (3f - 2f * time);
```

✅ **Non-obvious game logic:**
```csharp
// Check dash cooldown before allowing input
if (Time.time - lastDashTime < dashCooldown) return;
```

✅ **Workarounds/edge cases:**
```csharp
// Force re-enable collider (Unity bug: disabled on scene reload)
boxCol.enabled = false;
boxCol.enabled = true;
```

✅ **Performance considerations:**
```csharp
// Cache result to avoid expensive GetComponent every frame
if (cachedController == null)
    cachedController = GetComponent<P_Controller>();
```

✅ **Color manipulation:**
```csharp
// Preserve alpha channel when changing color
sr.color = new Color(newColor.r, newColor.g, newColor.b, sr.color.a);
```

---

### **4. What DOESN'T Need Comments:**

❌ **Self-explanatory code:**
```csharp
// BAD (obvious):
// Set isDead to true
isDead = true;

// GOOD (no comment needed):
isDead = true;
```

❌ **Clear method names:**
```csharp
// BAD (method name explains itself):
// Handle the death event
HandleDeath();

// GOOD (no comment needed):
HandleDeath();
```

❌ **Simple assignments:**
```csharp
// BAD (obvious):
// Store original color
originalColor = sr.color;

// GOOD (no comment needed):
originalColor = sr.color;
```

---

### **5. Comment Style Guide:**

**Single-line comments:**
```csharp
// Use for short explanations (preferred)
float speed = 5f;
```

**Inline comments:**
```csharp
int damage = baseDamage * 2; // Double damage on crit
```

**Multi-line comments:**
```csharp
// Use for longer explanations or algorithm descriptions
// Line 2 of explanation
// Line 3 of explanation
```

**Section separators:**
```csharp
// ========== MOVEMENT ==========
void HandleMovement() { ... }

// ========== COMBAT ==========
void HandleAttack() { ... }
```

**Header comments (optional):**
```csharp
/// <summary>
/// Applies damage with armor calculation and triggers events.
/// </summary>
/// <param name="damage">Base damage before armor reduction</param>
/// <returns>Actual damage dealt after armor</returns>
public int ApplyDamage(int damage) { ... }
```

---

### **6. Example - Properly Commented Complex Code:**

```csharp
IEnumerator FadeAndDestroy(GameObject go)
{
    float t = 0f;
    var c = sr.color;
    
    // Gradually fade out sprite over deathFadeTime duration
    while (t < deathFadeTime)
    {
        t += Time.deltaTime;
        
        // Calculate fade amount (1 = visible, 0 = invisible)
        float k = 1f - Mathf.Clamp01(t / deathFadeTime);
        
        // Apply fade to alpha channel only
        sr.color = new Color(c.r, c.g, c.b, k);
        
        yield return null;
    }
    
    // Enemy path: destroy GameObject
    if (destroySelfOnDeath)
    {
        Destroy(go);
    }
    // Player path: restore alpha and disable (for respawn)
    else
    {
        sr.color = new Color(c.r, c.g, c.b, 1f);
        go.SetActive(false);
    }
}
```

---

## 🎯 Commenting Philosophy

**Goal:** Code should be **self-documenting** through good naming, but **complex logic needs explanation**.

✅ **DO comment:**
- Complex algorithms
- Non-obvious game logic
- Math formulas
- Workarounds/edge cases
- Why something is done (not just what)

❌ **DON'T comment:**
- Obvious operations
- Self-explanatory code
- Redundant information
- What the code does (if clear from reading it)

**Golden Rule:** If someone unfamiliar with the code would ask "why?" or "how does this work?", add a comment! 💡

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
