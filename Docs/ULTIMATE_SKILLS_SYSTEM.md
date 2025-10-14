# Ultimate Skills System Implementation Plan

## 📋 Overview

This document outlines the implementation of the Ultimate Skills system with two special abilities: **Dagger Barrage** and **Divine Intervention**. These are active skills unlocked via consumable items, with cooldowns and visual effects.

---

## 🎯 Features

### Ultimate Skills System
1. **2 Ultimate Skills:**
   - **Dagger Barrage (E key)** - Projectile attack
   - **Divine Intervention (R key)** - Invulnerability buff

2. **Unlock Mechanism:**
   - Consumable items unlock ultimates
   - NPC dialog gives unlock items as rewards
   - Always visible in UI (greyed out when locked)

3. **Cooldown System:**
   - Each ultimate has independent cooldown
   - Cooldown UI displays remaining time
   - Visual feedback (radial fill, timer text)

4. **Skill Info Display:**
   - Shows skill name + description
   - Similar to shop's item info panel
   - Displays on hover in skill tree

5. **Special Animation Layer:**
   - Child GameObject under Player
   - Separate Animator for ultimate effects
   - Doesn't interfere with combat animations
   - Reusable for future buffs/debuffs

---

## 🏗️ Architecture

### File Structure
```
Assets/GAME/Scripts/
├── SkillTree/
│   ├── ST_Manager.cs (MODIFY - add unlock/cooldown/activation)
│   ├── ST_SkillSO.cs (MODIFY - add active ability fields)
│   └── ST_SkillInfo.cs (NEW - skill info panel)
│
├── Player/
│   ├── P_Controller.cs (MODIFY - cache ultimate components)
│   └── P_UltimateAnimator.cs (NEW - child animator controller)
│
├── Character/
│   └── C_Health.cs (MODIFY - add invulnerability)
│
├── Ultimate/
│   ├── ULT_DaggerBarrage.cs (NEW - barrage logic)
│   ├── ULT_DaggerProjectile.cs (NEW - enhanced projectile)
│   └── ULT_DivineIntervention.cs (NEW - invulnerability)
│
└── UI/
    └── UltimateUI.cs (NEW - cooldown display)
```

---

## 📦 Component Details

---

## 1️⃣ ST_SkillSO.cs (MODIFY)

**Purpose:** Extend skill ScriptableObject to support active abilities

### Changes Required (~40 lines added)

```csharp
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "SkillTree")]
public class ST_SkillSO : ScriptableObject
{
    [Header("Meta")]
    public string id;
    public string skillName = "Auto Filled";
    public Sprite skillIcon;
    [Min(1)] public int maxLevel = 1;
    
    [Header("Effects Per Level")]
    public List<P_StatEffect> StatEffectList;
    
    // NEW: Active Ability (Ultimates)
    [Header("Active Ability (Ultimates)")]
    public bool isActiveAbility = false;
    public float cooldown = 30f; // seconds
    public int manaCost = 0; // optional mana cost
    
    [Header("Unlock Requirements")]
    public bool requiresUnlockItem = false;
    public string unlockItemID = ""; // matches INV_ItemSO.id
    
    [Header("Skill Info Display")]
    [TextArea(3, 8)]
    public string description = "";
    
    void OnValidate()
    {
        if (skillName != name)
            skillName = name;
    }
}
```

### Key Additions:
1. `isActiveAbility` - Flag for ultimates (true) vs passive skills (false)
2. `cooldown` - Cooldown duration in seconds
3. `manaCost` - Optional mana cost for casting
4. `requiresUnlockItem` - If true, needs consumable to unlock
5. `unlockItemID` - ID of unlock item
6. `description` - Full skill description for info panel

### Example Ultimate SO Setup:

**Dagger Barrage:**
```
id: "dagger_barrage"
skillName: "Dagger Barrage"
isActiveAbility: true
cooldown: 30
requiresUnlockItem: true
unlockItemID: "ancient_scroll"
description: 
"Summon spectral daggers around you that fire at enemies.

Level 1: 6 daggers, 20 damage each
Level 2: 8 daggers, 25 damage each  
Level 3: 10 daggers, 30 damage each

Cooldown: 30s"
```

---

## 2️⃣ ST_Manager.cs (MODIFY)

**Purpose:** Add unlock system, cooldown tracking, and input handling

### Changes Required (~250 lines added)

