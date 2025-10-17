using UnityEngine;

// Centralized audio manager for all game sound effects with AudioSource pooling
public class SYS_SoundManager : MonoBehaviour
{
    // ===== COMBAT - 3-HIT COMBO =====
    [Header("Combat - 3-Hit Combo")]
    [Tooltip("Slash2.wav - First hit in combo")]
    public AudioClip comboSlash1;
    
    [Tooltip("Slash3.wav - Second hit in combo")]
    public AudioClip comboSlash2;
    
    [Tooltip("Slash5.wav - Third hit in combo")]
    public AudioClip comboSlash3;
    
    // ===== COMBAT - HIT FEEDBACK =====
    [Header("Combat - Hit Feedback")]
    [Tooltip("Hit9.wav - Player takes damage")]
    public AudioClip playerHit;
    
    [Tooltip("Impact2.wav - Enemy takes damage (quieter due to multiple enemies)")]
    public AudioClip enemyHit;
    
    [Tooltip("Whoosh.wav - Dodge roll")]
    public AudioClip dodge;
    
    [Tooltip("Slash.wav - Ranged attack")]
    public AudioClip rangedAttack;
    
    // ===== DIALOG =====
    [Header("Dialog")]
    [Tooltip("Voice5-Voice10 (6 clips) - Random NPC talk sounds")]
    public AudioClip[] npcTalkSounds = new AudioClip[6];
    
    // ===== HEALING =====
    [Header("Healing")]
    [Tooltip("Heal.wav - Instant heal (health potion)")]
    public AudioClip instantHeal;
    
    [Tooltip("Heal2.wav - Heal over time (regen effects)")]
    public AudioClip overtimeHeal;
    
    // ===== STAT BUFFS =====
    [Header("Item Effects - Stat Buffs")]
    [Tooltip("Magic1.wav - Attack/Magic buff (AD/AP increase)")]
    public AudioClip buffAttack;
    
    [Tooltip("Magic5.wav - Defense buff (AR/MR increase)")]
    public AudioClip buffDefense;
    
    [Tooltip("Magic2.wav - Generic stat buffs")]
    public AudioClip buffGeneric;
    
    [Tooltip("PLACEHOLDER - For future debuff implementation")]
    public AudioClip debuff;
    
    // ===== PROGRESSION =====
    [Header("Progression")]
    [Tooltip("Fx.wav - Player levels up")]
    public AudioClip levelUp;
    
    [Tooltip("Magic2.wav - Skill tree upgrade")]
    public AudioClip skillUpgrade;
    
    // ===== INVENTORY - ITEM PICKUP =====
    [Header("Inventory - Item Pickup by Tier")]
    [Tooltip("Bonus2.wav - Common items (Tier 1)")]
    public AudioClip itemPickup_Tier1;
    
    [Tooltip("Bonus3.wav - Rare items (Tier 2)")]
    public AudioClip itemPickup_Tier2;
    
    [Tooltip("Coin2.wav - Gold/currency pickup")]
    public AudioClip goldPickup;
    
    // ===== INVENTORY - ACTIONS =====
    [Header("Inventory - Actions")]
    [Tooltip("Bubble2.wav - Equip/change weapon")]
    public AudioClip weaponChange;
    
    [Tooltip("Water5.wav - Drop item")]
    public AudioClip dropItem;
    
    // ===== SHOP =====
    [Header("Shop")]
    [Tooltip("Accept5.wav - Purchase item")]
    public AudioClip buyItem;
    
    [Tooltip("Accept7.wav - Sell item")]
    public AudioClip sellItem;
    
    // ===== UI - PANELS =====
    [Header("UI - Panels")]
    [Tooltip("Menu5.wav - Open skill tree")]
    public AudioClip openSkillTree;
    
    [Tooltip("Menu5.wav - Open stats panel")]
    public AudioClip openStats;
    
    [Tooltip("Accept.wav - Open shop")]
    public AudioClip openShop;
    
    [Tooltip("Cancel2.wav - Universal panel close")]
    public AudioClip closePanel;
    
    // ===== UI - BUTTONS =====
    [Header("UI - Buttons")]
    [Tooltip("Menu8.wav - Button click")]
    public AudioClip buttonClick;
    
