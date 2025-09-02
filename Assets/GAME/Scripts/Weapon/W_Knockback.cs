using UnityEngine;

public static class W_Knockback
{
    // Push a rigidbody by an impulse along the direction
    public static void Push(Rigidbody2D rb, Vector2 direction, float impulse)
    {
        rb.AddForce(direction * impulse, ForceMode2D.Impulse);
    }

    // Push target
    public static void PushTarget(GameObject target, Vector2 direction, float knockbackForce)
    {
        // Player
        var pm = target.GetComponentInParent<P_Movement>();
        if (pm != null) { pm.ReceiveKnockback(direction * knockbackForce); return; }
        
        // Enemy
        var em = target.GetComponentInParent<E_Movement>();
        if (em != null) { em.ReceiveKnockback(direction * knockbackForce); return; }

        // Others
        var rb = target.GetComponentInParent<Rigidbody2D>();
        if (rb != null) rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
    }

    // Radial AoE push
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
