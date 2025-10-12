using System;
using UnityEngine;

public class C_Mana : MonoBehaviour
{
    [Header("References")]
    public C_Stats stats;

    public event Action<int> OnManaChanged;
    public event Action OnManaEmpty;

    public int  CurrentMana => stats.currentMP;
    public int  MaxMana => stats.maxMP;
    public bool HasMana => stats.currentMP > 0;

    void Awake()
    {
        stats ??= GetComponent<C_Stats>();

        if (!stats) Debug.LogError("C_Mana: C_Stats is missing.");
    }

    // Consume mana (returns true if successful, false if not enough mana)
    public bool ConsumeMana(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"C_Mana: Tried to consume negative mana: {amount}");
            return false;
        }

        if (stats.currentMP < amount)
        {
            return false; // Not enough mana
        }

        stats.currentMP -= amount;
        OnManaChanged?.Invoke(-amount);

        if (stats.currentMP <= 0)
        {
            stats.currentMP = 0;
            OnManaEmpty?.Invoke();
        }

        return true;
    }

    // Restore mana (healing/regen)
    public void RestoreMana(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"C_Mana: Tried to restore negative mana: {amount}");
            return;
        }

        int oldMana = stats.currentMP;
        stats.currentMP = Mathf.Min(stats.currentMP + amount, stats.maxMP);
        int actualRestored = stats.currentMP - oldMana;

        if (actualRestored > 0)
        {
            OnManaChanged?.Invoke(actualRestored);
        }
    }

    // Set mana to a specific value (for debugging/cheats)
    public void SetMana(int value)
    {
        int oldMana = stats.currentMP;
        stats.currentMP = Mathf.Clamp(value, 0, stats.maxMP);
        int delta = stats.currentMP - oldMana;

        if (delta != 0)
        {
            OnManaChanged?.Invoke(delta);
        }

        if (stats.currentMP <= 0)
        {
            OnManaEmpty?.Invoke();
        }
    }

    // Check if entity has enough mana
    public bool HasEnoughMana(int amount)
    {
        return stats.currentMP >= amount;
    }
}
