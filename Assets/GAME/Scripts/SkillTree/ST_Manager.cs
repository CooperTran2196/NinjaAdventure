using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class ST_Manager : MonoBehaviour
{
    [Header("Central API for the Skill Tree system")]
    [Header("References")]
    public ST_Slots[] st_Slots;
    public TMP_Text skillPointsText;
    public P_Exp p_Exp;
    public P_StatsChanged p_StatsChanged;

    private P_InputActions input;

    [Header("UI Toggle")]
    public CanvasGroup SkillsCanvas;
    public bool panelToggle = false;

    void Awake()
    {
        // Auto-wire UI components if not assigned in inspector
        p_Exp           ??= FindFirstObjectByType<P_Exp>();
        p_StatsChanged  ??= FindFirstObjectByType<P_StatsChanged>();
        SkillsCanvas    ??= GetComponent<CanvasGroup>();
        skillPointsText ??= GetComponentInChildren<TMP_Text>();

        if (!p_Exp)             Debug.LogError($"{name}: P_Exp missing in ST_Manager.", this);
        if (!p_StatsChanged)    Debug.LogError($"{name}: P_StatsChanged missing in ST_Manager.", this);
        if (!SkillsCanvas)      Debug.LogError($"{name}: SkillsCanvas missing in ST_Manager.", this);
        if (!skillPointsText)   Debug.LogError($"{name}: skillPointsText missing in ST_Manager.", this);

        input = new P_InputActions();
        input.UI.ToggleSkillTree.Enable();
        SetOpen(panelToggle);
    }

    void OnEnable()
    {
        p_Exp.OnSPChanged           += HandleSPChanged;
        p_Exp.OnLevelUp             += HandleLevelUp;

        ST_Slots.OnSkillUpgraded    += HandleSkillUpgraded;
        ST_Slots.OnSkillMaxed       += HandleSkillMaxed;
    }

    void OnDisable()
    {
        p_Exp.OnSPChanged           -= HandleSPChanged;
        p_Exp.OnLevelUp             -= HandleLevelUp;

        ST_Slots.OnSkillUpgraded    -= HandleSkillUpgraded;
        ST_Slots.OnSkillMaxed       -= HandleSkillMaxed;
    }

    void Start()
    {
        foreach (var slot in st_Slots)
        {
            slot.skillButton.onClick.AddListener(() => TryToUpgrade(slot));
        }

        // Initial UI state
        HandleSPChanged(p_Exp.skillPoints);
    }

    void Update()
    {
        if (input.UI.ToggleSkillTree.WasPressedThisFrame())
            SetOpen(!panelToggle);
    }

    //
    void SetOpen(bool open)
    {
        panelToggle = open;

        Time.timeScale = open ? 0f : 1f;
        SkillsCanvas.alpha = open ? 1f : 0f;
        SkillsCanvas.interactable = open;
        SkillsCanvas.blocksRaycasts = open;
    }

    void TryToUpgrade(ST_Slots slot)
    {
        // ST_Manager should check these first
        if (!slot.isUnlocked) return;
        if (slot.currentLevel >= slot.st_skillSO.maxLevel) return;

        if (p_Exp.TrySpendSkillPoints(1))
            slot.UpgradeTheSkill();
    }

    void HandleSkillUpgraded(ST_Slots slot)
    {
        var so = slot.st_skillSO;
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

    void HandleSkillMaxed(ST_Slots maxedSlot)
    {
        // When a skill maxed -> Check only its direct children to see if they can be unlocked
        foreach (var ChildSlot in st_Slots)
        {
            // To be a child, it must not be unlocked and must have the maxed slot as a prerequisite
            if (!ChildSlot.isUnlocked && ChildSlot.prerequisiteSkillSlots.Contains(maxedSlot))
            {
                // check if ALL its prerequisites are met
                if (ChildSlot.CanUnlockSkill())
                {
                    ChildSlot.Unlock();
                }
            }
        }
    }

    void HandleSPChanged(int sp)
    {
        skillPointsText.text = "SKILL POINTS: " + sp;
    }

    void HandleLevelUp(int newLevel)
    {
        HandleSPChanged(p_Exp.skillPoints);
    }
}