```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class ST_Manager : MonoBehaviour
{
    // ... existing fields (st_Slots, skillPointsText, p_Exp, etc.)
    
    [Header("Ultimate Skills")]
    public List<ST_SkillSO> allSkills; // All skills (passive + ultimates)
    
    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();
    private Dictionary<string, bool> unlockedSkills = new Dictionary<string, bool>();
    
    [Header("Ultimate Components")]
    public ULT_DaggerBarrage daggerBarrage;
    public ULT_DivineIntervention divineIntervention;
    
    // Events for UI updates
    public static event System.Action<string> OnSkillUnlocked; // skill ID
    public static event System.Action<string, float> OnUltimateActivated; // skill ID, cooldown
    public static event System.Action<string, float> OnCooldownTick; // skill ID, remaining time
    
    private P_InputActions input;
    
    void Awake()
    {
        // ... existing Awake code ...
        
        // NEW: Initialize ultimate system
        LoadUnlockedSkills();
        CacheUltimateComponents();
        
        input = new P_InputActions();
    }
    
    void OnEnable()
    {
        // ... existing event subscriptions ...
        
        // NEW: Enable Player input map for ultimates
        input.Player.Enable();
        input.Player.Ultimate1.performed += ActivateUltimate1;
        input.Player.Ultimate2.performed += ActivateUltimate2;
    }
    
    void OnDisable()
    {
        // ... existing event unsubscriptions ...
        
        // NEW: Disable ultimate inputs
        input.Player.Disable();
        input.Player.Ultimate1.performed -= ActivateUltimate1;
        input.Player.Ultimate2.performed -= ActivateUltimate2;
    }
    
    void Update()
    {
        // ... existing skill tree toggle code ...
        
        // NEW: Tick down cooldowns
        UpdateCooldowns();
    }
    
    // ===== NEW: Ultimate System Methods =====
    
    /// <summary>
    /// Cache references to ultimate components
    /// </summary>
    void CacheUltimateComponents()
    {
        P_Controller player = FindFirstObjectByType<P_Controller>();
        if (player != null)
        {
            daggerBarrage = player.GetComponentInChildren<ULT_DaggerBarrage>();
            divineIntervention = player.GetComponentInChildren<ULT_DivineIntervention>();
        }
        
        if (!daggerBarrage)
            Debug.LogWarning("ST_Manager: ULT_DaggerBarrage not found!");
        if (!divineIntervention)
            Debug.LogWarning("ST_Manager: ULT_DivineIntervention not found!");
    }
    
    /// <summary>
    /// Load unlocked skills from save data
    /// </summary>
    void LoadUnlockedSkills()
    {
        foreach (var skill in allSkills)
        {
            if (skill.requiresUnlockItem)
            {
                bool isUnlocked = PlayerPrefs.GetInt($"skill_unlocked_{skill.id}", 0) == 1;
                unlockedSkills[skill.id] = isUnlocked;
            }
            else
            {
                // Passive skills are always unlocked
                unlockedSkills[skill.id] = true;
            }
        }
    }
    
    /// <summary>
    /// Unlock skill via consumable item
    /// Called by INV_Manager when unlock item is used
    /// </summary>
    public void UnlockSkill(string skillID)
    {
        if (unlockedSkills.ContainsKey(skillID) && unlockedSkills[skillID])
        {
            Debug.Log($"Skill {skillID} already unlocked!");
            return;
        }
        
        unlockedSkills[skillID] = true;
        PlayerPrefs.SetInt($"skill_unlocked_{skillID}", 1);
        PlayerPrefs.Save();
        
        OnSkillUnlocked?.Invoke(skillID);
        Debug.Log($"Unlocked skill: {skillID}");
    }
    
    /// <summary>
    /// Check if skill is unlocked
    /// </summary>
    public bool IsSkillUnlocked(string skillID)
    {
        if (!unlockedSkills.ContainsKey(skillID))
            return false;
        
        return unlockedSkills[skillID];
    }
    
    /// <summary>
    /// Update all active cooldowns
    /// </summary>
    void UpdateCooldowns()
    {
        List<string> keys = new List<string>(cooldownTimers.Keys);
        
        foreach (string skillID in keys)
        {
            if (cooldownTimers[skillID] > 0)
            {
                cooldownTimers[skillID] -= Time.deltaTime;
                
                // Fire event for UI update
                OnCooldownTick?.Invoke(skillID, cooldownTimers[skillID]);
            }
        }
    }
    
    /// <summary>
    /// Get remaining cooldown time for a skill
    /// </summary>
    public float GetCooldownRemaining(string skillID)
    {
        if (!cooldownTimers.ContainsKey(skillID))
            return 0f;
        
        return Mathf.Max(0f, cooldownTimers[skillID]);
    }
    
    /// <summary>
    /// Check if ultimate can be activated
    /// </summary>
    bool CanActivateUltimate(ST_SkillSO skill)
    {
        // Check if unlocked
        if (!IsSkillUnlocked(skill.id))
            return false;
        
        // Check if on cooldown
        if (GetCooldownRemaining(skill.id) > 0)
            return false;
        
        // Check mana cost (if applicable)
        if (skill.manaCost > 0)
        {
            C_Mana mana = FindFirstObjectByType<C_Mana>();
            if (mana && mana.currentMP < skill.manaCost)
                return false;
        }
        
        // Check if player is dead
        C_Health health = FindFirstObjectByType<C_Health>();
        if (health && health.isDead)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Activate Ultimate 1 (Dagger Barrage)
    /// </summary>
    void ActivateUltimate1(InputAction.CallbackContext ctx)
    {
        ST_SkillSO skill = FindSkillByID("dagger_barrage");
        if (skill != null)
            ActivateUltimate(skill);
    }
    
    /// <summary>
    /// Activate Ultimate 2 (Divine Intervention)
    /// </summary>
    void ActivateUltimate2(InputAction.CallbackContext ctx)
    {
        ST_SkillSO skill = FindSkillByID("divine_intervention");
        if (skill != null)
            ActivateUltimate(skill);
    }
    
    /// <summary>
    /// Activate an ultimate skill
    /// </summary>
    void ActivateUltimate(ST_SkillSO skill)
    {
        if (!CanActivateUltimate(skill))
        {
            Debug.Log($"Cannot activate {skill.skillName}");
            return;
        }
        
        // Start cooldown
        cooldownTimers[skill.id] = skill.cooldown;
        
        // Consume mana if required
        if (skill.manaCost > 0)
        {
            C_Mana mana = FindFirstObjectByType<C_Mana>();
            if (mana)
                mana.currentMP -= skill.manaCost;
        }
        
        // Get skill level from slot
        int skillLevel = GetSkillLevel(skill.id);
        
        // Trigger specific ultimate
        if (skill.id == "dagger_barrage" && daggerBarrage)
        {
            daggerBarrage.Activate(skillLevel);
        }
        else if (skill.id == "divine_intervention" && divineIntervention)
        {
            divineIntervention.Activate(skillLevel);
        }
        
        // Fire event for UI
        OnUltimateActivated?.Invoke(skill.id, skill.cooldown);
        
        Debug.Log($"Activated {skill.skillName} (Level {skillLevel})");
    }
    
    /// <summary>
    /// Get current level of a skill
    /// </summary>
    public int GetSkillLevel(string skillID)
    {
        foreach (var slot in st_Slots)
        {
            if (slot.st_skillSO && slot.st_skillSO.id == skillID)
                return slot.currentLevel;
        }
        return 0; // Not upgraded yet
    }
    
    /// <summary>
    /// Find skill by ID
    /// </summary>
    ST_SkillSO FindSkillByID(string id)
    {
        foreach (var skill in allSkills)
        {
            if (skill.id == id)
                return skill;
        }
        return null;
    }
    
    // ... existing methods (TryToUpgrade, HandleSkillUpgraded, etc.)
}
```

