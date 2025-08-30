using UnityEngine;

[DisallowMultipleComponent]
public class E_Stats : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 20;
    public int currentHP = 20;
    public float moveSpeed = 5f;
    public int attackDmg = 1;
    public int magicDmg = 0;
    public int armor = 0;
    public int magicResist = 0;

    [Header("Combat")]
    public float attackCooldown = 1.2f;
    public int   collisionDamage = 1;   // per-enemy
    public float collisionTick   = 0.5f; // seconds between ticks while touching

    [Header("Placeholders")]
    public float knockbackForce = 0f;
    public float stunTime = 0f;
}
