using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_Melee : W_Base
{
    Vector2 attackDir;

    // Track hits to avoid multiple hits on same target during one swing
    readonly HashSet<int> hitThisSwing = new HashSet<int>();

    public override void Attack(Vector2 aimDir)
    {
        attackDir = aimDir.normalized;
        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        // Clear hit tracker after each swing
        hitThisSwing.Clear();

        // Continuous aim (mouse for Player / player transform for Enemy)
        Vector3 pos = GetPolarPosition(attackDir);
        float angle = GetPolarAngle(attackDir);
        BeginVisual(pos, angle, enableHitbox: true);

        // Thrust
        yield return ThrustOverTime(attackDir, weaponData.showTime, weaponData.thrustDistance);

        // End visuals
        hitbox.enabled = false;
        sprite.enabled = false;
    }

    // Hit detection
    void OnTriggerStay2D(Collider2D targetCollider)
    {
        // Check if the collider is a valid target
        var (targetHealth, root) = TryGetTarget(targetCollider);
        if (targetHealth == null) return;
        if (!hitThisSwing.Add(root.GetInstanceID())) return;   // one hit per swing
        ApplyHitEffects(c_Stats, weaponData, targetHealth, attackDir, targetCollider);
    }
}
