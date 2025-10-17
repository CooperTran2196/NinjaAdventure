# Weather Particle System Setup Guide
**Created:** October 17, 2025  
**Status:** 🚧 Phase 1 - Testing First 3 Effects  
**Reference:** Based on ENV_Destructible particle values

---

## 🎯 Quick Reference: Your Existing Particle Values

From your destructible objects (Week 10):
- **Start Speed:** 20-30 (reduced from 100 for tighter scatter)
- **Start Size:** 0.3-0.6 (half of sprite size)
- **Start Lifetime:** 1.5s (enough time to see movement)
- **Simulation Space:** World (particles persist after GameObject destroys)
- **Alpha Fade:** Stay visible until 70%, then fade to 0% at end
- **Bursts:** Used for instant spawn (not Rate over Time)

---

## 🌥️ **Effect 1: Clouds** (~10 min)

### **Godot Reference Values:**
```
speed_scale: 0.1 (VERY slow)
emission_rect_extents: Vector2(150, 250)
amount: 6
initial_velocity: 100
damping: 5.0
lifetime: 25.0
animation_speed: 0.3
```

### **Unity Setup Steps:**

#### **Step 1: Create GameObject**
1. In Hierarchy: Right-click → Create Empty → Name: `PFX_Clouds`
2. Position: `(0, 0, 0)` (will follow camera later)

#### **Step 2: Add Particle System**
1. Select `PFX_Clouds` → Add Component → Particle System

#### **Step 3: Main Module** ⚙️
```
Duration: 5.0                        (system duration - looping anyway)
Looping: ✓ ON                       (continuous clouds)
Start Lifetime: 25.0                 (long lifetime for slow drift)
Start Speed: 10.0                    (converted from Godot velocity 100)
Start Size: Random Between 1.5 - 2.5 (cloud variety)
Start Rotation: Random 0° - 360°     (natural variation)
Gravity Modifier: 0                  (top-down, no gravity)
Simulation Space: World              (persist independently)
Play On Awake: ✓ ON                 (auto-play)
```

**Why these values:**
- **Start Speed 10:** Godot's velocity 100 with damping 5.0 ≈ Unity's 10 (slower drift)
- **Lifetime 25s:** Clouds drift slowly across screen
- **Size 1.5-2.5:** Visible but not overwhelming (match your sprite scale)

#### **Step 4: Emission Module** 📊
```
Rate over Time: 6                    (Godot amount: 6)
Bursts: NONE                        (continuous emission, not burst)
```

**Why Rate over Time:**
- Clouds appear gradually (not all at once like destructible burst)
- 6 clouds per second keeps screen populated

#### **Step 5: Shape Module** 🔷
```
Shape: Box
Scale: (15, 25, 0.01)               (Godot emission_rect_extents: 150x250 → Unity units)
Emit from: Volume                    (spawn throughout box)
```

**Why Box shape:**
- Clouds spawn across wide area (not single point like pot burst)
- X: 15 = horizontal spread
- Y: 25 = vertical spread above/below

#### **Step 6: Velocity over Lifetime Module** ⚡
```
☑ Enable module
Linear: (1.0, 0, 0)                 (drift right at 1 unit/sec)
```

