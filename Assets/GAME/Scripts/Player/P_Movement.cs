using System;
using UnityEngine;

public class P_Movement : MonoBehaviour
{
    [Header("Independent component to manage Player's movement and animation")]
    [Header("References")]
    Rigidbody2D rb;
    Animator animator;
    P_InputActions input;

    C_Stats c_Stats;
    C_State c_State;
    P_Combat p_Combat;
    C_Dodge c_Dodge;

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

        if (!rb) Debug.LogError($"{name}: Rigidbody2D is missing in P_Movement");
        if (!animator) Debug.LogError($"{name}: Animator is missing in P_Movement");

        if (!c_Stats) Debug.LogError($"{name}: C_Stats is missing in P_Movement");
        if (!p_Combat) Debug.LogError($"{name}: P_Combat is missing in P_Movement");
        if (!c_Dodge) Debug.LogError($"{name}: C_Dodge is missing in P_Movement");
        if (!c_State) Debug.LogError($"{name}: C_State is missing in P_Movement");

        animator?.SetFloat("moveX", 0f);
        animator?.SetFloat("moveY", -1f);
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        // Normalize to avoid diagonal speed advantage
        // If raw is near zero, x,y = 0
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        Vector2 desired = raw.sqrMagnitude > MIN_DISTANCE ? raw.normalized : Vector2.zero;

        SetMoveAxis(desired);
        
        // Do not override lastMove when attacking
        // Attack facing is stored separately in animator atkX/atkY so movement (WASD) still controls idle facing
        c_State.UpdateAnimDirections(moveAxis, lastMove);
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

        // Valve is closed when disabled OR lockDuringAttack
        bool valveClosed = disabled || c_State.CheckIsBusy();
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
