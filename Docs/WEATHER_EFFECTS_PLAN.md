# Weather & Atmospheric Effects Implementation Plan
**Created:** October 17, 2025  
**Status:** 🚧 Planning Phase  
**Based on:** Godot 3 particle reference files

---

## 📋 Overview

Implement **6 atmospheric effects** from the Godot project using Unity Particle Systems:
1. **☁️ Clouds** - Slow-moving background clouds with fade
2. **🌧️ Rain** - Falling rain with ground splash effects  
3. **🍂 Falling Leaves** - Gentle leaf fall with flutter animation
4. **💨 Smoke** - Rising smoke with upward drift
5. **🌫️ Fog** - Low-hanging mist/fog layer
6. **⚡ Spark** - Magical sparkles/fireflies

**Plus Bonus Effects:**
- **❄️ Snow** - Winter weather variant (similar to rain)
- **☀️ Raylight** - God rays/light shafts (static sprite overlay)

**Reference Files Analyzed:**
- `GodotProjectv3/World/Particle/Clouds.tscn`
- `GodotProjectv3/World/Particle/Rain.tscn` + `RainOnFloor.tscn`
- `GodotProjectv3/World/Particle/FallingLeaf.tscn`
- `GodotProjectv3/World/Particle/Smoke.tscn`
- `GodotProjectv3/World/Particle/Snow.tscn`
- `GodotProjectv3/World/Particle/Spark.tscn`

**Unity Assets Available:**
- ✅ **Particle Textures:** `/Assets/GAME/Scripts/Environment/Particle/`
  - Clouds.png, Rain.png, RainOnFloor.png, Leaf.png, LeafPink.png, Snow.png, Spark.png
- ✅ **FX Textures:** `/Assets/GAME/Scripts/Environment/FX/`
  - Smoke/ (SmokeCircular variants), Environment/Fog.png, Environment/Raylight.png
- ✅ **Existing Prefabs:** Grass.prefab, Rock.prefab, Vase.prefab, Wood.prefab (destructibles)

**Existing Foundation:**
- ✅ `ENV_MovingCloud.cs` - Already have cloud movement logic
- ✅ `ENV_Destructible.cs` - Reference for particle system setup
- ✅ Week 10 particle system knowledge (from destructibles)
- ✅ All texture assets ready to use!

---

## 🎯 Effect Specifications (Godot → Unity Translation)

### **1. Clouds Effect** 🌥️

**Godot Settings:**
```gdscript
CPUParticles2D:
  modulate: Color(0.10, 0.20, 0.23, 0.75) # Dark blue-gray, semi-transparent
  z_index: -1                              # Behind everything
  speed_scale: 0.1                         # VERY slow
  emission_shape: Rectangle
  emission_rect_extents: Vector2(150, 250) # Wide spawn area
  direction: Vector2(1, 1)                 # Right-down diagonal
  spread: 0.0                              # No spread
  gravity: Vector2(0, 0)                   # No gravity
  initial_velocity: 100.0                  # Base speed
  initial_velocity_random: 0.46            # 46% speed variation
  damping: 5.0                             # Slow down quickly
  damping_random: 0.64                     # 64% damping variation
  color_ramp: Fade in → full → fade out   # Gradient alpha
  anim_offset_random: 1.0                  # Random frame start
```

**Unity Implementation:**
- **Particle System Type:** Continuous emission
- **Main Module:**
  - Start Lifetime: `8-12s` (very long, slow drift)
  - Start Speed: `10-20` (converted from velocity 100 × speed_scale 0.1)
  - Start Size: `2-4` (large cloud sprites)
  - Start Rotation: `0-360°` (random orientation)
  - Start Color: `(26, 52, 59, 191)` RGBA or `#1A343BBF` (dark blue-gray at 75% alpha)
  - Simulation Space: `World` (move independently of parent)
  
- **Emission Module:**
  - Rate over Time: `2-3` (sparse clouds)
  - Bursts: None
  
- **Shape Module:**
  - Shape: `Rectangle`
  - Scale: `(300, 500, 1)` (converted from 150×250 extents, doubled)
  - Emit from: `Volume`
  
- **Velocity over Lifetime Module:**
  - Linear: `(1, 1, 0)` (right-down diagonal drift)
  - Space: `World`
  
- **Limit Velocity over Lifetime Module:**
  - Dampen: `0.5` (equivalent to damping 5.0)
  - Speed: `5-10` (terminal velocity)
  
- **Color over Lifetime Module:**
  - Gradient Alpha: `0 → 255 (18%) → 255 (84%) → 0` (fade in/out)
  
- **Renderer Module:**
  - Render Mode: `Billboard`
  - Material: `Sprites/Default`
  - Sorting Layer: `Background` (behind all gameplay)
  - Sorting Order: `-10`

**Texture Requirements:**
- Cloud sprite (semi-transparent white/gray wispy clouds)
- Size: ~128x128 or 256x256 pixels
- Format: PNG with alpha channel

---

### **2. Rain Effect** 🌧️

