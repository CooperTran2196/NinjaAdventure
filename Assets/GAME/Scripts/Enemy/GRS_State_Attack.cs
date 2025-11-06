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
    public float attackCooldown  = 1.10f;
    public float attackDuration  = 0.45f;
    public float hitDelay        = 0.25f;

    [Header("Special (single clip flow)")]
    public float specialCooldown   = 8.0f;
    public float specialClipLength = 1.50f;
    public float specialHitDelay   = 0.50f;   // Charge ends -> start dash
    public float hitTime           = 1.05f;   // First contact; follow-up auto

    [Header("Auto Move Window")]
    public float preHitStopBias = 0.02f;

    [Header("Dash")]
    public float dashSpeed       = 9.0f;
    public float stopShortOffset = 0.96f;     // Distance kept in front of player

    [Header("Alignment Gate")]
    public float yHardCap = 0.55f;            // Must be within this to attack

    // Animator params
    const string isAttacking     = "isAttacking";
    const string isSpecialAttack = "isSpecialAttack";

    // Runtime state
    Transform target;
    Vector2   lastFace = Vector2.right;
    float     attackRange;
    float     nextAttackReadyAt;
    float     nextSpecialReadyAt;
    bool      isDashing;

    // Dash runtime
    Vector2 dashDest;
    Vector2 dashDir;

    // Follow-up gap for continuous second swing
    const float followupGap = 0.14f;

    public bool IsAttacking { get; private set; }

    // Computed move window
    float ComputedMoveWindow => Mathf.Max(0f, hitTime - specialHitDelay - preHitStopBias);
    float TimeReach => dashSpeed * ComputedMoveWindow;

    void Awake()
    {
        rb           ??= GetComponent<Rigidbody2D>();
        anim         ??= GetComponentInChildren<Animator>();
        controller   ??= GetComponent<I_Controller>();
        activeWeapon ??= GetComponentInChildren<W_Base>();
        sr           ??= GetComponentInChildren<SpriteRenderer>();
        afterimage   ??= sr ? sr.GetComponent<C_AfterimageSpawner>() : null;
    }

    void OnDisable()
    {
        IsAttacking = false;
        isDashing   = false;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
        anim?.SetBool(isAttacking, false);
        anim?.SetBool(isSpecialAttack, false);
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

        // Gates
        bool inInner       = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        bool specialReady  = Time.time >= nextSpecialReadyAt;
        float inner        = attackRange * 1.2f;
        float outer        = attackRange + TimeReach;
        bool inSpecialDist = Mathf.Abs(dx) >= inner && Mathf.Abs(dx) <= outer;
        bool alignedY      = Mathf.Abs(dy) <= yHardCap;
        bool canAttackNow  = Time.time >= nextAttackReadyAt;

        // Priority: Special > Normal
        if (canAttackNow)
        {
            if (specialReady && alignedY && inSpecialDist)
            {
                StartCoroutine(SpecialRoutine(dir));
            }
            else if (alignedY && inInner)
            {
                StartCoroutine(NormalRoutine(dir));
            }
        }
    }

    // CONTROLLER HOOKS
    public void SetTarget(Transform t) => target = t;

    public void SetRanges(float attackRange) => this.attackRange = attackRange;

    public bool CanSpecialNow(Vector2 bossPos, Vector2 playerPos)
    {
        if (Time.time < nextSpecialReadyAt || Time.time < nextAttackReadyAt) return false;
        float dx = Mathf.Abs(playerPos.x - bossPos.x);
        float dy = Mathf.Abs(playerPos.y - bossPos.y);
        float inner = attackRange * 1.2f;
        float outer = attackRange + TimeReach;
        return dy <= yHardCap && dx >= inner && dx <= outer;
    }

    // ATTACK ROUTINES
    IEnumerator NormalRoutine(Vector2 dirAtStart)
    {
        IsAttacking = true;
        anim.SetBool(isSpecialAttack, false);
        anim.SetBool(isAttacking, true);

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        yield return new WaitForSeconds(hitDelay);

        activeWeapon?.Attack(lastFace);

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

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        float t = 0f;
        float endMoveTime = specialHitDelay + ComputedMoveWindow;

        // A) Charge
        while (t < specialHitDelay) { t += Time.deltaTime; yield return null; }

        // B) Gap-close to face spot
        BeginDash();
        while (t < endMoveTime)
        {
            t += Time.deltaTime;
            if (ReachedDashDest()) break;
            yield return null;
        }
        StopDash();

        // C) Hit + quick follow-up
        while (t < hitTime) { t += Time.deltaTime; yield return null; }
        activeWeapon?.Attack(lastFace);

        float t2 = 0f;
        while (t2 < followupGap) { t2 += Time.deltaTime; yield return null; }
        activeWeapon?.Attack(lastFace);

        // D) Finish clip
        while (t < specialClipLength) { t += Time.deltaTime; yield return null; }

        nextAttackReadyAt  = Time.time + attackCooldown;
        nextSpecialReadyAt = Time.time + specialCooldown;

        IsAttacking = false;
        anim.SetBool(isSpecialAttack, false);
    }

    // DASH SYSTEM

    void BeginDash()
    {
        Vector2 start = transform.position;
        Vector2 p     = target ? (Vector2)target.position : start;

        int sign = (p.x - start.x) >= 0f ? +1 : -1;
        Vector2 faceSpot = new Vector2(p.x - sign * stopShortOffset, p.y);

        Vector2 toFace   = faceSpot - start;
        float distToFace = toFace.magnitude;

        float travel = Mathf.Min(distToFace, TimeReach);
        dashDir  = toFace.sqrMagnitude > 0f ? toFace / distToFace : new Vector2(sign, 0f);
        dashDest = start + dashDir * travel;

        controller?.SetDesiredVelocity(dashDir * dashSpeed);
        isDashing = true;

        afterimage?.StartBurst(ComputedMoveWindow, sr.sprite, sr.flipX, sr.flipY);
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

        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p, attackRange);

        if (!target) return;

        // Special attack range visualization
        float inner = attackRange * 1.2f;
        float outer = attackRange + TimeReach;
        Vector3 tp = target.position;
        float dx = Mathf.Abs(tp.x - p.x);
        float dy = Mathf.Abs(tp.y - p.y);

        bool inSpecialDist = dx >= inner && dx <= outer;
        bool alignedY = dy <= yHardCap;

        // Inner threshold (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, inner);

        // Outer reach (magenta)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(p, outer);

        // Y-gate lines (cyan)
        Gizmos.color = Color.cyan;
        float lineX = 4f;
        Gizmos.DrawLine(p + new Vector3(-lineX, yHardCap, 0f), p + new Vector3(lineX, yHardCap, 0f));
        Gizmos.DrawLine(p + new Vector3(-lineX, -yHardCap, 0f), p + new Vector3(lineX, -yHardCap, 0f));

        // Target indicator
        bool validSpecial = inSpecialDist && alignedY;
        Gizmos.color = validSpecial ? Color.green : Color.red;
        Gizmos.DrawWireSphere(tp, 0.3f);
    }
}
