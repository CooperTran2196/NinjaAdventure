using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class ST_Manager : MonoBehaviour
{
    [Header("Refs")]
    public ST_Slot[] skillSlots;
    public TMP_Text pointsText;
    public P_Exp p_Exp;
    public P_StatsChanged p_StatsChanged;

    [Header("UI Toggle")]
    public CanvasGroup panel;     // assign your SkillTree panel CanvasGroup
    public bool startClosed = true;

    private P_InputActions input;
    private bool isOpen;

    void Awake()
    {
        p_Exp ??= FindFirstObjectByType<P_Exp>();
        p_StatsChanged ??= FindFirstObjectByType<P_StatsChanged>();
        panel ??= GetComponent<CanvasGroup>();
        pointsText ??= GetComponentInChildren<TMP_Text>();

        if (!p_Exp) Debug.LogError($"{name}: P_Exp not found for ST_Manager.", this);
        if (!p_StatsChanged) Debug.LogError($"{name}: P_StatsChanged not found for ST_Manager.", this);
        if (!panel) Debug.LogError($"{name}: CanvasGroup (panel) missing.", this);
        if (!pointsText) Debug.LogWarning($"{name}: pointsText missing.", this);

        input = new P_InputActions();
        input.UI.ToggleSkillTree.Enable(); // K per your action map

        SetOpen(!startClosed);
    }

    void OnEnable()
    {
        p_Exp.OnSkillPointsChanged += HandleSPChanged;
        p_Exp.OnLevelUp += HandleLevelUp;

        ST_Slot.OnSkillUpgraded += HandleSkillUpgraded;
        ST_Slot.OnSkillMaxed += HandleSkillMaxed;
    }

    void OnDisable()
    {
        p_Exp.OnSkillPointsChanged -= HandleSPChanged;
        p_Exp.OnLevelUp -= HandleLevelUp;

        ST_Slot.OnSkillUpgraded -= HandleSkillUpgraded;
        ST_Slot.OnSkillMaxed -= HandleSkillMaxed;
    }

    void Start()
    {
        foreach (var slot in skillSlots)
        {
            var s = slot;
            s.skillButton.onClick.AddListener(() => TryToUpgrade(s));
        }

        HandleSPChanged(p_Exp ? p_Exp.skillPoints : 0);
    }

    void Update()
    {
        if (input.UI.ToggleSkillTree.WasPressedThisFrame())
            SetOpen(!isOpen);
    }

    void SetOpen(bool open)
    {
        isOpen = open;

        if (panel)
        {
            panel.alpha = open ? 1f : 0f;
            panel.interactable = open;
            panel.blocksRaycasts = open;
        }

        // Matches your Stats UI behavior
        Time.timeScale = open ? 0f : 1f;
    }

    void TryToUpgrade(ST_Slot slot)
    {
        if (!slot.isUnlocked) return;
        if (slot.currentLevel >= slot.skillSO.maxLevel) return;

        if (p_Exp.TrySpendSkillPoints(1))
            slot.UpgradeTheSkill();
    }

    void HandleSkillUpgraded(ST_Slot slot)
    {
        var so = slot.skillSO;
        if (!so) return;

        switch (so.kind)
        {
            case ST_SkillSO.Kind.Stat:
                p_StatsChanged.ApplyStat(so.stat, so.pointPerLv);
                break;

            case ST_SkillSO.Kind.Lifesteal:
                {
                    // Total lifesteal scales with current level
                    float totalPercent = slot.currentLevel * so.lifestealPercentPerLevel;
                    p_StatsChanged.SetLifestealPercent(totalPercent);
                    break;
                }
        }
    }

    void HandleSkillMaxed(ST_Slot _)
    {
        // Unlock children that now satisfy prerequisites
        for (int i = 0; i < skillSlots.Length; i++)
        {
            var s = skillSlots[i];
            if (!s.isUnlocked && s.CanUnlockSkill())
                s.Unlock();
        }
    }

    void HandleSPChanged(int sp)
    {
        pointsText.text = "SKILL POINTS: " + sp;
    }

    void HandleLevelUp(int newLevel)
    {
        HandleSPChanged(p_Exp.skillPoints);
    }
}
