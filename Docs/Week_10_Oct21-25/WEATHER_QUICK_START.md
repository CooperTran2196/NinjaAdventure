# Weather System - Quick Start Guide
**Week 10 - October 18, 2025**

---

## ⚡ TL;DR - 60 Second Setup

1. **Open prefab:** `Assets/GAME/Scripts/Environment/FX/Particle/Rain.prefab`
2. **Add AudioSource** component (if not present)
3. **Configure:**
   - Spatial Blend: **1.0**
   - Min Distance: **5**
   - Max Distance: **20**
4. **Drag into scene** at desired location
5. **Test:** Press Cmd+P, walk near/far to hear audio fade

**That's it!** No scripts needed. Unity handles everything.

---

## 🎯 What You Get

### 7 Ready-to-Use Weather Effects
- ✅ **Rain** (with splash)
- ✅ **Snow** (7-frame animation)
- ✅ **Clouds** (slow drift)
- ✅ **Fog** (low alpha)
- ✅ **Fire** (with smoke child)
- ✅ **Spark** (burst emission)
- ✅ **Leaf** (6-frame rotation)

### Zero Scripts Architecture
- **Particle Culling** - Auto stops rendering when off-screen
- **3D Spatial Audio** - Natural volume fade by distance
- **Play On Awake** - Weather starts automatically
- **No Maintenance** - Set once, works forever

---

## 📍 Where Things Are

| **What** | **Location** |
|----------|--------------|
| Prefabs | `Assets/GAME/Scripts/Environment/FX/Particle/` |
| Complete Guide | `Docs/Week_10_Oct21-25/3_COMPLETE_WEATHER_GUIDE.md` |
| Unity Setup | `Docs/Week_10_Oct21-25/3_UNITY_WEATHER_SETUP.md` |
| This Guide | `Docs/Week_10_Oct21-25/WEATHER_QUICK_START.md` |

---

## 🔧 The Only Settings You Need

### AudioSource Configuration (3D Spatial)
```
Spatial Blend: 1.0 (full 3D - makes distance work!)
Min Distance: 5 (full volume within 5 units)
Max Distance: 20 (silent beyond 20 units)
Rolloff: Logarithmic (natural fade)
Loop: TRUE
Play On Awake: TRUE
```

### Particle System (Already Configured)
```
Looping: TRUE (weather repeats)
Play On Awake: TRUE (starts automatically)
Culling Mode: Automatic (stops rendering off-screen)
```

---

## 🎮 How to Use in Your Game

### Scenario 1: Forest Rain Area
```
1. Drag Rain.prefab into scene
2. Position at (15, 10, 0)
3. Adjust Shape → Scale X: 20
4. Done! Rain appears in forest with audio fade
```

### Scenario 2: Mountain Snow
```
1. Drag Snow.prefab into scene
2. Position at (50, 12, 0)
3. Adjust Shape → Scale X: 30
4. Done! Snow falls with wind audio
```

### Scenario 3: Campfire
```
1. Drag Fire.prefab into scene
2. Position at (5, 0.5, 0)
3. Scale: 0.5-1.0
4. Done! Fire + Smoke with crackle audio
```

---

## 📊 Audio Distance Reference

| **Effect** | **Min Distance** | **Max Distance** | **Volume** |
|------------|------------------|------------------|------------|
| Rain | 5 | 20 | 0.5-0.8 |
| Snow | 5 | 15 | 0.3-0.5 |
| Fire | 3 | 15 | 0.6-0.8 |
| Fog | 10 | 25 | 0.2-0.4 |
| Spark | 2 | 10 | 0.4 |
| Leaf | 5 | 18 | 0.3-0.5 |

**What This Means:**
- **Min Distance:** Full volume when player within this range
- **Max Distance:** Silent when player beyond this range
- **Between:** Smooth fade using logarithmic curve

---

## ✅ Testing Checklist

- [ ] Prefab has ParticleSystem component
- [ ] ParticleSystem → Looping = TRUE
- [ ] ParticleSystem → Play On Awake = TRUE
- [ ] AudioSource added (if sound needed)
- [ ] AudioSource → Spatial Blend = 1.0
- [ ] AudioSource → Loop = TRUE
- [ ] AudioClip assigned to AudioSource
- [ ] Placed in scene at desired location
- [ ] Tested in Play mode (Cmd+P)
- [ ] Audio fades in when approaching
- [ ] Audio fades out when leaving

---

## 🐛 Common Issues

### "Audio doesn't fade by distance"
**Fix:** AudioSource → Spatial Blend must be **1.0** (not 0)

### "Particles don't appear"
**Fix:** Check GameObject is Active in Hierarchy (checkbox enabled)

### "Audio too loud everywhere"
**Fix:** Increase Max Distance (e.g., 20 → 30)

### "Performance lag"
**Fix:** Reduce Max Particles in Particle System settings

---

## 💡 Pro Tips

1. **Layer Effects:** Combine Fog + Rain for richer atmosphere
2. **Adjust Shape:** Particle System → Shape → Scale X (covers area width)
3. **Audio Volume:** Lower volume for subtle ambience (0.2-0.4)
4. **Reuse Prefabs:** Duplicate same prefab for multiple areas
5. **Test Early:** Always test audio distance in Play mode

---

## 📚 Full Documentation

- **Complete specs:** [3_COMPLETE_WEATHER_GUIDE.md](3_COMPLETE_WEATHER_GUIDE.md)
- **Unity walkthrough:** [3_UNITY_WEATHER_SETUP.md](3_UNITY_WEATHER_SETUP.md)
- **Week 10 overview:** [README.md](README.md)

---

## 🎯 Why This Design?

### Before: Manager-Based (267 lines of code)
- ❌ Complex camera following system
- ❌ Manual fade in/out logic
- ❌ Global weather coordination
- ❌ Script maintenance required
- ❌ Over-engineered for simple needs

### After: Always-On (0 lines of code)
- ✅ Unity particle culling (automatic)
- ✅ 3D spatial audio (built-in)
- ✅ Area-based placement (drag & drop)
- ✅ Zero maintenance (set and forget)
- ✅ Perfect for area-specific weather

**Result:** Simpler, faster, easier to maintain.

---

**Weather System Status:** ✅ Production Ready  
**Setup Time:** ~2 minutes per effect  
**Maintenance:** Zero (Unity handles everything)
