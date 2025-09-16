using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ST_Slot : MonoBehaviour
{
    [Header("Gate")]
    public List<ST_Slot> prerequisiteSkillSlots;

    [Header("Data")]
    public ST_SkillSO skillSO;

    [Header("State")]
    public int  currentLevel;
    public bool isUnlocked;

    [Header("UI")]
    public Image   SkillIcon;
    public Button  skillButton;
    public TMP_Text skillLevelText;

    public static event Action<ST_Slot> OnSkillUpgraded;
    public static event Action<ST_Slot> OnSkillMaxed;

    void Awake()
    {
        // Auto-wire UI components if not assigned in inspector
        SkillIcon ??= GetComponentInChildren<Image>();
        skillButton ??= GetComponent<Button>();
        skillLevelText ??= GetComponentInChildren<TMP_Text>();

        if (!skillSO) Debug.LogError($"{name}: ST_SkillSO is not assigned on {GetType().Name}.", this);
        if (!SkillIcon) Debug.LogWarning($"{name}: SkillIcon is not assigned.", this);
        if (!skillButton) Debug.LogWarning($"{name}: skillButton is not assigned.", this);
        if (!skillLevelText) Debug.LogWarning($"{name}: skillLevelText is not assigned.", this);

        prerequisiteSkillSlots ??= new List<ST_Slot>();

        UpdateUI();
    }

    void OnValidate()
    {
        if (skillSO != null && skillLevelText != null) UpdateUI();
    }

    // Called by SkillTreeManager after it successfully spends a point
    public void UpgradeTheSkill()
    {
        // Only for safety, SkillTreeManager should check these first
        // Will use for cheat/debug buttons
        if (!isUnlocked) return;
        if (currentLevel >= skillSO.maxLevel) return;

        currentLevel++;
        OnSkillUpgraded?.Invoke(this);
        if (currentLevel >= skillSO.maxLevel) OnSkillMaxed?.Invoke(this);

        UpdateUI();
    }

    public bool CanUnlockSkill()
    {
        foreach (var slot in prerequisiteSkillSlots)
        {
            if (!slot.isUnlocked || slot.currentLevel < slot.skillSO.maxLevel)
                return false;
        }
        return true;
    }

    public void Unlock()
    {
        isUnlocked = true;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (SkillIcon && skillSO) SkillIcon.sprite = skillSO.skillIcon;

        if (isUnlocked)
        {
            skillButton.interactable = true;
            skillLevelText.text = currentLevel + "/" + skillSO.maxLevel;
            SkillIcon.color = Color.white;
        }
        else
        {
            skillButton.interactable = false;
            skillLevelText.text = "Locked";
            SkillIcon.color = Color.gray;
        }
    }
}
