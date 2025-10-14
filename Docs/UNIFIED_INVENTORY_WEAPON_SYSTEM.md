# Unified Inventory & Weapon System Implementation Plan

## 📋 Overview

This document outlines the implementation of a unified inventory system that holds both items AND weapons, with hotbar support and weapon swapping functionality.

---

## 🎯 Features

### 1. Unified Inventory (9 Slots)
- Single inventory holds both weapons and items
- Number keys 1-9 for quick actions:
  - **Item slot?** → Consume/use the item
  - **Weapon slot?** → Equip weapon, swap old weapon into that slot
- Drag & drop between slots to reorganize
- Visual distinction between items and weapons

### 2. Weapon Display UI
- Simple display showing currently equipped weapon icon
- Event-driven updates (no polling in Update())
- Display-only (no drag & drop on this UI)
- Always shows 1 melee + 1 ranged weapon

### 3. Player Weapon System
- Player always has 2 weapon GameObjects:
  - MeleeWeapon (with W_Melee component)
  - RangedWeapon (with W_Ranged component)
- Weapon swapping = changing ScriptableObject reference (no GameObject destruction)
- One weapon is "active" at a time
- Future: Hotkey to toggle melee ↔ ranged

---

## 🏗️ Architecture

### File Structure
```
Assets/GAME/Scripts/
├── Inventory/
│   ├── INV_Manager.cs (MODIFY - add weapon handling)
│   ├── INV_Slots.cs (MODIFY - add weapon support)
│   ├── INV_ItemSO.cs (MODIFY - add unlock flag for ultimates)
│   └── INV_HotbarInput.cs (NEW - number key handling)
│
├── Player/
│   ├── P_Controller.cs (MODIFY - weapon swapping logic)
│   └── P_State_Attack.cs (MODIFY - activeWeapon as property)
│
├── UI/
│   └── WeaponUI.cs (NEW - weapon display)
│
└── Weapon/
    └── W_SO.cs (MODIFY - add icon field)
```

---

## 📦 Component Details

---

## 1️⃣ INV_Slots.cs (MODIFY)

**Purpose:** Extend existing slot to support weapons

### Changes Required (~50 lines added)

```csharp
public class INV_Slots : MonoBehaviour, IPointerClickHandler
{
    [Header("Slot Type")]
    public enum SlotType { Empty, Item, Weapon }
    public SlotType type = SlotType.Empty;
    
    [Header("Data")]
    public INV_ItemSO itemSO;      // for items (existing)
    public W_SO weaponSO;           // NEW: for weapons
    public int quantity;            // existing
    
    [Header("UI")]
    public Image itemImage;         // existing
    public TMP_Text amountText;     // existing
    
    // ... existing fields (shop references, etc.)
    
    public void UpdateUI()
    {
        if (type == SlotType.Item && itemSO)
        {
            // Existing item display
            itemImage.enabled = true;
            itemImage.sprite = itemSO.image;
            amountText.text = quantity.ToString();
        }
        else if (type == SlotType.Weapon && weaponSO)
        {
            // NEW: Weapon display
            itemImage.enabled = true;
            itemImage.sprite = weaponSO.icon; // uses new icon field
            amountText.text = ""; // weapons don't stack
            
            // Optional: Add visual indicator (border color, glow, etc.)
            // itemImage.color = Color.red; // example: red tint for weapons
        }
        else // Empty
        {
            itemImage.enabled = false;
            amountText.text = "";
            type = SlotType.Empty;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (type == SlotType.Empty) return;
        
        // Shop interactions (items only, weapons can't be sold in shop)
        if (shop_Manager && type == SlotType.Item)
        {
            // ... existing shop code ...
            return;
        }
        
        // Normal interactions (shop closed)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (type == SlotType.Item)
                inv_Manager.UseItem(this); // existing
            else if (type == SlotType.Weapon)
                inv_Manager.EquipWeapon(this); // NEW method
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Drop to world
            if (type == SlotType.Item)
            {
                // Existing drop item code
                inv_Manager.DropItem(itemSO, 1);
                quantity -= 1;
                if (quantity <= 0)
                {
                    itemSO = null;
                    type = SlotType.Empty;
                }
            }
            else if (type == SlotType.Weapon)
            {
                // NEW: Drop weapon (future feature)
                inv_Manager.DropWeapon(weaponSO);
                weaponSO = null;
                type = SlotType.Empty;
            }
            UpdateUI();
        }
    }
}
```