**Godot Settings:**
```gdscript
Rain.tscn (falling rain):
  z_index: 1                               # In front of background
  amount: 40                               # 40 particles
  speed_scale: 2.0                         # Fast
  emission_shape: Rectangle
  emission_rect_extents: Vector2(150, 100) # Wide horizontal spawn
  direction: Vector2(-0.5, 1)              # Slight left angle
  spread: 0.0                              # Straight lines
  gravity: Vector2(0, 0)                   # No gravity (uses velocity)
  initial_velocity: 60.0                   # Fast fall
  material: Sprite sheet animation (3 frames horizontal)
  color_ramp: Full → full → fade out

RainOnFloor.tscn (splash effect):
  z_index: -1                              # Below player
  amount: 60                               # More splashes
  speed_scale: 2.0                         # Fast
  Same emission/direction as Rain
  material: Sprite sheet animation (3 frames)
```

**Unity Implementation (2 Particle Systems):**

#### **Rain Drops (Top Layer):**
- **Main Module:**
  - Start Lifetime: `1-1.5s`
  - Start Speed: `120` (60 × 2.0 speed_scale)
  - Start Size: `0.1-0.2` (small streaks)
  - Start Rotation: `0°` (aligned)
  - Max Particles: `100`
  
- **Emission Module:**
  - Rate over Time: `40`
  
- **Shape Module:**
  - Shape: `Rectangle`
  - Scale: `(300, 200, 1)` (wide spawn above screen)
  - Position: `(0, 5, 0)` (spawn above viewport)
  - Rotation: `(-10, 0, 0)` (slight left angle)
  
- **Velocity over Lifetime:**
  - Linear: `(-0.5, -1, 0)` normalized and scaled
  
- **Texture Sheet Animation Module:**
  - Mode: `Sprites`
  - Tiles: `3×1` (3 horizontal frames)
  - Animation: `Single Row`
  - Frame over Time: Random start
  - Cycles: `1`
  
- **Color over Lifetime:**
  - Gradient Alpha: `255 → 255 (84%) → 0` (fade at end)
  
- **Renderer Module:**
  - Render Mode: `Stretched Billboard`
  - Length Scale: `2` (stretch into rain streaks)
  - Sorting Layer: `Effects`
  - Sorting Order: `10`

#### **Rain Splash (Ground Layer):**
- **Main Module:**
  - Start Lifetime: `0.3-0.5s` (quick splash)
  - Start Speed: `0` (spawn in place)
  - Start Size: `0.3-0.5`
  - Max Particles: `60`
  
- **Emission Module:**
  - Rate over Time: `60`
  
- **Shape Module:**
  - Shape: `Rectangle`
  - Scale: `(300, 1, 1)` (ground line)
  - Position: `(0, -2, 0)` (at ground level)
  
- **Texture Sheet Animation:**
  - Mode: `Sprites`
  - Tiles: `3×1`
  - Animation: `Whole Sheet` (play all 3 frames)
  - Frame over Time: `Linear` (0 → 1)
  
- **Renderer Module:**
  - Sorting Layer: `Default`
  - Sorting Order: `-5` (below characters)

**Texture Requirements:**
- Rain drop sprite sheet: 3 frames (vertical streak variations)
- Rain splash sprite sheet: 3 frames (splash animation)
- Size: 64x64 per frame (total 192x64 for 3-frame sheet)

---

### **3. Falling Leaves Effect** 🍂

**Godot Settings:**
```gdscript
CPUParticles2D:
  z_index: 1                               # Front layer
  amount: 10                               # Sparse
  speed_scale: 0.5                         # Slow
  emission_shape: Rectangle
  emission_rect_extents: Vector2(100, 100) # Square spawn
  direction: Vector2(1, 1)                 # Right-down diagonal
  spread: 0.0                              # No spread
  gravity: Vector2(0, 0)                   # No gravity
  initial_velocity: 30.0                   # Gentle fall
  initial_velocity_random: 0.46            # Variation
  damping: 5.0                             # Slow down
  damping_random: 0.64                     # Variation
  material: Sprite sheet animation (6 frames horizontal)
  anim_speed: 1.0                          # Normal animation speed
  color_ramp: Full → full → fade out
```

**Unity Implementation:**
- **Main Module:**
  - Start Lifetime: `6-10s` (long gentle fall)
  - Start Speed: `15` (30 × 0.5 speed_scale)
  - Start Size: `0.3-0.5` (small leaves)
  - Start Rotation: `0-360°` (random orientation)
  - Max Particles: `20`
  
- **Emission Module:**
  - Rate over Time: `2`
  
- **Shape Module:**
  - Shape: `Rectangle`
  - Scale: `(200, 200, 1)` (square spawn area above screen)
  - Position: `(0, 3, 0)` (spawn above)
  
- **Velocity over Lifetime:**
  - Linear: `(1, -1, 0)` (right-down diagonal)
  - Space: `World`
  
- **Limit Velocity over Lifetime:**
  - Dampen: `0.5`
  - Speed: `5-8` (gentle terminal velocity)
  
- **Rotation over Lifetime:**
  - Angular Velocity: `45-90°/s` (gentle spin/flutter)
  
- **Texture Sheet Animation Module:**
  - Mode: `Sprites`
  - Tiles: `6×1` (6 leaf types/rotation frames)
  - Animation: `Single Row`
  - Frame over Time: Random start
  - Cycles: `1-2` (subtle animation)
  
- **Color over Lifetime:**
  - Gradient Alpha: `255 → 255 (84%) → 0`
  
