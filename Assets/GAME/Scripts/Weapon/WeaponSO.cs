using UnityEngine;

// 0️⃣  Attack styles drive which strategy gets picked.
public enum AttackStyle { Barehand, Melee, Projectile }

    [System.Serializable]
public struct DirOffset
{
    public Vector2 up;
    public Vector2 down;
    public Vector2 left;
    public Vector2 right;
}

    [CreateAssetMenu(menuName = "Weapons/Weapon Action")]   // shows in Create menu
public class WeaponSO : ScriptableObject          // data-only asset
{
    [Header("Meta")]
    public string weaponName;
    public AttackStyle style;
    [Range(0, 9)] public int slotIndex;                 // hot-key

    [Header("Combat")]
    public float weaponDamage = 1f;
    public float knockbackForce = 2f;
    public float stunTime = 0.2f;
    public bool moveLock = false;

    [Header("Visual")]
    public Sprite icon;
    public Sprite spriteInHand;
    public DirOffset dirOffset;       // reuse the same struct as WeaponSO

    [Tooltip("Fallback push-out distance if dirOffset entry = (0,0)")]
    public float offsetDistance = 0.8f;

    [Header("Stats")]
    public float baseDamage = 1f;
    public float cooldown = 0.3f;

    [Header("Projectile (Projectile style only)")]
    public GameObject projectilePrefab;
}