### Key Changes:
1. Add `SlotType` enum (Empty, Item, Weapon)
2. Add `W_SO weaponSO` field
3. Update `UpdateUI()` to handle weapons
4. Update `OnPointerClick()` to call `EquipWeapon()` for weapons

---

## 2️⃣ INV_Manager.cs (MODIFY)

**Purpose:** Add weapon handling methods

### New Methods (~80 lines added)

```csharp
public class INV_Manager : MonoBehaviour
{
    // ... existing fields (slots, gold, etc.)
    
    // NEW: Weapon handling methods
    
    /// <summary>
    /// Equips weapon from inventory slot, swaps with currently equipped weapon
    /// </summary>
    public void EquipWeapon(INV_Slots slot)
    {
        if (slot.weaponSO == null) return;
        
        W_SO newWeapon = slot.weaponSO;
        
        // Swap weapon with player
        W_SO oldWeapon = P_Controller.instance.EquipWeapon(newWeapon);
        
        // Put old weapon back in the slot that was just pressed
        slot.weaponSO = oldWeapon;
        slot.type = INV_Slots.SlotType.Weapon;
        slot.UpdateUI();
    }
    
    /// <summary>
    /// Adds weapon to first empty inventory slot
    /// </summary>
    public bool AddWeapon(W_SO weaponSO)
    {
        // Find first empty slot
        foreach (var slot in inv_Slots)
        {
            if (slot.type == INV_Slots.SlotType.Empty)
            {
                slot.weaponSO = weaponSO;
                slot.type = INV_Slots.SlotType.Weapon;
                slot.UpdateUI();
                return true;
            }
        }
        
        // Inventory full
        Debug.Log("Inventory full! Cannot add weapon: " + weaponSO.id);
        return false;
    }
    
    /// <summary>
    /// Drops weapon to world (future implementation)
    /// </summary>
    public void DropWeapon(W_SO weaponSO)
    {
        // TODO: Implement weapon drop to world
        // Similar to DropItem but spawns weapon pickup prefab
        Debug.Log($"Dropped weapon: {weaponSO.id}");
        
        // Future implementation:
        // Vector3 dropPos = player.position + player.transform.right * 1f;
        // GameObject dropped = Instantiate(weaponSO.worldPrefab, dropPos, Quaternion.identity);
        // dropped.GetComponent<WeaponPickup>().weaponData = weaponSO;
    }
    
    // ... existing methods (UseItem, DropItem, etc.)
}
```

### Key Additions:
1. `EquipWeapon(slot)` - Swaps weapon with player
2. `AddWeapon(weaponSO)` - Adds weapon to inventory
3. `DropWeapon(weaponSO)` - Drops weapon to world (placeholder)

---

## 3️⃣ INV_HotbarInput.cs (NEW)

**Purpose:** Handle number key inputs for hotbar

### Full Implementation (~100 lines)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles number key inputs (1-9) for inventory hotbar
/// Attach to Player or GameManager
/// </summary>
public class INV_HotbarInput : MonoBehaviour
{
    private INV_Manager invManager;
    private P_InputActions inputActions;
    
    void Awake()
    {
        invManager = INV_Manager.Instance;
        inputActions = new P_InputActions();
        
        if (!invManager)
            Debug.LogError("INV_HotbarInput: INV_Manager.Instance is null!");
    }
    
