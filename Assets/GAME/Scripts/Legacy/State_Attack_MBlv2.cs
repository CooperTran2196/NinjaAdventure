using System.Collections;
using UnityEngine;

public class State_Attack_MBlv2 : MonoBehaviour
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
    public float attackHitDelay   = 2.00f;   // Telegraph duration

    [Header("Special Attack (Jump)")]
    public float specialCooldown       = 8.0f;
    public float specialClipLength     = 5.0f;
    public float specialHitDelay       = 3.0f;   // Telegraph duration
    public float specialAoERadius      = 1.8f;
    public float specialAoEOffsetY     = -1f;
    public int   specialDamage         = 25;
    public float specialKnockbackForce = 8f;

    [Header("Dash Settings")]
    public float preHitStopBias  = 0.02f;
    public float stopShortOffset = 0.96f;

    // Animator params
    const string kIsAttacking     = "isAttacking";
    const string kIsSpecialAttack = "isSpecialAttack";

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

    public bool IsAttacking { get; private set; }

    // Computed move windows
    float AttackComputedMoveWindow  => Mathf.Max(0f, attackClipLength - attackHitDelay - preHitStopBias);
    float SpecialComputedMoveWindow => Mathf.Max(0f, specialClipLength - specialHitDelay - preHitStopBias);

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
        // Clear idle state when entering attack
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
        anim?.SetBool(kIsAttacking, false);
        anim?.SetBool(kIsSpecialAttack, false);
    }

    // ATTACK DECISION

    void Update()
    {
        if (!isDashing) controller?.SetDesiredVelocity(Vector2.zero);
        if (!target) return;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float dx = to.x, dy = to.y;
        float d  = to.magnitude;

        Vector2 dir = d > 0.0001f ? to.normalized : lastFace;
        UpdateIdleFacing(IsAttacking ? lastFace : dir);

        if (IsAttacking) return;

        bool specialReady  = Time.time >= nextSpecialReadyAt;
        bool canAttackNow  = Time.time >= nextAttackReadyAt;
        bool inSpecialRange = d <= specialRange;
        bool inAttackRange = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);

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
        anim.SetBool(kIsSpecialAttack, isSpecial);
        anim.SetBool(kIsAttacking, !isSpecial);

        // Set facing direction
        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        // Select timing based on attack type
        float clipLength = isSpecial ? specialClipLength : attackClipLength;
        float hitDelay   = isSpecial ? specialHitDelay   : attackHitDelay;
        float moveWindow = isSpecial ? SpecialComputedMoveWindow : AttackComputedMoveWindow;

        float t = 0f;

        // A) Telegraph phase
        while (t < hitDelay) { t += Time.deltaTime; yield return null; }

        // B) Dash toward player
        BeginDash(moveWindow);
        
        // Normal attack: enable weapon hitbox during dash
        if (!isSpecial)
        {
            activeWeapon?.Attack(lastFace);
        }
        
        while (t < clipLength)
        {
            t += Time.deltaTime;
            if (ReachedDashDest()) StopDash();
            yield return null;
        }
        StopDash();
        
        // C) Special attack: AoE damage at landing
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
        anim.SetBool(kIsSpecialAttack, false);
        anim.SetBool(kIsAttacking, false);
    }

    void ApplyAoEDamageKnockback()
    {
        Vector2 aoeCenter = (Vector2)transform.position + new Vector2(0f, specialAoEOffsetY);
        Collider2D hit = Physics2D.OverlapCircle(aoeCenter, specialAoERadius, playerLayer);
        
        if (!hit || !hit.CompareTag("Player")) return;
        
        C_Health playerHealth = hit.GetComponent<C_Health>();
        if (!playerHealth) return;
        
        playerHealth.ApplyDamage(specialDamage, 0, 0, 0, 0, 0);
        
        // Radial knockback from AoE center
        Vector2 knockbackDir = ((Vector2)hit.transform.position - aoeCenter).normalized;
        P_Controller pc = hit.GetComponent<P_Controller>();
        
        if (pc)
        {
            pc.ReceiveKnockback(knockbackDir * specialKnockbackForce);
        }
        else
        {
            Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
            playerRb?.AddForce(knockbackDir * specialKnockbackForce, ForceMode2D.Impulse);
        }
    }

    // DASH SYSTEM
    void BeginDash(float moveWindow)
    {
        if (!target) return;

        Vector2 start     = transform.position;
        Vector2 targetPos = target.position;
        int     sign      = (targetPos.x - start.x) >= 0f ? +1 : -1;
        Vector2 faceSpot  = new Vector2(targetPos.x - sign * stopShortOffset, targetPos.y);
        Vector2 toFace    = faceSpot - start;
        
        float dashSpeed = (moveWindow > 0f) ? (toFace.magnitude / moveWindow) : 0f;

        dashDir  = toFace.sqrMagnitude > 0f ? toFace.normalized : new Vector2(sign, 0f);
        dashDest = faceSpot;

        controller.SetDesiredVelocity(dashDir * dashSpeed);
        isDashing = true;

        afterimage?.StartBurst(moveWindow, sr.sprite, sr.flipX, sr.flipY);
    }

    void StopDash()
    {
        isDashing = false;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    bool ReachedDashDest()
    {
        Vector2 pos = transform.position;
        Vector2 toDest = dashDest - pos;
        return Vector2.Dot(toDest, dashDir) <= 0f || (toDest.sqrMagnitude <= 0.0004f);
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
        Vector3 aoeCenter = p + new Vector3(0f, specialAoEOffsetY, 0f);

        // AoE radius (red) - jump attack damage area with Y offset
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Semi-transparent red
        Gizmos.DrawSphere(aoeCenter, specialAoERadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aoeCenter, specialAoERadius);
    }
}
