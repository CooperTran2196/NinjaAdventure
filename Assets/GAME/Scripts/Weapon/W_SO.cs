using UnityEngine;

public enum WeaponType { Melee, Ranged, Magic }

[CreateAssetMenu(menuName = "Weapon SO", fileName = "W_SO_NewWeapon")]
public class W_SO : ScriptableObject
{
    [Header("Common")]
    public string id = "weaponId";
    [TextArea] public string description = ""; // Weapon description for tooltip
    public WeaponType type = WeaponType.Melee;
    public Sprite sprite; // In-game weapon visual (what player holds)
    public Sprite image;  // UI icon (inventory, weapon display, etc.)
    public float offsetRadius = 0.4f; // Distance from player center to weapon handle

    [Header("Damage (set either/both)")]
    public int AD = 1;
    public int AP = 0;

    [Header("Knockback")]
    public float knockbackForce = 5f;

    [Header("Melee Timing")]
    public float thrustDistance = 0.25f;

    [Header("Only thrust applies knockback")]
    public bool onlyThrustKnocksBack = true;

    [Header("Combo System (SlashDown, SlashUp, Thrust)")]
    [Header("Arc coverage for slash attacks (degrees).")]
    [Range(15f, 180f)] public float slashArcDegrees = 45f;

    public float[] comboShowTimes = { 0.45f, 0.45f, 0.45f };
    public float[] comboDamageMultipliers = { 1.0f, 1.2f, 2.0f };
    public float[] comboMovePenalties = { 0.6f, 0.5f, 0.3f };
    public float[] comboStunTimes = { 0.1f, 0.2f, 0.5f };

    [Header("Ranged + Magic")]
    [Header("Timing + Settings")]
    public float showTime = 0.45f;
    [Range(0f, 1f)] public float attackMovePenalty = 1f; // For ranged weapons only

    public GameObject projectilePrefab;
    public int manaCost = 0;
    public float projectileSpeed = 0f;
    public float projectileLifetime = 0f;
    public float stickOnHit = 0f;
    public int   pierceCount = 0;

    void OnValidate()
    {
        // Ensure combo arrays have exactly 3 elements
        if (comboShowTimes == null || comboShowTimes.Length != 3)
            comboShowTimes = new float[] { 0.3f, 0.3f, 0.5f };
        
        if (comboDamageMultipliers == null || comboDamageMultipliers.Length != 3)
            comboDamageMultipliers = new float[] { 1.0f, 1.2f, 2.0f };
        
        if (comboMovePenalties == null || comboMovePenalties.Length != 3)
            comboMovePenalties = new float[] { 0.6f, 0.5f, 0.3f };
        
        if (comboStunTimes == null || comboStunTimes.Length != 3)
            comboStunTimes = new float[] { 0.1f, 0.2f, 0.5f };
    }
}
