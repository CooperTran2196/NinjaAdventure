using UnityEngine;

[DisallowMultipleComponent]
public class P_Stats : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 10;
    public int currentHP = 10;
    public float moveSpeed = 5f;
    public int attackDmg = 1;
    public int magicDmg = 0;
    public int armor = 0;
    public int magicResist = 0;
    public float knockbackResistance = 10f;

    [Header("Combat")]
    public float attackCooldown = 1.2f;

    [Header("Placeholders")]
    public float knockbackForce = 0f;
    public float stunTime = 0f;
}
