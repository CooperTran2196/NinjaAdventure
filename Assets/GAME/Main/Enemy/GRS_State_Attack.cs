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
    W_Melee             activeWeapon;

    [Header("Target")]
    public LayerMask playerLayer;

    [Header("Normal Attack")]
    public float attackCooldown     = 1.10f;
    public float attackDuration     = 0.45f;
    public float hitDelay           = 0.25f;
    public float attackRecoveryTime = 0.5f;

    [Header("Special Attack")]
    public float specialCooldown     = 8.0f;
    public float specialClipLength   = 1.50f;
    public float specialHitDelay     = 0.50f;
    public float specialDashSpeed    = 9.0f;
    public float specialRecoveryTime = 1.5f;

    [Header("Alignment Gate")]
    public float yHardCap = 0.55f;

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
    float     recoveryEndTime;
    bool      isDashing;

    // Dash runtime
    Vector2 dashDest;
    Vector2 dashDir;

    public bool IsAttacking { get; private set; }
    public bool IsRecovering => Time.time < recoveryEndTime;

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        anim            = GetComponent<Animator>();
        controller      = GetComponent<I_Controller>();
        activeWeapon    = GetComponentInChildren<W_Melee>();
        sr              = GetComponentInChildren<SpriteRenderer>();
        afterimage      = GetComponent<C_AfterimageSpawner>();
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
        float dx = to.x, dy = to.y;
        float d  = to.magnitude;

        Vector2 dir = d > 0.0001f ? to.normalized : lastFace;
        UpdateIdleFacing(IsAttacking ? lastFace : dir);

        if (IsAttacking || IsRecovering) return;

        bool inNormalRange  = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        bool specialReady   = Time.time >= nextSpecialReadyAt;
        bool canAttackNow   = Time.time >= nextAttackReadyAt;
        bool alignedY       = Mathf.Abs(dy) <= yHardCap;
        
        bool inSpecialRange = d <= detectionRange && d > attackRange;

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
        anim.speed = 1.0f;

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        yield return new WaitForSeconds(hitDelay);

        if (activeWeapon)
        {
            activeWeapon.AttackAsEnemy(lastFace, 2);
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));

        recoveryEndTime = Time.time + attackRecoveryTime;
        nextAttackReadyAt = Time.time + attackCooldown;
        IsAttacking = false;
        anim.SetBool(isAttacking, false);
    }

    IEnumerator SpecialRoutine(Vector2 dirAtStart)
    {
        IsAttacking = true;
        anim.SetBool(isAttacking, false);
        anim.SetBool(isSpecialAttack, true);
        anim.speed = 1.0f;

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        float t = 0f;

        while (t < specialHitDelay) 
        { 
            t += Time.deltaTime; 
            yield return null; 
        }

        float dashPhaseTime = specialClipLength - specialHitDelay;
        float actualDashDist = CalculateDashDistance();
        float timeNeeded = actualDashDist / specialDashSpeed;
        float animSpeed = dashPhaseTime / timeNeeded;
        
        anim.speed = animSpeed;

        BeginDash(specialDashSpeed, actualDashDist);
        
        if (activeWeapon)
        {
            activeWeapon.AttackAsEnemy(lastFace, 2);
        }
        
        while (t < specialClipLength)
        {
            t += Time.deltaTime;
            if (ReachedDashDest()) break;
            yield return null;
        }
        StopDash();

        recoveryEndTime = Time.time + specialRecoveryTime;
        nextAttackReadyAt  = Time.time + attackCooldown;
        nextSpecialReadyAt = Time.time + specialCooldown;

        IsAttacking = false;
        anim.speed = 1.0f;
        anim.SetBool(isSpecialAttack, false);
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

        Vector2 start = transform.position;
        Vector2 targetPos = target.position;
        
        Vector2 toPlayer = targetPos - start;
        dashDir = toPlayer.sqrMagnitude > 0f ? toPlayer.normalized : lastFace;
        dashDest = start + dashDir * actualDashDist;

        controller.SetDesiredVelocity(dashDir * dashSpeed);
        isDashing = true;

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

        // Normal attack range (orange)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(p, attackRange);

        // Special attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p, detectionRange);
    }
}
