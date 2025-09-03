using System;
using UnityEngine;

[DisallowMultipleComponent]
public class C_Health : MonoBehaviour
{
    [Header("Assign exactly one")]
    public P_Stats pStats;
    public E_Stats eStats;

    [Header("Debug Keys (Debug action map)")]
    public int takingDamageAmount = 1; // N
    public int healingAmount = 1;      // B

    [Header("FX (optional)")]
    public C_FX fx; // drag & drop

    // Events
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDied;

    // Accessors
    int MaxHP => pStats ? pStats.maxHP : eStats.maxHP;
    int CurrentHP { get => pStats ? pStats.currentHP : eStats.currentHP;
                    set { if (pStats) pStats.currentHP = value; else eStats.currentHP = value; } }
    public int AR => pStats ? pStats.AR : eStats.AR;
    public int MR => pStats ? pStats.MR : eStats.MR;
    public bool IsAlive => CurrentHP > 0;

    P_InputActions input;

    // cached delegates so we can unsubscribe
    System.Action<int> fxDamagedHandler;
    System.Action<int> fxHealedHandler;
    System.Action      fxDiedHandler;

    void Awake()
    {
        if (!pStats && !eStats) Debug.LogError($"{name}: C_Health needs P_Stats or E_Stats.", this);
        if (!fx) Debug.LogWarning($"{name}: C_FX not assigned; no flashes / death fade.", this);
    }

    void OnEnable()
    {
        input ??= new P_InputActions();
        input.Debug.Enable();

        if (fx != null)
        {
            fxDamagedHandler ??= _ => fx.FlashOnDamaged();
            fxHealedHandler  ??= _ => fx.FlashOnHealed();
            fxDiedHandler    ??= () => StartCoroutine(fx.FadeAndDestroy(gameObject));

            OnDamaged += fxDamagedHandler;
            OnHealed  += fxHealedHandler;
            OnDied    += fxDiedHandler;
        }
    }

    void OnDisable()
    {
        input?.Debug.Disable();

        if (fx != null)
        {
            OnDamaged -= fxDamagedHandler;
            OnHealed  -= fxHealedHandler;
            OnDied    -= fxDiedHandler;
        }
    }

    void Update()
    {
        if (input.Debug.OnDamaged.WasPressedThisFrame())
            ChangeHealth(-Mathf.Abs(takingDamageAmount));
        if (input.Debug.OnHealed.WasPressedThisFrame())
            ChangeHealth(Mathf.Abs(healingAmount));
    }

    // AD+AP combined calculation (armor/mres as % 0–100)
    public void ApplyDamage(int attackerAD, int attackerAP, int weaponAD, int weaponAP)
    {
        if (!IsAlive) return;

        int reqPhysical = Mathf.Max(0, attackerAD + weaponAD);
        int reqAbility  = Mathf.Max(0, attackerAP + weaponAP);

        float physRed = Mathf.Clamp01(AR / 100f);
        float abilRed = Mathf.Clamp01(MR / 100f);

        int phys = Mathf.RoundToInt(reqPhysical * (1f - physRed));
        int abil = Mathf.RoundToInt(reqAbility  * (1f - abilRed));
        int total = phys + abil;

        if (total > 0) ChangeHealth(-total);
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
