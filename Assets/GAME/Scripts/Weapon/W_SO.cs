using UnityEngine;

public enum WeaponType { Melee, Ranged, Magic }

[CreateAssetMenu(menuName = "Weapon SO", fileName = "W_SO_NewWeapon")]
public class W_SO : ScriptableObject
{
    [Header("Common")]
    public string id = "weaponId";
    public WeaponType type = WeaponType.Melee;
    public Sprite sprite;
    public bool pointsUp = false; // false = points down
    public float offsetRadius = 0.6f;  // polar placement radius
    public float angleBiasDeg = 0f;    // optional art twist

    [Header("Damage (set either/both)")]
    public int AD = 1;
    public int AP = 0;

    [Header("Impact")]
    public float knockbackForce = 5f;
    public float stunTime = .5f;

    [Header("Melee timings + Thrust Distance")]
    public float showTime = 0.3f;
    public float thrustDistance = 0.25f;

    [Header("Ranged + Magic")]
    public GameObject projectilePrefab;
    public int manaCost = 0;
    public float projectileSpeed = 0f;
    public float projectileLifetime = 0f;
    public float stickOnHitSeconds = 0f;
    public int   pierceCount = 0;
}
