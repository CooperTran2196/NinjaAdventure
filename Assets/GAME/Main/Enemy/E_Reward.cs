using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class LootDrop
{
    [Header("Reward Type (set ONE)")]
    public INV_ItemSO item;
    public W_SO       weapon;
    
    [Header("Quantity (items only)")]
    public int quantity = 1;
}

public class E_Reward : MonoBehaviour
{
    [Header("References")]
    C_Health c_Health;

    [Header("Rewards")]
    public int expReward = 10;

    [Header("Loot")]
                    public GameObject      lootPrefab;
                    public GameObject      weaponPrefab;
                    public float           dropSpread    = 0.75f;
    [Range(0, 100)] public float           dropChance    = 50f;
                    public int             numberOfDrops = 1;
                    public List<LootDrop>  lootTable;

    // Static reference
    static P_Exp p_Exp;

    void Awake()
    {
        c_Health ??= GetComponentInParent<C_Health>();
        
        if (!c_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
    }

    void Start()
    {
        p_Exp = GameObject.FindGameObjectWithTag("Player")?.GetComponent<P_Exp>();
        
        if (!p_Exp) { Debug.LogError($"{name}: P_Exp is missing on Player!", this); return; }
    }

    void OnEnable()
    {
        c_Health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        c_Health.OnDied -= HandleDied;
    }

    // Grant EXP/kill on death and spawn loot based on drop chance
    void HandleDied()
    {
        p_Exp?.AddXP(expReward);
        p_Exp?.AddKill();

        // Handle loot drops
        if (lootPrefab == null || lootTable.Count == 0) return;
        if (Random.Range(0f, 100f) > dropChance) return;

        // Determine which items to drop
        List<LootDrop> itemsToDrop = new List<LootDrop>();
        if (numberOfDrops >= lootTable.Count)
        {
            itemsToDrop.AddRange(lootTable);
        }
        else
        {
            itemsToDrop = lootTable.OrderBy(x => Random.value).Take(numberOfDrops).ToList();
        }

        // Spawn the loot items
        foreach (var lootDrop in itemsToDrop)
        {
            SpawnLoot(lootDrop);
        }
    }

    // Spawns loot at random position within dropSpread
    void SpawnLoot(LootDrop lootDrop)
    {
        if (lootDrop == null) return;
        
        // Validate that at least one reward type is set
        if (lootDrop.weapon == null && lootDrop.item == null) return;
        if (lootDrop.item != null && lootDrop.quantity <= 0) return;

        float randomX = Random.Range(-dropSpread, dropSpread);
        Vector2 spawnPos = new Vector2(transform.position.x + randomX, transform.position.y);

        // Spawn weapon if specified
        if (lootDrop.weapon != null)
        {
            if (weaponPrefab == null) return;
            GameObject lootGO = Instantiate(weaponPrefab, spawnPos, Quaternion.identity);
            INV_Loot loot = lootGO.GetComponent<INV_Loot>();
            if (loot != null)
            {
                // InitializeWeapon already has 1.5f delay to ensure stable spawn before pickup
                loot.InitializeWeapon(lootDrop.weapon);
            }
        }
        // Otherwise spawn item if specified
        else if (lootDrop.item != null)
        {
            if (lootPrefab == null) return;
            GameObject lootGO = Instantiate(lootPrefab, spawnPos, Quaternion.identity);
            INV_Loot loot = lootGO.GetComponent<INV_Loot>();
            if (loot != null)
            {
                // Initialize already has 1.5f delay to ensure stable spawn before pickup
                loot.Initialize(lootDrop.item, lootDrop.quantity);
            }
        }
    }
}
