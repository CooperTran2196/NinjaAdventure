using System;
using UnityEngine;

[DisallowMultipleComponent]
public class C_Health : MonoBehaviour
{
    [Header("References")]
    C_Stats        c_Stats;
    P_State_Dodge  p_State_Dodge;
    C_FX           c_FX;
    P_InputActions input;

    [Header("Allow Dodge/IFrames? (Only for Player)")]
    public bool useDodgeIFrames = true;

    [Header("Debug Keys (N/B)")]
    public int takingDamageAmount = 1;
    public int healingAmount      = 1;

    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDied;

    Action<int> fxDamagedHandler;
    Action<int> fxHealedHandler;
    Action      fxDiedHandler;

    int         CurrentHP  { get => c_Stats.currentHP; set => c_Stats.currentHP = value; }
    public bool IsAlive    => CurrentHP > 0;
    bool        IsDodging  => useDodgeIFrames && p_State_Dodge != null && p_State_Dodge.enabled;

    void Awake()
    {
        c_Stats       ??= GetComponent<C_Stats>();
        p_State_Dodge ??= GetComponent<P_State_Dodge>();
        c_FX          ??= GetComponent<C_FX>();

        if (!c_Stats) { Debug.LogError($"{name}: C_Stats is missing!", this); return; }
        if (!c_FX)    { Debug.LogError($"{name}: C_FX is missing!", this); return; }
    }

    void OnEnable()
    {
        input ??= new P_InputActions();
        input.Debug.Enable();

        fxDamagedHandler = _ => c_FX.FlashOnDamaged();
        fxHealedHandler  = _ => c_FX.FlashOnHealed();
        fxDiedHandler    = () => StartCoroutine(c_FX.FadeAndDestroy(gameObject));

        OnDamaged += fxDamagedHandler;
        OnHealed  += fxHealedHandler;
        OnDied    += fxDiedHandler;
    }

    void OnDisable()
    {
        input.Debug.Disable();

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
        if (!IsAlive || IsDodging) return 0;

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
        if (dealt > 0)
        {
            ChangeHealth(-dealt);
            
            // Cancel combo if player is attacking (damage interrupts combo)
            var playerAttackState = GetComponent<P_State_Attack>();
            if (playerAttackState != null && playerAttackState.enabled)
            {
                playerAttackState.ResetCombo();
            }
        }
        return dealt;
    }

    // Single entrypoint for damage/heal
    public void ChangeHealth(int amount)
    {
        // Ignore if dead, healing 0, or dodging with IFrames
        if (!IsAlive || (amount < 0 && IsDodging)) return;

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
