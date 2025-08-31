using UnityEngine;

public static class W_Knockback
{
    // Push a rigidbody by an impulse along 'direction' (assumed normalized).
    public static void Push(Rigidbody2D rb, Vector2 direction, float impulse)
    {
        rb.AddForce(direction * impulse, ForceMode2D.Impulse);
    }

    // Convenience: find Rigidbody2D on target or parents and push.
    public static void PushTarget(GameObject target, Vector2 direction, float impulse)
    {
        var rb = target.GetComponentInParent<Rigidbody2D>();
        if (rb != null) rb.AddForce(direction * impulse, ForceMode2D.Impulse);
        else Debug.LogWarning("W_Knockback: Rigidbody2D not found on target hierarchy.");
    }

    // Optional radial push helper for future AoE.
    public static int PushRadial(Vector2 center, float radius, float impulse, LayerMask mask)
    {
        int count = 0;
        var hits = Physics2D.OverlapCircleAll(center, radius, mask);
        for (int i = 0; i < hits.Length; i++)
        {
            var rb = hits[i].attachedRigidbody;
            if (rb == null) continue;
            Vector2 dir = (rb.position - center).normalized;
            rb.AddForce(dir * impulse, ForceMode2D.Impulse);
            count++;
        }
        return count;
    }
}
