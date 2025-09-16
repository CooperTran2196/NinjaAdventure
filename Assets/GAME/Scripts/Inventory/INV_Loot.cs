using System;
using UnityEngine;

[ExecuteAlways] // lets OnEnable run in Edit Mode so the icon updates in Inspector
public class INV_Loot : MonoBehaviour
{
    public static event Action<INV_ItemSO, int> OnItemLooted;

    [Header("References")]
    public INV_ItemSO itemSO;

    SpriteRenderer sr;
    Animator anim;

    [Header("Data")]
    public int quantity = 1;
    public bool canBePickedUp = true; // false when we drop it from inventory

    CircleCollider2D trigger;

    void Awake()
    {
        sr      ??= GetComponentInChildren<SpriteRenderer>();
        anim    ??= GetComponent<Animator>();
        trigger ??= GetComponent<CircleCollider2D>();

        if (!sr)      Debug.LogError($"{name}: SpriteRenderer missing.", this);
        if (!anim)    Debug.LogError($"{name}: Animator missing.", this);
        if (!trigger) Debug.LogError($"{name}: CircleCollider2D missing.", this);
    }

    void OnEnable() => RefreshAppearance();

    // Called by INV_Manager when spawning overflow/right-click drops
    public void Initialize(INV_ItemSO so, int qty)
    {
        itemSO = so;
        quantity = qty;
        canBePickedUp = false; // avoid instant repick
        RefreshAppearance();
    }

    void RefreshAppearance()
    {
        if (!sr || !itemSO) return;

        sr.sprite = itemSO.icon;
        gameObject.name = itemSO.itemName;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !canBePickedUp) return;

        trigger.enabled = false;
        OnItemLooted?.Invoke(itemSO, quantity);
        anim?.SetTrigger("Pickup");
        Destroy(gameObject, 0.5f); // Match animation length
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        canBePickedUp = true;
    }
}
