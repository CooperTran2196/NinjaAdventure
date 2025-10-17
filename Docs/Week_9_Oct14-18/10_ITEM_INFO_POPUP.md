# 10. Item Info Popup System - Shared Tooltip

**Status:** ✅ Complete  
**Date Completed:** October 15, 2025  
**Feature:** Unified hover tooltip for inventory and shop

---

## 📋 Overview

Implemented a shared info popup system that displays item/weapon stats on hover. The system reuses a single popup instance across both inventory and shop UI, with configurable hover delays.

---

## 🎯 Key Features

### 1. Shared Popup Instance
- **Single INV_ItemInfo component** managed by GameManager
- Used by both InventoryCanvas and ShopCanvas
- Prevents duplicate popups and inconsistent behavior

### 2. Dual Display Modes
- **Items:** Name + description + stat effects (duration-based)
- **Weapons:** Name + combat stats (formatted by weapon type)

### 3. Smart Hover Delays
- **Inventory:** 1.0s delay (configurable) - prevents accidental popups
- **Shop:** Instant (0s delay) - quick comparison shopping

### 4. Weapon Stat Formatting
**Melee Weapons (7 lines):**
```
AD: X  AP: Y
Knockback Force: Z
Thrust Distance: A
Slash Arc Degree: B
Speed: X - Y - Z
Speed Penalties: X - Y - Z
Stun Time: X - Y - Z
```

**Ranged Weapons (3 lines):**
```
AD: X  AP: Y
Mana Cost: Z
Projectile Speed: A
```

---

## 🔧 Implementation Details

### Files Modified

#### 1. **SYS_GameManager.cs**
**Added:**
```csharp
[Header("References")]
public INV_ItemInfo itemInfoPopup;  // Shared popup reference
```

**In Awake():**
```csharp
itemInfoPopup ??= FindFirstObjectByType<INV_ItemInfo>();
if (!itemInfoPopup) Debug.LogWarning("SYS_GameManager: ItemInfoPopup is missing.");
if (itemInfoPopup) itemInfoPopup.Hide(); // Start hidden
```

**Purpose:** Centralized access point for popup

---

#### 2. **INV_ItemInfo.cs**
**Extended with weapon support:**

**New Method - Show(W_SO):**
```csharp
public void Show(W_SO weaponSO)
{
    canvasGroup.alpha = 1f;
    
    if (itemNameText) itemNameText.text = weaponSO ? weaponSO.id : string.Empty;
    itemDescText.text = weaponSO.description;
    
    ClearStatLines();
    if (weaponSO)
    {
        var outLines = BuildWeaponStatLines(weaponSO);
        foreach (var line in outLines)
        {
            var textLine = Instantiate(statLinePrefab, statContainer);
            textLine.text = line;
        }
    }
}
```

**New Method - BuildWeaponStatLines(W_SO):**
- Formats weapon stats based on WeaponType (Melee/Ranged)
- Returns List<string> of formatted stat lines
- Melee: 7 lines (AD/AP, Knockback, Thrust, Arc, Speed arrays, Penalties, Stun)
- Ranged: 3 lines (AD/AP, Mana Cost, Projectile Speed)

**Existing Methods (Unchanged):**
- `Show(INV_ItemSO)` - Item display
- `Hide()` - Hide popup
- `FollowMouse(Vector2)` - Position at cursor
- `BuildItemStatLines()` - Item stat formatting

---

#### 3. **INV_Slots.cs**
**Added hover system:**

**New Interfaces:**
```csharp
public class INV_Slots : MonoBehaviour, 
    IPointerClickHandler, 
    IBeginDragHandler, 
    IDragHandler, 
    IEndDragHandler, 
    IDropHandler,
    IPointerEnterHandler,  // ← NEW
    IPointerExitHandler    // ← NEW
```

**New Fields:**
```csharp
[Header("Info Popup Settings")]
public float hoverDelay = 1f; // Delay before showing popup

INV_ItemInfo itemInfoPopup;  // Reference to shared popup
Coroutine hoverCoroutine;  // Tracks pending delay
```

**Hover Logic:**
```csharp
public void OnPointerEnter(PointerEventData eventData)
{
    if (type == SlotType.Empty || !itemInfoPopup) return;
    
    // Start delay coroutine
    hoverCoroutine = StartCoroutine(ShowPopupWithDelay(eventData.position));
}

public void OnPointerExit(PointerEventData eventData)
{
    // Cancel pending hover
    if (hoverCoroutine != null)
    {
        StopCoroutine(hoverCoroutine);
        hoverCoroutine = null;
    }
    
    // Hide popup
    if (itemInfoPopup) itemInfoPopup.Hide();
}

IEnumerator ShowPopupWithDelay(Vector2 mousePosition)
{
    yield return new WaitForSeconds(hoverDelay);
    
    itemInfoPopup.FollowMouse(mousePosition);
    
    if (type == SlotType.Item && itemSO)
        itemInfoPopup.Show(itemSO);
    else if (type == SlotType.Weapon && weaponSO)
        itemInfoPopup.Show(weaponSO);
    
    hoverCoroutine = null;
}
```

**Integration:**
- Gets popup via `SYS_GameManager.Instance.itemInfoPopup` in Awake()
- Hides popup when dragging starts (prevents popup during drag)
- Cancels hover if mouse exits before delay completes

---

#### 4. **UI Bug Fixes (Bonus)**

Fixed null reference errors during scene transitions in:

