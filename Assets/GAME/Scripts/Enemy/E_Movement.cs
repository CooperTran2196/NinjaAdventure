using System;
using UnityEngine;

public class E_Movement : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sprite;
    Rigidbody2D rb;
    Animator animator;
    
    public C_Stats c_Stats;   
    public E_Combat e_Combat;

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

    const float MIN_DISTANCE = 0.0001f;
    
    void Awake()
    {
        sprite      ??= GetComponent<SpriteRenderer>();
        rb          ??= GetComponent<Rigidbody2D>();
        animator    ??= GetComponent<Animator>();

        c_Stats     ??= GetComponent<C_Stats>();
        e_Combat    ??= GetComponent<E_Combat>();
        

        if (sprite      == null) Debug.LogError($"{name}: SpriteRenderer in E_Movement missing.");
        if (rb          == null) Debug.LogError($"{name}: Rigidbody2D in E_Movement missing.");
        if (animator    == null) Debug.LogError($"{name}: Animator in E_Movement missing.");
        
        if (c_Stats     == null) Debug.LogError($"{name}: E_Stats in E_Movement missing.");
        if (e_Combat    == null) Debug.LogError($"{name}: E_Combat in E_Movement missing.");
    }

    void Update()
    {
        Chase();
        C_Anim.UpdateAnimDirections(animator, animator.GetBool("isAttacking"), moveAxis, lastMove, MIN_DISTANCE);
    }

    void FixedUpdate()
    {
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
        // Setup target if player comes close
        var hit = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius, playerLayer);
        target = hit ? hit.transform : null;

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
        bool attacking = animator.GetBool("isAttacking");
        // Block velocity when disabled, holding, or attacking
        bool valveClosed = disabled || holdInRange || (attacking && e_Combat.lockDuringAttack);

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
            animator.SetBool("isMoving", false);
            animator.SetBool("isIdle", false);
        }
    }

    public void ReceiveKnockback(Vector2 force) { knockback += force; }

    // Combat asks to idle-in-place during cooldown
    public void SetHoldInRange(bool v) => holdInRange = v;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.55f, 0f, 0.9f); // orange detection ring
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
