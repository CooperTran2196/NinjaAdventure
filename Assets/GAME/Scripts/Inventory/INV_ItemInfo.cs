using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class INV_ItemInfo : MonoBehaviour
{
    [Header("Showing Info of Items")]
    [Header("References")]
    public CanvasGroup   canvasGroup;
    public RectTransform rectTransform;
    public TMP_Text      itemNameText;
    public TMP_Text      itemDescText;
    public TMP_Text      statLinePrefab;
    public Transform     statContainer;

    public Vector2 offset = new Vector2(12f, -8f);

    void Awake()
    {
        canvasGroup          ??= GetComponent<CanvasGroup>();
        rectTransform        ??= GetComponent<RectTransform>();

        if (!canvasGroup)    Debug.LogError("INV_ItemInfo: CanvasGroup is missing.");
        if (!rectTransform)  Debug.LogError("INV_ItemInfo: RectTransform is missing.");
        if (!itemDescText)   Debug.LogError("INV_ItemInfo: itemDescText is missing.");
        if (!statLinePrefab) Debug.LogError("INV_ItemInfo: statLinePrefab is missing.");
        if (!statContainer)  Debug.LogError("INV_ItemInfo: statContainer is missing.");
    }

    public void Show(INV_ItemSO itemSO)
    {
        canvasGroup.alpha = 1f;

        if (itemNameText) itemNameText.text = itemSO ? itemSO.itemName : string.Empty;
        itemDescText.text = itemSO.description;

        ClearStatLines();
        if (itemSO)
        {
            var outLines = BuildItemStatLines(itemSO);
            foreach (var line in outLines)
            {
                var textLine = Instantiate(statLinePrefab, statContainer);
                textLine.text = line;
            }
        }
    }

    // Hide and clear
    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }

    public void FollowMouse(Vector2 screenPos)
    {
        rectTransform.position = (Vector3)screenPos + (Vector3)offset;
    }

    // Remove all existing stat lines
    void ClearStatLines()
    {
        for (int i = statContainer.childCount - 1; i >= 0; i--)
            Destroy(statContainer.GetChild(i).gameObject);
    }

    // Build formatted stat description lines for the provided item
    List<string> BuildItemStatLines(INV_ItemSO inv_ItemSO)
    {
        var outLines = new List<string>();

        if (inv_ItemSO.isGold)
        {
            outLines.Add("A pile of gold coins.");
            return outLines;
        }

        if (inv_ItemSO.StatEffectList != null)
        {
            for (int i = 0; i < inv_ItemSO.StatEffectList.Count; i++)
            {
                var effects = inv_ItemSO.StatEffectList[i];
                string line;
                switch (effects.statName)
                {
                    case StatName.Heal:          line = $"Heals {effects.Value} HP"; break;
                    case StatName.Mana:          line = $"Restores {effects.Value} Mana"; break;
                    case StatName.MaxHealth:     line = $"+{effects.Value} Max HP"; break;
                    case StatName.MaxMana:       line = $"+{effects.Value} Max Mana"; break;
                    case StatName.AttackDamage:  line = $"+{effects.Value} Attack Damage"; break;
                    case StatName.AbilityPower:  line = $"+{effects.Value} Ability Power"; break;
                    case StatName.MoveSpeed:     line = $"+{effects.Value} Move Speed"; break;
                    case StatName.Armor:         line = $"+{effects.Value} Armor"; break;
                    case StatName.MagicResist:   line = $"+{effects.Value} Magic Resist"; break;
                    case StatName.Lifesteal:     line = $"+{effects.Value}% Lifesteal"; break;
                    default:                     line = $"+{effects.Value} {effects.statName}"; break;
                }
                if (effects.Duration > 1) line = $"{line} in ({effects.Duration}s)";
                outLines.Add(line);
            }
        }
        return outLines;
    }
}
