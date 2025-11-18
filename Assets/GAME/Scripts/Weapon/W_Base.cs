using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]

public abstract class W_Base : MonoBehaviour
{
    [Header("Central API for all weapons")]
    [Header("References")]
    protected SpriteRenderer sprite;
    protected BoxCollider2D hitbox;
    protected Animator ownerAnimator;
    protected C_Stats c_Stats;

    [Header("Weapon Data")]
    public W_SO weaponData;

    [Header("Owner + Targets")]
    public Transform owner;
    public LayerMask targetMask;

    [Header("Auto Sized Hitbox")]
    public bool autoSizeFromSprite = true;

    [Header("Debug")]
    [SerializeField] bool debugDrawHitbox = false;
    [SerializeField] Color debugHitboxColor = new Color(1f, 0.4f, 0.1f, 0.9f); // orange

    // Track original parent for anchoring during attacks
    Transform originalParent;

    void Awake()
    {
        // Cache components
        sprite ??= GetComponent<SpriteRenderer>();
        hitbox ??= GetComponent<BoxCollider2D>();

        originalParent = transform.parent;

        if (!sprite) Debug.LogError($"{name}: SpriteRenderer is missing in W_Base");
        if (!hitbox) Debug.LogError($"{name}: BoxCollider2D is missing in W_Base");

        // Collider setup
        hitbox.isTrigger = true;
        sprite.enabled   = false;
        hitbox.enabled   = false;

        // Owner references (use parent, not root, to support nested hierarchies)
        owner               = transform.parent;
        ownerAnimator       ??= owner ? owner.GetComponentInChildren<Animator>() : null;
        c_Stats             ??= owner ? owner.GetComponent<C_Stats>() : null;

        if (!owner)         Debug.LogError($"{name}: owner is missing in W_Base",this);
        if (!ownerAnimator) Debug.LogError($"{name}: ownerAnimator is missing in W_Base",this);
        if (!c_Stats)       Debug.LogError($"{name}: C_Stats is missing in W_Base",this);
        if (!weaponData)    Debug.LogWarning($"{name}: weaponData is missing in W_Base (weapon will be unusable)", this);

        // Visual + hitbox auto-sizing
        if (weaponData && sprite) sprite.sprite = weaponData.sprite;
        if (autoSizeFromSprite && sprite && sprite.sprite)
        {
            hitbox.size = sprite.sprite.bounds.size;
            
            // Shift hitbox UP to cover blade (sprite pivot at bottom)
            hitbox.offset = new Vector2(0f, hitbox.size.y * 0.5f);
        }
    }

    // Get position at offsetRadius along attackDir (LOCAL space)
    protected Vector3 GetPolarPosition(Vector2 attackDir) =>
        (Vector3)(attackDir * weaponData.offsetRadius);

    // Get rotation angle from attackDir (UP=0°, sprite points UP, pivot at BOTTOM)
    protected float GetPolarAngle(Vector2 attackDir)
    {
        return Vector2.SignedAngle(Vector2.up, attackDir);
    }

    // Show weapon at position/angle, enable hitbox
    protected void BeginVisual(Vector3 localPos, float angle, bool enableHitbox)
    {
        transform.SetParent(owner, false);  // Parent to owner for auto-follow
        
        transform.localPosition = localPos;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        
        sprite.enabled = true;
        hitbox.enabled = enableHitbox;
    }

    // Hide weapon + restore parent
    protected void EndVisual()
    {
        sprite.enabled = false;
        hitbox.enabled = false;
        transform.SetParent(originalParent, true);
    }

