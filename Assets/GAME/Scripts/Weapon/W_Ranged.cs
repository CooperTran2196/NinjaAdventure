using System.Collections;
using UnityEngine;

public class W_Ranged : W_Base
{
    public override void Attack(Vector2 dir)
    {
        StartCoroutine(Shoot(dir));
    }

    IEnumerator Shoot(Vector2 dir)
    {
        // Normalize aim for consistency
        dir = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector2.right;

        // Visuals (bow orbit + rotation)
        Vector3 pos = PolarPosition(dir);
        float angle = PolarAngle(dir);
        BeginVisual(pos, angle, enableHitbox: false);

        // Run the thrust motion in parallel
        StartCoroutine(ThrustOverTime(dir, data.showTime, data.thrustDistance));

        // Fire exactly at 50% of the show time
        yield return new WaitForSeconds(data.showTime * 0.5f);
        FireProjectile(dir);

        // Finish the remaining half of the show time
        yield return new WaitForSeconds(data.showTime * 0.5f);

        // Hide
        sprite.enabled = false;
    }

    void FireProjectile(Vector2 dir)
    {
        if (!data || !data.projectilePrefab) return;

        // Spawn from current bow position (mid-thrust)
        Vector3 p = transform.position;

        // Projectile default art faces RIGHT
        float projAngle = Vector2.SignedAngle(Vector2.right, dir);

        var go = Instantiate(data.projectilePrefab, p, Quaternion.Euler(0, 0, projAngle));
        var proj = go.GetComponent<W_Projectile>();
        if (proj != null) proj.Init(owner, c_Stats, data, dir, targetMask);
    }
}