    // ===== DESTRUCTIBLES =====
    [Header("Destructible Pools - Random Selection")]
    [Tooltip("Grass.wav, Grass2.wav - Grass break sounds (array size: 2)")]
    public AudioClip[] grassBreak = new AudioClip[2];
    
    [Tooltip("Water1.wav, Water2.wav - Vase break sounds (array size: 2)")]
    public AudioClip[] vaseBreak = new AudioClip[2];
    
    [Tooltip("Explosion.wav - Explosion6.wav - Object break sounds (array size: 6)")]
    public AudioClip[] objectBreak = new AudioClip[6];
    
    // ===== VOLUME SETTINGS =====
    [Header("Volume Settings")]
    [Range(0f, 1f)] [Tooltip("Master volume for all sounds")]
    public float masterVolume = 0.7f;
    
    [Range(0f, 1f)] [Tooltip("Combat sounds (attacks, hits)")]
    public float combatVolume = 1.0f;
    
    [Range(0f, 1f)] [Tooltip("UI sounds (buttons, panels)")]
    public float uiVolume = 0.8f;
    
    [Range(0f, 1f)] [Tooltip("Effect sounds (healing, buffs, progression)")]
    public float effectVolume = 0.9f;
    
    [Range(0f, 1f)] [Tooltip("Enemy sound volume multiplier")]
    public float enemyVolumeMult = 0.6f;
    
    // ===== INTERNAL =====
    AudioSource[] pool;
    int poolIndex = 0;
    
    void Awake()
    {
        // Create AudioSource pool (8 sources for simultaneous sounds)
        pool = new AudioSource[8];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 0f;  // 2D sound
        }
        
