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

        // Spawn from current bow position (mid-thrust)
        Vector3 currentPosition = transform.position;

        // Projectile default art faces RIGHT
        float projAngle = Vector2.SignedAngle(Vector2.right, attackDir);
        
        // Spawn + init
        var go = Instantiate(weaponData.projectilePrefab, currentPosition, Quaternion.Euler(0, 0, projAngle));
        var proj = go.GetComponent<W_Projectile>();
        if (proj != null) proj.Init(owner, c_Stats, weaponData, attackDir, targetMask);
    }
}
