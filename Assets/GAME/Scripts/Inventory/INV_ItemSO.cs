using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "INV_ItemSO", menuName = "Item")]
public class INV_ItemSO : ScriptableObject
{
    [Header("Item Data")]
                public string      id;
                public string      itemName = "Auto Filled by OnValidate";
    [TextArea]  public string      description;
                public Sprite      image;
                public int         stackSize = 3;
    [Min(0)]    public int         price     = 1;
    [Range(1,2)]public int         itemTier  = 1; // 1 = common, 2 = rare

    [Header("Flags")]
    public bool isGold;

    [Header("Ultimate Skill Unlock (Future Feature)")]
    public bool   unlocksSkill     = false;
    public string skillIDToUnlock  = "";

    [Header("Item Effects")]
    public List<P_StatEffect> StatEffectList;

    void OnValidate()
    {
        // Auto-sync itemName with asset file name
        if (itemName != name) itemName = name;
    }
}