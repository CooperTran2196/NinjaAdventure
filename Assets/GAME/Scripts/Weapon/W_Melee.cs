using System.Collections;
using UnityEngine;

public class W_Melee : W_Base
{
    Vector2 DirFromAnimator()
    {
        if (ownerAnimator == null) return Vector2.down; // default
        float x = ownerAnimator.GetFloat("atkX");
        float y = ownerAnimator.GetFloat("atkY");
        Vector2 d = new Vector2(x, y);
        return d.sqrMagnitude > 0.0001f ? d.normalized : Vector2.down;
    }

    public override void Attack()
    {
        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        Vector2 dir = DirFromAnimator();

        // Place beside owner and face attack direction
        transform.position = owner.position + (Vector3)(dir * data.offsetDistance);
        float angle = Vector2.SignedAngle(Vector2.down, dir);
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Show + enable hitbox
        sprite.enabled = true;
        hitbox.enabled = true;

        // Simple thrust over showTime
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
        // Ignore non-target layers
        if ((targetMask.value & (1 << other.gameObject.layer)) == 0) return;

        // Ignore hitting owner
        if (other.transform == owner || other.transform.IsChildOf(owner)) return;

        // Damage math
        int baseDmg = (pStats != null) ? pStats.attackDmg : (eStats != null ? eStats.attackDmg : 0);
        int final = baseDmg + (data ? data.baseDamage : 0);

        // Apply to either Player or Enemy
        var pc = other.GetComponentInParent<P_Combat>();
        if (pc != null) { pc.ChangeHealth(-final); return; }

        var ec = other.GetComponentInParent<E_Combat>();
        if (ec != null) { ec.ChangeHealth(-final); return; }
    }
}
