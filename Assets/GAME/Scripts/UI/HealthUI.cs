using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public C_Stats        p_Stats;
    public C_Health       p_Health;
    public P_StatsManager p_StatsManager;
    public Slider         healthSlider;
    public TMP_Text       healthText;

    void Awake()
    {
        p_Stats        ??= FindFirstObjectByType<C_Stats>();
        p_Health       ??= FindFirstObjectByType<C_Health>();
        p_StatsManager ??= FindFirstObjectByType<P_StatsManager>();

        if (!p_Stats)        Debug.LogError("HealthUI: C_Stats is missing.");
        if (!p_Health)       Debug.LogError("HealthUI: C_Health is missing.");
        if (!p_StatsManager) Debug.LogError("HealthUI: P_StatsManager is missing.");
        if (!healthSlider)   Debug.LogError("HealthUI: healthSlider is missing.");

        healthSlider.maxValue = p_Stats.maxHP;
        healthSlider.value    = p_Stats.currentHP;
    }

    void OnEnable()
    {
        if (p_Health != null && p_StatsManager != null)
        {
            p_Health.OnDamaged += HandleHealthChanged;
            p_Health.OnHealed  += HandleHealthChanged;
            p_Health.OnDied    += UpdateUI;

            p_StatsManager.OnStatsChanged += UpdateUI;

            UpdateUI();
        }
    }

    void OnDisable()
    {
        if (p_Health != null && p_StatsManager != null)
        {
            p_Health.OnDamaged -= HandleHealthChanged;
            p_Health.OnHealed  -= HandleHealthChanged;
            p_Health.OnDied    -= UpdateUI;

            p_StatsManager.OnStatsChanged -= UpdateUI;
        }
    }

    // A helper method to match the signature of OnDamaged and OnHealed events
    void HandleHealthChanged(int amount)
    {
        UpdateUI();
    }

    // Update the health UI elements
    public void UpdateUI()
    {
        // Update the slider's max value and current value
        healthSlider.maxValue = p_Stats.maxHP;
        healthSlider.value    = p_Stats.currentHP;

        healthText.text = $"{p_Stats.currentHP} / {p_Stats.maxHP}";
    }
}
