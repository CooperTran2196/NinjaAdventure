using UnityEngine;

public class C_State : MonoBehaviour
{
    public enum ActorState { Idle, Move, Attack, Dodge }

    [Header("References")]
    Animator animator;
    Rigidbody2D rb;

    public P_Movement p_Movement;
    public P_Combat   p_Combat;
    public C_Dodge    c_Dodge;

    // Holds the current state directly
    public ActorState CurrentState = ActorState.Idle;

    const float MIN_DISTANCE = 0.0001f;

    void Awake()
    {
        animator   ??= GetComponent<Animator>();
        rb         ??= GetComponent<Rigidbody2D>();
        p_Movement ??= GetComponent<P_Movement>();
        p_Combat   ??= GetComponent<P_Combat>();
        c_Dodge    ??= GetComponent<C_Dodge>();

        if (!animator) Debug.LogError($"{name}: Animator missing.");
        if (!rb)       Debug.LogError($"{name}: Rigidbody2D missing.");
    }

    void Update()
    {
        var next = PickState();
        if (next != CurrentState)
        {
            CurrentState = next;
        }

        ApplyAnimator(CurrentState);
    }

    // Function that checks if busy
    public bool CheckIsBusy()
    {
        if (CurrentState == ActorState.Dodge)
            return true;

        if (CurrentState == ActorState.Attack && p_Combat != null && p_Combat.lockDuringAttack)
            return true;

        return false;
    }


    ActorState PickState()
    {
        if (c_Dodge.IsDodging == true) return ActorState.Dodge;
        if (p_Combat.IsAttacking == true) return ActorState.Attack;

        // Use current body velocity to infer movement (works with ForcedVelocity + knockback)
        if (rb.linearVelocity.sqrMagnitude > MIN_DISTANCE) return ActorState.Move;

        return ActorState.Idle;
    }

    void ApplyAnimator(ActorState s)
    {
        // Single source of truth for these 3 bools
        animator.SetBool("isDodging",   s == ActorState.Dodge);
        animator.SetBool("isAttacking", s == ActorState.Attack);
        animator.SetBool("isMoving",    s == ActorState.Move);
    }
}
