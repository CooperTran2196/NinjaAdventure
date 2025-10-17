# Unity 6 Particle System - Sprite Setup Quick Guide
**Created:** October 17, 2025  
**Unity Version:** 6000.2.7f2  
**Purpose:** How to configure sprites in Unity 6 Particle System

---

## 🎯 The Correct Workflow (Unity 6)

### **Step 1: Slice Your Sprite Sheet (if animated)**

**For single sprites (Clouds):**
- No slicing needed
- Use sprite as-is

**For animated sprites (Rain, Leaf):**
1. Select sprite in Project (e.g., `Rain.png`)
2. Inspector → **Sprite Mode: Multiple**
3. Click **Sprite Editor** button
4. Click **Slice** dropdown → Choose:
   - **Automatic** (Unity detects frames)
   - **Grid By Cell Count** (manual: 3 columns, 1 row for Rain)
5. Click **Apply** in Sprite Editor
6. Close Sprite Editor
7. ✅ You now see child sprites: `Rain_0`, `Rain_1`, `Rain_2`

---

### **Step 2: Configure Particle System Modules**

#### **A. Texture Sheet Animation Module** (This assigns the sprites!)

**For STATIC sprites (Clouds):**
```
☑ Enable Texture Sheet Animation
Mode: Sprites                        ← Use this, NOT Grid!
Frame over Time: Constant 0          ← No animation
Start Frame: Constant 0              ← Always first sprite
Sprites: Size 1
  Element 0: [Drag Clouds.png here]  ← Drag from Project window
```

**For ANIMATED sprites (Rain, Leaf):**
```
☑ Enable Texture Sheet Animation
Mode: Sprites                        ← NOT Grid!
Animation: Whole Sheet               ← Cycle through all sprites
Frame over Time: Curve 0 → 1.0       ← Animate over lifetime
  - Click on curve → Add keys:
    - Key at 0.0: Value 0.0
    - Key at 1.0: Value 1.0
Start Frame: Random 0 - 2.999        ← Randomize start (for 3-frame sprite)
Sprites: Size 3                      ← Number of frames
  Element 0: [Drag Rain_0 here]      ← First sliced sprite
  Element 1: [Drag Rain_1 here]      ← Second sliced sprite
  Element 2: [Drag Rain_2 here]      ← Third sliced sprite
```

**Why Sprites mode (not Grid):**
- Grid mode requires perfectly aligned sprite sheets
- Sprites mode gives you full control over which sprites to use
- Same method you used for destructible objects
- More flexible (can skip frames, reorder, etc.)

---

#### **B. Renderer Module** (Display settings)

```
Render Mode: Billboard               ← Faces camera (2D sprite)
Material: Sprites/Default            ← Your project's standard sprite material
  ↳ Click circle picker → search "Sprites/Default" → select it
Sorting Layer ID: Default            ← Which sorting layer
Order in Layer: 10                   ← Rendering order within layer
  ↳ -1 = below everything (ground)
  ↳ 0 = gameplay layer
  ↳ 5-10 = above gameplay
  ↳ 100+ = UI layer
```

**Fields you DON'T need to touch:**
- ❌ Normal Direction (default: 0,0,1)
- ❌ Sort Mode (default is fine)
- ❌ Sorting Fudge (default: 0)
- ❌ Min/Max Particle Size (default)
- ❌ Render Alignment (default: View)
- ❌ Flip, Allow Roll (default: 0,0,0)
- ❌ Pivot, Visual Pivot (default: 0,0,0)
- ❌ Masking (usually disabled)
- ❌ Custom Vertex Streams (advanced)
- ❌ Cast Shadows (2D games = Off)
- ❌ Motion Vectors (usually Off)
- ❌ Light Probes (2D = Off)
- ❌ Rendering Layer Mask (default)

**Only change these 4 settings:**
1. ✅ Render Mode → Billboard
2. ✅ Material → Sprites/Default
3. ✅ Sorting Layer ID → Default (or custom layer)
4. ✅ Order in Layer → Your chosen order

---

## 🔄 Comparison: Grid vs Sprites Mode

### **Grid Mode** (❌ Don't use for this project)
```
Mode: Grid
Tiles: (3, 1)                        ← Must specify grid layout
Animation: Single Row                ← Which row to use
```
**Pros:** Fast setup if sprite sheet is perfectly aligned  
**Cons:** Less control, requires exact grid alignment

