using UnityEngine;

public class GS2_State_Chase : MonoBehaviour
{
    [Header("References")]
    Animator       anim;
    GS2_Controller controller;
    C_Stats        c_Stats;

    [Header("Retreat Settings")]
    public float retreatSpeedMultiplier = 0.7f;

    // Runtime state
    Transform target;
    bool      hasTarget;
    Vector2   moveVector;
    Vector2   lastMove = Vector2.down;

    void Awake()
    {
        anim       = GetComponent<Animator>();
        controller = GetComponent<GS2_Controller>();
        c_Stats    = GetComponent<C_Stats>();
    }

    void OnEnable()
    {
        anim.SetBool("isMoving", true);
        anim.SetBool("isIdle", false);
    }

    void OnDisable()
    {
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetBool("isMoving", false);
        anim.SetBool("isIdle", true);
        UpdateFloats(Vector2.zero);
    }

    void Update()
    {
        if (!hasTarget)
        {
            controller.SetDesiredVelocity(Vector2.zero);
            anim.SetBool("isMoving", false);
            UpdateFloats(Vector2.zero);
            return;
        }

        // Don't move if doing special attack
        if (controller.IsDoingSpecialAtk())
        {
            controller.SetDesiredVelocity(Vector2.zero);
            anim.SetBool("isMoving", false);
            return;
        }

        // PHASE 2: Check retreat behavior
        if (controller.IsRetreating())
        {
            // Retreat: move away from player
            moveVector = ((Vector2)transform.position - (Vector2)target.position).normalized;
            controller.SetDesiredVelocity(moveVector * c_Stats.MS * retreatSpeedMultiplier);
        }
        else if (controller.IsInRetreatCooldown())
        {
            // Cooldown window: stop moving (vulnerable)
            controller.SetDesiredVelocity(Vector2.zero);
            moveVector = Vector2.zero;
        }
        else
        {
            // Normal chase: move toward player
            moveVector = ((Vector2)target.position - (Vector2)transform.position).normalized;
            controller.SetDesiredVelocity(moveVector * c_Stats.MS);
        }

        bool isMoving = moveVector.sqrMagnitude > 0f;
        anim.SetBool("isMoving", isMoving);
        UpdateFloats(moveVector);
    }

    // CONTROLLER HOOKS
    public void SetTarget(Transform t)
    {
        target = t;
        hasTarget = (t != null);
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
