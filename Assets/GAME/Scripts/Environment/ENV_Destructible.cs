using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ENV_Destructible : MonoBehaviour
{
    public enum ObjectType { Grass, Vase, Generic }
    
    [Header("Sound Settings")]
    public ObjectType objectType = ObjectType.Generic;
    
    [Header("References")]
    C_Health       c_Health;
    C_FX           c_FX;
    SpriteRenderer sr;
    ParticleSystem breakParticleSystem;

    [Header("Loot Settings")]
                    public GameObject     lootPrefab;
                    public float          dropSpread    = 0.75f;
    [Range(0, 100)] public float          dropChance    = 50f;
                    public int            numberOfDrops = 1;
                    public List<LootDrop> lootTable;

    bool isDead;

    void Awake()
    {
        c_Health            ??= GetComponent<C_Health>();
        c_FX                ??= GetComponent<C_FX>();
        sr                  ??= GetComponent<SpriteRenderer>();
        breakParticleSystem ??= GetComponentInChildren<ParticleSystem>();

        if (!c_Health)            { Debug.LogError($"{name}: C_Health is missing!", this); return; }
        if (!c_FX)                { Debug.LogError($"{name}: C_FX is missing!", this); return; }
        if (!sr)                  { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
        if (!breakParticleSystem) { Debug.LogError($"{name}: ParticleSystem is missing (add as child)!", this); return; }

        // Ensure particle system doesn't play on start
        var main = breakParticleSystem.main;
        main.playOnAwake = false;
        breakParticleSystem.Stop();
    }

    void OnEnable()
    {
        c_Health.OnDied    += HandleDeath;
        c_Health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        c_Health.OnDied    -= HandleDeath;
        c_Health.OnDamaged -= HandleDamaged;
    }

    void HandleDamaged(int amount)
    {
        if (isDead) return;

        c_FX.FlashOnDamaged();
    }

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        sr.enabled = false;
        SpawnBreakParticles();
        HandleLootDrops();

        Destroy(gameObject, 2f);
    }

    void HandleLootDrops()
    {
        if (lootTable.Count == 0) return;
        if (Random.Range(0f, 100f) > dropChance) return;

        // Determine which items to drop
        List<LootDrop> itemsToDrop = numberOfDrops >= lootTable.Count
            ? lootTable
            : lootTable.OrderBy(x => Random.value).Take(numberOfDrops).ToList();

        // Spawn loot items
        foreach (var lootDrop in itemsToDrop)
        {
            SpawnLoot(lootDrop);
        }
    }

    void SpawnLoot(LootDrop lootDrop)
    {
        // Randomize spawn position within spread range
        float randomX = Random.Range(-dropSpread, dropSpread);
        Vector2 spawnPos = new Vector2(transform.position.x + randomX, transform.position.y);

        GameObject lootGO = Instantiate(lootPrefab, spawnPos, Quaternion.identity);
        INV_Loot loot = lootGO.GetComponent<INV_Loot>();
        loot.Initialize(lootDrop.item, lootDrop.quantity);
    }

    void SpawnBreakParticles()
    {
        breakParticleSystem.Play();
        
        // Play break sound at position with spatial audio
        AudioClip breakSound = objectType switch
        {
            ObjectType.Grass   => SYS_GameManager.Instance.sys_SoundManager.GetRandomGrassBreak(),
            ObjectType.Vase    => SYS_GameManager.Instance.sys_SoundManager.GetRandomVaseBreak(),
            ObjectType.Generic => SYS_GameManager.Instance.sys_SoundManager.GetRandomObjectBreak(),
            _                  => null
        };
        
        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position, 0.7f);
    }
}
