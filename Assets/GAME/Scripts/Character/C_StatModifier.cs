using System;
using UnityEngine;

// A central enum for all stats that can be modified.
public enum StatType
{
    // Core Stats
    AttackDamage,
    AbilityPower,
    MoveSpeed,
    MaxHealth,
    Armor,
    MagicResist,
    KnockbackResist,

    // Special Stats
    LifestealPercent,

    // Consumable-only Stats
    Heal,
}

// NEW: Defines how a modifier's value is applied.
public enum ModifierType
{
    Flat,       // Adds a fixed value (e.g., +10 Health).
    Percent,    // Adds a percentage of the stat's base value (e.g., +10% Health).
}

// Core definition for any change to a stat
[Serializable]
public class StatModifier
{
    public StatType Stat;
    public ModifierType Type; // Is this a Flat or Percent modifier?
    public float Value;

    [Header("0=Permanent, 1=Instant, >1=Timed Effect")]
    public float Duration;

    [Header("Tick Effect (for Duration > 1)")]
    public bool IsOverTime; // If true, Value is applied every second for Duration.
}
