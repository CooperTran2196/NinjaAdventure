using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class W_Melee : MonoBehaviour
{
    [Header("Owner / Targeting")]
    public Transform owner;            // If null, uses transform.root
    public LayerMask targetMask;       // e.g., Enemy

    [Header("Damage")]
    public int weaponDamage = 1;       // final = base + weaponDamage

    [Header("Placement / Motion")]
    public float offsetDistance = 0.18f;   // start at owner + dir * offset
    public float showTime = 0.18f;         // swing window
    public float thrustDistance = 0.20f;   // travel across showTime

    // cached
    SpriteRenderer sprite;
    BoxCollider2D col;
    P_Movement pMove;
    P_Stats pStats;
    Vector2 cachedDir = Vector2.down;
    Animator ownerAnimator;

    void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        col ??= GetComponent<BoxCollider2D>();
        owner ??= transform.root;
        if (col) col.isTrigger = true;

        if (owner)
        {
            pMove = owner.GetComponent<P_Movement>();
            pStats = owner.GetComponent<P_Stats>();
            ownerAnimator ??= owner ? owner.GetComponent<Animator>() : null;
        }

        if (sprite) sprite.enabled = false;
        if (col) { col.isTrigger = true; col.enabled = false; }
    }

    // Call from P_Combat at the hit tick
    public void Attack()
    {
        StartCoroutine(Swing());
    }
    Vector2 GetAimDir()
    {
        // 1) try movement.lastMove (preferred)
        Vector2 d = (pMove != null) ? pMove.lastMove : Vector2.zero;
        if (d.sqrMagnitude >= 0.0001f) { cachedDir = d.normalized; return cachedDir; }

        // 2) try animator attack facing (atkX/atkY)
        if (ownerAnimator != null)
        {
            float ax = ownerAnimator.GetFloat("atkX");
            float ay = ownerAnimator.GetFloat("atkY");
            d = new Vector2(ax, ay);
            if (d.sqrMagnitude >= 0.0001f) { cachedDir = d.normalized; return cachedDir; }

            // 3) fallback: moveX/moveY if you use those
            float mx = ownerAnimator.GetFloat("moveX");
            float my = ownerAnimator.GetFloat("moveY");
            d = new Vector2(mx, my);
            if (d.sqrMagnitude >= 0.0001f) { cachedDir = d.normalized; return cachedDir; }
        }

        // 4) last known or DOWN as final fallback
        return cachedDir;
    }

    IEnumerator Swing()
    {
        Vector2 dir = new Vector2(ownerAnimator.GetFloat("atkX"),
                          ownerAnimator.GetFloat("atkY")).normalized; // always valid now
        float ang = Vector2.SignedAngle(Vector2.down, dir);

        // slightly bigger default so it’s clearly in front of player
        float off = Mathf.Max(0.3f, offsetDistance);

        // place & face
        transform.position = owner.position + (Vector3)(dir * off);
        transform.rotation = Quaternion.Euler(0, 0, ang);

        if (sprite) sprite.enabled = true;
        if (col) col.enabled = true;

        // thrust across the window
        float t = 0f;
        Vector3 start = owner.position + (Vector3)(dir * (off - thrustDistance / 2));
        Vector3 end = owner.position + (Vector3)(dir * (off + thrustDistance / 2));

        while (t < showTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / showTime);
            transform.position = Vector3.Lerp(start, end, k);
            yield return null;
        }

        if (col) col.enabled = false;
        if (sprite) sprite.enabled = false;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore owner
        if (owner && other.transform.IsChildOf(owner)) return;

        // Layer mask gate
        if (((1 << other.gameObject.layer) & targetMask.value) == 0) return;

        // Damage: final = base + weapon
        int baseDmg = pStats.attackDmg;
        int dmg = baseDmg + weaponDamage;

        // Apply to Player or Enemy combat (your project uses these names)
        var pc = other.GetComponent<P_Combat>();
        if (pc != null) { pc.ChangeHealth(-dmg); return; }

        var ec = other.GetComponent<E_Combat>();
        if (ec != null) { ec.ChangeHealth(-dmg); return; }
    }
}
