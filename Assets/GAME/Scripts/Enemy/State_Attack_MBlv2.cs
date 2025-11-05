using System.Collections;
using UnityEngine;

public class State_Attack_MBlv2 : MonoBehaviour
{
    [Header("Target Layer")]
    public LayerMask playerLayer;

    [Header("Charge Attack")]
    public float chargeClipLength = 2.35f;  // GR_CAtk_L/R total length (0f to 2.35f)
    public float chargeHitDelay   = 2.0f;   // Charge phase (0f to 2f), then attack starts
    public float chargeDashSpeed  = 9.0f;
    public float chargeDashMaxDist = 5.0f;
    public int   chargeDamage     = 22;
    public float chargeKnockback  = 3.5f;

    [Header("Jump Attack")]
    public float jumpClipLength   = 5.0f;   // Jump animation total length (0f to 5f)
    public float jumpHitDelay     = 3.0f;   // Charge phase (0f to 3f), then jump starts
    public float jumpAoERadius    = 1.5f;
    public int   jumpDamage       = 18;
    public float jumpKnockback    = 2.5f;
    public float jumpCooldown     = 5.0f;
    public float jumpIdleTime     = 2.0f;  // Idle time after landing

    [Header("Weapon")]
    public W_Base activeWeapon;

    // Animator params
    const string kIsIdle     = "isIdle";
    const string kIsCharging = "isCharging";
    const string kIsJumping  = "isJumping";
    const string kIsMoving   = "isMoving";

    // Cache
    Rigidbody2D rb;
    Animator anim;
    I_Controller controller;
    C_Stats stats;
    SpriteRenderer sr;
    C_AfterimageSpawner afterimage;

    // Runtime
    Transform target;
    Vector2 lastFace = Vector2.right;
    float chargeRange = 5f;
    float jumpRange   = 8f;

    // Cooldowns
    float nextJumpReadyAt;

    // Status
    public bool IsAttacking { get; private set; }
    bool isDashing;
    bool hasHitTarget;

    void Awake()
    {
        rb         ??= GetComponent<Rigidbody2D>();
        anim       ??= GetComponentInChildren<Animator>();
        controller ??= GetComponent<I_Controller>();
        stats      ??= GetComponent<C_Stats>();
        sr         ??= GetComponentInChildren<SpriteRenderer>();
        activeWeapon ??= GetComponentInChildren<W_Base>();
        afterimage ??= sr ? sr.GetComponent<C_AfterimageSpawner>() : null;
    }

    void OnDisable()
    {
        IsAttacking = false;
        isDashing   = false;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
        anim.SetBool(kIsIdle, true);
        anim.SetBool(kIsCharging, false);
        anim.SetBool(kIsJumping, false);
        anim.SetBool(kIsMoving, false);
        UpdateIdleFacing(lastFace);
    }

    void Update()
    {
        if (!isDashing) controller?.SetDesiredVelocity(Vector2.zero);
        if (!target) return;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float dist = to.magnitude;

        Vector2 dir = dist > 0.0001f ? to.normalized : lastFace;
        
        // Update idle facing animator floats
        UpdateIdleFacing(IsAttacking ? lastFace : dir);

        bool jumpReady   = Time.time >= nextJumpReadyAt;
        bool canAttackNow = !IsAttacking;

        if (canAttackNow)
        {
            // Jump attack: long range + cooldown ready
            if (jumpReady && dist > chargeRange && dist <= jumpRange)
            {
                StartCoroutine(JumpRoutine(dir));
                return;
            }
            
            // Charge attack: normal range (no cooldown, always available)
            if (dist <= chargeRange)
            {
                StartCoroutine(ChargeRoutine(dir));
                return;
            }
        }
    }