### Key Additions:
1. **Unlock System**: `UnlockSkill()`, `IsSkillUnlocked()`, `LoadUnlockedSkills()`
2. **Cooldown System**: `UpdateCooldowns()`, `GetCooldownRemaining()`
3. **Activation Logic**: `ActivateUltimate()`, `CanActivateUltimate()`
4. **Input Handling**: Subscribe to Ultimate1/Ultimate2 input actions
5. **Events**: Notify UI of unlocks, activations, cooldown ticks

---

## 3️⃣ ST_SkillInfo.cs (NEW)

**Purpose:** Display skill information panel (like INV_ItemInfo)

### Full Implementation (~120 lines)

```csharp
using TMPro;
using UnityEngine;

/// <summary>
/// Displays skill information (name + description)
/// Similar to INV_ItemInfo but for skills
/// </summary>
public class ST_SkillInfo : MonoBehaviour
{
    [Header("Skill Info Panel")]
    [Header("References")]
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    public TMP_Text skillNameText;
    public TMP_Text descriptionText;
    
    public Vector2 offset = new Vector2(12f, -8f);
    
    void Awake()
    {
        canvasGroup ??= GetComponent<CanvasGroup>();
        rectTransform ??= GetComponent<RectTransform>();
        
        if (!canvasGroup)
            Debug.LogError("ST_SkillInfo: CanvasGroup is missing!");
        if (!rectTransform)
            Debug.LogError("ST_SkillInfo: RectTransform is missing!");
        if (!skillNameText)
            Debug.LogError("ST_SkillInfo: skillNameText is missing!");
        if (!descriptionText)
            Debug.LogError("ST_SkillInfo: descriptionText is missing!");
    }
    
    /// <summary>
    /// Show skill information
    /// </summary>
    public void Show(ST_SkillSO skill)
    {
        if (skill == null) return;
        
        canvasGroup.alpha = 1f;
        
        skillNameText.text = skill.skillName;
        descriptionText.text = skill.description;
    }
    
    /// <summary>
    /// Hide info panel
    /// </summary>
    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// Follow mouse cursor
    /// </summary>
    public void FollowMouse(Vector2 screenPos)
    {
        rectTransform.position = (Vector3)screenPos + (Vector3)offset;
    }
}
```

### Integration with ST_Slots:

Modify `ST_Slots.cs` to show info on hover:

```csharp
// In ST_Slots.cs, add:
using UnityEngine.EventSystems;

public class ST_Slots : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    // ... existing fields ...
    
    private static ST_SkillInfo skillInfo;
    
    void Awake()
    {
        // ... existing Awake code ...
        
        if (skillInfo == null)
            skillInfo = FindFirstObjectByType<ST_SkillInfo>();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (st_skillSO && skillInfo)
            skillInfo.Show(st_skillSO);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (skillInfo)
            skillInfo.Hide();
    }
    
    public void OnPointerMove(PointerEventData eventData)
    {
        if (st_skillSO && skillInfo)
            skillInfo.FollowMouse(eventData.position);
    }
}
```

---

## 4️⃣ UltimateUI.cs (NEW)

**Purpose:** Display ultimate cooldown with radial fill

### Full Implementation (~130 lines)

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays ultimate icon + cooldown overlay
/// Event-driven updates (no Update polling)
/// </summary>
public class UltimateUI : MonoBehaviour
{
    [Header("Ultimate Cooldown Display")]
    [Header("References")]
    public ST_SkillSO ultimateSkill; // Assigned in Inspector
    public Image iconImage;
    public Image cooldownOverlay; // Radial fill (fillAmount 0-1)
    public TMP_Text cooldownText; // "15s" remaining
    public GameObject lockedOverlay; // Grey overlay when locked
    
    private ST_Manager skillManager;
    
    void Awake()
    {
        skillManager = FindFirstObjectByType<ST_Manager>();
        
        if (!ultimateSkill)
            Debug.LogError("UltimateUI: ultimateSkill is not assigned!");
        if (!iconImage)
            Debug.LogError("UltimateUI: iconImage is missing!");
        if (!cooldownOverlay)
            Debug.LogError("UltimateUI: cooldownOverlay is missing!");
        if (!skillManager)
            Debug.LogError("UltimateUI: ST_Manager not found!");
        
        // Set icon from skill SO
        if (ultimateSkill && iconImage)
            iconImage.sprite = ultimateSkill.skillIcon;
    }
    
