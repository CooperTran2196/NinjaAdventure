using UnityEngine;

[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(State_Chase))]
[RequireComponent(typeof(State_Attack))]
[DisallowMultipleComponent]

public class E_Controller : MonoBehaviour
{
    public enum EState { Idle, Wander, Chase, Attack }

    [Header("Main controller for enemy AI states")]
    [Header("Ranges and Layers")]
    [Min(3f)] public float detectionRange = 3f;
    [Min(1.2f)] public float attackRange = 1.2f;
    public LayerMask playerLayer;

    [Header("States")]
    public EState defaultState = EState.Idle;
    EState currentState;
    public State_Idle idle;
    public State_Wander wander;
    public State_Chase chase;
    public State_Attack attack;

    // Runtime variables
    Transform currentTarget;
    // Movement blend + FX
    [Header("Movement / FX")]
    public string deathTrigger = "Die";
    public bool autoKill;

    Rigidbody2D rb;
    Animator animator;
    C_Stats stats;
    C_Health health;
    Collider2D[] cols;

    // Unified movement intent + knockback
    Vector2 desiredVelocity;
    Vector2 knockback;
    bool isStunned;
    bool isDead;
    Coroutine stunRoutine;


    void Awake()
    {
        idle = GetComponent<State_Idle>();
        wander = GetComponent<State_Wander>();
        chase = GetComponent<State_Chase>();
        attack = GetComponent<State_Attack>();

        if (!idle) Debug.LogError($"{name}: Missing State_Idle in E_Controller.");
        if (!wander) Debug.LogError($"{name}: Missing State_Wander in E_Controller.");
        if (!chase) Debug.LogError($"{name}: Missing State_Chase in E_Controller.");
        if (!attack) Debug.LogError($"{name}: Missing State_Attack in E_Controller.");

        rb       ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponentInChildren<Animator>();
        stats    ??= GetComponent<C_Stats>();
        health   ??= GetComponent<C_Health>();
        cols     ??= GetComponentsInChildren<Collider2D>(true);

        if (!rb)     Debug.LogError($"{name}: Rigidbody2D missing.");
        if (!health) Debug.LogError($"{name}: C_Health missing.");
    }


    void OnEnable()
    {
        if (health) health.OnDied += HandleDeath;

        if (autoKill && health)
        {
            autoKill = false;
            health.Kill(); // uses stats.maxHP internally per your style
        }

        SwitchState(defaultState);
    }

    void OnDisable()
    {
        if (health) health.OnDied -= HandleDeath;
        idle.enabled   = false;
        wander.enabled = false;
        chase.enabled  = false;
        attack.enabled = false;
    }
    public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;


    // W_Knockback now calls this first, so all states get shoved uniformly
    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;

    void Update()
    {
        // Always check attackCircle first. If this hits the player -> the player also inside the detectionCircle
        Collider2D attackCircle = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        // If attackCircle not null, reuse it. Otherwise check the detectionCircle
        Collider2D detectionCircle = attackCircle ?? Physics2D.OverlapCircle((Vector2)transform.position, detectionRange, playerLayer);
        // Check true/false for each circle depending on player location
        bool targetInsideAttackCircle = attackCircle;
        bool targetInsideDetectionCircle = detectionCircle;

        if (targetInsideDetectionCircle) currentTarget = detectionCircle.transform;

        // Optional: the current state reacts immediately without forcing a state change
        // if (chase.enabled) chase.SetRanges(attackRange);
        // if (attack.enabled) attack.SetRanges(attackRange);

        // Decide desired state from ring position
        EState desiredState =
        targetInsideAttackCircle    ? EState.Attack :
        targetInsideDetectionCircle ? EState.Chase  :
                                      defaultState;

        // Never interrupt an active attack clip
        if (currentState == EState.Attack && attack.IsAttacking && desiredState != EState.Attack) return;
        // If nothing changed just return
        if (desiredState == currentState) return;

        // Apply the change
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
        if (!rb) return;

        // 1) Apply this frame: block state intent when stunned/dead, but still allow knockback
        Vector2 baseVel = (isDead || isStunned) ? Vector2.zero : desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        // 2) Decay knockback for the NEXT frame — no defaultKR, require stats.KR
        if (knockback.sqrMagnitude > 0f)
        {
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, stats.KR * Time.fixedDeltaTime);
        }
    }



    public void Stun(float duration)
    {
        if (stunRoutine != null) StopCoroutine(stunRoutine);
        stunRoutine = StartCoroutine(StunRoutine(duration));
    }


    System.Collections.IEnumerator StunRoutine(float t)
    {
        isStunned = true;
        yield return new WaitForSeconds(t);
        isStunned = false;
        stunRoutine = null;

    }


    void HandleDeath()
    {
        isDead = true;
        desiredVelocity = Vector2.zero;

        // Stop AI scripts immediately so no more actions are scheduled


        if (idle) idle.enabled = false;
        if (wander) wander.enabled = false;
        if (chase) chase.enabled = false;
        if (attack) attack.enabled = false;

        // Turn off all hit/hurt boxes so this enemy can’t hit or be hit while fading
        if (cols != null) foreach (var c in cols) c.enabled = false;

        // Freeze motion
        desiredVelocity = Vector2.zero;
        knockback = Vector2.zero;
        if (rb) rb.linearVelocity = Vector2.zero;

        // Play death anim; let C_Health/C_FX handle FadeAndDestroy (do NOT call fade here)
        animator?.SetTrigger(string.IsNullOrEmpty(deathTrigger) ? "Die" : deathTrigger);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
