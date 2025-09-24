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
    [SerializeField] SHOP_Slot[] shopSlots;

    [Header("Inventory Link (for buy/sell step)")]
    public INV_Manager inv_Manager;

    void Awake()
    {
        inv_Manager ??= FindFirstObjectByType<INV_Manager>();

        if (!inv_Manager)
            Debug.LogError("SHOP_Manager: INV_Manager reference missing.", this);
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

    // Attempt to buy item, checking gold and space
    public void TryBuyItem(INV_ItemSO inv_ItemSO, int price)
    {
        if (inv_ItemSO == null) return;
        if (inv_Manager.gold < price) return;
        if (!HasSpace(inv_ItemSO)) return;
        
        inv_Manager.gold -= price;
        inv_Manager.goldText.text = inv_Manager.gold.ToString();
        inv_Manager.AddItem(inv_ItemSO, 1);
    }

    // Sell item from inventory to shop
    public void SellItem(INV_ItemSO inv_ItemSO)
    {
        if (inv_ItemSO == null) return;
        int price = GetPrice(inv_ItemSO);
        if (price <= 0) return;

        inv_Manager.gold += price;
        inv_Manager.goldText.text = inv_Manager.gold.ToString();
        // INV_Slots handles decreasing quantity & UpdateUI on click
    }

    // Check if there's space in inventory for this item
    bool HasSpace(INV_ItemSO inv_ItemSO)
    {
        // same item with room
        foreach (var slot in inv_Manager.inv_Slots)
            if (slot.itemSO == inv_ItemSO && slot.quantity < inv_ItemSO.stackSize)
                return true;
            else if (slot.itemSO == null)
                return true;
        return false;
    }

    // Get price of item from shop list; 0 if not sold here
    int GetPrice(INV_ItemSO inv_ItemSO)
    {
        for (int i = 0; i < shopItems.Count; i++)
            if (shopItems[i].inv_ItemSO == inv_ItemSO)
                return shopItems[i].price;
        return 0;
    }

    // Simple struct for shop item data
    [Serializable]
    public class ShopItem
    {
        public INV_ItemSO inv_ItemSO;
        public int price = 1;
    }
}
