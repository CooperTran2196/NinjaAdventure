using UnityEngine;

[DisallowMultipleComponent]
public class C_State : MonoBehaviour
{
    public enum ActorState { Idle, Move, Attack, Dodge }

    [Header("References")]
    Animator animator;
    Rigidbody2D rb;

    public P_Movement p_Movement;
    public P_Combat   p_Combat;
    public E_Movement e_Movement;
    public E_Combat   e_Combat;
    public C_Dodge    c_Dodge;
    C_Health   c_Health;

    [Header("Locks")]
    public bool lockMove  = true;
    public bool lockDodge = true;

    // Current finite state
    public ActorState CurrentState = ActorState.Idle;

    const float MIN_DISTANCE = 0.0001f;

    void Awake()
    {
        rb          ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponent<Animator>();

        p_Movement  ??= GetComponent<P_Movement>();
        p_Combat    ??= GetComponent<P_Combat>();
        e_Movement  ??= GetComponent<E_Movement>();
        e_Combat    ??= GetComponent<E_Combat>();
        c_Dodge     ??= GetComponent<C_Dodge>();
        c_Health    ??= GetComponent<C_Health>();

        if (!rb)                        Debug.LogError($"{name}: Rigidbody2D in C_State missing.");
        if (!animator)                  Debug.LogError($"{name}: Animator in C_State missing.");
        if (!p_Movement && !e_Movement) Debug.LogError($"{name}: *_Movement in C_State missing.");
        if (!p_Combat   && !e_Combat)   Debug.LogError($"{name}: *_Combat in C_State missing.");
    }

    void OnEnable()
    {
        c_Health.OnDied += OnDiedHandler;
    }

    void OnDisable()
    {
        c_Health.OnDied -= OnDiedHandler;
    }
    void OnDiedHandler()
    {
        // lock movement
        p_Movement?.SetDisabled(true);
        e_Movement?.SetDisabled(true);

        // play death animation
        animator?.SetTrigger("Die");
    }

    void Update()
    {
        // Pick next state
        var next = PickState();
        if (next != CurrentState) CurrentState = next;
        ApplyAnimator(CurrentState);
    }

    // Public API for Enum
    public bool Is(ActorState s) => CurrentState == s;
    public bool IsAttackingNow     => CurrentState == ActorState.Attack;
    public bool IsDodgingNow       => CurrentState == ActorState.Dodge;
    public bool IsMovingNow        => CurrentState == ActorState.Move;

    //  Public API for Bool
    public bool CheckIsBusy()
    {
        // busy if dodging, or attacking and movement is locked
        if (Is(ActorState.Dodge)) return true;
        if (Is(ActorState.Attack) && lockMove) return true;
        return false;
    }
    
    // Pick the correct state
    ActorState PickState()
    {
        // Dodge (optional component)
        if (c_Dodge && c_Dodge.IsDodging) return ActorState.Dodge;

        // Attack
        if ((p_Combat && p_Combat.isAttacking) || (e_Combat && e_Combat.isAttacking))
            return ActorState.Attack;

        // Movement inferred from body velocity (respects ForcedVelocity/knockback)
        if (rb.linearVelocity.sqrMagnitude > MIN_DISTANCE) return ActorState.Move;

        return ActorState.Idle;
    }

    void ApplyAnimator(ActorState s)
    {
        // Single source of truth for animator bools
        animator.SetBool("isDodging",   s == ActorState.Dodge);
        animator.SetBool("isAttacking", s == ActorState.Attack);
        animator.SetBool("isMoving",    s == ActorState.Move);
    }

    // Public API for Floats
    public void UpdateAnimDirections(Vector2 moveAxis, Vector2 lastMove)
    {
        bool busy = CheckIsBusy();
        if (!busy && moveAxis.sqrMagnitude > MIN_DISTANCE)
        {
            animator.SetFloat("moveX", moveAxis.x);
            animator.SetFloat("moveY", moveAxis.y);
        }
        animator.SetFloat("idleX", lastMove.x);
        animator.SetFloat("idleY", lastMove.y);
    }

    // Public API for Attack direction
    public void SetAttackDirection(Vector2 dir)
    {
        animator.SetFloat("atkX", dir.x);
        animator.SetFloat("atkY", dir.y);
    }

    // Get current attack/idle direction from animator
    public Vector2 GetAttackDirection()
    {
        return new Vector2(animator.GetFloat("atkX"), animator.GetFloat("atkY"));
    }
    public Vector2 GetIdleDirection()
    {
        return new Vector2(animator.GetFloat("idleX"), animator.GetFloat("idleY"));
    }
}
