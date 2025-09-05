using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class P_Combat : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sprite;
    Animator animator;
    P_InputActions input;

    public C_Stats     c_Stats;
    public P_Movement  p_Movement;
    public C_Health    p_Health;

    [Header("Weapons (Player can hold 3)")]
    public W_Melee  meleeWeapon;
    public W_Ranged rangedWeapon;
    public W_Base   magicWeapon;
    
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
    public bool IsAlive => c_Stats.currentHP > 0;

    void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        animator ??= GetComponent<Animator>();
        input ??= new P_InputActions();
        
        c_Stats ??= GetComponent<C_Stats>();
        p_Movement ??= GetComponent<P_Movement>();
        p_Health ??= GetComponent<C_Health>();
        meleeWeapon  ??= GetComponentInChildren<W_Melee>();
        rangedWeapon ??= GetComponentInChildren<W_Ranged>();
        magicWeapon  ??= null; // placeholder only

        
        if (!sprite) Debug.LogWarning($"{name}: SpriteRenderer in P_Combat missing.");
        if (!animator) Debug.LogError($"{name}: Animator in P_Combat missing.");

        if (!c_Stats) Debug.LogError($"{name}: P_Stats in P_Combat missing.");
        if (!p_Movement) Debug.LogError($"{name}: P_Movement in P_Combat missing.");
        if (!p_Health) Debug.LogError($"{name}: C_Health in P_Combat missing.");

        animator.SetFloat("atkX", 0f);
        animator.SetFloat("atkY", -1f);
    }

    void OnEnable()
    {
        input?.Enable();
        p_Health.OnDied += Die;
    }

    void OnDisable()
    {
        input?.Disable();
        p_Health.OnDied -= Die;
    }

    void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        if (raw.sqrMagnitude > MIN_DISTANCE) aimDir = raw.normalized;
        // Inputs:
        // - Left Mouse  -> Melee
        // - Right Mouse -> Ranged
        // (Keep old action for backward-compat: Attack -> Melee)
        if (input.Player.MeleeAttack.triggered)  RequestAttack(meleeWeapon);
        if (input.Player.RangedAttack.triggered) RequestAttack(rangedWeapon);

        if (autoKill) { autoKill = false; p_Health.ChangeHealth(-c_Stats.maxHP); }

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public void RequestAttack(W_Base weapon)
    {
        if (!IsAlive || cooldownTimer > 0f) return;

        cooldownTimer = c_Stats.attackCooldown;

        // Face once at attack start
        animator.SetFloat("atkX", aimDir.x);
        animator.SetFloat("atkY", aimDir.y);
        StartCoroutine(AttackRoutine(weapon));
    }

    IEnumerator AttackRoutine(W_Base weapon)
    {
        // STATE: Attack START
        IsAttacking = true;

        // Delay -> Attack -> Recover
        yield return new WaitForSeconds(hitDelay);
        weapon.Attack();
        yield return new WaitForSeconds(attackDuration - hitDelay);

        // STATE: Attack END
        IsAttacking = false;
    }

    void Die()
    {
        p_Movement.SetDisabled(true);
        animator.SetTrigger("Die");
    }
}