    void OnEnable()
    {
        ST_Manager.OnSkillUnlocked += HandleSkillUnlocked;
        ST_Manager.OnCooldownTick += HandleCooldownTick;
        ST_Manager.OnUltimateActivated += HandleUltimateActivated;
        
        // Initialize locked state
        UpdateLockedState();
    }
    
    void OnDisable()
    {
        ST_Manager.OnSkillUnlocked -= HandleSkillUnlocked;
        ST_Manager.OnCooldownTick -= HandleCooldownTick;
        ST_Manager.OnUltimateActivated -= HandleUltimateActivated;
    }
    
    /// <summary>
    /// Update locked/unlocked visual state
    /// </summary>
    void UpdateLockedState()
    {
        if (skillManager == null || ultimateSkill == null) return;
        
        bool isUnlocked = skillManager.IsSkillUnlocked(ultimateSkill.id);
        
        if (lockedOverlay)
            lockedOverlay.SetActive(!isUnlocked);
        
        // Grey out icon when locked
        if (iconImage)
            iconImage.color = isUnlocked ? Color.white : Color.grey;
    }
    
    /// <summary>
    /// Handle skill unlock event
    /// </summary>
    void HandleSkillUnlocked(string skillID)
    {
        if (ultimateSkill && skillID == ultimateSkill.id)
        {
            UpdateLockedState();
            // Optional: Play unlock animation/sound
        }
    }
    
    /// <summary>
    /// Handle cooldown tick event
    /// </summary>
    void HandleCooldownTick(string skillID, float remaining)
    {
        if (ultimateSkill && skillID == ultimateSkill.id)
        {
            UpdateCooldownDisplay(remaining);
        }
    }
    
    /// <summary>
    /// Handle ultimate activation event
    /// </summary>
    void HandleUltimateActivated(string skillID, float cooldown)
    {
        if (ultimateSkill && skillID == ultimateSkill.id)
        {
            UpdateCooldownDisplay(cooldown);
            // Optional: Play activation feedback (flash, sound)
        }
    }
    
    /// <summary>
    /// Update cooldown visual (radial fill + text)
    /// </summary>
    void UpdateCooldownDisplay(float remaining)
    {
        if (remaining > 0)
        {
            // Show cooldown
            float fillAmount = remaining / ultimateSkill.cooldown;
            
            if (cooldownOverlay)
                cooldownOverlay.fillAmount = fillAmount;
            
            if (cooldownText)
                cooldownText.text = Mathf.Ceil(remaining).ToString() + "s";
        }
        else
        {
            // Ready to use
            if (cooldownOverlay)
                cooldownOverlay.fillAmount = 0f;
            
            if (cooldownText)
                cooldownText.text = "";
        }
    }
}
```

### Notes:
- Attach to each ultimate cooldown UI element
- Assign respective `ST_SkillSO` (Dagger Barrage or Divine Intervention)
- Radial fill overlay shows cooldown progress visually
- Text shows remaining seconds numerically

---

## 5️⃣ P_UltimateAnimator.cs (NEW)

**Purpose:** Control child GameObject with separate Animator for ultimate effects

### GameObject Structure:
```
Player (GameObject)
├── Sprite (existing Animator - movement, attacks, dodge)
└── UltimateEffects (NEW child GameObject)
    ├── Animator (ultimate animations)
    └── Optional: Particle Systems
```

### Full Implementation (~100 lines)

```csharp
using UnityEngine;

/// <summary>
/// Controls child GameObject with separate Animator for ultimate effects
/// Prevents animation conflicts with main combat animator
/// </summary>
public class P_UltimateAnimator : MonoBehaviour
{
    [Header("Ultimate Animation Controller")]
    [Header("References")]
    public Animator ultimateAnimator; // On child GameObject
    public ParticleSystem[] particleEffects; // Optional
    
    void Awake()
    {
        // Auto-find animator on child if not assigned
        if (!ultimateAnimator)
        {
            Transform child = transform.Find("UltimateEffects");
            if (child)
                ultimateAnimator = child.GetComponent<Animator>();
        }
        
        if (!ultimateAnimator)
            Debug.LogError("P_UltimateAnimator: Animator not found on child GameObject!");
        
        // Auto-find particle systems
        if (particleEffects == null || particleEffects.Length == 0)
        {
            particleEffects = GetComponentsInChildren<ParticleSystem>();
        }
    }
    
    /// <summary>
    /// Play ultimate animation by trigger name
    /// </summary>
    public void PlayUltimateAnimation(string triggerName)
    {
        if (ultimateAnimator)
        {
            ultimateAnimator.SetTrigger(triggerName);
        }
    }
    
    /// <summary>
    /// Play specific particle effect by index
    /// </summary>
    public void PlayParticleEffect(int index)
    {
        if (particleEffects != null && index < particleEffects.Length)
        {
            particleEffects[index].Play();
        }
    }
    
