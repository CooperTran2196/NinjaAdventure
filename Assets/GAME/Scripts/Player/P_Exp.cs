using UnityEngine;
using System;

[DisallowMultipleComponent]
public class P_Exp : MonoBehaviour
{
    [Header("Linear XP (tweak in Inspector)")]
    public int level = 1;
    public int currentXP = 0;
    public int skillPoints = 0;
    public int xpBase = 60;      // L1→L2
    public int xpStep = 30;      // linear add per level
    public int skillPointsPerLevel = 2;

    [Header("Debug")]
    public bool debugStartAtLevel = false;
    public int  debugStartLevel   = 1;
    public bool debugGrantXPOnStart = false;
    public int  debugXPOnStart      = 0;

    public event Action<int> OnLevelUp;
    public event Action<int,int> OnXPChanged;
    public event Action<int> OnSkillPointsChanged;

    void Start()
    {
        if (debugStartAtLevel && debugStartLevel > level)
        {
            int target = debugStartLevel;
            for (int i = level; i < target; i++)
            {
                level++;
                skillPoints += skillPointsPerLevel;
                OnLevelUp?.Invoke(level);
            }
            OnSkillPointsChanged?.Invoke(skillPoints);
            OnXPChanged?.Invoke(currentXP, GetXPRequiredForNext());
        }

        if (debugGrantXPOnStart && debugXPOnStart > 0)
            AddXP(debugXPOnStart);
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
            OnSkillPointsChanged?.Invoke(skillPoints);

            req = GetXPRequiredForNext();
        }

        OnXPChanged?.Invoke(currentXP, req);
    }

    public bool TrySpendSkillPoints(int amount)
    {
        // return false if not enough points
        if (amount <= 0) return false;
        if (skillPoints < amount) return false;

        skillPoints -= amount;
        OnSkillPointsChanged?.Invoke(skillPoints);
        return true;
    }

    public int GetXPRequiredForNext()
    {
        int n = Mathf.Max(1, level);
        return xpBase + xpStep * (n - 1);
    }
}
