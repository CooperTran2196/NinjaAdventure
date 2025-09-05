using System;
using System.Collections;
using UnityEngine;

public class P_Combat : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sprite;
    Animator animator;
    P_InputActions input;

    public P_Stats p_stats;
    public P_Movement p_movement;
    public C_Health p_health;
    public W_Base activeWeapon;
    
    [Header("Attack")]
    [Header("ALWAYS matched full clip length (0.45)")]
    public float attackDuration = 0.45f;
    [Header("ALWAYS set when the hit happens (0.15)")]
    public float hitDelay = 0.15f;
    public bool lockDuringAttack = true;

    [Header("Debug")]
    [SerializeField] bool autoKill;

    Vector2 aimDir = Vector2.down;
    const float MIN_DISTANCE = 0.0001f;
    float cooldownTimer;
    public bool IsAttacking { get; private set; }

    // Quick state check
    public bool IsAlive => p_stats.currentHP > 0;

    void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        animator ??= GetComponent<Animator>();
        input ??= new P_InputActions();
        
        p_stats ??= GetComponent<P_Stats>();
        p_movement ??= GetComponent<P_Movement>();
        p_health ??= GetComponent<C_Health>();
        activeWeapon ??= GetComponentInChildren<W_Melee>();

        
        if (!animator) Debug.LogError($"{name}: Animator missing.");
        if (!sprite) Debug.LogWarning($"{name}: SpriteRenderer missing.");

        if (!p_stats) Debug.LogError($"{name}: P_Stats missing.");
        if (!p_movement) Debug.LogError($"{name}: P_Movement missing.");
        if (!p_health) Debug.LogError($"{name}: C_Health missing.");
        if (!activeWeapon) Debug.LogError($"{name}: C_Melee missing.");

        animator.SetFloat("atkX", 0f);
        animator.SetFloat("atkY", -1f);
    }

    void OnEnable()
    {
        input?.Enable();
        p_health.OnDied += Die;
    }

    void OnDisable()
    {
        input?.Disable();
        p_health.OnDied -= Die;
    }

    void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        if (raw.sqrMagnitude > MIN_DISTANCE) aimDir = raw.normalized;
        if (input.Player.Attack.triggered) RequestAttack();

        if (autoKill) { autoKill = false; p_health.ChangeHealth(-p_stats.maxHP); }

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public void RequestAttack()
    {
        if (!IsAlive || cooldownTimer > 0f) return;

        cooldownTimer = p_stats.attackCooldown;

        // Face once at attack start
        animator.SetFloat("atkX", aimDir.x);
        animator.SetFloat("atkY", aimDir.y);
        StartCoroutine(AttackRoutine());
    }


    IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        // Delay -> Attack -> Recover
        yield return new WaitForSeconds(hitDelay);
        activeWeapon?.Attack();
        yield return new WaitForSeconds(attackDuration - hitDelay);
        IsAttacking = false;
    }

    void Die()
    {
        p_movement.SetDisabled(true);
        animator.SetTrigger("Die");
    }
}
