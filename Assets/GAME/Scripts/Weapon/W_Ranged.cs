using System.Collections;
using UnityEngine;

public class W_Ranged : W_Base
{
    public override void Attack(Vector2 attackDir)
    {
        StartCoroutine(Shoot(attackDir));
    }

    IEnumerator Shoot(Vector2 attackDir)
    {
        // Normalize aim for consistency
        attackDir = attackDir.normalized;

        // Continuous aim (mouse for Player / player transform for Enemy)
        Vector3 posision = GetPolarPosition(attackDir);
        float angle = GetPolarAngle(attackDir);
        BeginVisual(posision, angle, enableHitbox: false);

        // Run the thrust motion in parallel
        StartCoroutine(ThrustOverTime(attackDir, weaponData.showTime, weaponData.thrustDistance));

        // Fire exactly at 50% of the show time
        yield return new WaitForSeconds(weaponData.showTime * 0.5f);
        FireProjectile(attackDir);

        // Finish the remaining half of the show time
        yield return new WaitForSeconds(weaponData.showTime * 0.5f);

        // Hide
        sprite.enabled = false;
    }

    void FireProjectile(Vector2 attackDir)
    {
        // Check prefab
        if (!weaponData.projectilePrefab) return;

        // Mana check for homing projectiles only (check if weapon has manaCost)
        if (weaponData.manaCost > 0)
        {
            var manaSystem = c_Stats.GetComponent<C_Mana>();
            if (manaSystem != null)
            {
                // Try to consume mana
                if (!manaSystem.ConsumeMana(weaponData.manaCost))
                {
                    Debug.Log($"{name}: Not enough mana to fire projectile! Need {weaponData.manaCost}, have {manaSystem.CurrentMana}");
                    return; // Cancel firing
                }
            }
        }

        // Spawn from current bow position (mid-thrust)
        Vector3 currentPosition = transform.position;

        // Projectile default art faces RIGHT
        float projAngle = Vector2.SignedAngle(Vector2.right, attackDir);
        
        // Spawn
        var go = Instantiate(weaponData.projectilePrefab, currentPosition, Quaternion.Euler(0, 0, projAngle));
        
        // Try homing first, fallback to regular
        var homingProj = go.GetComponent<W_ProjectileHoming>();
        if (homingProj != null)
        {
            homingProj.Init(owner, c_Stats, weaponData, attackDir, targetMask);
            return;
        }

        var proj = go.GetComponent<W_Projectile>();
        if (proj != null)
        {
            proj.Init(owner, c_Stats, weaponData, attackDir, targetMask);
            return;
        }

        // If neither found, log error
        Debug.LogError($"{name}: projectilePrefab missing W_Projectile or W_HomingProjectile component!");
        Destroy(go);
    }
}
