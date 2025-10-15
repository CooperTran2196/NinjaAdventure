using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpUI : MonoBehaviour
{
    [Header("References")]
    public P_Exp p_Exp;

    [Header("UI")]
    public Slider   expSlider;
    public TMP_Text currentLevelText;

    P_InputActions input;

    void Awake()
    {
        input = new P_InputActions();

        expSlider        ??= GetComponentInChildren<Slider>();
        currentLevelText ??= GetComponentInChildren<TMP_Text>();

        if (!p_Exp)            Debug.LogError("ExpUI: P_Exp is missing.");
        if (!expSlider)        Debug.LogError("ExpUI: expSlider is missing.");
        if (!currentLevelText) Debug.LogError("ExpUI: currentLevelText is missing.");
    }

    void OnEnable()
    {
        input.Debug.Enable();

        if (p_Exp != null)
        {
            p_Exp.OnLevelUp   += HandleLevelUp;
            p_Exp.OnXPChanged += HandleXPChanged;
            UpdateUI();
        }
    }

    void OnDisable()
    {
        input.Debug.Disable();

        if (p_Exp != null)
        {
            p_Exp.OnLevelUp   -= HandleLevelUp;
            p_Exp.OnXPChanged -= HandleXPChanged;
        }
    }

    // Debug: Gain XP with input
    void Update()
    {
        if (input.Debug.GainExp.WasPressedThisFrame())
            p_Exp.AddDebugXP();
    }

    void HandleLevelUp(int newLevel) => UpdateUI();
    void HandleXPChanged(int cur, int req) => UpdateUI();

    void UpdateUI()
    {
        int cur = p_Exp.currentExp;
        int req = p_Exp.GetXPRequiredForNext();
        expSlider.maxValue = req;
        expSlider.value = cur;
        currentLevelText.text = "Level: " + p_Exp.level;
    }
}
