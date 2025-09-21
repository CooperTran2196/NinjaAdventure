using System;
using UnityEngine;

// A central enum for all stats that can be modified
public enum StatName
{
    // Core Stats
    AttackDamage,
    AbilityPower,
    MoveSpeed,
    MaxHealth,
    Armor,
    MagicResist,
    ArmorPen,
    MagicPen,
    KnockbackResist,

    // Special Stats
    Lifesteal,

    // Consumable Stats
    Heal,
}

// Core definition for any change to a stat
[Serializable]
public class StatEffect
{
    public StatName statName;
    public float Value;

    [Header("0 = Permanent, 1 = Instant, >1 = Timed Effect")]
    public int Duration = 1;

    [Header("Tick Effect (for Duration > 1, Only Heal)")]
    [Header("True = Repeatedly, once per second")]
    [Header("False = One-time at start")]
    public bool IsOverTime = false;
}
