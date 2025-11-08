using System.Collections;
using UnityEngine;

public class GRS_State_Attack : MonoBehaviour
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

    [Header("Normal Attack")]
    public float attackCooldown = 1.10f;
    public float attackDuration = 0.45f;
    public float hitDelay       = 0.25f;

    [Header("Special (single clip flow)")]
    public float specialCooldown  = 8.0f;
    public float specialClipLength = 1.50f;
    public float specialHitDelay  = 0.50f;   // Charging duration
    public float specialDashSpeed = 9.0f;    // Constant dash velocity
    public float followupGap      = 0.14f;   // Gap between first and second hit

    [Header("Alignment Gate")]
    public float yHardCap = 0.55f;            // Must be within this to attack

    // Animator params
    const string isAttacking     = "isAttacking";
    const string isSpecialAttack = "isSpecialAttack";

    // Runtime state
    Transform target;
    Vector2   lastFace = Vector2.right;
    float     attackRange;
    float     detectionRange;
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
        rb           = GetComponent<Rigidbody2D>();
        anim         = GetComponent<Animator>();
        controller   = GetComponent<I_Controller>();
        activeWeapon = GetComponentInChildren<W_Base>();
        sr           = GetComponentInChildren<SpriteRenderer>();
        afterimage   = GetComponent<C_AfterimageSpawner>();
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

        // Gates
        bool inNormalRange  = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        bool specialReady   = Time.time >= nextSpecialReadyAt;
        bool canAttackNow   = Time.time >= nextAttackReadyAt;
        bool alignedY       = Mathf.Abs(dy) <= yHardCap;
        
        bool inSpecialRange = d <= detectionRange && d > attackRange;

        // Priority: Special > Normal
        if (canAttackNow && alignedY)
        {
            if (specialReady && inSpecialRange)
            {
                StartCoroutine(SpecialRoutine(dir));
            }
            else if (inNormalRange)
            {
                StartCoroutine(NormalRoutine(dir));
            }
        }
    }

    // CONTROLLER HOOKS
    public void SetTarget(Transform t) => target = t;

    public void SetRanges(float attackRange, float detectionRange)
    {
        this.attackRange = attackRange;
        this.detectionRange = detectionRange;
    }

    public bool CanSpecialNow(Vector2 bossPos, Vector2 playerPos)
    {
        if (Time.time < nextSpecialReadyAt || Time.time < nextAttackReadyAt) return false;
        float distance = Vector2.Distance(bossPos, playerPos);
        return distance <= detectionRange;
    }

    // ATTACK ROUTINES
    IEnumerator NormalRoutine(Vector2 dirAtStart)
    {
        IsAttacking = true;
        anim.SetBool(isSpecialAttack, false);
        anim.SetBool(isAttacking, true);
        anim.speed = 1.0f;  // Normal attack doesn't use dash, keep normal speed

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        yield return new WaitForSeconds(hitDelay);

        activeWeapon.Attack(lastFace);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));

        nextAttackReadyAt = Time.time + attackCooldown;
        IsAttacking = false;
        anim.SetBool(isAttacking, false);
    }

    IEnumerator SpecialRoutine(Vector2 dirAtStart)
    {
        IsAttacking = true;
        anim.SetBool(isAttacking, false);
        anim.SetBool(isSpecialAttack, true);
        anim.speed = 1.0f;  // Start at normal speed

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        float t = 0f;

        // 1/ Charging phase (normal speed)
        while (t < specialHitDelay) 
        { 
            t += Time.deltaTime; 
            yield return null; 
        }

        // 2/ Calculate and apply dash animation speed
        float dashPhaseTime = specialClipLength - specialHitDelay;
        float dashDistance = specialDashSpeed * dashPhaseTime;
        float animSpeed = dashPhaseTime / (dashDistance / specialDashSpeed);
        
        anim.speed = animSpeed;  // Change speed for dash phase only

        // 3/ Begin dash toward player
        BeginDash(specialDashSpeed, dashDistance);
        
        // 4/ Dash until clip ends
        while (t < specialClipLength)
        {
            t += Time.deltaTime;
            yield return null;
        }
        StopDash();

        // 5/ First hit right after dash ends
        activeWeapon.Attack(lastFace);

        // 6/ Quick follow-up second hit
        yield return new WaitForSeconds(followupGap);
        activeWeapon.Attack(lastFace);

        nextAttackReadyAt  = Time.time + attackCooldown;
        nextSpecialReadyAt = Time.time + specialCooldown;

        IsAttacking = false;
        anim.speed = 1.0f;  // Reset to normal
        anim.SetBool(isSpecialAttack, false);
    }

    // DASH SYSTEM

    float CalculateDashDistance()
    {
        // Distance = speed × time
        float dashPhaseTime = specialClipLength - specialHitDelay;
        return specialDashSpeed * dashPhaseTime;
    }

    void BeginDash(float dashSpeed, float dashDistance)
    {
        if (!target) return;

        Vector2 start = transform.position;
        Vector2 targetPos = target.position;
        
        // Direction toward player (simple vector to player position)
        Vector2 toPlayer = targetPos - start;
        dashDir = toPlayer.sqrMagnitude > 0f ? toPlayer.normalized : lastFace;
        dashDest = start + dashDir * dashDistance;
        currentDashSpeed = dashSpeed;

        controller.SetDesiredVelocity(dashDir * dashSpeed);
        isDashing = true;

        // Calculate actual travel time for afterimage
        float travelTime = dashDistance / dashSpeed;
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
        Gizmos.DrawWireSphere(p, detectionRange);

        // 3/ Target indicator: red = can use special attack, orange = normal attack only
        if (target)
        {
            Vector3 tp = target.position;
            float dist = Vector3.Distance(p, tp);
            bool canUseSpecial = dist <= detectionRange && dist > attackRange;
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
