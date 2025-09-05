using UnityEngine;

public enum WeaponType { Melee, Ranged, Magic }

[CreateAssetMenu(menuName = "GAME/Weapon SO", fileName = "W_SO_NewWeapon")]
public class W_SO : ScriptableObject
{
    [Header("Common")]
    public string id = "weapon_id";
    public WeaponType type = WeaponType.Melee;
    public Sprite sprite;
    public bool pointsUp = false; // false = points down

    [Header("Damage (set either/both)")]
    public int baseAD = 1;
    public int baseAP = 0;

    [Header("Impact")]
    public float knockbackForce = 5f;
    public float stunTime = .5f;

    [Header("Melee timings & Thrust Distance")]
    public float showTime = 0.3f;
    public float thrustDistance = 0.25f;

    [Header("Ranged + Magic")]
    public GameObject projectilePrefab;
    public int manaCost = 0;
    public float projectileSpeed = 8f;
    public float projectileLifetime = 3f;
    public float stickOnHitSeconds = 0f;
    public int   pierceCount = 0; 

    [Header("Optional directional offsets")]
    [Header("Cardinals")]
    public Vector2 offsetDown = new Vector2(-0.2f, -0.75f);
    public Vector2 offsetUp = new Vector2(-0.2f, 0.75f);
    public Vector2 offsetLeft = new Vector2(-0.7f, -0.25f);
    public Vector2 offsetRight = new Vector2(0.7f, -0.25f);
    [Header("Diagonals")]
    public Vector2 offsetUpRight = new Vector2(0.5f, 0.5f);
    public Vector2 offsetUpLeft = new Vector2(-0.5f, 0.5f);
    public Vector2 offsetDownRight = new Vector2(0.5f, -0.75f);
    public Vector2 offsetDownLeft = new Vector2(-0.5f, 0.75f);


}