- **Renderer Module:**
  - Render Mode: `Billboard`
  - Sorting Layer: `Effects`
  - Sorting Order: `5`

**Texture Requirements:**
- Leaf sprite sheet: 6 frames (different leaf types or rotation angles)
- Size: 32x32 per frame (total 192x32 for 6-frame sheet)
- Colors: Autumn colors (orange, red, yellow, brown)

---

### **4. Smoke Effect** 💨

**Godot Settings:**
```gdscript
CPUParticles2D:
  modulate: Color(0.59, 0.59, 0.59, 1.0) # Gray smoke
  amount: 4                               # Very sparse
  speed_scale: 0.5                        # Slow
  emission_shape: Point                   # Single point spawn
  direction: Vector2(1, -100)             # Mostly upward
  spread: 20.0                            # Some horizontal variation
  gravity: Vector2(0, 0)                  # No gravity
  initial_velocity: 30.0                  # Moderate rise
  damping: 5.0                            # Slow down quickly
  material: Sprite sheet animation (6 frames horizontal)
  anim_speed: 1.0                         # Normal animation
```

**Unity Implementation:**
- **Particle System Type:** Continuous emission
- **Main Module:**
  - Start Lifetime: `3-5s` (slow dissipation)
  - Start Speed: `15` (30 × 0.5 speed_scale)
  - Start Size: `0.5-1.0` (grows as it rises)
  - Start Rotation: `0-360°`
  - Start Color: `(151, 151, 151, 255)` RGBA (gray smoke)
  - Max Particles: `10`
  
- **Emission Module:**
  - Rate over Time: `1-2` (very sparse)
  
- **Shape Module:**
  - Shape: `Sphere`
  - Radius: `0.1` (point-like source)
  
- **Velocity over Lifetime Module:**
  - Linear: `(0.1, 1, 0)` (mostly upward with slight drift)
  - Space: `World`
  
- **Limit Velocity over Lifetime Module:**
  - Dampen: `0.7` (slow down significantly)
  - Speed: `5-8`
  
- **Size over Lifetime Module:**
  - Curve: `0.5 → 1.0` (grows as it rises/dissipates)
  
- **Color over Lifetime Module:**
  - Gradient Alpha: `255 → 0` (fade out completely)
  
- **Rotation over Lifetime:**
  - Angular Velocity: `30-60°/s` (gentle swirl)
  
- **Texture Sheet Animation Module:**
  - Mode: `Sprites` or `Grid`
  - Tiles: `6×1`
  - Frame over Time: `Linear 0→1` (animate through frames)
  - Cycles: `1`
  
- **Renderer Module:**
  - Render Mode: `Billboard`
  - Sorting Layer: `Effects`
  - Sorting Order: `0`

**Texture Requirements:**
- ✅ **Available:** `FX/Smoke/SmokeCircular/` (6-frame animation)
- Smoke puff sprite sheet with growing/dissipating animation

---

### **5. Fog Effect** 🌫️

**Godot Analysis:**
- Fog is NOT a particle system in Godot (no .tscn found)
- Likely implemented as static/scrolling sprite overlays
- Can be done with large, semi-transparent cloud particles OR as a static sprite

**Unity Implementation (Option A - Particle System):**
- **Particle System Type:** Continuous, very slow movement
- **Main Module:**
  - Start Lifetime: `20-30s` (very long-lived)
  - Start Speed: `2-5` (barely moves)
  - Start Size: `5-10` (HUGE fog banks)
  - Start Color: `(200, 200, 220, 80)` RGBA (light blue-gray, very transparent)
  - Max Particles: `10-15`
  
- **Emission Module:**
  - Rate over Time: `0.5` (extremely sparse)
  
- **Shape Module:**
  - Shape: `Rectangle`
  - Scale: `(500, 100, 1)` (huge horizontal layer)
  - Position: `(0, -1, 0)` (ground level)
  
- **Velocity over Lifetime:**
  - Linear: `(0.5, 0, 0)` (slow horizontal drift)
  
- **Color over Lifetime:**
  - Gradient Alpha: `0 → 80 → 80 → 0` (fade in, sustain, fade out)
  
- **Renderer Module:**
  - Render Mode: `Billboard`
  - Sorting Layer: `Background`
  - Sorting Order: `-5` (behind everything except far background)

**Unity Implementation (Option B - Static Sprite Overlay):**
- Use `Fog.png` as semi-transparent overlay sprite
- Attach to Camera as UI element or world-space quad
- Optional slow scroll with simple script
- Simpler, more performant for persistent fog

**Texture Requirements:**
- ✅ **Available:** `FX/Environment/Fog.png`

---

### **6. Spark Effect** ⚡

**Godot Settings:**
```gdscript
CPUParticles2D:
  z_index: 1                              # Front layer
  amount: 10                              # Sparse
  emission_shape: Rectangle
  emission_rect_extents: Vector2(100, 100) # Square area
  direction: Vector2(0, -1)               # Upward
  spread: 0.0                             # No spread
  gravity: Vector2(0, 0)                  # No gravity
  initial_velocity: 5.0                   # Very slow rise
  initial_velocity_random: 0.5            # 50% variation
  material: Sprite sheet animation (7 frames horizontal)
  anim_speed: 1.0                         # Normal animation
  color_ramp: Full → full → fade out
```

