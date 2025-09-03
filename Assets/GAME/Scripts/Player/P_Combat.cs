using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]

public class P_Combat : MonoBehaviour
{
    [Header("References")]
    public P_Stats stats;
    public P_Movement movement;
    public SpriteRenderer sprite;
    public Animator animator;
    public W_Base activeWeapon; // Placeholder
    public C_Health health;


    [Header("Attack")]
    public float attackDuration = 0.45f; // ALWAYS matched full clip length
    public float hitDelay = 0.15f; // ALWAYS set when the hit happens
    public bool lockDuringAttack = true; // read by E_Movement valve


    [Header("Keyword to Trigger Die Animation")]
    public string deathTrigger = "Die";

    [Header("Debug")]
    [SerializeField] private bool autoKill;

    P_InputActions input;
    Vector2 aimDir = Vector2.down;
    const float MIN_DISTANCE = 0.0001f;

    // Placeholder for Events
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDied;

    // Quick state check
    public bool IsAlive => stats.currentHP > 0;

    float cooldownTimer;
    void Awake()
    {
        animator ??= GetComponent<Animator>();
        sprite ??= GetComponent<SpriteRenderer>();
        stats ??= GetComponent<P_Stats>();
        movement ??= GetComponent<P_Movement>();
        input ??= new P_InputActions();

        if (animator == null) Debug.LogError($"{name}: Animator missing.");
        if (sprite == null) Debug.LogWarning($"{name}: SpriteRenderer missing.");
        if (stats == null) Debug.LogError($"{name}: P_Stats missing.");
        if (movement == null) Debug.LogError($"{name}: P_Movement missing.");

        animator?.SetFloat("atkX", 0f);
        animator?.SetFloat("atkY", -1f);
        baseColor = sprite.color;
    }

    void OnEnable()
    {
        input?.Enable();
        if (health != null) health.OnDied += Die;
    }


    void OnDisable()
    {
        input?.Disable();
        if (health != null) health.OnDied -= Die;
    }



    void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        if (raw.sqrMagnitude > MIN_DISTANCE) aimDir = raw.normalized;
        if (input.Player.Attack.triggered) RequestAttack();

        if (autoKill) { autoKill = false; health?.ChangeHealth(-stats.maxHP); }

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public void RequestAttack()
    {
        if (!IsAlive || cooldownTimer > 0f) return;

        cooldownTimer = stats.attackCooldown;

        // Face once at attack start
        animator.SetFloat("atkX", aimDir.x);
        animator.SetFloat("atkY", aimDir.y);
        StartCoroutine(AttackRoutine());
    }


    IEnumerator AttackRoutine()
    {
        animator.SetBool("isAttacking", true);

        // Delay -> Attack -> Recover
        yield return new WaitForSeconds(hitDelay);
        activeWeapon?.Attack();
        yield return new WaitForSeconds(attackDuration - hitDelay);

        animator.SetBool("isAttacking", false);
    }

    void Die()
    {
        movement?.SetDisabled(true);
        animator?.SetTrigger(deathTrigger);
    }

    /// DEBUG
    public void Kill() => health?.ChangeHealth(-stats.maxHP);
}