        Debug.Log($"[SYS_SoundManager] Initialized with {pool.Length} AudioSources");
    }
    
    // Core playback method - plays sound through AudioSource pool
    void PlaySound(AudioClip clip, float volumeMult = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SYS_SoundManager] Attempted to play null AudioClip");
            return;
        }
        
        AudioSource source = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Length;
        
        source.volume = masterVolume * volumeMult;
        source.PlayOneShot(clip);
    }
    
    // Get random clip from array
    AudioClip GetRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("[SYS_SoundManager] Attempted to get random from null/empty array");
            return null;
        }
        return clips[Random.Range(0, clips.Length)];
    }
    
    // ========================================
    // PUBLIC API - COMBAT
    // ========================================
    
    // Play combo slash sound for player based on hit index (0, 1, 2)
    public void PlayComboSlash(int hitIndex)
    {
        AudioClip clip = hitIndex switch
        {
            0 => comboSlash1,  // Slash2
            1 => comboSlash2,  // Slash3
            2 => comboSlash3,  // Slash5
            _ => comboSlash1
        };
        PlaySound(clip, combatVolume);
    }
    
    // Play combo slash sound for enemy (quieter - 60% volume)
    public void PlayComboSlash_Enemy(int hitIndex)
    {
        AudioClip clip = hitIndex switch
        {
            0 => comboSlash1,
            1 => comboSlash2,
            2 => comboSlash3,
            _ => comboSlash1
        };
        PlaySound(clip, combatVolume * enemyVolumeMult);
    }
    
    // Player takes damage - Hit9.wav
    public void PlayPlayerHit() => PlaySound(playerHit, combatVolume);
    
    // Enemy takes damage - Impact2.wav (quieter due to multiple enemies)
    public void PlayEnemyHit() => PlaySound(enemyHit, combatVolume * combatVolume);
    
    // Player dodge roll - Whoosh.wav
    public void PlayDodge() => PlaySound(dodge, combatVolume);
    
    // Ranged attack - Slash.wav
    public void PlayRangedAttack() => PlaySound(rangedAttack, combatVolume);
    
    // ========================================
    // PUBLIC API - DIALOG
    // ========================================
    
    // NPC talk - Random Voice5-10 sound
    public void PlayNPCTalk() => PlaySound(GetRandom(npcTalkSounds), uiVolume);
    
    // End dialog/conversation - Uses same sound as panel close (Cancel2.wav)
    public void PlayDialogEnd() => PlaySound(closePanel, uiVolume);
    
    // ========================================
    // PUBLIC API - HEALING
    // ========================================
    
    // Instant heal (health potion) - Heal.wav
    public void PlayInstantHeal() => PlaySound(instantHeal, effectVolume);
    
    // Heal over time (regen effects) - Heal2.wav
    public void PlayOvertimeHeal() => PlaySound(overtimeHeal, effectVolume);
    
    // ========================================
    // PUBLIC API - STAT BUFFS
    // ========================================
    
    // Attack/Magic buff (AD/AP increase) - Magic1.wav
    public void PlayBuffAttack() => PlaySound(buffAttack, effectVolume);
    
    // Defense buff (AR/MR increase) - Magic5.wav
    public void PlayBuffDefense() => PlaySound(buffDefense, effectVolume);
    
    // Generic stat buff - Magic2.wav
    public void PlayBuffGeneric() => PlaySound(buffGeneric, effectVolume);
    
    // Debuff - PLACEHOLDER for future implementation
    public void PlayDebuff() => PlaySound(debuff, effectVolume);
    
    // ========================================
    // PUBLIC API - PROGRESSION
    // ========================================
    
    // Player levels up - Fx.wav (slightly louder)
    public void PlayLevelUp() => PlaySound(levelUp, effectVolume * 1.2f);
    
    // Skill tree upgrade - Magic2.wav
    public void PlaySkillUpgrade() => PlaySound(skillUpgrade, effectVolume);
    
    // ========================================
    // PUBLIC API - INVENTORY
    // ========================================
    
    // Item pickup based on tier (1 = common, 2 = rare)
    public void PlayItemPickup(int tier)
    {
        AudioClip clip = tier == 1 ? itemPickup_Tier1 : itemPickup_Tier2;
        PlaySound(clip, effectVolume);
    }
    
    // Gold/currency pickup - Coin2.wav (slightly quieter)
    public void PlayGoldPickup() => PlaySound(goldPickup, effectVolume * 0.7f);
    
    // Equip/change weapon - Bubble2.wav
    public void PlayWeaponChange() => PlaySound(weaponChange, uiVolume);
    
    // Drop item - Water5.wav (quieter)
    public void PlayDropItem() => PlaySound(dropItem, uiVolume * 0.6f);
    
    // ========================================
    // PUBLIC API - SHOP
    // ========================================
    
    // Buy item from shop - Accept5.wav
    public void PlayBuyItem() => PlaySound(buyItem, uiVolume);
    
    // Sell item to shop - Accept7.wav
    public void PlaySellItem() => PlaySound(sellItem, uiVolume);
    
    // ========================================
    // PUBLIC API - UI
    // ========================================
    
    // Open skill tree panel - Menu5.wav
    public void PlayOpenSkillTree() => PlaySound(openSkillTree, uiVolume);
    
    // Open stats panel - Menu5.wav
    public void PlayOpenStats() => PlaySound(openStats, uiVolume);
    
    // Open shop - Accept.wav
    public void PlayOpenShop() => PlaySound(openShop, uiVolume);
    
    // Close any panel - Cancel2.wav (universal)
    public void PlayClosePanel() => PlaySound(closePanel, uiVolume);
    
    // Button click - Menu8.wav
    public void PlayButtonClick() => PlaySound(buttonClick, uiVolume);
    
    // ========================================
    // PUBLIC API - DESTRUCTIBLES
    // ========================================
    
    // Get random grass break sound (Grass.wav, Grass2.wav) - Returns AudioClip for AudioSource.PlayClipAtPoint()
    public AudioClip GetRandomGrassBreak() => GetRandom(grassBreak);
    
    // Get random vase break sound (Water1.wav, Water2.wav) - Returns AudioClip for AudioSource.PlayClipAtPoint()
    public AudioClip GetRandomVaseBreak() => GetRandom(vaseBreak);
    
    // Get random object break sound (Explosion.wav - Explosion6.wav) - Returns AudioClip for AudioSource.PlayClipAtPoint()
    public AudioClip GetRandomObjectBreak() => GetRandom(objectBreak);
}
