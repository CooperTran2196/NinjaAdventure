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
        // 1/ Cache weapon components
        sprite ??= GetComponent<SpriteRenderer>();
        hitbox ??= GetComponent<BoxCollider2D>();

        // Store original parent (usually owner transform)
        originalParent = transform.parent;

        if (!sprite) Debug.LogError($"{name}: SpriteRenderer is missing in W_Base");
        if (!hitbox) Debug.LogError($"{name}: BoxCollider2D is missing in W_Base");

        // 2/ Collider mode + default visibility
        hitbox.isTrigger = true;     // trigger-based hit detection
        sprite.enabled   = false;      // show only during attack window
        hitbox.enabled   = false;

        // 3/ Owner + deps
        owner               = transform.root;
        ownerAnimator       ??= owner ? owner.GetComponent<Animator>() : null;
        c_Stats             ??= owner ? owner.GetComponent<C_Stats>() : null;

        if (!owner)         Debug.LogError($"{name}: owner is missing in W_Base");
        if (!ownerAnimator) Debug.LogError($"{name}: ownerAnimator is missing in W_Base");
        if (!c_Stats)       Debug.LogError($"{name}: C_Stats is missing in W_Base");
        if (!weaponData)    Debug.LogError($"{name}: weaponData is missing in W_Base");

        // 4/ Visual + hitbox sizing
        if (weaponData && sprite) sprite.sprite = weaponData.sprite;
        if (autoSizeFromSprite && sprite && sprite.sprite)
        {
            hitbox.size = sprite.sprite.bounds.size;
            
            // Hitbox offset for bottom-pivot sprites (assumed for all weapons)
            // Bottom pivot (0.5, 0) means sprite center is shifted up by half its height
            // So hitbox needs to shift UP to cover the blade instead of empty space below handle
            hitbox.offset = new Vector2(0f, hitbox.size.y * 0.5f);
        }
    }

    // Get position around owner at offsetRadius along attackDir (LOCAL offset when parented)
    protected Vector3 GetPolarPosition(Vector2 attackDir) =>
        (Vector3)(attackDir * weaponData.offsetRadius);

    // Get angle in degrees from up baseline (sprite points UP, pivot at BOTTOM)
    protected float GetPolarAngle(Vector2 attackDir)
    {
        // Angle from UP baseline (0° = up, 90° = right, etc.)
        // No bias needed - sprite pivot at bottom makes this natural
        return Vector2.SignedAngle(Vector2.up, attackDir);
    }

    // Position/rotate + show sprite, optionally enable hitbox
    // Now parents to owner for automatic position anchoring
    protected void BeginVisual(Vector3 localPos, float angle, bool enableHitbox)
    {
        // Parent to owner so weapon follows player automatically
        transform.SetParent(owner, false);
        
        // Set LOCAL position and rotation
        transform.localPosition = localPos;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        
        sprite.enabled = true;
        hitbox.enabled = enableHitbox;
    }

    // Hide weapon and restore original parent
    protected void EndVisual()
    {
        sprite.enabled = false;
        hitbox.enabled = false;
        
        // Restore original parent hierarchy
        transform.SetParent(originalParent, true);
    }

    // Move forward/back along dir over showTime (LOCAL space motion)
    protected IEnumerator ThrustOverTime(Vector2 dir, float showTime, float thrustDist)
    {
        float t = 0f;
        Vector3 start = transform.localPosition - (Vector3)(dir * (thrustDist * 0.5f));
        Vector3 end = transform.localPosition + (Vector3)(dir * (thrustDist * 0.5f));

        while (t < showTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / showTime);
            transform.localPosition = Vector3.Lerp(start, end, k);
            yield return null;
        }
    }

    // Arc-based slash movement for combo attacks (LOCAL space motion)
    // Weapon sweeps in arc like radar arm rotating around player center
    // REQUIRES: Weapon sprite points UP with pivot at BOTTOM (handle)
    // Handle is offset from player by offsetRadius, blade extends further outward
    protected IEnumerator ArcSlashOverTime(Vector2 attackDir, float startAngleDeg, float endAngleDeg, float duration)
    {
        float t = 0f;
        
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            
            // Interpolate the angle from start to end
            float currentAngleDeg = Mathf.Lerp(startAngleDeg, endAngleDeg, k);
            
            // Convert angle to radians for trig calculations
            float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;
            
            // Calculate position on circular arc
            // Using polar coordinates with UP as 0°: x = r*sin(θ), y = r*cos(θ)
            // offsetRadius pushes handle away from player center (so player doesn't cover it)
            // IMPORTANT: Negate X to match the negated X in angle calculation (fixes left/right offset direction)
            float radius = weaponData.offsetRadius;
            Vector3 circularPosition = new Vector3(
                -Mathf.Sin(currentAngleRad) * radius,  // Negate X to fix left/right offset direction
                Mathf.Cos(currentAngleRad) * radius,   // y (up = 0°)
                0f
            );
            
            // Set position (handle at radius distance from player)
            transform.localPosition = circularPosition;
            
            // Set rotation (weapon points outward along radius)
            // Since pivot is at bottom, the weapon naturally extends outward from handle
            transform.localRotation = Quaternion.Euler(0, 0, currentAngleDeg);
            
            yield return null;
        }
    }
    
    // INSTANCE convenience wrappers for from W_Melee / W_Ranged
    protected (C_Health target, GameObject root) TryGetTarget(Collider2D targetCollider)
        => TryGetTarget(owner, targetMask, targetCollider);

    // STATIC versions for W_Projectile
    public static (C_Health target, GameObject root)
                TryGetTarget(Transform owner, LayerMask targetMask, Collider2D targetCollider)
    {
        // Layer filter
        if ((targetMask.value & (1 << targetCollider.gameObject.layer)) == 0)
            return (null, null);

        // Ignore owner
        if (targetCollider.transform == owner || targetCollider.transform.IsChildOf(owner))
            return (null, null);

        // Ignore weapon–weapon contacts
        if (targetCollider.GetComponentInParent<W_Base>() != null)
            return (null, null);

        // Find target health on targetCollider's root
        var target = targetCollider.GetComponentInParent<C_Health>();
        if (target == null || !target.IsAlive)
            return (null, null);

        // Success -> return C_Health and GameObject
        return (target, target.gameObject);
    }

    // INSTANCE convenience wrappers for from W_Melee / W_Ranged
    protected void ApplyHitEffects(C_Stats attackerStats, W_SO weaponData, C_Health targetHealth, Vector2 dir, Collider2D targetCollider, int comboIndex = 0)
                => ApplyHitEffects(attackerStats, weaponData, targetHealth, dir, targetCollider, this, comboIndex);

    // Apply damage + hit effects with combo support
    public static void ApplyHitEffects(C_Stats attackerStats, W_SO weaponData, C_Health targetHealth,
                                        Vector2 dir, Collider2D targetCollider, MonoBehaviour weapon, int comboIndex = 0)
    {
        int attackerAD = attackerStats.AD, attackerAP = attackerStats.AP;
        
        // Apply combo damage multiplier
        int baseWeaponAD = weaponData.AD;
        int baseWeaponAP = weaponData.AP;
        
        float damageMultiplier = weaponData.comboDamageMultipliers[comboIndex];
        int weaponAD = Mathf.RoundToInt(baseWeaponAD * damageMultiplier);
        int weaponAP = Mathf.RoundToInt(baseWeaponAP * damageMultiplier);
        
        float attackerArmorPen = attackerStats.armorPen;
        float attackerMagicPen = attackerStats.magicPen;

        int dealtDamage = targetHealth.ApplyDamage(attackerAD, attackerAP, weaponAD, weaponAP, attackerArmorPen, attackerMagicPen);

        // LIFESTEAL LOGIC
        // If damage was dealt and the attacker has lifesteal, heal the attacker.
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

        // Hit effects: Knockback (only on thrust for combos)
        bool shouldKnockback = !weaponData.onlyThrustKnocksBack || comboIndex == 2;
        if (shouldKnockback && weaponData.knockbackForce > 0f)
        {
            var ec = targetCollider.GetComponentInParent<E_Controller>();
            var pc = targetCollider.GetComponentInParent<P_Controller>();
            
            if (ec != null)
            {
                ec.ReceiveKnockback(dir * weaponData.knockbackForce);
            }
            else if (pc != null)
            {
                pc.ReceiveKnockback(dir * weaponData.knockbackForce);
            }
            else
            {
                // Fallback for entities without controller (NPCs, etc.)
                var rb = targetCollider.GetComponentInParent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.AddForce(dir * weaponData.knockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        // Hit effects: Stun (combo-based duration)
        float stunTime = weaponData.comboStunTimes[comboIndex];
        if (stunTime > 0f)
        {
            var ec = targetCollider.GetComponentInParent<E_Controller>();
            var pc = targetCollider.GetComponentInParent<P_Controller>();
            
            if (ec != null)
            {
                ec.StartCoroutine(ec.StunFor(stunTime));
            }
            else if (pc != null)
            {
                pc.StartCoroutine(pc.StunFor(stunTime));
            }
            // Note: Entities without controllers (NPCs) won't be stunned
        }
    }

    // Called by owner when attacking
    public abstract void Attack(Vector2 attackDir);

    // Called by owner when equipping
    public virtual void Equip(Transform newOwner)
    {
        owner = newOwner;
        ownerAnimator = owner.GetComponent<Animator>();
        c_Stats = owner.GetComponent<C_Stats>();
    }

    // Change weapon data at runtime
    public void SetData(W_SO weaponData)
    {
        this.weaponData = weaponData;
        sprite.sprite = this.weaponData.sprite;
    }


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



