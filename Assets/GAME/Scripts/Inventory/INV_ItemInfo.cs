using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class INV_ItemInfo : MonoBehaviour
{
    [Header("Showing Info of Items")]
    [Header("References")]
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    public TMP_Text itemNameText;
    public TMP_Text itemDescText;
    public Transform statContainer;
    public TMP_Text statLinePrefab;
    public Vector2 offset = new Vector2(12f, -8f);

    void Awake()
    {
        canvasGroup ??= GetComponent<CanvasGroup>();
        rectTransform ??= GetComponent<RectTransform>();

        if (!canvasGroup)    Debug.LogError("INV_ItemInfo: canvasGroup missing.", this);
        if (!rectTransform)  Debug.LogError("INV_ItemInfo: rectTransform missing.", this);
        // if (!itemNameText) Debug.LogError("INV_ItemInfo: itemNameText missing.", this);
        if (!itemDescText)   Debug.LogError("INV_ItemInfo: itemDescText missing.", this);
        if (!statContainer)  Debug.LogError("INV_ItemInfo: statContainer missing.", this);
        if (!statLinePrefab) Debug.LogError("INV_ItemInfo: statLinePrefab missing.", this);
    }

    public void Show(INV_ItemSO inv_ItemSO)
    {
        // make visible
        canvasGroup.alpha = 1f;

        // header texts
        if (itemNameText) itemNameText.text = inv_ItemSO ? inv_ItemSO.itemName : string.Empty;
        if (itemDescText) itemDescText.text = inv_ItemSO ? inv_ItemSO.itemDescription : string.Empty;

        // rebuild stat lines
        ClearStatLines();
        if (inv_ItemSO)
        {
            var lines = BuildStatLines(inv_ItemSO);
            for (int i = 0; i < lines.Count; i++)
            {
                var line = Instantiate(statLinePrefab, statContainer);
                line.text = lines[i];
            }
        }
    }

    // Hide and clear
    public void Hide()
    {
        canvasGroup.alpha = 0f;
        if (itemNameText) itemNameText.text = string.Empty;
        itemDescText.text = string.Empty;
        ClearStatLines();
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
    List<string> BuildStatLines(INV_ItemSO inv_ItemSO)
    {
        var outLines = new List<string>();

        if (inv_ItemSO.isGold)
        {
            outLines.Add("A pile of gold coins.");
            return outLines;
        }

        if (inv_ItemSO.modifiers != null)
        {
            for (int i = 0; i < inv_ItemSO.modifiers.Count; i++)
            {
                var fx = inv_ItemSO.modifiers[i];
                string line;
                switch (fx.statName)
                {
                    case StatName.Heal:          line = $"Heals {fx.Value} HP"; break;
                    case StatName.MaxHealth:     line = $"+{fx.Value} Max HP"; break;
                    case StatName.AttackDamage:  line = $"+{fx.Value} Attack Damage"; break;
                    case StatName.AbilityPower:  line = $"+{fx.Value} Ability Power"; break;
                    case StatName.MoveSpeed:     line = $"+{fx.Value} Move Speed"; break;
                    case StatName.Armor:         line = $"+{fx.Value} Armor"; break;
                    case StatName.MagicResist:   line = $"+{fx.Value} Magic Resist"; break;
                    case StatName.Lifesteal:     line = $"+{fx.Value}% Lifesteal"; break;
                    default:                     line = $"+{fx.Value} {fx.statName}"; break;
                }
                if (fx.Duration > 1) line = $"{line} ({fx.Duration}s)";
                outLines.Add(line);
            }
        }
        return outLines;
    }
}
