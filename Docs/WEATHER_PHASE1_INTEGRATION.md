# Phase 1: Weather Effects Integration & Testing
**Created:** October 17, 2025  
**Status:** 🧪 Testing 3 Effects (Clouds, Rain, Leaves)  
**Goal:** Integrate into scene, test all features, tune values

---

## 🎯 Current Progress

✅ **Completed:**
- Created 3 weather prefabs
- Configured particle systems
- Added ENV_ParticleWeather scripts
- Saved all prefabs

🚧 **Next Steps:**
1. Add prefabs to Level1 scene
2. Test all particle behaviors
3. Create WeatherController GameObject
4. Test ENV_WeatherManager system
5. Fine-tune values based on visual results
6. Document final settings

---

## 📦 Step 1: Add Weather Effects to Scene (~5 min)

### **A. Open Your Level Scene**
1. Project window → Navigate to `Assets/GAME/Scenes/`
2. Double-click `Level1.unity` (or your main gameplay scene)

### **B. Create Weather Container**
1. Hierarchy → Right-click → Create Empty
2. Rename: `_Weather` (underscore makes it sort to top)
3. Position: `(0, 0, 0)`

**Why a container:**
- Keeps Hierarchy organized
- Easy to toggle all weather on/off
- Groups related systems

### **C. Add Prefabs as Children**
1. From Project: `Assets/GAME/Prefabs/Environment/Weather/PFX_Clouds`
   - Drag into Hierarchy → Drop onto `_Weather` (make it a child)
   - Position: `(0, 0, 0)` (relative to parent)

2. From Project: `Assets/GAME/Prefabs/Environment/Weather/PFX_Rain`
   - Drag into `_Weather` as child
   - Position: `(0, 0, 0)`

3. From Project: `Assets/GAME/Prefabs/Environment/Weather/PFX_FallingLeaves`
   - Drag into `_Weather` as child
   - Position: `(0, 0, 0)`

**Your Hierarchy should look like:**
```
_Weather
  ├─ PFX_Clouds
  ├─ PFX_Rain
  │   ├─ RainDrops
  │   └─ RainSplash
  └─ PFX_FallingLeaves
```

### **D. Verify Prefab Settings**
1. Select each prefab in Hierarchy
2. Check Inspector → `ENV_ParticleWeather` component:
   - ✅ `Play On Start`: ON (all effects start automatically)
   - ✅ `Follow Camera`: ON (effects move with camera)

---

## 🧪 Step 2: Test Individual Effects (~10 min)

### **A. Test Clouds**

**Test Setup:**
1. Select `PFX_Rain` in Hierarchy → Inspector → **Disable** (uncheck top checkbox)
2. Select `PFX_FallingLeaves` → Inspector → **Disable**
3. Only `PFX_Clouds` should be active

**Press Play** ▶️ and verify:
- [ ] Clouds spawn gradually (not all at once)
- [ ] Clouds drift slowly to the right
- [ ] Clouds vary in size (some bigger, some smaller)
- [ ] Movement is smooth (not jittery)
- [ ] Clouds are visible (not black squares)
- [ ] Order in Layer correct (clouds behind UI, above ground)

**Move player around:**
- [ ] Clouds follow camera smoothly
- [ ] No performance issues (check FPS in Stats window)

**If clouds NOT visible:**
- Scene view: Click the "2D" button (top toolbar) to ensure 2D mode
- Check Camera: Main Camera should be at Z = -10 (looking at Z = 0)
- Check Material: Renderer → Material should show `Sprites/Default`

**Stop Play mode.**

---

### **B. Test Rain System**

**Test Setup:**
1. Select `PFX_Clouds` → Inspector → **Disable**
2. Select `PFX_Rain` → Inspector → **Enable** ✓
3. Select `PFX_FallingLeaves` → Inspector → **Disable**

**Press Play** ▶️ and verify:

**RainDrops (falling):**
- [ ] Rain drops fall downward (Start Speed 30)
- [ ] 3-frame animation plays (drops stretch/animate)
- [ ] ~40 drops visible at once
- [ ] Drops spawn across full screen (Box shape 20x15)

