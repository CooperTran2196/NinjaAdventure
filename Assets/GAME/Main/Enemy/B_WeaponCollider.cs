using System.Collections;
using UnityEngine;

/// <summary>
/// Universal boss weapon collider for sprites with baked-in weapons.
/// Switches between contact damage and attack damage based on boss attack state.
/// When NOT attacking: Deals collision damage (small, repeated)
/// When attacking: Deals boss AD damage (large, with knockback + stun)
/// Place on Sprite child GameObject alongside C_UpdateColliderShape.
/// Looks for C_Stats/C_Health/I_Controller on parent GameObject.
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
public class B_WeaponCollider : MonoBehaviour
{
    [Header("References")]
    C_Stats      c_Stats;
    C_Health     c_Health;
    I_Controller bossController;

    [Header("Attack Settings")]
    public float     knockbackForce = 5f;
    public float     stunDuration   = 0.3f;
    public LayerMask playerLayer;

    // Runtime state
    bool  isInAttackMode;
    float contactTimer;
    bool  hasDealtAttackDamage; // Track if damage dealt this attack cycle

    void Awake()
    {
        c_Stats        = GetComponentInParent<C_Stats>();
        c_Health       = GetComponentInParent<C_Health>();
        bossController = GetComponentInParent<I_Controller>();
        
        if (!c_Stats) { Debug.LogError($"{name}: C_Stats not found in parent!", this); return; }
        if (!c_Health) { Debug.LogError($"{name}: C_Health not found in parent!", this); return; }
        if (bossController == null) Debug.LogWarning($"{name}: I_Controller not found in parent!", this);
    }

    // PUBLIC API

    // Enable attack mode - boss deals AD damage with knockback
    public void EnableWeaponMode()
    {
        isInAttackMode = true;
        hasDealtAttackDamage = false; // Reset damage flag for new attack
    }

    // Disable attack mode - boss deals collision damage only
    public void DisableWeaponMode()
    {
        isInAttackMode = false;
        hasDealtAttackDamage = false; // Reset for next attack
    }

    // COLLISION DETECTION

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!c_Health.IsAlive) return;

        // Check if hit player
        if ((playerLayer.value & (1 << collision.collider.gameObject.layer)) == 0) return;

        C_Health playerHealth = collision.collider.GetComponent<C_Health>();
        if (!playerHealth || !playerHealth.IsAlive) return;

        if (isInAttackMode)
        {
            // ATTACK MODE: Deal boss AD damage with knockback + stun
            ApplyAttackDamage(playerHealth, collision.collider);
        }
        else
        {
            // CONTACT MODE: Deal collision damage with cooldown
            ApplyContactDamage(playerHealth);
        }
    }

    void ApplyContactDamage(C_Health playerHealth)
    {
        if (contactTimer > 0f)
        {
            contactTimer -= Time.fixedDeltaTime;
            return;
        }

        playerHealth.ChangeHealth(-c_Stats.collisionDamage);
        SYS_GameManager.Instance.sys_SoundManager.PlayPlayerHit();
        contactTimer = c_Stats.collisionTick;
    }

    void ApplyAttackDamage(C_Health playerHealth, Collider2D playerCollider)
    {
        // Only deal damage once per attack cycle
        if (hasDealtAttackDamage) return;
        
        hasDealtAttackDamage = true; // Mark damage as dealt
        
        // Boss AD damage (from stats, no separate weapon damage)
        int attackDamage = c_Stats.AD;
        
        playerHealth.ApplyDamage(attackDamage, 0, 0, 0, 0, 0);

        // Apply knockback + stun
        P_Controller playerController = playerCollider.GetComponent<P_Controller>();
        if (playerController)
        {
            Vector2 knockbackDir = (playerCollider.transform.position - transform.position).normalized;
            playerController.SetKnockback(knockbackDir * knockbackForce);

            if (stunDuration > 0)
            {
                StartCoroutine(playerController.SetStunTime(stunDuration));
            }
        }
    }
}
