using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ST_Slots : MonoBehaviour
{
    [Header("Slot represents one skill in the Skill Tree")]
    [Header("Gate")]
    public List<ST_Slots>   prerequisiteSkillSlots;

    [Header("Skill Data")]
    public ST_SkillSO       st_skillSO;

    [Header("State")]
    public int              currentLevel;
    public bool             isUnlocked;

    [Header("UI")]
    public Image            SkillIcon;
    public Button           skillButton;
    public TMP_Text         skillLevelText;

    public static event Action<ST_Slots> OnSkillUpgraded;
    public static event Action<ST_Slots> OnSkillMaxed;

    void Awake()
    {
        SkillIcon            ??= GetComponentInChildren<Image>();
        skillButton          ??= GetComponent<Button>();
        skillLevelText       ??= GetComponentInChildren<TMP_Text>();

        if (!st_skillSO)     Debug.LogError("ST_Slots: ST_SkillSO is not assigned", this);
        if (!SkillIcon)      Debug.LogWarning("ST_Slots: SkillIcon is not assigned.", this);
        if (!skillButton)    Debug.LogWarning("ST_Slots: skillButton is not assigned.", this);
        if (!skillLevelText) Debug.LogWarning("ST_Slots: skillLevelText is not assigned.", this);

        prerequisiteSkillSlots ??= new List<ST_Slots>();

        UpdateUI();
    }

    void OnValidate()
    {
        if (st_skillSO != null && skillLevelText != null) UpdateUI();
    }

    // Called by ST_Manager after it successfully spends a point
    public void UpgradeTheSkill()
    {
        // ST_Manager already check these first
        // Avoid cheat/debug buttons go over limits
        if (!isUnlocked) return;
        if (currentLevel >= st_skillSO.maxLevel) return;

        // Upgrade and fire events
        currentLevel++;
        OnSkillUpgraded?.Invoke(this);
        if (currentLevel >= st_skillSO.maxLevel) OnSkillMaxed?.Invoke(this);

        UpdateUI();
    }

    // Check all prerequisites and unlock the skill if they are met
    public bool TryUnlock()
    {
        // 1/ Don't do anything if the skill is already unlocked
        if (isUnlocked)
        {
            return false;
        }

        // 2/ Check if all prerequisite skills are maxed out
        foreach (var prereq in prerequisiteSkillSlots)
        {
            if (prereq.currentLevel < prereq.st_skillSO.maxLevel)
            {
                return false;
            }
        }

        // 3/ All prerequisites are met! Unlock the skill.
        isUnlocked = true;
        UpdateUI();
        return true;
    }

    // Update the UI elements based on the current state
    void UpdateUI()
    {
        // Update icon and level text
        SkillIcon.sprite = st_skillSO.skillIcon;

        // Gray out if locked, show level if unlocked
        if (isUnlocked)
        {
            skillButton.interactable = true;
            skillLevelText.text      = currentLevel + "/" + st_skillSO.maxLevel;
            SkillIcon.color          = Color.white;
        }
        else
        {
            skillButton.interactable = false;
            skillLevelText.text      = "Locked";
            SkillIcon.color          = Color.gray;
        }
    }
}
