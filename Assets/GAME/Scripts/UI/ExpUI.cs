using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpUI : MonoBehaviour
{
    [Header("References")]
    public P_Exp p_Exp;

    [Header("UI")]
    public Slider expSlider;
    public TMP_Text currentLevelText;

    P_InputActions input;

    void Awake()
    {
        input = new P_InputActions();
        p_Exp ??= FindFirstObjectByType<P_Exp>();
        expSlider ??= GetComponentInChildren<Slider>();
        currentLevelText ??= GetComponentInChildren<TMP_Text>();

        if (!p_Exp) Debug.LogError($"{name}: P_Exp is missing in ExpUI");
        if (!expSlider) Debug.LogError($"{name}: expSlider is missing in ExpUI");
        if (!currentLevelText) Debug.LogError($"{name}: currentLevelText is missing in ExpUI");
    }

    void OnEnable()
    {
        input.Debug.Enable();

        p_Exp.OnLevelUp += HandleLevelUp;
        p_Exp.OnXPChanged += HandleXPChanged;

        UpdateUI();
    }

    void OnDisable()
    {
        input.Debug.Disable();

        p_Exp.OnLevelUp -= HandleLevelUp;
        p_Exp.OnXPChanged -= HandleXPChanged;
    }

    // Debug: Gain XP with input
    void Update()
    {
        if (input.Debug.GainExp.WasPressedThisFrame())
            p_Exp.AddDebugXP();
    }

    void HandleLevelUp(int newLevel) => UpdateUI();
    void HandleXPChanged(int cur, int req) => UpdateUI();

    // Update the XP bar and level text
    void UpdateUI()
    {
        int cur = p_Exp.currentXP;
        int req = p_Exp.GetXPRequiredForNext();
        expSlider.maxValue = req;
        expSlider.value = cur;
        currentLevelText.text = "Level: " + p_Exp.level;
    }
}
