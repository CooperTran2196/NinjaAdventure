# Weather Manager Runtime Controls Guide
**Created:** October 17, 2025  
**Script:** ENV_WeatherManager.cs  
**Status:** ✅ Runtime Toggle Enabled

---

## 🎮 New Runtime Features

Your Weather Manager now supports **3 ways to control weather in Play mode**:

1. **Debug Hotkeys** (Keyboard shortcuts)
2. **Inspector Live Toggle** (Check/uncheck boxes while playing)
3. **Code/Script Calls** (For game events, triggers, etc.)

---

## ⌨️ Method 1: Debug Hotkeys (Testing)

### **Preset Hotkeys:**
Press these keys **during Play mode** to switch weather instantly:

| Key | Preset | Effect |
|-----|--------|--------|
| **1** | Clear Weather | Clouds only + Raylight |
| **2** | Rainy Weather | Clouds + Rain + Leaves |
| **3** | Snowy Weather | Clouds + Snow + Smoke + Fog |
| **4** | Magical Weather | Fog + Spark + Raylight |

### **Individual Effect Toggles:**
Press to toggle on/off:

| Key | Effect | Current State |
|-----|--------|---------------|
| **C** | Clouds | Toggles on/off |
| **R** | Rain | Toggles rain + splashes |
| **L** | Leaves | Toggles falling leaves |

**How to use:**
1. Press Play ▶️
2. Press `2` key → Rainy weather starts
3. Press `C` key → Clouds disappear
4. Press `C` again → Clouds come back
5. Press `1` key → Clear sky (all rain/leaves stop)

**To disable hotkeys:**
- Select `WeatherController` → Inspector
- ENV_WeatherManager → Runtime Controls section
- Uncheck **"Enable Debug Hotkeys"**

---

## 🎛️ Method 2: Inspector Live Toggle (Visual Control)

**While in Play mode:**

1. **Select `WeatherController` in Hierarchy**
2. **Inspector → ENV_WeatherManager → Weather State**
3. **Check/Uncheck boxes** to toggle effects:
   - ✓ Check `Enable Clouds` → Clouds start
   - ☐ Uncheck `Enable Rain` → Rain stops instantly
4. **Changes apply immediately** (no need to stop/restart)

**How it works:**
- Script has `OnValidate()` method
- Detects Inspector changes during Play mode
- Calls `ApplyWeatherState()` automatically
- Effects update in real-time

**Requirements:**
- `Allow Runtime Toggle` must be ✓ ON (default)
- Changes only work in Play mode (design intentional)
- Checkbox changes in Edit mode set initial state

---

## 💻 Method 3: Script/Code Control (Game Integration)

### **A. Toggle Individual Effects**

**Simple toggle (flip current state):**
```csharp
// From any script with reference to WeatherManager:
weatherManager.ToggleClouds();    // On → Off, or Off → On
weatherManager.ToggleRain();      // Toggle rain system
weatherManager.ToggleLeaves();    // Toggle falling leaves
```

**Set specific state:**
```csharp
// Turn ON specific effect:
weatherManager.ToggleWeather("clouds", true);
weatherManager.ToggleWeather("rain", true);

// Turn OFF specific effect:
weatherManager.ToggleWeather("leaves", false);
```

**Example: Player enters cave → stop rain:**
```csharp
public class CaveTrigger : MonoBehaviour
{
    [SerializeField] private ENV_WeatherManager weatherManager;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            weatherManager.ToggleWeather("rain", false);  // Stop rain
            weatherManager.ToggleWeather("leaves", false); // Stop leaves
            Debug.Log("Entered cave - weather stopped");
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            weatherManager.ToggleWeather("rain", true);   // Resume rain
            weatherManager.ToggleWeather("leaves", true);  // Resume leaves
            Debug.Log("Exited cave - weather resumed");
        }
    }
}
```

---

### **B. Switch Weather Presets**

**Use preset methods:**
```csharp
// Clear sunny day:
weatherManager.SetClearWeather();

// Rainy autumn:
weatherManager.SetRainyWeather();

// Snowy winter:
weatherManager.SetSnowyWeather();

// Magical atmosphere:
weatherManager.SetMagicalWeather();
```

**Example: Time-based weather:**
```csharp
public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private ENV_WeatherManager weatherManager;
    
    void Update()
    {
        // Morning (6am-12pm) = Clear
        if (currentHour >= 6 && currentHour < 12)
        {
            weatherManager.SetClearWeather();
        }
        // Afternoon (12pm-6pm) = Rainy
        else if (currentHour >= 12 && currentHour < 18)
        {
            weatherManager.SetRainyWeather();
        }
        // Night (6pm-6am) = Magical
        else
        {
            weatherManager.SetMagicalWeather();
        }
    }
}
```

