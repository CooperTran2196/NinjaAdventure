using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_Melee : W_Base
{
    Vector2 swingDir;

    readonly HashSet<int> hitThisSwing = new HashSet<int>();

    public override void Attack(Vector2 dir)
    {
        swingDir = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector2.right;
        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        hitThisSwing.Clear();

        // Continuous aim (mouse for Player / player transform for Enemy)
        Vector2 rawDir = swingDir;

        // Visuals
        Vector3 pos = PolarPosition(rawDir);
        float angle = PolarAngle(rawDir);
        BeginVisual(pos, angle, enableHitbox: true);

        // Thrust
        yield return ThrustOverTime(rawDir, data.showTime, data.thrustDistance);

        hitbox.enabled = false;
        sprite.enabled = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        var (targetHealth, root) = TryGetTarget(other);
        if (targetHealth == null) return;
        if (!hitThisSwing.Add(root.GetInstanceID())) return;   // one hit per swing
        ApplyHitEffects(c_Stats, data, targetHealth, swingDir, other);
    }
}
