using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "INV_ItemSO", menuName = "INV_ItemSO")]
public class INV_ItemSO : ScriptableObject
{
    public string itemName = "Auto Filled";
    [TextArea] public string itemDescription;
    public Sprite icon;
    public int stackSize = 3;

    [Header("Flags")]
    public bool isGold;

    [Header("Item Effects")]
    public List<StatEffect> modifiers;

    private void OnValidate()
    {
        if (itemName != name)
            itemName = name;
    }
}