---

### **C. Custom Weather Combinations**

**Set all effects at once:**
```csharp
// SetWeather(clouds, rain, leaves, smoke, fog, spark, snow, raylight)
weatherManager.SetWeather(
    clouds: true,
    rain: true,
    leaves: false,
    smoke: false,
    fog: false,
    spark: false,
    snow: false,
    raylight: false
);
```

**Example: Boss arena weather:**
```csharp
public class BossArena : MonoBehaviour
{
    [SerializeField] private ENV_WeatherManager weatherManager;
    
    void OnBossSpawn()
    {
        // Dramatic weather: dark clouds + fog + sparks
        weatherManager.SetWeather(
            clouds: true,
            rain: false,
            leaves: false,
            smoke: true,
            fog: true,
            spark: true,
            snow: false,
            raylight: false
        );
    }
    
    void OnBossDefeated()
    {
        // Clear skies + raylight (victory!)
        weatherManager.SetClearWeather();
    }
}
```

---

### **D. Stop All Weather**

**Emergency stop:**
```csharp
weatherManager.StopAllWeather();  // Stops everything instantly
```

**Use cases:**
- Cutscenes (no distractions)
- Indoor scenes (no weather inside)
- Performance optimization (too many particles)

---

### **E. Check Weather State**

**Query current state:**
```csharp
if (weatherManager.IsWeatherEnabled("rain"))
{
    Debug.Log("It's raining!");
    // Player takes damage from lightning, etc.
}

if (weatherManager.IsWeatherEnabled("snow"))
{
    Debug.Log("It's snowing!");
    // Player moves slower, etc.
}
```

**Example: Weather-based gameplay:**
```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private ENV_WeatherManager weatherManager;
    private float moveSpeed = 5f;
    
    void Update()
    {
        float speed = moveSpeed;
        
        // Slow down in snow
        if (weatherManager.IsWeatherEnabled("snow"))
        {
            speed *= 0.7f;  // 30% slower
        }
        
        // Speed up in clear weather
        if (weatherManager.IsWeatherEnabled("clouds") && 
            !weatherManager.IsWeatherEnabled("rain"))
        {
            speed *= 1.2f;  // 20% faster
        }
        
        // Apply movement...
    }
}
```

---

## 🛠️ Inspector Settings Reference

### **Weather Systems** (Drag particle systems here)
```
Clouds: PFX_Clouds (ParticleSystem)
Rain: RainDrops (ParticleSystem)
Rain Splash: RainSplash (ParticleSystem)
Leaves: PFX_FallingLeaves (ParticleSystem)
Smoke: (None) - Not created yet
Fog: (None)
Spark: (None)
Snow: (None)
Raylight: (None)
```

### **Weather State** (Initial state when scene loads)
```
☑ Enable Clouds       ← Scene starts with clouds
☑ Enable Rain         ← Scene starts raining
☑ Enable Leaves       ← Leaves fall at start
☐ Enable Smoke
☐ Enable Fog
☐ Enable Spark
☐ Enable Snow
☐ Enable Raylight
```

### **Runtime Controls** (New settings)
```
☑ Enable Debug Hotkeys    ← Keyboard shortcuts (1-4, C, R, L)
☑ Allow Runtime Toggle    ← Inspector changes work in Play mode
```

---

## 🎯 Common Use Cases

### **1. Scene Transition Weather**
```csharp
public class SceneManager : MonoBehaviour
{
    [SerializeField] private ENV_WeatherManager weatherManager;
    
    public void LoadForestScene()
    {
        weatherManager.SetRainyWeather();  // Forest = rainy
        // Load scene...
    }
    
    public void LoadMountainScene()
    {
        weatherManager.SetSnowyWeather();  // Mountain = snowy
        // Load scene...
    }
}
```

### **2. Trigger-Based Weather**
```csharp
public class WeatherTrigger : MonoBehaviour
{
    [SerializeField] private ENV_WeatherManager weatherManager;
    [SerializeField] private string weatherType = "rain";
    [SerializeField] private bool enableOnEnter = true;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            weatherManager.ToggleWeather(weatherType, enableOnEnter);
        }
    }
}
```

### **3. Random Weather System**
```csharp
public class RandomWeather : MonoBehaviour
{
    [SerializeField] private ENV_WeatherManager weatherManager;
    [SerializeField] private float changeInterval = 60f; // Change every 60 seconds
    
    private float timer;
    
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            timer = 0f;
            int random = Random.Range(0, 4);
            switch (random)
            {
                case 0: weatherManager.SetClearWeather(); break;
                case 1: weatherManager.SetRainyWeather(); break;
                case 2: weatherManager.SetSnowyWeather(); break;
                case 3: weatherManager.SetMagicalWeather(); break;
            }
        }
    }
}
```

