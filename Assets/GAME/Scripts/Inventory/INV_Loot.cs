using System;
using System.Collections;
using UnityEngine;

[ExecuteAlways] // lets OnEnable run in Edit Mode so the icon updates in Inspector
public class INV_Loot : MonoBehaviour
{
    public enum LootType { Item, Weapon }

    [Header("MUST have components for each loot prefab")]
    [Header("Loot Type")]
    public LootType lootType = LootType.Item;

    [Header("References (set ONE based on lootType)")]
    public INV_ItemSO itemSO;   // for items
    public W_SO weaponSO;        // for weapons

    [Header("Data")]
    public int  quantity = 1;    // only used for items
    public bool canBePickedUp = true;

    SpriteRenderer   sr;
    Animator         anim;
    CircleCollider2D trigger;

    public static event Action<INV_ItemSO, int> OnItemLooted;
    public static event Action<W_SO> OnWeaponLooted;  // NEW: Weapon event

    void Awake()
    {
        sr            ??= GetComponentInChildren<SpriteRenderer>();
        anim          ??= GetComponent<Animator>();
        trigger       ??= GetComponent<CircleCollider2D>();

        if (!sr)      Debug.LogError("INV_Loot: SpriteRenderer is missing.");
        if (!anim)    Debug.LogError("INV_Loot: Animator is missing.");
        if (!trigger) Debug.LogError("INV_Loot: CircleCollider2D is missing.");
    }

    // Run in Edit Mode to update sprite in Inspector
    void OnEnable() => RefreshAppearance();

    // Called by INV_Manager when spawning overflow/right-click drops (ITEMS)
    public void Initialize(INV_ItemSO itemSO, int qty)
    {
        this.lootType = LootType.Item;
        this.itemSO = itemSO;
        this.weaponSO = null;
        quantity = qty;
        RefreshAppearance();

        // Play drop animation and start pickup delay
        anim.SetTrigger("Drop");
        StartCoroutine(EnablePickupAfterDelay(1f));
    }

    // NEW: Called by INV_Manager when dropping weapons
    public void InitializeWeapon(W_SO weaponSO)
    {
        this.lootType = LootType.Weapon;
        this.weaponSO = weaponSO;
        this.itemSO = null;
        quantity = 1; // weapons don't stack
        RefreshAppearance();

        // Play drop animation and start pickup delay
        anim.SetTrigger("Drop");
        StartCoroutine(EnablePickupAfterDelay(1f));
    }

    // Update sprite and name based on loot type
    void RefreshAppearance()
    {
        if (lootType == LootType.Item && itemSO != null)
        {
            sr.sprite = itemSO.image;
            gameObject.name = itemSO.itemName;
        }
        else if (lootType == LootType.Weapon && weaponSO != null)
        {
            sr.sprite = weaponSO.image;
            gameObject.name = weaponSO.id;
        }
    }

    // Pickup when player enters trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !canBePickedUp) return;

        trigger.enabled = false;

        // Handle based on loot type
        if (lootType == LootType.Item)
        {
            OnItemLooted?.Invoke(itemSO, quantity);
            anim.SetTrigger("Pickup");
            Destroy(gameObject, 0.5f); // MUST Match animation length
        }
        else // Weapon
        {
            // Try to add weapon to inventory
            bool success = INV_Manager.Instance?.AddWeapon(weaponSO) ?? false;
            
            if (success)
            {
                OnWeaponLooted?.Invoke(weaponSO);
                anim.SetTrigger("Pickup");
                Destroy(gameObject, 0.5f);
            }
            else
            {
                // Inventory full - re-enable trigger so player can try again later
                Debug.Log($"Inventory full! Cannot pick up {weaponSO.id}");
                trigger.enabled = true;
            }
        }
    }

    // re-enable pickup when player leaves trigger (for drops)
    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Only re-enable if it was disabled due to player collision
        if (canBePickedUp)
        {
            canBePickedUp = true;
        }
    }

    // Prevent immediate pickup after drop
    IEnumerator EnablePickupAfterDelay(float delay)
    {
        canBePickedUp = false;
        yield return new WaitForSeconds(delay);
        canBePickedUp = true;
    }
}
