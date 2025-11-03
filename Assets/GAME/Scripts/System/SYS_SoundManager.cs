using UnityEngine;

[DisallowMultipleComponent]
public class SYS_SoundManager : MonoBehaviour
{
    [Header("Centralized audio manager for all game sound effects with AudioSource pooling")]
    [Header("MUST wire MANUALLY in Inspector")]
    public AudioClip   comboSlash1;
    public AudioClip   comboSlash2;
    public AudioClip   comboSlash3;
    public AudioClip   playerHit;
    public AudioClip   enemyHit;
    public AudioClip   dodge;
    public AudioClip   rangedAttack;
    public AudioClip   instantHeal;
    public AudioClip   overtimeHeal;
    public AudioClip   buffAttack;
    public AudioClip   buffDefense;
    public AudioClip   buffGeneric;
    public AudioClip   debuff;
    public AudioClip   levelUp;
    public AudioClip   skillUpgrade;
    public AudioClip   itemPickup_Tier1;
    public AudioClip   itemPickup_Tier2;
    public AudioClip   goldPickup;
    public AudioClip   weaponChange;
    public AudioClip   dropItem;
    public AudioClip   buyItem;
    public AudioClip   sellItem;
    public AudioClip   openSkillTree;
    public AudioClip   openStats;
    public AudioClip   openShop;
    public AudioClip[] npcTalkSounds;
    public AudioClip   closePanel;
    public AudioClip   buttonClick;
    public AudioClip[] grassBreak;
    public AudioClip[] vaseBreak;
    public AudioClip[] objectBreak;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume    = 0.7f;
    [Range(0f, 1f)] public float combatVolume    = 1.0f;
    [Range(0f, 1f)] public float uiVolume        = 0.8f;
    [Range(0f, 1f)] public float effectVolume    = 0.9f;
    [Range(0f, 1f)] public float enemyVolumeMult = 0.6f;

    // Runtime state
    AudioSource[] pool;
    int           poolIndex;

    void Awake()
    {
        pool = new AudioSource[8];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake  = false;
            pool[i].spatialBlend = 0f;
        }
    }

    void PlaySound(AudioClip clip, float volumeMult = 1f)
    {
        if (!clip) { Debug.LogWarning($"{name}: Attempted to play null AudioClip", this); return; }

        AudioSource source = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Length;

        source.volume = masterVolume * volumeMult;
        source.PlayOneShot(clip);
    }

    AudioClip GetRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"{name}: Attempted to get random from null/empty array", this);
            return null;
        }
        return clips[Random.Range(0, clips.Length)];
    }

    
    // COMBAT
    public void PlayComboSlash(int hitIndex)
    {
        AudioClip clip = hitIndex switch
        {
            0 => comboSlash1,
            1 => comboSlash2,
            2 => comboSlash3,
            _ => comboSlash1
        };
        PlaySound(clip, combatVolume);
    }

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

    public void PlayPlayerHit()    => PlaySound(playerHit, combatVolume);
    public void PlayEnemyHit()     => PlaySound(enemyHit, combatVolume * enemyVolumeMult);
    public void PlayDodge()        => PlaySound(dodge, combatVolume);
    public void PlayRangedAttack() => PlaySound(rangedAttack, combatVolume);
    
    // DIALOG
    public void PlayNPCTalk()   => PlaySound(GetRandom(npcTalkSounds), uiVolume);
    public void PlayDialogEnd() => PlaySound(closePanel, uiVolume);
    
    
    // HEALING
    public void PlayInstantHeal()  => PlaySound(instantHeal, effectVolume);
    public void PlayOvertimeHeal() => PlaySound(overtimeHeal, effectVolume);

    
    // STAT BUFFS
    public void PlayBuffAttack()  => PlaySound(buffAttack, effectVolume);
    public void PlayBuffDefense() => PlaySound(buffDefense, effectVolume);
    public void PlayBuffGeneric() => PlaySound(buffGeneric, effectVolume);
    public void PlayDebuff()      => PlaySound(debuff, effectVolume);

    
    // PROGRESSION
    public void PlayLevelUp()      => PlaySound(levelUp, effectVolume * 1.2f);
    public void PlaySkillUpgrade() => PlaySound(skillUpgrade, effectVolume);
    
    
    // INVENTORY
    public void PlayItemPickup(int tier)
    {
        AudioClip clip = tier == 1 ? itemPickup_Tier1 : itemPickup_Tier2;
        PlaySound(clip, effectVolume);
    }

    public void PlayGoldPickup()   => PlaySound(goldPickup, effectVolume * 0.7f);
    public void PlayWeaponChange() => PlaySound(weaponChange, uiVolume);
    public void PlayDropItem()     => PlaySound(dropItem, uiVolume * 0.6f);

    // SHOP
    public void PlayBuyItem()  => PlaySound(buyItem, uiVolume);
    public void PlaySellItem() => PlaySound(sellItem, uiVolume);
    
    // UI
    public void PlayOpenSkillTree() => PlaySound(openSkillTree, uiVolume);
    public void PlayOpenStats()     => PlaySound(openStats, uiVolume);
    public void PlayOpenShop()      => PlaySound(openShop, uiVolume);
    public void PlayClosePanel()    => PlaySound(closePanel, uiVolume);
    public void PlayButtonClick()   => PlaySound(buttonClick, uiVolume);

    // DESTRUCTIBLES
    public AudioClip GetRandomGrassBreak()  => GetRandom(grassBreak);
    public AudioClip GetRandomVaseBreak()   => GetRandom(vaseBreak);
    public AudioClip GetRandomObjectBreak() => GetRandom(objectBreak);
}
