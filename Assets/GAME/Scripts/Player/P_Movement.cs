using System;
using UnityEngine;

public class P_Movement : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    Animator animator;
    P_InputActions input;

    public P_Stats p_stats;
    public P_Combat p_combat;

    [Header("Facing / Animator")]
    public Vector2 lastMove = Vector2.down; // Default facing down

    // Runtime state
    bool disabled;      // When true, movement/animation stops
    Vector2 moveAxis;   // Desired direction of travel
    Vector2 velocity;   // Final velocity applied to Rigidbody2D.linearVelocity
    Vector2 knockback;

    const float MIN_DISTANCE = 0.0001f;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponent<Animator>();
        input ??= new P_InputActions();

        p_stats ??= GetComponent<P_Stats>();
        p_combat ??= GetComponent<P_Combat>();

        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing.");
        if (!animator) Debug.LogError($"{name}: Animator missing.");

        if (!p_stats) Debug.LogError($"{name}: P_Stats missing.");
        if (!p_combat) Debug.LogError($"{name}: P_Combat missing.");

        lastMove = Vector2.down;
        animator?.SetFloat("moveX", 0f);
        animator?.SetFloat("moveY", -1f);
    }

    void OnEnable()  => input.Enable();
    void OnDisable() => input.Disable();

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
            float step = (p_stats ? p_stats.KR : 0f) * Time.fixedDeltaTime;
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
        bool valveClosed = disabled || (attacking && p_combat.lockDuringAttack);
        // If valve is closed, stop; otherwise apply intended velocity
        Vector2 intendedVelocity = moveAxis * p_stats.MS;
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

    public void ReceiveKnockback(Vector2 force) => knockback += force;
}