    /// <summary>
    /// Stop all particle effects
    /// </summary>
    public void StopAllParticles()
    {
        if (particleEffects != null)
        {
            foreach (var particle in particleEffects)
            {
                particle.Stop();
            }
        }
    }
}
```

### Animator Controller Setup:

**Create new Animator Controller: `UltimateAnimator.controller`**

**States:**
- **Idle** (default state)
- **Dagger_Cast** (2 second animation)
- **Divine_Glow** (looping animation)

**Triggers:**
- `Dagger_Cast` → transitions to Dagger_Cast state → auto-return to Idle
- `Divine_Glow` → transitions to Divine_Glow state → auto-return to Idle

**Why Separate Animator?**
- Main animator handles combat (idle, move, attack, dodge)
- Ultimate animator handles special effects independently
- No animation conflicts or interruptions
- Reusable for future buffs/debuffs/status effects

---

## 6️⃣ C_Health.cs (MODIFY)

**Purpose:** Add invulnerability system for Divine Intervention

### Changes Required (~30 lines added)

```csharp
public class C_Health : MonoBehaviour
{
    // ... existing fields (currentHP, isDead, etc.)
    
    // NEW: Invulnerability (for Divine Intervention ultimate)
    private bool isInvulnerable = false;
    
    // NEW: Optional event for UI feedback
    public static event System.Action<bool> OnInvulnerabilityChanged;
    
    // ... existing Awake, OnEnable, etc.
    
    /// <summary>
    /// Set invulnerability state (called by ULT_DivineIntervention)
    /// </summary>
    public void SetInvulnerable(bool state)
    {
        isInvulnerable = state;
        OnInvulnerabilityChanged?.Invoke(state);
        
        Debug.Log($"Invulnerability: {state}");
    }
    
    /// <summary>
    /// Apply damage (existing method - ADD invulnerability check)
    /// </summary>
    public float ApplyDamage(float ad, float ap, float weaponAD, float weaponAP, float armorPen, float magicPen)
    {
        // NEW: Check invulnerability
        if (isDead || IsDodging || isInvulnerable) return 0f;
        
        // ... existing damage calculation code ...
    }
    
    /// <summary>
    /// Change health directly (existing method - ADD invulnerability check for damage)
    /// </summary>
    public void ChangeHealth(float amount)
    {
        // NEW: Invulnerability only blocks damage, not healing
        if (amount < 0 && (isDead || IsDodging || isInvulnerable)) return;
        
        // ... existing health change code ...
    }
    
    // ... existing methods (Kill, HandleDeath, etc.)
}
```

### Key Changes:
1. Add `isInvulnerable` private field
2. Add `SetInvulnerable(bool)` public method
3. Check `isInvulnerable` in `ApplyDamage()` and `ChangeHealth()`
4. Optional: Event for UI feedback (shield icon, etc.)

---

## 7️⃣ ULT_DivineIntervention.cs (NEW)

**Purpose:** Grant temporary invulnerability with visual effects

### Full Implementation (~140 lines)

```csharp
using System.Collections;
using UnityEngine;

/// <summary>
/// Divine Intervention Ultimate - Grants temporary invulnerability
/// </summary>
public class ULT_DivineIntervention : MonoBehaviour
{
    [Header("Divine Intervention Settings")]
    [Header("Duration Scaling")]
    public float baseDuration = 2f; // Level 1
    public float durationPerLevel = 0.5f; // +0.5s per level
    
    [Header("Visual Effects")]
    public GameObject auraPrefab; // Golden glow effect
    public Color invulnerableColor = new Color(1f, 1f, 0.5f, 1f); // Golden tint
    
    [Header("References")]
    private C_Health playerHealth;
    private SpriteRenderer playerSprite;
    private P_UltimateAnimator ultimateAnimator;
    private GameObject activeAura;
    private Color originalColor;
    
    void Awake()
    {
        playerHealth = GetComponent<C_Health>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        ultimateAnimator = GetComponentInChildren<P_UltimateAnimator>();
        
        if (!playerHealth)
            Debug.LogError("ULT_DivineIntervention: C_Health is missing!");
        if (!playerSprite)
            Debug.LogWarning("ULT_DivineIntervention: SpriteRenderer not found!");
        if (!ultimateAnimator)
            Debug.LogWarning("ULT_DivineIntervention: P_UltimateAnimator not found!");
        
        if (playerSprite)
            originalColor = playerSprite.color;
    }
    
    /// <summary>
    /// Activate Divine Intervention (called by ST_Manager)
    /// </summary>
    public void Activate(int skillLevel)
    {
        float duration = baseDuration + (skillLevel * durationPerLevel);
        // Level 0: 2s, Level 1: 2.5s, Level 2: 3s, etc.
        
        StartCoroutine(InvulnerabilityRoutine(duration));
    }
    
    /// <summary>
    /// Invulnerability coroutine
    /// </summary>
    IEnumerator InvulnerabilityRoutine(float duration)
    {
        // Enable invulnerability
        playerHealth.SetInvulnerable(true);
        
        // Spawn aura effect
        if (auraPrefab)
        {
            activeAura = Instantiate(auraPrefab, transform.position, Quaternion.identity, transform);
        }
        
        // Apply visual feedback
        if (playerSprite)
        {
            playerSprite.color = invulnerableColor; // Golden tint
        }
        
        // Play ultimate animation
        if (ultimateAnimator)
        {
            ultimateAnimator.PlayUltimateAnimation("Divine_Glow");
        }
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Disable invulnerability
        playerHealth.SetInvulnerable(false);
        
        // Remove aura effect
        if (activeAura)
        {
            Destroy(activeAura);
        }
        
        // Restore sprite color
        if (playerSprite)
        {
            playerSprite.color = originalColor;
        }
        
        Debug.Log("Divine Intervention ended");
    }
}
```

### Visual Effects:
- **Aura Prefab**: Golden glow particle system (ring, sparkles, etc.)
- **Sprite Tint**: Player sprite turns golden/white during effect
- **Animation**: Special glow animation on ultimate animator layer

---

## 8️⃣ ULT_DaggerBarrage.cs (NEW)

**Purpose:** Spawn and control dagger projectiles with complex behavior

### Full Implementation (~280 lines)

```csharp
using System.Collections;
using UnityEngine;

