using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    [Header("References")]
    public C_Stats        p_Stats;
    public C_Mana         p_Mana;
    public P_StatsManager p_StatsManager;
    public Slider         manaSlider;
    public TMP_Text       manaText;

    void Awake()
    {
        p_Stats        ??= FindFirstObjectByType<C_Stats>();
        p_Mana         ??= FindFirstObjectByType<C_Mana>();
        p_StatsManager ??= FindFirstObjectByType<P_StatsManager>();

        if (!p_Stats)        Debug.LogError("ManaUI: C_Stats is missing.");
        if (!p_Mana)         Debug.LogError("ManaUI: C_Mana is missing.");
        if (!p_StatsManager) Debug.LogError("ManaUI: P_StatsManager is missing.");
        if (!manaSlider)     Debug.LogError("ManaUI: manaSlider is missing.");
        if (!manaText)       Debug.LogError("ManaUI: manaText is missing.");

        manaSlider.maxValue = p_Stats.maxMP;
        manaSlider.value    = p_Stats.currentMP;
    }

    void OnEnable()
    {

        p_Mana.OnManaChanged += HandleManaChanged;
        p_Mana.OnManaEmpty   += UpdateUI;
        p_StatsManager.OnStatsChanged += UpdateUI;

        UpdateUI();
    }

    void OnDisable()
    {
        p_Mana.OnManaChanged -= HandleManaChanged;
        p_Mana.OnManaEmpty   -= UpdateUI;
        p_StatsManager.OnStatsChanged -= UpdateUI;

    }

    // A helper method to match the signature of OnManaChanged event
    void HandleManaChanged(int amount)
    {
        UpdateUI();
    }

    // Update the mana UI elements
    public void UpdateUI()
    {
        // Update the slider's max value and current value
        manaSlider.maxValue = p_Stats.maxMP;
        manaSlider.value    = p_Stats.currentMP;

        manaText.text = $"{p_Stats.currentMP} / {p_Stats.maxMP}";
    }
}