**Why this matters:**
- Clouds drift horizontally (like Godot's initial_velocity)
- Godot speed_scale 0.1 = very slow → Unity 1.0 linear velocity

#### **Step 7: Limit Velocity over Lifetime Module** 🛑
```
☑ Enable module
Damping: 0.25                       (Godot damping 5.0 → Unity 0.25)
```

**Why damping:**
- Clouds slow down gradually (natural easing)
- Lower value = gentler slowdown (match your destructible style)

#### **Step 8: Texture Sheet Animation Module** �
```
☑ Enable module
Mode: Sprites                        (NOT Grid - we use single sprite)
Frame over Time: Constant 0          (NO animation - clouds stay static)
Start Frame: Constant 0              (always show first/only sprite)
Sprites: Size 1
  ↳ Element 0: Clouds.png           (drag from Assets/.../Particle/)
```

**Why Sprites mode:**
- Single sprite (not animated sprite sheet)
- Same as your destructible setup
- Clouds don't animate (static texture)

#### **Step 9: Renderer Module** 🎨
```
Render Mode: Billboard               (faces camera)
Material: Sprites/Default            (your standard sprite material)
Sorting Layer ID: Default            (0)
Order in Layer: 10                   (above gameplay, below UI)
```

**Why Order 10:**
- Above ground (-1), above player (0), below UI (100+)
- Match your existing layer setup

#### **Step 10: Add ENV_ParticleWeather Component** 🔧
1. Select `PFX_Clouds` → Add Component → `ENV_ParticleWeather`
2. Configure:
   ```
   Play On Start: ✓ ON
   Follow Camera: ✓ ON              (IMPORTANT: clouds follow camera)
   Particle Systems: Auto-detected  (leave empty, script finds it)
   ```

#### **Step 11: Save as Prefab** 💾
1. Drag `PFX_Clouds` from Hierarchy → `Assets/GAME/Prefabs/Environment/Weather/`
2. Confirm prefab created

---

## 🌧️ **Effect 2: Rain System** (~20 min)

### **Godot Reference Values:**
```
Rain.tscn:
  amount: 40
  speed_scale: 2.0
  initial_velocity: 60
  lifetime: 3.0
  3-frame animation (rain drops falling)

RainOnFloor.tscn:
  amount: 60
  z_index: -1
  lifetime: 0.5
  3-frame animation (splash on ground)
```

### **Unity Setup Steps:**

#### **Step 1: Create Parent GameObject**
1. Hierarchy → Create Empty → Name: `PFX_Rain`
2. Position: `(0, 0, 0)`

#### **Step 2: Create Child Particle System 1 (Rain Drops)**
1. Select `PFX_Rain` → Right-click → Effects → Particle System
2. Rename to: `RainDrops`

#### **Step 3: Configure RainDrops - Main Module** ⚙️
```
Duration: 5.0
Looping: ✓ ON
Start Lifetime: 3.0                  (Godot: 3.0)
Start Speed: 30.0                    (Godot velocity 60 → Unity 30)
Start Size: 0.5                      (small rain drops)
Start Rotation: 0°                   (drops fall straight)
Gravity Modifier: 0                  (top-down, no gravity)
Simulation Space: World
Play On Awake: ✓ ON
```

**Why Start Speed 30:**
- Rain falls fast (not slow like clouds)
- Godot speed_scale 2.0 × velocity 60 ≈ Unity 30

#### **Step 4: RainDrops - Emission Module** 📊
```
Rate over Time: 40                   (Godot amount: 40)
```

#### **Step 5: RainDrops - Shape Module** 🔷
```
Shape: Box
Scale: (20, 15, 0.01)               (wide spawn area)
Emit from: Volume
```

**Why 20x15 box:**
- Covers camera view (Rain.png texture)
- Wider than Clouds (more coverage)

#### **Step 6: RainDrops - Texture Sheet Animation** 🎬
```
☑ Enable module
Mode: Sprites                        (use sprite list, not Grid)
Animation: Whole Sheet               (animate through all sprites)
Frame over Time: Curve (0.0 → 1.0)   (animate through 3 frames)
  - Add keyframes:
    - Key at 0.0s: Value 0           (first frame)
    - Key at 1.0s: Value 1.0         (last frame, wraps to 2nd frame index)
Start Frame: Random 0 - 2.999        (randomize starting frame)
Sprites: Size 3
  ↳ Element 0: Rain_0 (first slice)
  ↳ Element 1: Rain_1 (second slice)
  ↳ Element 2: Rain_2 (third slice)
```

**IMPORTANT: Sprite Sheet Slicing Required FIRST!**
Before this works, you must slice `Rain.png`:
1. Select `Rain.png` in Project → Inspector
2. **Sprite Mode:** Multiple
3. **Pixels Per Unit:** Match your project (probably 16 or 32)
4. Click **Sprite Editor** button
5. Slice → **Type: Automatic** or **Grid By Cell Count** (3 columns, 1 row)
6. Click **Apply**
7. You'll see 3 child sprites: `Rain_0`, `Rain_1`, `Rain_2`
8. Drag these 3 sprites into the Sprites list above

**Why Sprites mode (not Grid):**
- More control over which frames to use
- Matches your destructible setup (you used Sprites mode there too)
- Can reorder/skip frames if needed

#### **Step 7: RainDrops - Renderer Module** 🎨
```
Render Mode: Billboard
Material: Sprites/Default            (same as destructible)
Sorting Layer ID: Default
Order in Layer: 5                    (above ground, below clouds)
```

#### **Step 8: Create Child Particle System 2 (Rain Splash)**
1. Select `PFX_Rain` → Right-click → Effects → Particle System
2. Rename to: `RainSplash`

#### **Step 9: Configure RainSplash - Main Module** ⚙️
```
Duration: 5.0
Looping: ✓ ON
Start Lifetime: 0.5                  (quick splash animation)
Start Speed: 0                       (stationary on ground)
Start Size: 0.8
Start Rotation: Random 0° - 360°     (variety)
Gravity Modifier: 0
Simulation Space: World
Play On Awake: ✓ ON
```

**Why Start Speed 0:**
- Splashes don't move (appear on ground)
- Short lifetime (0.5s) for quick effect

#### **Step 10: RainSplash - Emission Module** 📊
```
Rate over Time: 60                   (more splashes than drops)
```

#### **Step 11: RainSplash - Shape Module** 🔷
```
Shape: Box
Scale: (20, 15, 0.01)               (same as RainDrops)
Emit from: Volume
```

#### **Step 12: RainSplash - Texture Sheet Animation** 🎬
```
☑ Enable module
Mode: Sprites
Animation: Whole Sheet
Frame over Time: Curve (0.0 → 1.0)
Start Frame: Random 0 - 2.999
Sprites: Size 3
  ↳ Element 0: RainOnFloor_0
  ↳ Element 1: RainOnFloor_1
  ↳ Element 2: RainOnFloor_2
```

**IMPORTANT: Slice `RainOnFloor.png` same as Rain.png!**
1. Select `RainOnFloor.png` → Sprite Mode: Multiple
2. Sprite Editor → Slice: Automatic or Grid (3 columns, 1 row)
3. Apply → you'll get 3 child sprites
4. Drag all 3 into Sprites list

#### **Step 13: RainSplash - Renderer Module** 🎨
```
Render Mode: Billboard
Material: Sprites/Default
Sorting Layer ID: Default
Order in Layer: -1                   (below everything - on ground)
```

**Why Order -1:**
- Splashes appear "on floor" (Godot z_index: -1)
- Below player, enemies, grass

#### **Step 14: Add ENV_ParticleWeather to Parent** 🔧
1. Select `PFX_Rain` (parent) → Add Component → `ENV_ParticleWeather`
2. Configure:
   ```
   Play On Start: ✓ ON
   Follow Camera: ✓ ON              (rain follows camera)
   Particle Systems: Auto-detected  (finds both children)
   ```

#### **Step 15: Save as Prefab** 💾
1. Drag `PFX_Rain` from Hierarchy → `Assets/GAME/Prefabs/Environment/Weather/`

---

## 🍂 **Effect 3: Falling Leaves** (~12 min)

### **Godot Reference Values:**
```
amount: 10
speed_scale: 0.5 (slow gentle fall)
initial_velocity: 60
damping: 5.0
lifetime: 20.0
6-frame animation (leaf rotation)
```

### **Unity Setup Steps:**

#### **Step 1: Create GameObject**
1. Hierarchy → Create Empty → Name: `PFX_FallingLeaves`
2. Position: `(0, 0, 0)`

#### **Step 2: Add Particle System**
1. Select `PFX_FallingLeaves` → Add Component → Particle System

#### **Step 3: Main Module** ⚙️
```
Duration: 5.0
Looping: ✓ ON
Start Lifetime: 20.0                 (long fall time)
Start Speed: 5.0                     (Godot velocity 60 × speed_scale 0.5 ≈ 5)
Start Size: Random 0.8 - 1.2         (leaf variety)
Start Rotation: Random 0° - 360°     (tumbling leaves)
Gravity Modifier: 0
Simulation Space: World
Play On Awake: ✓ ON
```

**Why Start Speed 5:**
- Slower than rain (leaves drift gently)
- Godot speed_scale 0.5 = half speed

#### **Step 4: Emission Module** 📊
```
Rate over Time: 10                   (Godot amount: 10)
```

**Why only 10:**
- Leaves are larger than rain drops
- Fewer particles = peaceful autumn feel

#### **Step 5: Shape Module** 🔷
```
Shape: Box
Scale: (20, 15, 0.01)
Emit from: Volume
```

#### **Step 6: Velocity over Lifetime Module** ⚡
```
☑ Enable module
Linear: (0, -0.5, 0)                (gentle downward drift)
```

**Why Y: -0.5:**
- Leaves fall downward slowly
- Combined with Start Speed for diagonal fall

#### **Step 7: Limit Velocity over Lifetime Module** 🛑
```
☑ Enable module
Damping: 0.25                       (Godot damping 5.0 → 0.25)
```

**Why damping:**
- Leaves slow down as they fall (air resistance)
- Match cloud damping style

#### **Step 8: Rotation over Lifetime Module** 🔄
```
☑ Enable module
Angular Velocity: Random -45 to 45   (tumbling rotation)
```

**Why rotation:**
- Leaves spin as they fall (natural tumbling)
- Random direction adds variety

#### **Step 9: Texture Sheet Animation Module** 🎬
```
☑ Enable module
Mode: Sprites
Animation: Whole Sheet
Frame over Time: Random 0 - 1        (each leaf different animation speed)
  ↳ Set dropdown to "Random Between Two Constants"
  ↳ Min: 0, Max: 1.0
Start Frame: Random 0 - 5.999        (randomize starting frame)
Sprites: Size 6
  ↳ Element 0: Leaf_0
  ↳ Element 1: Leaf_1
  ↳ Element 2: Leaf_2
  ↳ Element 3: Leaf_3
  ↳ Element 4: Leaf_4
  ↳ Element 5: Leaf_5
```

**IMPORTANT: Slice `Leaf.png` into 6 frames FIRST!**
1. Select `Leaf.png` → Inspector → Sprite Mode: Multiple
2. Sprite Editor → Slice: Automatic or Grid (6 columns, 1 row)
3. Apply → you'll get 6 child sprites (`Leaf_0` through `Leaf_5`)
4. Drag all 6 into the Sprites list

**Why random Frame over Time:**
- Each leaf animates at different speed (looks organic)
- Some leaves rotate fast through frames, some slow
- Matches natural leaf tumbling

#### **Step 10: Color over Lifetime Module** 🎨
```
☑ Enable module
Alpha: Gradient
  - 0.0s: Alpha 1.0   (fully visible)
  - 0.7s: Alpha 1.0   (stay visible)
  - 1.0s: Alpha 0.0   (fade out)
```

**Why fade at end:**
- Match your destructible fade style
- Leaves disappear smoothly (not pop)

#### **Step 11: Renderer Module** 🎨
```
Render Mode: Billboard
Material: Sprites/Default
Sorting Layer ID: Default
Order in Layer: 6                    (above rain drops)
```

#### **Step 12: Add ENV_ParticleWeather Component** 🔧
1. Select `PFX_FallingLeaves` → Add Component → `ENV_ParticleWeather`
2. Configure:
   ```
   Play On Start: ✓ ON
   Follow Camera: ✓ ON
   Particle Systems: Auto-detected
   ```

#### **Step 13: Save as Prefab** 💾
1. Drag `PFX_FallingLeaves` → `Assets/GAME/Prefabs/Environment/Weather/`

---

## 🧪 Testing Checklist (Phase 1)

After creating all 3 effects, test each one:

### **Clouds Test:**
- [ ] Clouds drift slowly horizontally
- [ ] 6 clouds visible at a time
- [ ] Clouds vary in size (1.5-2.5)
- [ ] No jittery movement (smooth drift)
- [ ] Follows camera when player moves

### **Rain Test:**
- [ ] Rain drops fall vertically
- [ ] 3-frame animation plays (drops animate)
- [ ] Splashes appear on ground (Order -1)
- [ ] Splashes animate through 3 frames
- [ ] 40 drops + 60 splashes feel balanced
- [ ] Rain follows camera

### **Leaves Test:**
- [ ] Leaves fall diagonally (down + slight horizontal)
- [ ] Leaves rotate/tumble as they fall
- [ ] 6-frame animation plays (leaf shape changes)
- [ ] Only ~10 leaves visible (not overwhelming)
- [ ] Leaves fade out at end of lifetime
- [ ] Leaves follow camera

### **Common Issues & Fixes:**

**❌ Particles spawn all at once then stop**
- Check: Emission → Rate over Time (NOT Bursts)
- Check: Looping = ON

**❌ Rain/Leaf animation not playing**
- Check: Sprite sliced into multiple frames (Rain = 3, Leaf = 6)
- Check: Texture Sheet Animation module enabled
- Check: Mode set to **Sprites** (not Grid)
- Check: All sliced sprites dragged into Sprites list
- Check: Frame over Time curve goes from 0 to 1.0

**❌ Particles don't follow camera**
- Check: `ENV_ParticleWeather` has `followCamera = true`
- Check: Main Camera tag is set

**❌ Particles too fast/slow**
- Adjust: Start Speed (lower = slower)
- Adjust: Velocity over Lifetime

**❌ Particles disappear too quickly**
- Increase: Start Lifetime
- Check: Color over Lifetime alpha curve

---

## 📊 Comparison: Your Destructible vs Weather Effects

| Setting              | Destructible (Burst) | Weather (Continuous) |
|----------------------|----------------------|----------------------|
| **Looping**          | ❌ OFF               | ✅ ON                |
| **Emission**         | Burst (6 instant)    | Rate over Time (6-60/s) |
| **Start Speed**      | 20-30                | 5-30 (varies)        |
| **Start Lifetime**   | 1.5s                 | 3-25s (varies)       |
| **Simulation Space** | World                | World                |
| **Alpha Fade**       | 70%-100%             | 70%-100% (same)      |
| **Play On Awake**    | ✅ ON                | ✅ ON                |
| **Texture Setup**    | Sprites mode         | Sprites mode (same!) |
| **Material**         | Sprites/Default      | Sprites/Default      |
| **Render Mode**      | Billboard            | Billboard            |

**Key Difference:**
- **Destructible:** ONE burst, then done (no looping)
- **Weather:** Continuous emission, looping (infinite)

**Same Setup Method:**
- Both use **Texture Sheet Animation → Sprites mode**
- Both use **Material: Sprites/Default**
- Both use **Render Mode: Billboard**

---

## 🎯 Next Steps

After testing these 3 effects:
1. ✅ Verify all sprites sliced correctly (Rain, Leaf)
2. ✅ Verify particle counts feel balanced
3. ✅ Verify camera following works
4. 📝 Document any tweaks needed
5. ➡️ Move to Phase 2: Smoke, Fog, Spark, Snow (5 more effects)

---

## 📚 References

- **Your System:** `ENV_Destructible.cs` (Week 10)
- **Godot Source:** `res://vfx/Particle/` (analyzed values)
- **Conversion Table:** See `WEATHER_EFFECTS_PLAN.md` Section 6
- **Week 10 Particle Guide:** `Docs/Week_10_Oct21-25/1_DESTRUCTIBLE_OBJECTS.md`
