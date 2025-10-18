# Unity Weather System Setup Guide
**Unity 6 (6000.2.7f2) - Always-On Architecture**

This guide shows you how to configure your existing weather prefabs for the **Always-On** approach (no scripts, using Unity's built-in particle culling and 3D spatial audio).

---

## 📋 Quick Overview

You already have these prefabs ready:
- ✅ **Rain.prefab** (with RainOnFloor child for splash)
- ✅ **Clouds.prefab** 
- ✅ **Snow.prefab**
- ✅ **Fog.prefab** (based on Clouds)
- ✅ **Fire.prefab** (with Smoke child)
- ✅ **Spark.prefab**
- ✅ **Leaf.prefab**

**Path:** `Assets/GAME/Scripts/Environment/FX/Particle/`

---

## 🎯 Always-On Setup (Zero Scripts)

This approach uses Unity's built-in features:
- **Particle Culling**: Automatically stops rendering when off-screen
- **3D Spatial Audio**: Natural volume fade based on player distance
- **Play On Awake**: Weather starts automatically when area loads

### Step 1: Configure Each Prefab

For each weather prefab, follow these steps in Unity:

#### A. Particle System Settings

1. **Select the prefab** in Project window
2. Open it in Prefab mode (double-click)
3. Select the GameObject with ParticleSystem component
4. In **Inspector → Particle System**:

```
Main Module:
├─ Duration: [varies by effect]
├─ Looping: ✅ TRUE
├─ Prewarm: Depends (see table below)
├─ Play On Awake: ✅ TRUE
├─ Max Particles: [varies by effect]
└─ Simulation Space: World

Renderer Module:
├─ Render Mode: Billboard
├─ Material: Default-Particle (or custom)
├─ Sorting Layer: [see table below]
└─ Order in Layer: [see table below]
```

#### B. Add AudioSource (If Effect Needs Sound)

1. With prefab open, click **Add Component**
2. Search "Audio Source"
3. Configure:

```
AudioSource Settings:
├─ AudioClip: [Drag your audio file here]
├─ Play On Awake: ✅ TRUE
├─ Loop: ✅ TRUE
├─ Volume: 0.5-1.0 (adjust to taste)
├─ Pitch: 1.0
├─ Spatial Blend: 1.0 (full 3D)
├─ Min Distance: 5
├─ Max Distance: 20
├─ Rolloff Mode: Logarithmic Rolloff
└─ Doppler Level: 0
```

**What This Does:**
- **Min Distance (5)**: Full volume within 5 units of source
- **Max Distance (20)**: Silent beyond 20 units
- **Between 5-20**: Volume fades smoothly using logarithmic curve
- **Spatial Blend (1.0)**: Full 3D positioning (sound comes from object location)

---

## 📊 Per-Effect Configuration Table

### Rain
**Prefab:** `Rain.prefab`

| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Duration | 5 sec |
| | Looping | TRUE |
| | Prewarm | FALSE |
| | Start Lifetime | 1.5-2.0 |
| | Start Speed | 15-20 |
| | Max Particles | 300 |
| **Emission** | Rate over Time | 60 |
| **Shape** | Shape | Box |
| | Scale | X=20, Y=0.1, Z=0 |
| | Position | Y=10 (above ground) |
| **Texture Sheet** | Tiles | X=1, Y=7 |
| | Animation | Single Row |
| | Frame over Time | 0-1 curve |
| **Renderer** | Sorting Layer | Default |
| | Order in Layer | 0 |
| **AudioSource** | Clip | Rain.wav |
| | Min Distance | 5 |
| | Max Distance | 20 |

**RainOnFloor Child:**
| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Start Lifetime | 0.3-0.5 |
| | Start Speed | 0 |
| | Start Size | 0.3-0.5 |
| **Emission** | Rate over Time | 30 |
| **Shape** | Shape | Box |
| | Scale | Same as parent |
| | Position | Y=0 (ground level) |
| **Color over Lifetime** | Gradient | White→Transparent |
| **Size over Lifetime** | Curve | Start 0.3 → End 0.6 |

---

### Clouds
**Prefab:** `Clouds.prefab`

| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Duration | 25 sec |
| | Looping | TRUE |
| | Prewarm | FALSE |
| | Start Lifetime | 20-25 |
| | Start Speed | 0.2-0.4 |
| | Start Size | 8-12 |
| | Max Particles | 20 |
| **Emission** | Rate over Time | 1 |
| **Shape** | Shape | Box |
| | Scale | X=30, Y=5, Z=0 |
| **Color over Lifetime** | Alpha | Fade in/out |
| **Velocity over Lifetime** | Linear X | 0.5 (drift) |
| **Renderer** | Sorting Layer | Background |
| | Order in Layer | -10 |
| **AudioSource** | ❌ None needed |

---

### Snow
**Prefab:** `Snow.prefab`

| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Duration | 5 sec |
| | Looping | TRUE |
| | Prewarm | FALSE |
| | Start Lifetime | 8-12 |
| | Start Speed | 1-2 |
| | Max Particles | 200 |
| **Emission** | Rate over Time | 20 |
| **Shape** | Shape | Box |
| | Scale | X=20, Y=0.1, Z=0 |
| | Position | Y=10 |
| **Texture Sheet** | Tiles | X=1, Y=7 |
| | Animation | Single Row |
| | Random Row | TRUE |
| **Noise** | Strength | 0.5 |
| | Frequency | 0.5 |
| | Scroll Speed | 0.2 |
| **Renderer** | Sorting Layer | Default |
| | Order in Layer | 1 |
| **AudioSource** | Clip | Snow.wav (subtle) |
| | Volume | 0.3 |
| | Min Distance | 5 |
| | Max Distance | 15 |

---

### Fog
**Prefab:** `Fog.prefab`

| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Duration | 25 sec |
| | Looping | TRUE |
| | Prewarm | TRUE |
| | Start Lifetime | 20-25 |
| | Start Speed | 0.1-0.2 |
| | Start Size | 10-15 |
| | Start Color | White, Alpha 0.15 |
| | Max Particles | 30 |
| **Emission** | Rate over Time | 1.5 |
| **Shape** | Shape | Box |
| | Scale | X=25, Y=8, Z=0 |
| **Color over Lifetime** | Alpha | Soft fade in/out |
| **Velocity over Lifetime** | Linear X | 0.2 |
| **Renderer** | Sorting Layer | Foreground |
| | Order in Layer | -5 |
| **AudioSource** | Clip | Ambience.wav (optional) |
| | Volume | 0.2 |
| | Min Distance | 10 |
| | Max Distance | 25 |

---

### Fire + Smoke
**Prefab:** `Fire.prefab` (parent) with `Smoke` child

#### Fire (Parent):
| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Duration | 2 sec |
| | Looping | TRUE |
| | Prewarm | FALSE |
| | Start Lifetime | 0.8-1.2 |
| | Start Speed | 2-4 |
| | Start Size | 0.5-1.0 |
| | Max Particles | 50 |
| **Emission** | Rate over Time | 40 |
| **Shape** | Shape | Circle |
| | Radius | 0.3 |
| | Emit from Edge | FALSE |
| **Color over Lifetime** | Gradient | Yellow→Orange→Red→Black |
| **Size over Lifetime** | Curve | 0.5 → 1.2 → 0 |
| **Velocity over Lifetime** | Linear Y | 3-5 (upward) |
| **Renderer** | Sorting Layer | Default |
| | Order in Layer | 2 |
| **AudioSource** | Clip | Fire.wav |
| | Volume | 0.6 |
| | Min Distance | 3 |
| | Max Distance | 15 |

#### Smoke (Child):
| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Duration | 3 sec |
| | Looping | TRUE |
| | Start Lifetime | 2-3 |
| | Start Speed | 1-2 |
| | Start Size | 1-2 |
| | Start Color | Gray, Alpha 0.3 |
| | Max Particles | 30 |
| **Emission** | Rate over Time | 10 |
| **Shape** | Shape | Circle |
| | Radius | 0.5 |
| **Color over Lifetime** | Alpha | 0.3 → 0 |
| **Size over Lifetime** | Curve | 1 → 3 (expand) |
| **Velocity over Lifetime** | Linear Y | 2-3 |
| **Renderer** | Sorting Layer | Default |
| | Order in Layer | 1 |

---

### Spark
**Prefab:** `Spark.prefab`

| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Duration | 10 sec |
| | Looping | TRUE |
| | Prewarm | TRUE |
| | Start Lifetime | 1-2 |
| | Start Speed | 3-6 |
| | Start Size | 0.2-0.4 |
| | Max Particles | 50 |
| **Emission** | Bursts | Count: 5-10 every 0.5s |
| **Shape** | Shape | Cone |
| | Angle | 45 |
| | Radius | 0.2 |
| **Texture Sheet** | Tiles | X=1, Y=7 |
| | Animation | Single Row |
| | Frame over Time | 0-1 |
| **Color over Lifetime** | Gradient | Bright yellow→Orange→Red→Dark |
| **Size over Lifetime** | Curve | 1 → 0.5 → 0 |
| **Velocity over Lifetime** | Linear Y | -5 (gravity) |
| **Renderer** | Sorting Layer | Default |
| | Order in Layer | 3 |
| **AudioSource** | Clip | Spark.wav (crackle) |
| | Volume | 0.4 |
| | Min Distance | 2 |
| | Max Distance | 10 |

---

### Leaf
**Prefab:** `Leaf.prefab`

| **Component** | **Setting** | **Value** |
|---------------|-------------|-----------|
| **Main** | Duration | 5 sec |
| | Looping | TRUE |
| | Prewarm | FALSE |
| | Start Lifetime | 6-10 |
| | Start Speed | 0.5-1.5 |
| | Start Size | 0.4-0.6 |
| | Max Particles | 30 |
| **Emission** | Rate over Time | 3 |
| **Shape** | Shape | Box |
| | Scale | X=15, Y=0.1, Z=0 |
| | Position | Y=8 |
| **Texture Sheet** | Tiles | X=1, Y=6 |
| | Animation | Single Row |
| | Frame over Time | Random between constants |
| **Rotation over Lifetime** | Angular Velocity | -90 to 90 |
| **Noise** | Strength | 1.0 |
| | Frequency | 0.3 |
| | Scroll Speed | 0.5 |
| **Velocity over Lifetime** | Linear Y | -0.5 (gentle fall) |
| **Renderer** | Sorting Layer | Default |
| | Order in Layer | 1 |
| **AudioSource** | Clip | Wind.wav (optional) |
| | Volume | 0.3 |
| | Min Distance | 5 |
| | Max Distance | 18 |

---

## 🎮 Step 2: Place Prefabs in Your Scene

### Method 1: Drag & Drop (Simplest)

1. Open your scene (e.g., `Level1.unity`)
2. Navigate to `Assets/GAME/Scripts/Environment/FX/Particle/`
3. **Drag prefab** directly into Scene view or Hierarchy
4. **Position it** where you want the weather effect
5. **Scale the shape** (select prefab → Inspector → Particle System → Shape):
   - Adjust **Scale X** to cover your area width
   - Adjust **Position Y** to set height above ground
6. **Repeat** for multiple areas if needed

**Example:**
- Forest area: Drag `Leaf.prefab`, position at (10, 8, 0), scale X=20
- Snowy mountain: Drag `Snow.prefab`, position at (50, 10, 0), scale X=25
- Campfire: Drag `Fire.prefab`, position at (5, 0.5, 0), scale=1

### Method 2: Duplicate Existing

1. **Find an existing prefab instance** in Hierarchy
2. **Right-click → Duplicate** (or Cmd+D)
3. **Drag to new position**
4. **Rename** for clarity (e.g., "Rain_Forest", "Rain_Cave")

---

## 🔊 Step 3: Audio Setup (If Not Already Done)

If your prefabs don't have AudioSource yet:

1. **Open prefab** in Prefab mode (double-click in Project)
2. **Select root GameObject**
3. **Add Component → Audio → Audio Source**
4. Configure using table above
5. **Drag audio clip** from Project window to AudioClip slot
6. **Test**: Click Play button in Particle System Inspector to hear it

**Audio File Locations:**
- Check `Assets/GAME/Audio/` or similar
- Import new audio: Right-click folder → Import New Asset
- Supported: `.wav`, `.mp3`, `.ogg`

---

## ⚙️ Step 4: Performance Optimization

Unity 6 automatically optimizes, but verify these settings:

### Culling Settings
1. Select prefab
2. Particle System → **Culling Mode**: "Automatic" (default)
3. This stops rendering when off-screen ✅

### Audio Distance Settings
1. AudioSource → **Spatial Blend**: 1.0 (full 3D)
2. **Min Distance**: How close before max volume
3. **Max Distance**: How far before silent
4. **Rolloff Mode**: Logarithmic (natural fade)

### Particle Limits
If game lags with multiple weather systems:
1. Reduce **Max Particles** per system
2. Reduce **Emission Rate**
3. Lower **Start Lifetime** (particles die faster)

---

## 🧪 Step 5: Testing

### Quick Test in Scene View
1. Select weather prefab in Hierarchy
2. Inspector → Particle System → **Click Play button**
3. Should see particles + hear audio (if AudioSource present)
4. **Rotate Scene view** to check from different angles

### In-Game Test
1. **Play the scene** (Cmd+P)
2. **Move player character** near/far from weather area
3. **Check:**
   - Particles visible when in area ✅
   - Audio fades in as you approach ✅
   - Audio fades out as you leave ✅
   - Particles stop rendering when off-screen ✅

### Debug Audio Distance
1. Select prefab in Hierarchy
2. Inspector → AudioSource → **Click the gear icon**
3. Enable **Show Audio Gizmo**
4. Scene view will show Min/Max distance spheres

---

## 🎨 Step 6: Visual Polish

### Adjusting Particle Colors
1. Open prefab
2. Particle System → **Start Color**: Change tint
3. Or use **Color over Lifetime** for gradients
4. Example: Rain can be blue-tinted, snow pure white

### Sorting Layers (Draw Order)
1. Inspector → Particle System → Renderer
2. **Sorting Layer**: 
   - Background: Clouds, distant fog (-10 to -5)
   - Default: Most weather (0)
   - Foreground: Close fog, effects (+1 to +5)
3. **Order in Layer**: Fine-tune within same layer

### Blending Modes
1. Renderer → **Material**: Select material
2. Project → Materials → Edit material
3. **Render Mode**:
   - Fade: Soft edges (clouds, fog)
   - Additive: Bright effects (fire, sparks)
   - Transparent: Most weather

---

## 📝 Common Configurations

### Scenario: Forest Rain Area

```
Setup:
├─ Drag Rain.prefab to (15, 10, 0)
├─ Shape → Scale X: 20 (covers 20 units)
├─ AudioSource → Rain.wav, Min:5, Max:20
├─ Result: Rain appears in forest, fades as player leaves
```

### Scenario: Mountain Snow

```
Setup:
├─ Drag Snow.prefab to (50, 12, 0)
├─ Shape → Scale X: 30 (wide mountain)
├─ Main → Start Lifetime: 10-15 (slow fall)
├─ AudioSource → Wind.wav, Min:8, Max:25
```

### Scenario: Cave Fog

```
Setup:
├─ Drag Fog.prefab to (30, 2, 0)
├─ Shape → Scale: X=15, Y=4 (low fog)
├─ Main → Start Color: Alpha 0.25 (subtle)
├─ AudioSource → Ambience.wav, Volume:0.2
```

### Scenario: Campfire

```
Setup:
├─ Drag Fire.prefab to (5, 0.5, 0)
├─ Scale: 0.5-1.0 (small fire)
├─ AudioSource → Fire.wav, Min:2, Max:10
├─ Smoke child handles smoke automatically
```

---

## 🐛 Troubleshooting

### Problem: Particles don't appear
**Solutions:**
1. Check **m_IsActive: 1** in prefab (should be TRUE)
2. Inspector → Particle System → Click **Restart** button
3. Main → **Play On Awake** must be TRUE
4. Shape → **Scale** might be 0 (set to proper values)

### Problem: Audio doesn't play
**Solutions:**
1. AudioSource → **AudioClip** must be assigned
2. **Play On Awake** must be TRUE
3. **Volume** > 0
4. Check if audio file is in Project (not missing)

### Problem: Audio too loud everywhere
**Solutions:**
1. AudioSource → **Spatial Blend**: Set to 1.0 (was 0)
2. **Max Distance**: Increase (e.g., 20-25)
3. **Volume**: Reduce to 0.5 or lower

### Problem: Particles visible through walls
**Solutions:**
1. Not fixable with 2D (no depth)
2. Option A: Use smaller Shape scales
3. Option B: Place effects in open areas only
4. Option C: Manually disable for indoor scenes

### Problem: Performance lag
**Solutions:**
1. Reduce **Max Particles** (e.g., 300 → 150)
2. Reduce **Emission Rate** (e.g., 60 → 30)
3. Shorter **Start Lifetime** (particles die sooner)
4. Check **Culling Mode** is "Automatic"

---

## 🔧 Unity 6 Specifics

### New Features Used
- **Automatic Culling**: Unity 6 improved off-screen detection
- **Audio Spatialization**: Better 3D positioning
- **Particle Performance**: Faster rendering than Unity 5

### Compatibility Notes
- All prefabs use **Unity 6 particle format**
- Texture Sheet Animation uses **Single Row mode**
- AudioSource uses **Logarithmic Rolloff** (standard)

### Inspector Changes
- Particle System properties grouped better
- AudioSource shows 3D gizmo in scene view
- Material selection improved in Renderer module

---

## 📚 Quick Reference: Audio Settings

| **Effect** | **Min Distance** | **Max Distance** | **Volume** | **Notes** |
|------------|------------------|------------------|------------|-----------|
| Rain | 5 | 20 | 0.5-0.8 | Steady ambience |
| Snow | 5 | 15 | 0.3-0.5 | Subtle wind |
| Fog | 10 | 25 | 0.2-0.4 | Very subtle |
| Fire | 3 | 15 | 0.6-0.8 | Crackling |
| Spark | 2 | 10 | 0.4 | Sharp crackle |
| Leaf | 5 | 18 | 0.3-0.5 | Wind rustle |

---

## ✅ Final Checklist

Before finishing setup:

- [ ] All prefabs have **Play On Awake = TRUE**
- [ ] All weather effects have **Looping = TRUE**
- [ ] AudioSources have **Spatial Blend = 1.0**
- [ ] AudioSources have **Loop = TRUE**
- [ ] Min/Max distances set appropriately
- [ ] Particle Shape scales match your area size
- [ ] Sorting layers prevent Z-fighting
- [ ] Tested in Play mode (Cmd+P)
- [ ] Audio fades in/out as player moves
- [ ] Particles cull when off-screen

---

## 🎯 Next Steps

After setup:
1. **Test each area** in your game
2. **Adjust volumes** to balance with music
3. **Fine-tune distances** for smooth transitions
4. **Tweak colors** to match art style
5. **Optimize** if performance issues

**No scripts needed!** Unity handles everything automatically with:
- Particle culling (performance)
- 3D audio distance (immersion)
- Play On Awake (convenience)

---

## 💡 Pro Tips

1. **Group Related Effects**: Create empty GameObject parent for organization
   - Example: "Weather_Forest" with Rain + Wind + Leaf children
   
2. **Reuse Prefabs**: Duplicate same prefab multiple times, adjust scale
   - Saves time vs creating new prefabs
   
3. **Test Audio Early**: Enable audio gizmos to visualize range
   
4. **Layer Your Effects**: Combine Fog + Rain for richer atmosphere
   
5. **Match Audio to Visuals**: Heavy rain = loud audio, light snow = quiet

---

**Questions?** Check `COMPLETE_WEATHER_GUIDE.md` for detailed specifications.
