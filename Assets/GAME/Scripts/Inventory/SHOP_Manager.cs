using System.Collections.Generic;
using UnityEngine;

public class SHOP_Manager : MonoBehaviour
{
    [Header("References")]
    INV_Manager inv_Manager;

    [Header("MUST wire MANUALLY in Inspector")]
    public SHOP_Slot[] shop_Slots;

    void Awake()
    {
        inv_Manager ??= FindFirstObjectByType<INV_Manager>();

        if (!inv_Manager)                             { Debug.LogError($"{name}: INV_Manager is missing!", this); return; }
        if (shop_Slots == null || shop_Slots.Length == 0) Debug.LogWarning($"{name}: shop_Slots array is empty!", this);
    }

    // Populate shop slots with items from given list
    public void PopulateShopItems(List<INV_ItemSO> itemList)
    {
        int count = Mathf.Min(itemList.Count, shop_Slots.Length);
        
        // Fill used slots
        for (int i = 0; i < count; i++)
        {
            shop_Slots[i].gameObject.SetActive(true);
            shop_Slots[i].Initialize(itemList[i]);
        }

        // Disable unused slots
        for (int i = count; i < shop_Slots.Length; i++)
            shop_Slots[i].gameObject.SetActive(false);
    }

    // Buy item if player has enough gold and inventory space
    public void TryBuyItem(INV_ItemSO itemSO, int price)
    {
        if (itemSO == null)                return;
        if (inv_Manager.gold < price)      return;
        if (!HasSpace(itemSO))             return;

        inv_Manager.gold -= price;
        inv_Manager.goldText.text = inv_Manager.gold.ToString();
        inv_Manager.AddItem(itemSO, 1);
    }

    // Sell item from inventory for gold
    public void SellItem(INV_ItemSO itemSO)
    {
        if (itemSO.price <= 0)
        {
            Debug.LogWarning($"{name}: Cannot sell item with zero or negative price!", this);
            return;
        }

        inv_Manager.gold += itemSO.price;
        inv_Manager.goldText.text = inv_Manager.gold.ToString();
    }

    // Check if inventory has space for item (stackable or empty slot)
    bool HasSpace(INV_ItemSO itemSO)
    {
        foreach (INV_Slots slot in inv_Manager.inv_Slots)
        {
            // Same item with room to stack
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
                return true;
            // Empty slot
            if (slot.itemSO == null)
                return true;
        }
        return false;
    }
}


