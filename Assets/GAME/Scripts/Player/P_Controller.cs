using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(P_State_Idle))]
[RequireComponent(typeof(P_State_Movement))]
[RequireComponent(typeof(P_State_Attack))]
[RequireComponent(typeof(P_State_Dodge))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]

public class P_Controller : MonoBehaviour
{
    public enum PState { Idle, Move, Attack, Dodge, Dead }

    [Header("Main controller for player input states")]
    [Header("References")]
    public PState defaultState = PState.Idle;
    public PState currentState;

    [Header("Weapons")]
    public W_Base meleeWeapon;
    public W_Base rangedWeapon;

    [Header("Debug")]
    public bool autoKill;

    Rigidbody2D     rb;
    Animator        anim;
    P_InputActions  input;

    P_State_Idle        idle;
    P_State_Movement    move;
    P_State_Attack      attack;
    P_State_Dodge       dodge;
    C_Stats             c_Stats;
    C_Health            c_Health;

    // Runtime vars - grouped by type
    Vector2 desiredVelocity, knockback, moveAxis, attackDir = Vector2.down, lastMove = Vector2.down;
    bool    isDead, isStunned, isAttacking, isDodging;
    float   stunUntil, attackCooldown, dodgeCooldown;
    W_Base currentWeapon;

    const float MIN_DISTANCE = 0.000001f;

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        anim     = GetComponent<Animator>();
        c_Stats  = GetComponent<C_Stats>();
        c_Health = GetComponent<C_Health>();

        idle     = GetComponent<P_State_Idle>();
        move     = GetComponent<P_State_Movement>();
        attack   = GetComponent<P_State_Attack>();
        dodge    = GetComponent<P_State_Dodge>();  // Changed from State_Dodge to P_State_Dodge

        input ??= new P_InputActions();

        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", -1f);
        anim.SetFloat("idleX", 0f);
        anim.SetFloat("idleY", -1f);
    }

    void OnEnable()
    {
        input.Enable();
        c_Health.OnDied += OnDiedHandler;
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        input.Disable();
        c_Health.OnDied -= OnDiedHandler;
        idle.enabled = move.enabled = attack.enabled = dodge.enabled = false;
    }

    void OnDiedHandler() => SwitchState(PState.Dead);

    void Update()
    {
        if (isDead || isStunned) return;
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;
        if (dodgeCooldown > 0f) dodgeCooldown -= Time.deltaTime;

        // Debug kill
        if (autoKill) { autoKill = false; c_Health.ChangeHealth(-c_Stats.maxHP); }

        // PROCESS INPUTS
        ProcessInputs();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Apply this frame: block state intent when stunned/dead, but still allow knockback
        Vector2 baseVel = isStunned ? Vector2.zero : desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        // Decay knockback for the NEXT frame (always decay - MoveTowards handles zero gracefully)
        knockback = Vector2.MoveTowards(knockback, Vector2.zero, c_Stats.KR * Time.fixedDeltaTime);
    }

    // I_Controller
    public void SetDesiredVelocity(Vector2 desiredVelocity) => this.desiredVelocity = desiredVelocity;
    public void ReceiveKnockback(Vector2 knockback) => this.knockback += knockback;
    // State setters for external components
    public void SetAttacking(bool value) => isAttacking = value;
    public void SetDodging(bool value) => isDodging = value;

    // Convert mouse position to world direction
    Vector2 ReadMouseAim()
    {
        Vector2 m = Mouse.current.position.ReadValue();
        var cam = Camera.main;
        if (!cam) return Vector2.zero;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -cam.transform.position.z));
        Vector2 dir = (Vector2)world - (Vector2)transform.position;
        return dir.sqrMagnitude > MIN_DISTANCE ? dir.normalized : Vector2.zero;
    }

    void ProcessInputs()
    {
        // Handle death first (highest)
        if (c_Stats.currentHP <= 0)
        {
            SwitchState(PState.Dead);
            return;
        }

        // Don't interrupt while atacking or dodging
        if (currentState == PState.Attack && isAttacking) return;
        if (currentState == PState.Dodge && isDodging) return;

        // Handle dodge input (high)
        if (input.Player.Dodge.triggered && dodgeCooldown <= 0f)
        {
            SwitchState(PState.Dodge);
            return;
        }

        // Handle attack inputs (mid)
        if (attackCooldown <= 0f)
        {
            Vector2 mouseAim = ReadMouseAim();
            if (mouseAim != Vector2.zero) attackDir = mouseAim;

            if (input.Player.MeleeAttack.triggered)
            {
                currentWeapon = meleeWeapon;
                SwitchState(PState.Attack);
                return;
            }
            if (input.Player.RangedAttack.triggered)
            {
                currentWeapon = rangedWeapon;
                SwitchState(PState.Attack);
                return;
            }
        }

        // Handle movement input (low priority)
        moveAxis = input.Player.Move.ReadValue<Vector2>();
        if (moveAxis.sqrMagnitude > 1f) moveAxis.Normalize();

        if (moveAxis.sqrMagnitude > MIN_DISTANCE)
        {
            lastMove = moveAxis;

            // Continue moving if already in Move state
            if (currentState == PState.Move)
            {
                // refresh axis without flipping states
                move.SetMoveAxis(moveAxis);
            }
            else // Switch to Move state if not already
            {
                SwitchState(PState.Move);
                move.SetMoveAxis(moveAxis);
            }
            return;
        }

        // Default to idle (lowest priority)
        SwitchState(PState.Idle);
    }

    // Switch states with integrated death handling and attack logic
    public void SwitchState(PState state)
    {
        if (currentState == state) return;
        currentState = state;

        // Disable all states first
        idle.enabled = move.enabled = attack.enabled = dodge.enabled = false;

        switch (state)
        {
            case PState.Dead: // Highest priority
                desiredVelocity     = Vector2.zero;
                knockback           = Vector2.zero;
                rb.linearVelocity   = Vector2.zero;
                isDead              = true;
                isAttacking         = false;
                isStunned           = false;
                isDodging           = false;

                anim.SetTrigger("Die");
                break;

            case PState.Dodge:
                dodge.enabled       = true;
                isDodging           = true;
                dodgeCooldown       = c_Stats.dodgeCooldown;

                dodge.Dodge(lastMove);
                break;

            case PState.Attack:
                desiredVelocity     = Vector2.zero;
                attack.enabled      = true;

                if (currentWeapon != null)
                {
                    attackCooldown  = c_Stats.attackCooldown;
                    isAttacking     = true;
                    attack.Attack(currentWeapon, attackDir);
                    currentWeapon   = null;
                }
                break;

            case PState.Move:
                move.enabled = true;

                move.SetMoveAxis(moveAxis);
                break;

            case PState.Idle: // Lowest priority
                desiredVelocity     = Vector2.zero;
                idle.enabled        = true;

                idle.SetIdleFacing(lastMove);
                break;
        }
    }

    // STUN FEATURE (same as enemy)
    public IEnumerator StunFor(float duration)
    {
        if (duration <= 0f) yield break;

        // Extend the stun end if a longer one arrives
        float newEnd = Time.time + duration;
        if (newEnd > stunUntil) stunUntil = newEnd;

        isStunned = true;
        while (Time.time < stunUntil) yield return null;

        isStunned = false;
    }
}
