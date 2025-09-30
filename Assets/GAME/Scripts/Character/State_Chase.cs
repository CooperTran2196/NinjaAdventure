using UnityEngine;

public class State_Chase : MonoBehaviour
{
    [Header("Tuning")]
    public float stopBuffer = 0.10f;

    float attackRange = 1.2f;

    // Cache
    Rigidbody2D rb;
    Animator anim;
    C_Stats stats;
    E_Controller controller;

    // Runtime
    Transform target;
    Vector2 velocity, lastMove = Vector2.down;

    void Awake()
    {
        rb         = GetComponent<Rigidbody2D>();
        anim       = GetComponent<Animator>();
        stats      = GetComponent<C_Stats>();
        controller = GetComponent<E_Controller>();

        if (!stats) Debug.LogError($"{name}: C_Stats is missing in State_Chase");
    }

    void OnEnable() { anim.SetBool("isMoving", false); }

    void OnDisable()
    {
        velocity = Vector2.zero;
        controller?.SetDesiredVelocity(Vector2.zero);
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);
    }

    void Update()
    {
        // No movement while stunned/dead; controller still applies knockback globaly
        if (!target)
        {
            velocity = Vector2.zero;
            controller?.SetDesiredVelocity(Vector2.zero);
            UpdateFloats(Vector2.zero);
            anim.SetBool("isMoving", false);
            return;
        }

        // Chase target
        Vector2 toTarget  = (Vector2)target.position - (Vector2)transform.position;
        float   distance  = toTarget.magnitude;
        Vector2 direction = distance > 0.0001f ? toTarget.normalized : lastMove;

        // Move if outside attack range + buffer
        velocity = (distance > (attackRange + stopBuffer)) ? direction * stats.MS : Vector2.zero;
        bool moving = velocity.sqrMagnitude > 0f;
        anim.SetBool("isMoving", moving);

        controller.SetDesiredVelocity(velocity);
        UpdateFloats(velocity);
    }

    // Set target + attack range
    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float attackRange) => this.attackRange = attackRange;

    // Update animator floats
    void UpdateFloats(Vector2 move)
    {
        if (move.sqrMagnitude > 0f) lastMove = move.normalized;
        anim.SetFloat("moveX", move.x);
        anim.SetFloat("moveY", move.y);
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);
    }
}
