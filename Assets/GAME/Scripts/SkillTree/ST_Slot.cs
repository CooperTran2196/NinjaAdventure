using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ST_Slots : MonoBehaviour
{
    [Header("Gate")]
    public List<ST_Slots> prerequisiteSkillSlots;

    [Header("Data")]
    public ST_SkillSO st_skillSO;

    [Header("State")]
    public int  currentLevel;
    public bool isUnlocked;

    [Header("UI")]
    public Image   SkillIcon;
    public Button  skillButton;
    public TMP_Text skillLevelText;

    public static event Action<ST_Slots> OnSkillUpgraded;
    public static event Action<ST_Slots> OnSkillMaxed;

    void Awake()
    {
        // Auto-wire UI components if not assigned in inspector
        SkillIcon ??= GetComponentInChildren<Image>();
        skillButton ??= GetComponent<Button>();
        skillLevelText ??= GetComponentInChildren<TMP_Text>();

        if (!st_skillSO) Debug.LogError($"{name}: ST_SkillSO is not assigned on {GetType().Name}.", this);
        if (!SkillIcon) Debug.LogWarning($"{name}: SkillIcon is not assigned.", this);
        if (!skillButton) Debug.LogWarning($"{name}: skillButton is not assigned.", this);
        if (!skillLevelText) Debug.LogWarning($"{name}: skillLevelText is not assigned.", this);

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

    public bool CanUnlockSkill()
    {
        foreach (var slot in prerequisiteSkillSlots)
        {
            if (!slot.isUnlocked || slot.currentLevel < slot.st_skillSO.maxLevel)
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
        if (SkillIcon && st_skillSO) SkillIcon.sprite = st_skillSO.skillIcon;

        if (isUnlocked)
        {
            skillButton.interactable = true;
            skillLevelText.text = currentLevel + "/" + st_skillSO.maxLevel;
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
