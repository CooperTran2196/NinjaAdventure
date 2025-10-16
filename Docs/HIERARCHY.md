# NinjaAdventure - Complete Game Hierarchy

**Last Updated:** October 15, 2025  
**Scene:** Level1.unity (Main Game Scene)  
**Total GameObjects:** 36 | **Root Objects:** 9

> **Note:** This documents Level1.unity only. Other scenes (SampleScene, Scene, Level1_Young Lord Room) will be added later as they become active.

---

## 📋 Table of Contents

- [Scene Root Objects](#-scene-root-objects)
- [Player System](#-player-system)
- [Enemy System](#-enemy-system)
- [UI System](#-ui-system)
- [Camera System](#-camera-system)
- [Audio System](#-audio-system)
- [Managers](#-managers)
- [Prefab Hierarchies](#-prefab-hierarchies)

---

## 🎮 Scene Root Objects

**Legend:** ✓ = Active, ✗ = Inactive

```
Level1
├── ✗ ----Legacy----
│   ├── ✗ Samurai bow
│   │   ├── ✓ W_Melee
│   │   └── ✓ W_Ranged
│   ├── ✗ Samurai
│   │   ├── ✓ W_Melee
│   │   └── ✓ W_Ranged
│   └── ✓ Old UI
│
├── ✓ Music Boss
│
├── ✓ ====System====
│
├── ✓ ====Enemies====
│
├── ✗ Testing Location trigger
│
├── ✓ ==PERSISTENT OBJECTS==
│
├── ✓ ----Items----
│
├── ✓ Music
│
└── ✓ Teleporter
    └── ✓ Spawn Point
```

**Root Object Summary:**
- **----Legacy----** (✗ Inactive) - Old player/enemy prefabs (deprecated)
- **Music Boss** (✓ Active) - Boss battle music trigger
- **====System====** (✓ Active) - System managers container
- **====Enemies====** (✓ Active) - Enemy spawn container
- **Testing Location trigger** (✗ Inactive) - Debug trigger (unused)
- **==PERSISTENT OBJECTS==** (✓ Active) - GameManager and persistent UI
- **----Items----** (✓ Active) - Item spawn container
- **Music** (✓ Active) - Background music system
- **Teleporter** (✓ Active) - Scene transition with spawn point

---

## 🎯 Player System

### Player GameObject

**Path:** `Player` (in ==PERSISTENT OBJECTS== container)  
**Purpose:** Main player character with controller/state architecture

> **Note:** Player details remain the same across all scenes. See previous hierarchy sections for full player component breakdown.

**Key Components on Player:**
- `P_Controller.cs` - State machine controller
- `P_State_Movement.cs` - Movement state
- `P_State_Attack.cs` - Attack/combo state
- `P_State_Dodge.cs` - Dodge state
- `C_Stats.cs` - Character stats (HP/MP/AD/AP)
- `C_Health.cs` - Health/damage system
- `C_Mana.cs` - Mana system
- `P_Combat.cs` - Weapon management
- `P_Exp.cs` - Experience/leveling
- `Rigidbody2D` - Physics
- `CapsuleCollider2D` - Collision
- `Animator` - Animation controller

---

## 👹 Enemy System

### Enemy GameObjects

**Path:** `====Enemies====` container  
**Purpose:** Enemy spawns and AI characters

> **Note:** Level1 scene uses the ==PERSISTENT OBJECTS== system. Specific enemy instances spawn dynamically or are placed in the ====Enemies==== container.

**Enemy Architecture (Generic):**
```
Enemy_Prefab
├── AttackPoint (Transform)
│   └── Purpose: Melee attack hit detection
│
└── DetectionPoint (Transform)
    └── Purpose: Player detection range center
```

**Key Components on Enemy:**
- `E_Controller.cs` - AI state machine controller
- `E_State_Idle.cs` - Idle state
- `E_State_Wander.cs` - Wandering patrol
- `E_State_Chase.cs` - Chase player
- `E_State_Attack.cs` - Attack execution
- `E_Detection.cs` - Player detection system
- `C_Stats.cs` - Character stats
- `C_Health.cs` - Health/damage system
- `E_Combat.cs` - Weapon management
- `Rigidbody2D` - Physics
- `CapsuleCollider2D` - Collision
- `Animator` - Animation controller

**Enemy State Flow:**
```
Idle → Wander → Chase (player detected) → Attack (in range) → Dead
```

---

## 🖼️ UI System

### Complete UI Structure

**Root:** `==PERSISTENT OBJECTS==` (Contains all persistent canvases)

> **Note:** All UI canvases are persistent across scenes via GameManager's `persistentObjects[]` array. The UI structure remains consistent throughout the game.

The UI system consists of multiple persistent canvases managed by `SYS_GameManager`:

---

### 1. PlayerCanvas (HUD)

**Purpose:** Main player HUD (health, mana, XP, weapons)

```
PlayerCanvas (Canvas - Screen Space Overlay)
├── HealthUI (HealthUI.cs)
│   ├── HealthSlider (Slider)
│   │   ├── Background (Image)
│   │   ├── Fill Area
│   │   │   └── Fill (Image - Red)
│   │   └── Handle Slide Area (optional)
│   └── HealthText (TMP_Text)
│
├── ManaUI (ManaUI.cs)
│   ├── ManaSlider (Slider)
│   │   ├── Background (Image)
│   │   ├── Fill Area
│   │   │   └── Fill (Image - Blue)
│   │   └── Handle Slide Area (optional)
│   └── ManaText (TMP_Text)
│
├── ExpUI (ExpUI.cs)
│   ├── ExpSlider (Slider)
│   │   ├── Background (Image)
│   │   ├── Fill Area
│   │   │   └── Fill (Image - Yellow/Green)
│   │   └── Handle Slide Area (optional)
│   ├── LevelText (TMP_Text) - "Level X"
│   └── ExpText (TMP_Text) - "XP: current/max"
│
├── MeleeWeaponUI (WeaponUI.cs)
│   └── WeaponImage (Image) - Displays equipped melee weapon icon
│
└── RangedWeaponUI (WeaponUI.cs)
    └── WeaponImage (Image) - Displays equipped ranged weapon icon
```

**Key Components:**
- `HealthUI.cs` - Health bar display
- `ManaUI.cs` - Mana bar display
- `ExpUI.cs` - Experience bar + level display
- `WeaponUI.cs` - Weapon icon display (2 instances: melee + ranged)

**Event-Driven Updates:**
- Subscribes to `C_Health.OnDamaged/OnHealed`
- Subscribes to `C_Mana.OnManaChanged`
- Subscribes to `P_Exp.OnLevelUp/OnXPChanged`
- Subscribes to `P_Controller.OnMeleeWeaponChanged/OnRangedWeaponChanged`

---

### 2. InventoryCanvas

**Purpose:** 9-slot unified inventory (items + weapons) with drag/drop

```
InventoryCanvas (Canvas - Screen Space Overlay)
├── GoldPanel (RectTransform)
│   ├── GoldImage (Image) - Coin icon
│   └── GoldText (TMP_Text) - Gold amount
│
└── InventoryPanel (Image - Background)
    ├── Slot_1 (INV_Slots.cs)
    │   ├── Icon (Image) - Item/Weapon sprite
    │   └── QuantityText (TMP_Text) - Stack count
    │
    ├── Slot_2 (INV_Slots.cs)
    │   ├── Icon (Image)
    │   └── QuantityText (TMP_Text)
    │
    ├── Slot_3 (INV_Slots.cs)
    │   ├── Icon (Image)
    │   └── QuantityText (TMP_Text)
    │
    ├── Slot_4 (INV_Slots.cs)
    │   ├── Icon (Image)
    │   └── QuantityText (TMP_Text)
    │
    ├── Slot_5 (INV_Slots.cs)
    │   ├── Icon (Image)
    │   └── QuantityText (TMP_Text)
    │
    ├── Slot_6 (INV_Slots.cs)
    │   ├── Icon (Image)
    │   └── QuantityText (TMP_Text)
    │
    ├── Slot_7 (INV_Slots.cs)
    │   ├── Icon (Image)
    │   └── QuantityText (TMP_Text)
    │
    ├── Slot_8 (INV_Slots.cs)
    │   ├── Icon (Image)
    │   └── QuantityText (TMP_Text)
    │
    └── Slot_9 (INV_Slots.cs)
        ├── Icon (Image)
        └── QuantityText (TMP_Text)
```

**Key Components:**
- `INV_Manager.cs` - Manages all inventory operations
- `INV_Slots.cs` - Individual slot (9 instances)
  - Implements: IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
  - Slot Types: Empty | Item | Weapon
  - Hover delay: 1.0s (configurable)

**Inventory Features:**
- ✅ Drag & drop between slots
- ✅ Left-click: Use item / Equip weapon
- ✅ Right-click: Drop item/weapon (shop closed) OR Sell item (shop open)
- ✅ Hotbar keys 1-9: Quick use/equip
- ✅ Unified loot pickup (items + weapons)
- ✅ Hover info popup (1s delay)
- ✅ Aspect ratio preservation (weapons preserve, items fill)

---

### 3. ShopCanvas

**Purpose:** Buy/sell items interface

```
ShopCanvas (Canvas - Screen Space Overlay)
├── ShopPanel (Image - Background)
│   ├── BuyPanel (RectTransform)
│   │   ├── ShopSlot_1 (SHOP_Slot component)
│   │   ├── ShopSlot_2 (SHOP_Slot component)
│   │   ├── ... (multiple shop slots)
│   │   └── ShopSlot_N (SHOP_Slot component)
│   │
│   └── SellPanel (RectTransform)
│       └── (References player inventory slots)
│
└── InfoPopup (INV_ItemInfo.cs) ⚠️ TO BE EXTRACTED
    ├── Name (TMP_Text) - Item/Weapon name
    ├── UniquePanel (Panel)
    │   └── Unique (TMP_Text) - "UNIQUE" label
    └── StatsPanel (Panel)
        └── Description (TMP_Text) - Item/Weapon stats
```

**Key Components:**
- `SHOP_Manager.cs` - Shop controller
- `SHOP_Keeper.cs` - Shop interaction trigger
- `INV_ItemInfo.cs` - Info popup display (shows items + weapons)

**Info Popup Features:**
- ✅ Shows item description + stat effects
- ✅ Shows weapon stats (melee: 7 lines, ranged: 3 lines)
- ⚠️ Currently child of ShopCanvas (to be extracted to shared popup)

---

### 4. StatsCanvas

**Purpose:** Detailed player stats screen (toggle with Tab key)

```
StatsCanvas (CanvasGroup - Fades in/out)
├── StatsPanel (Image - Background)
│   ├── TitleText (TMP_Text) - "Player Stats"
│   │
│   ├── CoreStatsPanel (RectTransform)
│   │   ├── LevelText (TMP_Text) - "Level: X"
│   │   ├── HPText (TMP_Text) - "HP: current/max"
│   │   ├── MPText (TMP_Text) - "MP: current/max"
│   │   └── XPText (TMP_Text) - "XP: current/max"
│   │
│   └── CombatStatsPanel (RectTransform)
│       ├── ADText (TMP_Text) - "Attack Damage: X"
│       ├── APText (TMP_Text) - "Ability Power: X"
│       ├── ArmorText (TMP_Text) - "Armor: X"
│       ├── MagicResistText (TMP_Text) - "Magic Resist: X"
│       ├── MoveSpeedText (TMP_Text) - "Move Speed: X"
│       └── ... (other stats)
│
└── CloseButton (Button) - Close stats screen
```

**Key Components:**
- `StatsUI.cs` - Stats display controller
- Updates from `P_StatsManager.OnStatsChanged` event

---

### 5. InfoPopupCanvas

**Purpose:** Shared tooltip popup for inventory hover

**Status:** ✅ Implemented (Week 9, System #10)

```
InfoPopupCanvas (Canvas - Screen Space Overlay)
└── InfoPopup (INV_ItemInfo.cs)
    ├── Name (TMP_Text) - Item/Weapon name
    ├── UniquePanel (Panel)
    │   └── Unique (TMP_Text) - "UNIQUE" label
    └── StatsPanel (Panel)
        └── Description (TMP_Text) - Multi-line stats
```

**Key Features:**
- **Shared Instance:** Referenced by both inventory and shop
- **Hover Delays:** Inventory: 1.0s (configurable), Shop: 0s (instant)
- **Dual Display:** Shows items (description + effects) and weapons (combat stats)
- **GameManager Integration:** Accessed via `SYS_GameManager.Instance.itemInfoPopup`

**Implementation Details:**
- Coroutine-based hover delay in `INV_Slots.cs`
- Polymorphic `Show()` methods for items and weapons
- Weapon formatting: Melee (7 stat lines), Ranged (3 stat lines)
- Cancels on drag, hides on exit

**Reference:** `Docs/Week_9_Oct14-18/10_ITEM_INFO_POPUP.md`

---

### 6. DialogCanvas

**Purpose:** NPC dialog system

```
DialogCanvas (Canvas - Screen Space Overlay)
├── DialogPanel (Image - Background)
│   ├── NPCNameText (TMP_Text) - Speaker name
│   ├── DialogText (TMP_Text) - Dialog content
│   └── ContinueButton (Button) - Next/Close
│
└── ChoicesPanel (RectTransform)
    ├── Choice_1 (Button)
    ├── Choice_2 (Button)
    └── Choice_3 (Button)
```

**Key Components:**
- `D_Manager.cs` - Dialog controller
- `D_SO.cs` - Dialog data ScriptableObject

---

## 📷 Camera System

### Camera Setup

**Note:** Camera setup is consistent across all scenes.

**Main Camera**
- **Component:** Camera (Main rendering camera)
- **Tag:** "MainCamera"
- **Follows:** Player character

**Cinemachine (if used)**
- **Component:** CinemachineCamera
- **Follow Target:** Player
- **Look At Target:** Player
- **Settings:** Dampening, dead zones for smooth following

---

## 🔊 Audio System

### Music System

**Path:** `Music` + `Music Boss` root objects  
**Purpose:** Background music and boss battle triggers

```
Music (Background Music)
└── Audio source for normal gameplay

Music Boss (Boss Battle Music)
└── Trigger for boss encounter music
```

**Key Components:**
- Audio sources for different music tracks
- Trigger-based music switching
- Area-specific music zones

---

## 🎛️ Managers

### System Managers

**Path:** `====System====` container  
**Purpose:** Core game systems and managers

**Key Managers:**

**SYS_GameManager** (Singleton)
- Location: `==PERSISTENT OBJECTS==`
- Purpose: Central game manager
- Fields:
  - `persistentObjects[]` - DontDestroyOnLoad array
  - `itemInfoPopup` - Shared tooltip reference
  - `d_Manager` - Dialog manager
  - `shop_Manager` - Shop manager
  - `sys_Fader` - Scene transition fader
- Methods:
  - `Awake()` - Initialize persistent objects
  - `CleanUpAndDestroy()` - Scene cleanup

**P_StatsManager**
- Location: `====System====` or attached to Player
- Purpose: Player stat effects and modifiers
- Features:
  - Applies item effects
  - Applies skill effects
  - Recalculates final stats
  - Fires `OnStatsChanged` event

**Stat Effect System:**
```
P_StatEffect
├── statType (Enum: HP, MP, AD, AP, Armor, etc.)
├── value (int/float)
├── operation (Set, Add, Multiply)
└── duration (0=permanent, 1=instant, >1=timed)
```

---

## 🎁 Prefab Hierarchies

### Persistent Prefabs

**Location:** `Assets/GAME/Scripts/System/PERSISTENT PREFABS/`

**Managed by:** `SYS_GameManager.persistentObjects[]` array

```
Persistent Prefabs (DontDestroyOnLoad):
├── InventoryCanvas.prefab
├── PlayerCanvas.prefab
├── ShopCanvas.prefab
├── StatsCanvas.prefab
├── DialogCanvas.prefab
└── InfoPopupCanvas.prefab ✅ Complete (Week 9)
```

**Persistence Pattern:**
- All prefabs added to `persistentObjects[]` in GameManager
- `DontDestroyOnLoad()` called on all objects in array
- Centralized cleanup in `SYS_GameManager.CleanUpAndDestroy()`

---

### Loot Prefabs

**Location:** `Assets/GAME/Prefabs/Loot/`

```
Loot Prefabs:
├── Loot_Item.prefab
│   ├── SpriteRenderer (Item image)
│   ├── CircleCollider2D (Trigger)
│   └── INV_Loot.cs (Pickup logic)
│
└── Loot_Weapon.prefab
    ├── SpriteRenderer (Weapon image)
    ├── CircleCollider2D (Trigger)
    └── INV_Loot.cs (Pickup logic)
```

**Unified Pickup System:**
- Both use same `INV_Loot.cs` script
- Spawned by `INV_Manager.DropItem()` / `DropWeapon()`
- Auto-add to inventory on trigger enter

---

### Weapon Prefabs

**Location:** `Assets/GAME/Prefabs/Weapons/`

**Weapon Hierarchy (Example: Melee):**
```
W_Melee.prefab
├── SpriteRenderer (Weapon visual)
├── PolygonCollider2D (Hit detection)
├── W_Melee.cs (Weapon logic)
└── W_SO (ScriptableObject reference)
```

**Sprite Configuration:**
- Pivot: BOTTOM (0.5, 0) - Required for combo system
- Anchoring: Parents to player during attacks
- Rotation: Controlled by combo attack patterns

**Weapon Components:**
- `W_Base.cs` - Base weapon class
- `W_Melee.cs` - Melee weapon implementation
- `W_Ranged.cs` - Ranged weapon implementation
- `W_SO.cs` - Weapon data ScriptableObject

---

### Enemy Prefabs

**Location:** `Assets/GAME/Prefabs/Enemies/`

**Enemy Hierarchy (Example: Samurai):**
```
Samurai.prefab
├── AttackPoint (Transform)
├── DetectionPoint (Transform)
├── SpriteRenderer (Enemy sprite)
├── Animator (Animation controller)
├── Rigidbody2D (Physics)
├── CapsuleCollider2D (Collision)
├── E_Controller.cs (AI state machine)
├── E_State_*.cs (State components)
├── C_Stats.cs (Stats)
└── C_Health.cs (Health)
```

---

## 🔗 System Connections

### Player → UI Flow

```
Player Action → Component Event → UI Update

Examples:
1. Take Damage:
   C_Health.ApplyDamage() 
   → OnDamaged event 
   → HealthUI.HandleHealthChanged() 
   → Update slider

2. Gain XP:
   P_Exp.AddXP() 
   → OnXPChanged event 
   → ExpUI.HandleXPChanged() 
   → Update XP bar

3. Equip Weapon:
   P_Combat.EquipWeapon() 
   → P_Controller.OnMeleeWeaponChanged event 
   → WeaponUI.UpdateDisplay() 
   → Show weapon icon

4. Use Item:
   INV_Manager.UseItem() 
   → P_StatsManager.ApplyModifier() 
   → OnStatsChanged event 
   → StatsUI.UpdateAllStats()
```

**Design Pattern:** Event-driven (no Update() polling)

---

### Inventory → Shop Integration

```
Shop Open:
SHOP_Keeper.OnTriggerEnter() 
→ SHOP_Keeper.OnShopStateChanged event 
→ INV_Slots.HandleShopStateChanged() 
→ Right-click changes to "Sell" mode

Shop Close:
SHOP_Keeper.OnTriggerExit() 
→ OnShopStateChanged(null, false) 
→ INV_Slots right-click back to "Drop" mode
```

---

### Hover Popup System (Current)

```
Inventory Slots:
OnPointerEnter() 
→ Wait 1.0s (coroutine) 
→ SYS_GameManager.Instance.itemInfoPopup.Show() 
→ Display item/weapon stats

Shop Slots:
OnPointerEnter() 
→ Show instantly (no delay) 
→ Use shop's local InfoPopup child
```

**Implementation:**
- Inventory: 1s hover delay (configurable)
- Shop: Instant popup (no delay)
- Shared popup component: `INV_ItemInfo.cs`

---

## 📊 Component Reference

### Key Script Locations

**Player:**
- `/Assets/GAME/Scripts/Player/` - Player-specific scripts
- `/Assets/GAME/Scripts/Character/` - Shared character scripts

**Enemy:**
- `/Assets/GAME/Scripts/Enemy/` - Enemy AI scripts

**Weapon:**
- `/Assets/GAME/Scripts/Weapon/` - Weapon system

**Inventory:**
- `/Assets/GAME/Scripts/Inventory/` - Inventory & loot

**UI:**
- `/Assets/GAME/Scripts/UI/` - All UI components

**System:**
- `/Assets/GAME/Scripts/System/` - Game managers

**Dialog:**
- `/Assets/GAME/Scripts/Dialog/` - Dialog system

**SkillTree:**
- `/Assets/GAME/Scripts/SkillTree/` - Skill system

---

## 🔄 State Machines

### Player State Machine (P_Controller.cs)

**States:**
```
P_State_Idle → Default state when no input
P_State_Movement → Walking/running
P_State_Attack → Combo attacks
P_State_Dodge → Dodge roll with i-frames
P_State_Dead → Death state
```

**State Priority (highest to lowest):**
1. Dead (overrides all)
2. Dodge (i-frames)
3. Attack (combo execution)
4. Movement (walking)
5. Idle (fallback)

**Concurrent States:**
- Movement + Attack can run together
- Attack controls upper body, Movement controls velocity

---

### Enemy State Machine (E_Controller.cs)

**States:**
```
E_State_Idle → Standing still
E_State_Wander → Random patrol movement
E_State_Chase → Follow player (in detection range)
E_State_Attack → Attack player (in attack range)
E_State_Dead → Death state
```

**State Transitions:**
```
Idle ⇄ Wander (random intervals)
   ↓
Chase (player enters detectionRange)
   ↓
Attack (player enters attackRange)
   ↓
Dead (health reaches 0)
```

---

## 📦 Data Structures

### ScriptableObjects

**Items:**
```csharp
INV_ItemSO
├── id (string)
├── itemName (string)
├── description (string)
├── image (Sprite)
├── isUnique (bool)
├── stackable (bool)
├── maxStack (int)
├── buyPrice (int)
├── sellPrice (int)
└── effects (List<P_StatEffect>)
```

**Weapons:**
```csharp
W_SO
├── id (string)
├── weaponName (string)
├── description (string)
├── image (Sprite)
├── weaponType (Melee/Ranged)
├── attackDamage (int)
├── abilityPower (int)
├── knockbackForce (float)
├── stunDuration (float)
├── attackSpeed (float)
├── manaCost (int) [Ranged only]
├── projectileSpeed (float) [Ranged only]
└── ... (combo-specific settings)
```

**Dialog:**
```csharp
D_SO
├── dialogName (string)
├── speakerName (string)
├── dialogLines (string[])
└── choices (D_Choice[])
```

**Skills:**
```csharp
ST_SkillSO
├── skillName (string)
├── description (string)
├── icon (Sprite)
├── unlockCost (int)
├── maxLevel (int)
├── effects (List<P_StatEffect>)
└── prerequisites (List<ST_SkillSO>)
```

---

## 🎯 Quick Reference

### Find GameObject by Path

```csharp
// Player
GameObject player = GameObject.Find("Player");

// Inventory slot
GameObject slot1 = GameObject.Find("InventoryCanvas/InventoryPanel/Slot_1");

// Health UI
GameObject healthUI = GameObject.Find("PlayerCanvas/HealthUI");

// Enemy
GameObject samurai = GameObject.Find("Samurai1");
```

### Find Component

```csharp
// Player controller
P_Controller pController = FindFirstObjectByType<P_Controller>();

// Inventory manager
INV_Manager invManager = FindFirstObjectByType<INV_Manager>();

// Game manager
SYS_GameManager gameManager = SYS_GameManager.Instance;
```

---

## 📝 Scene Notes

### Level1.unity (Current Main Scene)

**Purpose:** Main gameplay level  
**Status:** Active development scene  
**Key Features:**
- Persistent objects system via GameManager
- Enemy spawning in ====Enemies==== container
- Item drops in ----Items---- container
- Music system with boss triggers
- Teleporter for scene transitions
- Legacy objects (inactive) for reference

### Other Scenes (Not Yet Documented)

**Available Scenes:**
- `SampleScene.unity` - Test/sample scene
- `Scene.unity` - Legacy scene
- `Level1_Young Lord Room.unity` - Boss room scene

> **Note:** These scenes will be documented when they become part of active gameplay. HIERARCHY.md will be updated as new scenes are integrated.

---

**Last Updated:** October 15, 2025  
**Next Update:** When new scenes/systems added  
**Maintained By:** AI + User collaboration
