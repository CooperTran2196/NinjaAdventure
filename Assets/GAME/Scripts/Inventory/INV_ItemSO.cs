using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "INV_ItemSO", menuName = "Item")]
public class INV_ItemSO : ScriptableObject
{
    [Header("Item Data")]
    public string itemName = "Auto Filled";
    [TextArea] public string itemDescription;
    public Sprite image;
    public int stackSize = 3;
    public int price = 1;

    [Header("Flags")]
    public bool isGold;

    [Header("Item Effects")]
    public List<StatEffect> modifiers;

    // Auto-update name in Editor
    private void OnValidate()
    {
        if (itemName != name)
            itemName = name;
    }
}