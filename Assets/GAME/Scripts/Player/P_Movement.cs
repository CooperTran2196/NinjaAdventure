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

    // ===== DODGE ADD: fields =====
    [Header("Dodge")]
    public string dodgeTrigger = "Dodge";
    public Sprite dodgeSprite;

    bool isDodging;
    float dodgeCooldownTimer;
    Vector2 dodgeDir;
    Vector2 dodgeVelocity; // forced velocity during dodge

    C_AfterimageSpawner afterimage;
    // ===== end DODGE ADD =====

    void Awake()
    {
        // ===== DODGE ADD: Awake() =====
        afterimage ??= GetComponent<C_AfterimageSpawner>();
        if (!afterimage) Debug.LogWarning("P_Movement: C_AfterimageSpawner missing.");

        input ??= new P_InputActions();
        input.Player.Move.Enable();
        input.Player.Dodge.Enable(); // make sure this action exists in your Input Actions
        // ===== end DODGE ADD =====
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

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        // ===== DODGE ADD: Update() =====
        if (dodgeCooldownTimer > 0f) dodgeCooldownTimer -= Time.deltaTime;

        if (input.Player.Dodge.WasPressedThisFrame())
        {
            RequestDodge();
        }
        // ===== end DODGE ADD =====

        Vector2 raw = input.Player.Move.ReadValue<Vector2>();

        // Normalize to avoid diagonal speed advantage; also gives clean 4/8-way
        // If raw is near zero, normalized will be (0,0)
        Vector2 desired = raw.sqrMagnitude > MIN_DISTANCE ? raw.normalized : Vector2.zero;

        SetMoveAxis(desired);
        C_Anim.ApplyMoveIdle(animator, animator.GetBool("isAttacking"), moveAxis, lastMove, MIN_DISTANCE);
    }


    void FixedUpdate()
    {
        Vector2 final = isDodging ? (dodgeVelocity /* + knockback if you sum elsewhere */) : (velocity + knockback);
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
    // ===== DODGE ADD: methods =====
    void RequestDodge()
    {
        if (isDodging) return;
        if (dodgeCooldownTimer > 0f) return;

        // Direction from lastMove (fallback down if zero)
        dodgeDir = (lastMove == Vector2.zero) ? Vector2.down : lastMove.normalized;

        // Cancel current actions on purpose for animation-cancel gameplay
        animator?.SetBool("isAttacking", false);
        animator?.SetBool("isMoving", false);
        velocity = Vector2.zero;
        moveAxis = Vector2.zero;

        // Duration is derived from distance & speed
        float duration = (p_stats.dodgeSpeed > 0f) ? (p_stats.dodgeDistance / p_stats.dodgeSpeed) : 0f;

        // Enter dodge: set Animator bool and internal state immediately
        animator?.SetBool("isDodging", true);
        isDodging = true;
        dodgeVelocity = dodgeDir * p_stats.dodgeSpeed;

        // Trail uses your dedicated dodgeSprite (not current frame)
        afterimage?.StartBurst(duration, dodgeSprite);

        StartCoroutine(EndDodgeAfter(duration));
    }

    System.Collections.IEnumerator EndDodgeAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        dodgeVelocity = Vector2.zero;
        isDodging = false;
        animator?.SetBool("isDodging", false); // exits P_Dodge via transition
        dodgeCooldownTimer = p_stats.dodgeCooldown;
    }
    // ===== end DODGE ADD =====

}
