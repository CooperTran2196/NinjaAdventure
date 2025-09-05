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
        Vector2 dir = GetAimDir();                      // snapped 8-way
        Vector3 spawn = owner.position + (Vector3)GetSpawnOffset(dir);

        // Rotate bow from baseline depending on art (up vs down)
        Vector2 baseline = data.pointsUp ? Vector2.down : Vector2.up;
        float angle = Vector2.SignedAngle(baseline, dir);

        transform.position = spawn;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Show bow; hitbox stays DISABLED (no bow damage)
        if (sprite) sprite.enabled = true;
        if (hitbox) hitbox.enabled = false;

        // Thrust like melee over showTime
        float t = 0f;
        bool fired = false;
        Vector3 start = transform.position - (Vector3)(dir * (data.thrustDistance * 0.5f));
        Vector3 end   = transform.position + (Vector3)(dir * (data.thrustDistance * 0.5f));

        while (t < data.showTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / data.showTime);
            transform.position = Vector3.Lerp(start, end, k);

            // Fire once around mid-thrust
            if (!fired && k >= 0.5f)
            {
                FireProjectile(dir);
                fired = true;
            }

            yield return null;
        }

        // Hide bow
        if (sprite) sprite.enabled = false;
    }

    void FireProjectile(Vector2 dir)
    {
        if (!data || !data.projectilePrefab) return;

        // Where to spawn: from current bow position (already thrusting forward)
        Vector3 p = transform.position;

        // Projectile default art faces RIGHT; rotate from right baseline
        float projAngle = Vector2.SignedAngle(Vector2.right, dir);

        var go = Instantiate(data.projectilePrefab, p, Quaternion.Euler(0, 0, projAngle));
        var proj = go.GetComponent<W_Projectile>();
        if (proj != null) proj.Init(owner, c_Stats, data, dir, targetMask);
    }
}
