using UnityEngine;

[CreateAssetMenu(fileName = "INV_ItemSO", menuName = "INV_ItemSO")]
public class INV_ItemSO : ScriptableObject
{
    public string itemName = "Auto Filled";
    [TextArea] public string itemDescription;
    public Sprite icon;

    [Header("Flags")]
    public bool isGold;

    [Header("Stats (delta)")]
    public int currentHealth; // heal amount (0 = none)
    public int maxHealth;     // permanent max HP change (0 = none)
    public int speed;         // movement speed delta
    public int damage;        // basic damage delta
    public int stackSize = 3;

    [Header("Temporary")]
    public float durationSec; // 0 = permanent effect

    private void OnValidate()
    {
        if (itemName != name)
            itemName = name;
    }
}