**RainSplash (ground):**
- [ ] Splashes appear on ground (Order -1 = behind everything)
- [ ] Splashes animate through 3 frames
- [ ] ~60 splashes visible (more than drops)
- [ ] Splashes don't move (Start Speed 0)

**Move player around:**
- [ ] Rain follows camera
- [ ] Rain density feels balanced (not too sparse/dense)

**If animation NOT playing:**
- Select `RainDrops` in Hierarchy
- Inspector → Particle System → Texture Sheet Animation:
  - Mode: Sprites ✓
  - Sprites list has 3 elements (Rain_0, Rain_1, Rain_2) ✓
  - Frame over Time curve goes from 0 → 1.0 ✓
- Repeat check for `RainSplash` with RainOnFloor sprites

**Stop Play mode.**

---

### **C. Test Falling Leaves**

**Test Setup:**
1. Select `PFX_Rain` → Inspector → **Disable**
2. Select `PFX_FallingLeaves` → Inspector → **Enable** ✓

**Press Play** ▶️ and verify:

**Movement:**
- [ ] Leaves fall diagonally (downward + slight horizontal drift)
- [ ] Leaves rotate/tumble as they fall (Rotation over Lifetime)
- [ ] Movement feels gentle/organic (Start Speed 5, Damping 0.25)
- [ ] Only ~10 leaves visible (not overwhelming)

**Animation:**
- [ ] 6-frame leaf animation plays (leaf shape changes)
- [ ] Each leaf animates at different speed (random Frame over Time)
- [ ] Leaves fade out at end of lifetime (Color over Lifetime)

**Move player around:**
- [ ] Leaves follow camera
- [ ] Peaceful autumn atmosphere

**If leaves NOT rotating:**
- Check: Rotation over Lifetime module enabled ✓
- Check: Angular Velocity set to Random -45 to 45

**Stop Play mode.**

---

## 🎛️ Step 3: Create Weather Controller (~5 min)

Now create a central manager to control all 3 effects at once.

### **A. Create Controller GameObject**
1. Hierarchy → Right-click on `_Weather` → Create Empty
2. Rename: `WeatherController`
3. Position: `(0, 0, 0)`

### **B. Add ENV_WeatherManager Script**
1. Select `WeatherController`
2. Inspector → Add Component → `ENV_WeatherManager`

### **C. Assign Weather Systems**
In Inspector → `ENV_WeatherManager` component:

**Weather Systems section:**
1. **Clouds:** Drag `PFX_Clouds` from Hierarchy → drop on field
2. **Rain:** Drag `PFX_Rain/RainDrops` → drop on field
3. **Rain Splash:** Drag `PFX_Rain/RainSplash` → drop on field
4. **Leaves:** Drag `PFX_FallingLeaves` → drop on field
5. Leave other fields empty (Smoke, Fog, Spark, Snow, Raylight)

**Weather State section:**
Configure default weather (what plays on scene start):
```
Enable Clouds: ✓ ON
Enable Rain: ✓ ON       (test rainy weather)
Enable Leaves: ✓ ON     (autumn feel)
Enable Smoke: ☐ OFF
Enable Fog: ☐ OFF
Enable Spark: ☐ OFF
Enable Snow: ☐ OFF
Enable Raylight: ☐ OFF
```

**Why enable all 3:**
- Test combined effects together
- See how they layer visually
- Check performance with multiple systems

---

## 🧪 Step 4: Test Weather Manager System (~5 min)

### **A. Test Auto-Start**

**Press Play** ▶️ and verify:
- [ ] All 3 effects start automatically
- [ ] Clouds drift behind rain/leaves
- [ ] Rain falls through leaves
- [ ] Splashes appear on ground (below everything)
- [ ] All effects follow camera together
- [ ] No performance issues

**Visual Layering Check:**
- Clouds (Order 10) → Background atmosphere
- Leaves (Order 6) → Mid-layer
- Rain drops (Order 5) → Foreground
- Rain splashes (Order -1) → Ground layer

---

### **B. Test Weather Presets (Runtime)**

While in Play mode, test the preset methods:

1. **Select `WeatherController` in Hierarchy** (keep Inspector visible)
2. **Inspector → ENV_WeatherManager → Public Methods:**

