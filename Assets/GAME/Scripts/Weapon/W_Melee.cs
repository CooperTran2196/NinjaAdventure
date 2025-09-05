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
        Vector2 dir = GetAimDir();                      // snapped to 8-way
        Vector3 spawn = owner.position + (Vector3)GetSpawnOffset(dir);

        // rotate from baseline depending on art (up vs down)
        Vector2 baseline = data.pointsUp ? Vector2.up : Vector2.down;
        float angle = Vector2.SignedAngle(baseline, dir);

        transform.position = spawn;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        hitThisSwing.Clear();
        
        // Show + enable hitbox
        sprite.enabled = true;
        hitbox.enabled = true;

        // Thrust over showTime
        float t = 0f;
        Vector3 start = transform.position - (Vector3)(dir * (data.thrustDistance * 0.5f));
        Vector3 end   = transform.position + (Vector3)(dir * (data.thrustDistance * 0.5f));
        while (t < data.showTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / data.showTime);
            transform.position = Vector3.Lerp(start, end, k);
            yield return null;
        }

        // Hide
        hitbox.enabled = false;
        sprite.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Layer filter -> only react to colliders on targetMask
        if ((targetMask.value & (1 << other.gameObject.layer)) == 0) return;

        // Ignore owner
        if (other.transform == owner || other.transform.IsChildOf(owner)) return;

        // Ignore weapon–weapon contacts
        if (other.GetComponentInParent<W_Base>() != null) return;

        // Attacker stats
        int attackerAD = c_Stats.AD;
        int attackerAP = c_Stats.AP;

        // Weapon bases (allow mix)
        int weaponAD = data.baseAD;
        int weaponAP = data.baseAP;

        // Resolve target health once
        var targetHealth = other.GetComponentInParent<C_Health>();
        if (targetHealth == null || !targetHealth.IsAlive) return;

        // Per-swing de-dup
        GameObject root = targetHealth.gameObject;
        if (!hitThisSwing.Add(root.GetInstanceID())) return;

        // Apply damage (AR handled inside)
        targetHealth.ApplyDamage(attackerAD, attackerAP, weaponAD, weaponAP);

        // Knockback (apply regardless of reduced damage)
        if (data && data.knockbackForce > 0f)
        {
            Vector2 dir = GetAimDir(); // snapped 8-way from W_Base
            W_Knockback.PushTarget(other.gameObject, dir, data.knockbackForce);
        }

        // Stun time
        if (data && data.stunTime > 0f)
        {
            var pm = other.GetComponentInParent<P_Movement>();
            if (pm) { StartCoroutine(W_Stun.Apply(pm, data.stunTime)); }
            else
            {
                var em = other.GetComponentInParent<E_Movement>();
                if (em) { StartCoroutine(W_Stun.Apply(em, data.stunTime)); }
            }
        }
    }


}