    // Thrust forward/back along dir over showTime (EASE-IN-OUT motion)
    protected IEnumerator ThrustOverTime(Vector2 dir, float showTime, float thrustDist)
    {
        // Calculate dynamic duration based on maxAttackSpeed
        // Combine weapon base speed + player speed bonus
        float totalSpeed = weaponData.maxAttackSpeed + c_Stats.attackSpeed;
        float speedMultiplier = 1.0f - (totalSpeed * 0.1f); // Linear: 1 = 10% faster
        float actualDuration = showTime * speedMultiplier;
        
        float t = 0f;
        Vector3 start = transform.localPosition - (Vector3)(dir * (thrustDist * 0.5f));
        Vector3 end = transform.localPosition + (Vector3)(dir * (thrustDist * 0.5f));

        while (t < actualDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / actualDuration);
            
            // Apply ease-in-out curve (slow → fast → slow)
            float easedK = EaseInOutCubic(k);
            
            transform.localPosition = Vector3.Lerp(start, end, easedK);
            yield return null;
        }
    }

    // Sweep weapon in arc from startAngle to endAngle (EASE-IN-OUT motion)
    // Weapon rotates like radar arm: handle orbits at offsetRadius, blade extends outward
    // REQUIRES: sprite points UP, pivot at BOTTOM
    protected IEnumerator ArcSlashOverTime(Vector2 attackDir, float startAngleDeg, float endAngleDeg, float duration)
    {
        // Calculate dynamic duration based on maxAttackSpeed
        // Combine weapon base speed + player speed bonus
        float totalSpeed = weaponData.maxAttackSpeed + c_Stats.attackSpeed;
        float speedMultiplier = 1.0f - (totalSpeed * 0.1f); // Linear: 1 = 10% faster
        float actualDuration = duration * speedMultiplier;
        
        float t = 0f;
        float radius = weaponData.offsetRadius;
        
        while (t < actualDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / actualDuration);
            
            // Apply ease-in-out curve (slow → fast → slow)
            float easedK = EaseInOutCubic(k);
            
            // Lerp angle from start to end using eased progress
            float currentAngleDeg = Mathf.Lerp(startAngleDeg, endAngleDeg, easedK);
            float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;
            
            // Polar to Cartesian (UP=0°): x=-sin(θ)*r, y=cos(θ)*r (negated X fixes Unity coords)
            Vector3 circularPosition = new Vector3(
                -Mathf.Sin(currentAngleRad) * radius,
                Mathf.Cos(currentAngleRad) * radius,
                0f
            );
            
            transform.localPosition = circularPosition;  // Sets initial position on first frame
            transform.localRotation = Quaternion.Euler(0, 0, currentAngleDeg);
            
            yield return null;
        }
    }
    
    // Ease-in-out cubic curve: slow → fast → slow
    protected float EaseInOutCubic(float t)
    {
        return t < 0.7f 
            ? 4f * t * t * t  // Ease-in: accelerate (first half)
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;  // Ease-out: decelerate (second half)
    }
    
    // Instance wrapper -> calls static version
    protected (C_Health target, GameObject root) TryGetTarget(Collider2D targetCollider)
        => TryGetTarget(owner, targetMask, targetCollider);

    // Static version for projectiles
    // Returns valid target C_Health or null (filters layer, owner, weapons, dead entities)
    public static (C_Health target, GameObject root)
                TryGetTarget(Transform owner, LayerMask targetMask, Collider2D targetCollider)
    {
        // Layer filter
        if ((targetMask.value & (1 << targetCollider.gameObject.layer)) == 0)
            return (null, null);

        // Ignore owner + weapon colliders
        if (targetCollider.transform == owner || targetCollider.transform.IsChildOf(owner))
            return (null, null);

        if (targetCollider.GetComponentInParent<W_Base>() != null)
            return (null, null);

        // Find target health on root
        var target = targetCollider.GetComponentInParent<C_Health>();
        if (target == null || !target.IsAlive)
            return (null, null);

        return (target, target.gameObject);
    }

    // Instance wrapper -> calls static version
    protected void ApplyHitEffects(C_Stats attackerStats, W_SO weaponData, C_Health targetHealth, Vector2 dir, Collider2D targetCollider, int comboIndex = 0)
                => ApplyHitEffects(attackerStats, weaponData, targetHealth, dir, targetCollider, this, comboIndex);

    // Apply damage + combo effects (damage/stun/knockback scale with comboIndex)
    public static void ApplyHitEffects(C_Stats attackerStats, W_SO weaponData, C_Health targetHealth,
                                        Vector2 dir, Collider2D targetCollider, MonoBehaviour weapon, int comboIndex = 0)
    {
        int attackerAD = attackerStats.AD, attackerAP = attackerStats.AP;
        
        // Scale weapon damage by combo multiplier
        int baseWeaponAD = weaponData.AD;
        int baseWeaponAP = weaponData.AP;
        
        float damageMultiplier = weaponData.comboDamageMultipliers[comboIndex];
        int weaponAD = Mathf.RoundToInt(baseWeaponAD * damageMultiplier);
        int weaponAP = Mathf.RoundToInt(baseWeaponAP * damageMultiplier);
        
        float attackerArmorPen = attackerStats.armorPen;
        float attackerMagicPen = attackerStats.magicPen;

        int dealtDamage = targetHealth.ApplyDamage(attackerAD, attackerAP, weaponAD, weaponAP, attackerArmorPen, attackerMagicPen);

        // Lifesteal: heal attacker based on damage dealt
        if (dealtDamage > 0 && attackerStats.lifesteal > 0)
        {
            var attackerHealth = attackerStats.GetComponent<C_Health>();
            if (attackerHealth != null)
            {
                int healAmount = Mathf.RoundToInt(dealtDamage * attackerStats.lifesteal);
                if (healAmount > 0)
                {
                    attackerHealth.ChangeHealth(healAmount);
                }
            }
        }

        // Knockback (only thrust if onlyThrustKnocksBack=true)
        bool shouldKnockback = !weaponData.onlyThrustKnocksBack || comboIndex == 2;
        if (shouldKnockback && weaponData.knockbackForce > 0f)
        {
            var ec = targetCollider.GetComponentInParent<E_Controller>();
            var pc = targetCollider.GetComponentInParent<P_Controller>();
            
            if (ec != null)
            {
                ec.SetKnockback(dir * weaponData.knockbackForce);
            }
            else if (pc != null)
            {
                pc.SetKnockback(dir * weaponData.knockbackForce);
            }
            else
            {
                // Fallback for NPCs without controller
                var rb = targetCollider.GetComponentInParent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.AddForce(dir * weaponData.knockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        // Stun (duration from combo-specific array, with bonus: 1 = 1% increase)
        float baseStunTime = weaponData.comboStunTimes[comboIndex];
        float finalStunTime = baseStunTime * (1f + attackerStats.stunTimeBonus / 100f);
        
        if (finalStunTime > 0f)
        {
            var ec = targetCollider.GetComponentInParent<E_Controller>();
            var pc = targetCollider.GetComponentInParent<P_Controller>();
            
            if (ec != null)
            {
                ec.StartCoroutine(ec.SetStunTime(finalStunTime));
            }
            else if (pc != null)
            {
                pc.StartCoroutine(pc.SetStunTime(finalStunTime));
            }
        }
    }

    // Called by owner when attacking
    public abstract void Attack(Vector2 attackDir);

    // Debug: draw hitbox when active
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