    void OnEnable()
    {
        inputActions.UI.Enable();
        
        // Subscribe to hotbar actions (assuming you create these in Unity)
        inputActions.UI.InventorySlot1.performed += ctx => UseSlot(0);
        inputActions.UI.InventorySlot2.performed += ctx => UseSlot(1);
        inputActions.UI.InventorySlot3.performed += ctx => UseSlot(2);
        inputActions.UI.InventorySlot4.performed += ctx => UseSlot(3);
        inputActions.UI.InventorySlot5.performed += ctx => UseSlot(4);
        inputActions.UI.InventorySlot6.performed += ctx => UseSlot(5);
        inputActions.UI.InventorySlot7.performed += ctx => UseSlot(6);
        inputActions.UI.InventorySlot8.performed += ctx => UseSlot(7);
        inputActions.UI.InventorySlot9.performed += ctx => UseSlot(8);
    }
    
    void OnDisable()
    {
        inputActions.UI.Disable();
    }
    
    /// <summary>
    /// Uses inventory slot by index (0-8 for slots 1-9)
    /// </summary>
    private void UseSlot(int index)
    {
        if (invManager == null) return;
        if (index >= invManager.inv_Slots.Length) return;
        
        INV_Slots slot = invManager.inv_Slots[index];
        
        if (slot.type == INV_Slots.SlotType.Item)
        {
            invManager.UseItem(slot);
        }
        else if (slot.type == INV_Slots.SlotType.Weapon)
        {
            invManager.EquipWeapon(slot);
        }
        // If empty, do nothing
    }
}
```

### Notes:
- Attach to Player GameObject or a persistent manager
- Requires input actions in `P_InputActions.inputactions`:
  - UI Map → InventorySlot1 (binding: 1)
  - UI Map → InventorySlot2 (binding: 2)
  - ... through InventorySlot9 (binding: 9)

---

## 4️⃣ P_Controller.cs (MODIFY)

**Purpose:** Simplified weapon swapping (no GameObject destruction)

### Current Setup (Your Existing System)
```
Player (GameObject)
├── MeleeWeapon (GameObject)
│   └── W_Melee component (weaponData SO reference)
└── RangedWeapon (GameObject)
    └── W_Ranged component (weaponData SO reference)
```

### Changes Required (~80 lines added)

```csharp
public class P_Controller : MonoBehaviour
{
    [Header("Weapon Setup")]
    public W_Melee meleeWeaponComponent;   // Assigned in Inspector
    public W_Ranged rangedWeaponComponent; // Assigned in Inspector
    
    private W_SO currentMeleeData;   // Currently equipped melee SO
    private W_SO currentRangedData;  // Currently equipped ranged SO
    private W_Base activeWeapon;     // Currently active weapon (one of the two)
    
    // NEW: Event for UI updates
    public static event System.Action<W_SO> OnWeaponChanged;
    
    // ... existing fields (rb, anim, input, states, etc.)
    
    void Start()
    {
        // Initialize with starting weapons
        currentMeleeData = meleeWeaponComponent.weaponData;
        currentRangedData = rangedWeaponComponent.weaponData;
        
        // Start with melee active (or whatever you prefer)
        SwitchToMelee();
    }
    
    /// <summary>
    /// Equips a new weapon, returns the old weapon for inventory swap
    /// </summary>
    public W_SO EquipWeapon(W_SO newWeaponData)
    {
        W_SO oldWeaponData;
        
        if (newWeaponData.type == WeaponType.Melee)
        {
            // Swap melee weapon
            oldWeaponData = currentMeleeData;
            currentMeleeData = newWeaponData;
            meleeWeaponComponent.weaponData = newWeaponData;
            meleeWeaponComponent.Initialize(c_Stats); // Reinitialize with new data
            
            // If melee is currently active, notify UI
            if (activeWeapon == meleeWeaponComponent)
            {
                OnWeaponChanged?.Invoke(newWeaponData);
            }
        }
        else // Ranged or Magic
        {
            // Swap ranged weapon
            oldWeaponData = currentRangedData;
            currentRangedData = newWeaponData;
            rangedWeaponComponent.weaponData = newWeaponData;
            rangedWeaponComponent.Initialize(c_Stats);
            
            // If ranged is currently active, notify UI
            if (activeWeapon == rangedWeaponComponent)
            {
                OnWeaponChanged?.Invoke(newWeaponData);
            }
        }
        
        return oldWeaponData; // Return old weapon for inventory swap
    }
    
