using System.Collections;
using UnityEngine;

public class State_Attack_Boss : MonoBehaviour
{
    [Header("Target Layer")]
    public LayerMask playerLayer;

    [Header("Normal Attack")]
    public float attackCooldown = 1.10f;
    float attackDuration = 0.45f;
    float hitDelay = 0.25f;

    [Header("Special (single clip flow)")]
    public float specialCooldown   = 8.0f;
    public float specialClipLength = 1.50f;
    public float specialHitDelay   = 0.50f;   // charge ends → start dash
    public float hitTime           = 1.05f;   // first contact; follow-up auto

    [Header("Auto Move Window")]
    [Tooltip("Dash ends slightly before the hit for fairness.")]
    public float preHitStopBias = 0.02f;      // 10–30 ms typical

    [Header("Dash")]
    public float dashSpeed       = 9.0f;
    public float stopShortOffset = 0.96f;     // distance kept in front of the player (along X) at the end

    [Header("Alignment Gate")]
    public float yHardCap = 0.55f;            // must be within this to attack

    [Header("Weapon")]
    public W_Base activeWeapon;               // W_Melee, no sprite

    // Animator params
    const string kIsAttacking     = "isAttacking";
    const string kIsSpecialAttack = "isSpecialAttack";

    // Cache
    Rigidbody2D rb;
    Animator anim;
    I_Controller controller;
    SpriteRenderer sr;
    C_AfterimageSpawner afterimage;

    // Runtime
    Transform target;
    Vector2 lastFace = Vector2.right;
    float attackRange = 1.6f;

    // Local cooldowns
    float nextAttackReadyAt;
    float nextSpecialReadyAt;

    // Status
    public bool IsAttacking { get; private set; }
    bool isDashing;

    // follow-up gap for the continuous second swing
    const float kFollowupGap = 0.14f;

    // ---- Computed move window (time & reach) ----
    float ComputedMoveWindow
    {
        get
        {
            float w = hitTime - specialHitDelay - preHitStopBias;
            return (w > 0f) ? w : 0f;
        }
    }
    float TimeReach => dashSpeed * ComputedMoveWindow;

    void Awake()
    {
        rb         ??= GetComponent<Rigidbody2D>();
        anim       ??= GetComponentInChildren<Animator>();
        controller ??= GetComponent<I_Controller>();
        activeWeapon ??= GetComponentInChildren<W_Base>();
        sr         ??= GetComponentInChildren<SpriteRenderer>();
        afterimage ??= sr ? sr.GetComponent<C_AfterimageSpawner>() : null;
    }

    void OnDisable()
    {
        IsAttacking = false;
        isDashing   = false;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
        anim.SetBool(kIsAttacking, false);
        anim.SetBool(kIsSpecialAttack, false);
    }

    void Update()
    {
        if (!isDashing) controller?.SetDesiredVelocity(Vector2.zero);
        if (!target) return;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float dx = to.x, dy = to.y;
        float d  = to.magnitude;

        Vector2 dir = d > 0.0001f ? to.normalized : lastFace;
        UpdateIdleFacing(IsAttacking ? lastFace : dir);

        // gates
        bool inInner       = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        bool specialReady  = Time.time >= nextSpecialReadyAt;
        // new outer uses timing reach (no magenta cap)
        float inner = attackRange * 1.2f;
        float outer = attackRange + TimeReach;
        bool inSpecialDist = Mathf.Abs(dx) >= inner && Mathf.Abs(dx) <= outer;
        bool alignedY      = Mathf.Abs(dy) <= yHardCap;
        bool canAttackNow  = Time.time >= nextAttackReadyAt;

        if (!IsAttacking && canAttackNow)
        {
            if (specialReady && alignedY && inSpecialDist)
            {
                StartCoroutine(SpecialRoutine(dir));
                return;
            }
            if (alignedY && inInner)
            {
                StartCoroutine(NormalRoutine(dir));
                return;
            }
        }
    }

