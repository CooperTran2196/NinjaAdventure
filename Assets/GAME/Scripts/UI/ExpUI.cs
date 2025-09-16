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

        if (!p_Exp) Debug.LogError($"{name}: P_Exp in ExpUI missing.", this);
        if (!expSlider) Debug.LogError($"{name}: expSlider in ExpUI missing.", this);
        if (!currentLevelText) Debug.LogError($"{name}: currentLevelText in ExpUI missing.", this);
    }

    void OnEnable()
    {
        input.Debug.Enable();
        if (p_Exp != null)
        {
            p_Exp.OnLevelUp += HandleLevelUp;
            p_Exp.OnXPChanged += HandleXPChanged;
        }
        UpdateUI();
    }

    void OnDisable()
    {
        input.Debug.Disable();
        if (p_Exp != null)
        {
            p_Exp.OnLevelUp -= HandleLevelUp;
            p_Exp.OnXPChanged -= HandleXPChanged;
        }
    }

    void Update()
    {
        if (input.Debug.GainExp.WasPressedThisFrame())
            p_Exp?.AddDebugXP();
    }

    void HandleLevelUp(int newLevel) => UpdateUI();
    void HandleXPChanged(int cur, int req) => UpdateUI();

    void UpdateUI()
    {
        if (p_Exp == null) return;
        int cur = p_Exp.currentXP;
        int req = p_Exp.GetXPRequiredForNext();
        expSlider.maxValue = req;
        expSlider.value = cur;
        currentLevelText.text = "Level: " + p_Exp.level;
    }
}