    // -------- controller hooks ----------
    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float chargeRange, float jumpRange)
    {
        this.chargeRange = chargeRange;
        this.jumpRange   = jumpRange;
    }

    // -------- Charge Attack ----------
    IEnumerator ChargeRoutine(Vector2 dirAtStart)
    {
        IsAttacking = true;
        hasHitTarget = false;
        anim.SetBool(kIsIdle, false);
        anim.SetBool(kIsJumping, false);
        anim.SetBool(kIsMoving, false);
        anim.SetBool(kIsCharging, true);

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        
        // Set attack direction for blend tree
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);

        float t = 0f;

        // Wait for telegraph
        while (t < chargeHitDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Dash phase
        Vector2 startPos = transform.position;
        Vector2 targetPos = target ? (Vector2)target.position : startPos + lastFace * chargeDashMaxDist;
        
        // Dash toward target (any direction, blend tree handles animation)
        Vector2 dashDir = (targetPos - startPos).normalized;
        if (dashDir.sqrMagnitude == 0f) dashDir = lastFace;
        
        float dashDist = Mathf.Min(Vector2.Distance(startPos, targetPos), chargeDashMaxDist);
        Vector2 dashDest = startPos + dashDir * dashDist;

        controller?.SetDesiredVelocity(dashDir * chargeDashSpeed);
        isDashing = true;

        // Dash until clip ends OR destination reached
        while (t < chargeClipLength)
        {
            t += Time.deltaTime;

            // Check if reached destination early
            Vector2 currentPos = transform.position;
            Vector2 toDest = dashDest - currentPos;
            if (Vector2.Dot(toDest, dashDir) <= 0f || toDest.sqrMagnitude <= 0.01f)
                break;

            yield return null;
        }

        StopDash();

        IsAttacking = false;
        anim.SetBool(kIsCharging, false);
        anim.SetBool(kIsIdle, true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDashing || hasHitTarget || !other.CompareTag("Player")) return;

        C_Health playerHealth = other.GetComponent<C_Health>();
        if (playerHealth != null)
        {
            playerHealth.ApplyDamage(chargeDamage, 0, 0, 0, 0, 0);
            
            // Apply knockback
            Vector2 knockbackDir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb)
            {
                playerRb.linearVelocity = knockbackDir * chargeKnockback * 10f; // Multiply for force
            }

            hasHitTarget = true;
        }
    }

    // -------- Jump Attack ----------
    IEnumerator JumpRoutine(Vector2 dirAtStart)
    {
        IsAttacking = true;
        anim.SetBool(kIsIdle, false);
        anim.SetBool(kIsCharging, false);
        anim.SetBool(kIsMoving, false);
        anim.SetBool(kIsJumping, true);

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;

        // Set attack direction for blend tree
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);

        // Store landing position (player's current position)
        Vector2 landingPos = target ? (Vector2)target.position : (Vector2)transform.position + lastFace * 3f;

        float t = 0f;

        // Wait for telegraph
        while (t < jumpHitDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Jump phase - leap toward landing position
        Vector2 startPos = transform.position;
        Vector2 jumpDir = (landingPos - startPos).normalized;
        float jumpDist = Vector2.Distance(startPos, landingPos);
        float jumpPhaseTime = jumpClipLength - jumpHitDelay;
        float jumpSpeed = jumpDist / jumpPhaseTime;

        controller?.SetDesiredVelocity(jumpDir * jumpSpeed);

        // Start afterimage burst for jump duration
        if (afterimage && sr)
            afterimage.StartBurst(jumpPhaseTime, sr.sprite, sr.flipX, sr.flipY);

        // Jump arc
        while (t < jumpClipLength)
        {
            t += Time.deltaTime;

            // Simple arc effect (optional: scale sprite or use Y offset)
            float jumpProgress = (t - jumpHitDelay) / jumpPhaseTime;
            if (jumpProgress > 0f)
            {
                float arcHeight = Mathf.Sin(jumpProgress * Mathf.PI) * 1.5f;
                transform.position = new Vector3(transform.position.x, transform.position.y, arcHeight);
            }

            yield return null;
        }

        // Reset Z position
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        controller?.SetDesiredVelocity(Vector2.zero);

        // Landing - AoE damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, jumpAoERadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                C_Health playerHealth = hit.GetComponent<C_Health>();
                if (playerHealth != null)
                {
                    playerHealth.ApplyDamage(jumpDamage, 0, 0, 0, 0, 0);

                    // Apply knockback
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                    if (playerRb)
                    {
                        playerRb.linearVelocity = knockbackDir * jumpKnockback * 10f;
                    }
                }
            }
        }

        anim.SetBool(kIsJumping, false);

        // Idle after landing - vulnerable window
        yield return new WaitForSeconds(jumpIdleTime);

        nextJumpReadyAt = Time.time + jumpCooldown;
        IsAttacking = false;
        anim.SetBool(kIsIdle, true);
    }

    void StopDash()
    {
        isDashing = false;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    // -------- anim helper ----------
    void UpdateIdleFacing(Vector2 faceDir)
    {
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        Vector2 f = faceDir.sqrMagnitude > 0f ? faceDir.normalized : lastFace;
        anim.SetFloat("idleX", f.x);
        anim.SetFloat("idleY", f.y);
        anim.SetFloat("atkX", f.x);
        anim.SetFloat("atkY", f.y);
    }

    // -------- gizmos ----------
    void OnDrawGizmosSelected()
    {
        if (!target) return;

        Vector3 p = transform.position;

        // Charge range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, chargeRange);

        // Jump range (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p, jumpRange);

        // Jump AoE radius at current position (red)
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(p, jumpAoERadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p, jumpAoERadius);
    }
}