    // -------- controller hooks ----------
    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float attackRange) => this.attackRange = attackRange;

    // Let B_Controller ask if special is valid right now (uses timing reach + Y gate)
    public bool CanSpecialNow(Vector2 bossPos, Vector2 playerPos)
    {
        if (Time.time < nextSpecialReadyAt || Time.time < nextAttackReadyAt) return false;
        float dx = Mathf.Abs(playerPos.x - bossPos.x);
        float dy = Mathf.Abs(playerPos.y - bossPos.y);
        float inner = attackRange * 1.2f;
        float outer = attackRange + TimeReach;
        return dy <= yHardCap && dx >= inner && dx <= outer;
    }

    // -------- normal attack ----------
    IEnumerator NormalRoutine(Vector2 dirAtStart)
    {
        IsAttacking = true;
        anim.SetBool(kIsSpecialAttack, false);
        anim.SetBool(kIsAttacking, true);

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        yield return new WaitForSeconds(hitDelay);

        activeWeapon?.Attack(lastFace);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));

        nextAttackReadyAt = Time.time + attackCooldown;
        IsAttacking = false;
        anim.SetBool(kIsAttacking, false);
    }

    // -------- special (one clip; timers only) ----------
    IEnumerator SpecialRoutine(Vector2 dirAtStart)
    {
        IsAttacking = true;
        anim.SetBool(kIsAttacking, false);
        anim.SetBool(kIsSpecialAttack, true);

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);
        UpdateIdleFacing(lastFace);

        float t = 0f;
        float endMoveTime = specialHitDelay + ComputedMoveWindow;

        // A) Charge
        while (t < specialHitDelay) { t += Time.deltaTime; yield return null; }

        // B) Gap-close along vector to the "face spot"
        BeginDash(); // sets velocity and kicks afterimage
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
        while (t2 < kFollowupGap) { t2 += Time.deltaTime; yield return null; }
        activeWeapon?.Attack(lastFace);

        // D) Finish clip
        while (t < specialClipLength) { t += Time.deltaTime; yield return null; }

        nextAttackReadyAt  = Time.time + attackCooldown;   // reuse normal pacing after special
        nextSpecialReadyAt = Time.time + specialCooldown;

        IsAttacking = false;
        anim.SetBool(kIsSpecialAttack, false);
    }

    // -------- dash helpers ----------
    Vector2 dashDest;
    Vector2 dashDir;

    void BeginDash()
    {
        // “Face spot”: X is right in front of player (stop-short), Y is exactly player’s Y.
        Vector2 start = transform.position;
        Vector2 p     = target ? (Vector2)target.position : start;

        int sign = (p.x - start.x) >= 0f ? +1 : -1;
        Vector2 faceSpot = new Vector2(p.x - sign * stopShortOffset, p.y);

        Vector2 toFace   = faceSpot - start;
        float distToFace = toFace.magnitude;

        float travel = Mathf.Min(distToFace, TimeReach); // no magenta cap
        dashDir = (toFace.sqrMagnitude > 0f) ? (toFace / distToFace) : new Vector2(sign, 0f);
        dashDest = start + dashDir * travel;

        // velocity & afterimage
        controller?.SetDesiredVelocity(dashDir * dashSpeed);
        isDashing = true;

        if (afterimage && sr)
            afterimage.StartBurst(ComputedMoveWindow, sr.sprite, sr.flipX, sr.flipY);
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
        // stop when we pass the destination along the dash direction
        Vector2 toDest = dashDest - pos;
        return Vector2.Dot(toDest, dashDir) <= 0f || (toDest.sqrMagnitude <= 0.0004f);
    }

    // -------- anim lattice ----------
    void UpdateIdleFacing(Vector2 faceDir)
    {
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        Vector2 f = faceDir.sqrMagnitude > 0f ? faceDir.normalized : lastFace;
        anim.SetFloat("idleX", f.x);
        anim.SetFloat("idleY", f.y);
    }

    // -------- gizmos ----------
    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;

        // Y hard cap band (cyan)
        Gizmos.color = Color.cyan;
        float band = yHardCap;
        float width = (attackRange + TimeReach + 0.5f) * 2f;
        Vector3 L  = p + Vector3.left  * width * 0.5f;
        Vector3 R  = p + Vector3.right * width * 0.5f;
        Gizmos.DrawLine(L + Vector3.up * band,   R + Vector3.up * band);
        Gizmos.DrawLine(L + Vector3.down * band, R + Vector3.down * band);

        // Time-limited dash reach (blue) along current face vector (from boss toward face spot)
        if (target)
        {
            int sign = ((target.position.x - p.x) >= 0f) ? +1 : -1;
            Vector2 faceSpot = new Vector2(target.position.x - sign * stopShortOffset, target.position.y);
            Vector2 toFace   = faceSpot - (Vector2)p;
            Vector2 dir      = toFace.sqrMagnitude > 0f ? toFace.normalized : new Vector2(sign, 0f);
            float reach      = TimeReach;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(p, p + (Vector3)(dir * reach));
        }

        // Special distance window (thick “dent dots” on X)
        float inner = attackRange * 1.2f;
        float outer = attackRange + TimeReach;
        float dotR  = 0.18f;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(p + Vector3.right * inner, dotR);
        Gizmos.DrawSphere(p + Vector3.left  * inner, dotR);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(p + Vector3.right * outer, dotR);
        Gizmos.DrawSphere(p + Vector3.left  * outer, dotR);

        // Stop-short line relative to the player (yellow)
        if (target)
        {
            int sign = ((target.position.x - p.x) >= 0f) ? +1 : -1;
            float stopX = target.position.x - sign * stopShortOffset;
            Vector3 a = new Vector3(stopX, p.y - 0.6f, p.z);
            Vector3 b = new Vector3(stopX, p.y + 0.6f, p.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(a, b);
        }
    }
}
