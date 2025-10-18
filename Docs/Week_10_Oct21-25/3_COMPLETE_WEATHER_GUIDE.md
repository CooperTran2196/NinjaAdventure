# Complete Weather System Guide

**Unity Version:** Unity 6 (6000.2.7f2)  
**Project:** NinjaAdventure  
**Status:** Phase 1 Complete (Clouds, Rain, Leaves) | Phase 2 In Progress

---

## 📋 Table of Contents

1. [Quick Start](#quick-start)
2. [System Architecture](#system-architecture)
3. [Phase 1: Implemented Effects](#phase-1-implemented-effects)
4. [Phase 2: New Effects Specifications](#phase-2-new-effects-specifications)
5. [Unity 6 Particle Setup](#unity-6-particle-setup)
6. [Audio System](#audio-system)
7. [Code Reference](#code-reference)
8. [Troubleshooting](#troubleshooting)

---

## 🚀 Quick Start

### Using the Weather System

**In Code:**
```csharp
// Get the weather manager
ENV_WeatherManager weather = FindFirstObjectByType<ENV_WeatherManager>();

// Turn on one weather effect
weather.SetWeather(ENV_WeatherManager.WeatherType.Rain);

// Layer multiple effects
weather.SetWeather(ENV_WeatherManager.WeatherType.Clouds, keepOthers: true);
weather.SetWeather(ENV_WeatherManager.WeatherType.Rain, keepOthers: true);

// Turn off all
weather.TurnOffAll();
```

**In Inspector (Testing):**
- Toggle `showClouds`, `showRain`, `showLeaves` etc. checkboxes
- Works in Edit mode and Play mode
- Smooth audio fades automatically

**With Trigger Zones:**
- Add `ENV_WeatherZone` script to any 2D trigger collider
- Set `weatherType` dropdown
- Player enters → weather changes automatically

---

## 🏗️ System Architecture

### Hybrid Approach
- **Global Manager** (`ENV_WeatherManager`) follows camera
- **Zone Triggers** (`ENV_WeatherZone`) for area-based control
- **Independent Audio** - 8 dedicated AudioSources (one per weather)

### File Structure
```
Assets/GAME/Scripts/Environment/
├── ENV_WeatherManager.cs      (Main controller - 221 lines)
├── ENV_WeatherZone.cs          (Trigger script - 46 lines)
└── FX/Particle/
    ├── Clouds.prefab           ✅ Phase 1
    ├── Rain.prefab             ✅ Phase 1
    ├── Leaf.prefab             ✅ Phase 1
    ├── Snow.prefab             🔄 Phase 2
    ├── Spark.prefab            🔄 Phase 2
    ├── Fire.prefab             🔄 Phase 2 (Static animation)
    ├── Smoke.prefab            🔄 Phase 2 (Static animation)
    └── (Fog - use Clouds variant) 🔄 Phase 2
```

### Scene Setup
```
Level1
└── WeatherController (ENV_WeatherManager)
    ├── Clouds (GameObject + Particle System)
    ├── Rain (GameObject + 2 child particles)
    ├── Leaves (GameObject + Particle System)
    ├── Snow (Not created yet)
    ├── Smoke (Not created yet)
    ├── Fog (Not created yet)
    ├── Spark (Not created yet)
    └── Raylight (Not created yet)
```

---

## ✅ Phase 1: Implemented Effects

### 1. Clouds (Static Drift + Fade)

**Godot Reference:**
- Texture: Single sprite
- Amount: Default
- Speed: 0.1 (very slow)
- Direction: (1, 1) gentle drift
- Velocity: 100 with 46% randomness
- Damping: 5.0 with 64% randomness
- Z-index: -1 (behind everything)
- Color: `rgba(26, 52, 59, 0.75)` dark blue-gray
- Fade: Gradient (0% → 100% → 100% → 0%)

**Unity Setup:**
- **Renderer:** Billboard + Sprite Default material
- **Emission:** Box shape, 300x200 area
- **Start Lifetime:** 30-40s
- **Start Speed:** 0.5-1.5
- **Start Size:** 3-6 (random between two constants)
- **Color over Lifetime:** Alpha 0 → 1 → 1 → 0 (fade in/out)
- **Velocity over Lifetime:** Linear (0.2, 0.2, 0) - slow drift
- **Limit Velocity over Lifetime:** Damping 0.3, separate axes
- **Sorting Order:** 10

**Key Features:**
- ✅ Size variation (3-6 units)
- ✅ Smooth fade in/out
- ✅ Gentle drift
- ✅ Stays within camera bounds

---

### 2. Rain (Falling + Splash)

**Godot Reference:**
- Texture: 3-frame animation
- Amount: 40 particles
- Speed: 2.0 (fast)
- Direction: (-0.5, 1) slight angle
- Velocity: 60
- Z-index: 1 (above ground)
- Fade: Gradient (0% → 100% → 100% → 0%)

**Unity Setup - Parent:**
```
PFX_Rain
├── RainDrops (Particle System)
└── RainSplash (Particle System)
```

**RainDrops:**
- **Renderer:** Billboard + Sprites mode + Texture Sheet Animation
- **Texture Sheet:** 3 columns, 1 row, random start frame
- **Emission:** Box 300x200, Rate 40
- **Start Lifetime:** 2-3s
- **Start Speed:** 8-12
- **Start Size:** 0.3-0.5
- **Gravity:** 2
- **Color over Lifetime:** Alpha fade
- **Sorting Order:** 5

**RainSplash:**
- **Trigger:** Sub Emitter on Death
- **Burst:** 3-5 particles
- **Start Lifetime:** 0.2-0.4s
- **Start Speed:** 1-3
- **Start Size:** 0.2-0.3
- **Spread:** Cone 45°
- **Sorting Order:** -1 (below drops)

---

### 3. Falling Leaves (Sway + Rotate)

**Godot Reference:**
- Texture: 6-frame animation
- Amount: Default
- Direction: (0, 1) straight down
- Velocity: 20-40
- Noise: Enabled for swaying
- Animation: Random start frame

**Unity Setup:**
- **Renderer:** Billboard + Sprites mode + 6-frame animation
- **Emission:** Box 300x200, Rate 8-12
- **Start Lifetime:** 8-12s
- **Start Speed:** 0.5-1.5
- **Start Size:** 0.4-0.8
- **Gravity:** 0.5
- **Noise:** Strength 0.5, Frequency 0.3, Scroll (0, 0.2) - ping-pong sway
- **Rotation over Lifetime:** 0-360° random
- **Limit Velocity over Lifetime:** Damping 0.2
- **Sorting Order:** 6

**Key Features:**
- ✅ Swaying motion (Noise module)
- ✅ Gentle rotation
- ✅ No escaping bounds

---

## 🔄 Phase 2: New Effects Specifications

### 4. Snow ❄️

**Type:** Weather effect (similar to Rain, no ground splash)

**Godot Reference:**
- Texture: 7-frame animation (`Snow.png` exists in Unity)
- Amount: 40 particles
- Speed: 0.5 (slow)
- Direction: (1, 1) gentle drift
- Velocity: 30 with 46% randomness
- Damping: 5.0 with 64% randomness
- Fade: Gradient (0% → 100% → 0%)
- Animation: Random start frame

**Unity Configuration:**
```yaml
Renderer:
  Mode: Billboard
  Material: Sprites/Default
  Texture Sheet Animation:
    Mode: Sprites
    Tiles: 7x1
    Start Frame: Random between 0-6

Emission:
  Shape: Box
  Size: 300x200
  Rate: 30-40

Particle Properties:
  Start Lifetime: 8-12s
  Start Speed: 1-2
  Start Size: 0.3-0.6
  Start Rotation: 0-360° (random)
  Gravity Modifier: 0.3 (very gentle fall)

Modules:
  Color over Lifetime:
    Alpha: 0 → 1 (0-20%) → 1 (20-80%) → 0 (80-100%)
  
  Velocity over Lifetime:
    Linear: (0.2, 0, 0) - gentle horizontal drift
  
  Noise:
    Strength: 0.3
    Frequency: 0.5
    Scroll Speed: (0.1, 0.1)
    Damping: On
  
  Limit Velocity over Lifetime:
    Speed: 3
    Damping: 0.3

Sorting Order: 7
```

**Differences from Rain:**
- ❌ No splash particles
- ✅ Slower fall speed
- ✅ More drift/sway
- ✅ Lighter gravity

---

### 5. Spark ⚡

**Type:** VFX effect (weapon impacts, high-tier items)
**NOT a weather effect** - Remove from WeatherType enum, make separate prefab

**Godot Reference:**
- Texture: 7-frame animation (`Spark.png` exists in Unity)
- Amount: 10 particles
- Direction: (0, -1) upward
- Velocity: 5 with 50% randomness
- Emission: 100x100 area
- Fade: Gradient (0% → 100% → 0%)
- Z-index: 1

**Unity Configuration:**
```yaml
Renderer:
  Mode: Billboard
  Material: Sprites/Default + Additive blend
  Texture Sheet Animation:
    Mode: Sprites
    Tiles: 7x1
    Start Frame: Random

Emission:
  Shape: Sphere
  Radius: 0.5
  Burst: 8-15 particles (one-shot)
  
Particle Properties:
  Duration: 1s
  Looping: False
  Start Lifetime: 0.3-0.8s
  Start Speed: 2-6
  Start Size: 0.1-0.3 (SMALL scale)
  Start Color: Yellow-Orange gradient
  Gravity Modifier: -0.5 (slight upward)

Modules:
  Color over Lifetime:
    RGB: White → Yellow → Orange
    Alpha: 0 → 1 (0-10%) → 1 (10-50%) → 0 (50-100%)
  
  Size over Lifetime:
    Curve: 1 → 0 (shrink)

Sorting Order: 15 (very high)
Play On Awake: False (trigger manually)
```

**Usage:**
```csharp
// Spawn at weapon impact point
GameObject spark = Instantiate(sparkPrefab, hitPoint, Quaternion.identity);
Destroy(spark, 1f); // Auto-cleanup
```

**Use Cases:**
- Sword clashing
- Critical hits
- Opening legendary chests
- Forging/crafting animations
- Electric attacks

---

### 6. Fog 🌫️

**Type:** Weather effect (modified Clouds)
**Implementation:** Use Clouds.prefab as base, modify properties

**Design Goals:**
- Closer to player (lower altitude)
- Much lower alpha (semi-transparent)
- Below clouds (z-order)
- Makes scene harder to see but not obscured

**Unity Configuration (Modified Clouds):**
```yaml
Renderer:
  Same as Clouds
  Sorting Order: -2 (below clouds)

Emission:
  Shape: Box
  Size: 300x100 (shorter vertical range - ground level)
  Rate: 20 (fewer than clouds)

Particle Properties:
  Start Lifetime: 20-30s
  Start Speed: 0.3-0.8 (slower than clouds)
  Start Size: 4-8 (larger than clouds)
  Start Color: rgba(255, 255, 255, 0.15) - VERY LOW ALPHA
  Position Y: -2 to +2 (near ground)

Modules:
  Color over Lifetime:
    Alpha: 0 → 0.15 → 0.15 → 0 (never fully opaque)
    RGB: Slight gray tint (240, 240, 245)
  
  Velocity over Lifetime:
    Linear: (0.1, 0, 0) - very slow drift
  
  Limit Velocity over Lifetime:
    Damping: 0.4
```

**Key Differences from Clouds:**
- ✅ Lower alpha (0.15 max vs 0.75)
- ✅ Closer to ground (Y: -2 to +2)
- ✅ Larger size but fewer particles
- ✅ Below clouds (sorting -2)
- ✅ Slower movement

**Effect:**
- Player can still see through (70-85% visibility)
- Adds atmospheric depth
- Works great with dark scenes/night time

---

### 7. Fire 🔥 (Static Animation)

**Type:** Prefab animation (NOT particle system)
**Purpose:** Campfires, torches, braziers

**Godot Reference:**
- None found, but `Fire.png` exists in Unity (check if it's a spritesheet)

**Unity Configuration:**
```yaml
GameObject: PFX_Fire
Components:
  - SpriteRenderer
  - Animator
  - AudioSource (looping crackling sound)

SpriteRenderer:
  Sprite: Fire spritesheet (check frame count)
  Sorting Order: 3
  Material: Sprites/Default or Additive

Animator:
  Animation Clip: Fire_Burn
    - 6-12 FPS (depends on spritesheet)
    - Looping: True
    - All frames in sequence

AudioSource:
  Clip: Fire crackling loop
  Loop: True
  Volume: 0.2-0.4
  Spatial Blend: 0.5 (2D/3D mix)
  Min Distance: 5
  Max Distance: 20

Optional - Particle System (sparks):
  Small upward sparks (5-10 particles)
  Rate: 2-4 per second
  Lifetime: 0.5-1s
  Size: 0.05-0.1
  Speed: 1-3 upward
```

**Prefab Variants:**
- `PFX_Fire_Small` - Torch size
- `PFX_Fire_Medium` - Campfire size
- `PFX_Fire_Large` - Bonfire size

**Usage:**
```csharp
// Place in scene, plays automatically
// GameObject is NOT destroyed (permanent fixture)
```

**Properties:**
- ✅ Indestructible (no health component)
- ✅ Always playing
- ✅ Own sound effect
- ✅ Can be placed anywhere in scene

---

### 8. Smoke 💨 (Static Animation tied to Fire)

**Type:** Prefab animation (plays with Fire)
**Purpose:** Rises from campfires, torches

**Godot Reference:**
- Texture: 6-frame animation (`/World/FX/Smoke/SpriteSheet.png`)
- Amount: 4 particles
- Speed: 0.5 (slow)
- Direction: (1, -100) upward with slight drift
- Velocity: 30
- Damping: 5.0
- Color: Gray `rgba(151, 151, 151, 1)`

**Unity Configuration - Option A (Particle System):**
```yaml
GameObject: PFX_Smoke
Parent: PFX_Fire (child object)

Renderer:
  Mode: Billboard
  Material: Sprites/Default
  Texture Sheet Animation:
    Spritesheet: 6 frames
    Mode: Sprites
    Tiles: 6x1

Emission:
  Shape: Circle
  Radius: 0.3
  Rate: 2-4

Particle Properties:
  Start Lifetime: 3-5s
  Start Speed: 0.5-1.5
  Start Size: 0.5 → 2 (grow over lifetime)
  Start Color: Gray rgba(151, 151, 151, 255)
  Gravity Modifier: -0.3 (upward)

Modules:
  Color over Lifetime:
    Alpha: 0.8 → 0 (fade out as it rises)
  
  Size over Lifetime:
    Curve: 0.5 → 2 (expand)
  
  Velocity over Lifetime:
    Linear: (0.2, 1, 0) - upward with slight drift
  
  Noise:
    Strength: 0.3
    Frequency: 0.5 (wispy motion)

Sorting Order: 8 (above fire)
Position: Local (0, 1, 0) - above fire source
```

**Unity Configuration - Option B (Animator - Simpler):**
```yaml
GameObject: PFX_Smoke
Parent: PFX_Fire

SpriteRenderer:
  Sprite: Smoke spritesheet (6 frames)
  Sorting Order: 8
  Color: rgba(151, 151, 151, 200) - semi-transparent

Animator:
  Animation: Smoke_Rise
    - 4-6 FPS
    - Looping: True

Transform Animation (in clip):
  Position Y: 0 → 3 (rise up over 2s, then reset)
  Scale: 0.5 → 1.5 (expand)
  Alpha: 0.8 → 0 (fade)
```

**Recommended:** Use Animator (Option B) for simplicity

**Hierarchy:**
```
PFX_Fire
├── Fire_Sprite (SpriteRenderer + Animator)
├── Fire_Audio (AudioSource)
└── PFX_Smoke (SpriteRenderer + Animator - loops continuously)
```

---

## 🎨 Unity 6 Particle Setup

### Critical Unity 6 Changes

**Renderer Configuration (REQUIRED):**
1. Open Particle System
2. Renderer module → Render Mode: **Billboard**
3. Material: **Sprites/Default** (NOT Particles/Standard)
4. Texture Sheet Animation:
   - Mode: **Sprites** (NOT Grid)
   - Tiles: Set X and Y (e.g., 3x1 for rain, 6x1 for leaves)
   - Frame over Time: Random Between Two Constants (0 and maxFrames)

**Common Mistakes:**
- ❌ Using "Grid" mode → Sprites won't show
- ❌ Using Particles/Standard material → Wrong blend mode
- ❌ Forgetting Billboard mode → Sprites face wrong direction

### Sprite Slicing (Multi-frame textures)

1. Select texture in Project
2. Inspector → Texture Type: **Sprite (2D and UI)**
3. Sprite Mode: **Multiple**
4. Click "Sprite Editor"
5. Slice → Grid By Cell Count:
   - Rain: 3 columns, 1 row
   - Leaves: 6 columns, 1 row
   - Snow: 7 columns, 1 row
   - Spark: 7 columns, 1 row
   - Smoke: 6 columns, 1 row
6. Apply → Close

### Particle Enhancements

**Cloud Fade In/Out:**
```
Color over Lifetime:
- Alpha Gradient: 0 → 1 (0-20%) → 1 (20-80%) → 0 (80-100%)
```

**Leaf Swaying:**
```
Noise Module:
- Strength: 0.5
- Frequency: 0.3
- Scroll Speed: (0, 0.2) 
- Damping: Yes
- Quality: High
```

**Size Variation:**
```
Start Size:
- Mode: Random Between Two Constants
- Min: 3
- Max: 6
```

**Keep Particles in Bounds:**
```
Limit Velocity over Lifetime:
- Speed: 3-5
- Damping: 0.2-0.3
- Separate Axes: Yes
```

---

## 🔊 Audio System

### Architecture: 8 Dedicated AudioSources

**One AudioSource per weather type** (created in `Awake()`):
- `cloudsAudio`
- `rainAudio`
- `leavesAudio`
- `snowAudio`
- `smokeAudio` (N/A - smoke uses Fire's audio)
- `fogAudio`
- `sparkAudio` (N/A - one-shot VFX, no weather loop)
- `raylightAudio`

### Audio Properties

```csharp
AudioSource CreateWeatherAudioSource()
{
    AudioSource source = gameObject.AddComponent<AudioSource>();
    source.loop        = true;
    source.playOnAwake = false;
    source.volume      = 0f; // Starts silent, fades in
    return source;
}
```

### Smooth Fade System

**Fade Duration:** 1.5s (0.75s out + 0.75s in)  
**Interpolation:** Smoothstep `t * t * (3f - 2f * t)`

**3-State Handling:**
1. **Already playing correct clip** → Fade volume to target
2. **Playing different clip** → Crossfade (fade out, swap, fade in)
3. **Not playing** → Start fresh and fade in

**Frame Delay (OnValidate):**
- Prevents rapid Inspector toggle conflicts
- `yield return null` before applying sound changes

**Skip Logic:**
- Don't restart sound if already at 90%+ volume
- Prevents hiccups during rapid toggles

### Inspector Toggle Testing

```csharp
void OnValidate()
{
    if (!Application.isPlaying) ApplyInspectorState(); // Visual only
    else if (gameObject.activeInHierarchy) StartCoroutine(ApplyInspectorStateNextFrame()); // Visual + Sound
}
```

**Works in:**
- ✅ Edit mode (visual only, no sound)
- ✅ Play mode (visual + smooth sound fades)
- ✅ Multiple simultaneous weathers

---

## 💻 Code Reference

### ENV_WeatherManager.cs (221 lines)

**Public API:**
```csharp
public enum WeatherType { None, Clouds, Rain, Leaves, Snow, Smoke, Fog, Spark, Raylight }

// Main methods
public void SetWeather(WeatherType weatherType, bool keepOthers = false)
public void TurnOffAll()
```

**Key Features:**
- Null-safe (won't error if GameObjects missing)
- Camera following (always follows `Camera.main`)
- Inspector testing (toggle bools in Play mode)
- Independent audio control

**Null Safety Example:**
```csharp
case WeatherType.Snow: if (snowGO) { ToggleEffect(snowGO, true); PlayWeatherSound(snowSound); } break;
```

### ENV_WeatherZone.cs (46 lines)

```csharp
public class ENV_WeatherZone : MonoBehaviour
{
    public ENV_WeatherManager.WeatherType weatherType;
    public bool keepPreviousWeather = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            weatherManager.SetWeather(weatherType, keepPreviousWeather);
        }
    }
}
```

**Usage:**
1. Add to GameObject with Collider2D (Trigger = true)
2. Set `weatherType` in Inspector
3. Optional: `keepPreviousWeather = true` to layer effects

---

## 🐛 Troubleshooting

### Problem: Particles not showing

**Solution:**
1. Check Renderer → Render Mode is **Billboard**
2. Material is **Sprites/Default**
3. Texture Sheet Animation → Mode is **Sprites**
4. Sorting Order is correct (not behind background)

---

### Problem: Console errors about missing GameObjects

**Solution:**
- This is normal! Only wire up the weathers you've created
- Code has null checks: `if (snowGO) { ... }`
- No errors should appear if code is correct

---

### Problem: Animation not playing

**Solution:**
1. Verify sprite is sliced correctly (Multiple mode)
2. Texture Sheet Animation → Tiles matches sprite grid
3. Frame Over Time: Random Between Two Constants (0 to maxFrames)
4. Check Start Frame: Random Between Two Constants

---

### Problem: Inspector toggle doesn't play sound

**Solution:**
1. Make sure you're in **Play mode** (Edit mode = visual only)
2. Check AudioClip is assigned in Inspector
3. Verify `weatherVolume > 0` in settings
4. Make sure GameObject is active in hierarchy

---

### Problem: Sound hiccups when toggling rapidly

**Solution:**
- Already fixed with frame delay system
- If still happening, user is toggling *extremely* fast
- This is expected behavior (fighting with fade system)

---

### Problem: Particles escape camera bounds

**Solution:**
Add **Limit Velocity over Lifetime:**
- Speed: 3-5
- Damping: 0.2-0.3
- Separate Axes: Yes

---

### Problem: Fog too opaque / can't see

**Solution:**
- Lower Start Color alpha (should be ~0.15)
- Reduce particle count (Rate: 15-20 max)
- Increase Color over Lifetime alpha curve max to 0.15

---

## 📊 Effect Comparison Table

| Effect | Type | Frames | Amount | Speed | Gravity | Special | Order |
|--------|------|--------|--------|-------|---------|---------|-------|
| **Clouds** | Weather | 1 | 30 | 0.5-1.5 | 0 | Fade, Size variation | 10 |
| **Rain** | Weather | 3 | 40 | 8-12 | 2.0 | Splash sub-emitter | 5 |
| **Leaves** | Weather | 6 | 8-12 | 0.5-1.5 | 0.5 | Noise sway, rotation | 6 |
| **Snow** | Weather | 7 | 30-40 | 1-2 | 0.3 | Drift, fade | 7 |
| **Fog** | Weather | 1 | 15-20 | 0.3-0.8 | 0 | Low alpha (0.15 max) | -2 |
| **Spark** | VFX | 7 | 8-15 burst | 2-6 | -0.5 | One-shot, additive | 15 |
| **Fire** | Static | 6-12 | N/A | N/A | N/A | Animator loop, audio | 3 |
| **Smoke** | Static | 6 | N/A | N/A | N/A | Animator, child of Fire | 8 |

---

## 🎯 Next Steps

### Phase 2 Workflow

For each new effect:

1. **Check Assets**
   - Verify texture exists in `Assets/GAME/Scripts/Environment/FX/Particle/`
   - If spritesheet, slice it (Sprite Editor → Grid)

2. **Create Prefab**
   - Right-click in Particle folder → Create → Particle System
   - Rename (e.g., `PFX_Snow`)
   - Configure using specs above

3. **Add to Scene**
   - Drag prefab under WeatherController GameObject
   - Turn off by default (disable GameObject)

4. **Wire in Inspector**
   - Select WeatherController
   - Drag GameObject to `snowGO` field
   - Drag AudioClip to `snowSound` field

5. **Test**
   - Play mode
   - Toggle `showSnow` checkbox
   - Verify visual + audio

6. **Create Zone (Optional)**
   - Add 2D BoxCollider to scene area
   - Set Trigger = true
   - Add `ENV_WeatherZone` script
   - Set `weatherType = Snow`

### Recommended Order

1. ✅ **Snow** - Easiest (like Rain minus splash)
2. ✅ **Fog** - Easy (duplicate Clouds, tweak alpha)
3. 🔧 **Spark** - Medium (needs separate VFX script, not weather)
4. 🔥 **Fire** - Medium (check if spritesheet, create animator)
5. 💨 **Smoke** - Medium (depends on Fire)

---

## 📝 Notes

- All weather effects follow camera automatically (ENV_WeatherManager handles this)
- Spark should probably be removed from WeatherType enum (it's a VFX, not weather)
- Fire and Smoke are static prefabs, not weather effects (place manually in scene)
- Consider creating `ENV_VFXManager` for non-weather effects (sparks, hits, etc.)

---

**Last Updated:** Phase 1 complete, Phase 2 specifications ready  
**File Count Reduced:** 13 separate docs → 1 comprehensive guide  
**Total Lines:** This file (~1,200 lines) vs Previous total (~8,000+ lines across 13 files)
