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
    MaxMana,
    Armor,
    MagicResist,
    ArmorPen,
    MagicPen,
    KnockbackResist,

    // Special Stats
    Lifesteal,

    // Weapon Bonus Stats
    SlashArcBonus,       // Additive bonus degrees to slash arc (1 = +1°)
    MovePenaltyReduction, // Percentage reduction of movement penalties (1 = 1% less penalty)
    StunTimeBonus,       // Percentage increase to stun times (1 = 1% more stun)
    ThrustDistanceBonus, // Percentage increase to thrust distance (1 = 1% more distance)

    // Consumable Stats
    Heal,
    Mana, // Restore mana (like Heal for HP)
}

// Core definition for any change to a stat
[Serializable]
public class P_StatEffect
{
    [Header("Flat: AD, AP, MS, MaxHP, MaxMP, AR, MR, KR, SlashArc, Heal, Mana")]
    [Header("Percentage 1=1%: Lifesteal, ArmorPen, MagicPen, MovePenalty, StunTime, ThrustDistance")]
    public StatName statName;
    public float Value;

    [Header("0 = Permanent, 1 = Instant, >1 = Timed Effect")]
    public int Duration = 1;

    [Header("Tick Effect (for Duration > 1, Only Heal)")]
    [Header("True = Repeatedly, once per second")]
    [Header("False = One-time at start")]
    public bool IsOverTime = false;
}
