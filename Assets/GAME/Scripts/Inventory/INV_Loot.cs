using System;
using System.Collections;
using UnityEngine;

[ExecuteAlways] // lets OnEnable run in Edit Mode so the icon updates in Inspector
public class INV_Loot : MonoBehaviour
{
    [Header("MUST have components for each loot prefab")]
    [Header("References")]
    public INV_ItemSO itemSO;

    [Header("Data")]
    public int  quantity = 1;
    public bool canBePickedUp = true;

    SpriteRenderer   sr;
    Animator         anim;
    CircleCollider2D trigger;

    public static event Action<INV_ItemSO, int> OnItemLooted;

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

    // Called by INV_Manager when spawning overflow/right-click drops
    public void Initialize(INV_ItemSO itemSO, int qty)
    {
        this.itemSO = itemSO;
        quantity = qty;
        RefreshAppearance();

        // Play drop animation and start pickup delay
        anim.SetTrigger("Drop");
        StartCoroutine(EnablePickupAfterDelay(1f));
    }

    // Update sprite and name
    void RefreshAppearance()
    {
        sr.sprite = itemSO.image;
        gameObject.name = itemSO.itemName;
    }

    // Pickup when player enters trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !canBePickedUp) return;

        trigger.enabled = false;
        OnItemLooted?.Invoke(itemSO, quantity);
        anim.SetTrigger("Pickup");
        Destroy(gameObject, 0.5f); // MUST Match animation length
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
