using System;
using UnityEngine;

[ExecuteAlways] // lets OnEnable run in Edit Mode so the icon updates in Inspector
public class INV_Loot : MonoBehaviour
{
    [Header("MUST have components for each loot prefab")]
    [Header("References")]
    public INV_ItemSO inv_ItemSO;
    SpriteRenderer sr;
    Animator anim;
    CircleCollider2D trigger;

    [Header("Data")]
    public int quantity = 1;
    public bool canBePickedUp = true; // false when we drop it from inventory

    public static event Action<INV_ItemSO, int> OnItemLooted;

    void Awake()
    {
        sr      ??= GetComponentInChildren<SpriteRenderer>();
        anim    ??= GetComponent<Animator>();
        trigger ??= GetComponent<CircleCollider2D>();

        if (!sr)      Debug.LogError("INV_Loot: SpriteRenderer (sr) missing.", this);
        if (!anim)    Debug.LogError("INV_Loot: Animator (anim) missing.", this);
        if (!trigger) Debug.LogError("INV_Loot: CircleCollider2D (trigger) missing.", this);
    }

    // Run in Edit Mode to update sprite in Inspector
    void OnEnable() => RefreshAppearance();

    // Called by INV_Manager when spawning overflow/right-click drops
    public void Initialize(INV_ItemSO itemSO, int qty)
    {
        inv_ItemSO = itemSO;
        quantity = qty;
        canBePickedUp = false; // avoid instant repick
        RefreshAppearance();
    }

    // update sprite and name
    void RefreshAppearance()
    {
        sr.sprite = inv_ItemSO.image;
        gameObject.name = inv_ItemSO.itemName;
    }

    // pickup when player enters trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !canBePickedUp) return;

        trigger.enabled = false;
        OnItemLooted?.Invoke(inv_ItemSO, quantity);
        anim.SetTrigger("Pickup");
        Destroy(gameObject, 0.5f); // MUST Match animation length
    }

    // re-enable pickup when player leaves trigger (for drops)
    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        canBePickedUp = true;
    }
}
