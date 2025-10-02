using UnityEngine;

public class State_Chase_Boss : MonoBehaviour
{
    [Header("Tuning")]
    public float stopBuffer = 0.10f;
    public float yAlignBand = 0.35f;   // shrink |dy| toward this during chase

    float attackRange = 1.6f;
    float specialReach;                // ≈ attackRange + dashMaxDistance

    // Cache
    Rigidbody2D rb;
    Animator anim;
    C_Stats stats;
    I_Controller controller;           // <- use interface so it works with B_Controller / NPC / Enemy

    // Runtime
    Transform target;
    Vector2 velocity, lastMove = Vector2.down;

    void Awake()
    {
        rb         ??= GetComponent<Rigidbody2D>();
        anim       ??= GetComponentInChildren<Animator>();
        stats      ??= GetComponent<C_Stats>();
        controller ??= GetComponent<I_Controller>();

        if (!stats) Debug.LogError($"{name}: C_Stats missing in State_Chase_Boss");
        if (!anim)  Debug.LogError($"{name}: Animator missing in State_Chase_Boss");
        if (controller == null) Debug.LogError($"{name}: I_Controller missing on parent for State_Chase_Boss");
    }

    void OnEnable()  { anim.SetBool("isMoving", false); }
    void OnDisable()
    {
        velocity = Vector2.zero;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);
    }

    void Update()
    {
        if (!target)
        {
            velocity = Vector2.zero;
            controller?.SetDesiredVelocity(Vector2.zero);
            UpdateFloats(Vector2.zero);
            anim.SetBool("isMoving", false);
            return;
        }

        Vector2 toTarget  = (Vector2)target.position - (Vector2)transform.position;
        float dx = toTarget.x;
        float dy = toTarget.y;
        float distance = toTarget.magnitude;

        // Horizontal-first chase with vertical alignment bias
        Vector2 desired = Vector2.zero;
        if (Mathf.Abs(dy) > yAlignBand)
        {
            desired.y = Mathf.Sign(dy);
            desired.x = Mathf.Sign(dx) * 0.6f;
        }
        else
        {
            desired.x = Mathf.Sign(dx);
            desired.y = Mathf.Sign(dy) * 0.35f;
        }
        desired = desired.sqrMagnitude > 0f ? desired.normalized : lastMove;

        // Move if outside attack range + buffer
        velocity = (distance > (attackRange + stopBuffer)) ? desired * stats.MS : Vector2.zero;
        bool moving = velocity.sqrMagnitude > 0f;
        anim.SetBool("isMoving", moving);

        controller?.SetDesiredVelocity(velocity);
        UpdateFloats(velocity);
    }

    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float attackRange)
    {
        this.attackRange = attackRange;
        specialReach = attackRange + 2.9f; // default dashMaxDistance (boss attack state clamps exactly)
    }

    void UpdateFloats(Vector2 move)
    {
        if (move.sqrMagnitude > 0f) lastMove = move.normalized;
        anim.SetFloat("moveX", move.x);
        anim.SetFloat("moveY", move.y);
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);
    }
}
