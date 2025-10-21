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
    C_Stats  c_Stats;
    C_Health c_Health;
    C_Mana   c_Mana;

    // Base stats that can be modified
    private int baseAD, baseAP, baseMaxHP, baseMaxMP, baseAR, baseMR;
    private float baseMS, baseKR, baseLifesteal, baseArmorPen, baseMagicPen;
    
    // Weapon bonus base values
    private float baseSlashArcBonus, baseMovePenaltyReduction, baseStunTimeBonus, baseThrustDistanceBonus;

    // List of temporary buffs/debuffs.
    private readonly List<P_StatEffect> statsEffectList = new List<P_StatEffect>();

    public event Action OnStatsChanged;

    void Awake()
    {
        c_Stats  ??= GetComponent<C_Stats>();
        c_Health ??= GetComponent<C_Health>();
        c_Mana   ??= GetComponent<C_Mana>();

        if (!c_Stats)  Debug.LogError($"{name}: C_Stats is missing in P_StatsManager");
        if (!c_Health) Debug.LogError($"{name}: C_Health is missing in P_StatsManager");
        if (!c_Mana)   Debug.LogWarning($"{name}: C_Mana is missing in P_StatsManager (mana stats will not work)");

        // Make copy of basic Stats
        baseAD      = c_Stats.AD;
        baseAP      = c_Stats.AP;
        baseMS      = c_Stats.MS;
        baseMaxHP   = c_Stats.maxHP;
        baseMaxMP   = c_Stats.maxMP;
        baseAR      = c_Stats.AR;
        baseMR      = c_Stats.MR;
        baseKR      = c_Stats.KR;

        // Special stats
        baseLifesteal = c_Stats.lifesteal;
        baseArmorPen  = c_Stats.armorPen;
        baseMagicPen  = c_Stats.magicPen;

        // Weapon bonuses
        baseSlashArcBonus        = c_Stats.slashArcBonus;
        baseMovePenaltyReduction = c_Stats.movePenaltyReduction;
        baseStunTimeBonus        = c_Stats.stunTimeBonus;
        baseThrustDistanceBonus  = c_Stats.thrustDistanceBonus;
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
            // Instantaneous effects like Heal and Mana are handled directly
            if (stat.statName == StatName.Heal)
            {
                CommitStatChange(stat.statName, stat.Value);
                SYS_GameManager.Instance.sys_SoundManager.PlayInstantHeal();
            }
            else if (stat.statName == StatName.Mana)
            {
                CommitStatChange(stat.statName, stat.Value);
            }
            // Other stat changes are made permanent
            else
            {
                ApplyPermanentEffect(stat);
                RecalculateAllStats();
                PlayBuffSound(stat.statName); // Play appropriate buff sound
            }
            return;
        }

        // Duration > 1 is a OVER TIME effect
        if (stat.Duration > 1)
        {
            if (stat.IsOverTime)
            {
                // Play overtime heal sound once at the start
                if (stat.statName == StatName.Heal)
                    SYS_GameManager.Instance.sys_SoundManager.PlayOvertimeHeal();
                
                // Repeatedly, once per second
                StartCoroutine(ApplyOverTimeEffect(stat));
            }
            else
            {
                // One-time at start
                statsEffectList.Add(stat);
                RecalculateAllStats();
                PlayBuffSound(stat.statName); // Play appropriate buff sound
                StartCoroutine(StartEffectTimer(stat));
            }
        }
    }

    // Handles permanent stat changes
    private void ApplyPermanentEffect(P_StatEffect stat)
    {
        // Apply the permanent change to the BASE stat
        switch (stat.statName)
        {
            case StatName.AttackDamage:     baseAD      += (int)stat.Value; break;
            case StatName.AbilityPower:     baseAP      += (int)stat.Value; break;
            case StatName.MoveSpeed:        baseMS      +=      stat.Value; break;
            case StatName.MaxHealth:        baseMaxHP   += (int)stat.Value; break;
            case StatName.MaxMana:          baseMaxMP   += (int)stat.Value; break;
            case StatName.Armor:            baseAR      += (int)stat.Value; break;
            case StatName.MagicResist:      baseMR      += (int)stat.Value; break;
            case StatName.KnockbackResist:  baseKR      +=      stat.Value; break;

            case StatName.Lifesteal:        baseLifesteal += stat.Value; break;
            case StatName.ArmorPen:         baseArmorPen  += stat.Value; break;
            case StatName.MagicPen:         baseMagicPen  += stat.Value; break;

            case StatName.SlashArcBonus:        baseSlashArcBonus        += stat.Value; break;
            case StatName.MovePenaltyReduction: baseMovePenaltyReduction += stat.Value; break;
            case StatName.StunTimeBonus:        baseStunTimeBonus        += stat.Value; break;
            case StatName.ThrustDistanceBonus:  baseThrustDistanceBonus  += stat.Value; break;

            case StatName.Heal:             c_Health.ChangeHealth((int)stat.Value); break; // Permanent heal is just an instant heal
            case StatName.Mana:             c_Mana.RestoreMana((int)stat.Value); 
                break; // Permanent mana is just an instant restore
        }
    }

    /// Recalculates final stats based on: Base Stats + Flat Buffs
    public void RecalculateAllStats()
    {
        // 1/ Reset stats to their current base values
        c_Stats.AD      = baseAD;
        c_Stats.AP      = baseAP;
        c_Stats.MS      = baseMS;
        c_Stats.maxHP   = baseMaxHP;
        c_Stats.maxMP   = baseMaxMP;
        c_Stats.AR      = baseAR;
        c_Stats.MR      = baseMR;
        c_Stats.KR      = baseKR;

        c_Stats.lifesteal = baseLifesteal;
        c_Stats.armorPen  = baseArmorPen;
        c_Stats.magicPen  = baseMagicPen;

        c_Stats.slashArcBonus        = baseSlashArcBonus;
        c_Stats.movePenaltyReduction = baseMovePenaltyReduction;
        c_Stats.stunTimeBonus        = baseStunTimeBonus;
        c_Stats.thrustDistanceBonus  = baseThrustDistanceBonus;

        // 2/ Find all active effects and apply them
        foreach (var stat in statsEffectList)
        {
            CommitStatChange(stat.statName, stat.Value);
        }

        // Ensure current health is clamped after any MaxHP changes.
        c_Stats.currentHP = Mathf.Min(c_Stats.currentHP, c_Stats.maxHP);
        
        // Ensure current mana is clamped after any MaxMP changes.
        c_Stats.currentMP = Mathf.Min(c_Stats.currentMP, c_Stats.maxMP);

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
            case StatName.MaxMana:          c_Stats.maxMP   += (int)value; break;
            case StatName.Armor:            c_Stats.AR      += (int)value; break;
            case StatName.MagicResist:      c_Stats.MR      += (int)value; break;
            case StatName.KnockbackResist:  c_Stats.KR      +=      value; break;

            case StatName.Lifesteal:        c_Stats.lifesteal += value; break;
            case StatName.ArmorPen:         c_Stats.armorPen  += value; break;
            case StatName.MagicPen:         c_Stats.magicPen  += value; break;

            case StatName.SlashArcBonus:        c_Stats.slashArcBonus        += value; break;
            case StatName.MovePenaltyReduction: c_Stats.movePenaltyReduction += value; break;
            case StatName.StunTimeBonus:        c_Stats.stunTimeBonus        += value; break;
            case StatName.ThrustDistanceBonus:  c_Stats.thrustDistanceBonus  += value; break;

            case StatName.Heal:             c_Health.ChangeHealth((int)value); break;
            case StatName.Mana:             c_Mana.RestoreMana((int)value); 
            break;
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
        // RULE: Only allow ticking effects for Heal and Mana
        if (stat.statName != StatName.Heal && stat.statName != StatName.Mana)
        {
            Debug.LogWarning($"Stat Effect for {stat.statName} has IsOverTime=true, but this is only supported for Heal and Mana. Ignoring.", this);
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

    // Helper to play appropriate buff sound based on stat type
    private void PlayBuffSound(StatName statName)
    {
        var soundManager = SYS_GameManager.Instance.sys_SoundManager;
        
        switch (statName)
        {
            case StatName.AttackDamage:
            case StatName.AbilityPower:
                soundManager.PlayBuffAttack();
                break;
            
            case StatName.Armor:
            case StatName.MagicResist:
                soundManager.PlayBuffDefense();
                break;
            
            default:
                soundManager.PlayBuffGeneric();
                break;
        }
    }
}