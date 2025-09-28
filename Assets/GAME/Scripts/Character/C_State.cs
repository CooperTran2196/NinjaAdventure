using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]

[DisallowMultipleComponent]

public class C_State : MonoBehaviour
{   
    // Finite State Machine
    public enum ActorState { Idle, Move, Attack, Dodge, Wander, Dead }

    [Header("References(Only Player/Enemy/NPC)")]
    Animator animator;
    Rigidbody2D rb;

    public P_Movement p_Movement;
    public P_Combat   p_Combat;
    public E_Movement e_Movement;
    public E_Combat   e_Combat;

    public C_Dodge    c_Dodge;
    C_Health   c_Health;

    [Header("Locks Certain Actions")]
    public bool lockMoveWhileAttacking = true;
    public bool lockDodge = true;

    [Header("Wandering Ability")]
    public bool canWander;

    // Current finite state
    public ActorState CurrentState = ActorState.Idle;

    const float MIN_DISTANCE = 0.0001f;

    void Awake()
    {
        rb          ??= GetComponent<Rigidbody2D>();
        animator    ??= GetComponent<Animator>();

        p_Movement  ??= GetComponent<P_Movement>();
        p_Combat    ??= GetComponent<P_Combat>();
        e_Movement  ??= GetComponent<E_Movement>();
        e_Combat    ??= GetComponent<E_Combat>();

        c_Dodge     ??= GetComponent<C_Dodge>();
        c_Health    ??= GetComponent<C_Health>();

        if (!rb)                                     Debug.LogError($"{name}: Rigidbody2D in C_State missing.");
        if (!animator)                               Debug.LogError($"{name}: Animator in C_State missing.");
        if (!p_Movement && !e_Movement) Debug.LogError($"{name}: *_Movement/C_Wander is missing.");
        if (!p_Combat && !e_Combat)     Debug.LogError($"{name}: *_Combat/C_Wander is missing.");
    }

    void OnEnable()
    {
        c_Health.OnDied += OnDiedHandler;
    }

    void OnDisable()
    {
        c_Health.OnDied -= OnDiedHandler;
    }

    // Handle death
    void OnDiedHandler()
    {
        // lock movement
        p_Movement?.SetDisabled(true);
        e_Movement?.SetDisabled(true);

        // play death animation
        animator.SetTrigger("Die");

        // stop wandering and freeze state at Dead (not for player)

        rb.linearVelocity = Vector2.zero;
        CurrentState = ActorState.Dead;
    }

    // Update is called once per frame
    void Update()
    {
        // Pick next state
        var next = PickState();
        if (next != CurrentState) CurrentState = next;
        ApplyAnimator(CurrentState);
    }

    // Public API for Enum
    public bool Is(ActorState s)   => CurrentState == s;

    //  Public API for Bool
    public bool CheckIsBusy()
    {
        // busy if dodging, or attacking and movement is locked
        if (Is(ActorState.Dodge)) return true;
        if (Is(ActorState.Attack) && lockMoveWhileAttacking) return true;
        return false;
    }
    
    // Pick the correct state
    ActorState PickState()
    {
        // If dead, remain dead (no further transitions)
        if (CurrentState == ActorState.Dead || !c_Health.IsAlive)
            return ActorState.Dead;

        // Dodge is optional
        if (c_Dodge && c_Dodge.IsDodging) return ActorState.Dodge;

        // Attack
        if ((p_Combat && p_Combat.isAttacking) || (e_Combat && e_Combat.isAttacking))
            return ActorState.Attack;

        // Wander (must have wander component active & allowed) takes precedence over plain Move


        // Movement inferred from body velocity (respects ForcedVelocity/knockback) for non-wander movement
        if (rb.linearVelocity.sqrMagnitude > MIN_DISTANCE) return ActorState.Move;

        return ActorState.Idle;
    }

    // Single source of truth for animator bools
    void ApplyAnimator(ActorState s)
    {
        animator.SetBool("isDodging", s == ActorState.Dodge);
        animator.SetBool("isAttacking", s == ActorState.Attack);
        animator.SetBool("isMoving", s == ActorState.Move);
        animator.SetBool("isWandering", s == ActorState.Wander);
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
