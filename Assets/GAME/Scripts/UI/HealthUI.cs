using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public C_Stats  p_Stats;
    public C_Health p_Health;            // Player's C_Health
    public RectTransform root;
    public Image healthPointPrefab;

    [Header("Sprites (index = Empty -> Full)")]
    public Sprite[] healthPoint = new Sprite[4];

    readonly List<Image> hpIcons = new List<Image>();
    int lastMaxHP = -1;

    System.Action<int> onDamaged, onHealed;
    System.Action onDied;
    
    void Awake()
    {
        if (!p_Stats) Debug.LogError($"{name}: P_Stats in HealthUI missing.");
        if (!p_Health) Debug.LogError($"{name}: P_Health in HealthUI missing.");

    }

    void OnEnable()
    {
        onDamaged = _ => UpdateSprites();
        onHealed  = _ => UpdateSprites();
        onDied    = () => UpdateSprites();

        p_Health.OnDamaged += onDamaged;
        p_Health.OnHealed  += onHealed;
        p_Health.OnDied    += onDied;

        UpdateUI();
        UpdateSprites();
    }

    void OnDisable()
    {
        p_Health.OnDamaged -= onDamaged;
        p_Health.OnHealed  -= onHealed;
        p_Health.OnDied    -= onDied;
    }

    public void UpdateUI()
    {
        // Player-only
        int maxHP = p_Stats.maxHP;

        int HealthPointPerIcon = Mathf.Max(1, healthPoint.Length - 1);

        if (maxHP == lastMaxHP)
            return;

        lastMaxHP = maxHP;

        // Number of heart images needed
        int needed = Mathf.CeilToInt(maxHP / (float)HealthPointPerIcon);

        // Rebuild
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);

        hpIcons.Clear();

        for (int i = 0; i < needed; i++)
        {
            var img = Instantiate(healthPointPrefab, root);
            img.enabled = true;
            hpIcons.Add(img);
        }
    }

    public void UpdateSprites()
    {
        int hp  = p_Stats.currentHP;
        int pph = Mathf.Max(1, healthPoint.Length - 1); // fills per heart (excluding empty)

        for (int i = 0; i < hpIcons.Count; i++)
        {
            // Remaining HP that this heart can show
            int fill = Mathf.Clamp(hp - i * pph, 0, pph);
            hpIcons[i].sprite = healthPoint[fill];
        }
    }
}
