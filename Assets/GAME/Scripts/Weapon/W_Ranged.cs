using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class W_Ranged : W_Base
{
    public override void Attack()
    {
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        // Continuous aim for both visuals and physics
        Vector2 rawDir = GetRawAimDir();

        // Visuals (bow orbit + rotation)
        Vector3 pos = PolarPosition(rawDir);
        float angle = PolarAngle(rawDir);
        BeginVisual(pos, angle, enableHitbox: false);

        bool fired = false;
        yield return ThrustOverTime(rawDir, data.showTime, data.thrustDistance, (k) =>
        {
            if (!fired && k >= 0.5f) { FireProjectile(rawDir); fired = true; }
        });

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
