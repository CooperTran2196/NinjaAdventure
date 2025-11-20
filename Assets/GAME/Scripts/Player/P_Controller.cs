using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]
[RequireComponent(typeof(P_State_Idle))]
[RequireComponent(typeof(P_State_Movement))]
[RequireComponent(typeof(P_State_Attack))]
[RequireComponent(typeof(P_State_Dodge))]

public class P_Controller : MonoBehaviour
{
    public enum PState { Idle, Move, Attack, Dodge, Dead }

    [Header("Main controller for player input states")]
    [Header("References")]
    Rigidbody2D      rb;
    Animator         anim;
    P_InputActions   input;
    P_State_Idle     idle;
    P_State_Movement move;
    P_State_Attack   attack;
    P_State_Dodge    dodge;
    C_Stats          c_Stats;
    C_Health         c_Health;
    C_FX             c_FX;

    [Header("State")]
    public PState defaultState = PState.Idle;
    public PState currentState;

    [Header("Weapons")]
    public W_Base meleeWeapon;
    public W_Base rangedWeapon;

    // Events for weapon UI updates
    public static event System.Action<W_SO> OnMeleeWeaponChanged;
    public static event System.Action<W_SO> OnRangedWeaponChanged;

    // Runtime state
    Vector2 desiredVelocity, knockback, moveAxis, attackDir = Vector2.down, lastMove = Vector2.down;
    bool    isDead, isStunned, isAttacking, isDodging;
    float   stunUntil, attackCooldown, dodgeCooldown;
    
    // Tutorial death zone tracking
    public bool isInTutorialZone;
    
    // Weapon system
    W_Base currentWeapon;
    W_SO   currentMeleeData;
    W_SO   currentRangedData;
    
    // Ladder system
    ENV_Ladder currentLadder;

    const float MIN_DISTANCE = 0.000001f;

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        anim     = GetComponent<Animator>();
        c_Stats  = GetComponent<C_Stats>();
        c_Health = GetComponent<C_Health>();
        c_FX     = GetComponent<C_FX>();
        idle     = GetComponent<P_State_Idle>();
        move     = GetComponent<P_State_Movement>();
        attack   = GetComponent<P_State_Attack>();
        dodge    = GetComponent<P_State_Dodge>();
        input    = new P_InputActions();

        // Set default animator facing (down)
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

    void Start()
    {
        // Notify UI of starting weapons
        if (meleeWeapon?.weaponData != null)
        {
            currentMeleeData = meleeWeapon.weaponData;
            OnMeleeWeaponChanged?.Invoke(currentMeleeData);
        }

        if (rangedWeapon?.weaponData != null)
        {
            currentRangedData = rangedWeapon.weaponData;
            OnRangedWeaponChanged?.Invoke(currentRangedData);
        }
    }

    void OnDisable()
    {
        input.Disable();
        c_Health.OnDied -= OnDiedHandler;
        idle.enabled = move.enabled = attack.enabled = dodge.enabled = false;
    }

    void OnDestroy() => input?.Dispose();

    void OnDiedHandler()
    {
        SwitchState(PState.Dead);
        // Let GameManager handle death outcome (normal vs tutorial)
        SYS_GameManager.Instance.HandlePlayerDeath(this);
    }

    void Update()
    {
        if (isDead || isStunned) return;
        
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;
        if (dodgeCooldown > 0f)  dodgeCooldown  -= Time.deltaTime;

        ProcessInputs();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Block movement when stunned, but allow knockback
        Vector2 baseVel = isStunned ? Vector2.zero : desiredVelocity;
        rb.linearVelocity = baseVel + knockback;

        // Decay knockback each frame
        knockback = Vector2.MoveTowards(knockback, Vector2.zero, c_Stats.KR * Time.fixedDeltaTime);
    }

    // INPUT PROCESSING