**Unity Implementation:**
- **Particle System Type:** Continuous emission (magical sparkles/fireflies)
- **Main Module:**
  - Start Lifetime: `3-5s`
  - Start Speed: `5` (very slow)
  - Start Size: `0.2-0.4` (small sparkles)
  - Start Rotation: `0-360°`
  - Max Particles: `20`
  
- **Emission Module:**
  - Rate over Time: `3-5`
  
- **Shape Module:**
  - Shape: `Rectangle`
  - Scale: `(200, 200, 1)` (square spawn area)
  
- **Velocity over Lifetime Module:**
  - Linear: `(0, 0.5, 0)` (gentle upward drift)
  - Space: `World`
  
- **Color over Lifetime Module:**
  - Gradient: Full white → slight color tint → fade out
  - Gradient Alpha: `255 → 255 (84%) → 0`
  
- **Texture Sheet Animation Module:**
  - Mode: `Grid`
  - Tiles: `7×1`
  - Frame over Time: `Linear 0→1` (twinkle animation)
  - Cycles: `1-2`
  
- **Renderer Module:**
  - Render Mode: `Billboard`
  - Material: Use Additive shader for glow effect
  - Sorting Layer: `Effects`
  - Sorting Order: `10` (in front)

**Texture Requirements:**
- ✅ **Available:** `Particle/Spark.png` (7-frame animation)

---

### **BONUS: Snow Effect** ❄️

**Godot Settings:**
```gdscript
CPUParticles2D:
  amount: 40                              # Same as rain
  speed_scale: 0.5                        # Slower than rain
  emission_shape: Rectangle
  emission_rect_extents: Vector2(150, 100)
  direction: Vector2(1, 1)                # Diagonal fall
  spread: 0.0                             # No spread
  gravity: Vector2(0, 0)                  # No gravity
  initial_velocity: 30.0                  # Gentle fall
  initial_velocity_random: 0.46           # Variation
  damping: 5.0                            # Flutter/slow down
  damping_random: 0.64                    # Variation
  material: Sprite sheet animation (7 frames)
  color_ramp: Full → full → fade out
```

**Unity Implementation:**
- **Similar to rain but slower and more flutter**
- Start Speed: `15` (30 × 0.5)
- Add rotation over lifetime for snowflake spin
- Use white color instead of rain's blue tint
- ✅ **Texture Available:** `Particle/Snow.png`

---

### **BONUS: Raylight Effect** ☀️

**Godot Analysis:**
- Raylight is NOT a particle system (no .tscn found)
- Static god rays/light shafts effect
- Sprite overlay with gradient transparency

**Unity Implementation:**
- **NOT a particle system** - use static sprite
- Place `Raylight.png` as world-space sprite or UI overlay
- Position coming from light source direction (top-left/top-right)
- Semi-transparent, additive blend mode
- Optional: Subtle fade in/out animation with script
- ✅ **Texture Available:** `FX/Environment/Raylight.png`

---

## 🗂️ File Structure

```
Assets/GAME/
├── Scripts/
│   └── Environment/
│       ├── ENV_MovingCloud.cs          ✅ EXISTS (for scripted clouds)
│       ├── ENV_WeatherManager.cs       🆕 NEW (control weather zones/states)
│       ├── ENV_ParticleWeather.cs      🆕 NEW (individual weather control)
│       ├── FX/                         ✅ EXISTS (FX textures)
│       │   ├── Smoke/                  ✅ (smoke sprite sheets)
│       │   └── Environment/            ✅ (Fog.png, Raylight.png)
│       └── Particle/                   ✅ EXISTS (particle textures)
│           ├── Clouds.png              ✅ READY
│           ├── Rain.png                ✅ READY
│           ├── RainOnFloor.png         ✅ READY
│           ├── Leaf.png                ✅ READY
│           ├── LeafPink.png            ✅ READY (variant)
│           ├── Snow.png                ✅ READY
│           └── Spark.png               ✅ READY
│
├── Prefabs/
│   └── Environment/
│       └── Weather/                    🆕 NEW FOLDER
│           ├── PFX_Clouds.prefab       🆕 NEW
│           ├── PFX_Rain.prefab         🆕 NEW (2 child particle systems)
│           ├── PFX_FallingLeaves.prefab 🆕 NEW
│           ├── PFX_Smoke.prefab        🆕 NEW
│           ├── PFX_Fog.prefab          🆕 NEW (particle or sprite variant)
│           ├── PFX_Spark.prefab        🆕 NEW
│           ├── PFX_Snow.prefab         🆕 NEW (bonus)
│           └── ENV_Raylight.prefab     🆕 NEW (static sprite)
│
└── Scenes/
    └── Level1.unity                     📝 MODIFY (add weather prefabs)
```

**Note:** All texture assets already exist! No need to create sprite sheets from scratch.

---

## 🔧 Implementation Phases

### **Phase 1: Asset Verification & Preparation** (5 min)
1. **✅ All textures already exist!** Navigate to:
   - `Assets/GAME/Scripts/Environment/Particle/` (weather particles)
   - `Assets/GAME/Scripts/Environment/FX/` (smoke, fog, raylight)

