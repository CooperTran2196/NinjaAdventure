using UnityEngine;

public class SkillManager : MonoBehaviour
{
    //public Player_Combat combat;
    public WeaponSO Katana, Bow1, Stick, Sword1;
    WeaponManager weaponManager;

    void Awake()
    {
        weaponManager = GetComponent<WeaponManager>();
    }

    private void OnEnable()
    {
        SkillSlot.OnAbilityPointSpent += HandleAbilityPointSpent;
    }

    private void OnDisable()
    {
        SkillSlot.OnAbilityPointSpent -= HandleAbilityPointSpent;
    }

    private void HandleAbilityPointSpent(SkillSlot slot)
    {
        string skillName = slot.skillSO.skillName;

        switch (skillName)
        {
            case "Max Health":
                StatsManager.Instance.UpdateMaxHealth(1);
                break;

            case "Max Attack":
                    StatsManager.Instance.baseDamage += 1;               // +1 per level
                break;

            case "Unlock Katana":
                weaponManager.AddWeapon(Katana);
                break;

            case "Unlock Bow1":
                weaponManager.AddWeapon(Bow1);
                break;

            case "Unlock Stick":
                weaponManager.AddWeapon(Stick);
                break;

            case "Unlock Sword1":
                weaponManager.AddWeapon(Sword1);
                break;

            default:
                Debug.LogWarning("Unknow skill: " + skillName);
                break;
        }
    }
}
