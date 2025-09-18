// d:\Google Drive\Code\Final project\NinjaAdventure\Assets\GAME\Scripts\Character\C_StatsManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[DisallowMultipleComponent]
public class C_StatsManager : MonoBehaviour
{
    public event Action OnStatsChanged;

    [Header("Component References")]
    public C_Stats c_Stats;
    public C_Health c_Health;

    // Base stats that can be permanently modified.
    private int baseAD, baseAP, baseMaxHP, baseAR, baseMR;
    private float baseMS, baseKR, baseLifestealPercent;

    // List of temporary buffs/debuffs.
    private readonly List<StatModifier> activeModifiers = new List<StatModifier>();

    void Awake()
    {
        c_Stats ??= GetComponent<C_Stats>();
        c_Health ??= GetComponent<C_Health>();

        if (!c_Stats) Debug.LogError($"{name}: C_Stats is missing.", this);
        if (!c_Health) Debug.LogError($"{name}: C_Health is missing.", this);

        // Snapshot the initial stats from the editor into our base stats.
        baseAD = c_Stats.AD;
        baseAP = c_Stats.AP;
        baseMS = c_Stats.MS;
        baseMaxHP = c_Stats.maxHP;
        baseAR = c_Stats.AR;
        baseMR = c_Stats.MR;
        baseKR = c_Stats.KR;
        baseLifestealPercent = c_Stats.lifestealPercent;
    }

    /// <summary>
    /// The main entry point for applying any stat change.
    /// </summary>
    public void ApplyModifier(StatModifier modifier)
    {
        // Rule: Duration == 1 is an INSTANT, one-shot effect.
        if (modifier.Duration == 1)
        {
            ApplySingleStatChange(modifier.Stat, modifier.Value, modifier.Type);
            return; // It's done. Do not store or track it.
        }

        // Rule: Duration == 0 is a PERMANENT upgrade to base stats.
        if (modifier.Duration == 0)
        {
            PromoteModifierToBaseStats(modifier);
            RecalculateAllStats(); // Recalculate to apply the new base stat.
            return;
        }

        // Rule: Duration > 1 is a TIMED effect.
        if (modifier.Duration > 1)
        {
            if (modifier.IsOverTime)
            {
                // This is a ticking effect, like Heal-over-Time.
                StartCoroutine(ApplyOverTimeEffect(modifier));
            }
            else
            {
                // This is a temporary buff, like +10 AD for 10s.
                activeModifiers.Add(modifier);
                RecalculateAllStats();
                StartCoroutine(RevertModifierAfterDelay(modifier));
            }
        }
    }

    /// <summary>
    /// Bakes a permanent modifier directly into the base stats.
    /// </summary>
    private void PromoteModifierToBaseStats(StatModifier modifier)
    {
        // For now, we only support promoting Flat modifiers permanently.
        if (modifier.Type == ModifierType.Percent)
        {
            Debug.LogWarning($"Cannot permanently promote a Percentage modifier for {modifier.Stat}. This is not supported. Ignoring modifier.", this);
            return;
        }

        switch (modifier.Stat)
        {
            case StatType.AttackDamage:     baseAD += (int)modifier.Value; break;
            case StatType.AbilityPower:     baseAP += (int)modifier.Value; break;
            case StatType.MoveSpeed:        baseMS += modifier.Value; break;
            case StatType.MaxHealth:        baseMaxHP += (int)modifier.Value; break;
            case StatType.Armor:            baseAR += (int)modifier.Value; break;
            case StatType.MagicResist:      baseMR += (int)modifier.Value; break;
            case StatType.KnockbackResist:  baseKR += modifier.Value; break;
            case StatType.LifestealPercent: baseLifestealPercent += modifier.Value; break;
            case StatType.Heal:             c_Health.ChangeHealth((int)modifier.Value); break; // Permanent heal is just an instant heal.
        }
    }