### **4. UI Button Controls**
```csharp
public class WeatherUI : MonoBehaviour
{
    [SerializeField] private ENV_WeatherManager weatherManager;
    
    // Assign these to UI buttons:
    public void OnCloudsButtonClick() => weatherManager.ToggleClouds();
    public void OnRainButtonClick() => weatherManager.ToggleRain();
    public void OnLeavesButtonClick() => weatherManager.ToggleLeaves();
    public void OnStopAllButtonClick() => weatherManager.StopAllWeather();
}
```

---

## 🧪 Testing Workflow

### **Quick Test (30 seconds):**
1. Press Play ▶️
2. Press `1` key → Clear weather
3. Press `2` key → Rainy weather
4. Press `C` key → Clouds disappear
5. Press `R` key → Rain stops
6. Select WeatherController → Check `Enable Rain` box → Rain resumes
7. Press `3` key → Snowy weather (clouds + smoke + fog when created)

### **Integration Test:**
1. Create trigger box in scene
2. Add `CaveTrigger` script (example above)
3. Assign WeatherController reference
4. Press Play → Walk into trigger
5. Verify weather stops/resumes

---

## 📝 API Summary

### **Public Methods:**

| Method | Parameters | Description |
|--------|------------|-------------|
| `SetWeather()` | 8 bools | Set all effects at once |
| `SetClearWeather()` | None | Preset: Clouds only |
| `SetRainyWeather()` | None | Preset: Clouds + Rain + Leaves |
| `SetSnowyWeather()` | None | Preset: Clouds + Snow + Smoke + Fog |
| `SetMagicalWeather()` | None | Preset: Fog + Spark + Raylight |
| `ToggleWeather()` | string, bool | Toggle specific effect by name |
| `ToggleClouds()` | None | Flip clouds on/off |
| `ToggleRain()` | None | Flip rain on/off |
| `ToggleLeaves()` | None | Flip leaves on/off |
| `ToggleSmoke()` | None | Flip smoke on/off |
| `ToggleFog()` | None | Flip fog on/off |
| `ToggleSpark()` | None | Flip spark on/off |
| `ToggleSnow()` | None | Flip snow on/off |
| `ToggleRaylight()` | None | Flip raylight on/off |
| `StopAllWeather()` | None | Stop all effects |
| `IsWeatherEnabled()` | string | Check if effect is on |

---

## 🎨 Customization Tips

### **Add More Hotkeys:**
Edit `Update()` method in `ENV_WeatherManager.cs`:
```csharp
if (Input.GetKeyDown(KeyCode.S)) ToggleWeather("smoke", !enableSmoke);
if (Input.GetKeyDown(KeyCode.F)) ToggleWeather("fog", !enableFog);
if (Input.GetKeyDown(KeyCode.Alpha5)) StopAllWeather();
```

### **Disable Runtime Toggle:**
If you don't want Inspector changes during Play:
- Uncheck `Allow Runtime Toggle`
- Only code/hotkeys will work

### **Custom Debug Keys:**
Change hotkey assignments in `Update()`:
```csharp
// Change from number keys to function keys:
if (Input.GetKeyDown(KeyCode.F1)) SetClearWeather();
if (Input.GetKeyDown(KeyCode.F2)) SetRainyWeather();
```

---

## ✅ Verification Checklist

Test runtime controls work:

- [ ] **Hotkey 1** switches to clear weather
- [ ] **Hotkey 2** switches to rainy weather
- [ ] **Hotkey C** toggles clouds on/off
- [ ] **Hotkey R** toggles rain on/off
- [ ] **Inspector checkboxes** work during Play mode
- [ ] Checking `Enable Rain` → rain starts immediately
- [ ] Unchecking `Enable Leaves` → leaves stop immediately
- [ ] Code calls work (if testing script integration)

---

## 🚀 Next Steps

Now that you have runtime controls:

1. **Test all hotkeys** (make sure they feel responsive)
2. **Try Inspector toggling** (visual feedback for designers)
3. **Create trigger zones** (caves, buildings stop rain)
4. **Add weather UI** (let players control weather for testing)
5. **Integrate with game events** (boss fights, cutscenes)

---

## 📚 References

- **Script:** `Assets/GAME/Scripts/Environment/ENV_WeatherManager.cs`
- **Scene Setup:** `WEATHER_PHASE1_INTEGRATION.md`
- **Fixes Guide:** `WEATHER_MANAGER_FIXES.md`
- **Particle Setup:** `WEATHER_PARTICLE_SETUP.md`

---

**You can now control weather dynamically during gameplay!** 🌦️⚡🎮
