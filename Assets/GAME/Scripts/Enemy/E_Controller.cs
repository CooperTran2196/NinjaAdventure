using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(State_Chase))]
[RequireComponent(typeof(State_Attack))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]
[DisallowMultipleComponent]

public class E_Controller : MonoBehaviour, I_Controller
{
    public enum EState { Idle, Wander, Chase, Attack, Dead }

    [Header("Main controller for enemy AI states")]
    [Header("References")]
    [Min(3f)]   public float detectionRange = 3f;
    [Min(1.2f)] public float attackRange = 1.2f;
                public LayerMask playerLayer;
                public EState defaultState = EState.Idle;
                public EState currentState;
                
    [Header("Weapons")]
    // public W_Base activeWeapon; // No longer needed, State_Attack will handle it

    [Header("Attack Delay Buffer (For Easy/Hard Mode)")]
    public float attackStartBuffer = 0.2f;  

    Rigidbody2D     rb;
    Animator        anim;

    State_Idle      idle;
    State_Wander    wander;
    State_Chase     chase;
    State_Attack    attack;
    C_Stats         c_Stats;
    C_Health        c_Health;

    // Runtime vars
    Transform       currentTarget;
    Vector2         knockback, desiredVelocity; //, attackDir, lastAimDir;
    bool            isStunned, isDead, isAttacking;    
    float           stunUntil, attackCooldown, attackInRangeTimer, contactTimer;

    void Awake()
    {
        rb           = GetComponent<Rigidbody2D>();
        anim         = GetComponentInChildren<Animator>();

        idle         = GetComponent<State_Idle>();
        wander       = GetComponent<State_Wander>();
        chase        = GetComponent<State_Chase>();
        attack       = GetComponent<State_Attack>();
        c_Stats      = GetComponent<C_Stats>();
        c_Health     = GetComponent<C_Health>();

        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", -1f);
        anim.SetFloat("idleX", 0f);
        anim.SetFloat("idleY", -1f);
    }

    void OnEnable()
    {
        c_Health.OnDied += OnDiedHandler;
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        c_Health.OnDied -= OnDiedHandler;
        idle.enabled = wander.enabled = chase.enabled = attack.enabled = false;
    }

    void OnDiedHandler() => SwitchState(EState.Dead);

    void Update()
    {
        // Death override
        if (isDead) return;
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;

        ProcessAI();
    }

    void FixedUpdate()
    {
        // Apply this frame: block state intent when stunned/dead, but still allow knockback
        Vector2 baseVel = (isDead || isStunned) ? Vector2.zero : desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        // Decay knockback for the NEXT frame
        if (!isDead)
        {
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, c_Stats.KR * Time.fixedDeltaTime);
        }
    }

    void ProcessAI()
    {
        // 1/ Early exit conditions
        if (c_Stats.currentHP <= 0)
        {
            SwitchState(EState.Dead);
            return;
        }

        // Never interrupt an active attack animation
        if (isAttacking) return;

        // 2/ Sense for the player and update target
        Collider2D targetInAttackRange = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        Collider2D targetInDetectRange = targetInAttackRange ?? Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        currentTarget = targetInDetectRange ? targetInDetectRange.transform : null;

        // 3/ Decide on the next state based on target's position
        if (currentTarget == null)
        {
            SwitchState(defaultState);
            attackInRangeTimer = 0f; // Reset timer when no target
            return;
        }

        // Update timer only when a target is in attack range
        if (targetInAttackRange)
            attackInRangeTimer += Time.deltaTime;

        else // Reset if target steps out of range
            attackInRangeTimer = 0f;

        // Check if conditions are met to perform an attack
        bool canAttack = targetInAttackRange 
                      && attackInRangeTimer >= attackStartBuffer 
                      && attackCooldown <= 0f;

        if (canAttack)
            SwitchState(EState.Attack);
        else // If not attacking, but target is detected, chase it
            SwitchState(EState.Chase);
    }

    // Switch states, enabling the chosen one and disabling others
    public void SwitchState(EState state)
    {
        if (currentState == state) return;
        currentState = state;

        // disable all states first
        idle.enabled = wander.enabled = chase.enabled = attack.enabled = false;

        switch (state)
        {
            case EState.Dead: // Highest priority
                desiredVelocity     = Vector2.zero;
                knockback           = Vector2.zero;
                rb.linearVelocity   = Vector2.zero;
                isDead              = true;
                isAttacking         = false;
                isStunned           = false;

                anim.SetTrigger("Die");
                break;

            case EState.Attack:
                desiredVelocity     = Vector2.zero;
                attack.enabled      = true;
                isAttacking         = true;
                attackCooldown      = c_Stats.attackCooldown;
                break;

            case EState.Chase:
                chase.enabled       = true;
                break;

            case EState.Wander:
                wander.enabled      = true;
                break;

            case EState.Idle: // Lowest priority
                desiredVelocity     = Vector2.zero;
                idle.enabled        = true;
                break;
        }
    }

    // Stun coroutine called by W_Base when applying stun effect
    public IEnumerator StunFor(float duration)
    {
        if (duration <= 0f) yield break;

        // Extend the stun end if a longer one arrives
        float newEnd = Time.time + duration;
        if (newEnd > stunUntil) stunUntil = newEnd;

        isStunned = true;
        while (Time.time < stunUntil) yield return null;

        isStunned = false;
    }

    // Collision damage while touching player
    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        // Only interact with the player layer (you already have playerLayer on the controller)
        if ((playerLayer.value & (1 << collision.collider.gameObject.layer)) == 0) return;

        // Throttle ticks using physics timestep
        if (contactTimer > 0f) { contactTimer -= Time.fixedDeltaTime; return; }

        // Apply damage if player has C_Health
        var playerHealth = collision.collider.GetComponent<C_Health>();
        if (!playerHealth.IsAlive) return;

        playerHealth.ChangeHealth(-c_Stats.collisionDamage);   // apply tick damage
        contactTimer = c_Stats.collisionTick;                  // reset tick window
    }

    // Get/Set methods
    public void SetDesiredVelocity(Vector2 desiredVelocity) => this.desiredVelocity = desiredVelocity;
    public void SetAttacking(bool value) => isAttacking = value;
    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;
    public Transform GetTarget() => currentTarget;
    public float GetAttackRange() => attackRange;
    
    // DEBUG: visualize detection/attack ranges
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
