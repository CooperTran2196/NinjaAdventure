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
    public P_StatsManager statsManager;

    private P_InputActions input;

    [Header("UI Toggle")]
    public CanvasGroup skillsCanvas;
    public bool panelToggle = false;

    void Awake()
    {
        // Auto-wire UI components if not assigned in inspector
        p_Exp           ??= FindFirstObjectByType<P_Exp>();
        statsManager    ??= FindFirstObjectByType<P_StatsManager>();
        skillsCanvas    ??= GetComponent<CanvasGroup>();
        skillPointsText ??= GetComponentInChildren<TMP_Text>();

        if (!p_Exp)             Debug.LogError($"{name}: P_Exp is missing in ST_Manager");
        if (!statsManager)      Debug.LogError($"{name}: P_StatsManager is missing in ST_Manager");
        if (!skillsCanvas)      Debug.LogError($"{name}: skillsCanvas is missing in ST_Manager");
        if (!skillPointsText)   Debug.LogError($"{name}: skillPointsText is missing in ST_Manager");

        input = new P_InputActions();
        input.UI.ToggleSkillTree.Enable();

        // start closed
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
        input?.UI.Disable();
        input?.Dispose();

        p_Exp.OnSPChanged           -= HandleSPChanged;
        p_Exp.OnLevelUp             -= HandleLevelUp;

        ST_Slots.OnSkillUpgraded    -= HandleSkillUpgraded;
        ST_Slots.OnSkillMaxed       -= HandleSkillMaxed;
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

    // Open/close the skill tree UI
    void SetOpen(bool open)
    {
        panelToggle = open;

        Time.timeScale = open ? 0f : 1f;
        skillsCanvas.alpha = open ? 1f : 0f;
        skillsCanvas.interactable = open;
        skillsCanvas.blocksRaycasts = open;
    }

    // Called when a skill button is clicked
    void TryToUpgrade(ST_Slots slot)
    {
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
            // Note: The modifier's value is the amount *per level*.
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

// REPLACE body to use SkillRegistry
private ST_SkillSO ResolveSkillById(string id)
{
    return SkillRegistry.Get(id);
}

public SYS_SaveSystem.SkillsSave SavingSkills()
{
    var save = new SYS_SaveSystem.SkillsSave();

    if (st_Slots != null)
    {
        foreach (var slot in st_Slots)
        {
            if (!slot || !slot.st_skillSO) continue;
            if (slot.currentLevel <= 0) continue;

            save.skills.Add(new SYS_SaveSystem.SkillsSave.SkillEntry
            {
                id = slot.st_skillSO.id,        // using name as ID
                level = slot.currentLevel
            });
        }
    }

    return save;
}
public void LoadingSkills(SYS_SaveSystem.SkillsSave save)
{
    if (st_Slots == null) return;

    // reset all
    foreach (var slot in st_Slots)
    {
        if (!slot) continue;
        slot.currentLevel = 0;
        slot.isUnlocked   = false;
        // if you have per-slot UI, consider updating it here (optional)
    }

    if (save == null || save.skills == null) return;

    // apply saved levels
    foreach (var rec in save.skills)
    {
        var so = ResolveSkillById(rec.id);
        if (!so) continue;

        // find the slot that holds this skillSO
        foreach (var slot in st_Slots)
        {
            if (!slot || slot.st_skillSO != so) continue;

            // clamp to skill's max level if needed
            int lvl = Mathf.Clamp(rec.level, 0, so.maxLevel);
            slot.currentLevel = lvl;
            slot.isUnlocked   = lvl > 0;
            break;
        }
    }

    // Any ST_Manager UI refresh can be called here if you need it (optional)
}


}
