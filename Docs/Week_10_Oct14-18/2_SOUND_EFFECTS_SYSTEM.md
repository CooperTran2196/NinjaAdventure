# Sound Effects System - Combat Audio
**Created:** October 17, 2025  
**Status:** 🚧 PLANNING  
**Priority:** HIGH - Critical for game feel

---

## 📋 Overview

Add audio feedback to combat for better player experience. The Godot reference project has this and it's a crucial feature for game feel - silent combat feels unresponsive.

**What We Have:**
- ✅ Sound assets already imported (`/Assets/GAME/Audio/Sounds/`)
- ✅ Music system working (SYS_GameManager)
- ✅ Event system for damage/attacks (OnDied, etc.)

**What We Need:**
- ❌ Sound effect manager/player system
- ❌ Wire sounds to combat events
- ❌ Attack swing sounds on weapon activation
- ❌ Hit impact sounds on damage dealt

---

## 🎯 Goals

1. **Attack Sounds** - Play "whoosh/slash" when attacking
2. **Hit Sounds** - Play "impact/hit" when damage connects
3. **Death Sounds** - Optional death audio
4. **Randomization** - Vary sounds to avoid repetition
5. **Volume Control** - Separate SFX volume from music

---

## 📂 Available Sound Assets

### **Whoosh & Slash** (Attack Sounds)
Located: `/Assets/GAME/Audio/Sounds/Whoosh & Slash/`
```
Slash.wav, Slash2.wav, Slash3.wav, Slash4.wav, Slash5.wav  → Melee attacks
Whoosh.wav, Whoosh2.wav                                      → Dodge/movement
Sword2.wav                                                    → Heavy attack?
Launch.wav                                                    → Ranged/magic?
```

### **Hit & Impact** (Damage Sounds)
Located: `/Assets/GAME/Audio/Sounds/Hit & Impact/`
```
Hit1.wav, Hit2.wav, Hit3.wav, Hit5.wav, Hit6.wav, Hit7.wav, Hit8.wav, Hit9.wav
Impact.wav, Impact2.wav, Impact3.wav, Impact4.wav, Impact5.wav
```

---

## 🏗️ Architecture Design

### **Option 1: Simple Approach (Godot-style)**
**Pros:** Quick, minimal code, works immediately  
**Cons:** AudioSource on every entity, harder to manage volume

```csharp
// On Player/Enemy prefab - Add AudioSource component
public class C_Health : MonoBehaviour
{
    [Header("Sound Effects")]
    AudioSource audioSource;
    public AudioClip[] hitSounds;  // Assign 3-5 hit sounds
    
    void Awake() {
        audioSource ??= GetComponent<AudioSource>();
    }
    
    public int ApplyDamage(...) {
        // ... damage logic ...
        
        if (dealt > 0 && hitSounds.Length > 0)
        {
            int index = Random.Range(0, hitSounds.Length);
            audioSource.PlayOneShot(hitSounds[index]);
        }
        
        return dealt;
    }
}
```

---

### **Option 2: Centralized Manager (Recommended)**
**Pros:** Better control, consistent volume, easy to pool AudioSources  
**Cons:** Slightly more code, needs manager setup

```csharp
// New file: SYS_SoundManager.cs
public class SYS_SoundManager : MonoBehaviour
{
    public static SYS_SoundManager Instance { get; private set; }
    
    [Header("Sound Effects")]
    public AudioClip[] attackSlash;   // Melee attack sounds
    public AudioClip[] attackWhoosh;  // Dodge/fast movement
    public AudioClip[] hitImpact;     // Enemy takes damage
    public AudioClip[] hitFlesh;      // Player takes damage (meatier sound?)
    
    [Header("Settings")]
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    
    AudioSource[] pool;  // Pool of 5-10 AudioSources
    int poolIndex = 0;
    
    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Create AudioSource pool
        pool = new AudioSource[8];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 0f;  // 2D sound
        }
    }
    
    public void PlaySound(AudioClip[] clips) {
        if (clips == null || clips.Length == 0) return;
        
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        AudioSource source = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Length;
        
        source.volume = sfxVolume;
        source.PlayOneShot(clip);
    }
    
    // Convenience methods
    public void PlayAttackSlash() => PlaySound(attackSlash);
    public void PlayAttackWhoosh() => PlaySound(attackWhoosh);
    public void PlayHitImpact() => PlaySound(hitImpact);
    public void PlayHitFlesh() => PlaySound(hitFlesh);
}
```

