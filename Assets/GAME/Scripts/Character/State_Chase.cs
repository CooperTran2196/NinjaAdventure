using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[DisallowMultipleComponent]

public class State_Chase : MonoBehaviour
{
    [Header("Tuning")]
    public float stopBuffer = 0.10f;

    // Ranges are injected by controller
    float attackRange    = 1.2f;

    // Cache
    Rigidbody2D rb;
    Animator anim;
    C_Stats stats;

    // Runtime
    Transform target;
    Vector2 velocity, knockback, lastMove = Vector2.down;

    void Awake()
    {
        rb    ??= GetComponent<Rigidbody2D>();
        anim  ??= GetComponent<Animator>();
        stats ??= GetComponent<C_Stats>();

        if (!stats) Debug.LogError($"{name}: C_Stats missing on State_Chase.");
    }

    void OnEnable()
    {
        anim.SetBool("isMoving", false);
    }

    void OnDisable()
    {
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);
    }

    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float attackRange) => this.attackRange = attackRange;

    void Update()
    {
        // No target = no movement
        if (!target)
        {
            velocity = Vector2.zero;
            UpdateFloats(Vector2.zero);
            anim.SetBool("isMoving", false);
            return;
        }

        Vector2 toTarget  = (Vector2)target.position - (Vector2)transform.position;
        float   distance  = toTarget.magnitude;
        Vector2 direction = distance > 0.0001f ? toTarget.normalized : lastMove;

        // Move toward target while outside inner ring
        velocity = (distance > (attackRange + stopBuffer)) ? direction * stats.MS : Vector2.zero;
        bool moving = velocity.sqrMagnitude > 0f;
        anim.SetBool("isMoving", moving);

        // Update animator direction floats
        UpdateFloats(velocity);
    }

    void FixedUpdate()
    {
        Vector2 final = velocity + knockback;
        rb.linearVelocity = final;

        if (knockback.sqrMagnitude > 0f)
        {
            float step = stats.KR * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
        }
    }

    void UpdateFloats(Vector2 move)
    {
        if (move.sqrMagnitude > 0f) lastMove = move.normalized;
        anim.SetFloat("moveX", move.x);
        anim.SetFloat("moveY", move.y);
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);
    }

    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;
}
