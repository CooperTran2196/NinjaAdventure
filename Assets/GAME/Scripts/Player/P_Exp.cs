using UnityEngine;
using System;

[DisallowMultipleComponent]
public class P_Exp : MonoBehaviour
{
    [Header("Independent component to manage Player's XP, Level, and Skill Points")]
    [Header("Progression Stats")]
    public int level = 1;
    public int currentExp = 0;
    public int skillPoints = 0;
    public int totalKills = 0;
    public float playTime = 0f;

    [Header("XP Curve Settings")]
    public int xpBase = 60;      // L1 -> L2
    public int xpStep = 30;      // linear add per level
    public int skillPointsPerLevel = 2;

    [Header("Debug")]
    public int debugXPAmount = 20;

    public event Action<int>        OnLevelUp;
    public event Action<int,int>    OnXPChanged;
    public event Action<int>        OnSPChanged;

    void Start()
    {
        OnXPChanged?.Invoke(currentExp, GetXPRequiredForNext());
    }

    void Update()
    {
        // Track total time played
        playTime += Time.deltaTime;
    }

    public void AddDebugXP()
    {
        AddXP(debugXPAmount);
    }

    // Called by enemies when they die
    public void AddKill()
    {
        totalKills++;
    }

    // Add XP and handle level up
    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        // Add XP and check for level up
        currentExp += amount;
        int req = GetXPRequiredForNext();

        // Level up while enough XP for next level
        while (currentExp >= req)
        {
            currentExp -= req;
            level++;
            skillPoints += skillPointsPerLevel;

            OnLevelUp?.Invoke(level);
            OnSPChanged?.Invoke(skillPoints);

            req = GetXPRequiredForNext();
        }

        OnXPChanged?.Invoke(currentExp, req);
    }

    // Return true if successfully spent points
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

    // XP required for next level
    public int GetXPRequiredForNext()
    {
        int n = Mathf.Max(1, level);
        return xpBase + xpStep * (n - 1);
    }
}
