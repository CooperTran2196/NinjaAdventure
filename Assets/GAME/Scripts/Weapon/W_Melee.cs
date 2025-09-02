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
        Vector2 baseline = (data != null && data.pointsUp) ? Vector2.up : Vector2.down;
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
        // Layer filter
        if ((targetMask.value & (1 << other.gameObject.layer)) == 0) return;

        // Ignore owner
        if (other.transform == owner || other.transform.IsChildOf(owner)) return;

        // Ignore weapon–weapon contacts
        if (other.GetComponentInParent<W_Base>() != null) return;

        // Damage
        int baseDmg = (pStats != null) ? pStats.AD : (eStats != null ? eStats.AD : 0);
        int final = baseDmg + (data ? data.baseDamage : 0);

        bool hit = false;
        var pc = other.GetComponentInParent<P_Combat>();
        if (pc != null) { pc.ChangeHealth(-final); hit = true; }

        var ec = other.GetComponentInParent<E_Combat>();
        if (ec != null) { ec.ChangeHealth(-final); hit = true; }

        GameObject root = pc ? pc.gameObject : (ec ? ec.gameObject : null);
        if (root == null) return;                    // no valid target
        if (!hitThisSwing.Add(root.GetInstanceID())) return; // already hit this swing

        // Knockback
        if (hit && data != null && data.knockbackForce > 0f)
        {
            Vector2 dir = GetAimDir(); // snapped 8-way from W_Base
            W_Knockback.PushTarget(other.gameObject, dir, data.knockbackForce);
        }

        // Stun time
        if (data != null && data.stunTime > 0f)
        {
            var pm = other.GetComponentInParent<P_Movement>();
            if (pm != null) { StartCoroutine(W_Stun.Apply(pm, data.stunTime)); }
            else
            {
                var em = other.GetComponentInParent<E_Movement>();
                if (em != null) { StartCoroutine(W_Stun.Apply(em, data.stunTime)); }
            }
        }


    }
}
