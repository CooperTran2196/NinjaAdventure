using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class INV_Manager : MonoBehaviour
{
    public static INV_Manager Instance;

    [Header("Central API for the Inventory system, depend on P_StatsManager")]
    [Header("References")]
    P_StatsManager    p_StatsManager;

    [Header("MUST wire MANUALLY in Inspector")]
    public TMP_Text   goldText;
    public GameObject lootPrefab;
    public Transform  player;

    [Header("Data")]
    public int         gold;
    public INV_Slots[] inv_Slots;

    void Awake()
    {
        Instance = this;

        p_StatsManager ??= FindFirstObjectByType<P_StatsManager>();

        if (!p_StatsManager) { Debug.LogError($"{name}: P_StatsManager is missing!", this); return; }
        if (!goldText)       { Debug.LogError($"{name}: goldText is missing!", this); return; }
        if (!lootPrefab)     { Debug.LogError($"{name}: lootPrefab is missing!", this); return; }
        if (!player)         { Debug.LogError($"{name}: player is missing!", this); return; }
        
        if (inv_Slots == null || inv_Slots.Length == 0)
        {
            Debug.LogError($"{name}: inv_Slots array is empty!", this); return; 
        }
    }

    void OnEnable()  => INV_Loot.OnItemLooted += AddItem;
    void OnDisable() => INV_Loot.OnItemLooted -= AddItem;

    // Update all slots & gold text at start
    void Start()
    {
        foreach (INV_Slots slot in inv_Slots) slot.UpdateUI();
        UpdateGoldText();
    }

    // Adds item to inventory, stacking into existing slots first
    public void AddItem(INV_ItemSO itemSO, int quantity)
    {
        // Gold is handled specially
        if (itemSO.isGold)
        {
            gold += quantity;
            UpdateGoldText();
            SYS_GameManager.Instance.sys_SoundManager.PlayGoldPickup();
            return;
        }

        // Play item pickup sound based on tier
        SYS_GameManager.Instance.sys_SoundManager.PlayItemPickup(itemSO.itemTier);

        // Stack into existing slots of the same item
        foreach (INV_Slots slot in inv_Slots)
        {
            if (slot.type == INV_Slots.SlotType.Item && 
                slot.itemSO == itemSO && 
                slot.quantity < itemSO.stackSize)
            {
                int availableSpace = itemSO.stackSize - slot.quantity;
                int amountToAdd    = Mathf.Min(availableSpace, quantity);

                slot.quantity += amountToAdd;
                quantity      -= amountToAdd;

                slot.UpdateUI();
                if (quantity <= 0) return;
            }
        }

        // Fill empty slots with remaining quantity
        foreach (INV_Slots slot in inv_Slots)
        {
            if (slot.type == INV_Slots.SlotType.Empty)
            {
                int amountToAdd = Mathf.Min(itemSO.stackSize, quantity);

                slot.itemSO   = itemSO;
                slot.quantity = amountToAdd;
                slot.type     = INV_Slots.SlotType.Item;
                slot.UpdateUI();

                quantity -= amountToAdd;
                if (quantity <= 0) return;
            }
        }

        // Inventory full - drop overflow at player position
        if (quantity > 0) DropItem(itemSO, quantity);
    }

    // Update gold text UI
    void UpdateGoldText()
    {
        goldText.text = gold.ToString();
    }

    // Use item from slot and apply its stat effects
    public void UseItem(INV_Slots slot)
    {
        // nothing to use
        if (slot.itemSO == null || slot.itemSO.StatEffectList.Count == 0) return;

        // Apply all stat effects from the item
        foreach (P_StatEffect effect in slot.itemSO.StatEffectList)
            p_StatsManager.ApplyModifier(effect);

        // Consume one item
        slot.quantity -= 1;
        if (slot.quantity <= 0) slot.itemSO = null;
        slot.UpdateUI();
    }

    // Spawn loot prefab at player position
    public void DropItem(INV_ItemSO itemSO, int quantity)
    {
        if (!itemSO) return;
        
        // Play drop item sound
        SYS_GameManager.Instance.sys_SoundManager.PlayDropItem();
        
        GameObject lootGO   = Instantiate(lootPrefab, player.position, Quaternion.identity);
        INV_Loot   loot     = lootGO.GetComponent<INV_Loot>();
        
        loot.Initialize(itemSO, quantity);
    }

    // Check if inventory contains at least 1 of this item
    public bool HasItem(INV_ItemSO itemSO)
    {
        foreach (INV_Slots slot in inv_Slots)
        {
            if (slot.itemSO == itemSO && slot.quantity > 0)
                return true;
        }
        return false;
    }

    // Equip weapon from inventory slot (swaps with currently equipped weapon)
    public void EquipWeapon(INV_Slots slot)
    {
        if (slot.weaponSO == null) return;

        P_Controller p_Controller = player.GetComponent<P_Controller>();
        if (!p_Controller) return;

        // Play weapon change sound
        SYS_GameManager.Instance.sys_SoundManager.PlayWeaponChange();

        W_SO oldWeapon = p_Controller.EquipWeapon(slot.weaponSO);

        slot.weaponSO = oldWeapon;
        slot.type     = INV_Slots.SlotType.Weapon;
        slot.UpdateUI();
    }

    // Add weapon to first empty inventory slot
    public bool AddWeapon(W_SO weaponSO)
    {
        foreach (INV_Slots slot in inv_Slots)
        {
            if (slot.type == INV_Slots.SlotType.Empty)
            {
                slot.weaponSO = weaponSO;
                slot.type     = INV_Slots.SlotType.Weapon;
                slot.UpdateUI();
                
                // Play weapon pickup sound
                SYS_GameManager.Instance.sys_SoundManager.PlayItemPickup(2); // Tier 2 for weapons
                
                return true;
            }
        }

        Debug.Log($"Inventory full! Cannot add weapon: {weaponSO.id}");
        return false;
    }

    // Drop weapon as loot at player position
    public void DropWeapon(W_SO weaponSO)
    {
        if (!weaponSO) return;
        
        // Play drop item sound
        SYS_GameManager.Instance.sys_SoundManager.PlayDropItem();
        
        GameObject lootGO = Instantiate(lootPrefab, player.position, Quaternion.identity);
        lootGO.GetComponent<INV_Loot>().InitializeWeapon(weaponSO);
    }
}