    // Converts mouse screen position to world direction from player
    public Vector2 ReadMouseAim()
    {
        Vector2 m     = Mouse.current.position.ReadValue();
        var     cam   = Camera.main;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -cam.transform.position.z));
        Vector2 dir   = (Vector2)world - (Vector2)transform.position;
        
        return dir.sqrMagnitude > MIN_DISTANCE ? dir.normalized : Vector2.zero;
    }

    // Processes player input with priority: Death > Dodge > Attack > Movement > Idle
    void ProcessInputs()
    {
        if (c_Stats.currentHP <= 0) { SwitchState(PState.Dead); return; }
        if (currentState == PState.Dodge && isDodging) return;

        // Dodge cancels combo
        if (input.Player.Dodge.triggered && dodgeCooldown <= 0f)
        {
            if (isAttacking && attack != null) attack.ResetCombo();
            SwitchState(PState.Dodge);
            return;
        }

        // Attack input: queue if attacking, trigger if not
        if (attackCooldown <= 0f)
        {
            Vector2 mouseAim = ReadMouseAim();
            if (mouseAim != Vector2.zero) attackDir = mouseAim;

            if (input.Player.MeleeAttack.triggered)
            {
                currentWeapon = meleeWeapon;
                if (isAttacking) attack.QueueComboInput();
                else TriggerAttack();
            }
            else if (input.Player.RangedAttack.triggered)
            {
                currentWeapon = rangedWeapon;
                if (isAttacking) attack.QueueComboInput();
                else TriggerAttack();
            }
        }

        // Movement input
        moveAxis = input.Player.Move.ReadValue<Vector2>();
        if (moveAxis.sqrMagnitude > 1f) moveAxis.Normalize();

        if (moveAxis.sqrMagnitude > MIN_DISTANCE)
        {
            lastMove = moveAxis;
            if (!move.enabled) move.enabled = true;
            move.SetMoveAxis(moveAxis);
            
            if (currentState != PState.Attack && currentState != PState.Dodge)
                currentState = PState.Move;
            return;
        }

        // No input: return to idle
        if (move.enabled) move.enabled = false;
        if (currentState != PState.Attack && currentState != PState.Dodge)
            SwitchState(PState.Idle);
    }

    // STATE SYSTEM

    // Switches to new state, disabling others (except attack if mid-combo)
    public void SwitchState(PState state)
    {
        if (currentState == state) return;
        currentState = state;

        idle.enabled  = false;
        move.enabled  = false;
        dodge.enabled = false;
        if (!isAttacking) attack.enabled = false;

        switch (state)
        {
            case PState.Dead:
                desiredVelocity   = Vector2.zero;
                knockback         = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
                isDead            = true;
                isAttacking       = false;
                isStunned         = false;
                isDodging         = false;
                attack.enabled    = false;
                anim.SetTrigger("Die");
                break;

            case PState.Dodge:
                dodge.enabled   = true;
                isDodging     = true;
                dodgeCooldown = c_Stats.dodgeCooldown;
                dodge.Dodge(lastMove);
                break;

            case PState.Move:
                move.enabled = true;
                move.SetMoveAxis(moveAxis);
                break;

            case PState.Idle:
                desiredVelocity = Vector2.zero;
                isDead          = false;  // Clear dead flag for revival
                isAttacking     = false;
                isStunned       = false;
                isDodging       = false;
                idle.enabled    = true;
                idle.SetIdleFacing(lastMove);
                break;
        }
    }

    // Initiates attack without disabling movement state (allows attack while moving)
    void TriggerAttack()
    {
        if (currentWeapon == null || currentWeapon.weaponData == null)
        {
            Debug.LogWarning("Cannot attack: weapon or weaponData is null");
            return;
        }
        
        currentState   = PState.Attack;
        attack.enabled = true;
        attackCooldown = c_Stats.attackCooldown;
        isAttacking    = true;
        
        attack.Attack(currentWeapon, attackDir);
        currentWeapon = null;
    }

    // COMBAT EFFECTS

    // Stuns player for duration (extends if longer stun applied)
    public IEnumerator SetStunTime(float duration)
    {
        if (duration <= 0f) yield break;

        float newEnd = Time.time + duration;
        if (newEnd > stunUntil) stunUntil = newEnd;

        isStunned = true;
        while (Time.time < stunUntil) yield return null;
        isStunned = false;
    }

    public void SetDesiredVelocity(Vector2 desiredVelocity) => this.desiredVelocity = desiredVelocity;
    public void SetKnockback(Vector2 knockback) => this.knockback += knockback;
    
    // Called by P_State_Attack when attack animation ends
    public void SetAttacking(bool value)
    {
        isAttacking = value;
        if (!value && currentState == PState.Attack)
        {
            if (move.enabled && moveAxis.sqrMagnitude > MIN_DISTANCE) currentState = PState.Move;
            else SwitchState(PState.Idle);
        }
    }
    
    // Called by P_State_Dodge when dodge animation ends
    public void SetDodging(bool value)
    {
        isDodging = value;
        if (!value && currentState == PState.Dodge)
        {
            if (moveAxis.sqrMagnitude > MIN_DISTANCE)
            {
                move.enabled = true;
                move.SetMoveAxis(moveAxis);
                currentState = PState.Move;
            }
            else SwitchState(PState.Idle);
        }
    }

    // WEAPON SYSTEM

    // Swaps equipped weapon, returns old weapon for inventory management
    public W_SO SetWeapon(W_SO newWeaponData)
    {
        if (newWeaponData == null) { Debug.LogError($"{name}: Cannot equip null weapon!", this); return null; }

        bool   isMelee       = newWeaponData.type == WeaponType.Melee;
        W_SO   oldWeaponData = isMelee ? currentMeleeData : currentRangedData;
        W_Base weaponSlot    = isMelee ? meleeWeapon : rangedWeapon;
        
        if (isMelee) currentMeleeData  = newWeaponData;
        else         currentRangedData = newWeaponData;
        
        if (weaponSlot != null)
        {
            weaponSlot.weaponData = newWeaponData;
            var spriteRenderer = weaponSlot.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && newWeaponData.sprite != null)
                spriteRenderer.sprite = newWeaponData.sprite;
        }

        if (isMelee) OnMeleeWeaponChanged?.Invoke(newWeaponData);
        else         OnRangedWeaponChanged?.Invoke(newWeaponData);

        return oldWeaponData;
    }

    public W_SO GetMeleeWeapon()  => currentMeleeData;
    public W_SO GetRangedWeapon() => currentRangedData;

    // REVIVAL & ZONE TRACKING

    public void Revive(Vector3 spawnPosition)
    {
        // 1/ Heal to full
        c_Stats.currentHP = c_Stats.maxHP;

        // 2/ Reset physics
        rb.linearVelocity = Vector2.zero;
        knockback = Vector2.zero;

        // 3/ Position at spawn
        transform.position = spawnPosition;

        // 4/ Reset animator
        anim.Rebind();
        anim.Update(0f);

        // 5/ Reset to Idle state (clears isDead flag)
        SwitchState(PState.Idle);

        // 6/ Restore sprite alpha
        c_FX.ResetAlpha();
    }
    
    // Tutorial death zone tracking
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<TutorialDeathZone>()) isInTutorialZone = true;
    }

    void OnTriggerExit2D(Collider2D col)
    { 
        if (col.GetComponent<TutorialDeathZone>()) isInTutorialZone = false;
    }
    
    // LADDER SYSTEM
    public void EnterLadder(ENV_Ladder ladder) => currentLadder = ladder;
    public void ExitLadder() => currentLadder = null;
    public Vector2 ApplyLadderModifiers(Vector2 velocity) => currentLadder ? currentLadder.ApplyLadderSpeed(velocity) : velocity;
}

