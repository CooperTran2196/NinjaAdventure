using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[DisallowMultipleComponent]
public class P_StatsManager : MonoBehaviour
{
    [Header("Depend on C_Stats and C_Health")]

    [Header("References")]
    C_Stats c_Stats;
    C_Health c_Health;

    // Base stats that can be modified
    private int baseAD, baseAP, baseMaxHP, baseAR, baseMR;
    private float baseMS, baseKR, baseLifesteal, baseArmorPen, baseMagicPen;

    // List of temporary buffs/debuffs.
    private readonly List<P_StatEffect> statsEffectList = new List<P_StatEffect>();

    public event Action OnStatsChanged;

    void Awake()
    {
        c_Stats  ??= GetComponent<C_Stats>();
        c_Health ??= GetComponent<C_Health>();

        if (!c_Stats)  Debug.LogError($"{name}: C_Stats in C_StatsManager is missing.", this);
        if (!c_Health) Debug.LogError($"{name}: C_Health in C_StatsManager is missing.", this);

        // Make copy of basic Stats
        baseAD      = c_Stats.AD;
        baseAP      = c_Stats.AP;
        baseMS      = c_Stats.MS;
        baseMaxHP   = c_Stats.maxHP;
        baseAR      = c_Stats.AR;
        baseMR      = c_Stats.MR;
        baseKR      = c_Stats.KR;

        // Special stats
        baseLifesteal = c_Stats.lifesteal;
        baseArmorPen  = c_Stats.armorPen;
        baseMagicPen  = c_Stats.magicPen;
    }

    /// The main entry point for applying any stat change
    public void ApplyModifier(P_StatEffect stat)
    {
        // Duration == 0 is a PERMANENT
        if (stat.Duration == 0)
        {
            ApplyPermanentEffect(stat);
            RecalculateAllStats();
            return;
        }

        // Duration == 1 is an INSTANT, treat as permanent for stat boosts
        if (stat.Duration == 1)
        {
            // Instantaneous effects like Heal are handled directly
            if (stat.statName == StatName.Heal)
            {
                CommitStatChange(stat.statName, stat.Value);
            }
            // Other stat changes are made permanent
            else
            {
                ApplyPermanentEffect(stat);
                RecalculateAllStats();
            }
            return;
        }

        // Duration > 1 is a OVER TIME effect
        if (stat.Duration > 1)
        {
            if (stat.IsOverTime)
            {
                // Repeatedly, once per second
                StartCoroutine(ApplyOverTimeEffect(stat));
            }
            else
            {
                // One-time at start
                statsEffectList.Add(stat);
                RecalculateAllStats();
                StartCoroutine(StartEffectTimer(stat));
            }
        }
    }

    /// Handles permanent stat changes
    private void ApplyPermanentEffect(P_StatEffect stat)
    {
        // Apply the permanent change to the BASE stat
        switch (stat.statName)
        {
            case StatName.AttackDamage:     baseAD      += (int)stat.Value; break;
            case StatName.AbilityPower:     baseAP      += (int)stat.Value; break;
            case StatName.MoveSpeed:        baseMS      +=      stat.Value; break;
            case StatName.MaxHealth:        baseMaxHP   += (int)stat.Value; break;
            case StatName.Armor:            baseAR      += (int)stat.Value; break;
            case StatName.MagicResist:      baseMR      += (int)stat.Value; break;
            case StatName.KnockbackResist:  baseKR      +=      stat.Value; break;

            case StatName.Lifesteal:        baseLifesteal += stat.Value; break;
            case StatName.ArmorPen:         baseArmorPen  += stat.Value; break;
            case StatName.MagicPen:         baseMagicPen  += stat.Value; break;
            case StatName.Heal:             c_Health.ChangeHealth((int)stat.Value); break; // Permanent heal is just an instant heal
        }
    }

    /// Recalculates final stats based on: Base Stats + Flat Buffs
    private void RecalculateAllStats()
    {
        // 1/ Reset stats to their current base values
        c_Stats.AD      = baseAD;
        c_Stats.AP      = baseAP;
        c_Stats.MS      = baseMS;
        c_Stats.maxHP   = baseMaxHP;
        c_Stats.AR      = baseAR;
        c_Stats.MR      = baseMR;
        c_Stats.KR      = baseKR;

        c_Stats.lifesteal = baseLifesteal;
        c_Stats.armorPen  = baseArmorPen;
        c_Stats.magicPen  = baseMagicPen;

        // 2/ Find all active effects and apply them
        foreach (var stat in statsEffectList)
        {
            CommitStatChange(stat.statName, stat.Value);
        }

        // Ensure current health is clamped after any MaxHP changes.
        c_Stats.currentHP = Mathf.Min(c_Stats.currentHP, c_Stats.maxHP);

        OnStatsChanged?.Invoke();
    }

    /// Applies temporary adjustments on top of the base stats
    private void CommitStatChange(StatName statName, float value)
    {
        switch (statName)
        {

            case StatName.AttackDamage:     c_Stats.AD      += (int)value; break;
            case StatName.AbilityPower:     c_Stats.AP      += (int)value; break;
            case StatName.MoveSpeed:        c_Stats.MS      +=      value; break;
            case StatName.MaxHealth:        c_Stats.maxHP   += (int)value; break;
            case StatName.Armor:            c_Stats.AR      += (int)value; break;
            case StatName.MagicResist:      c_Stats.MR      += (int)value; break;
            case StatName.KnockbackResist:  c_Stats.KR      +=      value; break;

            case StatName.Lifesteal:        c_Stats.lifesteal += value; break;
            case StatName.ArmorPen:         c_Stats.armorPen  += value; break;
            case StatName.MagicPen:         c_Stats.magicPen  += value; break;
            case StatName.Heal:             c_Health.ChangeHealth((int)value); break;
        }
    }

    // Timer for temporary effects
    private IEnumerator StartEffectTimer(P_StatEffect stat)
    {
        yield return new WaitForSeconds(stat.Duration);
        statsEffectList.Remove(stat);
        RecalculateAllStats();
    }

    // Over-time effects (1 per second)
    private IEnumerator ApplyOverTimeEffect(P_StatEffect stat)
    {
        // RULE: Only allow ticking effects for Heal
        if (stat.statName != StatName.Heal)
        {
            Debug.LogWarning($"Stat Effect for {stat.statName} has IsOverTime=true, but this is only supported for Heal. Ignoring.", this);
            yield break;
        }

        // Repeat once per second for the duration
        int timePassed = 0;
        while (timePassed < stat.Duration)
        {
            yield return new WaitForSeconds(1);
            // Apply one "tick" of the effect
            CommitStatChange(stat.statName, stat.Value);
            timePassed += 1;
        }
    }
}