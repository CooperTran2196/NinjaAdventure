using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class W_Projectile : MonoBehaviour
{
    // Cache
    SpriteRenderer sprite;
    BoxCollider2D col;
    Rigidbody2D rb;

    // Runtime
    Transform owner;
    C_Stats attackerStats;
    W_SO data;
    LayerMask targetMask;
    Vector2 moveDir;
    int remainingPierces;
    readonly HashSet<int> hitOnce = new HashSet<int>();

    void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        col    ??= GetComponent<BoxCollider2D>();
        rb     ??= GetComponent<Rigidbody2D>();

        col.isTrigger = true;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Init(Transform owner, C_Stats attackerStats, W_SO data, Vector2 dir, LayerMask mask)
    {
        this.owner = owner;
        this.attackerStats = attackerStats;
        this.data = data;
        this.moveDir = dir.normalized;
        this.targetMask = mask;
        this.remainingPierces = Mathf.Max(0, data.pierceCount);

        // Keep prefab’s arrow sprite (don’t overwrite with data.sprite which is bow art)
        rb.linearVelocity = moveDir * data.projectileSpeed;

        if (data.projectileLifetime > 0f)
            Destroy(gameObject, data.projectileLifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // FILTER via W_Base static
        if (!W_Base.TryGetTarget(owner, targetMask, other, out var targetHealth, out var root)) return;

        // per-projectile de-dup
        if (!hitOnce.Add(root.GetInstanceID())) return;

        // EFFECTS via W_Base static
        W_Base.ApplyHitEffects(attackerStats, data, targetHealth, moveDir, other, this);

        // <-- Add this block: consume pierce budget or stop now
        if (remainingPierces > 0)
        {
            remainingPierces--;      // pass through this target
            return;                  // keep flying
        }

        // No more pierce: stick (if enabled) or destroy
        if (data.stickOnHitSeconds > 0f)
        {
            StartCoroutine(StickAndDie(other, data.stickOnHitSeconds));
        }
        else
        {
            Destroy(gameObject);
        }

    }



    IEnumerator StickAndDie(Collider2D hitCol, float seconds)
    {
        // Compute a precise surface point on the hit collider
        var dist = col.Distance(hitCol);          // distance info between our collider and hit collider
        Vector2 snap = dist.pointB;               // point on the *hit* collider
        transform.position = snap + (moveDir * 0.02f); // tiny embed so it visually "bites" in

        // Pin it: stop physics & collisions, then parent to the *collider* (so it rides the sprite/bone)
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        col.enabled = false;
        transform.SetParent(hitCol.transform, true);

        // Optional: ensure it renders above the target while stuck
        if (sprite) sprite.sortingOrder += 1;

        // Fade over 'seconds'
        if (sprite)
        {
            Color c = sprite.color;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float a = 1f - Mathf.Clamp01(t / seconds);
                sprite.color = new Color(c.r, c.g, c.b, a);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}
