using System;
using UnityEngine;

[DisallowMultipleComponent]
public class C_Health : MonoBehaviour
{
    [Header("References (Only P or E _Stats)")]
    P_InputActions input;
    public C_Stats c_Stats;
    public C_FX fx;

    [Header("Debug Keys (N/B)")]
    public int takingDamageAmount = 1;
    public int healingAmount = 1;

    // Events
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDied;

    // Accessors
    int MaxHP    => c_Stats.maxHP;
    int CurrentHP { get => c_Stats.currentHP; set => c_Stats.currentHP = value; }
    public int AR => c_Stats.AR;
    public int MR => c_Stats.MR;
    public bool IsAlive => CurrentHP > 0;

    // cached delegates so we can unsubscribe
    Action<int> fxDamagedHandler;
    Action<int> fxHealedHandler;
    Action      fxDiedHandler;

    void Awake()
    {
        c_Stats ??= GetComponent<C_Stats>();
        fx      ??= GetComponent<C_FX>();

        if (!c_Stats) Debug.LogError($"{name}: C_Stats in C_Health missing.", this);
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

        int phys  = Mathf.RoundToInt(reqPhysical * (1f - physRed));
        int abil  = Mathf.RoundToInt(reqAbility  * (1f - abilRed));
        int total = phys + abil;

        if (total > 0) ChangeHealth(-total);
    }
    
    // Single entrypoint for damage/heal
    public void ChangeHealth(int amount)
    {
        if (!IsAlive) return;

        int before    = CurrentHP;
        int after     = Mathf.Clamp(before + amount, 0, MaxHP);
        int actual    = after - before;
            CurrentHP = after;

        if (actual < 0) OnDamaged?.Invoke(-actual);
        else if (actual > 0) OnHealed?.Invoke(actual);

        if (after == 0) OnDied?.Invoke();
    }

    public void Kill() => ChangeHealth(-MaxHP);
}
