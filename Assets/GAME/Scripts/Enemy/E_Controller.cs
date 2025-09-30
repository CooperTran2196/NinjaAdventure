using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(State_Chase))]
[RequireComponent(typeof(State_Attack))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]


[DisallowMultipleComponent]

public class E_Controller : MonoBehaviour
{
    public enum EState { Idle, Wander, Chase, Attack }

    [Header("Main controller for enemy AI states")]
    [Header("References")]
    [Min(3f)]   public float detectionRange = 3f;
    [Min(1.2f)] public float attackRange = 1.2f;
                public LayerMask playerLayer;
                public EState defaultState = EState.Idle;
    Rigidbody2D rb;
    Animator anim;
    EState currentState;
    State_Idle idle;
    State_Wander wander;
    State_Chase chase;
    State_Attack attack;
    C_Stats c_Stats;
    C_Health c_Health;

    // Runtime vars
    Transform currentTarget;
    Vector2 desiredVelocity;
    Vector2 knockback;
    bool isStunned;
    bool isDead;
    float stunUntil;
    float attackCooldown;

    // Helper for State scripts to read/write movement intent + apply knockback
    public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;
    // W_Knockback now calls this first, so all states get shoved uniformly
    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;

    void Awake()
    {
        rb       ??= GetComponent<Rigidbody2D>();
        anim ??= GetComponentInChildren<Animator>();
        idle = GetComponent<State_Idle>();
        wander = GetComponent<State_Wander>();
        chase = GetComponent<State_Chase>();
        attack = GetComponent<State_Attack>();
        c_Stats = GetComponent<C_Stats>();
        c_Health = GetComponent<C_Health>();
        c_Stats    ??= GetComponent<C_Stats>();
        c_Health   ??= GetComponent<C_Health>();

        if (!rb)     Debug.LogError($"{name}: Rigidbody2D is missing in E_Controller.");
        if (!anim)   Debug.LogError($"{name}: Animator is missing in E_Controller.");
        if (!idle) Debug.LogError($"{name}: State_Idle is missing in E_Controller.");
        if (!wander) Debug.LogError($"{name}:State_Wander is missing in E_Controller.");
        if (!chase) Debug.LogError($"{name}: Missing State_Chase in E_Controller.");
        if (!attack) Debug.LogError($"{name}: Missing State_Attack in E_Controller.");
        if (!c_Stats)  Debug.LogError($"{name}: C_Stats missing.");
        if (!c_Health) Debug.LogError($"{name}: C_Health missing.");
    }

    void OnEnable()
    {
        c_Health.OnDied += HandleDeath;
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        c_Health.OnDied -= HandleDeath;
        idle.enabled   = false;
        wander.enabled = false;
        chase.enabled  = false;
        attack.enabled = false;
    }

    void Update()
    {
        if (isDead) return;
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;
        // Always check attackCircle first. If this hits the player -> the player also inside the detectionCircle
        Collider2D attackCircle = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        // If attackCircle not null, reuse it. Otherwise check the detectionCircle
        Collider2D detectionCircle = attackCircle ?? Physics2D.OverlapCircle((Vector2)transform.position, detectionRange, playerLayer);
        // Check true/false for each circle depending on player location
        bool targetInsideAttackCircle = attackCircle;
        bool targetInsideDetectionCircle = detectionCircle;

        // Set current target to the player if inside either circle, otherwise null
        if (targetInsideDetectionCircle) currentTarget = detectionCircle.transform;

        // Decide desired state from ring position
        EState desiredState =
        targetInsideAttackCircle    ? EState.Attack :   // Attack if inside attack range
        targetInsideDetectionCircle ? EState.Chase  :   // Chase if inside detection range
                                      defaultState;     // Otherwise default (Idle/Wander)

        // Never interrupt an active attack clip
        if (currentState == EState.Attack && attack.IsAttacking && desiredState != EState.Attack) return;
        // If nothing changed just return
        if (desiredState == currentState) return;

        // Apply state change
        switch (desiredState)
        {
            case EState.Attack:
                attack.SetTarget(currentTarget);
                SwitchState(EState.Attack);
                break;

            case EState.Chase:
                chase.SetTarget(currentTarget);
                SwitchState(EState.Chase);
                break;

            default:
                currentTarget = null;
                SwitchState(defaultState);
                break;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // 1) Apply this frame: block state intent when stunned/dead, but still allow knockback
        Vector2 baseVel = (isDead || isStunned) ? Vector2.zero : desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        // 2) Decay knockback for the NEXT frame — no defaultKR, require stats.KR
        if (knockback.sqrMagnitude > 0f)
        {
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, c_Stats.KR * Time.fixedDeltaTime);
        }
    }

    // Set/Get attack cooldown timer for use by State_Attack
    public float GetAttackCooldown => attackCooldown;
    public void SetAttackCooldown(float attackCooldown)
    {
        this.attackCooldown = attackCooldown;
    }

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



    void HandleDeath()
    {
        // Mark as dead
        isDead = true;

        // Freeze motion
        knockback = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        // Stop AI scripts immediately so no more actions are scheduled
        idle.enabled = false;
        wander.enabled = false;
        chase.enabled = false;
        attack.enabled = false;

        // Play death anim
        anim.SetTrigger("Die");
    }


    public void SwitchState(EState s)
    {
        if (currentState == s) return;
        currentState = s;

        idle.enabled = (s == EState.Idle);
        wander.enabled = (s == EState.Wander);
        chase.enabled = (s == EState.Chase);
        attack.enabled = (s == EState.Attack);

        // On enter, provide context to newly enabled state
        if (s == EState.Chase)
        {
            chase.SetTarget(currentTarget);
            chase.SetRanges(attackRange);
        }
        else if (s == EState.Attack)
        {
            attack.SetTarget(currentTarget);
            attack.SetRanges(attackRange);
        }
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