**ExpUI.cs:**
```csharp
void OnEnable()
{
    if (p_Exp != null)  // ← Added null check
    {
        p_Exp.OnLevelUp += HandleLevelUp;
        p_Exp.OnXPChanged += HandleXPChanged;
        UpdateUI();
    }
}

void OnDisable()
{
    if (p_Exp != null)  // ← Added null check
    {
        p_Exp.OnLevelUp -= HandleLevelUp;
        p_Exp.OnXPChanged -= HandleXPChanged;
    }
}
```

**ManaUI.cs & HealthUI.cs:**
- Same null check pattern before event subscription/unsubscription
- Prevents NullReferenceException when GameManager destroys persistent objects

**WeaponUI.cs:**
- Removed `weaponNameText` field (cleaner display)
- Only shows weapon icon now

**Aspect Ratio Preservation:**
- Weapons: `itemImage.preserveAspect = true` (keep weapon shape)
- Items: `itemImage.preserveAspect = false` (fill slot)

---

## 🎨 UI Structure

### InfoPopup Hierarchy
```
InfoPopup (INV_ItemInfo component)
├── Name (TMP_Text) - Item/Weapon name
├── UniquePanel (Panel)
│   └── Unique (TMP_Text) - "UNIQUE" label
└── StatsPanel (Panel)
    └── Description (TMP_Text) - Stat lines spawn here
```

**Key Properties:**
- **RectTransform:** Size 250 x 421.49, Pivot (0, 1)
- **CanvasGroup:** Alpha 0/1 for show/hide
- **Offset:** (50, -10) from mouse position

---

## 📊 Data Flow

### Inventory Hover Flow
```
User hovers over slot
   ↓
INV_Slots.OnPointerEnter()
   ↓
StartCoroutine(ShowPopupWithDelay(1.0s))
   ↓
Wait 1.0 seconds
   ↓
itemInfoPopup.FollowMouse(mousePos)
   ↓
Check slot type → Show(itemSO) OR Show(weaponSO)
   ↓
Build stat lines → Display popup
```

### Shop Hover Flow
```
User hovers over shop item
   ↓
SHOP_Slot.OnPointerEnter()
   ↓
itemInfoPopup.Show(itemSO) ← INSTANT (no delay)
   ↓
Display popup
```

### Popup Display Logic
```
Show(INV_ItemSO):
- Display item name
- Display description
- Build item stat lines (Duration: 0/1/>1 formatting)
- Show effects (HP+10, AD+5, etc.)

Show(W_SO):
- Display weapon name
- Display description
- Check WeaponType (Melee/Ranged)
- Build appropriate stat lines (7 or 3 lines)
- Show combat stats
```

---

## 🔍 Technical Highlights

### 1. Coroutine-Based Delay
**Why:** Simple, clean, Unity-native solution
- One coroutine per slot
- Auto-cancels on exit
- No static flags or complex state tracking
- Configurable via Inspector (`hoverDelay` field)

### 2. Centralized Popup Management
**Why:** Single source of truth
- GameManager owns the popup
- All UI references same instance
- No duplicate popups
- Consistent behavior everywhere

### 3. Event-Driven Updates
**Why:** Performance and decoupling
- No Update() polling
- UI subscribes to events (OnDamaged, OnManaChanged, etc.)
- Defensive null checks prevent scene transition errors

### 4. Polymorphic Display
**Why:** Code reuse and extensibility
- Single component handles items AND weapons
- Overloaded `Show()` methods
- Easy to add new types (e.g., skills, consumables)

---

## 🎯 Usage Examples

### In Inventory:
1. **Hover over item slot** → Wait 1s → See item stats
2. **Hover over weapon slot** → Wait 1s → See weapon stats
3. **Exit before 1s** → No popup shown (cancelled)
4. **Start dragging** → Popup hidden automatically

### In Shop:
1. **Hover over shop item** → Instant popup with item info
2. **Compare items** → Quick hover, no delay
3. **Purchase decision** → See stats before buying

### Inspector Configuration:
```
INV_Slots component:
├── Slot Type: Empty/Item/Weapon
├── Item SO: [Reference]
├── Weapon SO: [Reference]
├── Hover Delay: 1.0 ← Adjustable (0 = instant, 2 = slower)
```

---

## ✅ Completion Checklist

- ✅ Extended INV_ItemInfo with weapon support
- ✅ Added Show(W_SO) overload
- ✅ Added BuildWeaponStatLines() method
- ✅ Implemented hover interfaces in INV_Slots
- ✅ Added configurable hover delay (1.0s default)
- ✅ Integrated with GameManager
- ✅ Fixed UI null reference errors (ExpUI, ManaUI, HealthUI)
- ✅ Removed weapon name text from WeaponUI
- ✅ Added aspect ratio preservation for weapons
- ✅ Tested hover delay system
- ✅ Tested drag cancellation
- ✅ Verified item/weapon display formatting

---

## 🚀 Future Enhancements (Optional)

**Not implemented, but possible:**
1. **Skill tooltips** - Add Show(ST_SkillSO) for skill tree
2. **Comparison mode** - Show two popups side-by-side
3. **Dynamic positioning** - Smart positioning to avoid screen edges
4. **Animation** - Fade in/out instead of instant show/hide
5. **Keyboard shortcuts** - Hold key to show tooltip instantly

---

## 📖 Related Documentation

- **Unity Setup:** `Docs/UNITY_SETUP_GUIDE.md` (if popup needs extraction)
- **Hierarchy:** `Docs/HIERARCHY.md` - UI system structure
- **Inventory System:** `Week_9_Oct14-18/9_UNIFIED_INVENTORY.md`
- **Balance References:** `Docs/BALANCE_QUICK_REF.md`

---

**Status:** ✅ Production Ready  
**Completion Date:** October 15, 2025  
**Code Quality:** Clean, maintainable, event-driven  
**Performance:** Optimized (no Update() polling, coroutine-based)
