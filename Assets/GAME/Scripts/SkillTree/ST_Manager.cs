using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ST_Manager : MonoBehaviour
{
    [Header("Central API for the Skill Tree system")]
    [Header("References")]
    public ST_Slots[]     st_Slots;
    public TMP_Text       skillPointsText;
    public P_Exp          p_Exp;
    public P_StatsManager statsManager;

    [Header("UI Toggle")]
    public CanvasGroup skillsCanvas;
    public bool panelToggle = false;
    
    private P_InputActions input;

    void Awake()
    {
        p_Exp                 ??= FindFirstObjectByType<P_Exp>();
        statsManager          ??= FindFirstObjectByType<P_StatsManager>();
        skillsCanvas          ??= GetComponent<CanvasGroup>();
        skillPointsText       ??= GetComponentInChildren<TMP_Text>();

        if (!p_Exp)           Debug.LogError("ST_Manager: P_Exp is missing.");
        if (!statsManager)    Debug.LogError("ST_Manager: P_StatsManager is missing.");
        if (!skillsCanvas)    Debug.LogError("ST_Manager: skillsCanvas is missing.");
        if (!skillPointsText) Debug.LogError("ST_Manager: skillPointsText is missing.");

        input = new P_InputActions();
        input.UI.ToggleSkillTree.Enable();

        // start closed
        SetOpen(panelToggle);
    }

    void OnEnable()
    {
        p_Exp.OnSPChanged        += HandleSPChanged;
        p_Exp.OnLevelUp          += HandleLevelUp;

        ST_Slots.OnSkillUpgraded += HandleSkillUpgraded;
        ST_Slots.OnSkillMaxed    += HandleSkillMaxed;
    }

    void OnDisable()
    {
        input.UI.Disable();

        p_Exp.OnSPChanged        -= HandleSPChanged;
        p_Exp.OnLevelUp          -= HandleLevelUp;

        ST_Slots.OnSkillUpgraded -= HandleSkillUpgraded;
        ST_Slots.OnSkillMaxed    -= HandleSkillMaxed;
    }

    void OnDestroy()
    {
        input?.Dispose();
    }
    
    // Initialize skill buttons and UI state
    void Start()
    {
        foreach (var slot in st_Slots)
        {
            slot.skillButton.onClick.AddListener(() => TryToUpgrade(slot));
        }

        // Initial UI state
        HandleSPChanged(p_Exp.skillPoints);
    }

    // Toggle skill tree UI with input
    void Update()
    {
        if (input.UI.ToggleSkillTree.WasPressedThisFrame())
            SetOpen(!panelToggle);
    }

    // Public method for UI button onClick events
    public void OnClickToggle()
    {
        SetOpen(!panelToggle);
    }

    // Open/close the skill tree UI
    void SetOpen(bool open)
    {
        panelToggle = open;

        // Play appropriate sound
        if (open)
            SYS_GameManager.Instance.sys_SoundManager.PlayOpenSkillTree();
        else
            SYS_GameManager.Instance.sys_SoundManager.PlayClosePanel();

        Time.timeScale              = open ? 0f : 1f;
        skillsCanvas.alpha          = open ? 1f : 0f;
        skillsCanvas.interactable   = open;
        skillsCanvas.blocksRaycasts = open;
    }

    // Called when a skill button is clicked
    void TryToUpgrade(ST_Slots slot)
    {
        // Play button click sound
        SYS_GameManager.Instance.sys_SoundManager.PlayButtonClick();
        
        // ST_Manager should check these first
        if (!slot.isUnlocked) return;
        if (slot.currentLevel >= slot.st_skillSO.maxLevel) return;

        if (p_Exp.TrySpendSkillPoints(1))
            slot.UpgradeTheSkill();
    }

    // Called when a skill is upgraded
    void HandleSkillUpgraded(ST_Slots slot)
    {
        var so = slot.st_skillSO;
        if (!so) return;

        // Apply all modifiers from the skill upgrade
        foreach (P_StatEffect modifier in so.StatEffectList)
        {
            // Note: The modifier's value is the amount *per level*
            statsManager.ApplyModifier(modifier);
        }
    }

    // Called when a skill is maxed out
    void HandleSkillMaxed(ST_Slots maxedSlot)
    {
        // When a skill maxed -> Check only its direct children to see if they can be unlocked
        foreach (var slot in st_Slots)
        {
            // Check if this slot is a child of the maxed slot
            if (!slot.isUnlocked && slot.prerequisiteSkillSlots.Contains(maxedSlot))
            {
                // Try to unlock it
                slot.TryUnlock();
            }
        }
    }

    // Called when skill points changed
    void HandleSPChanged(int sp)
    {
        skillPointsText.text = "SKILL POINTS: " + sp;
    }

    // Called when player levels up
    void HandleLevelUp(int newLevel)
    {
        HandleSPChanged(p_Exp.skillPoints);
    }
}