2. **Verify texture import settings:**
   - Check each texture: Texture Type should be `Sprite (2D and UI)`
   - Check sprite sheets (Rain, Leaf, Snow, Spark): Set Sprite Mode: `Multiple`
   - Use Sprite Editor to slice sprite sheets if not already sliced:
     - Rain.png: 3 horizontal frames
     - Leaf.png: 6 horizontal frames
     - Snow.png: 7 horizontal frames
     - Spark.png: 7 horizontal frames
     - Smoke sheets: 6 frames (check existing slicing)

3. **Create Prefabs folder:**
   - Right-click `Assets/GAME/Prefabs/Environment/`
   - Create New Folder: `Weather`

---

### **Phase 2: Create Cloud Particle System** (10 min)

1. **Create Empty GameObject:**
   - Name: `PFX_Clouds`
   - Position: `(0, 0, 0)` (center of map or follow camera)
   - Add Component: `Particle System`

2. **Configure Modules (follow specifications above):**
   - Main, Emission, Shape, Velocity over Lifetime, Limit Velocity over Lifetime, Color over Lifetime, Renderer

3. **Test & Tune:**
   - Adjust spawn area to cover visible screen
   - Verify fade in/out is smooth
   - Check sorting layer (should be behind everything)

4. **Optional - Add Movement Script:**
   - If you want clouds to drift across map boundaries, add `ENV_MovingCloud.cs`
   - Or keep particle system stationary and let individual particles handle movement

5. **Save as Prefab:**
   - Drag to `Assets/GAME/Prefabs/Environment/Weather/`

---

### **Phase 3: Create Rain Particle System** (20 min)

1. **Create Parent GameObject:**
   - Name: `PFX_Rain`
   - Position: `(0, 0, 0)`

2. **Create Child 1 - Rain Drops:**
   - Name: `RainDrops`
   - Add Component: `Particle System`
   - Configure (follow "Rain Drops" specs above)
   - Key settings: Stretched Billboard, high velocity, texture sheet animation

3. **Create Child 2 - Rain Splash:**
   - Name: `RainSplash`
   - Add Component: `Particle System`
   - Configure (follow "Rain Splash" specs above)
   - Key settings: Ground-level spawn, short lifetime, splash animation

4. **Test & Tune:**
   - Verify rain falls at correct angle
   - Check splash timing matches rain landing
   - Adjust emission rates for desired intensity
   - Test with different camera positions

5. **Save as Prefab**

---

### **Phase 4: Create Falling Leaves Particle System** (12 min)

1. **Create Empty GameObject:**
   - Name: `PFX_FallingLeaves`
   - Position: `(0, 0, 0)`
   - Add Component: `Particle System`

2. **Configure Modules (follow specifications above):**
   - Focus on gentle motion: low speed, damping, rotation
   - Texture sheet animation for variety (6 frames)

3. **Test & Tune:**
   - Verify leaves flutter naturally
   - Check spawn area covers screen
   - Adjust sorting order (should be in front of background, behind UI)

4. **Save as Prefab**

---

### **Phase 5: Create Smoke Particle System** (12 min)

1. **Create Empty GameObject:**
   - Name: `PFX_Smoke`
   - Position: `(0, 0, 0)` (place at chimney/campfire location)
   - Add Component: `Particle System`

2. **Configure Modules (follow "Smoke Effect" specs):**
   - Use smoke texture from `FX/Smoke/`
   - 6-frame animation
   - Rising motion with growth and fade

3. **Test & Tune:**
   - Verify smoke rises and dissipates naturally
   - Check gray color tint
   - Adjust emission rate for desired density

4. **Save as Prefab**

---

### **Phase 6: Create Fog Effect** (10 min)

**Choose ONE implementation:**

**Option A - Particle System:**
1. Create `PFX_Fog` with large, slow-moving fog particles
2. Use `Fog.png` texture
3. Very sparse emission, huge size, long lifetime

**Option B - Static Sprite (Recommended for performance):**
1. Create `ENV_Raylight` GameObject
2. Add SpriteRenderer component
3. Set sprite: `FX/Environment/Fog.png`
4. Set sorting layer: Background, order: -5
5. Scale to cover desired area
6. Set alpha: 30-50% transparency
7. Optional: Add simple scroll script for movement

---

### **Phase 7: Create Spark Particle System** (12 min)

1. **Create Empty GameObject:**
   - Name: `PFX_Spark`
   - Position: `(0, 0, 0)` (place in magical areas)
   - Add Component: `Particle System`

2. **Configure Modules (follow "Spark Effect" specs):**
   - 7-frame animation
   - Slow upward drift
   - Additive material for glow

3. **Create Additive Material (for glow):**
   - Right-click in Project: Create → Material
   - Name: `MAT_Additive_Spark`
   - Shader: Particles → Standard Unlit (or Sprites/Default with Additive blend)
   - Assign to Renderer Module

4. **Test & Tune:**
   - Verify sparkle/twinkle animation
   - Check glow effect
   - Adjust color tint (gold/white)

5. **Save as Prefab**

---

### **Phase 8: BONUS - Snow & Raylight** (15 min)

**Snow Particle System:**
1. Duplicate `PFX_Rain` prefab
2. Rename to `PFX_Snow`
3. Replace textures with `Snow.png`
4. Adjust settings: slower speed, more rotation, white color
5. Remove or adjust splash system (optional snow ground impact)

