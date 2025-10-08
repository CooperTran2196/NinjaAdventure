using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_State))]
[RequireComponent(typeof(E_Combat))]
[DisallowMultipleComponent]

public class E_Movement : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sprite;
    Rigidbody2D rb;
    Animator animator;

    C_Stats c_Stats;
    C_State c_State;
    E_Combat e_Combat;


    [Header("Detection (OverlapCircle)")]
    public LayerMask playerLayer;
    [Min(3f)] public float detectionRadius = 3f;

    [Header("Facing / Animator")]
    public Vector2 lastMove = Vector2.down;

    // Runtime state
    Transform target;
    bool disabled;
    bool holdInRange;
    Vector2 moveAxis;
    Vector2 velocity;
    Vector2 knockback;

    const float MIN_DISTANCE = 0.000001f;

    void Awake()
    {
        sprite   ??= GetComponent<SpriteRenderer>();
        rb       ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponent<Animator>();

        c_Stats  ??= GetComponent<C_Stats>();
        c_State  ??= GetComponent<C_State>();
        e_Combat ??= GetComponent<E_Combat>();


        if (!sprite)   Debug.LogError($"{name}: SpriteRenderer is missing in E_Movement");
        if (!rb)       Debug.LogError($"{name}: Rigidbody2D is missing in E_Movement");
        if (!animator) Debug.LogError($"{name}: Animator is missing in E_Movement");

        if (!c_Stats)  Debug.LogError($"{name}: C_Stats is missing in E_Movement");
        if (!c_State)  Debug.LogError($"{name}: C_State is missing in E_Movement");
        if (!e_Combat) Debug.LogError($"{name}: E_Combat is missing in E_Movement");

    }

    void Update()
    {
        Chase();
        
        // Preserve lastMove during pauses/idle
        if (c_State.Is(C_State.ActorState.Attack))
        {
            lastMove = c_State.GetAttackDirection();
        }
        c_State.UpdateAnimDirections(moveAxis, lastMove);
    }

    void FixedUpdate()
    {
        // Skip movement if wandering
        if (c_State.Is(C_State.ActorState.Wander)) return;

        // Apply movement + knockback
        Vector2 final = velocity + knockback;
        rb.linearVelocity = final; // replace linearVelocity

        if (knockback.sqrMagnitude > 0f)
        {
            float step = c_Stats.KR * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
        }
    }

    void Chase()
    {
        // If dead, ensure everything stays stopped and wander cannot reactivate


        // Setup target if player comes close
        var hit = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius, playerLayer);
        target = hit ? hit.transform : null;

        // Toggle wandering by detection


        if (disabled)
        {
            moveAxis = Vector2.zero;
            velocity = Vector2.zero;
            return;
        }

        if (target == null)
        {
            moveAxis = Vector2.zero;
        }
        else
        {
            Vector2 to = (Vector2)target.position - (Vector2)transform.position;
            bool hasDir = to.sqrMagnitude > MIN_DISTANCE;
            if (hasDir) lastMove = to.normalized;

            // If holding then don't create intent, otherwise face & move toward target
            moveAxis = (holdInRange || !hasDir) ? Vector2.zero : lastMove;
        }

        // Velocity valve
        bool valveClosed = disabled || holdInRange || (c_State != null && c_State.CheckIsBusy());
        Vector2 intendedVelocity = moveAxis * c_Stats.MS;
        velocity = valveClosed ? Vector2.zero : intendedVelocity;
    }

    // Freeze movement/anim immediately
    public void SetDisabled(bool isDisabled)
    {
        disabled = isDisabled;
        if (isDisabled)
        {
            holdInRange = false;
            moveAxis = Vector2.zero;
            velocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero; // immediate stop
        }
    }

    // External knockback request
    public void ReceiveKnockback(Vector2 force) { knockback += force; }

    // Combat asks to idle-in-place during cooldown
    public void SetHoldInRange(bool v) => holdInRange = v;


    // Debug: show detection radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.55f, 0f, 0.9f); // orange detection ring
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
