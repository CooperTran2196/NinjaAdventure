using UnityEngine;

public static class W_ApplyDamage
{
    public static void ApplyPhysical(int attackerAD, int weaponBase, C_ChangeHealth target)
    {
        if (target == null || !target.IsAlive) return;
        int requested = attackerAD + weaponBase;
        float reduction = Mathf.Clamp01(target.AR / 100f);
        int reduced = Mathf.RoundToInt(requested * (1f - reduction));
        if (reduced <= 0) return;
        target.ChangeHealth(-reduced);
    }

    public static void ApplyAbility(int attackerAP, int weaponBase, C_ChangeHealth target)
    {
        if (target == null || !target.IsAlive) return;
        int requested = attackerAP + weaponBase;
        float reduction = Mathf.Clamp01(target.MR / 100f);
        int reduced = Mathf.RoundToInt(requested * (1f - reduction));
        if (reduced <= 0) return;
        target.ChangeHealth(-reduced);
    }
}
    