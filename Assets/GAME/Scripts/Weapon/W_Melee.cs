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
        hitThisSwing.Clear();

        // Raw direction to target (mouse for player, player transform for enemies)
        Vector2 rawDir = GetRawAimDir();

        // 🔒 Lock Animator facing from aim for this swing window
        LockAttackFacing(rawDir);

        var a = AimDirection(); // keep using your existing visuals (pointsUp honored)

        Vector3 pos = owner.position + (Vector3)a.offset;
        BeginVisual(pos, a.angleDeg, enableHitbox: true);

        yield return ThrustOverTime(a.dir, data.showTime, data.thrustDistance);

        if (hitbox) hitbox.enabled = false;
        if (sprite) sprite.enabled = false;
    }



    void OnTriggerStay2D(Collider2D other)
    {
        var (targetHealth, root) = TryGetTarget(other);
        if (targetHealth == null) return;
        if (!hitThisSwing.Add(root.GetInstanceID())) return;   // one hit per swing
        ApplyHitEffects(c_Stats, data, targetHealth, AimDirection().dir, other);
    }
}
