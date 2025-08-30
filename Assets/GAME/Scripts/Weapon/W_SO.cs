using UnityEngine;

[CreateAssetMenu(menuName = "W/W_SO")]
public class W_SO : ScriptableObject
{
    public enum Style { Melee, Ranged, Magic }

    [Header("Identity")]
    public string id = "Weapon_ID";
    public Style style = Style.Melee;

    [Header("Runtime Prefab (W_Melee / W_Ranged)")]
    public GameObject prefab;

    [Header("Combat")]
    public int weaponDamage = 1;

    [Tooltip("Where to place/aim the weapon relative to owner: dir * offsetDistance.")]
    public float offsetDistance = 0.5f;
}
