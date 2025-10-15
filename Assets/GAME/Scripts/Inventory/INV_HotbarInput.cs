using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles number key inputs (1-9) for inventory hotbar
/// Attach to Player or GameManager
/// NOTE: You need to add InventorySlot1-9 actions to P_InputActions.inputactions (UI map)
/// For now, using direct Keyboard input as fallback
/// </summary>
public class INV_HotbarInput : MonoBehaviour
{
    private INV_Manager invManager;

    void Start()
    {
        // Use Start instead of Awake to ensure INV_Manager.Instance is initialized
        invManager = INV_Manager.Instance;

        if (!invManager)
            Debug.LogError("INV_HotbarInput: INV_Manager.Instance is null! Make sure INV_Manager exists in the scene.");
    }

    void Update()
    {
        // Fallback: Direct keyboard input (will work immediately)
        // TODO: Replace with proper Input Actions once created in Unity
        if (Keyboard.current == null) return;
        if (invManager == null) return; // Safety check

        if (Keyboard.current.digit1Key.wasPressedThisFrame) UseSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) UseSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) UseSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) UseSlot(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) UseSlot(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) UseSlot(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) UseSlot(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) UseSlot(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) UseSlot(8);
    }

    /// <summary>
    /// Uses inventory slot by index (0-8 for slots 1-9)
    /// </summary>
    private void UseSlot(int index)
    {
        if (invManager == null) return;
        if (index >= invManager.inv_Slots.Length) return;

        INV_Slots slot = invManager.inv_Slots[index];

        if (slot.type == INV_Slots.SlotType.Item)
        {
            invManager.UseItem(slot);
        }
        else if (slot.type == INV_Slots.SlotType.Weapon)
        {
            invManager.EquipWeapon(slot);
        }
        // If empty, do nothing
    }
}
