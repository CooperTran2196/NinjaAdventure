using System;
using UnityEngine;

[DisallowMultipleComponent]
public class C_ChangeHealth : MonoBehaviour
{
    [Header("Assign exactly one")]
    public P_Stats pStats;
    public E_Stats eStats;

    // Unified events (deliver the ACTUAL applied amount)
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDied;

    // Quick accessors into whichever stats is assigned
    int MaxHP => pStats ? pStats.maxHP : eStats.maxHP;
    int CurrentHP
    {
        get => pStats ? pStats.currentHP : eStats.currentHP;
        set { if (pStats) pStats.currentHP = value; else eStats.currentHP = value; }
    }
    public int AR => pStats ? pStats.AR : eStats.AR;
    public int MR => pStats ? pStats.MR : eStats.MR;

    public bool IsAlive => CurrentHP > 0;

    void Awake()
    {
        if (!pStats && !eStats)
            Debug.LogWarning($"{name}: C_ChangeHealth needs P_Stats or E_Stats.", this);
    }

    public void ChangeHealth(int amount)
    {
        if (!IsAlive) return;

        int before = CurrentHP;
        int after  = Mathf.Clamp(before + amount, 0, MaxHP);
        int actual = after - before;

        CurrentHP = after;

        if (actual < 0) OnDamaged?.Invoke(-actual);
        else if (actual > 0) OnHealed?.Invoke(actual);

        if (after == 0) OnDied?.Invoke();
    }

    public void Kill() => ChangeHealth(-MaxHP);
}
