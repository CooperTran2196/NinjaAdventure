using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class INV_ItemInfo : MonoBehaviour
{
    [Header("References")]
    CanvasGroup   canvasGroup;
    RectTransform rectTransform;

    [Header("MUST wire MANUALLY in Inspector")]
    public TMP_Text      itemNameText;
    public TMP_Text      itemDescText;
    public TMP_Text      statLinePrefab;
    public Transform     statContainer;

    [Header("Offset")]
    public Vector2 offset = new Vector2(12f, -8f);

    void Awake()
    {
        canvasGroup   ??= GetComponent<CanvasGroup>();
        rectTransform ??= GetComponentInChildren<RectTransform>();

        if (!canvasGroup)    { Debug.LogError($"{name}: CanvasGroup is missing!", this); return; }
        if (!rectTransform)  { Debug.LogError($"{name}: RectTransform is missing!", this); return; }
        if (!itemNameText)   { Debug.LogError($"{name}: itemNameText is missing!", this); return; }
        if (!itemDescText)   { Debug.LogError($"{name}: itemDescText is missing!", this); return; }
        if (!statLinePrefab) { Debug.LogError($"{name}: statLinePrefab is missing!", this); return; }
        if (!statContainer)  { Debug.LogError($"{name}: statContainer is missing!", this); return; }
    }

    public void Show(INV_ItemSO itemSO)
    {
        canvasGroup.alpha = 1f;

        if (itemNameText) itemNameText.text = itemSO ? itemSO.itemName : string.Empty;
        itemDescText.text = itemSO.description;

        ClearStatLines();
        if (itemSO)
        {
            List<string> statLines = BuildItemStatLines(itemSO);
            foreach (string line in statLines)
            {
                TMP_Text textLine = Instantiate(statLinePrefab, statContainer);
                textLine.text = line;
            }
        }
    }

    public void Show(W_SO weaponSO)
    {
        canvasGroup.alpha = 1f;

        if (itemNameText) itemNameText.text = weaponSO ? weaponSO.id : string.Empty;
        itemDescText.text = weaponSO ? weaponSO.description : string.Empty;

        ClearStatLines();
        if (weaponSO)
        {
            List<string> statLines = BuildWeaponStatLines(weaponSO);
            foreach (string line in statLines)
            {
                TMP_Text textLine = Instantiate(statLinePrefab, statContainer);
                textLine.text = line;
            }
        }
    }

    public void Hide()
    {
        if (!canvasGroup) return;
        canvasGroup.alpha = 0f;
    }

    public void FollowMouse(Vector2 screenPos)
    {
        rectTransform.position = (Vector3)screenPos + (Vector3)offset;
    }

    void ClearStatLines()
    {
        for (int i = statContainer.childCount - 1; i >= 0; i--)
            Destroy(statContainer.GetChild(i).gameObject);
    }

    List<string> BuildItemStatLines(INV_ItemSO inv_ItemSO)
    {
        List<string> statLines = new List<string>();

        if (inv_ItemSO.isGold)
        {
            statLines.Add("A pile of gold coins.");
            return statLines;
        }

        if (inv_ItemSO.StatEffectList != null)
        {
            foreach (P_StatEffect effect in inv_ItemSO.StatEffectList)
            {
                string line;
                switch (effect.statName)
                {
                    case StatName.Heal:          line = $"Heals {effect.Value} HP"; break;
                    case StatName.Mana:          line = $"Restores {effect.Value} Mana"; break;
                    case StatName.MaxHealth:     line = $"+{effect.Value} Max HP"; break;
                    case StatName.MaxMana:       line = $"+{effect.Value} Max Mana"; break;
                    case StatName.AttackDamage:  line = $"+{effect.Value} Attack Damage"; break;
                    case StatName.AbilityPower:  line = $"+{effect.Value} Ability Power"; break;
                    case StatName.MoveSpeed:     line = $"+{effect.Value} Move Speed"; break;
                    case StatName.Armor:         line = $"+{effect.Value} Armor"; break;
                    case StatName.MagicResist:   line = $"+{effect.Value} Magic Resist"; break;
                    case StatName.Lifesteal:     line = $"+{effect.Value}% Lifesteal"; break;
                    default:                     line = $"+{effect.Value} {effect.statName}"; break;
                }
                
                // Add duration if effect is temporary
                if (effect.Duration > 1) line = $"{line} in ({effect.Duration}s)";
                
                statLines.Add(line);
            }
        }
        return statLines;
    }

    List<string> BuildWeaponStatLines(W_SO weaponSO)
    {
        List<string> statLines = new List<string>();

        if (weaponSO.type == WeaponType.Melee)
        {
            statLines.Add($"AD: {weaponSO.AD}   AP: {weaponSO.AP}");
            statLines.Add($"Knockback: {weaponSO.knockbackForce}");
            statLines.Add($"Thrust    : {weaponSO.thrustDistance}");
            statLines.Add($"Slash      : {weaponSO.slashArcDegrees} Degrees");
            statLines.Add($"Speed      : {string.Join(" - ", weaponSO.comboShowTimes)}");
            statLines.Add($"Penalties: {string.Join(" - ", weaponSO.comboMovePenalties)}");
            statLines.Add($"Stun Time: {string.Join(" - ", weaponSO.comboStunTimes)}");
        }
        else if (weaponSO.type == WeaponType.Ranged)
        {
            statLines.Add($"AD: {weaponSO.AD}  AP: {weaponSO.AP}");
            statLines.Add($"Mana Cost: {weaponSO.manaCost}");
            statLines.Add($"Projectile Speed: {weaponSO.projectileSpeed}");
        }

        return statLines;
    }
}
