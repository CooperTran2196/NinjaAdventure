using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_Melee : W_Base
{
    Vector2 attackDir;

    // Track hits to avoid multiple hits on same target during one git
    readonly HashSet<int> alreadyHit = new HashSet<int>();

    // Override attack method
    public override void Attack(Vector2 aimDir)
    {
        attackDir = aimDir.normalized;
        StartCoroutine(Hit());
    }

    // Coroutine for handling the hit process
    IEnumerator Hit()
    {
        // Clear hit tracker after each git
        alreadyHit.Clear();

        // Continuous aim (mouse for Player / player transform for Enemy)
        Vector3 posision = GetPolarPosition(attackDir);
        float angle = GetPolarAngle(attackDir);
        BeginVisual(posision, angle, enableHitbox: true);

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
        if (!alreadyHit.Add(root.GetInstanceID())) return;   // one hit per swing
        ApplyHitEffects(c_Stats, weaponData, targetHealth, attackDir, targetCollider);
    }
}
