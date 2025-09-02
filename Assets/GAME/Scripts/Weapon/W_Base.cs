using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class W_Base : MonoBehaviour
{
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
    // Cached
    protected SpriteRenderer sprite;
    protected BoxCollider2D hitbox;
    protected Animator ownerAnimator;
    protected P_Stats pStats;
    protected E_Stats eStats;

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
        pStats ??= owner ? owner.GetComponent<P_Stats>() : null;
        eStats ??= owner ? owner.GetComponent<E_Stats>() : null;

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
        pStats = owner ? owner.GetComponent<P_Stats>() : null;
        eStats = owner ? owner.GetComponent<E_Stats>() : null;
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


    // Choose one of the 8 offsets from SO
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


    // Shared math utility (same as enemy)
    protected static Vector2 SnapToEightDirections(Vector2 direction)
    {
        if (direction.sqrMagnitude < 1e-9f) return Vector2.down;
        float degrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int octantIndex = Mathf.RoundToInt(degrees / 45f);
        float radians = octantIndex * 45f * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }


    public abstract void Attack();

    void OnDrawGizmos()
    {
        if (!debugDrawHitbox) return;

        // Use cached if available; fall back to GetComponent for edit-time draws
        var sr = sprite ? sprite : GetComponent<SpriteRenderer>();
        var bc = hitbox ? hitbox : GetComponent<BoxCollider2D>();
        if (!bc) return;

        // Only draw while the weapon is "live": sprite visible OR collider enabled
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



