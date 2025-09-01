using UnityEngine;

[DisallowMultipleComponent]
public class P_Stats : MonoBehaviour
{
    [Header("Stats")]
    public int AD = 1; // Attack Damage
    public int AP = 0; // Ability Power
    public float MS = 5f; // Move Speed

    public int maxHP = 10;
    public int currentHP = 10;
    public int AR = 0; // Armor
    public int MR = 0; // Magic Resist
    public float KR = 10f; // knockback Resist

    [Header("Combat")]
    public float attackCooldown = 1.2f;

    [Header("Placeholders")]
    public float knockbackForce = 0f;
    public float stunTime = 0f;
}
