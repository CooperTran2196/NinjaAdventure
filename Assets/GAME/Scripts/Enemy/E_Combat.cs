using System;
using System.Collections;
using UnityEngine;

public class E_Combat : MonoBehaviour
{
    [Header("References")]
    public E_Stats e_stats;
    public E_Movement e_movement;
    public SpriteRenderer sprite;
    public Animator animator;
    public W_Base activeWeapon;
    public C_Health e_health;

    [Header("AI")]
    public LayerMask playerLayer;
    [Min(1.3f)] public float attackRange = 1.3f;
    [Min(0.5f)] public float thinkInterval = 0.5f;

    [Header("Attack")]
    [Header("ALWAYS matched full clip length (0.45)")]
    public float attackDuration = 0.45f;
    [Header("ALWAYS set when the hit happens (0.15)")]
    public float hitDelay = 0.15f;
    public bool lockDuringAttack = true;

    [Header("Debug")]
    [SerializeField] bool autoKill;

    // Quick state check
    public bool IsAlive => e_stats.currentHP > 0;

    bool isAttacking;
    float contactTimer; // for collision damage
    float cooldownTimer; // for attacking cooldown

    void Awake()
    {
        sprite      ??= GetComponent<SpriteRenderer>();
        animator    ??= GetComponent<Animator>();
        e_stats     ??= GetComponent<E_Stats>();
        e_movement  ??= GetComponent<E_Movement>();
        e_health    ??= GetComponent<C_Health>();

        if (sprite      == null) Debug.LogError($"{name}: SpriteRenderer missing.");
        if (animator    == null) Debug.LogError($"{name}: Animator missing.");
        if (e_stats     == null) Debug.LogError($"{name}: E_Stats missing.");
        if (e_movement  == null) Debug.LogError($"{name}: E_Movement missing.");
        if (e_health    == null) Debug.LogError($"{name}: C_Health missing.");
    }

    void OnEnable()
    {
        StartCoroutine(ThinkLoop());
        e_health.OnDied += Die;
    }


    void OnDisable()
    {
        e_health.OnDied -= Die;
    }

    void Update()
    {
        if (autoKill) { autoKill = false; e_health?.ChangeHealth(-e_stats.maxHP); }
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    IEnumerator ThinkLoop()
    {
        var wait = new WaitForSeconds(thinkInterval);

        while (true)
        {
            if (!IsAlive) yield break;

            if (!isAttacking)
            {
                bool inAttackRange = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);

                if (lockDuringAttack)
                {
                    // Normal mode: Idle in place if player is still in range
                    bool shouldHold = inAttackRange && cooldownTimer > 0f;
                    e_movement.SetHoldInRange(shouldHold);
                }
                else
                {
                    // Hard mode: Enemies never hold between attack
                    e_movement.SetHoldInRange(false);
                }
                // If close enough and off cooldown, begin an attack
                if (inAttackRange && cooldownTimer <= 0f)
                    StartCoroutine(AttackRoutine());
            }
            yield return wait;
        }
    }

    // Collision Damage
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsAlive) return;

        // Count down using physics timestep
        if (contactTimer > 0f)
        {
            contactTimer -= Time.fixedDeltaTime;
            return;
        }
        var playerHealth = collision.collider.GetComponent<C_Health>();
        if (playerHealth == null || !playerHealth.IsAlive) return; // Only damage a live player
        playerHealth.ChangeHealth(-e_stats.collisionDamage); //Apply damage
        contactTimer = e_stats.collisionTick; // reset tick window
    }


    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetBool("isAttacking", true);

        // Aim the directional attack
        var dir = e_movement.lastMove;
        animator.SetFloat("atkX", dir.x);
        animator.SetFloat("atkY", dir.y);

        // Delay -> Attack -> recover
        yield return new WaitForSeconds(hitDelay);
        // Placeholder
        activeWeapon?.Attack();
        yield return new WaitForSeconds(attackDuration - hitDelay);
        
        // Decide keep chasing/idle based on lockDuringAttack (MODE)
        if (lockDuringAttack)
        {
            bool stillInRange = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
            e_movement.SetHoldInRange(stillInRange);
        }
        else
        {
            e_movement.SetHoldInRange(false);
        }

        isAttacking = false;
        animator.SetBool("isAttacking", false);

        cooldownTimer = e_stats.attackCooldown;
    }

    void Die()
    {
        e_movement?.SetDisabled(true);
        animator?.SetTrigger("Die");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.9f); // red attack ring
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