### **Sprites Mode** (✅ Use this!)
```
Mode: Sprites
Sprites: Size 3                      ← Drag individual sprites
  Element 0: Rain_0
  Element 1: Rain_1
  Element 2: Rain_2
```
**Pros:** Full control, works with any sprite layout, matches your destructible setup  
**Cons:** Must drag sprites manually (but more reliable)

---

## 🎨 Material Setup (Unity 6)

### **Where is Sprites/Default material?**

Unity 6 includes it by default, but if you can't find it:

**Option 1: Use existing material**
1. Find any working sprite in your scene (player, enemy, etc.)
2. Check its SpriteRenderer → Material field
3. Use that same material for particles

**Option 2: Create new material**
1. Project window → Right-click → Create → Material
2. Name: `ParticleSpriteMaterial`
3. Inspector → Shader dropdown → Select: `Sprites/Default`
4. Save and use this material

**Option 3: Use Universal Render Pipeline material**
If your project uses URP:
- Material → Shader: `Universal Render Pipeline/2D/Sprite-Lit-Default`

---

## 🆚 Unity 6 vs Older Unity Versions

| Feature | Unity 2021-2022 | Unity 6 (2024+) |
|---------|-----------------|-----------------|
| **Texture assignment** | Renderer → Sprite field | Texture Sheet Animation → Sprites list |
| **Render modes** | Billboard, Sprite, etc. | Billboard only (for 2D) |
| **Material location** | Renderer module | Renderer module (same) |
| **Sprite slicing** | Sprite Editor | Sprite Editor (same) |

**Key change:** Sprites are now assigned in **Texture Sheet Animation module**, NOT Renderer module!

---

## 🐛 Common Issues & Solutions

### **❌ "I don't see any sprites/particles rendering"**
**Check these in order:**
1. ✅ Texture Sheet Animation module enabled?
2. ✅ Sprites list populated with sprites?
3. ✅ Material set to `Sprites/Default`?
4. ✅ Order in Layer correct? (not -1000 or behind everything)
5. ✅ Particle System playing? (check in Scene view)
6. ✅ Camera can see particles? (check z-position = 0)

### **❌ "Sprites are black squares"**
**Fix:** Material shader wrong
- Renderer → Material → Click material asset
- Material Inspector → Shader → Change to `Sprites/Default`

### **❌ "Animation not playing (all particles same frame)"**
**Check:**
1. ✅ Frame over Time is a curve (not flat at 0)
2. ✅ Curve goes from 0.0 → 1.0
3. ✅ All sprite frames added to Sprites list

### **❌ "Particles rendering in wrong order"**
**Fix:** Adjust Order in Layer
- Higher number = in front
- Lower number = behind
- Ground splashes = -1
- Clouds = 10

### **❌ "Can't find Sprites/Default material"**
**Fix:** Use your player's sprite material
1. Select player in Hierarchy
2. Check SpriteRenderer → Material (probably says "Sprites-Default")
3. Drag that material from Inspector → Particle System Renderer → Material slot

---

## 📝 Quick Reference Card

**For every weather particle system:**

```
✅ Main Module:
   - Duration: 5.0
   - Looping: ON
   - Start Lifetime: [varies]
   - Start Speed: [varies]
   - Simulation Space: World

✅ Emission:
   - Rate over Time: [varies]

✅ Shape:
   - Shape: Box
   - Scale: (20, 15, 0.01)

✅ Texture Sheet Animation:
   - Mode: Sprites
   - Sprites: [Drag sliced sprites here]
   - Frame over Time: [0 for static, 0→1 for animated]

✅ Renderer:
   - Render Mode: Billboard
   - Material: Sprites/Default
   - Order in Layer: [varies by effect]
```

---

## 🎯 Summary

**The Unity 6 way to add sprites to particles:**

1. **Slice sprite** (if animated) → Sprite Editor
2. **Enable Texture Sheet Animation** module
3. **Set Mode: Sprites** (NOT Grid)
4. **Drag sprites** into Sprites list (Element 0, 1, 2...)
5. **Configure Frame over Time** (0 = static, curve = animated)
6. **Set Renderer Material** to `Sprites/Default`
7. **Set Order in Layer** for rendering order

**That's it!** No "Texture" field in Renderer - everything is in **Texture Sheet Animation**.

---

## 📚 References

- **Your destructible setup:** `Docs/Week_10_Oct21-25/1_DESTRUCTIBLE_OBJECTS.md`
- **Unity Manual:** Particle System → Texture Sheet Animation Module
- **Project:** NinjaAdventure (Unity 6000.2.7f2)