---

## 🔌 Integration Points

### **1. Weapon Attacks** (W_Base.cs)
```csharp
// In W_Base.Attack() or weapon-specific implementations
public override void Attack(Vector2 dir)
{
    // Play attack sound when weapon swings
    SYS_SoundManager.Instance.PlayAttackSlash();
    
    // ... existing attack logic ...
}
```

### **2. Hit Feedback** (C_Health.cs)
```csharp
// In ApplyDamage() when damage > 0
public int ApplyDamage(...)
{
    // ... armor/penetration math ...
    
    if (dealt > 0)
    {
        // Play hit sound
        SYS_SoundManager.Instance.PlayHitImpact();
        
        OnDamaged?.Invoke(dealt);
    }
    
    return dealt;
}
```

### **3. Dodge Roll** (P_State_Dodge.cs)
```csharp
void OnEnable()
{
    // Play whoosh sound when dodging
    SYS_SoundManager.Instance.PlayAttackWhoosh();
    
    // ... existing dodge logic ...
}
```

---

## 🎨 Sound Assignment Strategy

### **Recommended Mapping:**

| Action | Sound Category | Files | Notes |
|--------|---------------|-------|-------|
| **Melee Attack** | Slash | Slash1-5 | Random variation |
| **Ranged Attack** | Launch | Launch | Single projectile sound |
| **Dodge Roll** | Whoosh | Whoosh1-2 | Fast movement |
| **Enemy Hit** | Impact | Impact1-5 | Harder sound |
| **Player Hit** | Hit | Hit1,2,3,5,6 | Flesh/softer sound |
| **Weapon Heavy** | Sword | Sword2 | Critical hits? |

---

## 📊 Comparison with Godot

### **Godot Implementation:**
```gdscript
# Player.gd
func hit(damage = 1):
    emit_signal("hit", damage)
    $SndHit.play()  # Direct child AudioStreamPlayer
    hit_fx()

# Weapon.gd (no attack sound shown in code)
# Monster.gd
func hit(damage):
    life_bar.value -= damage
    $SndHit.play()  # Direct child AudioStreamPlayer
    hit_fx()
```

**Pattern:** Each entity has its own `AudioStreamPlayer` node as child.

### **Our Unity Approach:**
**Option A (Simple):** AudioSource on each entity (same as Godot)  
**Option B (Better):** Centralized SoundManager with pooled AudioSources

**Recommendation:** Use **Option B** for consistency with SYS_GameManager pattern.

---

## ✅ Implementation Checklist

### **Phase 1: Setup (15 minutes)**
- [ ] Create `SYS_SoundManager.cs` script
- [ ] Add SoundManager to GameManager GameObject (or create new SYS_SoundManager object)
- [ ] Assign sound clips in Inspector:
  - [ ] Assign 3-5 Slash sounds to `attackSlash[]`
  - [ ] Assign 2 Whoosh sounds to `attackWhoosh[]`
  - [ ] Assign 3-5 Impact sounds to `hitImpact[]`
  - [ ] Assign 3-5 Hit sounds to `hitFlesh[]`
- [ ] Set `sfxVolume` to 0.5-0.7 (test and adjust)

### **Phase 2: Wire Attack Sounds (10 minutes)**
- [ ] Add `SYS_SoundManager.Instance.PlayAttackSlash()` to `W_Melee.Attack()`
- [ ] Add `SYS_SoundManager.Instance.PlaySound(attackLaunch)` to `W_Ranged.Attack()` (use Launch.wav)
- [ ] Test melee attacks → should hear slash sounds
- [ ] Test ranged attacks → should hear launch sound

### **Phase 3: Wire Hit Sounds (10 minutes)**
- [ ] Add `SYS_SoundManager.Instance.PlayHitImpact()` to `C_Health.ApplyDamage()` (when dealt > 0)
- [ ] Test hitting enemies → should hear impact
- [ ] Test player taking damage → should hear hit

