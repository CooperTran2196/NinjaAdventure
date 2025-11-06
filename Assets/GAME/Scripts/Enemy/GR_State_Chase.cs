using UnityEngine;

public class GR_State_Chase : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D  rb;
    Animator     anim;
    C_Stats      stats;
    I_Controller controller;

    [Header("Chase Settings")]
                 public float stopBuffer = 0.10f;

    // Runtime state
    Transform target;
    Vector2   velocity;
    Vector2   lastMove = Vector2.down;
    float     chargeRange = 5f;

    void Awake()
    {
        rb         ??= GetComponent<Rigidbody2D>();
        anim       ??= GetComponentInChildren<Animator>();
        stats      ??= GetComponent<C_Stats>();
        controller ??= GetComponent<I_Controller>();

        if (!stats)           Debug.LogError($"{name}: C_Stats missing in GR_State_Chase");
        if (!anim)            Debug.LogError($"{name}: Animator missing in GR_State_Chase");
        if (controller == null) Debug.LogError($"{name}: I_Controller missing in GR_State_Chase");
    }

    void OnEnable()
    {
        anim?.SetBool("isMoving", true);
        anim?.SetBool("isIdle", false);
    }
    
    void OnDisable()
    {
        velocity = Vector2.zero;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
        anim?.SetBool("isMoving", false);
        anim?.SetBool("isIdle", true);
        UpdateFloats(Vector2.zero);
    }

    void Update()
    {
        if (!target)
        {
            velocity = Vector2.zero;
            controller?.SetDesiredVelocity(Vector2.zero);
            anim?.SetBool("isMoving", false);
            return;
        }

        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        float distance = toTarget.magnitude;

        Vector2 desired = distance > 0.0001f ? toTarget.normalized : lastMove;

        // Move if outside charge range + buffer
        velocity = (distance > (chargeRange + stopBuffer)) ? desired * stats.MS : Vector2.zero;
        bool moving = velocity.sqrMagnitude > 0f;
        anim?.SetBool("isMoving", moving);

        controller?.SetDesiredVelocity(velocity);
        UpdateFloats(velocity);
        
        if (moving) lastMove = desired;
    }

    public void SetTarget(Transform t) => target = t;
    
    public void SetRanges(float chargeRange) => this.chargeRange = chargeRange;

    void UpdateFloats(Vector2 move)
    {
        if (move.sqrMagnitude > 0f) lastMove = move.normalized;
        anim?.SetFloat("moveX", move.x);
        anim?.SetFloat("moveY", move.y);
        anim?.SetFloat("idleX", lastMove.x);
        anim?.SetFloat("idleY", lastMove.y);
    }
}
