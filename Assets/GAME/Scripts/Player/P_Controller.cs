using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class P_Controller : MonoBehaviour, I_Controller
{
    public enum PState { Idle, Move, Attack, Dodge, Dead }

    [Header("Main controller for player input states")]
    [Header("References")]
    public PState defaultState = PState.Idle;

    [Header("Weapons")]
    public W_Base meleeWeapon;
    public W_Base rangedWeapon;

    [Header("Attack Settings")]
    public float attackDuration = 0.45f;
    public float hitDelay = 0.15f;

    [Header("Debug")]
    public bool autoKill;

    Rigidbody2D rb;
    Animator anim;
    P_InputActions input;

    PState currentState;
    State_Idle idle;
    P_State_Movement move;
    P_State_Attack attack;
    State_Dodge dodge;
    C_Stats c_Stats;
    C_Health c_Health;

    // Runtime vars - grouped by type
    Vector2 desiredVelocity, knockback, moveAxis, attackDir = Vector2.down, lastMove = Vector2.down;
    bool isStunned, isDead, isAttacking, isDodging;
    float stunUntil, attackCooldown;
    W_Base currentWeapon;

    const float MIN_DISTANCE = 0.000001f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        c_Stats = GetComponent<C_Stats>();
        c_Health = GetComponent<C_Health>();

        idle = GetComponent<State_Idle>();
        move = GetComponent<P_State_Movement>();
        attack = GetComponent<P_State_Attack>();
        dodge = GetComponent<State_Dodge>();

        input ??= new P_InputActions();
        ValidateComponents();
        anim?.SetFloat("moveX", 0f);
        anim?.SetFloat("moveY", -1f);
        anim?.SetFloat("idleX", 0f);
        anim?.SetFloat("idleY", -1f);
    }

    void ValidateComponents()
    {
        var missing = new System.Collections.Generic.List<string>();
        if (!rb) missing.Add("Rigidbody2D");
        if (!anim) missing.Add("Animator");
        if (!c_Stats) missing.Add("C_Stats");
        if (!c_Health) missing.Add("C_Health");
        if (!idle) missing.Add("State_Idle");
        if (!move) missing.Add("P_State_Movement");
        if (!attack) missing.Add("P_State_Attack");
        if (!dodge) missing.Add("State_Dodge");
        if (missing.Count > 0)
            Debug.LogError($"P_Controller: Missing components: {string.Join(", ", missing)}");
    }

    void OnEnable()
    {
        input.Enable();
        c_Health.OnDied += () => SwitchState(PState.Dead);
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        input.Disable();
        c_Health.OnDied -= () => SwitchState(PState.Dead);
        idle.enabled = move.enabled = attack.enabled = dodge.enabled = false;
    }

    void Update()
    {
        if (isDead || isStunned) return;
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;

        // Debug kill
        if (autoKill) { autoKill = false; c_Health.ChangeHealth(-c_Stats.maxHP); }

        // PROCESS INPUTS
        ProcessInputs();
    }

    void FixedUpdate()
    {
        if (isDead || isStunned) return;

        // Apply this frame: block state intent when stunned/dead, but still allow knockback
        Vector2 baseVel = desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        // Decay knockback for the NEXT frame (always decay - MoveTowards handles zero gracefully)
        knockback = Vector2.MoveTowards(knockback, Vector2.zero, c_Stats.KR * Time.fixedDeltaTime);
    }

    // I_Controller
    public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;
    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;
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
        // Movement input
        moveAxis = input.Player.Move.ReadValue<Vector2>();
        if (moveAxis.sqrMagnitude > 1f) moveAxis.Normalize();
        if (moveAxis.sqrMagnitude > MIN_DISTANCE) lastMove = moveAxis;

        // Aim input
        Vector2 mouseAim = ReadMouseAim();
        if (mouseAim != Vector2.zero) attackDir = mouseAim;

        // Attack inputs
        if (attackCooldown <= 0f)
        {
            if (input.Player.MeleeAttack.triggered)
            {
                isAttacking = true;
                currentWeapon = meleeWeapon;
            }
            if (input.Player.RangedAttack.triggered)
            {
                isAttacking = true;
                currentWeapon = rangedWeapon;
            }
        }

        // Dodge input
        if (input.Player.Dodge.triggered)
        {
            isDodging = true;
        }

        // Determine and switch to desired state
        PState desiredState = c_Stats.currentHP <= 0 ? PState.Dead :
                             isDodging ? PState.Dodge :
                             isAttacking ? PState.Attack :
                             (moveAxis.sqrMagnitude > MIN_DISTANCE) ? PState.Move :
                             PState.Idle;

        // Don't interrupt ongoing attack unless it's death
        if (currentState == PState.Attack && isAttacking && desiredState != PState.Dead)
        {
            return;
        }

        if (desiredState != currentState)
        {
            SwitchState(desiredState);
        }
    }



    // Switch states with integrated death handling and attack logic
    public void SwitchState(PState s)
    {
        if (currentState == s) return;
        currentState = s;

        idle.enabled = (s == PState.Idle);
        move.SetDisabled(s != PState.Move);
        attack.enabled = (s == PState.Attack);
        dodge.enabled = (s == PState.Dodge);

        // Handle Dead state (integrated C_Dead functionality)
        if (s == PState.Dead)
        {
            isDead = true;
            knockback = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            anim.SetTrigger("Die");
        }

        // On enter, provide context to newly enabled state
        if (s == PState.Move) move.SetMoveAxis(moveAxis);

        // Handle attack state entry (integrated RequestAttack logic)
        if (s == PState.Attack && currentWeapon != null)
        {
            attackCooldown = c_Stats.attackCooldown;
            attack.RequestAttack(attackDir, currentWeapon);
            currentWeapon = null; // Clear after use
        }

        // Handle dodge state entry (integrated RequestDodge logic)
        if (s == PState.Dodge)
        {
            dodge.RequestDodge(lastMove);
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
