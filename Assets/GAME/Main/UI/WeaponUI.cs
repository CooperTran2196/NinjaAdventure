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
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<P_Controller>();
        
        if (!weaponImage) { Debug.LogError($"{name}: weaponImage is missing!", this); return; }
        if (!player)      { Debug.LogError($"{name}: Player (P_Controller) not found!", this); return; }
    }

    void OnEnable()
    {
        // Safety check in case player isn't ready yet
        if (!player) 
        {
            player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<P_Controller>();
            if (!player) return; // Player not in scene yet, skip initialization
        }

        if (weaponSlot == WeaponSlot.Melee)
        {
            P_Controller.OnMeleeWeaponChanged += UpdateDisplay;
            
            W_SO currentWeapon = player.GetMeleeWeapon();
            if (currentWeapon != null) UpdateDisplay(currentWeapon);
        }
        else
        {
            P_Controller.OnRangedWeaponChanged += UpdateDisplay;
            
            W_SO currentWeapon = player.GetRangedWeapon();
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
