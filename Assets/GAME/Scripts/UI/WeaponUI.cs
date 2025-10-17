using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays currently equipped weapon icon (melee or ranged)
/// Event-driven, no Update() polling
/// </summary>
public class WeaponUI : MonoBehaviour
{
    public enum WeaponSlot { Melee, Ranged }

    [Header("Weapon Display")]
    public WeaponSlot weaponSlot = WeaponSlot.Melee;
    public Image weaponImage;

    P_Controller player;

    void Awake()
    {
        player = FindFirstObjectByType<P_Controller>();

        if (!weaponImage)
            Debug.LogError("UI_WeaponDisplay: weaponImage is missing!");
        if (!player)
            Debug.LogError("UI_WeaponDisplay: P_Controller not found!");
    }

    void OnEnable()
    {
        if (weaponSlot == WeaponSlot.Melee)
        {
            P_Controller.OnMeleeWeaponChanged += UpdateDisplay;
            
            if (player != null)
            {
                W_SO currentWeapon = player.GetCurrentMeleeWeaponSO();
                if (currentWeapon != null) UpdateDisplay(currentWeapon);
            }
        }
        else
        {
            P_Controller.OnRangedWeaponChanged += UpdateDisplay;
            
            if (player != null)
            {
                W_SO currentWeapon = player.GetCurrentRangedWeaponSO();
                if (currentWeapon != null) UpdateDisplay(currentWeapon);
            }
        }
    }

    void OnDisable()
    {
        if (weaponSlot == WeaponSlot.Melee)
            P_Controller.OnMeleeWeaponChanged -= UpdateDisplay;
        else
            P_Controller.OnRangedWeaponChanged -= UpdateDisplay;
    }

    void UpdateDisplay(W_SO newWeapon)
    {
        if (newWeapon == null) return;

        weaponImage.sprite = newWeapon.image;
        weaponImage.enabled = true;
    }
}
