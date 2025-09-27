using System;
using UnityEngine;

[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_FX))]
[DisallowMultipleComponent]
public class C_Health : MonoBehaviour
{
    [Header("References")]
    public C_Stats c_Stats;
    public C_Dodge c_Dodge;
    C_FX fx;

    P_InputActions input;
    [Header("Allow Dodge/IFrames? (Only for Player)")]
    public bool useDodgeIFrames = true;

    [Header("Debug Keys (N/B)")]
    public int takingDamageAmount = 1;
    public int healingAmount = 1;

    // Events
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDied;

    // Accessors
    int CurrentHP { get => c_Stats.currentHP; set => c_Stats.currentHP = value; }
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

        if (!c_Stats)                    Debug.LogError($"{name}: C_Stats in C_Health missing.", this);
        if (!c_Dodge && useDodgeIFrames) Debug.LogError($"{name}: C_Dodge in C_Health missing.", this);
        if (!fx)                         Debug.LogWarning($"{name}: C_FX not assigned; no flashes / death fade.", this);
    }

    void OnEnable()
    {
        input ??= new P_InputActions();
        input.Debug.Enable();
        
        // subscribe to FX events
        fxDamagedHandler ??= _ => fx.FlashOnDamaged();
        fxHealedHandler  ??= _ => fx.FlashOnHealed();
        fxDiedHandler    ??= () => StartCoroutine(fx.FadeAndDestroy(gameObject));

        OnDamaged += fxDamagedHandler;
        OnHealed  += fxHealedHandler;
        OnDied    += fxDiedHandler;
    }

    void OnDisable()
    {
        input?.Debug.Disable();

        OnDamaged -= fxDamagedHandler;
        OnHealed  -= fxHealedHandler;
        OnDied    -= fxDiedHandler;
    }

    void Update()
    {
        // Debug keys
        if (input.Debug.OnDamaged.WasPressedThisFrame())
            ChangeHealth(-Mathf.Abs(takingDamageAmount));

        if (input.Debug.OnHealed.WasPressedThisFrame())
            ChangeHealth(Mathf.Abs(healingAmount));
    }

    // AD+AP combined calculation (armor/mres as % 0–100)
    public int ApplyDamage(int attackerAD, int attackerAP, int weaponAD, int weaponAP, float attackerArmorPen, float attackerMagicPen)
    {
        // Ignore if dead or dodging with IFrames
        if (!IsAlive) return 0;
        if (useDodgeIFrames && c_Dodge.IsDodging) return 0;

        // Calculate effective armor and magic resist after penetration
        float effectiveAR = c_Stats.AR * (1f - Mathf.Clamp01(attackerArmorPen / 100f));
        float effectiveMR = c_Stats.MR * (1f - Mathf.Clamp01(attackerMagicPen / 100f));

        // Calculate damage reduction from effective armor and magic resist
        float damageReductionAR = 1f - Mathf.Clamp01(effectiveAR / 100f);
        float damageReductionMR = 1f - Mathf.Clamp01(effectiveMR / 100f);

        // Final damage calculation
        int total =
            Mathf.RoundToInt((attackerAD + weaponAD) * damageReductionAR) +
            Mathf.RoundToInt((attackerAP + weaponAP) * damageReductionMR);

        // Clamp to valid range and apply
        int before = CurrentHP;
        int dealt = Mathf.Clamp(total, 0, before);
        if (dealt > 0) ChangeHealth(-dealt);
        return dealt;
    }

    // Single entrypoint for damage/heal
    public void ChangeHealth(int amount)
    {
        // Ignore if dead, healing 0, or dodging with IFrames
        if (!IsAlive || (amount < 0 && useDodgeIFrames && c_Dodge.IsDodging)) return;

        // Clamp to valid range and apply
        int before    = CurrentHP;
        int after     = Mathf.Clamp(before + amount, 0, c_Stats.maxHP);
        int actual    = after - before;
            CurrentHP = after;

        // Invoke events
        if      (actual < 0) OnDamaged?.Invoke(-actual);
        else if (actual > 0) OnHealed?.Invoke(actual);
        if      (after == 0) OnDied?.Invoke();
    }

    // Kill instantly
    public void Kill() => ChangeHealth(-c_Stats.maxHP);
}