    /// <summary>
    /// Switches to melee weapon
    /// </summary>
    public void SwitchToMelee()
    {
        activeWeapon = meleeWeaponComponent;
        rangedWeaponComponent.gameObject.SetActive(false);
        meleeWeaponComponent.gameObject.SetActive(true);
        OnWeaponChanged?.Invoke(currentMeleeData);
    }
    
    /// <summary>
    /// Switches to ranged weapon
    /// </summary>
    public void SwitchToRanged()
    {
        activeWeapon = rangedWeaponComponent;
        meleeWeaponComponent.gameObject.SetActive(false);
        rangedWeaponComponent.gameObject.SetActive(true);
        OnWeaponChanged?.Invoke(currentRangedData);
    }
    
    /// <summary>
    /// Gets current weapon SO for UI display
    /// </summary>
    public W_SO GetCurrentWeaponSO()
    {
        return activeWeapon?.weaponData;
    }
    
    /// <summary>
    /// Gets active weapon component for states
    /// </summary>
    public W_Base GetActiveWeapon()
    {
        return activeWeapon;
    }
    
    // ... rest of existing code (Update, FixedUpdate, etc.)
}
```

### Key Changes:
1. Track both melee and ranged weapon data separately
2. `EquipWeapon()` swaps SO reference, no GameObject destruction
3. `SwitchToMelee()` / `SwitchToRanged()` toggle active weapon
4. `OnWeaponChanged` event for UI updates
5. Getter methods for weapon access

---

## 5️⃣ P_State_Attack.cs (MODIFY)

**Purpose:** Always use fresh weapon reference

### Change Required (~5 lines modified)

**OLD (Cached Reference):**
```csharp
private W_Base activeWeapon;

void Awake()
{
    activeWeapon = GetComponentInChildren<W_Base>();
}

// Used in methods:
activeWeapon.Attack(attackDir);
```

**NEW (Property - Always Fresh):**
```csharp
// Remove cached field, use property instead
private W_Base ActiveWeapon => controller.GetActiveWeapon();

// Used in methods (capital A):
ActiveWeapon.Attack(attackDir);
float penalty = ActiveWeapon.weaponData.comboMovePenalties[comboIndex];
```

### Full Changes:

```csharp
public class P_State_Attack : MonoBehaviour
{
    // ... existing fields (controller, input, etc.)
    
    // REMOVE THIS:
    // private W_Base activeWeapon;
    
    // ADD THIS:
    private W_Base ActiveWeapon => controller.GetActiveWeapon();
    
    void Awake()
    {
        // ... existing code ...
        
        // REMOVE THIS LINE:
        // activeWeapon = GetComponentInChildren<W_Base>();
    }
    
    // Update all references from `activeWeapon` to `ActiveWeapon`:
    
    void StartAttack()
    {
        // ... existing code ...
        ActiveWeapon.Attack(attackDir); // Updated
        comboIndex = ActiveWeapon.GetComboIndex(); // Updated
    }
    
    public float GetCurrentMovePenalty()
    {
        float basePenalty = ActiveWeapon.weaponData.comboMovePenalties[comboIndex]; // Updated
        // ... rest of code
    }
    
