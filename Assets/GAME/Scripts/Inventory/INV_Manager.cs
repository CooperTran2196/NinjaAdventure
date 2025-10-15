using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class INV_Manager : MonoBehaviour
{
    public static INV_Manager Instance;

    [Header("Central API for the Inventory system, depend on P_StatsManager")]
    [Header("References")]
    public P_StatsManager p_statsManager;

    [Header("MUST wire MANUALLY in Inspector")]
    public TMP_Text   goldText;
    public GameObject lootPrefab;
    public Transform  player;

    public int          gold;
    public INV_Slots[]  inv_Slots;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        p_statsManager       ??= FindFirstObjectByType<P_StatsManager>();

        if (!p_statsManager) Debug.LogError("INV_Manager: P_StatsManager is missing.");
    }

    void OnEnable()  => INV_Loot.OnItemLooted += AddItem;
    void OnDisable() => INV_Loot.OnItemLooted -= AddItem;

    // Update all slots & gold text at start
    void Start()
    {
        foreach (var slot in inv_Slots) slot.UpdateUI();
        UpdateGoldText();
    }

    // Adds item to inventory, stacking into existing slots first
    public void AddItem(INV_ItemSO itemSO, int quantity)
    {
        // Gold is special
        if (itemSO.isGold)
        {
            gold += quantity;
            UpdateGoldText();
            return;
        }

        // Stack into existing slots of the same item
        foreach (var slot in inv_Slots)
        {
            if (slot.type == INV_Slots.SlotType.Item && 
                slot.itemSO == itemSO && 
                slot.quantity < itemSO.stackSize)
            {
                int availableSpace = itemSO.stackSize - slot.quantity;
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
            if (slot.type == INV_Slots.SlotType.Empty)
            {
                int amountToAdd = Mathf.Min(itemSO.stackSize, quantity);

                slot.itemSO = itemSO;
                slot.quantity = amountToAdd;
                slot.type = INV_Slots.SlotType.Item;
                slot.UpdateUI();

                quantity -= amountToAdd;
                if (quantity <= 0) return;
            }
        }

        // No room -> drop overflow at player
        if (quantity > 0) DropItem(itemSO, quantity);
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
        if (slot.itemSO == null || slot.itemSO.StatEffectList.Count == 0) return;

        // Apply all StatEffectList from the item
        foreach (var modifier in slot.itemSO.StatEffectList)
            p_statsManager.ApplyModifier(modifier);

        // Consume the item
        slot.quantity -= 1;
        if (slot.quantity <= 0) slot.itemSO = null;
        slot.UpdateUI();
    }

    // Spawns loot prefab at player position with given item & quantity
    public void DropItem(INV_ItemSO itemSO, int quantity)
    {
        if (!itemSO) return;
        var go = Instantiate(lootPrefab, player.position, Quaternion.identity);
        var loot = go.GetComponent<INV_Loot>();
        loot.Initialize(itemSO, quantity); // sets sprite/name & canBePickedUp=false
    }

    // Do we have at least 1 of this item?
    public bool HasItem(INV_ItemSO itemSO)
    {
        // Adjust the array name if yours differs (e.g., slots, itemSlots, inventorySlots)
        foreach (var slot in inv_Slots)
        {
            if (slot.itemSO == itemSO && slot.quantity > 0)
                return true;
        }
        return false;
    }

    // ========== WEAPON HANDLING METHODS ==========

    // ========== WEAPON HANDLING ==========

    public void EquipWeapon(INV_Slots slot)
    {
        if (slot.weaponSO == null) return;

        P_Controller playerController = player.GetComponent<P_Controller>();
        if (!playerController) return;

        W_SO oldWeapon = playerController.EquipWeapon(slot.weaponSO);

        slot.weaponSO = oldWeapon;
        slot.type = INV_Slots.SlotType.Weapon;
        slot.UpdateUI();
    }

    public bool AddWeapon(W_SO weaponSO)
    {
        foreach (var slot in inv_Slots)
        {
            if (slot.type == INV_Slots.SlotType.Empty)
            {
                slot.weaponSO = weaponSO;
                slot.type = INV_Slots.SlotType.Weapon;
                slot.UpdateUI();
                return true;
            }
        }

        Debug.Log("Inventory full! Cannot add weapon: " + weaponSO.id);
        return false;
    }

    public void DropWeapon(W_SO weaponSO)
    {
        if (!weaponSO) return;
        
        var go = Instantiate(lootPrefab, player.position, Quaternion.identity);
        go.GetComponent<INV_Loot>().InitializeWeapon(weaponSO);
    }
}