**Raylight Static Sprite:**
1. Create `ENV_Raylight` GameObject
2. Add SpriteRenderer
3. Set sprite: `FX/Environment/Raylight.png`
4. Position coming from light source (top-right corner)
5. Rotate to angle rays diagonally
6. Set alpha: 20-40% transparency
7. Material: Additive blend for glow
8. Optional: Add gentle fade in/out script

---

### **Phase 9: Integration & Scene Setup** (10 min)

1. **Add to Level1.unity:**
   - Drag `PFX_Clouds` prefab into scene
   - Position: Center of map or attach to Camera (for following player)
   - Drag other weather prefabs as appropriate for level theme:
     - `PFX_Rain` + `PFX_FallingLeaves` = Autumn rainy day
     - `PFX_Snow` = Winter scene
     - `PFX_Fog` = Mysterious forest
     - `PFX_Spark` = Magical areas/shrines
     - `PFX_Smoke` = Villages with chimneys
     - `ENV_Raylight` = Morning/sunset scenes

2. **Layer Settings:**
   - Verify all particle systems use correct sorting layers
   - Check render order (fog behind, sparks in front, etc.)

3. **Performance Check:**
   - Total particle count should be < 200 across all systems
   - Check frame rate in Profiler
   - Disable systems not needed for current scene

---

---

### **Phase 10: Optional Scripts** (20 min)

**Create `ENV_WeatherManager.cs`:**
```csharp
using UnityEngine;

/// <summary>
/// Central manager for controlling weather particle systems.
/// Allows toggling weather effects on/off and switching between weather states.
/// </summary>
public class ENV_WeatherManager : MonoBehaviour
{
    [Header("Weather Systems")]
    [SerializeField] private ParticleSystem clouds;
    [SerializeField] private ParticleSystem rain;
    [SerializeField] private ParticleSystem rainSplash;
    [SerializeField] private ParticleSystem leaves;
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private ParticleSystem fog;
    [SerializeField] private ParticleSystem spark;
    [SerializeField] private ParticleSystem snow;
    
    [Header("Static Effects")]
    [SerializeField] private GameObject raylight;
    
    [Header("Weather State")]
    [SerializeField] private bool enableClouds = true;
    [SerializeField] private bool enableRain = false;
    [SerializeField] private bool enableLeaves = false;
    [SerializeField] private bool enableSmoke = false;
    [SerializeField] private bool enableFog = false;
    [SerializeField] private bool enableSpark = false;
    [SerializeField] private bool enableSnow = false;
    [SerializeField] private bool enableRaylight = false;
    
    void Start()
    {
        ApplyWeatherState();
    }
    
    /// <summary>
    /// Set all weather effects at once.
    /// </summary>
    public void SetWeather(bool clouds, bool rain, bool leaves, bool smoke, bool fog, bool spark, bool snow, bool raylight)
    {
        enableClouds = clouds;
        enableRain = rain;
        enableLeaves = leaves;
        enableSmoke = smoke;
        enableFog = fog;
        enableSpark = spark;
        enableSnow = snow;
        enableRaylight = raylight;
        ApplyWeatherState();
    }
    
    /// <summary>
    /// Preset: Clear sky with clouds only.
    /// </summary>
    public void SetClearWeather()
    {
        SetWeather(clouds: true, rain: false, leaves: false, smoke: false, fog: false, spark: false, snow: false, raylight: true);
    }
    
    /// <summary>
    /// Preset: Rainy autumn day.
    /// </summary>
    public void SetRainyWeather()
    {
        SetWeather(clouds: true, rain: true, leaves: true, smoke: false, fog: false, spark: false, snow: false, raylight: false);
    }
    
    /// <summary>
    /// Preset: Snowy winter.
    /// </summary>
    public void SetSnowyWeather()
    {
        SetWeather(clouds: true, rain: false, leaves: false, smoke: true, fog: true, spark: false, snow: true, raylight: false);
    }
    
    /// <summary>
    /// Preset: Magical/mystical atmosphere.
    /// </summary>
    public void SetMagicalWeather()
    {
        SetWeather(clouds: false, rain: false, leaves: false, smoke: false, fog: true, spark: true, snow: false, raylight: true);
    }
    
    void ApplyWeatherState()
    {
        ToggleParticleSystem(clouds, enableClouds);
        ToggleParticleSystem(rain, enableRain);
        ToggleParticleSystem(rainSplash, enableRain); // Linked to rain
        ToggleParticleSystem(leaves, enableLeaves);
        ToggleParticleSystem(smoke, enableSmoke);
        ToggleParticleSystem(fog, enableFog);
        ToggleParticleSystem(spark, enableSpark);
        ToggleParticleSystem(snow, enableSnow);
        
        if (raylight) raylight.SetActive(enableRaylight);
    }
    
    void ToggleParticleSystem(ParticleSystem ps, bool enable)
    {
        if (!ps) return;
        
        if (enable && !ps.isPlaying) 
        {
            ps.Play();
        }
        else if (!enable && ps.isPlaying) 
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    
    /// <summary>
    /// Toggle individual weather effect by name (for external calls).
    /// </summary>
    public void ToggleWeather(string weatherName, bool enable)
    {
        switch (weatherName.ToLower())
        {
            case "clouds": enableClouds = enable; ToggleParticleSystem(clouds, enable); break;
            case "rain": enableRain = enable; ToggleParticleSystem(rain, enable); ToggleParticleSystem(rainSplash, enable); break;
            case "leaves": enableLeaves = enable; ToggleParticleSystem(leaves, enable); break;
            case "smoke": enableSmoke = enable; ToggleParticleSystem(smoke, enable); break;
            case "fog": enableFog = enable; ToggleParticleSystem(fog, enable); break;
            case "spark": enableSpark = enable; ToggleParticleSystem(spark, enable); break;
            case "snow": enableSnow = enable; ToggleParticleSystem(snow, enable); break;
            case "raylight": enableRaylight = enable; if (raylight) raylight.SetActive(enable); break;
            default: Debug.LogWarning($"Unknown weather type: {weatherName}"); break;
        }
    }
}
```