    // ... etc. Replace all `activeWeapon` with `ActiveWeapon`
}
```

### Why This Works:
- Property always fetches current weapon from controller
- No stale references after weapon swap
- Automatically updates when player switches melee ↔ ranged

---

## 6️⃣ WeaponUI.cs (NEW)

**Purpose:** Display currently equipped weapon (event-driven)

### Full Implementation (~80 lines)

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays currently equipped weapon icon and name
/// Event-driven, no Update() polling
/// </summary>
public class WeaponUI : MonoBehaviour
{
    [Header("Weapon Display UI")]
    [Header("References")]
    public Image weaponIcon;
    public TMP_Text weaponNameText; // Optional
    
    private P_Controller player;
    
    void Awake()
    {
        player = FindFirstObjectByType<P_Controller>();
        
        if (!weaponIcon)
            Debug.LogError("WeaponUI: weaponIcon is missing!");
        if (!player)
            Debug.LogError("WeaponUI: P_Controller not found!");
    }
    
    void OnEnable()
    {
        // Subscribe to weapon change event
        P_Controller.OnWeaponChanged += UpdateDisplay;
        
        // Initial display
        if (player != null)
        {
            W_SO currentWeapon = player.GetCurrentWeaponSO();
            if (currentWeapon != null)
                UpdateDisplay(currentWeapon);
        }
    }
    
    void OnDisable()
    {
        P_Controller.OnWeaponChanged -= UpdateDisplay;
    }
    
    /// <summary>
    /// Updates UI when weapon changes (event callback)
    /// </summary>
    void UpdateDisplay(W_SO newWeapon)
    {
        if (newWeapon == null) return;
        
        weaponIcon.sprite = newWeapon.icon;
        weaponIcon.enabled = true;
        
        if (weaponNameText)
            weaponNameText.text = newWeapon.id; // or use a displayName field
    }
}
```

### Notes:
- Attach to weapon display UI panel
- Automatically updates when weapon changes
- No performance overhead (event-driven, not polling)

---

## 7️⃣ W_SO.cs (MODIFY)

**Purpose:** Add icon field for UI display

### Change Required (~2 lines added)

```csharp
[CreateAssetMenu(menuName = "Weapon SO", fileName = "W_SO_NewWeapon")]
public class W_SO : ScriptableObject
{
    [Header("Common")]
    public string id = "weaponId";
    public WeaponType type = WeaponType.Melee;
    public Sprite sprite; // existing - used for in-game weapon visual
    public Sprite icon;   // NEW - used for inventory/UI display
    public float offsetRadius = 0.4f;
    
    // ... rest of existing fields
}
```

### Notes:
- `sprite` = In-game weapon visual (what player holds)
- `icon` = UI icon (inventory, weapon display, etc.)
- Can use same sprite for both if you want

---

## 8️⃣ INV_ItemSO.cs (MODIFY)

**Purpose:** Add unlock flag for ultimate skills (future feature)

### Change Required (~10 lines added)

```csharp
[CreateAssetMenu(fileName = "INV_ItemSO", menuName = "Item")]
public class INV_ItemSO : ScriptableObject
{
    // ... existing fields (id, itemName, image, stackSize, etc.)
    
    [Header("Flags")]
    public bool isGold; // existing
    
    // NEW: Ultimate skill unlock
    public bool unlocksSkill = false;
    public string skillIDToUnlock = ""; // matches ST_SkillSO.id
    
    [Header("Item Effects")]
    public List<P_StatEffect> StatEffectList;
    
    // ... existing OnValidate
}
```

### Usage (Future Ultimate System):
```csharp
// In INV_Manager.UseItem():
if (itemSO.unlocksSkill)
{
    ST_Manager.instance.UnlockSkill(itemSO.skillIDToUnlock);
    Debug.Log($"Unlocked ultimate: {itemSO.skillIDToUnlock}");
}
```

---

## 🧩 Unity Setup Checklist

### Input Actions (P_InputActions.inputactions)
- [ ] Open Input Actions asset
- [ ] Go to **UI** action map
- [ ] Add 9 new actions:
  - [ ] InventorySlot1 (binding: Keyboard 1)
  - [ ] InventorySlot2 (binding: Keyboard 2)
  - [ ] InventorySlot3 (binding: Keyboard 3)
  - [ ] InventorySlot4 (binding: Keyboard 4)
  - [ ] InventorySlot5 (binding: Keyboard 5)
  - [ ] InventorySlot6 (binding: Keyboard 6)
  - [ ] InventorySlot7 (binding: Keyboard 7)
  - [ ] InventorySlot8 (binding: Keyboard 8)
  - [ ] InventorySlot9 (binding: Keyboard 9)
- [ ] Save asset
- [ ] Right-click → Generate C# Class (regenerate `P_InputActions.cs`)

### Inventory System
- [ ] Update all `INV_Slots` prefabs/instances in scene
- [ ] Ensure slots have references to INV_Manager
- [ ] Add `INV_HotbarInput` component to Player or GameManager

