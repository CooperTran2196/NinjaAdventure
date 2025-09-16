using System.Collections;
using UnityEngine;

public class INV_UseItem : MonoBehaviour
{
    [Header("Targets")]
    public C_Health c_Health;
    public P_StatsChanged p_Stats;

    void Awake()
    {
        c_Health ??= FindFirstObjectByType<C_Health>();
        p_Stats  ??= FindFirstObjectByType<P_StatsChanged>();

        if (!c_Health) Debug.LogError($"{name}: C_Health in INV_UseItemmissing.", this);
        if (!p_Stats)  Debug.LogError($"{name}: P_StatsChanged in INV_UseItemmissing missing.", this);
    }

    // Returns true if any effect actually applied
    public bool ApplyItemEffects(INV_ItemSO itemSO)
    {
        bool applied = false;
        var s = p_Stats.c_Stats; // holds currentHP / maxHP

        // Heal only if not already full (avoid consuming pointlessly)
        if (itemSO.currentHealth > 0 && s.currentHP < s.maxHP)
        {
            c_Health.ChangeHealth(itemSO.currentHealth); // positive = heal
            applied = true;
        }

        // Apply stat bumps immediately
        if (itemSO.maxHealth != 0) { p_Stats.ApplyStat(ST_SkillSO.Stat.MaxHP, itemSO.maxHealth); applied = true; }
        if (itemSO.speed     != 0) { p_Stats.ApplyStat(ST_SkillSO.Stat.MS,     itemSO.speed);     applied = true; }
        if (itemSO.damage    != 0) { p_Stats.ApplyStat(ST_SkillSO.Stat.AD,     itemSO.damage);    applied = true; }

        // Single timer for all temporary effects
        if (applied && itemSO.durationSec > 0f)
            StartCoroutine(EffectTimer(itemSO));

        return applied;
    }

    IEnumerator EffectTimer(INV_ItemSO itemSO)
    {
        var s = p_Stats.c_Stats;
        yield return new WaitForSeconds(itemSO.durationSec);

        if (itemSO.maxHealth != 0)
        {
            p_Stats.ApplyStat(ST_SkillSO.Stat.MaxHP, -itemSO.maxHealth);
            s.currentHP = Mathf.Min(s.currentHP, s.maxHP); // clamp after MaxHP shrink
        }
        if (itemSO.speed  != 0) p_Stats.ApplyStat(ST_SkillSO.Stat.MS, -itemSO.speed);
        if (itemSO.damage != 0) p_Stats.ApplyStat(ST_SkillSO.Stat.AD, -itemSO.damage);
    }
}
