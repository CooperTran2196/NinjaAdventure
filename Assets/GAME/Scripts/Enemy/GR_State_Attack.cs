using System.Collections;
using UnityEngine;

public class GR_State_Attack : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D         rb;
    Animator            anim;
    I_Controller        controller;
    SpriteRenderer      sr;
    C_AfterimageSpawner afterimage;
    W_Melee             activeWeapon;

    [Header("Target")]
    public LayerMask playerLayer;

    [Header("Normal Attack (Charge)")]
    public float attackCooldown      = 2f;
    public float attackClipLength    = 1.35f;
    public float attackHitDelay      = 1.00f;   // Charging duration
    public float attackDashSpeed     = 15.0f;   // Constant dash velocity
    public float attackRecoveryTime  = 1f;      // Vulnerable idle time after attack

    [Header("Special Attack (Jump)")]
    public float specialCooldown     = 8.0f;
    public float specialClipLength   = 3.44f;
    public float specialHitDelay     = 3f;     // Charging duration
    public float specialDashSpeed    = 20.0f;  // Constant dash velocity
    public int   specialDamage       = 20;     // Weapon AD override for special attack
    public float specialRecoveryTime = 2f;     // Vulnerable idle time after special

    // Animator params
    const string isAttacking     = "isAttacking";
    const string isSpecialAttack = "isSpecialAttack";

    // Runtime state
    Transform target;
    Vector2   lastFace = Vector2.right;
    float     attackRange;
    float     specialRange;
    float     nextAttackReadyAt;
    float     nextSpecialReadyAt;
    float     recoveryEndTime;   // Time when vulnerable window ends
    bool      isDashing;

    // Dash runtime
    Vector2 dashDest;
    Vector2 dashDir;

    public bool IsAttacking { get; private set; }
    public bool IsRecovering => Time.time < recoveryEndTime;

    void Awake()
    {
        rb           = GetComponent<Rigidbody2D>();
        anim         = GetComponent<Animator>();
        controller   = GetComponent<I_Controller>();
        activeWeapon = GetComponentInChildren<W_Melee>();
        sr           = GetComponentInChildren<SpriteRenderer>();
        afterimage   = GetComponent<C_AfterimageSpawner>();
    }

    void OnEnable()
    {
        anim.SetBool("isIdle", false);
        anim.SetBool("isMoving", false);
        anim.SetBool("isWandering", false);
    }

    void OnDisable()
    {
        IsAttacking = false;
        isDashing   = false;
        controller.SetDesiredVelocity(Vector2.zero);
        rb.linearVelocity = Vector2.zero;
        anim.speed = 1.0f;
        anim.SetBool(isAttacking, false);
        anim.SetBool(isSpecialAttack, false);
    }

    // ATTACK DECISION

    void Update()
    {
        if (!isDashing) controller.SetDesiredVelocity(Vector2.zero);
        if (!target) return;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float d  = to.magnitude;

        Vector2 dir = d > 0.0001f ? to.normalized : lastFace;
        UpdateIdleFacing(IsAttacking ? lastFace : dir);

        // Block new attacks during recovery window
        if (IsAttacking || IsRecovering) return;

        bool specialReady   = Time.time >= nextSpecialReadyAt;
        bool canAttackNow   = Time.time >= nextAttackReadyAt;
        bool inSpecialRange = d <= specialRange;
        bool inAttackRange  = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);

        // Priority: Special > Normal
        if (specialReady && inSpecialRange)
        {
            StartCoroutine(AttackRoutine(dir, isSpecial: true));
        }
        else if (canAttackNow && inAttackRange)
        {
            StartCoroutine(AttackRoutine(dir, isSpecial: false));
        }
    }

    // CONTROLLER HOOKS

    public void SetTarget(Transform t) => target = t;

    public void SetRanges(float attackRange, float specialRange)
    {
        this.attackRange  = attackRange;
        this.specialRange = specialRange;
    }

    public bool CanSpecialNow(Vector2 bossPos, Vector2 playerPos)
    {
        if (Time.time < nextSpecialReadyAt) return false;
        return Vector2.Distance(bossPos, playerPos) <= specialRange;
    }

    // ATTACK ROUTINES

    IEnumerator AttackRoutine(Vector2 dirAtStart, bool isSpecial)
    {
        IsAttacking = true;
        
        // Set animator bools
        anim.SetBool(isSpecialAttack, isSpecial);
        anim.SetBool(isAttacking, !isSpecial);
        anim.speed = 1.0f;  // Start at normal speed

        // Set facing direction
        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        // Select parameters based on attack type
        float clipLength = isSpecial ? specialClipLength : attackClipLength;
        float hitDelay   = isSpecial ? specialHitDelay   : attackHitDelay;
        float dashSpeed  = isSpecial ? specialDashSpeed  : attackDashSpeed;

        float t = 0f;

        // 1/ Charging phase (normal speed)
        while (t < hitDelay) 
        { 
            t += Time.deltaTime; 
            yield return null; 
        }

        // 2/ Calculate and apply dash animation speed
        float dashPhaseTime = clipLength - hitDelay;
        float actualDashDist = CalculateDashDistance();
        float timeNeeded = actualDashDist / dashSpeed;
        float animSpeed = dashPhaseTime / timeNeeded;
        
        anim.speed = animSpeed;  // Change speed for dash phase only

        // 3/ Begin dash + weapon activation
        BeginDash(dashSpeed, actualDashDist);
        
        if (activeWeapon)
        {
            // Special attack: temporarily override weapon AD for extra damage
            int originalAD = activeWeapon.weaponData.AD;
            if (isSpecial)
            {
                activeWeapon.weaponData.AD = specialDamage;
            }
            
            // Always use thrust attack (combo index 2) for both normal and special
            activeWeapon.AttackAsEnemy(lastFace, 2);
            
            // Restore original weapon AD after attack starts
            if (isSpecial)
            {
                activeWeapon.weaponData.AD = originalAD;
            }
        }
        
        // 4/ Dash phase
        while (t < clipLength)
        {
            t += Time.deltaTime;
            if (ReachedDashDest()) break;
            yield return null;
        }
        StopDash();

        // Set cooldowns + recovery window
        float recoveryTime = isSpecial ? specialRecoveryTime : attackRecoveryTime;
        recoveryEndTime = Time.time + recoveryTime;
        
        nextAttackReadyAt = Time.time + attackCooldown;
        if (isSpecial)
        {
            nextSpecialReadyAt = Time.time + specialCooldown;
        }

        IsAttacking = false;
        anim.speed = 1.0f;  // Reset to normal
        anim.SetBool(isSpecialAttack, false);
        anim.SetBool(isAttacking, false);
    }

    // DASH SYSTEM

    float CalculateDashDistance()
    {
        if (!target) return 0f;
        
        Vector2 start = transform.position;
        Vector2 targetPos = target.position;
        
        return Vector2.Distance(start, targetPos);
    }

    void BeginDash(float dashSpeed, float actualDashDist)
    {
        if (!target) return;

        Vector2 start     = transform.position;
        Vector2 targetPos = target.position;
        Vector2 toPlayer  = targetPos - start;

        dashDir  = toPlayer.sqrMagnitude > 0f ? toPlayer.normalized : lastFace;
        dashDest = start + dashDir * actualDashDist;

        controller.SetDesiredVelocity(dashDir * dashSpeed);
        isDashing = true;

        // Calculate actual travel time for afterimage
        float travelTime = actualDashDist / dashSpeed;
        afterimage.StartBurst(travelTime, sr.sprite, sr.flipX, sr.flipY);
    }

    void StopDash()
    {
        if (!isDashing) return;
        isDashing = false;
        controller.SetDesiredVelocity(Vector2.zero);
        rb.linearVelocity = Vector2.zero;
    }

    bool ReachedDashDest()
    {
        Vector2 pos = transform.position;
        Vector2 toDest = dashDest - pos;
        return Vector2.Dot(toDest, dashDir) <= 0f || toDest.sqrMagnitude <= 0.0004f;
    }

    // ANIMATION

    void UpdateIdleFacing(Vector2 faceDir)
    {
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        Vector2 f = faceDir.sqrMagnitude > 0f ? faceDir.normalized : lastFace;
        anim.SetFloat("idleX", f.x);
        anim.SetFloat("idleY", f.y);
    }
}
