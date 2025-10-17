# Unified Inventory & Weapon System - Complete Documentation
**Date Created:** October 14, 2025 (Monday)  
**Date Completed:** October 15, 2025 (Tuesday)  
**Week:** 9 (Oct 14-18, 2025)  
**Status:** ✅ COMPLETE & Production Ready

---

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Features Implemented](#features-implemented)
4. [Code Structure](#code-structure)
5. [Unity Setup Guide](#unity-setup-guide)
6. [How It Works](#how-it-works)
7. [Troubleshooting](#troubleshooting)
8. [Implementation Timeline](#implementation-timeline)

---

## Overview

A complete unified inventory system that handles both **items** and **weapons** in a single 9-slot inventory with drag-and-drop functionality.

### Key Features
- ✅ **Unified Slots**: Items and weapons share the same inventory slots
- ✅ **Drag & Drop**: Intuitive drag-and-drop to rearrange inventory
- ✅ **Weapon Swapping**: Left-click to equip weapons from inventory
- ✅ **Hotbar Input**: Number keys 1-9 to quickly use/equip
- ✅ **Dual Weapon UI**: Separate displays for equipped melee and ranged weapons
- ✅ **Unified Loot System**: Single loot GameObject handles both items and weapons
- ✅ **Shop Integration**: Sell items in shop, weapons can't be sold

---

## System Architecture

### Core Components

#### **1. INV_Slots.cs** - Individual Inventory Slot
**Purpose:** Handles one slot's data, UI, click events, and drag/drop

**Key Fields:**
```csharp
public SlotType type;           // Empty, Item, or Weapon
public INV_ItemSO itemSO;       // Item data (if type = Item)
public W_SO weaponSO;           // Weapon data (if type = Weapon)
public int quantity;            // Stack count (items only)
public GameObject draggingIconPrefab;  // Drag visual
```

**Key Methods:**
- `UpdateUI()` - Refreshes icon and quantity text
- `OnPointerClick()` - Left-click = use/equip, Right-click = drop
- `OnBeginDrag()` - Creates floating icon
- `OnDrag()` - Moves icon with mouse
- `OnEndDrag()` - Destroys icon, restores slot
- `OnDrop()` - Swaps with dragged slot
- `SwapSlots()` - Exchanges all data between slots

#### **2. INV_Manager.cs** - Inventory Controller
**Purpose:** Manages all 9 slots, adding/removing items/weapons

**Key Methods:**
```csharp
AddItem(INV_ItemSO, int)        // Add item, stack if possible
AddWeapon(W_SO)                 // Add weapon to empty slot
EquipWeapon(INV_Slots)          // Swap weapon with player
UseItem(INV_Slots)              // Consume item effect
DropItem(INV_ItemSO, int)       // Spawn item in world
DropWeapon(W_SO)                // Spawn weapon in world
```

#### **3. P_Controller.cs** - Player Weapon Manager
**Purpose:** Tracks equipped weapons, fires events on weapon change

**Key Fields:**
```csharp
public W_Base meleeWeapon;      // Equipped melee weapon component
public W_Base rangedWeapon;     // Equipped ranged weapon component
W_SO currentMeleeData;          // Current melee weapon data
W_SO currentRangedData;         // Current ranged weapon data
```

**Events:**
```csharp
OnMeleeWeaponChanged(W_SO)      // Fired when melee weapon changes
OnRangedWeaponChanged(W_SO)     // Fired when ranged weapon changes
```

#### **4. INV_Loot.cs** - Pickup System
**Purpose:** Unified loot for both items and weapons

**Key Fields:**
```csharp
public LootType lootType;       // Item or Weapon
public INV_ItemSO itemSO;       // If lootType = Item
public W_SO weaponSO;           // If lootType = Weapon
```

**Flow:**
1. Player enters trigger
2. Check `lootType`
3. If Item: Fire `OnItemLooted` event → subscribers add to inventory
4. If Weapon: Call `INV_Manager.Instance.AddWeapon()` directly

#### **5. UI_WeaponDisplay.cs** - Weapon UI
**Purpose:** Shows currently equipped weapon (melee or ranged)

**Key Fields:**
```csharp
public WeaponSlot weaponSlot;   // Melee or Ranged
public Image weaponImage;       // UI icon
```

**Flow:**
1. Subscribe to appropriate event in `OnEnable()`
2. When event fires, call `UpdateDisplay()`
3. Set `weaponImage.sprite` to weapon's icon

---

## Features Implemented

### 1. Unified Inventory (Items + Weapons)

**Before:**
- Items only, weapons handled separately
- Different systems for managing each

**After:**
- Single 9-slot inventory
- `SlotType` enum: Empty, Item, Weapon
- Smart type checking prevents conflicts

**Code:**
```csharp
public enum SlotType { Empty, Item, Weapon }

if (slot.type == SlotType.Empty)
{
    // Can add item or weapon
}
```

---

### 2. Drag and Drop System

**How It Works:**

#### **Visual Flow:**
```
Click + Hold on Slot → OnBeginDrag()
         ↓
  Create floating icon from prefab
  Set icon sprite = slot's sprite
  Fade original slot (alpha 0.5)
         ↓
Move mouse → OnDrag() (every frame)
         ↓
  Update icon position to mouse
         ↓
Release mouse → OnEndDrag()
         ↓
  Destroy floating icon
  Restore slot opacity (alpha 1.0)
         ↓
If released over another slot → OnDrop()
         ↓
  Swap all data between slots
  Update both UIs
```

#### **Unity Event System Magic:**

You implement **interfaces** (contracts):
```csharp
IBeginDragHandler → OnBeginDrag()
IDragHandler      → OnDrag()
IEndDragHandler   → OnEndDrag()
IDropHandler      → OnDrop()
```

Unity's EventSystem **automatically calls** these methods when you drag!

**You never call them yourself.** Unity detects:
- Mouse down + move = dragging
- Which GameObject was clicked (via raycasting)
- Calls your methods automatically

---

### 3. Weapon Swapping

**Old System:**
- Destroyed and instantiated new weapon GameObjects
- Performance heavy, caused GC spikes

**New System:**
- Only swaps **ScriptableObject references**
- Weapons are components on player, not separate GameObjects
- Changes sprite only

**Code Flow:**
```csharp
// Player left-clicks weapon in inventory
INV_Manager.EquipWeapon(slot)
    ↓
// Get weapon type (melee or ranged)
WeaponType weaponType = slot.weaponSO.type
    ↓
// Call player controller
P_Controller.EquipWeapon(slot.weaponSO)
    ↓
// Swap ScriptableObject reference
if (weaponType == Melee)
{
    W_SO oldWeapon = currentMeleeData;
    currentMeleeData = newWeaponSO;
    meleeWeapon.AssignWeaponSO(newWeaponSO);
    OnMeleeWeaponChanged?.Invoke(newWeaponSO);
    
    // Put old weapon back in inventory
    slot.weaponSO = oldWeapon;
}
    ↓
// UI automatically updates (event subscribers)
UI_WeaponDisplay receives event → UpdateDisplay()
```

---

### 4. Hotbar Input (Number Keys 1-9)

**Component:** `INV_HotbarInput.cs`

**Code:**
```csharp
void Update()
{
    if (Keyboard.current.digit1Key.wasPressedThisFrame)
        UseSlot(0);
    if (Keyboard.current.digit2Key.wasPressedThisFrame)
        UseSlot(1);
    // ... up to digit9
}

void UseSlot(int index)
{
    INV_Slots slot = INV_Manager.Instance.inv_Slots[index];
    
    if (slot.type == SlotType.Item)
        INV_Manager.Instance.UseItem(slot);
    else if (slot.type == SlotType.Weapon)
        INV_Manager.Instance.EquipWeapon(slot);
}
```

**Initialization:**
- Uses `Start()` instead of `Awake()`
- Ensures `INV_Manager.Instance` is initialized first
- Avoids null reference errors

---

### 5. Transparency During Drag

**Original Plan:** Use CanvasGroup on each slot
**Problem:** All slots under one parent CanvasGroup → creates 9 extra components

**Final Solution:** Direct Image alpha manipulation
```csharp
// OnBeginDrag - fade icon
Color color = itemImage.color;
itemImage.color = new Color(color.r, color.g, color.b, 0.5f);

// OnEndDrag - restore
Color color = itemImage.color;
itemImage.color = new Color(color.r, color.g, color.b, 1f);
```

**Benefits:**
- ✅ No extra components
- ✅ Works with existing InventoryCanvas CanvasGroup
- ✅ Only icon fades, not entire slot
- ✅ Simpler code

---

### 6. Unified Loot System

**One Prefab, Two Types:**

```
Loot Prefab (GameObject)
├── INV_Loot component
│   ├── lootType: [Item OR Weapon]
│   ├── itemSO: [if Item]
│   ├── weaponSO: [if Weapon]
│   └── quantity: [if Item]
├── SpriteRenderer (child)
├── Animator
└── CircleCollider2D (trigger)
```

**Inspector Setup Examples:**

**Item Loot:**
```
INV_Loot Component:
┌─────────────────────────┐
│ Loot Type: [Item ▼]     │
│ Item SO: [Potion]       │
│ Weapon SO: [None]       │
│ Quantity: [3]           │
│ Can Be Picked Up: ✓    │
└─────────────────────────┘
```

**Weapon Loot:**
```
INV_Loot Component:
┌─────────────────────────┐
│ Loot Type: [Weapon ▼]   │
│ Item SO: [None]         │
│ Weapon SO: [Sword]      │
│ Quantity: [0]           │
│ Can Be Picked Up: ✓    │
└─────────────────────────┘
```

**Why Unified?**
- ✅ One prefab handles everything
- ✅ Less clutter in hierarchy
- ✅ Easier to maintain
- ✅ Consistent pickup behavior

---

## Code Structure

### Variable Naming Conventions

**Clear Naming:**
```csharp
// OLD (confusing)
dragIcon         // Is this the prefab or the instance?
dragIconPrefab   // Too similar!

// NEW (clear)
draggingIconPrefab   // The prefab template in Inspector
currentDragIcon      // The runtime instance while dragging
```

### Alignment & Readability

**Before:**
```csharp
SlotType tempType = type;
INV_ItemSO tempItemSO = itemSO;
W_SO tempWeaponSO = weaponSO;
int tempQuantity = quantity;
```

**After:**
```csharp
SlotType    tempType     = type;
INV_ItemSO  tempItemSO   = itemSO;
W_SO        tempWeaponSO = weaponSO;
int         tempQuantity = quantity;
```

### Code Cleanup Done

**Removed Bloat:**
- ❌ CanvasGroup on each slot (9 extra components)
- ❌ Unnecessary rectTransform references
- ❌ Quantity text support in drag icon
- ❌ Verbose XML comments
- ❌ Runtime icon creation fallback

**Added Clarity:**
- ✅ Clear section headers
- ✅ Aligned formatting
- ✅ Educational inline comments
- ✅ Better variable names
- ✅ Organized imports

---

## Unity Setup Guide

### 1. Create Drag Icon Prefab

**Steps:**
1. Right-click Hierarchy → UI → Image
2. Name: `DraggingIconPrefab`
3. Configure:
   - **Image Component:**
     - Source Image: None
     - Color: White
     - Raycast Target: **UNCHECKED** ✅
   - **Rect Transform:**
     - Width: 64
     - Height: 64
4. Drag to Project folder → Creates prefab
5. Delete from Hierarchy

### 2. Assign to Inventory Slots

1. Select all 9 inventory slots
2. Inspector → **"Dragging Icon Prefab"** field
3. Drag prefab there
4. Done!

### 3. Required Components on Slots

Each slot needs:
- ✅ INV_Slots script
- ✅ Image component (even if transparent) with **Raycast Target checked**
- ✅ Child "Icon" (Image)
- ✅ Child "QuantityText" (TextMeshProUGUI)

### 4. Required Components on Canvas

- ✅ Graphic Raycaster
- ✅ Canvas (Screen Space Overlay or Camera)

### 5. Required in Scene

- ✅ EventSystem GameObject
- ✅ Input System UI Input Module (NOT Standalone Input Module)

---

## How It Works

### OnDrag() - Follows Mouse Every Frame

**What it does:**
```csharp
public void OnDrag(PointerEventData eventData)
{
    if (currentDragIcon != null)
    {
        currentDragIcon.transform.position = eventData.position;
    }
}
```

**Explanation:**
- Unity calls this **every frame** while you're dragging
- `eventData.position` = current mouse position (Unity provides this)
- We set the drag icon's position to match
- Result: Icon smoothly follows your cursor

**Visual:**
```
Frame 1: Mouse at (100, 200) → Icon moves to (100, 200)
Frame 2: Mouse at (105, 202) → Icon moves to (105, 202)
Frame 3: Mouse at (110, 204) → Icon moves to (110, 204)
... continues every frame until you release
```

---

### OnEndDrag() - Cleanup When Released

**What it does:**
```csharp
public void OnEndDrag(PointerEventData eventData)
{
    // Destroy the floating icon
    if (currentDragIcon != null)
    {
        Destroy(currentDragIcon);
    }
    
    // Restore original slot icon to full opacity
    Color originalColor = itemImage.color;
    itemImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
}
```

**Explanation:**
- Unity calls this **once** when you release the mouse button
- `Destroy(currentDragIcon)` removes the floating icon from scene
- Restore `itemImage.color.a` to 1.0 (full opacity)
- Slot returns to normal appearance

**Why both are needed:**
- `OnDrag()` = Movement (continuous, every frame)
- `OnEndDrag()` = Cleanup (one-time, when finished)

---

## Troubleshooting

### Drag doesn't start
- **Check:** Prefab assigned in Inspector?
- **Check:** Slot's Image has "Raycast Target" enabled?
- **Check:** Canvas has Graphic Raycaster?

### Icon doesn't follow mouse
- **Check:** Canvas render mode (should be Screen Space)
- **Check:** `OnDrag()` method is being called (add Debug.Log)

### Can't drop on other slots
- **Check:** Target slots have Image with Raycast Target
- **Check:** Slots have INV_Slots script
- **Check:** EventSystem exists in scene

### Slots don't swap
- **Check:** Console for errors
- **Check:** Both slots call `UpdateUI()`

### Icon doesn't fade during drag
- **Check:** `itemImage.color.a` is being set to 0.5f
- **Check:** Image is not disabled

### Items overwrite weapons (or vice versa)
- **Fixed:** Added type checking in `INV_Manager.AddItem()`
- Now checks `slot.type == SlotType.Empty` before adding

### INV_Manager.Instance is null
- **Fixed:** Changed `INV_HotbarInput` from `Awake()` to `Start()`
- Ensures `INV_Manager` initializes first

---

## Implementation Timeline

### **Monday, October 14, 2025**
**Phase 1: Unified Inventory System**
- ✅ Added `SlotType` enum to `INV_Slots.cs`
- ✅ Added weapon support to inventory slots
- ✅ Created `EquipWeapon()` in `INV_Manager.cs`
- ✅ Modified `P_Controller.cs` for weapon swapping
- ✅ Created `UI_WeaponDisplay.cs` for dual weapon UI

**Phase 2: Bug Fixes**
- ✅ Fixed `INV_Manager.Instance` null error
- ✅ Fixed items overwriting weapons in inventory

**Phase 3: Loot System**
- ✅ Created unified `INV_Loot.cs` with `LootType` enum
- ✅ Added weapon pickup support
- ✅ Tested item and weapon pickups

**Phase 4: Code Cleanup**
- ✅ Removed verbose comments from `INV_Manager.cs`
- ✅ Simplified code structure
- ✅ Consolidated UI components

### **Tuesday, October 15, 2025**
**Phase 5: Tooltip Attempt (Abandoned)**
- ❌ Attempted hover tooltip system
- ❌ Hit canvas alpha issues with SHOP parent
- ✅ Reverted all tooltip code

**Phase 6: Drag & Drop**
- ✅ Implemented full drag/drop with Unity interfaces
- ✅ Added prefab support for drag icon
- ✅ Removed CanvasGroup bloat
- ✅ Switched to Image.color.alpha transparency

**Phase 7: Final Code Cleanup**
- ✅ Reorganized code sections
- ✅ Renamed variables for clarity
- ✅ Added educational comments
- ✅ Aligned formatting
- ✅ Removed unused code

**Phase 8: Documentation**
- ✅ Created comprehensive documentation
- ✅ Week 9 folder organization

---

## Summary

### Files Modified
1. ✅ `INV_Slots.cs` - Drag/drop, weapon support, cleaned up
2. ✅ `INV_Manager.cs` - AddWeapon, EquipWeapon, DropWeapon
3. ✅ `P_Controller.cs` - Weapon events, SO swapping
4. ✅ `W_SO.cs` - Added `image` field
5. ✅ `INV_ItemSO.cs` - Added `unlocksSkill` flag
6. ✅ `INV_Loot.cs` - LootType enum, weapon support
7. ✅ `INV_HotbarInput.cs` - Number key handling
8. ✅ `UI_WeaponDisplay.cs` - Created (unified component)

### Files Created
1. ✅ `INV_HotbarInput.cs` - Hotbar input handler
2. ✅ `UI_WeaponDisplay.cs` - Unified weapon UI display

### Key Achievements
- ✅ 100% working drag and drop
- ✅ Items and weapons unified in single inventory
- ✅ No performance issues (ScriptableObject swapping)
- ✅ Clean, maintainable code
- ✅ Beginner-friendly with comments
- ✅ Event-driven UI updates (no Update() polling)
- ✅ Zero extra components (removed CanvasGroup bloat)

---

**Status:** ✅ Complete & Production Ready  
**Week:** 9 (Oct 14-18, 2025)  
**Total Work Days:** 2 days (Mon-Tue)
