using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class W_Base : MonoBehaviour
{
    [Header("References")]
    protected SpriteRenderer sprite;
    protected BoxCollider2D hitbox;
    protected Animator ownerAnimator;
    protected C_Stats c_Stats;

    [Header("Data")]
    public W_SO data;

    [Header("Owner + Targets")]
    public Transform owner;
    public LayerMask targetMask;

    [Header("Hitbox Auto")]
    public bool autoSizeFromSprite = true;

    [Header("Debug")]
    [SerializeField] bool debugDrawHitbox = false;
    [SerializeField] Color debugHitboxColor = new Color(1f, 0.4f, 0.1f, 0.9f); // orange-red

    const float MIN_DISTANCE = 0.001f;

    protected virtual void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        hitbox ??= GetComponent<BoxCollider2D>();
        hitbox.isTrigger = true;

        // Hide by default and shown only during Attack
        if (sprite) sprite.enabled = false;
        if (hitbox) hitbox.enabled = false;

        owner ??= transform.root;
        ownerAnimator ??= owner ? owner.GetComponent<Animator>() : null;
        c_Stats ??= owner ? owner.GetComponent<C_Stats>() : null;

        if (data && sprite) sprite.sprite = data.sprite;
        if (autoSizeFromSprite && sprite && sprite.sprite && hitbox)
        {
            hitbox.size = sprite.sprite.bounds.size;
            hitbox.offset = Vector2.zero;
        }
    }

    public virtual void Equip(Transform newOwner)
    {
        owner = newOwner;
        ownerAnimator = owner ? owner.GetComponent<Animator>() : null;
        c_Stats = owner ? owner.GetComponent<C_Stats>() : null;
        if (data && sprite) sprite.sprite = data.sprite;
    }

    public void SetData(W_SO d, bool applySprite = true)
    {
        data = d;
        if (applySprite && data && sprite) sprite.sprite = data.sprite;
    }

    // Read atkX/atkY; normalize if above threshold; else default to down.
    // Always returns a clean 8-way vector from atkX/atkY, or Down if unset
    protected Vector2 GetAimDir()
    {
        if (!ownerAnimator) return Vector2.down;

        float ax = ownerAnimator.GetFloat("atkX");
        float ay = ownerAnimator.GetFloat("atkY");
        Vector2 v = new Vector2(ax, ay);

        if (v.sqrMagnitude <= (MIN_DISTANCE * MIN_DISTANCE)) return Vector2.down;
        return SnapToEightDirections(v);
    }


    // Choose 8 offsets from SO
    protected Vector2 GetSpawnOffset(Vector2 snappedDir)
    {
        if (!data) return Vector2.zero;

        float t = MIN_DISTANCE;
        bool hasX = Mathf.Abs(snappedDir.x) > t;
        bool hasY = Mathf.Abs(snappedDir.y) > t;

        if (hasX && hasY)
        {
            if (snappedDir.x >= 0f && snappedDir.y >= 0f) return data.offsetUpRight;
            if (snappedDir.x < 0f && snappedDir.y >= 0f) return data.offsetUpLeft;
            if (snappedDir.x >= 0f && snappedDir.y < 0f) return data.offsetDownRight;
            return data.offsetDownLeft;
        }
        if (hasX) return (snappedDir.x >= 0f) ? data.offsetRight : data.offsetLeft;
        return (snappedDir.y >= 0f) ? data.offsetUp : data.offsetDown;
    }

    // Shared math utility for all characters
    protected static Vector2 SnapToEightDirections(Vector2 direction)
    {
        if (direction.sqrMagnitude < 1e-9f) return Vector2.down;
        float degrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int octantIndex = Mathf.RoundToInt(degrees / 45f);
        float radians = octantIndex * 45f * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }
    // Consistent sprite rotation based on pointsUp convention
    protected float GetVisualAngle(Vector2 dir)
    {
        Vector2 baseline = data.pointsUp ? Vector2.up : Vector2.down;
        return Vector2.SignedAngle(baseline, dir);
    }

    // Position/rotate + show sprite; optionally enable hitbox
    protected void BeginVisual(Vector3 pos, float angle, bool enableHitbox)
    {
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        if (sprite) sprite.enabled = true;
        if (hitbox) hitbox.enabled = enableHitbox;
    }

    // Move forward/back along dir over showTime; call onProgress(k) if provided
    protected System.Collections.IEnumerator ThrustOverTime(
        Vector2 dir, float showTime, float thrustDist, System.Action<float> onProgress = null)
    {
        float t = 0f;
        Vector3 start = transform.position - (Vector3)(dir * (thrustDist * 0.5f));
        Vector3 end   = transform.position + (Vector3)(dir * (thrustDist * 0.5f));

        while (t < showTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / showTime);
            transform.position = Vector3.Lerp(start, end, k);
            onProgress?.Invoke(k);
            yield return null;
        }
        // Caller will hide sprite/hitbox.
    }
