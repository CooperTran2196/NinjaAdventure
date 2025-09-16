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
        // Visuals (bow orbit + rotation)
        Vector3 pos = PolarPosition(dir);
        float angle = PolarAngle(dir);
        BeginVisual(pos, angle, enableHitbox: false);

        bool fired = false;
        yield return ThrustOverTime(dir, data.showTime, data.thrustDistance, (k) =>
        {
            if (!fired && k >= 0.5f) { FireProjectile(dir); fired = true; }
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
