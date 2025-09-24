using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UI_ItemInfoPanel : MonoBehaviour
{
    [Header("Panel")]
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;  // Set in Inspector (this panel)
    [Header("References")]
    public TMP_Text nameText;
    public TMP_Text descText;
    [Header("Stat Lines")]
    public Transform statContainer;      // Vertical group parent
    public TMP_Text statLinePrefab; // Simple TMP text prefab

    [Header("Mouse Offset")]
    public Vector2 offset = new Vector2(12f, -8f);

    // Cache
    void Awake()
    {
        canvasGroup ??= GetComponent<CanvasGroup>();
        rectTransform ??= GetComponent<RectTransform>();
        if (!canvasGroup) Debug.LogError("UI_ItemInfoPanel: Missing CanvasGroup.");
        if (!rectTransform) Debug.LogError("UI_ItemInfoPanel: Missing RectTransform.");
    }

    public void Show(INV_ItemSO item)
    {
        // visible
        canvasGroup.alpha = 1f;

        // header fields
        if (nameText) nameText.text = item ? item.itemName : "";
        if (descText) descText.text = item ? item.itemDescription : "";

        // rebuild stat lines
        ClearStatLines();
        if (item)
        {
            var lines = BuildStatLines(item);
            for (int i = 0; i < lines.Count; i++)
            {
                var line = Instantiate(statLinePrefab, statContainer);
                line.text = lines[i];
            }
        }
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        if (nameText) nameText.text = "";
        if (descText) descText.text = "";
        ClearStatLines();
    }

    public void FollowMouse(Vector2 screenPos)
    {
        rectTransform.position = (Vector3)screenPos + (Vector3)offset;
    }

    void ClearStatLines()
    {
        if (!statContainer) return;
        for (int i = statContainer.childCount - 1; i >= 0; i--)
            Destroy(statContainer.GetChild(i).gameObject);
    }

    // --- Formatting ---

    // Adjust mapping here to match your StatEffect fields exactly.
    List<string> BuildStatLines(INV_ItemSO item)
    {
        var outLines = new List<string>();

        // Currency example (optional display)
        if (item.isGold)
        {
            // Note: gold value is not on the item, it's on the shop or determined otherwise.
            // This info panel just knows it's a gold item.
            outLines.Add("A pile of gold coins.");
            return outLines;
        }

        // From your existing item modifiers list:
        if (item.modifiers != null)
        {
            for (int i = 0; i < item.modifiers.Count; i++)
            {
                var fx = item.modifiers[i];
                string line = "";

                switch (fx.statName)
                {
                    case StatName.Heal:
                        line = $"Heals {fx.Value} HP";
                        break;
                    case StatName.MaxHealth:
                        line = $"+{fx.Value} Max HP";
                        break;
                    case StatName.AttackDamage:
                        line = $"+{fx.Value} Attack Damage";
                        break;
                    case StatName.AbilityPower:
                        line = $"+{fx.Value} Ability Power";
                        break;
                    case StatName.MoveSpeed:
                        line = $"+{fx.Value} Move Speed";
                        break;
                    case StatName.Armor:
                        line = $"+{fx.Value} Armor";
                        break;
                    case StatName.MagicResist:
                        line = $"+{fx.Value} Magic Resist";
                        break;
                    case StatName.Lifesteal:
                        line = $"+{fx.Value}% Lifesteal";
                        break;
                    default:
                        line = $"+{fx.Value} {fx.statName}";
                        break;
                }


                if (fx.Duration > 1)
                {
                    line = $"{line} ({fx.Duration}s)";
                }
                outLines.Add(line);
            }
        }

        return outLines;
    }
}
