using System;
using UnityEngine;

[DisallowMultipleComponent]
public class C_Health : MonoBehaviour
{
    [Header("References (Only P or E _Stats)")]
    public C_Stats c_Stats;
    public C_Dodge c_Dodge;
    public C_FX fx;

    P_InputActions input;
    [Header("Allow Dodge/IFrames?")]
    public bool useDodgeIFrames = true;

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
        c_Dodge ??= GetComponent<C_Dodge>();
        fx      ??= GetComponent<C_FX>();

        if (!c_Stats) Debug.LogError($"{name}: C_Stats in C_Health missing.", this);
        if (!c_Dodge && useDodgeIFrames) Debug.LogError($"{name}: C_Dodge in C_Health missing.", this);
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
    public int ApplyDamage(int attackerAD, int attackerAP, int weaponAD, int weaponAP)
    {
        if (!IsAlive) return 0;
        if (useDodgeIFrames && c_Dodge.IsDodging) return 0;

        int total =
            Mathf.RoundToInt((attackerAD + weaponAD) * (1f - Mathf.Clamp01(AR / 100f))) +
            Mathf.RoundToInt((attackerAP + weaponAP) * (1f - Mathf.Clamp01(MR / 100f)));

        int before = CurrentHP;
        int dealt = Mathf.Clamp(total, 0, before);
        if (dealt > 0) ChangeHealth(-dealt);
        return dealt;
    }

    // Single entrypoint for damage/heal
    public void ChangeHealth(int amount)
    {
        if (!IsAlive || (amount < 0 && useDodgeIFrames && c_Dodge.IsDodging)) return;

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
