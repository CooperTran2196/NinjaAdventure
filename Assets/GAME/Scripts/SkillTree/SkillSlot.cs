using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
public class SkillSlot : MonoBehaviour
{
    public List<SkillSlot> prerequisiteSkillSlots;
    
    public SkillSO skillSO;

    public int currentLevel;
    public bool isUnclocked;

    public Image SkillIcon;
    public Button skillButton;
    public TMP_Text skillLevelText;

    public static event Action<SkillSlot> OnAbilityPointSpent;
    public static event Action<SkillSlot> OnSkillMaxed;

    private void OnValidate()
    {
        if (skillSO != null && skillLevelText != null)
        {

            UpdateUI();
        }
    }


    public void TryUpgradeSkill()
    {
        if (isUnclocked && currentLevel < skillSO.maxLevel)
        {
            currentLevel++;
            OnAbilityPointSpent?.Invoke(this);

            if (currentLevel >= skillSO.maxLevel)
            {
                OnSkillMaxed?.Invoke(this);
            }

            UpdateUI();
        }
    }

    public bool CanUnlockSkill()
    {
        foreach (SkillSlot slot in prerequisiteSkillSlots)
        {
            if (!slot.isUnclocked || slot.currentLevel < slot.skillSO.maxLevel)
            {
                return false;
            }
        }
        return true;
    }

    public void Unlock()
    {
        isUnclocked = true;
        UpdateUI();
    }

    private void UpdateUI()
    {
        SkillIcon.sprite = skillSO.skillIcon;

        if (isUnclocked)
        {
            skillButton.interactable = true;
            skillLevelText.text = currentLevel.ToString() + "/" + skillSO.maxLevel.ToString();
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