**Test Clear Weather:**
- Scroll to bottom of script component
- Find `SetClearWeather()` method
- Click the **▶️ button** (or right-click → Invoke)
- **Verify:** Only clouds remain, rain/leaves stop

**Test Rainy Weather:**
- Click `SetRainyWeather()` 
- **Verify:** Clouds + Rain + Leaves all playing

**Test Snowy Weather:**
- Click `SetSnowyWeather()`
- **Verify:** All current effects stop (snow not created yet, so just clouds)

**Stop Play mode.**

---

## 🎨 Step 5: Fine-Tune Values (~15 min)

Based on visual results, adjust these common tweaks:

### **If Clouds Too Fast/Slow:**
1. Select `PFX_Clouds` → Particle System
2. Adjust:
   - **Start Speed:** Lower = slower drift (try 5-15 range)
   - **Velocity over Lifetime → Linear X:** Lower = slower (try 0.5-2.0)
   - **Limit Velocity → Damping:** Higher = slows down faster

### **If Rain Too Heavy/Light:**
1. Select `PFX_Rain/RainDrops`
2. Adjust **Emission → Rate over Time:**
   - Too heavy: Lower from 40 → 25-30
   - Too light: Increase to 50-60
3. Select `PFX_Rain/RainSplash`
4. Match splash rate: ~1.5× rain drop rate

### **If Rain Animation Too Fast:**
1. Select `PFX_Rain/RainDrops`
2. **Texture Sheet Animation → Frame over Time:**
   - Currently: 0 → 1.0 (full speed)
   - Slower: 0 → 0.5 (half speed)
   - Click curve → adjust keyframe values

### **If Leaves Fall Too Fast:**
1. Select `PFX_FallingLeaves`
2. Adjust:
   - **Start Speed:** Lower = gentler (try 2-8 range)
   - **Velocity over Lifetime → Linear Y:** Less negative = slower fall (try -0.2 to -0.8)

### **If Leaves Too Many/Few:**
1. **Emission → Rate over Time:**
   - Too many: Lower from 10 → 5-7
   - Too few: Increase to 12-15

### **If Effects Don't Follow Camera:**
1. Check: `ENV_ParticleWeather` → `Follow Camera` = ON
2. Check: Main Camera has tag "MainCamera" (Inspector → Tag dropdown)

---

## 📝 Step 6: Document Final Settings (~5 min)

After tuning, record your final values for future reference.

### **A. Check Your Final Values**

**For each effect, note these in Scene view:**

**Clouds:**
- Start Speed: `_____`
- Velocity Linear X: `_____`
- Emission Rate: `_____`
- Start Size Range: `_____ - _____`

**Rain Drops:**
- Start Speed: `_____`
- Emission Rate: `_____`
- Frame over Time range: `_____ - _____`

**Rain Splash:**
- Emission Rate: `_____`
- Start Size: `_____`

**Leaves:**
- Start Speed: `_____`
- Velocity Linear Y: `_____`
- Emission Rate: `_____`
- Angular Velocity: `_____ - _____`

### **B. Save Your Scene**
1. File → Save Scene (Ctrl/Cmd + S)
2. Commit to Git:
   ```bash
   git add .
   git commit -m "feat: Add weather effects (Clouds, Rain, Leaves) with WeatherManager"
   ```

---

## 🎯 Step 7: Performance Check (~5 min)

Verify effects run smoothly:

### **A. Check Particle Counts**

**Press Play** ▶️

**Window → Analysis → Profiler:**
1. Click "Rendering" section
2. Find "Particles" row
3. **Target:** <500 total particles at once

**Your expected counts:**
- Clouds: ~6 particles (Rate 6, Lifetime 25s)
- Rain Drops: ~120 particles (Rate 40, Lifetime 3s)
- Rain Splash: ~30 particles (Rate 60, Lifetime 0.5s)
- Leaves: ~200 particles (Rate 10, Lifetime 20s)
- **Total: ~356 particles** ✅ Under budget!

### **B. Check Frame Rate**

**Window → Analysis → Frame Debugger** or **Stats window:**
- **Target:** 60 FPS (or your target frame rate)
- Weather should add <5% overhead

