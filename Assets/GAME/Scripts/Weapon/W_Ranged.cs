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
        var a = AimDirection(); // you keep using this for sprite angle & offset (pointsUp honored)

        // Raw direction for PHYSICS (mouse for player, player transform for enemies)
        Vector2 rawDir = GetRawAimDir();

        // 🔒 Lock Animator facing from aim for the whole attack window
        LockAttackFacing(rawDir);

        Vector3 pos = owner.position + (Vector3)a.offset;
        BeginVisual(pos, a.angleDeg, enableHitbox: false);

        bool fired = false;
        yield return ThrustOverTime(a.dir, data.showTime, data.thrustDistance, (k) =>
        {
            if (!fired && k >= 0.5f) { FireProjectile(rawDir); fired = true; } // physics stays precise
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
