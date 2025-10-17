using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]

public class W_ProjectileHoming : MonoBehaviour
{
    // Cache
    SpriteRenderer sprite;
    BoxCollider2D col;
    Rigidbody2D rb;

    // Runtime (same as W_Projectile)
    Transform owner;
    C_Stats attackerStats;
    W_SO weaponData;
    LayerMask targetMask;
    Vector2 fireDir;

    int remainingPierces;
    readonly HashSet<int> alreadyHit = new HashSet<int>();
    Vector2 lastHitDir; // Store hit direction for stick positioning

    // Homing settings
    [Header("Homing Settings")]
    [SerializeField] float homingRange = 5f;
    [SerializeField] float homingStrength = 180f; // degrees per second turn rate
    [SerializeField] float homingDelay = 0.1f; // start homing after this delay

    [Header("Visual Settings")]
    [SerializeField] bool spinWhileFlying = false; // toggle for shuriken spin
    [SerializeField] float spinSpeed = 2f; // rotations per second (2 = 2 full rotations/sec)

    Transform currentTarget;
    bool canHome = false;
    bool isStuck = false; // Flag to stop spinning when stuck

    void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        col    ??= GetComponent<BoxCollider2D>();
        rb     ??= GetComponent<Rigidbody2D>();

        if (!sprite) Debug.LogError($"{name}: SpriteRenderer is missing in W_HomingProjectile");
        if (!col)    Debug.LogError($"{name}: BoxCollider2D is missing in W_HomingProjectile");
        if (!rb)     Debug.LogError($"{name}: Rigidbody2D is missing in W_HomingProjectile");
    }

    // Called by W_Ranged when spawning
    public void Init(Transform owner, C_Stats attackerStats, W_SO weaponData, Vector2 fireDir, LayerMask targetMask)
    {
        this.owner = owner;
        this.attackerStats = attackerStats;
        this.weaponData = weaponData;
        this.fireDir = fireDir.normalized;
        this.targetMask = targetMask;
        this.remainingPierces = weaponData.pierceCount;

        // Set initial velocity (same as W_Projectile)
        rb.linearVelocity = this.fireDir * weaponData.projectileSpeed;

        // Auto-destroy after lifetime
        if (weaponData.projectileLifetime > 0f)
            Destroy(gameObject, weaponData.projectileLifetime);

        // Start homing after delay
        StartCoroutine(EnableHomingAfterDelay());
    }

    IEnumerator EnableHomingAfterDelay()
    {
        yield return new WaitForSeconds(homingDelay);
        canHome = true;
    }

    void FixedUpdate()
    {
        // Handle spinning visual (independent of homing, but stop when stuck)
        if (spinWhileFlying && !isStuck)
        {
            float currentZ = transform.rotation.eulerAngles.z;
            float degreesPerSecond = spinSpeed * 360f; // convert rotations/sec to degrees/sec
            float newZ = currentZ + (degreesPerSecond * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, 0, newZ);
        }
        else if (canHome)
        {
            // Only update rotation for homing if not spinning
            UpdateHomingRotation();
        }

        // Always update homing velocity (even if spinning)
        if (canHome)
        {
            UpdateHomingVelocity();
        }
    }

    void UpdateHomingVelocity()
    {
        // Find closest enemy in range
        currentTarget = FindClosestTarget();

        if (currentTarget != null)
        {
            // Calculate desired direction to target
            Vector2 toTarget = (currentTarget.position - transform.position).normalized;
            Vector2 currentDir = rb.linearVelocity.normalized;

            // Smoothly rotate towards target
            float angle = Vector2.SignedAngle(currentDir, toTarget);
            float maxTurn = homingStrength * Time.fixedDeltaTime;
            float turnAmount = Mathf.Clamp(angle, -maxTurn, maxTurn);

            // Apply rotation to velocity
            Vector2 newDir = Rotate(currentDir, turnAmount);
            rb.linearVelocity = newDir * weaponData.projectileSpeed;
        }
    }

    void UpdateHomingRotation()
    {
        // Update visual rotation to match velocity direction (for kunai/arrows)
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Vector2 vel = rb.linearVelocity.normalized;
            float visualAngle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, visualAngle);
        }
    }

    Transform FindClosestTarget()
    {
        // Find all potential targets in range
        var hits = Physics2D.OverlapCircleAll(transform.position, homingRange, targetMask);
        
        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            // Skip owner
            if (hit.transform == owner || hit.transform.IsChildOf(owner))
                continue;

            // Skip already-hit targets (if not piercing)
            var root = hit.transform.root;
            if (remainingPierces <= 0 && alreadyHit.Contains(root.GetInstanceID()))
                continue;

            // Skip dead/invalid targets
            var health = hit.GetComponentInParent<C_Health>();
            if (!health || !health.IsAlive)
                continue;

            // Check distance
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }

    Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    void OnTriggerEnter2D(Collider2D targetCollider)
    {
        // Same hit detection as W_Projectile
        var (targetHealth, root) = W_Base.TryGetTarget(owner, targetMask, targetCollider);
        if (targetHealth == null) return;
        if (!alreadyHit.Add(root.GetInstanceID())) return;

        // Calculate hit direction
        Vector2 hitDir = (targetCollider.transform.position - transform.position).normalized;
        lastHitDir = hitDir; // Store for stick positioning

        // Apply damage + effects
        W_Base.ApplyHitEffects(attackerStats, weaponData, targetHealth, hitDir, targetCollider, this);

        // Handle pierce
        if (remainingPierces > 0)
        {
            remainingPierces--;
            return; // continue flying
        }

        // Stick or destroy (matching W_Projectile implementation)
        if (weaponData.stickOnHit > 0f)
        {
            StartCoroutine(FadeAndDestroy(targetCollider, weaponData.stickOnHit));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator FadeAndDestroy(Collider2D targetCollider, float fadeDuration)
    {
        // Stop spinning when stuck
        isStuck = true;

        // Compute a precise surface point on the hit collider
        var dist = col.Distance(targetCollider);          // distance info between our collider and hit collider
        Vector2 snap = dist.pointB;               // point on the *hit* collider
        transform.position = snap + (lastHitDir * 0.02f); // tiny embed so it visually "bites" in

        // Pin it: stop physics & collisions, then parent to the collider
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;
        transform.SetParent(targetCollider.transform, true);

        // Optional: ensure it renders above the target while stuck
        sprite.sortingOrder += 1;

        // Fade over seconds
        Color c = sprite.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / fadeDuration);
            sprite.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        Destroy(gameObject);
    }
}