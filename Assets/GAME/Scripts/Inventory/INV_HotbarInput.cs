using UnityEngine;

public class INV_HotbarInput : MonoBehaviour
{
    [Header("References")]
    INV_Manager    inv_Manager;
    P_InputActions input;

    void Awake()
    {
        input = new P_InputActions();
    }

    void Start()
    {
        inv_Manager = INV_Manager.Instance;

        if (!inv_Manager) { Debug.LogError($"{name}: INV_Manager.Instance is null!", this); return; }
    }

    void OnEnable()
    {
        // Subscribe to Inventory action (keys 1-9 return their number as float: 1.0, 2.0, etc.)
        input.UI.Inventory.performed += ctx => UseSlot(ctx.ReadValue<float>());
        input.UI.Enable();
    }

    void OnDisable()
    {
        input.UI.Inventory.performed -= ctx => UseSlot(ctx.ReadValue<float>());
        input.UI.Disable();
    }

    void OnDestroy()
    {
        input?.Dispose();
    }

    void UseSlot(float slotNumber)
    {
        // Convert slot number (1-9) to array index (0-8)
        int index = Mathf.RoundToInt(slotNumber) - 1;
        
        // Validate index is within bounds
        if (index < 0 || index >= inv_Manager.inv_Slots.Length) return;

        INV_Slots slot = inv_Manager.inv_Slots[index];

        // Use item if slot contains an item
        if (slot.type == INV_Slots.SlotType.Item)
        {
            inv_Manager.UseItem(slot);
        }
        // Equip weapon if slot contains a weapon
        else if (slot.type == INV_Slots.SlotType.Weapon)
        {
            inv_Manager.SetWeapon(slot);
        }
    }
}
