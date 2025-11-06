using UnityEngine;

public class GRS_State_Chase : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D  rb;
    Animator     anim;
    C_Stats      c_Stats;
    I_Controller controller;

    [Header("Tuning")]
                    public float stopBuffer = 0.10f;
                    public float yAlignBand = 0.35f;   // Shrink |dy| toward this during chase

    // Runtime state
    Transform target;
    Vector2   velocity;
    Vector2   lastMove = Vector2.down;
    float     attackRange;
    float     specialReach;

    void Awake()
    {
        rb         ??= GetComponent<Rigidbody2D>();
        anim       ??= GetComponentInChildren<Animator>();
        c_Stats    ??= GetComponent<C_Stats>();
        controller ??= GetComponent<I_Controller>();

        if (!c_Stats)           Debug.LogError($"{name}: C_Stats missing in GRS_State_Chase", this);
        if (!anim)              Debug.LogError($"{name}: Animator missing in GRS_State_Chase", this);
        if (controller == null) Debug.LogError($"{name}: I_Controller missing in GRS_State_Chase", this);
    }

    void OnEnable()
    {
        anim?.SetBool("isMoving", false);
    }

    void OnDisable()
    {
        velocity = Vector2.zero;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
        anim?.SetBool("isMoving", false);
    }

    void Update()
    {
        if (!target)
        {
            velocity = Vector2.zero;
            controller?.SetDesiredVelocity(Vector2.zero);
            UpdateFloats(Vector2.zero);
            anim?.SetBool("isMoving", false);
            return;
        }

        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
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
        velocity = (distance > (attackRange + stopBuffer)) ? desired * c_Stats.MS : Vector2.zero;
        bool moving = velocity.sqrMagnitude > 0f;
        anim?.SetBool("isMoving", moving);

        controller?.SetDesiredVelocity(velocity);
        UpdateFloats(velocity);
    }

    // CONTROLLER HOOKS

    public void SetTarget(Transform t) => target = t;

    public void SetRanges(float attackRange)
    {
        this.attackRange = attackRange;
        specialReach = attackRange + 2.9f;
    }

    void UpdateFloats(Vector2 move)
    {
        if (move.sqrMagnitude > 0f) lastMove = move.normalized;
        anim?.SetFloat("moveX", move.x);
        anim?.SetFloat("moveY", move.y);
        anim?.SetFloat("idleX", lastMove.x);
        anim?.SetFloat("idleY", lastMove.y);
    }
}