// --- STATIC versions (usable from W_Projectile) ---
public static bool TryGetTarget(Transform owner, LayerMask targetMask, Collider2D other,
                                out C_Health target, out GameObject root)
{
    if ((targetMask.value & (1 << other.gameObject.layer)) == 0) { target = null; root = null; return false; }
    if (other.transform == owner || other.transform.IsChildOf(owner)) { target = null; root = null; return false; }
    if (other.GetComponentInParent<W_Base>() != null) { target = null; root = null; return false; }

    target = other.GetComponentInParent<C_Health>();
    if (target == null || !target.IsAlive) { root = null; return false; }

    root = target.gameObject;
    return true;
}

public static void ApplyHitEffects(C_Stats attacker, W_SO data,
                                   C_Health target, Vector2 dir, Collider2D hitCol, MonoBehaviour host)
{
    int attackerAD = attacker.AD, attackerAP = attacker.AP;
    int weaponAD   = data.baseAD, weaponAP   = data.baseAP;

    target.ApplyDamage(attackerAD, attackerAP, weaponAD, weaponAP);

    if (data.knockbackForce > 0f)
        W_Knockback.PushTarget(hitCol.gameObject, dir, data.knockbackForce);

    if (data.stunTime > 0f)
    {
        var pm = hitCol.GetComponentInParent<P_Movement>();
        if (pm) { host.StartCoroutine(W_Stun.Apply(pm, data.stunTime)); }
        else
        {
            var em = hitCol.GetComponentInParent<E_Movement>();
            if (em) { host.StartCoroutine(W_Stun.Apply(em, data.stunTime)); }
        }
    }
}

// --- INSTANCE convenience wrappers (usable from W_Melee / W_Ranged) ---
protected bool TryGetTarget(Collider2D other, out C_Health target, out GameObject root)
    => TryGetTarget(owner, targetMask, other, out target, out root);

protected void ApplyHitEffects(C_Stats attacker, W_SO d, C_Health target, Vector2 dir, Collider2D hitCol)
    => ApplyHitEffects(attacker, d, target, dir, hitCol, this);




    public abstract void Attack();

    void OnDrawGizmos()
    {
        if (!debugDrawHitbox) return;

        // Use cached if available; fall back to GetComponent for edit-time draws
        var sr = sprite ? sprite : GetComponent<SpriteRenderer>();
        var bc = hitbox ? hitbox : GetComponent<BoxCollider2D>();
        if (!bc) return;

        // Only draw while the weapon is live: sprite visible OR collider enabled
        if ((sr && sr.enabled) || bc.enabled)
        {
            var prevMatrix = Gizmos.matrix;
            var prevColor = Gizmos.color;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = debugHitboxColor;

            // Fill (very faint) + outline so it’s easy to see
            var fill = debugHitboxColor; fill.a = 0.12f;
            Gizmos.color = fill; Gizmos.DrawCube(bc.offset, bc.size);
            Gizmos.color = debugHitboxColor; Gizmos.DrawWireCube(bc.offset, bc.size);

            Gizmos.matrix = prevMatrix;
            Gizmos.color = prevColor;
        }
    }
}



