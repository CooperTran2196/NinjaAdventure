using UnityEngine;
using System;

[DisallowMultipleComponent]
public class P_Exp : MonoBehaviour
{
    [Header("Independent component to manage Player's XP, Level, and Skill Points")]
    [Header("Linear XP (tweak in Inspector)")]
    public int level = 1;
    public int currentXP = 0;
    public int skillPoints = 0;
    public int xpBase = 60;      // L1→L2
    public int xpStep = 30;      // linear add per level
    public int skillPointsPerLevel = 2;

    [Header("Debug")]
    public int debugXPAmount = 20;

    public event Action<int>        OnLevelUp;
    public event Action<int,int>    OnXPChanged;
    public event Action<int>        OnSPChanged;

    void Start()
    {
        OnXPChanged?.Invoke(currentXP, GetXPRequiredForNext());
    }

    public void AddDebugXP()
    {
        AddXP(debugXPAmount);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;
        int req = GetXPRequiredForNext();

        while (currentXP >= req)
        {
            currentXP -= req;
            level++;
            skillPoints += skillPointsPerLevel;

            OnLevelUp?.Invoke(level);
            OnSPChanged?.Invoke(skillPoints);

            req = GetXPRequiredForNext();
        }

        OnXPChanged?.Invoke(currentXP, req);
    }

    public bool TrySpendSkillPoints(int amount)
    {
        // return false if not enough points
        if (amount <= 0) return false;
        if (skillPoints < amount) return false;

        // spend points and return true
        skillPoints -= amount;
        OnSPChanged?.Invoke(skillPoints);
        return true;
    }

    public int GetXPRequiredForNext()
    {
        int n = Mathf.Max(1, level);
        return xpBase + xpStep * (n - 1);
    }
}
