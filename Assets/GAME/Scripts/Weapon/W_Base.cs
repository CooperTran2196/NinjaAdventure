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

    void Awake()
    {
        // 1/ Cache weapon components
        sprite ??= GetComponent<SpriteRenderer>();
        hitbox ??= GetComponent<BoxCollider2D>();

        if (!sprite) Debug.LogError($"{name}: SpriteRenderer missing on {gameObject.name}", this);
        if (!hitbox) Debug.LogError($"{name}: BoxCollider2D missing on {gameObject.name}", this);

        // 2/ Collider mode + default visibility
        hitbox.isTrigger = true;     // trigger-based hit detection
        sprite.enabled   = false;      // show only during attack window
        hitbox.enabled   = false;

        // 3/ Owner + deps
        owner               = transform.root;
        ownerAnimator       ??= owner ? owner.GetComponent<Animator>() : null;
        c_Stats             ??= owner ? owner.GetComponent<C_Stats>() : null;

        if (!owner)         Debug.LogError($"{name}: Owner (root transform) not found for {gameObject.name}", this);
        if (!ownerAnimator) Debug.LogError($"{name}: Animator missing on owner {owner?.name}", this);
        if (!c_Stats)       Debug.LogError($"{name}: C_Stats missing on owner {owner?.name}", this);
        if (!weaponData)    Debug.LogError($"{name}: W_SO weaponData is not assigned on {gameObject.name}", this);

        // 4/ Visual + hitbox sizing
        if (weaponData && sprite) sprite.sprite = weaponData.sprite;
        if (autoSizeFromSprite && sprite && sprite.sprite)
        {
            hitbox.size = sprite.sprite.bounds.size;
            hitbox.offset = Vector2.zero;
        }
    }

    // Get position around owner at offsetRadius along attackDir
    protected Vector3 GetPolarPosition(Vector2 attackDir) =>
        owner.position + (Vector3)(attackDir * weaponData.offsetRadius);

    // Get angle in degrees from up/down baseline + bias
    protected float GetPolarAngle(Vector2 attackDir)
    {
        // Angle from up/down baseline + bias
        Vector2 baseline = weaponData.pointsUp ? Vector2.up : Vector2.down;
        // Get the signed angle between the baseline and the attack direction
        return Vector2.SignedAngle(baseline, attackDir) + weaponData.angleBiasDeg;
    }

    // Position/rotate + show sprite, optionally enable hitbox
    protected void BeginVisual(Vector3 pos, float angle, bool enableHitbox)
    {
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        sprite.enabled = true;
        hitbox.enabled = enableHitbox;
    }

    // Move forward/back along dir over showTime (no callback)
    protected IEnumerator ThrustOverTime(Vector2 dir, float showTime, float thrustDist)
    {
        float t = 0f;
        Vector3 start = transform.position - (Vector3)(dir * (thrustDist * 0.5f));
        Vector3 end = transform.position + (Vector3)(dir * (thrustDist * 0.5f));

        while (t < showTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / showTime);
            transform.position = Vector3.Lerp(start, end, k);
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
    protected void ApplyHitEffects(C_Stats attackerStats, W_SO weaponData, C_Health targetHealth, Vector2 dir, Collider2D targetCollider)
                => ApplyHitEffects(attackerStats, weaponData, targetHealth, dir, targetCollider, this);

    // Apply damage + hit effects
    public static void ApplyHitEffects(C_Stats attackerStats, W_SO weaponData, C_Health targetHealth,
                                        Vector2 dir, Collider2D targetCollider, MonoBehaviour weapon)
    {
        int attackerAD = attackerStats.AD, attackerAP = attackerStats.AP;
        int weaponAD = weaponData.AD, weaponAP = weaponData.AP;
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

        // Hit effects
        if (weaponData.knockbackForce > 0f)
        {
            // NEW system first: direct call into E_Controller
            var ec = targetCollider.GetComponentInParent<E_Controller>();
            if (ec != null)
            {
                ec.ReceiveKnockback(dir * weaponData.knockbackForce);
            }
            else
            {
                // OLD system fallback (player/old enemy/rigidbody)
                W_Knockback.PushTarget(targetCollider.gameObject, dir, weaponData.knockbackForce);
            }
        }


        if (weaponData.stunTime > 0f)
        {
            // NEW system first: stun handled inside controller (single coroutine)
            var ec = targetCollider.GetComponentInParent<E_Controller>();
            if (ec)
            {
                ec.StartCoroutine(ec.StunFor(weaponData.stunTime));
            }
            else
            {
                // OLD system fallbacks
                var pm = targetCollider.GetComponentInParent<P_Movement>();
                if (pm)
                {
                    weapon.StartCoroutine(W_Stun.Apply(pm, weaponData.stunTime));
                }
                else
                {
                    var em = targetCollider.GetComponentInParent<E_Movement>();
                    if (em) weapon.StartCoroutine(W_Stun.Apply(em, weaponData.stunTime));
                }
            }
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



