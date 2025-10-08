using UnityEngine;

public class State_Chase : MonoBehaviour
{
    [Header("Tuning")]
    [Min(0f)] public float stopBuffer = 0.10f; // used by controller’s axis calc

    // cache
    Animator     anim;
    C_Stats      c_Stats;
    E_Controller controller;

    // runtime
    // Vector2 moveAxis; // Will be calculated in Update
    Vector2 lastMove = Vector2.down;

    void Awake()
    {
        anim        = GetComponent<Animator>();
        c_Stats     = GetComponent<C_Stats>();
        controller  = GetComponent<E_Controller>();
    }

    void OnEnable() {}

    void OnDisable()
    {
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetBool("isMoving", false);
    }

    void Update()
    {
        // Calculate chase direction
        Vector2 moveAxis = ComputeChaseDir();

        // Calculate and apply movement velocity
        controller.SetDesiredVelocity(moveAxis * c_Stats.MS);

        // Set movement animation
        anim.SetFloat("moveX", moveAxis.x);
        anim.SetFloat("moveY", moveAxis.y);

        // Continue to face the last direction it was moving in
        if (moveAxis.sqrMagnitude > 0f) lastMove = moveAxis;
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);

        // Optional: derive isMoving from axis (keeps old visuals)
        bool moving = moveAxis.sqrMagnitude > 0f;
        anim.SetBool("isMoving", moving);
    }

    Vector2 ComputeChaseDir()
    {
        Transform target = controller.GetTarget();
        if (!target) return Vector2.zero;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float dist = to.magnitude;
        if (dist <= 0.000001f) return Vector2.zero;

        float stop = controller.GetAttackRange() + stopBuffer;
        return (dist > stop) ? (to / dist) : Vector2.zero; // normalized or zero if within stop band
    }
}
