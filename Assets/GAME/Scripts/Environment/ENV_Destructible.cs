using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Destructible environment object (pots, crates, bushes).
/// Spawns break piece sprites with physics on death.
/// Handles loot drops internally (no E_Reward needed).
/// </summary>
[RequireComponent(typeof(C_Health))]
public class ENV_Destructible : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private C_Health health;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Damage Flash FX")]
    [Tooltip("Color to flash when damaged")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [Tooltip("How long the flash lasts")]
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Break Piece Settings")]
    [Tooltip("Prefab with SpriteRenderer + Rigidbody2D (GravityScale=0 for top-down) for break pieces")]
    [SerializeField] private GameObject breakPiecePrefab;
    [Tooltip("All sprites to spawn as break pieces (uses ALL sprites provided)")]
    [SerializeField] private Sprite[] breakSprites;
    [Tooltip("Horizontal distance pieces can spread when spawned (matches E_Reward pattern)")]
    public float dropSpread = 0.75f;
    [Tooltip("Force applied to scatter pieces")]
    [SerializeField] private float scatterForce = 3f;
    [Tooltip("How long before break pieces are destroyed")]
    [SerializeField] private float pieceLifetime = 2f;

    [Header("Loot Settings")]
    [Tooltip("Loot prefab to spawn (same as enemy drops)")]
    [SerializeField] private GameObject lootPrefab;
    [Tooltip("Chance (0-100%) to drop any items at all")]
    [Range(0, 100)] public float dropChance = 50f;
    [Tooltip("How many different items to drop from the loot table")]
    public int numberOfDrops = 1;
    [Tooltip("A list of all possible items this destructible can drop")]
    public List<LootDrop> lootTable;

    bool isDead = false;
    Color originalColor;
    Coroutine flashCoroutine;

    void Awake()
    {
        health ??= GetComponent<C_Health>();
        spriteRenderer ??= GetComponent<SpriteRenderer>();

        if (!health) Debug.LogError($"{name}: C_Health is missing on ENV_Destructible");
        if (!spriteRenderer) Debug.LogWarning($"{name}: SpriteRenderer is missing on ENV_Destructible");

        // Store original color for flash effect
        if (spriteRenderer) originalColor = spriteRenderer.color;
    }

    void OnEnable()
    {
        if (health != null)
        {
            health.OnDied += HandleDeath;
            health.OnDamaged += HandleDamaged; // Subscribe to damage event for flash
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleDeath;
            health.OnDamaged -= HandleDamaged;
        }
    }

    void HandleDamaged(int amount)
    {
        // Flash red when damaged
        if (spriteRenderer != null && !isDead)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashDamage());
        }
    }

    IEnumerator FlashDamage()
    {
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        // Hide original sprite
        if (spriteRenderer) spriteRenderer.enabled = false;

        // Spawn break pieces (uses ALL sprites provided)
        SpawnBreakPieces();

        // Drop loot (same logic as E_Reward)
        HandleLootDrops();

        // Destroy this object
        Destroy(gameObject, pieceLifetime);
    }

    /// <summary>
    /// Handle loot drops (same pattern as E_Reward).
    /// </summary>
    void HandleLootDrops()
    {
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

        // Calculate a random spawn position with a horizontal offset (same pattern as E_Reward)
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

    /// <summary>
    /// Spawns all break piece sprites with random scatter.
    /// Pattern matches E_Reward.SpawnLoot() for consistency.
    /// </summary>
    void SpawnBreakPieces()
    {
        if (breakPiecePrefab == null || breakSprites.Length == 0) return;

        // Spawn EVERY sprite in the array (not random selection)
        foreach (var sprite in breakSprites)
        {
            SpawnBreakPiece(sprite);
        }
    }

    void SpawnBreakPiece(Sprite sprite)
    {
        if (sprite == null) return;

        // Calculate random spawn position with horizontal offset (same pattern as E_Reward)
        float randomX = Random.Range(-dropSpread, dropSpread);
        Vector2 spawnPos = new Vector2(transform.position.x + randomX, transform.position.y);

        // Instantiate break piece at calculated position
        GameObject pieceGO = Instantiate(breakPiecePrefab, spawnPos, Quaternion.identity);

        // Set sprite
        SpriteRenderer pieceSR = pieceGO.GetComponent<SpriteRenderer>();
        if (pieceSR != null)
        {
            pieceSR.sprite = sprite;
        }

        // Apply random physics scatter (TOP-DOWN: use XY velocity, no gravity)
        Rigidbody2D rb = pieceGO.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0; // TOP-DOWN: No gravity

            // Random velocity in XY plane (top-down)
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * scatterForce, ForceMode2D.Impulse);

            // Random rotation spin
            float randomTorque = Random.Range(-5f, 5f);
            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
        }

        // Auto-cleanup
        Destroy(pieceGO, pieceLifetime);
    }
}
