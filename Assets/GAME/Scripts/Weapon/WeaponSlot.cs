using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WeaponSlot : MonoBehaviour
{
    [Header("UI")]
    public Image    iconImage;        // drag WeaponIcon
    public TMP_Text hotkeyText;       // drag Hotkey (TMP)

    [Header("Data")]
    public WeaponSO action;         // set by WeaponManager
    public bool     unlocked;         // slot shows when true

    

    static WeaponSO WeaponManagerCurrent() => GameObject.FindWithTag("Player")?.GetComponent<WeaponManager>()?.GetCurrentAction();         // create this getter

    void OnEnable () => WeaponManager.OnWeaponEquipped += HandleEquip;
    void OnDisable() => WeaponManager.OnWeaponEquipped -= HandleEquip;

    void HandleEquip(WeaponSO equipped)
    {
        iconImage.color = (action == equipped) ? Color.white : Color.gray;
    }

    // henever you tweak slot in Inspector,
    // the sprite / label refresh automatically – same pattern SkillSlot uses.
    private void OnValidate()
    {
        if (action != null) UpdateUI();
    }

    public void SetWeapon(WeaponSO a)
    {
        action   = a;
        unlocked = true;
        UpdateUI();
        HandleEquip(WeaponManagerCurrent());   // helper in next step
    }


    void UpdateUI()
    {
        iconImage.sprite = action != null ? action.icon : null;
        hotkeyText.text  = action != null ? action.slotIndex.ToString() : "";
        gameObject.SetActive(unlocked);
    }
}