/// <summary>
/// Dagger Barrage Ultimate - Spawns projectiles in circle formation
/// </summary>
public class ULT_DaggerBarrage : MonoBehaviour
{
    [Header("Dagger Barrage Settings")]
    [Header("Spawn Settings")]
    public GameObject daggerPrefab;
    public int baseDaggerCount = 6; // Scales with level
    public float spawnRadius = 2f; // Distance from player
    
    [Header("Timing (for projectile behavior)")]
    public float fadeInDuration = 0.5f; // X seconds to appear
    public float waitDuration = 1f; // Y seconds to wait before firing
    public float straightFireDuration = 0.8f; // Z seconds to fire straight
    
    [Header("Damage Scaling")]
    public float baseDamage = 20f;
    public int daggersPerLevel = 2; // +2 daggers per level
    public float damagePerLevel = 5f; // +5 damage per level
    
    [Header("References")]
    private Transform player;
    private P_UltimateAnimator ultimateAnimator;
    
    void Awake()
    {
        player = transform;
        ultimateAnimator = GetComponentInChildren<P_UltimateAnimator>();
        
        if (!daggerPrefab)
            Debug.LogError("ULT_DaggerBarrage: daggerPrefab is not assigned!");
        if (!ultimateAnimator)
            Debug.LogWarning("ULT_DaggerBarrage: P_UltimateAnimator not found!");
    }
    
    /// <summary>
    /// Activate Dagger Barrage (called by ST_Manager)
    /// </summary>
    public void Activate(int skillLevel)
    {
        int daggerCount = baseDaggerCount + (skillLevel * daggersPerLevel);
        float damage = baseDamage + (skillLevel * damagePerLevel);
        // Level 0: 6 daggers, 20 dmg
        // Level 1: 8 daggers, 25 dmg
        // Level 2: 10 daggers, 30 dmg
        
        StartCoroutine(SpawnDaggersCoroutine(daggerCount, damage));
    }
    
    /// <summary>
    /// Spawn daggers in circle formation
    /// </summary>
    IEnumerator SpawnDaggersCoroutine(int count, float damage)
    {
        // Play ultimate animation
        if (ultimateAnimator)
        {
            ultimateAnimator.PlayUltimateAnimation("Dagger_Cast");
        }
        
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            // Calculate clock position (0° = north/up, 90° = east/right, etc.)
            float angle = i * angleStep;
            
            // Convert to direction vector (0° points up)
            float angleRad = (angle - 90f) * Mathf.Deg2Rad; // -90 to make 0° point up
            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            
            // Spawn position (circle around player)
            Vector2 spawnPos = (Vector2)player.position + direction * spawnRadius;
            
            // Instantiate dagger
            GameObject daggerObj = Instantiate(daggerPrefab, spawnPos, Quaternion.identity);
            ULT_DaggerProjectile dagger = daggerObj.GetComponent<ULT_DaggerProjectile>();
            
            if (dagger)
            {
                // Initialize dagger with behavior parameters
                float spawnDirectionDegrees = angle; // Direction to fire straight
                dagger.Initialize(
                    fadeInDuration,
                    waitDuration,
                    straightFireDuration,
                    spawnDirectionDegrees,
                    damage
                );
            }
            
            // Small delay between spawns (stagger effect)
            yield return new WaitForSeconds(0.05f);
        }
        
        Debug.Log($"Spawned {count} daggers");
    }
}
```

### Key Features:
1. Spawns daggers in clock positions (12, 1, 2, 3... based on count)
2. Scales with skill level (more daggers, more damage)
3. Passes timing parameters to projectiles
4. Plays cast animation via ultimate animator

---

## 9️⃣ ULT_DaggerProjectile.cs (NEW)

**Purpose:** Enhanced projectile with 3-phase behavior

### Full Implementation (~260 lines)

```csharp
using UnityEngine;

