using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Movement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public C_Stats c_Stats;

    I_Controller controller;

    public bool HasMoveInput { get; private set; }
    public Vector2 lastMove = Vector2.down;

    Vector2 moveAxis;

    void Awake()
    {
        animator   ??= GetComponent<Animator>();
        c_Stats    ??= GetComponent<C_Stats>();
        controller ??= GetComponent<I_Controller>();

        if (!animator) Debug.LogWarning($"{name}: Animator missing on State_Move_Player");
        if (!c_Stats) Debug.LogWarning($"{name}: C_Stats missing on State_Move_Player");
        if (controller == null) Debug.LogWarning($"{name}: I_Controller missing on State_Move_Player");
    }

    public void SetDisabled(bool v)
    {
        enabled = !v;
        if (v)
        {
            HasMoveInput = false;
            moveAxis = Vector2.zero;
            controller?.SetDesiredVelocity(Vector2.zero);
        }
    }

    public void SetMoveAxis(Vector2 axis)
    {
        moveAxis = axis;
        HasMoveInput = axis.sqrMagnitude > 0f;
        if (HasMoveInput) lastMove = axis.normalized;
    }

    void Update()
    {
        Vector2 vel = moveAxis * c_Stats.MS;
        controller?.SetDesiredVelocity(vel);

        if (HasMoveInput)
        {
            animator?.SetFloat("moveX", moveAxis.x);
            animator?.SetFloat("moveY", moveAxis.y);
        }

        var dir = HasMoveInput ? moveAxis : lastMove;
        animator?.SetFloat("idleX", dir.x);
        animator?.SetFloat("idleY", dir.y);
    }
}
