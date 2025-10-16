# Unity Particle System Guide for Destructibles

**Purpose:** This guide explains every Particle System setting we configured for the break effect in `ENV_Destructible`. Understanding these settings will help you tweak particle effects yourself.

---

## 📋 Overview

**What we created:** A particle burst effect that spawns 6 break pieces when a pot/crate is destroyed.

**Key behavior:**
- Instant burst (not continuous)
- Particles scatter in a cone
- Each particle shows a different break sprite
- Particles fade out over time
- No gravity (top-down 2D game)

---

## 🎛️ Module Settings Breakdown

### **1. Main Module** (Core Settings)

#### **Duration: 0.5**
- **What it is:** How long the particle system runs before stopping
- **Our value:** 0.5 seconds (half a second)
- **Why:** Short duration for a quick burst effect
- **Effect:** System stops emitting after 0.5s, but particles continue living based on their lifetime

#### **Looping: OFF** ☐
- **What it is:** Whether the system repeats after Duration ends
- **Our value:** OFF (unchecked)
- **Why:** We want ONE burst when pot breaks, not continuous emission
- **Effect:** System plays once then stops

#### **Start Lifetime: 1.5**
- **What it is:** How long each particle exists after being spawned
- **Our value:** 1.5 seconds
- **Why:** Enough time to see particles scatter and fade
- **Effect:** Each particle lives for 1.5s before disappearing
- **Tuning tip:** Increase = particles travel farther before fading; Decrease = quicker fade

#### **Start Speed: 20-30**
- **What it is:** Initial velocity when particles spawn
- **Our value:** 20-30 (was 100, too fast!)
- **Why:** Controls scatter distance - lower = tighter spread
- **Effect:** Particles shoot out at this speed then slow down
- **Tuning tip:** 10-20 = small scatter; 30-50 = medium; 50+ = flies off-screen

#### **Start Size: Random Between 0.3 - 0.6**
- **What it is:** Size of each particle in Unity units
- **Our value:** 0.3 to 0.6 (randomized per particle)
- **Why:** Pot sprite is 0.875 units, pieces should be ~half size
- **Effect:** Each particle spawns with random size in this range
- **Tuning tip:** Match your sprite size - smaller sprites = smaller particles

#### **Start Rotation: Random Between 0° - 360°**
- **What it is:** Initial rotation angle of particles
- **Our value:** 0-360 (fully random)
- **Why:** Makes pieces tumble naturally
- **Effect:** Each particle starts with random rotation
- **Tuning tip:** Keep this for variety

#### **Gravity Modifier: 0**
- **What it is:** How much gravity affects particles (0 = none, 1 = normal)
- **Our value:** 0
- **Why:** Top-down 2D game has no gravity
- **Effect:** Particles don't "fall" downward
- **Tuning tip:** Side-scrollers use 0.3-1.0; Top-down uses 0

#### **Simulation Space: World**
- **What it is:** Whether particles move relative to system (Local) or world (World)
- **Our value:** World
- **Why:** Pot GameObject destroys, but particles should stay in place
- **Effect:** Particles exist independently after spawning
- **Important:** Local = particles die when GameObject destroys; World = particles persist

#### **Play On Awake: ON** ☑
- **What it is:** Auto-play when instantiated
- **Our value:** ON (checked)
- **Why:** Script instantiates prefab → particles play immediately
- **Effect:** No need to call `Play()` in code

---

### **2. Emission Module** (How Many Particles)

#### **Rate over Time: 0**
- **What it is:** Continuous emission rate (particles per second)
- **Our value:** 0
- **Why:** We use Bursts instead (instant spawn, not gradual)
- **Effect:** No continuous emission

#### **Bursts: 1 Burst**
- **Time: 0**
  - **What it is:** When burst happens (seconds after system starts)
  - **Our value:** 0 (instant)
  - **Why:** Spawn all particles immediately
  
- **Count: 6**
  - **What it is:** Number of particles spawned in this burst
  - **Our value:** 6 particles
  - **Why:** 6 different break piece sprites
  - **Tuning tip:** More particles = denser effect; Fewer = cleaner
  
- **Cycles: 1**
  - **What it is:** How many times burst repeats
  - **Our value:** 1 (once)
  - **Why:** Single burst event
  
- **Probability: 1.0**
  - **What it is:** Chance burst happens (0-1)
  - **Our value:** 1.0 (100%)
  - **Why:** Always spawn particles

**Summary:** Spawn 6 particles at time 0, once, guaranteed.

---

### **3. Shape Module** (Spawn Direction)

#### **Shape: Cone**
- **What it is:** Emission shape/direction
- **Our value:** Cone
- **Why:** Particles scatter in a directional spread (like explosion)
- **Alternatives:** Sphere = 360°; Circle = outward ring; Box = rectangular area

