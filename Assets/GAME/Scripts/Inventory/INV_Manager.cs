// Assets/GAME/Scripts/INV/INV_InventoryManager.cs
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class INV_Manager : MonoBehaviour
{
    [Header("Central API for the Inventory system")]
    [Header("References")]
    public C_StatsManager statsManager; // <-- New reference

    public TMP_Text goldText;
    public GameObject lootPrefab;
    public Transform player;
    public int gold;
    public INV_Slots[] inv_Slots;

    void OnEnable()  => INV_Loot.OnItemLooted += AddItem;
    void OnDisable() => INV_Loot.OnItemLooted -= AddItem;

    void Awake()
    {
        statsManager ??= FindFirstObjectByType<C_StatsManager>();
        if (!statsManager) Debug.LogError($"{name}: C_StatsManager missing.", this);
    }

    void Start()
    {
        foreach (var slot in inv_Slots) slot.UpdateUI();
        UpdateGoldText();
    }

    public void AddItem(INV_ItemSO inv_ItemSO, int quantity)
    {
        if (inv_ItemSO.isGold)
        {
            gold += quantity;
            UpdateGoldText();
            return;
        }

        // Stack into existing slots of the same item
        foreach (var slot in inv_Slots)
        {
            if (slot.item == inv_ItemSO && slot.quantity < inv_ItemSO.stackSize)
            {
                int availableSpace = inv_ItemSO.stackSize - slot.quantity;
                int amountToAdd    = Mathf.Min(availableSpace, quantity);

                slot.quantity += amountToAdd;
                quantity      -= amountToAdd;

                slot.UpdateUI();
                if (quantity <= 0) return;
            }
        }

        // Fill empty slots
        foreach (var slot in inv_Slots)
        {
            if (slot.item == null)
            {
                int amountToAdd = Mathf.Min(inv_ItemSO.stackSize, quantity);

                slot.item     = inv_ItemSO;
                slot.quantity = amountToAdd;
                slot.UpdateUI();

                quantity -= amountToAdd;
                if (quantity <= 0) return;
            }
        }

        // No room -> drop overflow at player
        if (quantity > 0) DropLoot(inv_ItemSO, quantity);
    }

    void UpdateGoldText()
    {
        goldText.text = gold.ToString();
    }

    public void UseItem(INV_Slots slot)
    {
        if (slot.item == null || slot.item.modifiers.Count == 0) return;

        // Apply all modifiers from the item
        foreach (var modifier in slot.item.modifiers)
        {
            statsManager.ApplyModifier(modifier);
        }

        // Consume the item
        slot.quantity -= 1;
        if (slot.quantity <= 0) slot.item = null;
        slot.UpdateUI();
    }

    // Drops 1 item from the given slot at player position
    public void DropItemFromSlot(INV_Slots slot)
    {
        DropLoot(slot.item, 1);
        slot.quantity -= 1;
        if (slot.quantity <= 0) slot.item = null;
        slot.UpdateUI();
    }

    // Spawns loot prefab at player position with given item & quantity
    void DropLoot(INV_ItemSO item, int qty)
    {
        var go = Instantiate(lootPrefab, player.position, Quaternion.identity);
        var loot = go.GetComponent<INV_Loot>();
        loot.Initialize(item, qty); // sets sprite/name & canBePickedUp=false
    }
}
