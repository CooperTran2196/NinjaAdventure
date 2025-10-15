# NinjaAdventure - Inventory UI Hierarchy

## Complete UI Structure (from InventoryCanvas.prefab)

```
InventoryCanvas (Canvas)
├── GoldPanel (RectTransform)
│   ├── goldImage (Image)
│   └── goldText (TextMeshProUGUI)
│
└── InventoryPanel (Image - Background panel)
    ├── Slot_1 (INV_Slots component)
    ├── Slot_2 (INV_Slots component)
    ├── Slot_3 (INV_Slots component)
    ├── Slot_4 (INV_Slots component)
    ├── Slot_5 (INV_Slots component)
    ├── Slot_6 (INV_Slots component)
    ├── Slot_7 (INV_Slots component)
    ├── Slot_8 (INV_Slots component)
    └── Slot_9 (INV_Slots component)
```

## Slot Structure (Each INV_Slots GameObject has)

Based on `INV_Slots.cs` code analysis:

```
Slot_X (GameObject with INV_Slots script)
├── Icon (Image) - itemImage reference
└── QuantityText (TMP_Text) - amountText reference
```

## Key Components

### InventoryCanvas
- **Component**: Canvas
- **Render Mode**: Screen Space - Overlay (typical for UI)
- **Purpose**: Root container for entire inventory UI

### GoldPanel
- **Position**: Anchored to top-right (1,1)
- **Size**: 100x100
- **Purpose**: Display player's gold/currency
- **Children**:
  - `goldImage`: Visual coin icon
  - `goldText`: Displays gold amount

### InventoryPanel
- **Position**: Bottom-center of screen
- **Size**: 610x80 (wide horizontal bar)
- **Purpose**: Container for 9 inventory slots
- **Component**: Image (visual background)
- **Children**: 9 slots arranged horizontally

### INV_Slots Script (on each slot)
- **Type Enum**: Empty | Item | Weapon
- **Data Fields**:
  - `itemSO`: Reference to INV_ItemSO
  - `weaponSO`: Reference to W_SO
  - `quantity`: Stack count
- **UI References**:
  - `itemImage`: Icon display
  - `amountText`: Quantity text
  - `draggingIconPrefab`: Visual during drag operation
- **Interfaces**: IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler

## Info Popup Requirements

For implementing an item/weapon info popup on hover, you'll likely need:

1. **Popup GameObject** (new - to be created)
   ```
   ItemInfoPopup (Panel)
   ├── TitleText (TMP_Text) - Item/Weapon name
   ├── IconImage (Image) - Larger icon
   ├── DescriptionText (TMP_Text) - Item description
   ├── TypeText (TMP_Text) - "Item" or "Weapon" type
   └── StatsPanel (optional - for weapons/stat effects)
       ├── StatLine_1 (TMP_Text)
       ├── StatLine_2 (TMP_Text)
       └── ...
   ```

2. **Positioning**: Should follow mouse or anchor near hovered slot

3. **Show/Hide Logic**: 
   - Show on mouse enter slot (OnPointerEnter)
   - Hide on mouse exit slot (OnPointerExit)
   - Hide on inventory close

4. **Data Display**:
   - From `INV_ItemSO.description`
   - From `W_SO.description`
   - List `P_StatEffect` details from items
   - Show weapon stats (AD, AP, knockback, etc.)

## Current Testing Branch Features

Based on `INV_Slots.cs`:
- ✅ Drag & drop between slots
- ✅ Left-click to use/consume items
- ✅ Right-click to drop items (when shop closed)
- ✅ Right-click to sell items (when shop open)
- ✅ Visual dragging icon follows mouse
- ✅ Swap slots functionality
- ❌ Hover info popup (NOT YET IMPLEMENTED)

## File Locations

- Prefab: `Assets/GAME/Scripts/System/PERSISTENT PREFRABS/InventoryCanvas.prefab`
- Slot Script: `Assets/GAME/Scripts/Inventory/INV_Slots.cs`
- Manager: `Assets/GAME/Scripts/Inventory/INV_Manager.cs`
- Item Data: `Assets/GAME/Scripts/Inventory/INV_ItemSO.cs`
- Weapon Data: `Assets/GAME/Scripts/Weapon/W_SO.cs`