#### **Angle: 45°**
- **What it is:** Cone's opening angle
- **Our value:** 45 degrees
- **Why:** Narrow cone = directional scatter (matches Godot's "spread")
- **Effect:** Particles shoot within 45° cone
- **Tuning tip:** 0° = straight line; 90° = wide fan; 180° = hemisphere; 360° = sphere

#### **Radius: 0.1**
- **What it is:** Base size of cone's spawn area
- **Our value:** 0.1 (small)
- **Why:** Spawn near pot center (tight origin)
- **Effect:** All particles start close together
- **Tuning tip:** Larger radius = particles spawn more spread out

#### **Emit from: Base**
- **What it is:** Where on cone particles spawn
- **Our value:** Base (bottom of cone)
- **Why:** Single spawn point
- **Alternatives:** Volume = spawn throughout cone shape

---

### **4. Color over Lifetime Module** (Fade Effect)

#### **Alpha Gradient:**
- **What it is:** Transparency curve over particle's lifetime
- **Our value:**
  - **0.0s (Start): Alpha 1.0** (fully visible)
  - **0.7s (70%): Alpha 1.0** (stay visible)
  - **1.0s (End): Alpha 0.0** (fully transparent)
- **Why:** Particles fade out at the end instead of instant disappear
- **Effect:** Smooth fade in last 30% of lifetime
- **Tuning tip:** Fade earlier = particles disappear sooner; Keep visible longer = more noticeable effect

**How to read the gradient:**
- Left side = Start of particle life (time 0)
- Right side = End of particle life (time 1.0 = Start Lifetime)
- Vertical = Alpha (0 = invisible, 1 = solid)

---

### **5. Texture Sheet Animation Module** (Sprite Management)

**Purpose:** Show different sprites per particle and control animation.

#### **Mode: Sprites**
- **What it is:** Use individual sprite list (not sprite sheet grid)
- **Our value:** Sprites mode
- **Why:** We have 6 separate break piece sprites
- **Alternatives:** Grid = use sprite sheet with tiles

#### **Frame over Time: Constant 0**
- **What it is:** Controls sprite animation over particle lifetime
- **Our value:** Flat curve at 0 (NO animation)
- **Why:** Each particle keeps its sprite (doesn't cycle through frames)
- **Effect:** Particle's sprite is locked
- **What we fixed:** Was animating 0→1, causing sprites to change continuously

#### **Start Frame: Random Between 0 - 0.9999**
- **What it is:** Which sprite each particle starts with
- **Our value:** Random 0-6 (0.9999 = sprite index 5, Unity's quirk)
- **Why:** Each of the 6 particles gets a different break piece
- **Effect:** Variety - not all particles show same sprite
- **How it works:** 0-0.16 = sprite 0, 0.17-0.33 = sprite 1, etc.

#### **Sprites List: 6 sprites**
- **What it is:** Array of sprites particles can use
- **Our value:** 6 break piece sprites from pot sprite sheet
- **Why:** Different shapes for visual variety
- **Effect:** Particles randomly show one of these sprites
- **Tuning tip:** Add more sprites = more variety; Remove = less

**Summary:** Each particle gets a random sprite from the list and keeps it (no animation).

---

### **6. Renderer Module** (Display Settings)

#### **Render Mode: Billboard**
- **What it is:** How particle faces camera
- **Our value:** Billboard (always faces camera)
- **Why:** 2D game - sprites should face forward
- **Effect:** Particles rotate to face camera regardless of their angle
- **Alternatives:** Stretched = elongated; Mesh = 3D model

#### **Material: Sprites/Default**
- **What it is:** Shader/material for rendering
- **Our value:** Default sprite material
- **Why:** Standard 2D sprite rendering
- **Effect:** Particles render like normal sprites

#### **Sorting Layer: Default**
- **What it is:** 2D rendering order layer
- **Our value:** Default (0)
- **Why:** Render with environment objects
- **Tuning tip:** Change if particles appear behind/in front of wrong objects

#### **Sorting Order: 0**
- **What it is:** Order within Sorting Layer
- **Our value:** 0
- **Why:** Render at same depth as broken pot
- **Tuning tip:** Higher = in front; Lower = behind

---

## 🎯 Quick Settings Summary Table

| Module | Setting | Value | Purpose |
|--------|---------|-------|---------|
| **Main** | Duration | 0.5s | System runtime |
| | Looping | OFF | One-time burst |
| | Start Lifetime | 1.5s | Particle duration |
| | Start Speed | 20-30 | Scatter velocity |
| | Start Size | 0.3-0.6 | Piece size |
| | Gravity Modifier | 0 | Top-down (no fall) |
| | Simulation Space | World | Persist after destroy |
| **Emission** | Rate over Time | 0 | No continuous |
| | Burst Count | 6 | Number of pieces |
| **Shape** | Shape | Cone | Directional scatter |
| | Angle | 45° | Spread width |
| | Radius | 0.1 | Spawn tightness |
| **Color over Lifetime** | Alpha End | 0.0 | Fade out effect |
| **Texture Sheet Animation** | Frame over Time | 0 (flat) | No animation |
| | Start Frame | Random 0-6 | Different sprites |
| | Sprites | 6 pieces | Variety |
| **Renderer** | Render Mode | Billboard | Face camera |
| | Sorting Layer | Default | Render order |

---

## 🔧 Common Tuning Scenarios

### **"Particles fly too far"**
- ⬇️ Decrease **Start Speed** (Main Module)
- ⬇️ Decrease **Start Lifetime** (Main Module)

### **"Too few/many pieces"**
- Adjust **Burst Count** (Emission Module)

### **"Pieces are too big/small"**
- Adjust **Start Size** (Main Module)
- Match your sprite size (pot = 0.875, pieces = ~0.4)

### **"Scatter is too wide/narrow"**
- Adjust **Angle** (Shape Module)
- 30° = tight; 90° = wide; 180° = hemisphere

### **"Particles fade too quickly"**
- Adjust **Alpha Gradient** (Color over Lifetime)
- Move fade-to-0 keyframe closer to 1.0 (end)

### **"Sprites keep changing"**
- **Frame over Time** must be flat at 0 (Texture Sheet Animation)
- Check curve is horizontal, not sloped

### **"All particles show same sprite"**
- **Start Frame** should be Random (not Constant)
- Set to Random Between 0 - 0.9999

### **"Particles disappear when pot destroys"**
- **Simulation Space** must be World (not Local)

---

## 📐 How Values Scale

### **For Different Sprite Sizes:**

If your pot is **1.0 units** (instead of 0.875):
- Start Size: 0.35 - 0.65 (scale proportionally)
- Start Speed: Same (20-30)
- Shape Angle: Same (45°)

If your pot is **2.0 units** (large):
- Start Size: 0.6 - 1.2 (double)
- Start Speed: 40-60 (increase for larger scatter)
- Shape Angle: Same (45°)

### **For Different Game Speeds:**

Slower-paced game:
- Start Lifetime: 2.0-3.0s (longer visible)
- Start Speed: 10-20 (slower scatter)

Fast-paced game:
- Start Lifetime: 0.5-1.0s (quick disappear)
- Start Speed: 30-50 (rapid scatter)

---

## 🎨 Visual Debugging Tips

### **Can't see particles?**
1. Check **Play On Awake** is ON
2. Check **Start Lifetime** > 0
3. Check **Burst Count** > 0
4. Check **Looping** is OFF (or ON for testing)
5. Switch to **Scene View** (particles might be off-camera)
6. Check **Simulation Space** (World vs Local)

### **Particles look wrong?**
1. **Too big:** Lower Start Size
2. **Too fast:** Lower Start Speed
3. **Wrong direction:** Check Shape settings
4. **Fade wrong:** Check Color over Lifetime alpha
5. **Sprites changing:** Frame over Time must be 0

### **Test Mode:**
1. Enable **Looping** temporarily
2. Place particle system in scene (not prefab)
3. Press Play → particles continuously spawn
4. Tweak settings in real-time
5. Disable Looping when done

---

## 🚀 Next Steps

**To create variants:**
1. Duplicate `BreakParticles` prefab
2. Change **Sprites List** (different pieces)
3. Adjust **Burst Count** (more/fewer pieces)
4. Tweak **Color over Lifetime** (different fade)
5. Assign to different destructibles (wood vs ceramic)

**Advanced effects:**
- Add **Size over Lifetime** module (shrink as fade)
- Add **Rotation over Lifetime** module (spinning pieces)
- Add **Velocity over Lifetime** module (gravity-like fall)
- Use **Sub Emitters** module (spawn dust on death)

---

## 📚 Module Reference Quick Guide

**Core Modules (Always Use):**
- Main = Particle behavior
- Emission = How many spawn
- Shape = Where/how they emit

**Visual Modules (Common):**
- Color over Lifetime = Fade effects
- Size over Lifetime = Grow/shrink
- Rotation over Lifetime = Spinning

**Advanced Modules (Optional):**
- Velocity over Lifetime = Change speed
- Force over Lifetime = Wind/gravity
- Noise = Turbulence
- Collision = Bounce off objects
- Sub Emitters = Chain effects

**Rendering (Required):**
- Renderer = How it draws
- Texture Sheet Animation = Sprite management

---

**Remember:** Particle systems are all about iteration. Change one value, test, repeat. Start with our settings and tweak from there! 🎨
