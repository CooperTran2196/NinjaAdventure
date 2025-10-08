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
    public W_Base activeWeapon;

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
    Vector2         knockback, desiredVelocity, attackDir, lastAimDir;
    bool            isStunned, isDead, isAttacking;    
    float           stunUntil, attackCooldown, attackInRangeTimer, contactTimer;

    const float MIN_DISTANCE = 0.000001f;

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
        activeWeapon = GetComponentInChildren<W_Base>();

        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", -1f);
        anim.SetFloat("idleX", 0f);
        anim.SetFloat("idleY", -1f);
    }

    void OnEnable()
    {
        c_Health.OnDied += OnDiedHandler;
        // desiredVelocity  = Vector2.zero;
        // rb.linearVelocity = Vector2.zero;
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        c_Health.OnDied -= OnDiedHandler;
        idle.enabled = wander.enabled = chase.enabled = attack.enabled = false;
        
        // desiredVelocity  = Vector2.zero;
        // rb.linearVelocity = Vector2.zero;
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
        if (isDead) return;

        // Apply this frame: block state intent when stunned/dead, but still allow knockback
        Vector2 baseVel = (isDead || isStunned) ? Vector2.zero : desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        // Decay knockback for the NEXT frame
        knockback = Vector2.MoveTowards(knockback, Vector2.zero, c_Stats.KR * Time.fixedDeltaTime);
    }

    // Set/Get attack cooldown timer for use by State_Attack
    public float GetAttackCooldown => attackCooldown;
    public void SetAttackCooldown(float attackCooldown) => this.attackCooldown = attackCooldown;
    // Only E_Controller handle the movement + knockback
    // Helper for State scripts to read/write movement intent + apply knockback
    public void SetDesiredVelocity(Vector2 desiredVelocity) => this.desiredVelocity = desiredVelocity;
    // W_Knockback now calls this first, so all states get shoved uniformly
    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;
    public void SetAttacking(bool value) => isAttacking = value;

    Vector2 ComputeAimDir() // for attacks
    {
        Vector2 dirToTarget = (Vector2)currentTarget.position - (Vector2)transform.position;
        if (dirToTarget.sqrMagnitude > MIN_DISTANCE)
            return dirToTarget.normalized;
        else
            return Vector2.zero;
    }

    Vector2 ComputeChaseDir() // for chase axis
    {
        if (!currentTarget) return Vector2.zero;
        Vector2 to = (Vector2)currentTarget.position - (Vector2)transform.position;
        float dist = to.magnitude;
        if (dist <= MIN_DISTANCE) return Vector2.zero;

        float stop = attackRange + (chase ? chase.stopBuffer : 0f);
        return (dist > stop) ? (to / dist) : Vector2.zero; // normalized or zero if within stop band
    }

    void ProcessAI()
    {
        // 1/ Check if dead
        if (c_Stats.currentHP <= 0)
        {
            SwitchState(EState.Dead);
            return;
        }

        // 2/ Sense player
        // Always check attackCircle first. If this hits the target -> the target also inside the detectionCircle
        Collider2D attackCircle = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        // No target in attackCircle -> check the detectionCircle
        Collider2D detectionCircle = attackCircle ?? Physics2D.OverlapCircle((Vector2)transform.position, detectionRange, playerLayer);

        // Check true/false for each circle depending on target location
        bool targetInsideAttackCircle    = attackCircle;
        bool targetInsideDetectionCircle = detectionCircle;

        // Set current target transform if inside either circle
        Vector2 chaseAxis = Vector2.zero;
        if (targetInsideDetectionCircle)
        {
            currentTarget = detectionCircle.transform;
            attackDir = ComputeAimDir();          // safe: currentTarget just set
            chaseAxis = ComputeChaseDir();        // normalized or zero if within stop band
        }

        // Target need to be inside attack circle for a short time before attacking
        if (targetInsideAttackCircle) attackInRangeTimer += Time.deltaTime;
        else attackInRangeTimer = 0f;

        // Only ready to attack if: inside attack circle long enough AND off cooldown
        bool readyToAttack = targetInsideAttackCircle
                          && attackInRangeTimer >= attackStartBuffer
                          && attackCooldown <= 0f;

        // 3/ NEVER interrupt an active clip
        if (currentState == EState.Attack && isAttacking) return;

        // 4/ Decide to chase/attack
        if (readyToAttack) { SwitchState(EState.Attack); return; }
        if (targetInsideDetectionCircle)
        {
            chase.SetMoveAxis(chaseAxis);
            SwitchState(EState.Chase);
            return;
        }

        // 5/ Fallback
        chase.SetMoveAxis(Vector2.zero);
        SwitchState(defaultState);
    }

    // Switch states, enabling the chosen one and disabling others
    public void SwitchState(EState s)
    {
        if (currentState == s) return;
        currentState = s;

        // disable all states first
        idle.enabled = wander.enabled = chase.enabled = attack.enabled = false;

        switch (s)
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
                attack.StartAttack(activeWeapon, attackDir);
                // attack.SetTarget(currentTarget);
                // attack.SetRanges(attackRange);
                // cooldown & “attacking” are still driven by State_Attack (no logic change)
                break;

            case EState.Chase:
                chase.enabled       = true;

                // chase.SetTarget(currentTarget);
                // chase.SetRanges(attackRange);
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

    // DEBUG: visualize detection/attack ranges
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
