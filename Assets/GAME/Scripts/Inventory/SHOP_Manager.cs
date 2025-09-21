// Assets/GAME/Scripts/INV/SHOP_Manager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class SHOP_Manager : MonoBehaviour
{
    public static event Action<SHOP_Manager, bool> OnShopStateChanged;

    [Header("Shopping List")]
    [SerializeField] List<ShopItem> shopItems = new();   // per-shopkeeper list

    [Header("Slots")]
    [SerializeField] SHOP_Slot[] shopSlots;              // set in Inspector

    [Header("Inventory Link (for buy/sell step)")]
    public INV_Manager inv;                              // drag your Inventory Canvas

    void Awake()
    {
        // Auto-wire inventory if not assigned (optional)
        inv ??= FindFirstObjectByType<INV_Manager>();

        if (!inv)
            Debug.LogError($"{name}: INV_Manager reference missing on SHOP_Manager.", this);
        if (shopSlots == null || shopSlots.Length == 0)
            Debug.LogWarning($"{name}: No shopSlots assigned.", this);
    }

    void Start()
    {
        PopulateShopItems();
        OnShopStateChanged?.Invoke(this, true); // open by default for now
    }

    void OnDisable() => OnShopStateChanged?.Invoke(this, false);

    public void PopulateShopItems()
    {
        // fill used slots
        int count = Mathf.Min(shopItems.Count, shopSlots.Length);
        for (int i = 0; i < count; i++)
        {
            var data = shopItems[i];
            var slot = shopSlots[i];
            slot.gameObject.SetActive(true);
            slot.Initialize(data.inv_ItemSO, data.price);
        }

        // turn off the rest
        for (int i = count; i < shopSlots.Length; i++)
            shopSlots[i].gameObject.SetActive(false);
    }

    // ---- next-video hooks (already here so you don’t refactor later) ----
    public void TryBuyItem(INV_ItemSO inv_ItemSO, int price)
    {
        if (inv_ItemSO == null) return;
        if (!inv) return;
        if (inv.gold < price) return;
        if (!HasSpace(inv_ItemSO)) return;

        inv.gold -= price;
        inv.goldText.text = inv.gold.ToString();
        inv.AddItem(inv_ItemSO, 1);
    }

    public void SellItem(INV_ItemSO inv_ItemSO)
    {
        if (inv_ItemSO == null) return;
        if (!inv) return;
        int price = GetPrice(inv_ItemSO);
        if (price <= 0) return;

        inv.gold += price;
        inv.goldText.text = inv.gold.ToString();
        // INV_Slots handles decreasing quantity & UpdateUI on click
    }

    bool HasSpace(INV_ItemSO inv_ItemSO)
    {
        // same item with room
        foreach (var slot in inv.inv_Slots)
            if (slot.item == inv_ItemSO && slot.quantity < inv_ItemSO.stackSize) return true;

        // empty slot
        foreach (var slot in inv.inv_Slots)
            if (slot.item == null) return true;

        return false;
    }

    int GetPrice(INV_ItemSO inv_ItemSO)
    {
        for (int i = 0; i < shopItems.Count; i++)
            if (shopItems[i].inv_ItemSO == inv_ItemSO) return shopItems[i].price;
        return 0;
    }

    [Serializable]
    public class ShopItem
    {
        public INV_ItemSO inv_ItemSO;
        public int price = 1;
    }
}
