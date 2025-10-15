# Item Info Popup - Complete Implementation Plan
**Date:** October 15, 2025  
**Branch:** Testing-drag-and-drop-UI  
**Status:** ✅ CODE COMPLETE - Unity Setup Required  
**Week:** Week 9 (still in progress)

> **Note:** All code has been implemented. Follow Unity Editor Tasks below to complete setup.
> **Quick Unity Guide:** See `Docs/UNITY_SETUP_GUIDE.md` for 5-minute setup

---

## 🎯 Project Goals

1. **Reuse InfoP## ⚠️ Important Notes

1. **Don't rename INV_ItemInfo.cs** - Keep current name
2. **GUID in prefab** (4080615f50927db40a6ab67efef46189) must match script
3. **Sorting Order** critical - must be high enough to render over shop/inventory
4. **Persistent Objects Array** - Add InfoPopupCanvas to GameManager's `persistentObjects[]` array (GameManager handles DontDestroyOnLoad centrally)
5. **Singleton Access** - Slots get popup via `SYS_GameManager.Instance.itemInfoPopup`
6. **Hide on Drag** - Prevent popup showing while dragging items between slots
7. **Empty Slots** - Check `type != SlotType.Empty` before showing
8. **Hover Delay** - First hover has 0.5s delay (configurable), subsequent hovers show instantly
9. **Static Flag** - `hasShownPopupOnce` persists across all slots for smooth UXr both Shop and Inventory
2. **Keep INV_ItemInfo.cs** name (don't rename to UI_ItemInfoPanel)
3. **Extract InfoPopup** from ShopCanvas to make it independent
4. **Support both Items and Weapons** with different display formats
5. **Add to GameManager** persistent object list

---

## 📊 Current State Analysis

### ShopCanvas InfoPopup Structure (UPDATED):
```
ShopCanvas
└── ShopPanel
    └── InfoPopup (GameObject) ✅ EXISTS
        │   Size: 250 x 421.49
        │   Pivot: (0, 1) ← Top-left anchor
        │   Components:
        │   - Image (background panel)
        │   - CanvasGroup (Alpha: 1)
        │   - INV_ItemInfo script (GUID: 4080615f50927db40a6ab67efef46189)
        │       • canvasGroup: wired
        │       • rectTransform: wired
        │       • itemNameText: wired ✅ (fileID: 1215554433850977911)
        │       • itemDescText: wired ✅ (fileID: 6887775150327825536)
        │       • statLinePrefab: wired ✅
        │       • statContainer: wired ✅
        │       • offset: (50, -10)
        │
        ├── Name (TextMeshProUGUI) ✅ NEW!
        │       Referenced by: itemNameText
        │
        ├── Unique (Panel)
        │       Visual decoration panel
        │
        └── Stats (Panel) ← statContainer
            └── Description (TextMeshProUGUI) ✅ NEW!
                    Referenced by: itemDescText
                    (Stat lines spawn here as children)
```

### INV_ItemInfo.cs Current Capabilities:
```csharp
✅ Show(INV_ItemSO) - displays item info
✅ Hide() - hides popup  
✅ FollowMouse(Vector2) - position at cursor
✅ ClearStatLines() - removes old stats
✅ BuildItemStatLines() - formats stat effects
✅ itemNameText reference ← ALREADY HAS IT!
✅ itemDescText reference ← ALREADY HAS IT!
✅ statLinePrefab reference
✅ statContainer reference
❌ Show(W_SO) - MISSING weapon support
❌ BuildWeaponStatLines() - MISSING weapon formatting
```

---

## 🔧 Required Changes

### Phase 1: Extract InfoPopup from ShopCanvas

#### Unity Editor Steps:
1. **Open ShopCanvas.prefab** in Unity Editor
2. **Select InfoPopup** GameObject (child of ShopPanel)
3. **Drag InfoPopup** to the root level (make it a sibling of ShopPanel, not child)
4. **Create new Canvas** as parent of InfoPopup:
   ```
   InfoPopupCanvas (NEW)
   ├── Canvas (Render Mode: Screen Space - Overlay)
   ├── CanvasScaler (1920x1080)
   ├── GraphicRaycaster
   └── InfoPopup (moved here)
       ├── Name
       ├── Unique  
       └── Stats
           └── Description
   ```
5. **Set Sorting Order** on InfoPopupCanvas to HIGH (e.g., 100) so it appears above everything
6. **Save ShopCanvas.prefab**
7. **Create new prefab** "InfoPopupCanvas.prefab" in `Assets/GAME/Scripts/System/PERSISTENT PREFRABS/`
8. **Delete InfoPopup** from ShopCanvas (since it's now independent)

#### Why This Structure:
- ✅ InfoPopup needs its own Canvas to render on top of both Shop AND Inventory
- ✅ Independent GameObject = can be in GameManager persistent list
- ✅ High sorting order = always visible over other UI
- ✅ Both Shop and Inventory can reference the same instance

---

### Phase 2: Update GameManager

#### File: `SYS_GameManager.cs`

**Add field in References section:**
```csharp
[Header("References")]
public D_Manager         d_Manager;
public D_HistoryTracker  d_HistoryTracker;
public SYS_Fader         sys_Fader;
public SHOP_Manager      shop_Manager;
public INV_ItemInfo      itemInfoPopup;  // ← NEW: Shared info popup
```

**In Awake() (after existing FindFirstObjectByType calls):**
```csharp
itemInfoPopup ??= FindFirstObjectByType<INV_ItemInfo>();
if (!itemInfoPopup) Debug.LogWarning("SYS_GameManager: ItemInfoPopup is missing.");
if (itemInfoPopup) itemInfoPopup.Hide(); // Start hidden
```

**Note:** No DontDestroyOnLoad needed here - GameManager handles this via `persistentObjects[]` array

#### Unity Editor Steps:
1. **Open any scene** with SYS_GameManager
2. **Select GameManager** GameObject
3. **Drag InfoPopupCanvas** prefab into the scene (spawns instance)
4. **Drag InfoPopupCanvas instance** into GameManager's `Persistent Objects` array
5. **Drag InfoPopup's INV_ItemInfo component** into GameManager's `itemInfoPopup` field
6. **Save scene**

---

### Phase 3: Extend INV_ItemInfo.cs for Weapons

#### New Methods to Add:

```csharp
// Overload for weapons
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

// New method for weapon stat formatting
List<string> BuildWeaponStatLines(W_SO weaponSO)
{
    var outLines = new List<string>();
    
    // Format based on weapon type
    if (weaponSO.type == WeaponType.Melee)
    {
        // Line 1: AD and AP
        outLines.Add($"AD: {weaponSO.AD}  AP: {weaponSO.AP}");
        
        // Line 2: Knockback Force
        outLines.Add($"Knockback Force: {weaponSO.knockbackForce}");
        
        // Line 3: Thrust Distance
        outLines.Add($"Thrust Distance: {weaponSO.thrustDistance}");
        
        // Line 4: Slash Arc Degree
        outLines.Add($"Slash Arc Degree: {weaponSO.slashArcDegrees}");
        
        // Line 5: Speed (combo show times)
        string speedStr = string.Join(" - ", weaponSO.comboShowTimes);
        outLines.Add($"Speed: {speedStr}");
        
        // Line 6: Speed Penalties
        string penaltiesStr = string.Join(" - ", weaponSO.comboMovePenalties);
        outLines.Add($"Speed Penalties: {penaltiesStr}");
        
        // Line 7: Stun Time
        string stunStr = string.Join(" - ", weaponSO.comboStunTimes);
        outLines.Add($"Stun Time: {stunStr}");
    }
    else if (weaponSO.type == WeaponType.Ranged)
    {
        // Line 1: AD and AP
        outLines.Add($"AD: {weaponSO.AD}  AP: {weaponSO.AP}");
        
        // Line 2: Mana Cost
        outLines.Add($"Mana Cost: {weaponSO.manaCost}");
        
        // Line 3: Projectile Speed
        outLines.Add($"Projectile Speed: {weaponSO.projectileSpeed}");
    }
    
    return outLines;
}
```

#### Code Changes Summary:
1. ✅ Add `Show(W_SO weaponSO)` overload
2. ✅ Add `BuildWeaponStatLines(W_SO)` method
3. ✅ Keep existing `Show(INV_ItemSO)` and `BuildItemStatLines()` unchanged
4. ✅ No renaming needed - script stays as `INV_ItemInfo.cs`

---

### Phase 4: Update INV_Slots.cs (Inventory Hover)

#### Add Interfaces:
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

#### Add Fields:
```csharp
[Header("Info Popup Settings")]
public float hoverDelay = 0.5f; // Delay before showing popup on first hover

INV_ItemInfo itemInfoPopup;  // Reference to shared popup
static bool hasShownPopupOnce = false;  // Tracks if user has hovered at least once
Coroutine hoverCoroutine;  // Coroutine for delay
```

#### In Awake():
```csharp
// Get reference to shared popup from GameManager
itemInfoPopup = SYS_GameManager.Instance?.itemInfoPopup;
if (!itemInfoPopup) Debug.LogWarning($"INV_Slots ({name}): itemInfoPopup not found in GameManager.");
```

#### Implement Hover Methods:
```csharp
public void OnPointerEnter(PointerEventData eventData)
{
    if (type == SlotType.Empty || !itemInfoPopup) return;
    
    // If we've already shown popup once, show immediately
    if (hasShownPopupOnce)
    {
        ShowPopupImmediate(eventData.position);
    }
    else
    {
        // First time hovering - use delay
        hoverCoroutine = StartCoroutine(ShowPopupWithDelay(eventData.position));
    }
}

public void OnPointerExit(PointerEventData eventData)
{
    // Cancel pending hover coroutine if still waiting
    if (hoverCoroutine != null)
    {
        StopCoroutine(hoverCoroutine);
        hoverCoroutine = null;
    }
    
    if (itemInfoPopup)
    {
        itemInfoPopup.Hide();
    }
}

void ShowPopupImmediate(Vector2 mousePosition)
{
    itemInfoPopup.FollowMouse(mousePosition);
    
    if (type == SlotType.Item && itemSO)
    {
        itemInfoPopup.Show(itemSO);
    }
    else if (type == SlotType.Weapon && weaponSO)
    {
        itemInfoPopup.Show(weaponSO);
    }
}

System.Collections.IEnumerator ShowPopupWithDelay(Vector2 mousePosition)
{
    yield return new WaitForSeconds(hoverDelay);
    hasShownPopupOnce = true;  // Mark that we've shown popup once
    ShowPopupImmediate(mousePosition);
    hoverCoroutine = null;
}
```

#### Handle Drag Conflicts:
```csharp
public void OnBeginDrag(PointerEventData eventData)
{
    // Cancel pending hover coroutine if dragging starts
    if (hoverCoroutine != null)
    {
        StopCoroutine(hoverCoroutine);
        hoverCoroutine = null;
    }
    
    // Hide popup when starting drag
    if (itemInfoPopup) itemInfoPopup.Hide();
    
    // ... existing drag code ...
}
```

---

### Phase 5: Update SHOP (if needed)

#### File: `SHOP_Manager.cs` or shop item slot script

Same pattern as inventory:
```csharp
// In shop item slot's OnPointerEnter:
public void OnPointerEnter(PointerEventData eventData)
{
    if (!shopItemSO || !itemInfoPopup) return;
    
    itemInfoPopup.FollowMouse(eventData.position);
    itemInfoPopup.Show(shopItemSO);  // Show item from shop
}
```

---

## 📋 Step-by-Step Execution Checklist

### Unity Editor Tasks:
- [ ] 1. Open `ShopCanvas.prefab`
- [ ] 2. Move InfoPopup out of ShopPanel (make it root-level sibling)
- [ ] 3. Create new Canvas parent for InfoPopup
- [ ] 4. Configure Canvas (Screen Space Overlay, Sorting Order: 100)
- [ ] 5. Configure CanvasScaler (1920x1080)
- [ ] 6. Add GraphicRaycaster component
- [ ] 7. Save as new prefab: `InfoPopupCanvas.prefab`
- [ ] 8. Delete old InfoPopup from ShopCanvas
- [ ] 9. Open scene with GameManager
- [ ] 10. Add InfoPopupCanvas instance to scene
- [ ] 11. Add InfoPopupCanvas to GameManager's `Persistent Objects` array
- [ ] 12. Wire InfoPopup's INV_ItemInfo component to GameManager's `itemInfoPopup` field
- [ ] 13. Test that InfoPopup persists across scene loads

### Code Tasks:
- [x] 14. Add `itemInfoPopup` field to `SYS_GameManager.cs`
- [x] 15. Add FindFirstObjectByType + Hide logic in GameManager.Awake()
- [x] 16. Add `Show(W_SO)` method to `INV_ItemInfo.cs`
- [x] 17. Add `BuildWeaponStatLines()` to `INV_ItemInfo.cs`
- [x] 18. Add hover interfaces to `INV_Slots.cs`
- [x] 19. Implement `OnPointerEnter/Exit` in `INV_Slots.cs`
- [x] 20. Get popup reference in `INV_Slots.Awake()`
- [x] 21. Hide popup in `INV_Slots.OnBeginDrag()`
- [ ] 22. Update shop slot script (if separate from inventory)

### Testing Tasks:
- [ ] 23. Test inventory hover shows items correctly
- [ ] 24. Test inventory hover shows weapons correctly
- [ ] 25. Test weapon stat formatting (melee vs ranged)
- [ ] 26. Test popup hides on drag start
- [ ] 27. Test popup follows mouse
- [ ] 28. Test shop hover still works
- [ ] 29. Test popup appears above all other UI
- [ ] 30. Test empty slots don't show popup
- [ ] 31. **Test first hover delay (0.5s default)**
- [ ] 32. **Test subsequent hovers show instantly (no delay)**
- [ ] 33. **Test canceling delay by exiting slot before timer**
- [ ] 34. **Test canceling delay by starting drag before timer**

---

## 🎨 Display Format Reference

### For Items (Existing - Keep As Is):
```
[Item Name]
[Description]

Heals 50 HP
+10 Attack Damage
+5 Armor in (30s)
```

### For Melee Weapons (NEW):
```
[Weapon Name]
[Description]

AD: 15  AP: 0
Knockback Force: 5
Thrust Distance: 0.25
Slash Arc Degree: 45
Speed: 0.45 - 0.45 - 0.45
Speed Penalties: 0.6 - 0.5 - 0.3
Stun Time: 0.1 - 0.2 - 0.5
```

### For Ranged Weapons (NEW):
```
[Weapon Name]
[Description]

AD: 8  AP: 0
Mana Cost: 10
Projectile Speed: 15
```

---

## ⚠️ Important Notes

1. **Don't rename INV_ItemInfo.cs** - Keep current name
2. **GUID in prefab** (4080615f50927db40a6ab67efef46189) must match script
3. **Sorting Order** critical - must be high enough to render over shop/inventory
4. **Persistent Objects Array** - Add InfoPopupCanvas to GameManager's `persistentObjects[]` array (GameManager handles DontDestroyOnLoad centrally)
5. **Singleton Access** - Slots get popup via `SYS_GameManager.Instance.itemInfoPopup`
6. **Hide on Drag** - Prevent popup showing during drag operations
7. **Empty Slots** - Check `type != SlotType.Empty` before showing

---

## 🔍 W_SO Fields Reference

From `W_SO.cs`:
```csharp
// Common
public string id;
public string description;
public WeaponType type; // Melee, Ranged, Magic
public Sprite sprite;
public Sprite image;

// Damage
public int AD;
public int AP;

// Melee
public float knockbackForce;
public float thrustDistance;
public float slashArcDegrees;
public float[] comboShowTimes;     // Speed array
public float[] comboMovePenalties; // Speed Penalties array
public float[] comboStunTimes;     // Stun Time array

// Ranged
public int manaCost;
public float projectileSpeed;
```

---

## ✅ Success Criteria

- [ ] InfoPopup is independent GameObject with its own Canvas
- [ ] InfoPopup is in GameManager persistent list
- [ ] Inventory slots show popup on hover
- [ ] Shop items show popup on hover (reusing same popup)
- [ ] Items display with current format (unchanged)
- [ ] Melee weapons display with 7-line format
- [ ] Ranged weapons display with 3-line format
- [ ] Popup hides when drag starts
- [ ] Popup renders above all other UI
- [ ] No duplicate InfoPopup instances
- [ ] Script name stays `INV_ItemInfo.cs`

---

**Ready to proceed?** Review this plan and confirm before we start coding!