/// <summary>
/// Dagger projectile with complex 3-phase behavior:
/// 1. Fade In - Appears gradually
/// 2. Wait - Stays in place
/// 3. Fire Straight - Moves in spawn direction
/// 4. Homing - Tracks nearest enemy
/// </summary>
public class ULT_DaggerProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float damage = 20f;
    public float speed = 8f;
    
    [Header("Behavior Phases")]
    private float fadeInDuration;
    private float waitDuration;
    private float straightFireDuration;
    private float spawnDirectionDegrees;
    
    [Header("State")]
    private enum Phase { FadingIn, Waiting, FiringStraight, Homing }
    private Phase currentPhase = Phase.FadingIn;
    private float phaseTimer = 0f;
    
    [Header("Visuals")]
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trail; // NEW: Trail effect
    private float currentAlpha = 0f;
    
    [Header("Targeting")]
    private Transform target;
    private LayerMask enemyLayer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
        enemyLayer = LayerMask.GetMask("Enemy");
        
        if (!spriteRenderer)
            Debug.LogError("ULT_DaggerProjectile: SpriteRenderer is missing!");
    }
    
    /// <summary>
    /// Initialize projectile behavior
    /// </summary>
    public void Initialize(float fadeIn, float wait, float straightFire, float direction, float dmg)
    {
        fadeInDuration = fadeIn;
        waitDuration = wait;
        straightFireDuration = straightFire;
        spawnDirectionDegrees = direction;
        damage = dmg;
        
        // Start invisible
        if (spriteRenderer)
        {
            Color col = spriteRenderer.color;
            col.a = 0f;
            spriteRenderer.color = col;
        }
        
        currentPhase = Phase.FadingIn;
        phaseTimer = 0f;
    }
    
    void Update()
    {
        phaseTimer += Time.deltaTime;
        
        switch (currentPhase)
        {
            case Phase.FadingIn:
                UpdateFadeIn();
                break;
            
            case Phase.Waiting:
                UpdateWaiting();
                break;
            
            case Phase.FiringStraight:
                UpdateStraightFire();
                break;
            
            case Phase.Homing:
                UpdateHoming();
                break;
        }
        
        // Rotate sprite to face movement direction
        UpdateRotation();
    }
    
    /// <summary>
    /// Phase 1: Fade in gradually
    /// </summary>
    void UpdateFadeIn()
    {
        if (spriteRenderer)
        {
            currentAlpha = Mathf.Clamp01(phaseTimer / fadeInDuration);
            Color col = spriteRenderer.color;
            col.a = currentAlpha;
            spriteRenderer.color = col;
        }
        
        if (phaseTimer >= fadeInDuration)
        {
            currentPhase = Phase.Waiting;
            phaseTimer = 0f;
        }
    }
    
    /// <summary>
    /// Phase 2: Wait in place
    /// </summary>
    void UpdateWaiting()
    {
        // Stay stationary
        // Optional: Add pulsing glow effect here
        
        if (phaseTimer >= waitDuration)
        {
            currentPhase = Phase.FiringStraight;
            phaseTimer = 0f;
        }
    }
    
    /// <summary>
    /// Phase 3: Fire straight in spawn direction
    /// </summary>
    void UpdateStraightFire()
    {
        // Convert spawn direction to vector
        float angleRad = (spawnDirectionDegrees - 90f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        
        // Move in straight line
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        
        if (phaseTimer >= straightFireDuration)
        {
            // Transition to homing
            target = FindClosestEnemy();
            currentPhase = Phase.Homing;
            phaseTimer = 0f;
        }
    }
    
    /// <summary>
    /// Phase 4: Home to nearest enemy
    /// </summary>
    void UpdateHoming()
    {
        // If no target or target is dead, find new one
        if (target == null || IsTargetDead(target))
        {
            target = FindClosestEnemy();
            
            // If still no target, destroy after timeout
            if (target == null)
            {
                Destroy(gameObject, 2f);
                return;
            }
        }
        
        // Move toward target
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }
    
    /// <summary>
    /// Rotate sprite to face movement direction
    /// </summary>
    void UpdateRotation()
    {
        Vector2 moveDir = Vector2.zero;
        
        if (currentPhase == Phase.FiringStraight)
        {
            float angleRad = (spawnDirectionDegrees - 90f) * Mathf.Deg2Rad;
            moveDir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }
        else if (currentPhase == Phase.Homing && target != null)
        {
            moveDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        }
        
        if (moveDir != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    /// <summary>
    /// Find closest enemy
    /// </summary>
    Transform FindClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            20f, // Search radius
            enemyLayer
        );
        
        if (enemies.Length == 0) return null;
        
        Transform closest = null;
        float closestDist = Mathf.Infinity;
        
        foreach (var enemy in enemies)
        {
            C_Health health = enemy.GetComponent<C_Health>();
            if (health != null && !health.isDead)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemy.transform;
                }
            }
        }
        
        return closest;
    }
    
    /// <summary>
    /// Check if target is dead
    /// </summary>
    bool IsTargetDead(Transform target)
    {
        C_Health health = target.GetComponent<C_Health>();
        return health == null || health.isDead;
    }
    
    /// <summary>
    /// Handle collision with enemy
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            C_Health health = collision.GetComponent<C_Health>();
            if (health != null && !health.isDead)
            {
                health.ChangeHealth(-damage);
                
                // Future: Spawn impact particles
                // Future: Play hit sound
                
                Destroy(gameObject);
            }
        }
    }
}
```

### Visual Effects:
- **Trail Renderer** (implemented) - Ghosting effect behind dagger
- **Spawn Particles** (future) - Magic sparkles when appearing
- **Glow Effect** (future) - Pulsing during wait phase
- **Impact Particles** (future) - Burst on enemy hit

---

## 🧩 Unity Setup Checklist

### Input Actions (P_InputActions.inputactions)
- [ ] Open Input Actions asset
- [ ] Go to **Player** action map
- [ ] Add 2 new actions:
  - [ ] Ultimate1 (binding: Keyboard E)
  - [ ] Ultimate2 (binding: Keyboard R)
- [ ] Save asset
- [ ] Right-click → Generate C# Class

### Skill Tree Setup
- [ ] Create 2 ultimate skill SOs:
  - [ ] Dagger Barrage
    - id: "dagger_barrage"
    - isActiveAbility: true
    - cooldown: 30
    - requiresUnlockItem: true
    - unlockItemID: "ancient_scroll"
  - [ ] Divine Intervention
    - id: "divine_intervention"
    - isActiveAbility: true
    - cooldown: 45
    - requiresUnlockItem: true
    - unlockItemID: "divine_blessing"
- [ ] Add to ST_Manager.allSkills list

### Unlock Items
- [ ] Create 2 unlock items (INV_ItemSO):
  - [ ] Ancient Scroll
    - id: "ancient_scroll"
    - unlocksSkill: true
    - skillIDToUnlock: "dagger_barrage"
  - [ ] Divine Blessing
    - id: "divine_blessing"
    - unlocksSkill: true
    - skillIDToUnlock: "divine_intervention"

### Player Setup
- [ ] Create child GameObject: "UltimateEffects"
- [ ] Add Animator to child
- [ ] Create UltimateAnimator.controller:
  - [ ] Idle state (default)
  - [ ] Dagger_Cast state (2s animation)
  - [ ] Divine_Glow state (looping)
- [ ] Add P_UltimateAnimator component to child
- [ ] Add ULT_DaggerBarrage component to Player
- [ ] Add ULT_DivineIntervention component to Player

### Dagger Projectile
- [ ] Create dagger sprite (spinning blade, 32x32px)
- [ ] Create dagger prefab:
  - [ ] SpriteRenderer
  - [ ] CircleCollider2D (trigger, radius 0.2)
  - [ ] Rigidbody2D (kinematic, no gravity)
  - [ ] TrailRenderer (ghosting effect)
  - [ ] ULT_DaggerProjectile script
- [ ] Assign prefab to ULT_DaggerBarrage.daggerPrefab

### Divine Intervention
- [ ] Create aura effect prefab (particle system):
  - [ ] Golden glow particles
  - [ ] Ring shape emitter
  - [ ] Pulsing animation
- [ ] Assign to ULT_DivineIntervention.auraPrefab

### Cooldown UI
- [ ] Create 2 UI elements on HUD (bottom-right corner):
  - [ ] Background panel
  - [ ] Icon image (ultimate sprite)
  - [ ] Cooldown overlay (Image, type: Filled, Radial 360, clockwise)
  - [ ] Cooldown text (TMP)
  - [ ] Locked overlay (grey panel)
- [ ] Add UltimateUI component to each
- [ ] Assign respective ultimate skill SO

### Skill Info Panel
- [ ] Create info panel UI (similar to INV_ItemInfo):
  - [ ] Background panel
  - [ ] Skill name text (header)
  - [ ] Description text (body, word wrap)
- [ ] Add ST_SkillInfo component
- [ ] Link references
- [ ] Modify ST_Slots to show info on hover

---

## 📊 Testing Scenarios

### Ultimate System Testing
1. **Lock/Unlock**
   - Try activating locked ultimates (should fail)
   - Use unlock items
   - Verify UI unlocks (greyed out → colored)

2. **Cooldowns**
   - Activate ultimate
   - Verify cooldown prevents re-activation
   - Watch cooldown UI update (radial fill, timer text)

3. **Dagger Barrage**
   - Spawns correct number of daggers (6 base, +2 per level)
   - Daggers fade in → wait → fire straight → home to enemy
   - All daggers target closest enemy
   - Damage scales with level

4. **Divine Intervention**
   - Invulnerability blocks all damage
   - Visual effects appear (aura, glow, tint)
   - Duration scales with level
   - Effects end correctly

5. **Animations**
   - Ultimate animations don't interrupt combat
   - Player can move during cast
   - Separate animator works independently

### Edge Cases
- [ ] Activate ultimate while attacking
- [ ] Activate ultimate while dodging
- [ ] Take damage during Divine Intervention
- [ ] Spawn daggers with no enemies nearby
- [ ] Kill target enemy while dagger is homing
- [ ] Activate ultimate with insufficient mana (if mana cost enabled)

---

## 🔄 Future Enhancements

### Additional Ultimate Ideas
1. **Shadow Clone** - Spawn AI clones that mirror attacks
2. **Time Dilation** - Slow enemies, speed up player
3. **Life Drain Aura** - Damage enemies, heal player in radius

### Skill Tree Integration
- Display ultimates in skill tree UI
- Upgrade system (increase level with skill points)
- Visual indicators (locked icon, key binding hint)

### Visual Polish
- Spawn particles for daggers (magic sparkles)
- Glow effect during wait phase
- Impact particles on hit
- Screen effects (flash, shake, etc.)

---

## ✅ Completion Criteria

- [ ] Can activate ultimates with E/R keys (when unlocked)
- [ ] Unlock items grant ultimate skills
- [ ] Cooldown UI displays accurate remaining time
- [ ] Skill info panel shows descriptions on hover
- [ ] Dagger Barrage spawns correctly at clock positions
- [ ] Daggers fade in → wait → fire straight → home
- [ ] Divine Intervention grants invulnerability
- [ ] Ultimate animations play without blocking movement
- [ ] All systems work together without conflicts
- [ ] No major bugs or crashes

---

## 📝 Code Summary

### Files Modified (3)
1. **ST_SkillSO.cs** - Active ability fields (~40 lines)
2. **ST_Manager.cs** - Unlock/cooldown/activation (~250 lines)
3. **C_Health.cs** - Invulnerability (~30 lines)

### Files Created (6)
1. **ST_SkillInfo.cs** - Skill info panel (~120 lines)
2. **UltimateUI.cs** - Cooldown display (~130 lines)
3. **P_UltimateAnimator.cs** - Animation controller (~100 lines)
4. **ULT_DaggerBarrage.cs** - Barrage logic (~280 lines)
5. **ULT_DaggerProjectile.cs** - Enhanced projectile (~260 lines)
6. **ULT_DivineIntervention.cs** - Invulnerability (~140 lines)

### Total Code: ~1,350 lines

**Implementation Time Estimate: 6-8 days**

**This system provides flashy, satisfying ultimate abilities that enhance combat without overwhelming complexity!** ⚡✨
