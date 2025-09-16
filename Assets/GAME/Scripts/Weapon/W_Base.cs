using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class W_Base : MonoBehaviour
{
    [Header("References")]
    protected SpriteRenderer sprite;
    protected BoxCollider2D hitbox;
    protected Animator ownerAnimator;
    protected C_Stats c_Stats;

    [Header("Weapon Data")]
    public W_SO data;

    [Header("Owner + Targets")]
    public Transform owner;
    public LayerMask targetMask;

    [Header("Hitbox Auto")]
    public bool autoSizeFromSprite = true;

    [Header("Debug")]
    [SerializeField] bool debugDrawHitbox = false;
    [SerializeField] Color debugHitboxColor = new Color(1f, 0.4f, 0.1f, 0.9f); // orange

    const float MIN_DISTANCE = 0.001f;

    protected virtual void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        hitbox ??= GetComponent<BoxCollider2D>();

        if (!sprite) Debug.LogError($"{name}: SpriteRenderer missing on {gameObject.name}", this);
        if (!hitbox) Debug.LogError($"{name}: BoxCollider2D missing on {gameObject.name}", this);

        hitbox.isTrigger = true;

        // Hide by default and shown only during Attack
        sprite.enabled = false;
        hitbox.enabled = false;

        // Owner is always the root
        owner = transform.root;
        ownerAnimator = owner.GetComponent<Animator>();
        c_Stats = owner.GetComponent<C_Stats>();

        if (!owner) Debug.LogError($"{name}: Owner (root transform) not found for {gameObject.name}", this);
        if (!ownerAnimator) Debug.LogError($"{name}: Animator missing on owner {owner.name}", this);
        if (!c_Stats) Debug.LogError($"{name}: C_Stats missing on owner {owner.name}", this);
        if (!data) Debug.LogError($"{name}: W_SO data is not assigned on {gameObject.name}", this);

        // Always apply the data sprite
        sprite.sprite = data.sprite;

        // Auto size the weapon hitbox
        if (autoSizeFromSprite)
        {
            hitbox.size = sprite.sprite.bounds.size;
            hitbox.offset = Vector2.zero;
        }
    }

    protected Vector3 PolarPosition(Vector2 rawAim) =>
        owner.position + (Vector3)(rawAim * data.offsetRadius);

    protected float PolarAngle(Vector2 rawAim)
    {
        Vector2 baseline = data.pointsUp ? Vector2.up : Vector2.down;
        return Vector2.SignedAngle(baseline, rawAim) + data.angleBiasDeg;
    }

    // Position/rotate + show sprite, optionally enable hitbox
    protected void BeginVisual(Vector3 pos, float angle, bool enableHitbox)
    {
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        sprite.enabled = true;
        hitbox.enabled = enableHitbox;
    }

    // Move forward/back along dir over showTime, call onProgress if ranged
    protected IEnumerator ThrustOverTime(
        Vector2 dir, float showTime, float thrustDist, System.Action<float> onProgress = null)
    {
        float t = 0f;
        Vector3 start = transform.position - (Vector3)(dir * (thrustDist * 0.5f));
        Vector3 end = transform.position + (Vector3)(dir * (thrustDist * 0.5f));

        while (t < showTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / showTime);
            transform.position = Vector3.Lerp(start, end, k);
            onProgress?.Invoke(k);
            yield return null;
        }
    }

    // INSTANCE convenience wrappers for from W_Melee / W_Ranged
    protected (C_Health target, GameObject root) TryGetTarget(Collider2D other)
        => TryGetTarget(owner, targetMask, other);

    // STATIC versions for W_Projectile
    public static (C_Health target, GameObject root)
                TryGetTarget(Transform owner, LayerMask targetMask, Collider2D other)
    {
        // Layer filter
        if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            return (null, null);

        // Ignore owner
        if (other.transform == owner || other.transform.IsChildOf(owner))
            return (null, null);

        // Ignore weapon–weapon contacts
        if (other.GetComponentInParent<W_Base>() != null)
            return (null, null);

        // Find target health on other's root
        var target = other.GetComponentInParent<C_Health>();
        if (target == null || !target.IsAlive)
            return (null, null);

        // Success -> return C_Health and GameObject
        return (target, target.gameObject);
    }

    // INSTANCE convenience wrappers for from W_Melee / W_Ranged
    protected void ApplyHitEffects(C_Stats attacker, W_SO d, C_Health target, Vector2 dir, Collider2D hitCol)
        => ApplyHitEffects(attacker, d, target, dir, hitCol, this);

    public static void ApplyHitEffects(C_Stats attacker, W_SO data,
                                       C_Health target, Vector2 dir, Collider2D hitCol, MonoBehaviour weapon)
    {
        int attackerAD = attacker.AD, attackerAP = attacker.AP;
        int weaponAD = data.AD, weaponAP = data.AP;
        int dealt = target.ApplyDamage(attackerAD, attackerAP, weaponAD, weaponAP);

        var pStatsChanged = attacker.GetComponent<P_StatsChanged>();
        if (pStatsChanged != null && dealt > 0)
        {
            pStatsChanged.OnDealtDamage(dealt);
        }

        if (data.knockbackForce > 0f)
            W_Knockback.PushTarget(hitCol.gameObject, dir, data.knockbackForce);

        if (data.stunTime > 0f)
        {
            var pm = hitCol.GetComponentInParent<P_Movement>();
            if (pm) { weapon.StartCoroutine(W_Stun.Apply(pm, data.stunTime)); }
            else
            {
                var em = hitCol.GetComponentInParent<E_Movement>();
                if (em) { weapon.StartCoroutine(W_Stun.Apply(em, data.stunTime)); }
            }
        }
    }

    public abstract void Attack(Vector2 dir);

    public virtual void Equip(Transform newOwner)
    {
        owner = newOwner;
        ownerAnimator = owner.GetComponent<Animator>();
        c_Stats = owner.GetComponent<C_Stats>();
    }

    public void SetData(W_SO d, bool applySprite = true)
    {
        data = d;
        if (applySprite && data && sprite) sprite.sprite = data.sprite;
    }
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



