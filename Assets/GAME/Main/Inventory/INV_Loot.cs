using System;
using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class INV_Loot : MonoBehaviour
{
    public enum LootType { Item, Weapon }

    [Header("MUST have components for each loot prefab")]
    [Header("Loot Type")]
    public LootType lootType = LootType.Item;

    [Header("References (set ONE based on lootType)")]
    public INV_ItemSO itemSO;
    public W_SO       weaponSO;

    [Header("Data")]
    public int  quantity      = 1;
    public bool canBePickedUp = true;

    SpriteRenderer   sr;
    Animator         anim;
    CircleCollider2D trigger;

    public static event Action<INV_ItemSO, int> OnItemLooted;
    public static event Action<W_SO>            OnWeaponLooted;

    void Awake()
    {
        sr      ??= GetComponentInChildren<SpriteRenderer>();
        anim    ??= GetComponent<Animator>();
        trigger ??= GetComponent<CircleCollider2D>();

        if (!sr)      { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
        if (!anim)    { Debug.LogError($"{name}: Animator is missing!", this); return; }
        if (!trigger) { Debug.LogError($"{name}: CircleCollider2D is missing!", this); return; }
    }

    void OnEnable() => RefreshAppearance();

    // Called by INV_Manager when spawning overflow/right-click drops (ITEMS)
    public void Initialize(INV_ItemSO itemSO, int qty)
    {
        lootType      = LootType.Item;
        this.itemSO   = itemSO;
        weaponSO      = null;
        quantity      = qty;
        
        RefreshAppearance();
        anim.SetTrigger("Drop");
        StartCoroutine(EnablePickupAfterDelay(1f));
    }

    // Called by INV_Manager when dropping weapons
    public void InitializeWeapon(W_SO weaponSO)
    {
        lootType      = LootType.Weapon;
        this.weaponSO = weaponSO;
        itemSO        = null;
        quantity      = 1; // Weapons are always quantity 1
        
        RefreshAppearance();
        anim.SetTrigger("Drop");
        StartCoroutine(EnablePickupAfterDelay(1f));
    }

    // Update sprite and name based on loot type
    void RefreshAppearance()
    {
        if (lootType == LootType.Item && itemSO != null)
        {
            sr.sprite       = itemSO.image;
            gameObject.name = itemSO.itemName;
        }
        else if (lootType == LootType.Weapon && weaponSO != null)
        {
            sr.sprite       = weaponSO.image;
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