### **Phase 4: Polish (15 minutes)**
- [ ] Add dodge whoosh to `P_State_Dodge.OnEnable()`
- [ ] Test sound randomization (attack 10 times, should vary)
- [ ] Adjust volumes if needed
- [ ] Optional: Add death sound to `C_Health.OnDied` event

### **Phase 5: Documentation**
- [ ] Update this doc to COMPLETE status
- [ ] Add code snippets showing final implementation
- [ ] Update Week 10 README

---

## 🎮 Testing Checklist

After implementation, verify:
- [ ] ✅ Melee attack plays slash sound (varies each time)
- [ ] ✅ Ranged attack plays launch sound
- [ ] ✅ Dodge roll plays whoosh sound
- [ ] ✅ Enemy hit plays impact sound
- [ ] ✅ Player hit plays flesh hit sound
- [ ] ✅ Sounds don't cut each other off (pooling works)
- [ ] ✅ Volume is balanced with music
- [ ] ✅ No audio clipping or distortion

---

## 🚨 Potential Issues & Solutions

### **Issue 1: Sounds Cut Off Early**
**Cause:** Reusing same AudioSource before clip finishes  
**Solution:** Use AudioSource pool (8-10 sources) with round-robin

### **Issue 2: Sounds Too Loud/Quiet**
**Cause:** Volume not balanced with music  
**Solution:** 
- SFX volume: 0.5-0.7
- Music volume: 0.3-0.5
- Test with both playing

### **Issue 3: Repetitive Sounds**
**Cause:** Only using 1-2 sound files  
**Solution:** Assign 3-5 variations per action, use `Random.Range()`

### **Issue 4: Sounds Play Multiple Times**
**Cause:** Multiple colliders hitting same frame  
**Solution:** Add cooldown timer or use `PlayOneShot()` instead of `Play()`

---

## 🎯 Future Enhancements (Optional)

- **Sound Categories:** Separate volume sliders (SFX, Music, Ambient)
- **Spatial Audio:** Add 3D sound with distance falloff for ranged enemies
- **Footsteps:** Walking sounds on different terrain
- **UI Sounds:** Menu clicks, inventory drag/drop
- **Voice Lines:** Damage grunts, death cries (Voice folder has assets)
- **Ambient Sounds:** Wind, fire, water (Ambient folder has assets)

---

## 📚 Code Style Integration

### **Follows Coding Style Guide:**
```csharp
// SYS_SoundManager.cs structure
public class SYS_SoundManager : MonoBehaviour
{
    public static SYS_SoundManager Instance { get; private set; }
    
    [Header("Sound Effects - Assign in Inspector")]
    public AudioClip[] attackSlash;
    public AudioClip[] hitImpact;
    
    [Header("Settings")]
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    
    // Private cache
    AudioSource[] pool;
    int poolIndex;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializePool();
    }
    
    void InitializePool()
    {
        // ... setup logic ...
    }
    
    public void PlaySound(AudioClip[] clips)
    {
        // ... implementation ...
    }
}
```

**Validation:**
- ✅ Public static Instance (singleton exception)
- ✅ Headers for Inspector organization
- ✅ Private fields for internal state
- ✅ Awake() for initialization
- ✅ Public methods for external calls

---

## 📈 Impact on Game Feel

**Before (Silent Combat):**
- ❌ Feels unresponsive
- ❌ Hard to tell if hits connect
- ❌ Less satisfying to play

**After (With Sound Effects):**
- ✅ Immediate feedback
- ✅ Clear hit confirmation
- ✅ More "juicy" combat
- ✅ Matches Godot reference quality

**Expected Impact:** **+30% perceived polish** - Audio feedback is one of the biggest improvements for minimal code.

---

## 🏆 Success Criteria

**System is complete when:**
1. ✅ All combat actions have audio feedback
2. ✅ Sounds don't cut each other off (pooling works)
3. ✅ Volume is balanced and pleasant
4. ✅ Sound variations prevent repetition
5. ✅ No performance impact (pooling is efficient)
6. ✅ Code follows style guide
7. ✅ Documentation updated

---

**Next Steps After Completion:**
1. Mark this doc as COMPLETE
2. Update Week 10 README with completion
3. Consider adding UI sounds (menu clicks)
4. Consider adding footstep sounds for movement

---

**Last Updated:** October 17, 2025
