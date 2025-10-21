using UnityEngine;

[DisallowMultipleComponent]
public class C_Stats : MonoBehaviour
{
    [Header("Core Stats")]
    public int   AD        = 1;   // Attack Damage
    public int   AP        = 0;   // Ability Power
    public float MS        = 5f;  // Move Speed
    public int   maxHP     = 10;
    public int   currentHP = 10;
    public int   AR        = 0;   // Armor
    public int   MR        = 0;   // Magic Resist
    public int   maxMP     = 50;  // Max Mana
    public int   currentMP = 0;   // Current Mana
    public float KR        = 10f; // Knockback Resist

    [Header("Special Stats")]
    public float lifesteal  = 0f;
    public float armorPen   = 0f;
    public float magicPen   = 0f;

    [Header("Weapon Bonuses (Player Only)")]
    public float slashArc        = 0f; // Additive degrees to slash arc
    public float attackSpeed     = 0f; // Attack speed (1 = 10% faster)
    public float movePenaltyReduction = 0f; // % reduction (0.1 = 10% less penalty)
    public float stunTimeBonus        = 0f; // % increase (0.1 = 10% more stun)
    public float thrustDistanceBonus  = 0f; // % increase (0.1 = 10% more distance)

    [Header("Combat")]
    public float attackCooldown  = 1.2f;
    public int   collisionDamage = 1;
    public float collisionTick   = 0.5f;

    [Header("Dodge (Player Only)")]
    public float dodgeSpeed    = 11f;
    public float dodgeDistance = 2.0f;
    public float dodgeCooldown = 0.45f;
}
