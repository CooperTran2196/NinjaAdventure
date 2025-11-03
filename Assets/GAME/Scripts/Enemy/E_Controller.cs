using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]
[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(State_Chase))]

public class E_Controller : MonoBehaviour, I_Controller
{
    public enum EState { Idle, Wander, Chase, Attack, Dead }

    [Header("Main controller for enemy AI states")]
    [Header("References")]
    Rigidbody2D  rb;
    Animator     anim;
    C_Stats      c_Stats;
    C_Health     c_Health;
    State_Idle   idle;
    State_Wander wander;
    State_Chase  chase;
    State_Attack attack;  // Optional - may be null for collision-only enemies


    [Header("AI Settings")]
    [Min(3f)] public float       detectionRange    = 3f;
              public float       attackRange        = 1.2f;
              public float       attackStartBuffer  = 0.2f;
              public LayerMask   playerLayer;

    [Header("State")]
    public EState defaultState = EState.Idle;
    public EState currentState;

    // Runtime state
    Transform currentTarget;
    Vector2   knockback, desiredVelocity;
    bool      isStunned, isDead, isAttacking;
    float     stunUntil, attackCooldown, attackInRangeTimer, contactTimer;
    
    // Ladder system
    ENV_Ladder currentLadder;

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        c_Stats  = GetComponent<C_Stats>();
        c_Health = GetComponent<C_Health>();
        idle     = GetComponent<State_Idle>();
        wander   = GetComponent<State_Wander>();
        chase    = GetComponent<State_Chase>();
        attack   = GetComponent<State_Attack>();  // Optional - may be null

        // Set default animator facing (down)
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
        idle.enabled = wander.enabled = chase.enabled = false;
        if (attack != null) attack.enabled = false;
    }

    void OnDiedHandler() => SwitchState(EState.Dead);

    void Update()
    {
        if (isDead) return;
        
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;

        ProcessAI();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Block movement when stunned, but allow knockback
        Vector2 baseVel = isStunned ? Vector2.zero : desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        // Decay knockback each frame
        knockback = Vector2.MoveTowards(knockback, Vector2.zero, c_Stats.KR * Time.fixedDeltaTime);
    }

    // AI LOGIC

    // Processes enemy AI: detects player, manages attack timing, switches states
    void ProcessAI()
    {
        if (c_Stats.currentHP <= 0) { SwitchState(EState.Dead); return; }

        // Detect player in attack/detection ranges
        Collider2D targetInAttackRange = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        Collider2D targetInDetectRange = targetInAttackRange ?? Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        currentTarget = targetInDetectRange ? targetInDetectRange.transform : null;

        // No target: return to default behavior
        if (currentTarget == null)
        {
            if (currentState != defaultState && !isAttacking) SwitchState(defaultState);
            attackInRangeTimer = 0f;
            return;
        }

        // Update attack timer
        if (targetInAttackRange) attackInRangeTimer += Time.deltaTime;
        else attackInRangeTimer = 0f;

        // Attack if conditions met
        bool canAttack = targetInAttackRange 
                      && attackInRangeTimer >= attackStartBuffer 
                      && attackCooldown <= 0f
                      && !isAttacking
                      && attack != null;

        if (canAttack) TriggerAttack();
        else if (currentTarget != null && currentState != EState.Attack) SwitchState(EState.Chase);
    }

    // STATE SYSTEM

    // Switches to new state, disabling others (except attack if mid-combo)
    public void SwitchState(EState state)
    {
        if (currentState == state) return;
        currentState = state;

        idle.enabled = wander.enabled = chase.enabled = false;
        if (!isAttacking && attack != null) attack.enabled = false;

        switch (state)
        {
            case EState.Dead:
                desiredVelocity   = Vector2.zero;
                knockback         = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
                isDead            = true;
                isAttacking       = false;
                isStunned         = false;
                if (attack != null) attack.enabled = false;
                anim.SetTrigger("Die");
                break;

            case EState.Chase:
                chase.enabled = true;
                break;

            case EState.Wander:
                wander.enabled = true;
                break;

            case EState.Idle:
                desiredVelocity = Vector2.zero;
                idle.enabled    = true;
                break;
        }
    }

    // Initiates attack without disabling movement state
    void TriggerAttack()
    {
        if (isAttacking || attack == null) return;

        attack.enabled = true;
        currentState   = EState.Attack;
        isAttacking    = true;
        attackCooldown = c_Stats.attackCooldown;
    }

    // COMBAT EFFECTS

    // Stuns enemy for duration (extends if longer stun applied)
    public IEnumerator StunFor(float duration)
    {
        if (duration <= 0f) yield break;

        float newEnd = Time.time + duration;
        if (newEnd > stunUntil) stunUntil = newEnd;

        isStunned = true;
        while (Time.time < stunUntil) yield return null;
        isStunned = false;
    }

    // Applies contact damage to player on collision
    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if ((playerLayer.value & (1 << collision.collider.gameObject.layer)) == 0) return;
        if (contactTimer > 0f) { contactTimer -= Time.fixedDeltaTime; return; }

        var playerHealth = collision.collider.GetComponent<C_Health>();
        if (!playerHealth.IsAlive) return;

        playerHealth.ChangeHealth(-c_Stats.collisionDamage);
        contactTimer = c_Stats.collisionTick;
    }

    public void SetDesiredVelocity(Vector2 desiredVelocity) => this.desiredVelocity = desiredVelocity;
    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;
    public Transform GetTarget() => currentTarget;
    public float GetAttackRange() => attackRange;
    
    // Called by State_Attack when attack animation ends
    public void SetAttacking(bool value)
    {
        isAttacking = value;
        if (!value)
        {
            if (currentTarget != null) SwitchState(EState.Chase);
            else SwitchState(defaultState);
        }
    }
    
    // LADDER SYSTEM
    
    public void EnterLadder(ENV_Ladder ladder) => currentLadder = ladder;
    public void ExitLadder() => currentLadder = null;
    public Vector2 ApplyLadderModifiers(Vector2 velocity) => currentLadder ? currentLadder.ApplyLadderSpeed(velocity) : velocity;
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
