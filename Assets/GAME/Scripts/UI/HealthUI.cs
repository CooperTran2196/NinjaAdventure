using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public C_Stats p_Stats;
    public C_Health p_Health;
    public P_StatsChanged p_StatsChanged;
    public Slider healthSlider;
    public TMP_Text healthText;

    void Awake()
    {
        // Find references if not assigned
        p_Stats ??= FindFirstObjectByType<C_Stats>();
        p_Health ??= FindFirstObjectByType<C_Health>();
        p_StatsChanged ??= FindFirstObjectByType<P_StatsChanged>();

        if (!p_Stats) Debug.LogError($"{name}: C_Stats in HealthUI missing.", this);
        if (!p_Health) Debug.LogError($"{name}: C_Health in HealthUI missing.", this);
        if (!p_StatsChanged) Debug.LogError($"{name}: P_StatsChanged in HealthUI missing.", this);
        if (!healthSlider) Debug.LogError($"{name}: healthSlider in HealthUI missing.", this);

        // Initialize slider
        healthSlider.maxValue = p_Stats.maxHP;
        healthSlider.value = p_Stats.currentHP;
    }

    void OnEnable()
    {
        // Subscribe to health and stat change events
        p_Health.OnDamaged += HandleHealthChanged;
        p_Health.OnHealed += HandleHealthChanged;
        p_Health.OnDied += UpdateUI;

        p_StatsChanged.OnStatsRecalculated += UpdateUI;

        UpdateUI();
    }

    void OnDisable()
    {
        p_Health.OnDamaged -= HandleHealthChanged;
        p_Health.OnHealed -= HandleHealthChanged;
        p_Health.OnDied -= UpdateUI;

        p_StatsChanged.OnStatsRecalculated -= UpdateUI;

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
        healthSlider.value = p_Stats.currentHP;

        healthText.text = $"{p_Stats.currentHP} / {p_Stats.maxHP}";
    }
}
