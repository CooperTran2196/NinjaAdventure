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
    W_Base              activeWeapon;

    [Header("Target")]
                    public LayerMask playerLayer;

    [Header("Normal Attack (Charge)")]
                    public float attackCooldown   = 1.10f;
                    public float attackClipLength = 2.35f;
                    public float attackHitDelay   = 2.00f;   // Charging duration
                    public float attackDashSpeed  = 9.0f;    // Constant dash velocity

    [Header("Special Attack (Jump)")]
                    public float specialCooldown       = 8.0f;
                    public float specialClipLength     = 5.0f;
                    public float specialHitDelay       = 3.0f;   // Charging duration
                    public float specialDashSpeed      = 12.0f;  // Constant dash velocity (can be different)
                    public float specialAoERadius      = 1.8f;
                    public float specialAoEOffsetY     = -1f;
                    public int   specialDamage         = 25;
                    public float specialKnockbackForce = 8f;

    [Header("Dash Settings")]
                    public float stopShortOffset = 0.96f;    // Distance kept in front of player

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
    bool      isDashing;

    // Dash runtime
    Vector2 dashDest;
    Vector2 dashDir;
    float   currentDashSpeed;

    public bool IsAttacking { get; private set; }

    void Awake()
    {
        rb           ??= GetComponent<Rigidbody2D>();
        anim         ??= GetComponentInChildren<Animator>();
        controller   ??= GetComponent<I_Controller>();
        activeWeapon ??= GetComponentInChildren<W_Base>();
        sr           ??= GetComponentInChildren<SpriteRenderer>();
        afterimage   ??= sr ? sr.GetComponent<C_AfterimageSpawner>() : null;
    }

    void OnEnable()
    {
        anim?.SetBool("isIdle", false);
        anim?.SetBool("isMoving", false);
        anim?.SetBool("isWandering", false);
    }

    void OnDisable()
    {
        IsAttacking = false;
        isDashing   = false;
        controller.SetDesiredVelocity(Vector2.zero);
        rb.linearVelocity = Vector2.zero;
        anim.speed = 1.0f;  // Reset animation speed
        anim.SetBool(isAttacking, false);
        anim.SetBool(isSpecialAttack, false);
    }

    // ATTACK DECISION

    void Update()
    {
        if (!isDashing) controller.SetDesiredVelocity(Vector2.zero);
        if (!target) return;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float dx = to.x, dy = to.y;
        float d  = to.magnitude;

        Vector2 dir = d > 0.0001f ? to.normalized : lastFace;
        UpdateIdleFacing(IsAttacking ? lastFace : dir);

        if (IsAttacking) return;

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
        float maxRange   = isSpecial ? specialRange      : attackRange;

        float t = 0f;

        // 1/ Charging phase (normal speed)
        while (t < hitDelay) 
        { 
            t += Time.deltaTime; 
            yield return null; 
        }

        // 2/ Calculate and apply dash animation speed
        float dashPhaseTime = clipLength - hitDelay;
        float actualDashDist = CalculateDashDistance(maxRange);
        float timeNeeded = actualDashDist / dashSpeed;
        float animSpeed = dashPhaseTime / timeNeeded;
        
        anim.speed = animSpeed;  // Change speed for dash phase only

        // 3/ Begin dash
        BeginDash(dashSpeed, actualDashDist);
        
        // Normal attack: enable weapon hitbox during dash
        if (!isSpecial)
        {
            activeWeapon.Attack(lastFace);
        }
        
        // 4/ Dash phase
        while (t < clipLength)
        {
            t += Time.deltaTime;
            if (ReachedDashDest()) break;
            yield return null;
        }
        StopDash();
        
        // 5/ Special attack: AoE damage at landing
        if (isSpecial)
        {
            ApplyAoEDamageKnockback();
        }

        // Set cooldowns
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

    void ApplyAoEDamageKnockback()
    {
        Vector2 aoeCenter = (Vector2)transform.position + new Vector2(0f, specialAoEOffsetY);
        Collider2D hit = Physics2D.OverlapCircle(aoeCenter, specialAoERadius, playerLayer);
        
        if (!hit || !hit.CompareTag("Player")) return;
        
        C_Health playerHealth = hit.GetComponent<C_Health>();
        P_Controller pc = hit.GetComponent<P_Controller>();
        
        if (!playerHealth || !pc) return;
        
        playerHealth.ApplyDamage(specialDamage, 0, 0, 0, 0, 0);
        
        Vector2 knockbackDir = ((Vector2)hit.transform.position - aoeCenter).normalized;
        pc.ReceiveKnockback(knockbackDir * specialKnockbackForce);
    }

    // DASH SYSTEM

    float CalculateDashDistance(float maxRange)
    {
        if (!target) return 0f;
        
        Vector2 start = transform.position;
        Vector2 targetPos = target.position;
        int sign = (targetPos.x - start.x) >= 0f ? +1 : -1;
        Vector2 faceSpot = new Vector2(targetPos.x - sign * stopShortOffset, targetPos.y);
        
        float distToFaceSpot = Vector2.Distance(start, faceSpot);
        // Always dash to player position (no maxRange limit - will reach player unless they dodge)
        return distToFaceSpot;
    }

    void BeginDash(float dashSpeed, float actualDashDist)
    {
        if (!target) return;

        Vector2 start     = transform.position;
        Vector2 targetPos = target.position;
        int     sign      = (targetPos.x - start.x) >= 0f ? +1 : -1;
        Vector2 faceSpot  = new Vector2(targetPos.x - sign * stopShortOffset, targetPos.y);
        Vector2 toFace    = faceSpot - start;

        dashDir  = toFace.sqrMagnitude > 0f ? toFace.normalized : new Vector2(sign, 0f);
        dashDest = start + dashDir * actualDashDist;
        currentDashSpeed = dashSpeed;

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

    // GIZMOS

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 p = transform.position;

        // 1/ Normal attack range (orange)
        Gizmos.color = new Color(1f, 0.5f, 0f);  // Orange
        Gizmos.DrawWireSphere(p, attackRange);

        // 2/ Special attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p, specialRange);

        // 3/ Target indicator: red = can use special attack, orange = normal attack only
        if (target)
        {
            Vector3 tp = target.position;
            float dist = Vector3.Distance(p, tp);
            bool canUseSpecial = dist <= specialRange && dist > attackRange;
            Gizmos.color = canUseSpecial ? Color.red : new Color(1f, 0.5f, 0f);  // Red or orange
            Gizmos.DrawWireSphere(tp, 0.3f);
            
            // 4/ Cyan line showing dash trajectory (special attack only)
            if (canUseSpecial)
            {
                Vector2 toPlayer = (Vector2)(tp - p);
                Vector2 dashDirection = toPlayer.normalized;
                float dashPhaseTime = specialClipLength - specialHitDelay;
                float dashDistance = specialDashSpeed * dashPhaseTime;
                Vector3 dashEnd = p + (Vector3)(dashDirection * dashDistance);
                
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(p, dashEnd);
            }
        }
    }
}
