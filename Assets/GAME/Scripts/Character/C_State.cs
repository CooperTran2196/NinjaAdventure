using UnityEngine;

public class C_State : MonoBehaviour
{
    public enum ActorState { Idle, Move, Attack, Dodge }

    // References
    Animator animator;
    Rigidbody2D rb;
    public P_Movement p_Movement;
    public P_Combat   p_combat;
    public C_Dodge    c_dodge;

    // State
    public ActorState Current { get; private set; } = ActorState.Idle;
    public bool IsBusy => Current == ActorState.Dodge || (Current == ActorState.Attack && (p_combat?.lockDuringAttack ?? false));

    const float MIN_DISTANCE = 0.0001f; // same epsilon as elsewhere

    void Awake()
    {
        animator   ??= GetComponent<Animator>();
        rb         ??= GetComponent<Rigidbody2D>();
        p_Movement ??= GetComponent<P_Movement>();
        p_combat   ??= GetComponent<P_Combat>();
        c_dodge    ??= GetComponent<C_Dodge>();

        if (!animator) Debug.LogError($"{name}: Animator missing.");
        if (!rb)       Debug.LogError($"{name}: Rigidbody2D missing.");
    }

    void Update()
    {
        var next = PickState();
        if (next != Current)
        {
            Current = next;
            // (Optional) fire an event later if you need VFX/audio
        }
        ApplyAnimator(Current);
    }

    ActorState PickState()
    {
        if (c_dodge?.IsDodging == true) return ActorState.Dodge;
        if (p_combat?.IsAttacking == true) return ActorState.Attack;

        // Use current body velocity to infer movement (works with ForcedVelocity + knockback)
        if (rb.linearVelocity.sqrMagnitude > MIN_DISTANCE) return ActorState.Move;

        return ActorState.Idle;
    }

    void ApplyAnimator(ActorState s)
    {
        // Single source of truth for these 3 bools
        animator?.SetBool("isDodging",   s == ActorState.Dodge);
        animator?.SetBool("isAttacking", s == ActorState.Attack);
        animator?.SetBool("isMoving",    s == ActorState.Move);
    }
}
