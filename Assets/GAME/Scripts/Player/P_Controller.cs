using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class P_Controller : MonoBehaviour, I_Controller
{
    public enum PState { Idle, Move, Attack, Dodge }

    [Header("States")]
    public PState defaultState = PState.Idle;
    public State_Idle           idle;
    public P_State_Movement    move;
    public P_State_Attack  attack;
    public State_Dodge          dodge;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public C_Stats c_Stats;
    public C_Health c_Health;

    [Header("Debug")]
    public bool autoKill;

    // runtime
    P_InputActions input;
    Vector2 desiredVelocity;
    Vector2 knockback;
    Vector2 moveAxis;
    Vector2 aimDir = Vector2.down;
    Vector2 lastMove = Vector2.down;
    PState current;

    const float MIN = 0.000001f;

    void Awake()
    {
        rb       ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponent<Animator>();
        c_Stats  ??= GetComponent<C_Stats>();
        c_Health ??= GetComponent<C_Health>();
        idle     ??= GetComponent<State_Idle>();
        move     ??= GetComponent<P_State_Movement>();
        attack   ??= GetComponent<P_State_Attack>();
        dodge    ??= GetComponent<State_Dodge>();

        input ??= new P_InputActions();

        if (!rb) Debug.LogWarning($"{name}: Rigidbody2D missing on P_Controller");
        if (!animator) Debug.LogWarning($"{name}: Animator missing on P_Controller");
        if (!c_Stats) Debug.LogWarning($"{name}: C_Stats missing on P_Controller");
        if (!c_Health) Debug.LogWarning($"{name}: C_Health missing on P_Controller");

        animator?.SetFloat("moveX", 0f);
        animator?.SetFloat("moveY", -1f);
        animator?.SetFloat("idleX", 0f);
        animator?.SetFloat("idleY", -1f);
    }

    void OnEnable()
    {
        input.Enable();
        c_Health.OnDied += HandleDied;
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        c_Health.OnDied -= HandleDied;
        input.Disable();
    }

    void HandleDied()
    {
        animator?.SetTrigger("Die");
        move?.SetDisabled(true);
        attack?.ForceStop();
        desiredVelocity = Vector2.zero;
    }

    void Update()
    {
        if (autoKill) { autoKill = false; c_Health.ChangeHealth(-c_Stats.maxHP); }

        // input
        moveAxis = input.Player.Move.ReadValue<Vector2>();
        if (moveAxis.sqrMagnitude > 1f) moveAxis.Normalize();
        move?.SetMoveAxis(moveAxis);
        if (moveAxis.sqrMagnitude > MIN) lastMove = moveAxis;

        aimDir = ReadMouseAim();
        if (aimDir.sqrMagnitude <= MIN) aimDir = lastMove == Vector2.zero ? Vector2.down : lastMove;

        if (input.Player.MeleeAttack.triggered)  attack?.RequestAttack(aimDir, attack.meleeWeapon);
        if (input.Player.RangedAttack.triggered) attack?.RequestAttack(aimDir, attack.rangedWeapon);
        if (input.Player.Dodge.triggered)        dodge?.RequestDodge(lastMove);

        // precedence
        if (c_Stats.currentHP <= 0) { SwitchState(PState.Idle); return; } // death anim via event
        else if (dodge && dodge.IsDodging) SwitchState(PState.Dodge);
        else if (attack && attack.IsAttacking) SwitchState(PState.Attack);
        else if (move && move.HasMoveInput) SwitchState(PState.Move);
        else SwitchState(PState.Idle);
    }

    Vector2 ReadMouseAim()
    {
        Vector2 m = Mouse.current.position.ReadValue();
        var cam = Camera.main;
        if (!cam) return Vector2.zero;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -cam.transform.position.z));
        Vector2 dir = ((Vector2)world - (Vector2)transform.position);
        return dir.sqrMagnitude > MIN ? dir.normalized : Vector2.zero;
    }

    void FixedUpdate()
    {
        Vector2 forced = (dodge != null) ? dodge.ForcedVelocity : Vector2.zero;
        Vector2 baseVel = forced != Vector2.zero ? forced : desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        if (knockback.sqrMagnitude > 0f)
        {
            float step = c_Stats.KR * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
        }
    }

    void SwitchState(PState s)
    {
        if (current == s) return;
        current = s;

        if (idle)   idle.enabled = (s == PState.Idle);
        if (move)   move.SetDisabled(s != PState.Move);
        if (attack) attack.SetDisabled(s != PState.Attack);
        // dodge runs autonomously during its window
    }

    // I_Controller
    public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;
    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;
}
