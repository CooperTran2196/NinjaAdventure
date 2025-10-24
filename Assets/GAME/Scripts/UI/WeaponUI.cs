using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    public enum WeaponSlot { Melee, Ranged }

    [Header("References")]
    P_Controller player;

    [Header("Weapon Display")]
    public Image weaponImage;

    [Header("Settings")]
    public WeaponSlot weaponSlot = WeaponSlot.Melee;

    void Awake()
    {
        if (!weaponImage) { Debug.LogError($"{name}: weaponImage is missing!", this); return; }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<P_Controller>();
    }

    void OnEnable()
    {
        if (weaponSlot == WeaponSlot.Melee)
        {
            P_Controller.OnMeleeWeaponChanged += UpdateDisplay;
            
            W_SO currentWeapon = player.GetCurrentMeleeWeaponSO();
            if (currentWeapon != null) UpdateDisplay(currentWeapon);
        }
        else
        {
            P_Controller.OnRangedWeaponChanged += UpdateDisplay;
            
            W_SO currentWeapon = player.GetCurrentRangedWeaponSO();
            if (currentWeapon != null) UpdateDisplay(currentWeapon);
        }
    }

    void OnDisable()
    {
        if (weaponSlot == WeaponSlot.Melee) P_Controller.OnMeleeWeaponChanged  -= UpdateDisplay;
        else                                P_Controller.OnRangedWeaponChanged -= UpdateDisplay;
    }

    void UpdateDisplay(W_SO newWeapon)
    {
        if (newWeapon == null) return;

        weaponImage.sprite  = newWeapon.image;
        weaponImage.enabled = true;
    }
}
