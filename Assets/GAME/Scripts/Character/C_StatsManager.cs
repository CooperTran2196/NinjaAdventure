// d:\Google Drive\Code\Final project\NinjaAdventure\Assets\GAME\Scripts\Character\C_StatsManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DisallowMultipleComponent]
public class C_StatsManager : MonoBehaviour
{
    [Header("Component References")]
    public C_Stats c_Stats;
    public C_Health c_Health;

    // --- CORRECTED: Store base stats in simple variables ---
    private int baseAD, baseAP, baseMaxHP, baseAR, baseMR;
    private float baseMS, baseKR, baseLifestealPercent;

    private readonly List<StatModifier> activeModifiers = new List<StatModifier>();

    void Awake()
    {
        c_Stats ??= GetComponent<C_Stats>();
        c_Health ??= GetComponent<C_Health>();

        if (!c_Stats) Debug.LogError($"{name}: C_Stats is missing.", this);
        if (!c_Health) Debug.LogError($"{name}: C_Health is missing.", this);

        // --- CORRECTED: Snapshot base stats into the private variables ---
        baseAD = c_Stats.AD;
        baseAP = c_Stats.AP;
        baseMS = c_Stats.MS;
        baseMaxHP = c_Stats.maxHP;
        baseAR = c_Stats.AR;
        baseMR = c_Stats.MR;
        baseKR = c_Stats.KR;
        baseLifestealPercent = c_Stats.lifestealPercent;
    }

    public void ApplyModifier(StatModifier modifier)
    {
        if (modifier.IsOverTime)
        {
            StartCoroutine(ApplyOverTimeEffect(modifier));
        }
        else
        {
            activeModifiers.Add(modifier);
            RecalculateAllStats();

            if (modifier.Duration > 0)
            {
                StartCoroutine(RevertModifierAfterDelay(modifier));
            }
        }
    }

    private void RecalculateAllStats()
    {
        // 1. Reset stats to their base values
        c_Stats.AD = baseAD;
        c_Stats.AP = baseAP;
        c_Stats.MS = baseMS;
        c_Stats.maxHP = baseMaxHP;
        c_Stats.AR = baseAR;
        c_Stats.MR = baseMR;
        c_Stats.KR = baseKR;
        c_Stats.lifestealPercent = baseLifestealPercent;

        // 2. Apply all FLAT modifiers first
        foreach (var mod in activeModifiers.Where(m => m.Type == ModifierType.Flat))
        {
            ApplySingleStatChange(mod.Stat, mod.Value);
        }

        // 3. Apply all PERCENT modifiers next
        foreach (var mod in activeModifiers.Where(m => m.Type == ModifierType.Percent))
        {
            float bonus = GetBaseStatValue(mod.Stat) * mod.Value;
            ApplySingleStatChange(mod.Stat, bonus);
        }

        c_Stats.currentHP = Mathf.Min(c_Stats.currentHP, c_Stats.maxHP);
    }

    private void ApplySingleStatChange(StatType stat, float value)
    {
        switch (stat)
        {
            case StatType.Heal:
                c_Health.ChangeHealth((int)value);
                break;
            case StatType.AttackDamage:
                c_Stats.AD += (int)value;
                break;
            case StatType.AbilityPower:
                c_Stats.AP += (int)value;
                break;
            case StatType.MoveSpeed:
                c_Stats.MS += value;
                break;
            case StatType.MaxHealth:
                c_Stats.maxHP += (int)value;
                break;
            case StatType.Armor:
                c_Stats.AR += (int)value;
                break;
            case StatType.MagicResist:
                c_Stats.MR += (int)value;
                break;
            case StatType.KnockbackResist:
                c_Stats.KR += value;
                break;
            case StatType.LifestealPercent:
                c_Stats.lifestealPercent += value;
                break;
        }
    }

    private float GetBaseStatValue(StatType stat)
    {
        // --- CORRECTED: Read from the private base stat variables ---
        switch (stat)
        {
            case StatType.AttackDamage: return baseAD;
            case StatType.AbilityPower: return baseAP;
            case StatType.MoveSpeed: return baseMS;
            case StatType.MaxHealth: return baseMaxHP;
            case StatType.Armor: return baseAR;
            case StatType.MagicResist: return baseMR;
            case StatType.KnockbackResist: return baseKR;
            case StatType.LifestealPercent: return baseLifestealPercent;
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
        float timePassed = 0;
        while (timePassed < modifier.Duration)
        {
            yield return new WaitForSeconds(1.0f);
            ApplySingleStatChange(modifier.Stat, modifier.Value);
            timePassed += 1.0f;
        }
    }
}