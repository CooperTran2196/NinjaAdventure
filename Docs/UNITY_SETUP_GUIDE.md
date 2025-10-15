# Unity Editor Setup - Quick Guide
**For:** Item Info Popup System  
**Time:** ~5 minutes

---

## ✅ Code Status
All C# code is complete and compiles with no errors:
- `SYS_GameManager.cs` - Added itemInfoPopup field
- `INV_ItemInfo.cs` - Added Show(W_SO) + BuildWeaponStatLines()
- `INV_Slots.cs` - Added hover events with 0.5s delay on first hover

**New Feature:**
- First hover has configurable delay (default: 0.5s) to prevent instant popup spam
- After first hover, all subsequent hovers show instantly for smooth UX
- Delay can be adjusted in Inspector: `Hover Delay` field on each slot

---

## 🔧 What You Need to Do in Unity

### Step 1: Extract InfoPopup from ShopCanvas (3 min)

1. **Open** `ShopCanvas.prefab`
2. **Find InfoPopup** (child of ShopPanel)
3. **Create parent Canvas:**
   - Right-click Hierarchy → Create Empty → Rename to `InfoPopupCanvas`
   - Drag **InfoPopup** into InfoPopupCanvas (make it child)
   - Drag **InfoPopupCanvas** to root level (out of ShopPanel)
4. **Configure InfoPopupCanvas:**
   - Add Component → **Canvas**
     - Render Mode: Screen Space - Overlay
     - Sorting Order: **100**
   - Add Component → **Canvas Scaler**
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 x 1080
   - Add Component → **Graphic Raycaster**
5. **Save as new prefab:**
   - Drag **InfoPopupCanvas** from Hierarchy to:
     `Assets/GAME/Scripts/System/PERSISTENT PREFRABS/`
   - Creates: `InfoPopupCanvas.prefab`
6. **Clean up ShopCanvas:**
   - Delete InfoPopup from ShopPanel (it's now separate)
   - Save ShopCanvas.prefab

### Step 2: Wire to GameManager (2 min)

1. **Open Level1 scene** (or any scene with GameManager)
2. **Add to scene:**
   - Drag `InfoPopupCanvas.prefab` into Hierarchy
3. **Select GameManager:**
   - Find **Persistent Objects** array → Increase size by 1
   - Drag **InfoPopupCanvas** into new array slot
4. **Wire popup reference:**
   - Find **Item Info Popup** field (under References)
   - Expand InfoPopupCanvas → InfoPopup in Hierarchy
   - Drag **InfoPopup** GameObject into field
5. **Save scene**

---

## ✅ Validation

After setup, you should see:
- GameManager Inspector:
  - ✅ Persistent Objects contains: `InfoPopupCanvas`
  - ✅ Item Info Popup shows: `InfoPopup (INV_ItemInfo)`
- Hierarchy:
  - ✅ InfoPopupCanvas exists (DontDestroyOnLoad object)

---

## 🧪 Test It

### Test 1: First Hover Delay
1. **Play scene** and **open inventory** (press I)
2. **Hover over first item slot**
3. ✅ Should wait 0.5s before popup appears

### Test 2: Moving Between Slots (No Reset)
1. Popup visible on Slot 1
2. **Move mouse to Slot 2** (don't leave inventory area)
3. ✅ Popup appears **INSTANTLY** on Slot 2
4. **Move to Slot 3**
5. ✅ Popup appears **INSTANTLY** on Slot 3
6. ✅ No delay as long as you stay inside inventory slots

### Test 3: Exit and Re-enter (Reset Timer)
1. Popup visible on any slot
2. **Move mouse completely outside inventory** (hover empty space)
3. **Hover item slot again**
4. ✅ Should wait 0.5s again (timer was reset)

### Test 4: Quick Pass (No Spam)
1. During gameplay, **accidentally hover inventory slot**
2. **Move away within 0.3s** (before delay completes)
3. ✅ No popup should appear (cancelled)

### Test 5: Drag Interaction
1. Hover Slot 1 (popup showing)
2. **Start dragging item**
3. ✅ Popup hides immediately
4. ✅ System resets (next hover will have delay)

**Adjust Delay (Optional):**
- Select any inventory slot in Inspector
- Find `Hover Delay` field (default: 0.5)
- Change to desired seconds (e.g., 0.3 for faster, 1.0 for slower)

---

## 📋 Full Documentation

See `Docs/Week_10_Oct15/1_ItemInfoPopup_Implementation.md` for:
- Complete technical details
- Display format examples (items vs melee vs ranged weapons)
- Full testing checklist
- Troubleshooting guide