**Create `ENV_ParticleWeather.cs` (Individual Weather Control):**
```csharp
using UnityEngine;

/// <summary>
/// Simple component for individual weather particle control.
/// Attach to individual weather prefabs for standalone enable/disable.
/// </summary>
public class ENV_ParticleWeather : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool followCamera = false;
    
    [Header("References")]
    [SerializeField] private ParticleSystem[] particleSystems;
    
    private Camera mainCamera;
    private Vector3 cameraOffset;
    
    void Awake()
    {
        // Auto-detect particle systems if not assigned
        if (particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
        
        if (followCamera)
        {
            mainCamera = Camera.main;
            if (mainCamera)
            {
                cameraOffset = transform.position - mainCamera.transform.position;
            }
        }
    }
    
    void Start()
    {
        if (playOnStart)
        {
            PlayWeather();
        }
        else
        {
            StopWeather();
        }
    }
    
    void Update()
    {
        if (followCamera && mainCamera)
        {
            transform.position = mainCamera.transform.position + cameraOffset;
        }
    }
    
    public void PlayWeather()
    {
        foreach (var ps in particleSystems)
        {
            if (ps && !ps.isPlaying) ps.Play();
        }
    }
    
    public void StopWeather()
    {
        foreach (var ps in particleSystems)
        {
            if (ps && ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    
    public void ToggleWeather()
    {
        if (particleSystems.Length > 0 && particleSystems[0].isPlaying)
        {
            StopWeather();
        }
        else
        {
            PlayWeather();
        }
    }
}
```

**Usage:**
- Attach `ENV_WeatherManager` to a GameObject in scene (e.g., `WeatherController`)
- Assign all weather prefab references in Inspector
- Call presets from other scripts: `weatherManager.SetRainyWeather();`
- Or toggle individual effects: `weatherManager.ToggleWeather("rain", true);`
- Attach `ENV_ParticleWeather` to individual prefabs for camera-following behavior

---

## 📊 Godot → Unity Conversion Reference

| **Godot Property** | **Unity Equivalent** | **Conversion Notes** |
|-------------------|---------------------|---------------------|
| `amount` | Emission → Rate over Time | Direct 1:1 |
| `speed_scale` | Simulation Speed OR multiply velocities | Multiply all speed values |
| `emission_rect_extents` | Shape → Scale | Extents = half-size, Unity uses full size (×2) |
| `direction` | Velocity over Lifetime → Linear | Normalize and scale |
| `spread` | Shape → Arc/Cone Angle | 0 = no spread = 0° cone |
| `gravity` | External Forces OR Velocity over Lifetime | Use Velocity if simple |
| `initial_velocity` | Start Speed | Direct 1:1 (then multiply by speed_scale) |
| `initial_velocity_random` | Start Speed → Random Between Two Constants | Use percentage range |
| `damping` | Limit Velocity over Lifetime → Dampen | Higher = slower; Unity uses 0-1 |
| `damping_random` | (No direct equivalent) | Approximate with velocity curves |
| `color_ramp` | Color over Lifetime → Gradient | Copy alpha curve |
| `anim_offset_random` | Texture Sheet Animation → Start Frame | Random Between Constants |
| `z_index` | Renderer → Sorting Order | -1 = background, 1+ = foreground |
| `modulate` | Start Color | Direct color mapping |
| `particles_animation` | Texture Sheet Animation Module | Enable module |
| `particles_anim_h_frames` | Tiles → X | Horizontal frame count |
| `particles_anim_v_frames` | Tiles → Y | Vertical frame count |

---

## 🎨 Visual Tuning Tips

### **If Clouds Look Wrong:**
- **Too fast:** Reduce Start Speed or increase Dampen
- **Too opaque:** Lower Start Color alpha (e.g., 128 instead of 191)
- **Too small/large:** Adjust Start Size
- **Not drifting:** Check Velocity over Lifetime is enabled
- **Popping in/out:** Adjust Color over Lifetime gradient smoothness

### **If Rain Looks Wrong:**
- **Not angled:** Check Shape rotation or Velocity direction
- **Too slow/fast:** Adjust Start Speed
- **Splash misaligned:** Move RainSplash position Y to match ground
- **Splash too slow:** Reduce Start Lifetime on splash
- **Not raining hard enough:** Increase Emission Rate over Time

