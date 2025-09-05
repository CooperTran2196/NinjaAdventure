using System.Collections;
using UnityEngine;

public class W_Ranged : W_Base
{
    public override void Attack()
    {
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        Vector2 dir = GetAimDir();
        Vector3 pos = owner.position + (Vector3)GetSpawnOffset(dir);
        float angle = GetVisualAngle(dir);

        bool fired = false;
        BeginVisual(pos, angle, enableHitbox: false);   // bow never does damage

        yield return ThrustOverTime(dir, data.showTime, data.thrustDistance, (k) =>
        {
            if (!fired && k >= 0.5f) { FireProjectile(dir); fired = true; }
        });

        if (sprite) sprite.enabled = false;
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
