using UnityEngine;
using TMPro;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class INV_Manager : MonoBehaviour
{
    public static INV_Manager Instance;

    [Header("Central API for the Inventory system, depend on P_StatsManager")]
    [Header("References")]
    public P_StatsManager p_statsManager;

    [Header("MUST wire MANUALLY in Inspector")]
    public TMP_Text goldText;
    public GameObject lootPrefab;
    public Transform player;

    public int gold;
    public INV_Slots[] inv_Slots;

    void OnEnable() => INV_Loot.OnItemLooted += AddItem;
    void OnDisable() => INV_Loot.OnItemLooted -= AddItem;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        p_statsManager ??= FindFirstObjectByType<P_StatsManager>();

        if (!p_statsManager) Debug.LogError($"{name}: P_StatsManager is missing in INV_Manager");
    }

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
            // same item and not full
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
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
            if (slot.itemSO == null)
            {
                int amountToAdd = Mathf.Min(itemSO.stackSize, quantity);

                slot.itemSO = itemSO;
                slot.quantity = amountToAdd;
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
        {
            p_statsManager.ApplyModifier(modifier);
        }

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

    // REPLACE body to use ItemRegistry
    private INV_ItemSO ResolveItemById(string id)
    {
        return ItemRegistry.Get(id);

    }

    // Clears all slots UI
    private void ClearAllSlots()
    {
        if (inv_Slots == null) return;
        foreach (var s in inv_Slots)
        {
            s.itemSO = null;
            s.quantity = 0;
            s.UpdateUI();
        }
    }
    public SYS_SaveSystem.InventorySave SavingInventory()
    {
        var save = new SYS_SaveSystem.InventorySave();
        save.gold = gold;

        if (inv_Slots != null)
        {
            // Collapse to {id, qty}
            var counts = new Dictionary<string, int>();
            foreach (var s in inv_Slots)
            {
                if (s.itemSO == null || s.quantity <= 0) continue;
                string id = s.itemSO.id; // using name as ID for now
                if (!counts.ContainsKey(id)) counts[id] = 0;
                counts[id] += s.quantity;
            }
            foreach (var kv in counts)
                save.items.Add(new SYS_SaveSystem.InventorySave.InvItem { id = kv.Key, qty = kv.Value });
        }

        return save;
    }
    public void LoadingInventory(SYS_SaveSystem.InventorySave save)
    {
        // reset gold
        gold = save.gold;
        if (goldText) goldText.text = gold.ToString();

        // clear all slots
        ClearAllSlots();

        if (inv_Slots == null || save == null || save.items == null) return;

        // Fill respecting stackSize
        foreach (var entry in save.items)
        {
            if (string.IsNullOrEmpty(entry.id) || entry.qty <= 0) continue;

            var so = ResolveItemById(entry.id);
            if (!so) continue;

            int remaining = entry.qty;

            // 1) top-up existing stacks of the SAME item
            foreach (var s in inv_Slots)
            {
                if (remaining <= 0) break;
                if (s.itemSO == so && s.quantity < so.stackSize)
                {
                    int space = so.stackSize - s.quantity;
                    int add = Mathf.Min(space, remaining);
                    s.quantity += add;
                    s.UpdateUI();
                    remaining -= add;
                }
            }

            // 2) fill empty slots
            foreach (var s in inv_Slots)
            {
                if (remaining <= 0) break;
                if (s.itemSO == null)
                {
                    int add = Mathf.Min(so.stackSize, remaining);
                    s.itemSO = so;
                    s.quantity = add;
                    s.UpdateUI();
                    remaining -= add;
                }
            }

            if (remaining > 0)
                Debug.LogWarning($"[INV_Manager] Inventory overflow while loading '{so.name}', left={remaining}.");
        }
    }




}
