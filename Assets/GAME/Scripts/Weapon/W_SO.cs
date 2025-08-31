using UnityEngine;

public enum WeaponType { Melee, Ranged, Magic }

[CreateAssetMenu(menuName = "GAME/Weapon SO", fileName = "W_SO_NewWeapon")]
public class W_SO : ScriptableObject
{
    [Header("Common")]
    public string id = "weapon_id";
    public WeaponType type = WeaponType.Melee;
    public Sprite sprite;
    public int baseDamage = 1;

    [Header("Melee")]
    public float offsetDistance = 0.3f;
    public float showTime = 0.18f;
    public float thrustDistance = 0.2f;

    [Header("Ranged (placeholders)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    [Header("Magic (placeholders)")]
    public int manaCost = 0;
}