    /// <summary>
    /// Recalculates final stats based on: Base Stats + Flat Buffs + Percent Buffs.
    /// </summary>
    private void RecalculateAllStats()
    {
        // 1. Reset stats to their current base values.
        c_Stats.AD = baseAD;
        c_Stats.AP = baseAP;
        c_Stats.MS = baseMS;
        c_Stats.maxHP = baseMaxHP;
        c_Stats.AR = baseAR;
        c_Stats.MR = baseMR;
        c_Stats.KR = baseKR;
        c_Stats.lifestealPercent = baseLifestealPercent;

        // 2. Apply all temporary FLAT modifiers.
        foreach (var mod in activeModifiers.Where(m => m.Type == ModifierType.Flat))
        {
            ApplySingleStatChange(mod.Stat, mod.Value, mod.Type);
        }

        // 3. Apply all temporary PERCENT modifiers.
        // This now scales on top of the (base + flat) values.
        foreach (var mod in activeModifiers.Where(m => m.Type == ModifierType.Percent))
        {
            ApplySingleStatChange(mod.Stat, mod.Value, mod.Type);
        }

        // Ensure current health is clamped after any MaxHP changes.
        c_Stats.currentHP = Mathf.Min(c_Stats.currentHP, c_Stats.maxHP);

        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// Applies a single stat change. Handles Flat vs Percent logic.
    /// </summary>
    private void ApplySingleStatChange(StatType stat, float value, ModifierType type)
    {
        if (type == ModifierType.Percent)
        {
            // For percentages, get the stat's current value and multiply.
            float currentValue = GetCurrentStatValue(stat);
            value = currentValue * value; // e.g., 150 * 0.10 = 15
        }

        switch (stat)
        {
            case StatType.Heal:             c_Health.ChangeHealth((int)value); break;
            case StatType.AttackDamage:     c_Stats.AD += (int)value; break;
            case StatType.AbilityPower:     c_Stats.AP += (int)value; break;
            case StatType.MoveSpeed:        c_Stats.MS += value; break;
            case StatType.MaxHealth:        c_Stats.maxHP += (int)value; break;
            case StatType.Armor:            c_Stats.AR += (int)value; break;
            case StatType.MagicResist:      c_Stats.MR += (int)value; break;
            case StatType.KnockbackResist:  c_Stats.KR += value; break;
            case StatType.LifestealPercent: c_Stats.lifestealPercent += value; break;
        }
    }

    // Helper to get the LIVE value of a stat for percentage calculations.
    private float GetCurrentStatValue(StatType stat)
    {
        switch (stat)
        {
            case StatType.AttackDamage: return c_Stats.AD;
            case StatType.AbilityPower: return c_Stats.AP;
            case StatType.MoveSpeed: return c_Stats.MS;
            case StatType.MaxHealth: return c_Stats.maxHP;
            case StatType.Armor: return c_Stats.AR;
            case StatType.MagicResist: return c_Stats.MR;
            case StatType.KnockbackResist: return c_Stats.KR;
            case StatType.LifestealPercent: return c_Stats.lifestealPercent;
            default: return 0;
        }
    }

    private IEnumerator RevertModifierAfterDelay(StatModifier modifier)
    {
        yield return new WaitForSeconds(modifier.Duration);
        activeModifiers.Remove(modifier);
        RecalculateAllStats();
    }

    private IEnumerator ApplyOverTimeEffect(StatModifier modifier)
    {
        // Rule: Only allow ticking effects for Heal.
        if (modifier.Stat != StatType.Heal)
        {
            Debug.LogWarning($"Stat Modifier for {modifier.Stat} has IsOverTime=true, but this is only supported for Heal. Ignoring.", this);
            yield break;
        }

        float timePassed = 0;
        while (timePassed < modifier.Duration)
        {
            yield return new WaitForSeconds(1.0f);
            // Apply one "tick" of the effect. It's always Flat for ticking effects.
            ApplySingleStatChange(modifier.Stat, modifier.Value, ModifierType.Flat);
            timePassed += 1.0f;
        }
    }
}