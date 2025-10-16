using System;
using UnityEngine;

public class C_Mana : MonoBehaviour
{
    [Header("References")]
    C_Stats c_Stats;

    public event Action<int> OnManaChanged;
    public event Action      OnManaEmpty;

    public int  CurrentMana => c_Stats.currentMP;
    public int  MaxMana     => c_Stats.maxMP;
    public bool HasMana     => c_Stats.currentMP > 0;

    void Awake()
    {
        c_Stats ??= GetComponent<C_Stats>();

        if (!c_Stats) { Debug.LogError($"{name}: C_Stats is missing!", this); return; }
    }

    // Consume mana (returns true if successful, false if not enough mana)
    public bool ConsumeMana(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"C_Mana: Tried to consume negative mana: {amount}");
            return false;
        }

        // Check if enough mana available
        if (c_Stats.currentMP < amount)
        {
            return false; // Not enough mana
        }

        // Deduct mana and notify listeners
        c_Stats.currentMP -= amount;
        OnManaChanged?.Invoke(-amount);

        // Check if mana depleted
        if (c_Stats.currentMP <= 0)
        {
            c_Stats.currentMP = 0;
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

        // Calculate actual mana restored (capped at maxMP)
        int oldMana = c_Stats.currentMP;
        c_Stats.currentMP = Mathf.Min(c_Stats.currentMP + amount, c_Stats.maxMP);
        int actualRestored = c_Stats.currentMP - oldMana;

        // Notify listeners if mana changed
        if (actualRestored > 0)
        {
            OnManaChanged?.Invoke(actualRestored);
        }
    }

    // Set mana to a specific value (for debugging/cheats)
    public void SetMana(int value)
    {
        // Clamp value between 0 and maxMP
        int oldMana = c_Stats.currentMP;
        c_Stats.currentMP = Mathf.Clamp(value, 0, c_Stats.maxMP);
        int delta = c_Stats.currentMP - oldMana;

        // Notify listeners if mana changed
        if (delta != 0)
        {
            OnManaChanged?.Invoke(delta);
        }

        // Check if mana depleted
        if (c_Stats.currentMP <= 0)
        {
            OnManaEmpty?.Invoke();
        }
    }

    // Check if entity has enough mana
    public bool HasEnoughMana(int amount)
    {
        return c_Stats.currentMP >= amount;
    }
}
