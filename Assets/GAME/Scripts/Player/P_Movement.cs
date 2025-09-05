using System;
using UnityEngine;

public class P_Movement : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    Animator animator;
    P_InputActions input;

    public C_Stats c_Stats;
    public P_Combat p_Combat;
    public C_Dodge c_Dodge;
    public C_State c_State;

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

        c_Stats ??= GetComponent<C_Stats>();
        p_Combat ??= GetComponent<P_Combat>();
        c_Dodge ??= GetComponent<C_Dodge>();
        c_State ??= GetComponent<C_State>();

        if (!rb) Debug.LogError($"{name}: Rigidbody2D in P_Movement missing.");
        if (!animator) Debug.LogError($"{name}: Animator in P_Movement missing.");
        if (!c_Stats) Debug.LogError($"{name}: C_Stats in P_Movement missing.");
        if (!p_Combat) Debug.LogError($"{name}: P_Combat in P_Movement missing.");
        if (!c_Dodge) Debug.LogError($"{name}: C_Dodge in P_Movement missing.");
        if (!c_State) Debug.LogError($"{name}: C_State in P_Movement missing.");

        lastMove = Vector2.down;
        animator?.SetFloat("moveX", 0f);
        animator?.SetFloat("moveY", -1f);
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        // Normalize to avoid diagonal speed advantage; also gives clean 4/8-way
        // If raw is near zero, normalized will be (0,0)
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        Vector2 desired = raw.sqrMagnitude > MIN_DISTANCE ? raw.normalized : Vector2.zero;

        SetMoveAxis(desired);
        
        bool busy = c_State != null && c_State.CheckIsBusy();
        C_Anim.UpdateAnimDirections(animator, busy, moveAxis, lastMove, MIN_DISTANCE);
    }


    void FixedUpdate()
    {
        Vector2 forced  = c_Dodge.ForcedVelocity;
        Vector2 baseVel = (forced != Vector2.zero) ? forced : velocity;
        Vector2 final   = baseVel + knockback;
        rb.linearVelocity = final;


        if (knockback.sqrMagnitude > 0f)
        {
            float step = c_Stats.KR * Time.fixedDeltaTime;
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
        bool valveClosed = disabled || (attacking && p_Combat.lockDuringAttack);
        // If valve is closed, stop; otherwise apply intended velocity
        Vector2 intendedVelocity = moveAxis * c_Stats.MS;
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
        }
    }

    public void ReceiveKnockback(Vector2 force) => knockback += force;

}