### Player Setup
- [ ] Ensure Player has 2 weapon GameObjects as children:
  - [ ] MeleeWeapon (with W_Melee component)
  - [ ] RangedWeapon (with W_Ranged component)
- [ ] Assign weapon components in P_Controller Inspector:
  - [ ] Drag MeleeWeapon → `meleeWeaponComponent` field
  - [ ] Drag RangedWeapon → `rangedWeaponComponent` field
- [ ] Set starting weapon ScriptableObjects on each component

### Weapon Display UI
- [ ] Create UI panel (Canvas → Panel)
- [ ] Add Image component (weapon icon)
- [ ] Add TextMeshProUGUI (weapon name) - optional
- [ ] Add `WeaponUI` component to panel
- [ ] Assign icon and text references in Inspector

### Weapon ScriptableObjects
- [ ] Update all weapon SOs to include `icon` sprite
- [ ] Can reuse `sprite` field if you don't have separate icons

---

## 📊 Testing Scenarios

### Inventory Testing
1. **Fill inventory with items and weapons**
   - Verify items stack, weapons don't
   - Check visual distinction (icon, border, etc.)

2. **Hotbar functionality**
   - Press 1-9 to use items/equip weapons
   - Verify item consumption reduces quantity
   - Verify weapon swap updates equipped weapon

3. **Weapon swapping**
   - Equip weapon from inventory
   - Check old weapon appears in inventory slot
   - Verify weapon display UI updates

4. **Edge cases**
   - Try equipping weapon while attacking
   - Full inventory - try adding new weapon
   - Drop weapon to world (when implemented)

### Weapon Display Testing
1. **Event-driven updates**
   - Swap weapons, verify display updates immediately
   - No lag or delay

2. **Initial display**
   - Start game, verify starting weapon shows correctly

3. **Melee ↔ Ranged toggle (future)**
   - If implemented, test switching with hotkey

---

## 🔄 Future Enhancements

### Weapon Switching Hotkey
Add to `P_Controller.cs`:
```csharp
void Update()
{
    // ... existing input handling ...
    
    // Toggle melee ↔ ranged (example: Q key)
    if (input.Player.SwitchWeapon.triggered)
    {
        if (activeWeapon == meleeWeaponComponent)
            SwitchToRanged();
        else
            SwitchToMelee();
    }
}
```

### Weapon Pickup System
- Create `WeaponPickup.cs` component
- Similar to item loot system
- Adds weapon to inventory on collision

### Drag & Drop Between Inventory Slots
- Implement `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler` on `INV_Slots`
- Allow reorganizing inventory by dragging
- Swap slot contents on drop

---

## ✅ Completion Criteria

- [ ] Can press 1-9 to use items or equip weapons
- [ ] Weapon swapping works (old weapon goes to inventory)
- [ ] Weapon display UI shows correct weapon icon
- [ ] No stale weapon references in attack state
- [ ] Inventory slots visually distinguish items vs weapons
- [ ] Starting weapons load correctly on game start
- [ ] No GameObject destruction (just SO reference swaps)
- [ ] Event-driven UI updates (no Update() polling)

---

## 📝 Code Summary

### Files Modified (6)
1. **INV_Slots.cs** - Add weapon support (~50 lines)
2. **INV_Manager.cs** - Add weapon methods (~80 lines)
3. **P_Controller.cs** - Weapon swapping logic (~80 lines)
4. **P_State_Attack.cs** - Property instead of cached field (~5 lines)
5. **W_SO.cs** - Add icon field (~2 lines)
6. **INV_ItemSO.cs** - Add unlock flag (~10 lines)

### Files Created (2)
1. **INV_HotbarInput.cs** - Number key handling (~100 lines)
2. **WeaponUI.cs** - Weapon display UI (~80 lines)

### Total Code: ~407 lines (much less than original 1630!)

---

**Implementation Time Estimate: 3-4 days**

**Ready to start coding?** This system integrates cleanly with your existing architecture! 🚀
