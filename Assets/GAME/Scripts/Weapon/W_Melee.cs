using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_Melee : W_Base
{
    readonly HashSet<int> hitThisSwing = new HashSet<int>();

    public override void Attack()
    {
        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        // Reset enemy id per attack -> avoid machine-gun hits
        hitThisSwing.Clear();
        var a = AimDirection(); // a.dir, a.angleDeg, a.offset

        Vector3 pos = owner.position + (Vector3)a.offset;
        BeginVisual(pos, a.angleDeg, enableHitbox: true);

        yield return ThrustOverTime(a.dir, data.showTime, data.thrustDistance);

        if (hitbox) hitbox.enabled = false;
        if (sprite) sprite.enabled = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!TryGetTarget(other, out var targetHealth, out var root)) return;

        if (!hitThisSwing.Add(root.GetInstanceID())) return;   // one hit per swing
        ApplyHitEffects(c_Stats, data, targetHealth, AimDirection().dir, other, this);
    }
}
