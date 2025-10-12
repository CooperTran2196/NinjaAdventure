using UnityEngine;

public enum WeaponType { Melee, Ranged, Magic }

[CreateAssetMenu(menuName = "Weapon SO", fileName = "W_SO_NewWeapon")]
public class W_SO : ScriptableObject
{
    [Header("Common")]
    public string id = "weaponId";
    public WeaponType type = WeaponType.Melee;
    public Sprite sprite;
    public bool pointsUp = true; // Sprite should point UP with pivot at BOTTOM (handle)
    public float offsetRadius = 0.7f; // Distance from player center to weapon handle

    [Header("Damage (set either/both)")]
    public int AD = 1;
    public int AP = 0;

    [Header("Impact")]
    public float knockbackForce = 5f;
    public float stunTime = .5f;

    [Header("Melee timings + Thrust Distance")]
    public float showTime = 0.3f;
    public float thrustDistance = 0.25f;
    [Range(0f, 1f)]
    public float attackMovePenalty = 0.5f; // Movement speed multiplier while attacking (0.5 = 50% speed)

    [Header("Combo System (3-Hit Chain)")]
    [Tooltip("Arc coverage for slash attacks (degrees). Dagger=30°, Sword=45°, Greatsword=90°")]
    [Range(15f, 180f)]
    public float slashArcDegrees = 45f;

    [Tooltip("ShowTime for each combo attack [SlashDown, SlashUp, Thrust]")]
    public float[] comboShowTimes = { 0.3f, 0.3f, 0.5f };

    [Tooltip("Damage multipliers for combo [SlashDown, SlashUp, Thrust]")]
    public float[] comboDamageMultipliers = { 1.0f, 1.2f, 2.0f };

    [Tooltip("Movement penalties for combo [SlashDown, SlashUp, Thrust]")]
    public float[] comboMovePenalties = { 0.6f, 0.5f, 0.3f };

    [Tooltip("Stun times for combo [SlashDown, SlashUp, Thrust]")]
    public float[] comboStunTimes = { 0.1f, 0.2f, 0.5f };

    [Tooltip("Only thrust (index 2) applies knockback")]
    public bool onlyThrustKnocksBack = true;

    [Header("Ranged + Magic")]
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
