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

        if (!inv_Manager) { Debug.LogError($"{name}: INV_Manager is missing!", this); return; }
        if (shop_Slots == null || shop_Slots.Length == 0) { Debug.LogWarning($"{name}: No shop_Slots assigned!", this); }
    }

    // Populates shop UI with items from given list
    public void PopulateShopItems(List<INV_ItemSO> inv_ItemSOList)
    {
        // Fill used slots
        int count = Mathf.Min(inv_ItemSOList.Count, shop_Slots.Length);
        for (int i = 0; i < count; i++)
        {
            shop_Slots[i].gameObject.SetActive(true);
            shop_Slots[i].Initialize(inv_ItemSOList[i]);
        }

        // Turn off the rest
        for (int i = count; i < shop_Slots.Length; i++)
            shop_Slots[i].gameObject.SetActive(false);
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
    public void SellItem(INV_ItemSO itemSO)
    {
        // Update gold
        inv_Manager.gold += itemSO.price;
        inv_Manager.goldText.text = inv_Manager.gold.ToString();
        // INV_Slots handles decreasing quantity & UpdateUI on click
    }

    // Returns true if there's space in inventory for this item
    bool HasSpace(INV_ItemSO itemSO)
    {
        // Loop through inventory slots
        foreach (var slot in inv_Manager.inv_Slots)
        {
            // Same item with room
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
                return true;
            // Empty slot
            if (slot.itemSO == null)
                return true;
        }
        // No space found
        return false;
    }
}


