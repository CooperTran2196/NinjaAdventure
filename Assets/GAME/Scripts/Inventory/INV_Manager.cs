// Assets/GAME/Scripts/INV/INV_InventoryManager.cs
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class INV_Manager : MonoBehaviour
{
    [Header("Central API for the Inventory system, depend on P_StatsManager")]
    [Header("References")]
    public P_StatsManager p_statsManager;

    // MUST wire MANUALLY in Inspector
    public TMP_Text goldText;
    public GameObject lootPrefab;
    public Transform player;
    public int gold;
    public INV_Slots[] inv_Slots;

    void OnEnable()  => INV_Loot.OnItemLooted += AddItem;
    void OnDisable() => INV_Loot.OnItemLooted -= AddItem;

    void Awake()
    {
        p_statsManager ??= FindFirstObjectByType<P_StatsManager>();

        if (!p_statsManager) Debug.LogError("INV_Manager: P_StatsManager missing.", this);
    }

    // Update all slots & gold text at start
    void Start()
    {
        foreach (var slot in inv_Slots) slot.UpdateUI();
        UpdateGoldText();
    }

    // Adds item to inventory, stacking into existing slots first
    public void AddItem(INV_ItemSO inv_ItemSO, int quantity)
    {
        // Gold is special
        if (inv_ItemSO.isGold)
        {
            gold += quantity;
            UpdateGoldText();
            return;
        }

        // Stack into existing slots of the same item
        foreach (var slot in inv_Slots)
        {
            // same item and not full
            if (slot.itemSO == inv_ItemSO && slot.quantity < inv_ItemSO.stackSize)
            {
                int availableSpace = inv_ItemSO.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(availableSpace, quantity);

                slot.quantity += amountToAdd;
                quantity -= amountToAdd;

                slot.UpdateUI();
                if (quantity <= 0) return;
            }
        }

        // Fill empty slot
        foreach (var slot in inv_Slots)
        {
            if (slot.itemSO == null)
            {
                int amountToAdd = Mathf.Min(inv_ItemSO.stackSize, quantity);

                slot.itemSO = inv_ItemSO;
                slot.quantity = amountToAdd;
                slot.UpdateUI();

                quantity -= amountToAdd;
                if (quantity <= 0) return;
            }
        }

        // No room -> drop overflow at player
        if (quantity > 0) DropLoot(inv_ItemSO, quantity);
    }

    // Update gold text UI
    void UpdateGoldText()
    {
        goldText.text = gold.ToString();
    }

    // Uses the item in the given slot
    public void UseItem(INV_Slots slot)
    {
        // nothing to use
        if (slot.itemSO == null || slot.itemSO.modifiers.Count == 0) return;

        // Apply all modifiers from the item
        foreach (var modifier in slot.itemSO.modifiers)
        {
            p_statsManager.ApplyModifier(modifier);
        }

        // Consume the item
        slot.quantity -= 1;
        if (slot.quantity <= 0) slot.itemSO = null;
        slot.UpdateUI();
    }

    // Drops 1 item from the given slot at player position
    public void DropItemFromSlot(INV_Slots slot)
    {
        DropLoot(slot.itemSO, 1);
        slot.quantity -= 1;
        if (slot.quantity <= 0) slot.itemSO = null;
        slot.UpdateUI();
    }

    // Spawns loot prefab at player position with given item & quantity
    void DropLoot(INV_ItemSO itemSO, int qty)
    {
        var go = Instantiate(lootPrefab, player.position, Quaternion.identity);
        var loot = go.GetComponent<INV_Loot>();
        loot.Initialize(itemSO, qty); // sets sprite/name & canBePickedUp=false
    }
}
