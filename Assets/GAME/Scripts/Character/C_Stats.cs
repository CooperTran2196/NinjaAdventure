using UnityEngine;

[DisallowMultipleComponent]
public class C_Stats : MonoBehaviour
{
    [Header("Core Stats")]
    public int AD = 1; // Attack Damage
    public int AP = 0; // Ability Power
    public float MS = 5f; // Move Speed

    public int maxHP = 10;
    public int currentHP = 10;
    public int AR = 0; // Armor
    public int MR = 0; // Magic Resist
    public float KR = 10f; // knockback Resist

    [Header("Special Stats")]
    public float lifestealPercent = 0f;

    [Header("Combat")]
    public float attackCooldown = 1.2f;
    public int   collisionDamage = 1;   // per-enemy
    public float collisionTick   = 0.5f; // seconds between ticks while touching

    [Header("Dodge (used by Player; Enemy can ignore)")]
    public float dodgeSpeed = 11f;
    public float dodgeDistance = 2.0f;
    public float dodgeCooldown = 0.45f;

    [Header("Placeholders")]
    public float knockbackForce = 0f;
    public float stunTime = 0f;
}