### **If Leaves Look Wrong:**
- **Not fluttering:** Enable Rotation over Lifetime
- **Too fast:** Reduce Start Speed
- **Not varied:** Ensure Texture Sheet Animation is randomized
- **All same leaf:** Check sprite sheet frames are different

---

## 🧪 Testing Checklist

- [ ] **Clouds:**
  - [ ] Fade in/out smoothly
  - [ ] Move slowly in diagonal direction
  - [ ] Render behind all gameplay objects
  - [ ] No pop-in at spawn edges
  
- [ ] **Rain:**
  - [ ] Rain drops fall at slight angle
  - [ ] Splash timing matches drop landing
  - [ ] Rain visible but not overwhelming
  - [ ] Proper layering (drops in front, splash behind player)
  
- [ ] **Leaves:**
  - [ ] Flutter/rotate naturally
  - [ ] Multiple leaf sprites visible (6 variants)
  - [ ] Gentle falling speed
  - [ ] Fade out before hitting ground
  
- [ ] **Smoke:**
  - [ ] Rises with slight drift
  - [ ] Grows in size as it rises
  - [ ] Fades out completely
  - [ ] Animation plays through 6 frames
  
- [ ] **Fog:**
  - [ ] Covers ground area
  - [ ] Semi-transparent (not blocking gameplay)
  - [ ] Slow drift motion (if particle system)
  - [ ] Proper depth sorting (behind everything)
  
- [ ] **Spark:**
  - [ ] Gentle upward float
  - [ ] Twinkling animation visible
  - [ ] Glow/additive effect working
  - [ ] Not distracting from gameplay
  
- [ ] **Snow (Bonus):**
  - [ ] Falls slower than rain
  - [ ] Snowflakes rotate/flutter
  - [ ] White/light coloration
  
- [ ] **Raylight (Bonus):**
  - [ ] Positioned correctly from light source
  - [ ] Semi-transparent
  - [ ] Adds atmosphere without blocking view
  
- [ ] **Performance:**
  - [ ] No frame drops (check Profiler)
  - [ ] Total particle counts under 200-300
  - [ ] Memory usage stable
  - [ ] No lag spikes when starting/stopping effects
  
- [ ] **Integration:**
  - [ ] Works with camera movement
  - [ ] Sorting layers correct for all effects
  - [ ] Scripts work (if added WeatherManager)
  - [ ] Prefabs saved correctly
  - [ ] Audio syncs (if added - rain/wind sounds)

---

## 📝 Next Steps After Implementation

1. **Document in Week 10:** Move this to `Week_10_Oct21-25/3_WEATHER_EFFECTS.md` when complete
2. **Update HIERARCHY.md:** Add weather particle systems to scene hierarchy documentation
3. **Update README.md:** Add weather system to completed features list
4. **Audio Integration:** Add ambient weather sounds:
   - Rain sound loop (already have in `Audio/Sounds/`)
   - Wind ambiance for clouds/leaves
   - Magical chimes for spark effect
5. **Consider Weather Zones:**
   - Use Trigger colliders to enable/disable weather in areas
   - Example: Rain stops when entering buildings (interior zones)
   - Fog only in forest areas
6. **Seasonal Variants:**
   - Create weather presets for different biomes/seasons
   - Spring: Leaves + light rain + raylight
   - Summer: Clear clouds + spark (fireflies)
   - Autumn: Heavy leaves + fog
   - Winter: Snow + smoke (from fireplaces) + fog
7. **Performance Optimization:**
   - If needed, add LOD system for distant cameras
   - Consider distance-based particle reduction
   - Pool particle systems for dynamic weather transitions

---

## 🔗 Related Documentation

- **Week 10 Destructibles:** `Week_10_Oct21-25/1_DESTRUCTIBLE_OBJECTS.md` (particle system reference)
- **Week 10 Audio:** `Week_10_Oct21-25/README.md` (for adding weather sounds)
- **Coding Style:** `Guild/CODING_STYLE_GUIDE.md` (naming conventions: `ENV_*`, `PFX_*`)
- **Godot Comparison:** `Guild/GODOT_VS_UNITY_COMPARISON.md` (feature parity tracking)

---

## 📊 Summary

**Total Effects:** 8 (6 core + 2 bonus)
**Estimated Total Time:** ~110 minutes (1h 50min)
**Difficulty:** ⭐⭐ Intermediate (requires particle system knowledge)
**Dependencies:** Unity Particle System, ✅ All sprite assets available!
**Status:** Ready to implement ✅

**Key Advantages:**
- ✅ All textures already exist in project
- ✅ Existing particle system experience (from destructibles)
- ✅ Clear Godot reference implementations
- ✅ Optional management scripts for easy control
- ✅ Modular prefab system for mix-and-match weather

**What You Get:**
1. **Atmospheric Enhancement** - Rich, dynamic environments
2. **Scene Variety** - Different moods per level (rainy forest, snowy village, magical shrine)
3. **Visual Polish** - Professional-looking weather effects matching Godot reference
4. **Performance** - Optimized particle counts, tested settings
5. **Flexibility** - Easy to enable/disable per scene with `ENV_WeatherManager`
6. **Reusability** - Prefabs can be used across multiple scenes
