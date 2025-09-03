using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public class P_Movement : MonoBehaviour
{
    [Header("Facing / Animator")]
    public Vector2 lastMove = Vector2.down; // Default facing down

    [Header("References")]
    public P_Stats stats;
    public P_Combat combat;

    // Components
    private Rigidbody2D rb;
    private Animator animator;

    // New Input System
    P_InputActions input;

    // Runtime state
    bool disabled;      // When true, movement/animation stops
    Vector2 moveAxis;   // Desired direction of travel
    Vector2 velocity;   // Final velocity applied to Rigidbody2D.linearVelocity
    Vector2 knockback;

    const float MIN_DISTANCE = 0.0001f;

    // Placeholder for events
    public event Action<Vector2> OnDirectionChanged;
    public event Action<Vector2> OnVelocityChanged;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponent<Animator>();
        stats ??= GetComponent<P_Stats>();
        combat ??= GetComponent<P_Combat>();

        input ??= new P_InputActions();

        if (rb == null) Debug.LogError($"{name}: Rigidbody2D missing.");
        if (animator == null) Debug.LogError($"{name}: Animator missing.");
        if (stats == null) Debug.LogError($"{name}: P_Stats missing.");
        if (combat == null) Debug.LogError($"{name}: P_Combat missing.");

        lastMove = Vector2.down;
        animator?.SetFloat("moveX", 0f);
        animator?.SetFloat("moveY", -1f);
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();

        // Normalize to avoid diagonal speed advantage; also gives clean 4/8-way
        // If raw is near zero, normalized will be (0,0)
        Vector2 desired = raw.sqrMagnitude > MIN_DISTANCE ? raw.normalized : Vector2.zero;

        SetMoveAxis(desired);
        C_Anim.ApplyMoveIdle(animator, animator.GetBool("isAttacking"), moveAxis, lastMove, MIN_DISTANCE);
    }


    void FixedUpdate()
    {
        Vector2 final = velocity + knockback;
        rb.linearVelocity = final;

        if (knockback.sqrMagnitude > 0f)
        {
            float step = (stats ? stats.KR : 0f) * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
        }
    
    }


    void SetMoveAxis(Vector2 v)
    {
        if (disabled)
        {
            moveAxis = Vector2.zero;
            velocity = Vector2.zero;
            return;
        }

        // Only fire direction event if direction actually changed.
        if (moveAxis != v && v.sqrMagnitude > MIN_DISTANCE)
        {
            lastMove = v; // Idle facing uses latest non-zero direction
        }

        moveAxis = v;

        // Velocity valve: stop only if attacking AND lockDuringAttack
        // Read attack-state from Animator
        bool attacking = animator.GetBool("isAttacking");
        // Valve is closed when disabled OR lockDuringAttack
        bool valveClosed = disabled || (attacking && combat.lockDuringAttack);
        // If valve is closed, stop; otherwise apply intended velocity
        Vector2 intendedVelocity = moveAxis * stats.MS;
        velocity = valveClosed ? Vector2.zero : intendedVelocity;
    }

    public void SetDisabled(bool isDisabled)
    {
        disabled = isDisabled;
        if (isDisabled)
        {
            moveAxis = Vector2.zero;
            velocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero; // immediate stop
            animator?.SetBool("isMoving", false);
        }
    }

    public void ReceiveKnockback(Vector2 force)
    {
        knockback += force;
    }

    
}
