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
        var a = AimDirection(); // snapped for visuals

        Vector3 pos = owner.position + (Vector3)a.offset;
        BeginVisual(pos, a.angleDeg, enableHitbox: false);

        // --- compute raw projectile direction ---
        Vector2 rawDir = a.dir; // fallback

        if (owner.CompareTag("Player")) // player shooting
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(Camera.main.transform.position.z - owner.position.z))
            );
            rawDir = ((Vector2)(mouseWorld - transform.position)).normalized;
            if (rawDir.sqrMagnitude < 1e-6f) rawDir = a.dir;
        }
        else // enemy shooting
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
                rawDir = ((Vector2)(player.transform.position - transform.position)).normalized;
        }

        bool fired = false;
        yield return ThrustOverTime(a.dir, data.showTime, data.thrustDistance, (k) =>
        {
            if (!fired && k >= 0.5f) { FireProjectile(rawDir); fired = true; }
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
