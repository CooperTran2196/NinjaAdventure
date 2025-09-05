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
        Vector2 dir = GetAimDir();
        Vector3 pos = owner.position + (Vector3)GetSpawnOffset(dir);
        float angle = GetVisualAngle(dir);

        hitThisSwing.Clear();
        BeginVisual(pos, angle, enableHitbox: true);    // show + enable hitbox

        yield return ThrustOverTime(dir, data.showTime, data.thrustDistance);

        // Hide
        if (hitbox) hitbox.enabled = false;
        if (sprite) sprite.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!TryGetTarget(other, out var targetHealth, out var root)) return;
        if (!hitThisSwing.Add(root.GetInstanceID())) return;   // de-dup this swing

        // Use current aim for knockback direction
        ApplyHitEffects(c_Stats, data, targetHealth, GetAimDir(), other, this);
    }


}