**If performance issues:**
- Lower emission rates (fewer particles)
- Reduce Start Lifetime (particles die sooner)
- Disable expensive modules (Rotation over Lifetime)

---

## ✅ Completion Checklist

Before moving to Phase 2 (more effects), verify:

### **Visual Quality:**
- [ ] All 3 effects visible and rendering correctly
- [ ] Sprites show (not black squares or missing)
- [ ] Animations play smoothly (Rain, Leaves)
- [ ] Layering correct (splashes behind, clouds in back)
- [ ] No flickering or visual glitches

### **Behavior:**
- [ ] Effects loop continuously (don't stop)
- [ ] Camera following works for all effects
- [ ] Movement feels natural (not too fast/slow)
- [ ] Particle counts feel balanced

### **System Integration:**
- [ ] ENV_WeatherManager controls all effects
- [ ] Can toggle effects on/off via script
- [ ] Preset methods work (Clear, Rainy, Snowy)
- [ ] Scene saves with all references intact

### **Performance:**
- [ ] <500 total particles
- [ ] Maintains target FPS
- [ ] No memory leaks (play for 2+ min)

### **Organization:**
- [ ] Prefabs saved in Weather folder
- [ ] Hierarchy organized under `_Weather`
- [ ] Scene saved and committed to Git

---

## 🐛 Common Issues & Solutions

### **❌ "Effects stop playing after a few seconds"**
**Fix:** Check Looping
- Select particle system → Main Module → Looping: ✓ ON

### **❌ "Can't see particles at all"**
**Fix 1:** Camera position
- Main Camera Z = -10, Particles Z = 0
**Fix 2:** Material/Renderer
- Renderer → Material: Sprites/Default
- Texture Sheet Animation → Sprites list populated

### **❌ "Particles don't follow camera"**
**Fix:** ENV_ParticleWeather settings
- Select prefab → ENV_ParticleWeather → Follow Camera: ✓ ON
- Camera tag = "MainCamera"

### **❌ "Rain/Leaves not animating"**
**Fix:** Frame over Time curve
- Texture Sheet Animation → Frame over Time
- Should be curve from 0 → 1.0, NOT flat line at 0

### **❌ "WeatherManager can't find particle systems"**
**Fix:** Drag references manually
- Select WeatherController
- Drag each particle system from Hierarchy → Inspector fields

### **❌ "Splashes render on top of player"**
**Fix:** Order in Layer
- RainSplash → Renderer → Order in Layer: -1 (negative = behind)

---

## 🚀 Next Steps After Phase 1

Once all checks pass:

1. **Test in Different Scenes:**
   - Try adding weather to other levels
   - Verify prefabs work across scenes

2. **Create Weather Transition:**
   - Script gradual fade between weather states
   - Smooth Start/Stop methods

3. **Add Sound Effects** (if desired):
   - Rain ambient sound
   - Leaf rustle audio
   - Integrate with SYS_AudioManager

4. **Document Results:**
   - Take screenshots of final effects
   - Note any custom tuning you did
   - Update WEATHER_EFFECTS_PLAN.md with actual values

5. **Move to Phase 2:**
   - Create remaining effects (Smoke, Fog, Spark, Snow)
   - Use same workflow as Phase 1

---

## 📚 References

- **Setup Guide:** `WEATHER_PARTICLE_SETUP.md`
- **Unity 6 Guide:** `UNITY6_PARTICLE_SPRITE_SETUP.md`
- **Week 10 Particles:** `Docs/Week_10_Oct21-25/1_DESTRUCTIBLE_OBJECTS.md`
- **Scripts:** `ENV_WeatherManager.cs`, `ENV_ParticleWeather.cs`

---

## 📊 Expected Results

**When working correctly, you should see:**
- Clouds drifting peacefully in background
- Rain falling with animated drops + ground splashes
- Leaves tumbling gently downward with rotation
- All effects following camera smoothly
- Clean, organized Hierarchy
- 60 FPS with all effects running

**This is your rainy autumn atmosphere!** 🌧️🍂☁️

If everything looks good, you're ready for Phase 2! 🎉
