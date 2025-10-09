using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class LootDrop
{
    public INV_ItemSO item;
    [Header("How many of this item to drop.")]
    public int quantity = 1;
}

public class E_Reward : MonoBehaviour
{
    [Header("Rewards")]
    public int expReward = 10; // tweak per enemy prefab

    [Header("Loot")]
    [Tooltip("The prefab with INV_Loot script to spawn when dropping items.")]
    public GameObject lootPrefab;
    [Tooltip("The horizontal distance items can spread out when dropped.")]
    public float dropSpread = 0.75f;
    [Tooltip("Chance (0-100%) to drop any items at all.")]
    [Range(0, 100)] public float dropChance = 50f;
    [Tooltip("How many different items to drop from the loot table.")]
    public int numberOfDrops = 1;
    [Tooltip("A list of all possible items this enemy can drop.")]
    public List<LootDrop> lootTable;

    C_Health c_Health;
    static P_Exp p_Exp;

    void Awake()
    {
        c_Health ??= GetComponentInParent<C_Health>();
        if (!p_Exp) p_Exp = FindFirstObjectByType<P_Exp>();

        if (!c_Health) Debug.LogError($"{name}: C_Health is missing in E_Reward");
        if (!p_Exp) Debug.LogError($"{name}: P_Exp is missing in E_Reward");
    }

    void OnEnable()
    {
        c_Health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        c_Health.OnDied -= HandleDied;
    }

    void HandleDied()
    {
        // Grant EXP
        p_Exp?.AddXP(expReward);

        // --- Handle Loot Drops ---
        if (lootPrefab == null || lootTable.Count == 0) return;

        // 1. Check if we should drop anything based on drop chance
        if (Random.Range(0f, 100f) > dropChance) return;

        // 2. Determine which items to drop
        List<LootDrop> itemsToDrop = new List<LootDrop>();
        if (numberOfDrops >= lootTable.Count)
        {
            // Drop all items if requested number is greater or equal
            itemsToDrop.AddRange(lootTable);
        }
        else
        {
            // Select a random subset of unique items to drop
            itemsToDrop = lootTable.OrderBy(x => Random.value).Take(numberOfDrops).ToList();
        }

        // 3. Spawn the loot items
        foreach (var lootDrop in itemsToDrop)
        {
            SpawnLoot(lootDrop);
        }
    }

    void SpawnLoot(LootDrop lootDrop)
    {
        if (lootDrop == null || lootDrop.item == null || lootDrop.quantity <= 0) return;

        // Calculate a random spawn position with a horizontal offset
        float randomX = Random.Range(-dropSpread, dropSpread);
        Vector2 spawnPos = new Vector2(transform.position.x + randomX, transform.position.y);

        // Instantiate the loot prefab at the calculated position
        GameObject lootGO = Instantiate(lootPrefab, spawnPos, Quaternion.identity);
        
        // Initialize the loot item
        INV_Loot loot = lootGO.GetComponent<INV_Loot>();
        if (loot != null)
        {
            // Initialize will now handle the animation trigger and pickup delay
            loot.Initialize(lootDrop.item, lootDrop.quantity);
        }
    }
}
